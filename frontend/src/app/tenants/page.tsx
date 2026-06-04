"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { ArrowLeft, Building2, Loader2, Plus, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { createTenant, getTenants } from "@/lib/api";

export default function TenantManagementPage() {
  const queryClient = useQueryClient();
  const [name, setName] = useState("");
  const [slug, setSlug] = useState("");
  const [status, setStatus] = useState("Active");
  const [settingsJson, setSettingsJson] = useState("{}");

  const tenantsQuery = useQuery({ queryKey: ["tenants"], queryFn: getTenants });
  const createTenantMutation = useMutation({
    mutationFn: createTenant,
    onSuccess: () => {
      setName("");
      setSlug("");
      setStatus("Active");
      setSettingsJson("{}");
      queryClient.invalidateQueries({ queryKey: ["tenants"] });
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    createTenantMutation.mutate({ name, slug, status, settingsJson });
  }

  return (
    <ProtectedRoute>
      <main className="min-h-screen bg-background text-foreground">
      <header className="border-b border-border bg-surface">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-6 py-4">
          <div className="flex items-center gap-3">
            <Button variant="ghost" size="icon" asChild aria-label="Back">
              <Link href="/">
                <ArrowLeft className="h-4 w-4" aria-hidden="true" />
              </Link>
            </Button>
            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Management</p>
              <h1 className="text-xl font-semibold">Tenants</h1>
            </div>
          </div>
          <Button variant="outline" onClick={() => tenantsQuery.refetch()}>
            <RefreshCw className="h-4 w-4" aria-hidden="true" />
            Refresh
          </Button>
        </div>
      </header>

      <section className="mx-auto grid max-w-6xl gap-6 px-6 py-8 lg:grid-cols-[0.8fr_1.2fr]">
        <form onSubmit={handleSubmit} className="rounded-md border border-border bg-surface p-5">
          <div className="flex items-center gap-3 border-b border-border pb-4">
            <Building2 className="h-5 w-5 text-accent" aria-hidden="true" />
            <h2 className="text-base font-semibold">New Tenant</h2>
          </div>
          <div className="mt-5 grid gap-4">
            <label className="grid gap-2 text-sm font-medium">
              Name
              <input className="h-10 rounded-md border border-border bg-background px-3" value={name} onChange={(event) => setName(event.target.value)} required />
            </label>
            <label className="grid gap-2 text-sm font-medium">
              Slug
              <input className="h-10 rounded-md border border-border bg-background px-3" value={slug} onChange={(event) => setSlug(event.target.value)} pattern="[a-z0-9-]+" required />
            </label>
            <label className="grid gap-2 text-sm font-medium">
              Status
              <select className="h-10 rounded-md border border-border bg-background px-3" value={status} onChange={(event) => setStatus(event.target.value)}>
                <option>Active</option>
                <option>Suspended</option>
                <option>Provisioning</option>
              </select>
            </label>
            <label className="grid gap-2 text-sm font-medium">
              Settings JSON
              <textarea className="min-h-28 rounded-md border border-border bg-background px-3 py-2 font-mono text-sm" value={settingsJson} onChange={(event) => setSettingsJson(event.target.value)} required />
            </label>
            {createTenantMutation.isError ? (
              <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
                {createTenantMutation.error instanceof AxiosError && createTenantMutation.error.response?.status === 401
                  ? "Your session expired. Please log in again."
                  : "Tenant could not be created."}
              </p>
            ) : null}
            <Button type="submit" disabled={createTenantMutation.isPending}>
              {createTenantMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" /> : <Plus className="h-4 w-4" aria-hidden="true" />}
              Create
            </Button>
          </div>
        </form>

        <div className="rounded-md border border-border bg-surface">
          <div className="grid grid-cols-[1.2fr_0.9fr_0.5fr] border-b border-border px-4 py-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
            <span>Name</span>
            <span>Slug</span>
            <span>Status</span>
          </div>
          <div className="divide-y divide-border">
            {tenantsQuery.isLoading ? (
              <div className="flex items-center gap-2 px-4 py-6 text-sm text-muted-foreground">
                <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
                Loading
              </div>
            ) : tenantsQuery.data?.length ? (
              tenantsQuery.data.map((tenant) => (
                <div key={tenant.id} className="grid grid-cols-[1.2fr_0.9fr_0.5fr] px-4 py-3 text-sm">
                  <span className="font-medium">{tenant.name}</span>
                  <span className="text-muted-foreground">{tenant.slug}</span>
                  <span>{tenant.status}</span>
                </div>
              ))
            ) : (
              <div className="px-4 py-6 text-sm text-muted-foreground">No tenants</div>
            )}
          </div>
        </div>
      </section>
      </main>
    </ProtectedRoute>
  );
}
