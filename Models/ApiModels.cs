namespace AiMediaGenerator.Models;

public class ServicesResponse
{
    public List<AIService> AvailableServices { get; set; } = new();
}

public class AIService
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Cost { get; set; }
    public bool IsPremium { get; set; }
    public bool IsDemo { get; set; }
    public bool HasValidApiKey { get; set; }
    public string RegistrationUrl { get; set; }
    public List<string> Capabilities { get; set; } = new();
}

public class JobResult
{
    public string JobId { get; set; }
    public string ServiceName { get; set; }
    public string EstimatedCost { get; set; }
}

public class JobStatus
{
    public string Status { get; set; }
    public string Error { get; set; }
    public string ServiceStatus { get; set; }
    public string CurrentService { get; set; }
    public string FinalService { get; set; }
    public bool ServiceSwitched { get; set; }
}