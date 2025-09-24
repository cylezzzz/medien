using AiMediaGenerator.Models;
using Microsoft.Maui.Storage;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AiMediaGenerator.Services;

public class ApiService
{
    readonly HttpClient _http;

    public ApiService(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromMinutes(10);
    }

    string BaseUrl => Preferences.Get("ApiBaseUrl", "");
    string ApiKey => Preferences.Get("ApiKey", "");
    bool AllowNsfw => Preferences.Get("AllowNSFW", false);
    const string CapsPrefKey = "CapabilitiesJson";

    HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new InvalidOperationException("API-URL fehlt (Settings).");
        var url = $"{BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        var req = new HttpRequestMessage(method, url);
        if (!string.IsNullOrWhiteSpace(ApiKey))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        return req;
    }

    public async Task<bool> PingAsync()
    {
        var req = CreateRequest(HttpMethod.Get, "status");
        using var res = await _http.SendAsync(req);
        return res.IsSuccessStatusCode;
    }

    public Capabilities? GetCachedCapabilities()
    {
        var json = Preferences.Get(CapsPrefKey, "");
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<Capabilities>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); }
        catch { return null; }
    }

    public async Task<Capabilities?> GetCapabilitiesAsync()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        async Task<Capabilities?> tryFetch(string path)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Get, path);
                using var res = await _http.SendAsync(req);
                if (!res.IsSuccessStatusCode) return null;

                var json = await res.Content.ReadAsStringAsync();

                Capabilities? caps = null;
                try
                {
                    caps = JsonSerializer.Deserialize<Capabilities>(json, options);
                    if (caps != null && !IsEmpty(caps))
                        return caps;
                }
                catch { }

                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
                if (dict == null) return null;

                var result = new Capabilities();

                if (dict.TryGetValue("max_image_resolution", out var mir))
                {
                    if (mir is JsonElement je)
                    {
                        if (je.ValueKind == JsonValueKind.String)
                            result.MaxImageResolution = je.GetString();
                        else if (je.ValueKind == JsonValueKind.Object)
                        {
                            if (je.TryGetProperty("width", out var w)) result.MaxImageWidth = w.GetInt32();
                            if (je.TryGetProperty("height", out var h)) result.MaxImageHeight = h.GetInt32();
                        }
                    }
                    else
                    {
                        result.MaxImageResolution = mir?.ToString();
                    }
                }
                if (dict.TryGetValue("max_image_width", out var mw) && mw is JsonElement mwJe && mwJe.ValueKind == JsonValueKind.Number)
                    result.MaxImageWidth = mwJe.GetInt32();
                if (dict.TryGetValue("max_image_height", out var mh) && mh is JsonElement mhJe && mhJe.ValueKind == JsonValueKind.Number)
                    result.MaxImageHeight = mhJe.GetInt32();

                if (dict.TryGetValue("max_frames", out var mf) && mf is JsonElement mfJe && mfJe.ValueKind == JsonValueKind.Number)
                    result.MaxFrames = mfJe.GetInt32();
                if (dict.TryGetValue("max_fps", out var fps) && fps is JsonElement fpsJe && fpsJe.ValueKind == JsonValueKind.Number)
                    result.MaxFps = fpsJe.GetInt32();
                if (dict.TryGetValue("max_video_seconds", out var mvs) && mvs is JsonElement mvsJe && mvsJe.ValueKind == JsonValueKind.Number)
                    result.MaxVideoSeconds = mvsJe.GetInt32();
                if (dict.TryGetValue("max_upload_mb", out var mb) && mb is JsonElement mbJe && mbJe.ValueKind == JsonValueKind.Number)
                    result.MaxUploadMb = mbJe.GetInt32();
                if (dict.TryGetValue("notes", out var notes))
                    result.Notes = notes.ToString();

                return IsEmpty(result) ? null : result;
            }
            catch
            {
                return null;
            }
        }

        var cap = await tryFetch("capabilities") ?? await tryFetch("limits");

        if (cap != null)
        {
            Preferences.Set(CapsPrefKey, JsonSerializer.Serialize(cap));
        }

        return cap;
    }

    static bool IsEmpty(Capabilities c) =>
        c.MaxFrames == null && c.MaxFps == null && c.MaxVideoSeconds == null &&
        c.MaxUploadMb == null && c.MaxImageHeight == null && c.MaxImageWidth == null &&
        string.IsNullOrWhiteSpace(c.MaxImageResolution) && string.IsNullOrWhiteSpace(c.Notes);

    public async Task<string> Img2ImgAsync(string imagePath, string prompt, string negative)
    {
        var req = CreateRequest(HttpMethod.Post, "img2img");
        using var multi = new MultipartFormDataContent();

        var img = File.OpenRead(imagePath);
        multi.Add(new StreamContent(img), "image", Path.GetFileName(imagePath));
        multi.Add(new StringContent(prompt, Encoding.UTF8), "prompt");
        multi.Add(new StringContent(negative ?? string.Empty, Encoding.UTF8), "negative_prompt");
        multi.Add(new StringContent(AllowNsfw ? "true" : "false", Encoding.UTF8), "nsfw");

        req.Content = multi;
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();
        var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        var resultUrl = payload != null && payload.TryGetValue("result_url", out var v) ? v?.ToString() ?? "" : "";
        SaveHistory(new Job
        {
            Id = Guid.NewGuid().ToString(),
            Type = "img2img",
            Prompt = prompt,
            NegativePrompt = negative,
            CreatedAt = DateTimeOffset.Now,
            ResultUrl = resultUrl,
            InputPath = imagePath
        });
        return resultUrl;
    }

    public async Task<string> InpaintAsync(string imagePath, string maskPath, string prompt, string negative)
    {
        var req = CreateRequest(HttpMethod.Post, "inpaint");
        using var multi = new MultipartFormDataContent();
        multi.Add(new StreamContent(File.OpenRead(imagePath)), "image", Path.GetFileName(imagePath));
        multi.Add(new StreamContent(File.OpenRead(maskPath)), "mask", Path.GetFileName(maskPath));
        multi.Add(new StringContent(prompt, Encoding.UTF8), "prompt");
        multi.Add(new StringContent(negative ?? string.Empty, Encoding.UTF8), "negative_prompt");
        multi.Add(new StringContent(AllowNsfw ? "true" : "false", Encoding.UTF8), "nsfw");

        req.Content = multi;
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();
        var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        var resultUrl = payload != null && payload.TryGetValue("result_url", out var v) ? v?.ToString() ?? "" : "";
        SaveHistory(new Job
        {
            Id = Guid.NewGuid().ToString(),
            Type = "inpaint",
            Prompt = prompt,
            NegativePrompt = negative,
            CreatedAt = DateTimeOffset.Now,
            ResultUrl = resultUrl,
            InputPath = imagePath,
            MaskPath = maskPath
        });
        return resultUrl;
    }

    public async Task<string> VideoAsync(string imagePath, string prompt, int frames, int fps)
    {
        var req = CreateRequest(HttpMethod.Post, "video");
        using var multi = new MultipartFormDataContent();
        multi.Add(new StreamContent(File.OpenRead(imagePath)), "image", Path.GetFileName(imagePath));
        multi.Add(new StringContent(prompt, Encoding.UTF8), "prompt");
        multi.Add(new StringContent(frames.ToString(), Encoding.UTF8), "frames");
        multi.Add(new StringContent(fps.ToString(), Encoding.UTF8), "fps");
        multi.Add(new StringContent(AllowNsfw ? "true" : "false", Encoding.UTF8), "nsfw");

        req.Content = multi;
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();
        var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        var resultUrl = payload != null && payload.TryGetValue("result_url", out var v) ? v?.ToString() ?? "" : "";
        SaveHistory(new Job
        {
            Id = Guid.NewGuid().ToString(),
            Type = "video",
            Prompt = prompt,
            CreatedAt = DateTimeOffset.Now,
            ResultUrl = resultUrl,
            InputPath = imagePath,
            Frames = frames,
            Fps = fps
        });
        return resultUrl;
    }

    void SaveHistory(Job job)
    {
        const string key = "HistoryJson";
        var list = new List<Job>();
        var current = Preferences.Get(key, "[]");
        try
        {
            var parsed = JsonSerializer.Deserialize<List<Job>>(current);
            if (parsed != null) list = parsed;
        }
        catch { }
        list.Add(job);
        Preferences.Set(key, JsonSerializer.Serialize(list));
    }
}
