namespace Clinic.Application.Reporting;

public sealed class ReportingOptions
{
    public const string SectionName = "Reporting";

    public decimal DefaultVisitRevenue { get; set; } = 125m;
    public int CacheMinutes { get; set; } = 10;
}
