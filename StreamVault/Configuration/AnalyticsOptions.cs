namespace StreamVault.Configuration;

public class AnalyticsOptions
{
    public const string SectionName = "Analytics";

    public bool EnableSimulator { get; set; } = true;

    public int PlayEventRetentionDays { get; set; } = 30;

    public int EventsPerSecond { get; set; } = 3;
}
