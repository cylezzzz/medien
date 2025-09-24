namespace AiMediaGenerator.Models;

public class Job
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string Prompt { get; set; } = "";
    public string? NegativePrompt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? ResultUrl { get; set; }
    public string? InputPath { get; set; }
    public string? MaskPath { get; set; }
    public int? Frames { get; set; }
    public int? Fps { get; set; }
}

public class Capabilities
{
    public string? MaxImageResolution { get; set; }
    public int? MaxImageWidth { get; set; }
    public int? MaxImageHeight { get; set; }
    public int? MaxFrames { get; set; }
    public int? MaxFps { get; set; }
    public int? MaxVideoSeconds { get; set; }
    public int? MaxUploadMb { get; set; }
    public string? Notes { get; set; }

    public override string ToString()
    {
        string res = MaxImageResolution ?? ((MaxImageWidth.HasValue && MaxImageHeight.HasValue) ? $"{MaxImageWidth}x{MaxImageHeight}" : "—");
        return $"IMG {res} · Frames {MaxFrames?.ToString() ?? "—"} · FPS {MaxFps?.ToString() ?? "—"} · Video {MaxVideoSeconds?.ToString() ?? "—"}s · Upload {MaxUploadMb?.ToString() ?? "—"}MB";
    }
}
