import Link from "next/link";
import { Activity, Building2, CalendarDays, ClipboardList, Database, LogIn, MapPinned, Server, ShieldCheck, UsersRound } from "lucide-react";
import { Button } from "@/components/ui/button";

const foundationItems = [
  { label: "Tenant Registry", detail: "Tenant profile, slug, status, and settings", icon: Building2, href: "/tenants" },
  { label: "Location Directory", detail: "Clinic sites scoped by tenant context", icon: MapPinned, href: "/locations" },
  { label: "Patient Management", detail: "Search, profiles, documents, and timeline", icon: UsersRound, href: "/patients" },
  { label: "Appointments", detail: "Daily, weekly, and monthly schedules", icon: CalendarDays, href: "/appointments" },
  { label: "Clinical Encounters", detail: "SOAP notes, vitals, diagnoses, prescriptions", icon: ClipboardList, href: "/encounters" },
  { label: "Tenant Resolution", detail: "Header resolver and scoped tenant context", icon: ShieldCheck, href: "/locations" },
  { label: "Data Platform", detail: "PostgreSQL, EF Core migrations, MinIO documents", icon: Database, href: "/patients" },
];

export default function Home() {
  return (
    <main className="min-h-screen bg-background text-foreground">
      <header className="border-b border-border bg-surface">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
              Phase 2 Foundation
            </p>
            <h1 className="text-xl font-semibold">Clinic Management SaaS</h1>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <Button variant="outline" asChild>
            <Link href="/tenants">
              <Building2 className="h-4 w-4" aria-hidden="true" />
              Tenants
            </Link>
            </Button>
            <Button variant="outline" asChild>
            <Link href="/locations">
              <MapPinned className="h-4 w-4" aria-hidden="true" />
              Locations
            </Link>
            </Button>
            <Button variant="outline" asChild>
              <Link href="/patients">
                <UsersRound className="h-4 w-4" aria-hidden="true" />
                Patients
              </Link>
            </Button>
            <Button variant="outline" asChild>
              <Link href="/appointments">
                <CalendarDays className="h-4 w-4" aria-hidden="true" />
                Appointments
              </Link>
            </Button>
            <Button variant="outline" asChild>
              <Link href="/encounters">
                <ClipboardList className="h-4 w-4" aria-hidden="true" />
                Encounters
              </Link>
            </Button>
            <Button variant="outline" asChild>
              <Link href="/login">
                <LogIn className="h-4 w-4" aria-hidden="true" />
                Login
              </Link>
            </Button>
            <Button variant="ghost">
              <Activity className="h-4 w-4" aria-hidden="true" />
              Health
            </Button>
          </div>
        </div>
      </header>

      <section className="mx-auto grid max-w-6xl gap-8 px-6 py-10 lg:grid-cols-[1.3fr_0.7fr]">
        <div className="space-y-8">
          <div className="space-y-3">
            <p className="text-sm font-medium text-accent">Multi-tenant foundation</p>
            <h2 className="max-w-3xl text-4xl font-semibold leading-tight">
              Tenant and location operations are ready for business modules.
            </h2>
            <p className="max-w-2xl text-base leading-7 text-muted-foreground">
              The platform now includes tenants, locations, authentication, authorization,
              patient CRUD, document uploads, appointments, and clinical encounters.
            </p>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            {foundationItems.map((item) => (
              <Link
                key={item.label}
                href={item.href}
                className="rounded-md border border-border bg-surface p-5 transition-colors hover:border-accent"
              >
                <item.icon className="mb-4 h-5 w-5 text-accent" aria-hidden="true" />
                <h3 className="text-base font-semibold">{item.label}</h3>
                <p className="mt-2 text-sm leading-6 text-muted-foreground">{item.detail}</p>
              </Link>
            ))}
          </div>
        </div>

        <aside className="rounded-md border border-border bg-surface p-5">
          <div className="flex items-center gap-3 border-b border-border pb-4">
            <Server className="h-5 w-5 text-accent" aria-hidden="true" />
            <div>
              <h2 className="text-base font-semibold">API Surface</h2>
              <p className="text-sm text-muted-foreground">Phase 2 endpoints</p>
            </div>
          </div>
          <dl className="mt-4 grid gap-3 text-sm">
            {["POST /api/encounters", "POST /api/encounters/{id}/vitals", "POST /api/encounters/{id}/diagnoses", "GET /api/encounters/{id}/pdf"].map(
              (endpoint) => (
                <div key={endpoint} className="flex items-center justify-between rounded-md bg-muted px-3 py-2">
                  <dt>{endpoint}</dt>
                  <dd className="text-muted-foreground">Ready</dd>
                </div>
              ),
            )}
          </dl>
        </aside>
      </section>
    </main>
  );
}
