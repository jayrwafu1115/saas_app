import { Activity, Database, KeyRound, Layers3, Server, ShieldCheck } from "lucide-react";
import { Button } from "@/components/ui/button";

const foundationItems = [
  { label: "Clean Architecture", detail: "API, Application, Domain, Infrastructure", icon: Layers3 },
  { label: "Multi-Tenant Core", detail: "Tenant context and UUID persistence foundation", icon: ShieldCheck },
  { label: "Data Platform", detail: "PostgreSQL, EF Core migrations, Redis cache", icon: Database },
  { label: "API Operations", detail: "Swagger, Serilog, Sentry, health checks", icon: Server },
];

export default function Home() {
  return (
    <main className="min-h-screen bg-background text-foreground">
      <header className="border-b border-border bg-surface">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
              Phase 1 Foundation
            </p>
            <h1 className="text-xl font-semibold">Clinic Management SaaS</h1>
          </div>
          <Button variant="outline">
            <Activity className="h-4 w-4" aria-hidden="true" />
            Health
          </Button>
        </div>
      </header>

      <section className="mx-auto grid max-w-6xl gap-8 px-6 py-10 lg:grid-cols-[1.3fr_0.7fr]">
        <div className="space-y-8">
          <div className="space-y-3">
            <p className="text-sm font-medium text-accent">Production foundation</p>
            <h2 className="max-w-3xl text-4xl font-semibold leading-tight">
              Multi-tenant SaaS architecture is ready for business modules.
            </h2>
            <p className="max-w-2xl text-base leading-7 text-muted-foreground">
              This first phase establishes the platform shell, backend boundaries,
              infrastructure wiring, Docker topology, and CI gates.
            </p>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            {foundationItems.map((item) => (
              <article key={item.label} className="rounded-md border border-border bg-surface p-5">
                <item.icon className="mb-4 h-5 w-5 text-accent" aria-hidden="true" />
                <h3 className="text-base font-semibold">{item.label}</h3>
                <p className="mt-2 text-sm leading-6 text-muted-foreground">{item.detail}</p>
              </article>
            ))}
          </div>
        </div>

        <aside className="rounded-md border border-border bg-surface p-5">
          <div className="flex items-center gap-3 border-b border-border pb-4">
            <KeyRound className="h-5 w-5 text-accent" aria-hidden="true" />
            <div>
              <h2 className="text-base font-semibold">RBAC Baseline</h2>
              <p className="text-sm text-muted-foreground">Configured for Phase 1 only</p>
            </div>
          </div>
          <dl className="mt-4 grid gap-3 text-sm">
            {["Super Admin", "Clinic Owner", "Clinic Admin", "Doctor", "Nurse", "Receptionist", "Patient"].map(
              (role) => (
                <div key={role} className="flex items-center justify-between rounded-md bg-muted px-3 py-2">
                  <dt>{role}</dt>
                  <dd className="text-muted-foreground">Role</dd>
                </div>
              ),
            )}
          </dl>
        </aside>
      </section>
    </main>
  );
}
