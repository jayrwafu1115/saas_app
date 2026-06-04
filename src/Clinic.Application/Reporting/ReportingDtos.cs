namespace Clinic.Application.Reporting;

public sealed record DashboardKpiDto(
    int TotalPatients,
    int NewPatients,
    int Appointments,
    decimal Revenue,
    int ActiveDoctors);

public sealed record DailyVisitDto(DateOnly Date, int Visits);
public sealed record MonthlyRevenueDto(int Year, int Month, decimal Revenue);
public sealed record DoctorPerformanceDto(Guid DoctorUserId, int Appointments, int CompletedVisits, decimal Revenue);
public sealed record LocationPerformanceDto(Guid LocationId, string LocationName, int Appointments, int CompletedVisits, decimal Revenue);

public sealed record ReportingChartsDto(
    IReadOnlyList<DailyVisitDto> DailyVisits,
    IReadOnlyList<MonthlyRevenueDto> MonthlyRevenue,
    IReadOnlyList<DoctorPerformanceDto> DoctorPerformance,
    IReadOnlyList<LocationPerformanceDto> LocationPerformance);

public sealed record ReportingDashboardDto(DashboardKpiDto Kpis, ReportingChartsDto Charts);
