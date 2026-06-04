using System.Text;
using Clinic.Application.Common.Security;
using Clinic.Application.Reporting;
using Clinic.Application.Reporting.Queries;
using ClosedXML.Excel;
using MediatR;

namespace Clinic.Api.Endpoints;

public static class ReportingEndpoints
{
    public static IEndpointRouteBuilder MapReportingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports")
            .RequireAuthorization(AuthorizationPolicyNames.ViewReports)
            .WithTags("Reports");

        group.MapGet("/dashboard", async (
            Guid? tenantId,
            DateOnly? from,
            DateOnly? to,
            ISender sender,
            CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetReportingDashboardQuery(tenantId, ResolveFrom(from), ResolveTo(to)), cancellationToken)))
            .WithName("GetReportingDashboard")
            .WithSummary("Get dashboard KPIs and charts")
            .Produces<ReportingDashboardDto>(StatusCodes.Status200OK);

        group.MapGet("/charts", async (
            Guid? tenantId,
            DateOnly? from,
            DateOnly? to,
            ISender sender,
            CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetReportingChartsQuery(tenantId, ResolveFrom(from), ResolveTo(to)), cancellationToken)))
            .WithName("GetReportingCharts")
            .WithSummary("Get analytics chart data")
            .Produces<ReportingChartsDto>(StatusCodes.Status200OK);

        group.MapGet("/export/excel", async (
            Guid? tenantId,
            DateOnly? from,
            DateOnly? to,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var dashboard = await sender.Send(new GetReportingDashboardQuery(tenantId, ResolveFrom(from), ResolveTo(to)), cancellationToken);
            return Results.File(BuildExcel(dashboard), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "clinic-report.xlsx");
        })
        .WithName("ExportReportsExcel")
        .WithSummary("Export reports as Excel")
        .Produces(StatusCodes.Status200OK, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        group.MapGet("/export/pdf", async (
            Guid? tenantId,
            DateOnly? from,
            DateOnly? to,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var dashboard = await sender.Send(new GetReportingDashboardQuery(tenantId, ResolveFrom(from), ResolveTo(to)), cancellationToken);
            return Results.File(BuildPdf(dashboard), "application/pdf", "clinic-report.pdf");
        })
        .WithName("ExportReportsPdf")
        .WithSummary("Export reports as PDF")
        .Produces(StatusCodes.Status200OK, contentType: "application/pdf");

        return app;
    }

    private static DateOnly ResolveFrom(DateOnly? from) =>
        from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

    private static DateOnly ResolveTo(DateOnly? to) =>
        to ?? DateOnly.FromDateTime(DateTime.UtcNow);

    private static byte[] BuildExcel(ReportingDashboardDto dashboard)
    {
        using var workbook = new XLWorkbook();
        var kpis = workbook.Worksheets.Add("KPIs");
        kpis.Cell(1, 1).Value = "Metric";
        kpis.Cell(1, 2).Value = "Value";
        kpis.Cell(2, 1).Value = "Total Patients";
        kpis.Cell(2, 2).Value = dashboard.Kpis.TotalPatients;
        kpis.Cell(3, 1).Value = "New Patients";
        kpis.Cell(3, 2).Value = dashboard.Kpis.NewPatients;
        kpis.Cell(4, 1).Value = "Appointments";
        kpis.Cell(4, 2).Value = dashboard.Kpis.Appointments;
        kpis.Cell(5, 1).Value = "Revenue";
        kpis.Cell(5, 2).Value = dashboard.Kpis.Revenue;
        kpis.Cell(6, 1).Value = "Active Doctors";
        kpis.Cell(6, 2).Value = dashboard.Kpis.ActiveDoctors;
        kpis.Columns().AdjustToContents();

        var visits = workbook.Worksheets.Add("Daily Visits");
        visits.Cell(1, 1).Value = "Date";
        visits.Cell(1, 2).Value = "Visits";
        for (var i = 0; i < dashboard.Charts.DailyVisits.Count; i++)
        {
            visits.Cell(i + 2, 1).Value = dashboard.Charts.DailyVisits[i].Date.ToString("yyyy-MM-dd");
            visits.Cell(i + 2, 2).Value = dashboard.Charts.DailyVisits[i].Visits;
        }
        visits.Columns().AdjustToContents();

        var revenue = workbook.Worksheets.Add("Monthly Revenue");
        revenue.Cell(1, 1).Value = "Month";
        revenue.Cell(1, 2).Value = "Revenue";
        for (var i = 0; i < dashboard.Charts.MonthlyRevenue.Count; i++)
        {
            var item = dashboard.Charts.MonthlyRevenue[i];
            revenue.Cell(i + 2, 1).Value = $"{item.Year}-{item.Month:00}";
            revenue.Cell(i + 2, 2).Value = item.Revenue;
        }
        revenue.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] BuildPdf(ReportingDashboardDto dashboard)
    {
        var text = $"Clinic Report Total Patients: {dashboard.Kpis.TotalPatients} New Patients: {dashboard.Kpis.NewPatients} Appointments: {dashboard.Kpis.Appointments} Revenue: {dashboard.Kpis.Revenue:C} Active Doctors: {dashboard.Kpis.ActiveDoctors}";
        var stream = new MemoryStream();
        var body = $"BT /F1 12 Tf 72 740 Td ({EscapePdfText(text)}) Tj ET";
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(body)} >>\nstream\n{body}\nendstream"
        };

        void Write(string value) => stream.Write(Encoding.ASCII.GetBytes(value));
        Write("%PDF-1.4\n");
        var offsets = new List<long> { 0 };
        for (var index = 0; index < objects.Length; index++)
        {
            offsets.Add(stream.Position);
            Write($"{index + 1} 0 obj\n{objects[index]}\nendobj\n");
        }

        var xref = stream.Position;
        Write($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            Write($"{offset:0000000000} 00000 n \n");
        }

        Write($"trailer << /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        return stream.ToArray();
    }

    private static string EscapePdfText(string text) =>
        text.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
}
