namespace Sandbox.Configuration;

public record ApplicationOptions
{
    public const string SectionName = "Application";

    public required int ProjectId { get; init; }
    public required string SystemName { get; init; }
    public required string Deployment { get; init; }
}