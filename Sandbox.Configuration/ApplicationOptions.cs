namespace Sandbox.Configuration;

public record ApplicationOptions
{
    public const string SectionName = "Application";

    public int? ProjectId { get; init; }
    public string? SystemName { get; init; }
    public string? Deployment { get; init; }
}