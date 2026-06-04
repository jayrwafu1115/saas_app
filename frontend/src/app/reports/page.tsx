"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { ArrowLeft, BarChart3, Download, FileSpreadsheet, FileText, LineChart, UsersRound } from "lucide-react";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { Button } from "@/components/ui/button";
import { getReportExcel, getReportPdf, getReportingDashboard } from "@/lib/api";

export default function ReportsPage() {
  const today = useMemo(() => new Date().toISOString().slice(0, 10), []);
  const thirtyDaysAgo = useMemo(() => {
    const date = new Date();
    date.setDate(date.getDate() - 30);
    return date.toISOString().slice(0, 10);
  }, []);
  const [from, setFrom] = useState(thirtyDaysAgo);
  const [to, setTo] = useState(today);
  const params = { from, to };
  const dashboardQuery = useQuery({
    queryKey: ["report-dashboard", from, to],
    queryFn: () => getReportingDashboard(params),
  });
  const dashboard = dashboardQuery.data;

  async function downloadExcel() {
    const blob = await getReportExcel(params);
    downloadBlob(blob, "clinic-report.xlsx");
  }

  async function downloadPdf() {
    const blob = await getReportPdf(params);
    downloadBlob(blob, "clinic-report.pdf");
  }

  return (
    <ProtectedRoute>
      <main className="min-h-screen bg-background text-foreground">
        <header className="border-b border-border bg-surface">
          <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-6 py-4">
            <div className="flex items-center gap-3">
              <Button variant="ghost" size="icon" asChild aria-label="Back">
                <Link href="/"><ArrowLeft className="h-4 w-4" aria-hidden="true" /></Link>
              </Button>
              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Reporting</p>
                <h1 className="text-xl font-semibold">Dashboard</h1>
              </div>
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <input className="h-10 rounded-md border border-border bg-background px-3 text-sm" type="date" value={from} onChange={(event) => setFrom(event.target.value)} />
              <input className="h-10 rounded-md border border-border bg-background px-3 text-sm" type="date" value={to} onChange={(event) => setTo(event.target.value)} />
              <Button variant="outline" onClick={downloadExcel}>
                <FileSpreadsheet className="h-4 w-4" aria-hidden="true" />
                Excel
              </Button>
              <Button variant="outline" onClick={downloadPdf}>
                <FileText className="h-4 w-4" aria-hidden="true" />
                PDF
              </Button>
            </div>
          </div>
        </header>

        <section className="mx-auto max-w-6xl px-6 py-8">
          <div className="grid gap-4 md:grid-cols-5">
            <Kpi label="Total Patients" value={dashboard?.kpis.totalPatients ?? 0} />
            <Kpi label="New Patients" value={dashboard?.kpis.newPatients ?? 0} />
            <Kpi label="Appointments" value={dashboard?.kpis.appointments ?? 0} />
            <Kpi label="Revenue" value={formatCurrency(dashboard?.kpis.revenue ?? 0)} />
            <Kpi label="Active Doctors" value={dashboard?.kpis.activeDoctors ?? 0} />
          </div>

          <div className="mt-6 grid gap-6 lg:grid-cols-2">
            <section className="rounded-md border border-border bg-surface p-5">
              <div className="mb-4 flex items-center gap-2">
                <LineChart className="h-4 w-4 text-accent" aria-hidden="true" />
                <h2 className="text-sm font-semibold">Daily Visits</h2>
              </div>
              <BarList items={(dashboard?.charts.dailyVisits ?? []).map((item) => ({ label: item.date, value: item.visits }))} />
            </section>

            <section className="rounded-md border border-border bg-surface p-5">
              <div className="mb-4 flex items-center gap-2">
                <BarChart3 className="h-4 w-4 text-accent" aria-hidden="true" />
                <h2 className="text-sm font-semibold">Monthly Revenue</h2>
              </div>
              <BarList items={(dashboard?.charts.monthlyRevenue ?? []).map((item) => ({ label: `${item.year}-${String(item.month).padStart(2, "0")}`, value: item.revenue, display: formatCurrency(item.revenue) }))} />
            </section>

            <section className="rounded-md border border-border bg-surface p-5">
              <div className="mb-4 flex items-center gap-2">
                <UsersRound className="h-4 w-4 text-accent" aria-hidden="true" />
                <h2 className="text-sm font-semibold">Doctor Performance</h2>
              </div>
              <PerformanceTable rows={(dashboard?.charts.doctorPerformance ?? []).map((item) => ({ label: item.doctorUserId.slice(0, 8), appointments: item.appointments, visits: item.completedVisits, revenue: item.revenue }))} />
            </section>

            <section className="rounded-md border border-border bg-surface p-5">
              <div className="mb-4 flex items-center gap-2">
                <Download className="h-4 w-4 text-accent" aria-hidden="true" />
                <h2 className="text-sm font-semibold">Location Performance</h2>
              </div>
              <PerformanceTable rows={(dashboard?.charts.locationPerformance ?? []).map((item) => ({ label: item.locationName, appointments: item.appointments, visits: item.completedVisits, revenue: item.revenue }))} />
            </section>
          </div>
        </section>
      </main>
    </ProtectedRoute>
  );
}

function Kpi({ label, value }: { label: string; value: string | number }) {
  return (
    <section className="rounded-md border border-border bg-surface p-4">
      <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">{label}</p>
      <p className="mt-2 text-2xl font-semibold">{value}</p>
    </section>
  );
}

function BarList({ items }: { items: { label: string; value: number; display?: string }[] }) {
  const max = Math.max(1, ...items.map((item) => item.value));
  return (
    <div className="grid gap-3">
      {items.length ? items.map((item) => (
        <div key={item.label} className="grid grid-cols-[8rem_1fr_5rem] items-center gap-3 text-sm">
          <span className="truncate text-muted-foreground">{item.label}</span>
          <span className="h-2 rounded bg-muted">
            <span className="block h-2 rounded bg-accent" style={{ width: `${Math.max(4, (item.value / max) * 100)}%` }} />
          </span>
          <span className="text-right font-medium">{item.display ?? item.value}</span>
        </div>
      )) : <p className="text-sm text-muted-foreground">No data</p>}
    </div>
  );
}

function PerformanceTable({ rows }: { rows: { label: string; appointments: number; visits: number; revenue: number }[] }) {
  return (
    <div className="divide-y divide-border">
      {rows.length ? rows.map((row) => (
        <div key={row.label} className="grid grid-cols-[1fr_5rem_5rem_6rem] gap-3 py-2 text-sm">
          <span className="truncate font-medium">{row.label}</span>
          <span className="text-muted-foreground">{row.appointments}</span>
          <span className="text-muted-foreground">{row.visits}</span>
          <span className="text-right">{formatCurrency(row.revenue)}</span>
        </div>
      )) : <p className="text-sm text-muted-foreground">No data</p>}
    </div>
  );
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", maximumFractionDigits: 0 }).format(value);
}

function downloadBlob(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = filename;
  anchor.click();
  URL.revokeObjectURL(url);
}
