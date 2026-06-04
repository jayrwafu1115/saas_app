"use client";

import Link from "next/link";
import { useState, type ReactNode } from "react";
import {
  Activity,
  Bell,
  Bot,
  Building2,
  CalendarDays,
  CheckCircle2,
  ChevronDown,
  ClipboardList,
  Command,
  CreditCard,
  FileText,
  Home,
  MapPin,
  Menu,
  MoreHorizontal,
  Search,
  Settings,
  ShieldCheck,
  Stethoscope,
  UsersRound,
  X,
} from "lucide-react";
import { cn } from "@/lib/utils";

type NavItem = {
  label: string;
  href: string;
  icon: React.ComponentType<{ className?: string; "aria-hidden"?: boolean }>;
};

export const clinicNavItems: NavItem[] = [
  { label: "Dashboard", href: "/clinicos-ph", icon: Home },
  { label: "Appointments", href: "/appointments", icon: CalendarDays },
  { label: "Queue / Check-in", href: "/clinicos-ph#queue", icon: Activity },
  { label: "Patients", href: "/patients", icon: UsersRound },
  { label: "Encounters", href: "/encounters", icon: Stethoscope },
  { label: "Billing", href: "/billing", icon: CreditCard },
  { label: "Reports", href: "/reports", icon: ClipboardList },
  { label: "AI Reports", href: "/encounters", icon: Bot },
  { label: "Locations", href: "/locations", icon: Building2 },
  { label: "Settings", href: "/tenants", icon: Settings },
];

export function AppShell({ children, breadcrumb = "Dashboard" }: { children: ReactNode; breadcrumb?: string }) {
  const [open, setOpen] = useState(false);

  return (
    <div className="min-h-screen bg-background text-foreground">
      <Sidebar open={open} onClose={() => setOpen(false)} />
      <div className="lg:pl-72">
        <Topbar onMenu={() => setOpen(true)} breadcrumb={breadcrumb} />
        <main className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">{children}</main>
      </div>
    </div>
  );
}

export function Sidebar({ open, onClose }: { open: boolean; onClose: () => void }) {
  return (
    <>
      <div className={cn("fixed inset-0 z-40 bg-slate-950/35 lg:hidden", open ? "block" : "hidden")} onClick={onClose} />
      <aside className={cn("fixed inset-y-0 left-0 z-50 w-72 border-r border-border bg-surface px-4 py-4 shadow-[var(--shadow-soft)] transition-transform lg:translate-x-0", open ? "translate-x-0" : "-translate-x-full")}>
        <div className="flex items-center justify-between">
          <Link href="/clinicos-ph" className="flex items-center gap-3">
            <span className="flex h-10 w-10 items-center justify-center rounded-md bg-primary text-primary-foreground">
              <ShieldCheck className="h-5 w-5" aria-hidden />
            </span>
            <span>
              <span className="block text-sm font-semibold">ClinicOS PH</span>
              <span className="block text-xs text-muted-foreground">Makati Care Group</span>
            </span>
          </Link>
          <button className="rounded-md p-2 text-muted-foreground lg:hidden" onClick={onClose} aria-label="Close navigation">
            <X className="h-5 w-5" aria-hidden />
          </button>
        </div>
        <nav className="mt-6 grid gap-1">
          {clinicNavItems.map((item) => (
            <Link key={item.label} href={item.href} className="flex h-10 items-center gap-3 rounded-md px-3 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground">
              <item.icon className="h-4 w-4" aria-hidden />
              {item.label}
            </Link>
          ))}
        </nav>
      </aside>
    </>
  );
}

export function Topbar({ onMenu, breadcrumb }: { onMenu: () => void; breadcrumb: string }) {
  return (
    <header className="sticky top-0 z-30 border-b border-border bg-background/88 backdrop-blur">
      <div className="flex min-h-16 items-center gap-3 px-4 sm:px-6 lg:px-8">
        <button className="rounded-md p-2 text-muted-foreground lg:hidden" onClick={onMenu} aria-label="Open navigation">
          <Menu className="h-5 w-5" aria-hidden />
        </button>
        <div className="hidden text-sm text-muted-foreground md:block">ClinicOS PH / {breadcrumb}</div>
        <div className="ml-auto flex items-center gap-2">
          <TenantSwitcher />
          <LocationSwitcher />
          <SearchCommand />
          <NotificationMenu />
          <button className="flex h-10 items-center gap-2 rounded-md border border-border bg-surface px-3 text-sm font-medium">
            <span className="h-6 w-6 rounded-full bg-accent" aria-hidden />
            Dra. Santos
            <ChevronDown className="h-4 w-4 text-muted-foreground" aria-hidden />
          </button>
        </div>
      </div>
    </header>
  );
}

export function TenantSwitcher() {
  return <Switcher icon={<Building2 className="h-4 w-4" aria-hidden />} label="Makati Care Group" />;
}

export function LocationSwitcher() {
  return <Switcher icon={<MapPin className="h-4 w-4" aria-hidden />} label="BGC Branch" />;
}

function Switcher({ icon, label }: { icon: ReactNode; label: string }) {
  return (
    <button className="hidden h-10 items-center gap-2 rounded-md border border-border bg-surface px-3 text-sm md:flex">
      {icon}
      {label}
      <ChevronDown className="h-4 w-4 text-muted-foreground" aria-hidden />
    </button>
  );
}

export function SearchCommand() {
  return (
    <button className="hidden h-10 min-w-64 items-center gap-2 rounded-md border border-border bg-surface px-3 text-sm text-muted-foreground xl:flex">
      <Search className="h-4 w-4" aria-hidden />
      Search patients, OR number, appointments
      <span className="ml-auto flex items-center gap-1 rounded bg-muted px-1.5 py-0.5 text-xs"><Command className="h-3 w-3" aria-hidden />K</span>
    </button>
  );
}

export function NotificationMenu() {
  return (
    <button className="relative flex h-10 w-10 items-center justify-center rounded-md border border-border bg-surface" aria-label="Notifications">
      <Bell className="h-4 w-4" aria-hidden />
      <span className="absolute right-2 top-2 h-2 w-2 rounded-full bg-danger" aria-hidden />
    </button>
  );
}

export function StatCard({ label, value, detail, icon: Icon, tone = "accent" }: { label: string; value: string; detail: string; icon: NavItem["icon"]; tone?: "accent" | "success" | "warning" | "info" }) {
  const tones = {
    accent: "bg-accent/10 text-accent",
    success: "bg-success/10 text-success",
    warning: "bg-warning/10 text-warning",
    info: "bg-info/10 text-info",
  };

  return (
    <section className="rounded-md border border-border bg-surface p-4 shadow-[var(--shadow-soft)]">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">{label}</p>
          <p className="mt-2 text-2xl font-semibold tracking-tight">{value}</p>
        </div>
        <span className={cn("flex h-10 w-10 items-center justify-center rounded-md", tones[tone])}>
          <Icon className="h-5 w-5" aria-hidden />
        </span>
      </div>
      <p className="mt-3 text-sm text-muted-foreground">{detail}</p>
    </section>
  );
}

export function ChartCard({ title, children, action }: { title: string; children: ReactNode; action?: ReactNode }) {
  return (
    <section className="rounded-md border border-border bg-surface p-5 shadow-[var(--shadow-soft)]">
      <div className="mb-4 flex items-center justify-between gap-3">
        <h2 className="text-sm font-semibold">{title}</h2>
        {action}
      </div>
      {children}
    </section>
  );
}

export function StatusBadge({ status }: { status: string }) {
  const normalized = status.toLowerCase();
  const tone = normalized.includes("cancel") || normalized.includes("show")
    ? "bg-danger/10 text-danger"
    : normalized.includes("complete") || normalized.includes("paid")
      ? "bg-success/10 text-success"
      : normalized.includes("waiting") || normalized.includes("trial")
        ? "bg-warning/10 text-warning"
        : "bg-info/10 text-info";
  return <span className={cn("inline-flex rounded-full px-2.5 py-1 text-xs font-semibold", tone)}>{status}</span>;
}

export function DataTable({ columns, rows }: { columns: string[]; rows: ReactNode[][] }) {
  return (
    <div className="overflow-hidden rounded-md border border-border bg-surface">
      <div className="flex items-center gap-2 border-b border-border p-3">
        <div className="flex h-9 flex-1 items-center gap-2 rounded-md border border-border bg-background px-3 text-sm text-muted-foreground">
          <Search className="h-4 w-4" aria-hidden />
          Search, filter, sort, export
        </div>
        <button className="h-9 rounded-md border border-border px-3 text-sm">Export</button>
      </div>
      <table className="w-full text-left text-sm">
        <thead className="bg-muted text-xs uppercase tracking-wide text-muted-foreground">
          <tr>{columns.map((column) => <th key={column} className="px-4 py-3 font-semibold">{column}</th>)}</tr>
        </thead>
        <tbody className="divide-y divide-border">
          {rows.map((row, index) => (
            <tr key={index} className="hover:bg-muted/60">{row.map((cell, cellIndex) => <td key={cellIndex} className="px-4 py-3">{cell}</td>)}</tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function PatientCard({ name, meta, alerts }: { name: string; meta: string; alerts: string[] }) {
  return (
    <section className="rounded-md border border-border bg-surface p-4">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-lg font-semibold">{name}</p>
          <p className="mt-1 text-sm text-muted-foreground">{meta}</p>
        </div>
        <MoreHorizontal className="h-4 w-4 text-muted-foreground" aria-hidden />
      </div>
      <div className="mt-3 flex flex-wrap gap-2">{alerts.map((alert) => <StatusBadge key={alert} status={alert} />)}</div>
    </section>
  );
}

export function AppointmentCard({ time, patient, doctor, status }: { time: string; patient: string; doctor: string; status: string }) {
  return (
    <div className="grid grid-cols-[5rem_1fr_auto] items-center gap-3 rounded-md border border-border bg-surface p-3 text-sm">
      <span className="font-mono text-muted-foreground">{time}</span>
      <span><strong>{patient}</strong><span className="block text-muted-foreground">{doctor}</span></span>
      <StatusBadge status={status} />
    </div>
  );
}

export function EmptyState({ title, detail, icon: Icon = FileText }: { title: string; detail: string; icon?: NavItem["icon"] }) {
  return (
    <div className="flex min-h-40 flex-col items-center justify-center rounded-md border border-dashed border-border bg-surface p-6 text-center">
      <Icon className="h-8 w-8 text-muted-foreground" aria-hidden />
      <p className="mt-3 font-semibold">{title}</p>
      <p className="mt-1 max-w-sm text-sm text-muted-foreground">{detail}</p>
    </div>
  );
}

export function ConfirmDialog({ title, action }: { title: string; action: string }) {
  return (
    <div className="rounded-md border border-border bg-surface p-4 shadow-[var(--shadow-soft)]">
      <p className="text-sm font-semibold">{title}</p>
      <div className="mt-3 flex justify-end gap-2">
        <button className="h-9 rounded-md border border-border px-3 text-sm">Cancel</button>
        <button className="h-9 rounded-md bg-primary px-3 text-sm text-primary-foreground">{action}</button>
      </div>
    </div>
  );
}

export function FormSection({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="rounded-md border border-border bg-surface p-5">
      <h2 className="border-b border-border pb-3 text-sm font-semibold">{title}</h2>
      <div className="mt-4 grid gap-4">{children}</div>
    </section>
  );
}

export function AIResultPanel({ title, status, children }: { title: string; status: string; children: ReactNode }) {
  return (
    <section className="rounded-md border border-accent/30 bg-accent/5 p-5">
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <Bot className="h-4 w-4 text-accent" aria-hidden />
          <h2 className="text-sm font-semibold">{title}</h2>
        </div>
        <StatusBadge status={status} />
      </div>
      <div className="mt-4 rounded-md border border-border bg-surface p-3 text-sm leading-6">{children}</div>
      <p className="mt-3 text-xs text-muted-foreground">AI-generated content must be reviewed by authorized clinical staff.</p>
    </section>
  );
}

export function AuditTimeline({ events }: { events: { title: string; detail: string; time: string }[] }) {
  return (
    <div className="grid gap-3">
      {events.map((event) => (
        <div key={`${event.title}-${event.time}`} className="grid grid-cols-[1rem_1fr] gap-3 text-sm">
          <span className="mt-1 h-2.5 w-2.5 rounded-full bg-accent" aria-hidden />
          <span>
            <span className="block font-medium">{event.title}</span>
            <span className="block text-muted-foreground">{event.detail} · {event.time}</span>
          </span>
        </div>
      ))}
    </div>
  );
}

export function MiniBars({ values }: { values: number[] }) {
  const max = Math.max(...values, 1);
  return (
    <div className="flex h-36 items-end gap-2">
      {values.map((value, index) => (
        <span key={index} className="flex-1 rounded-t bg-accent/80" style={{ height: `${Math.max(8, (value / max) * 100)}%` }} aria-label={`${value}`} />
      ))}
    </div>
  );
}

export { CalendarDays, UsersRound, Activity, CreditCard, Stethoscope, CheckCircle2 };
