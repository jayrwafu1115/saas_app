"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Loader2, MapPinned, Plus, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { createLocation, getLocations, getTenants } from "@/lib/api";
import { useAppStore } from "@/store/app-store";

export default function LocationManagementPage() {
  const queryClient = useQueryClient();
  const tenantId = useAppStore((state) => state.tenantId);
  const setTenantId = useAppStore((state) => state.setTenantId);
  const [name, setName] = useState("");
  const [code, setCode] = useState("");
  const [address, setAddress] = useState("");
  const [phone, setPhone] = useState("");

  const tenantsQuery = useQuery({ queryKey: ["tenants"], queryFn: getTenants });
  const locationsQuery = useQuery({
    queryKey: ["locations", tenantId],
    queryFn: () => getLocations(tenantId),
  });
  const createLocationMutation = useMutation({
    mutationFn: createLocation,
    onSuccess: () => {
      setName("");
      setCode("");
      setAddress("");
      setPhone("");
      queryClient.invalidateQueries({ queryKey: ["locations"] });
    },
  });

  useEffect(() => {
    if (!tenantId && tenantsQuery.data?.[0]) {
      setTenantId(tenantsQuery.data[0].id);
    }
  }, [setTenantId, tenantId, tenantsQuery.data]);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!tenantId) {
      return;
    }

    createLocationMutation.mutate({ tenantId, name, code, address, phone });
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
              <h1 className="text-xl font-semibold">Locations</h1>
            </div>
          </div>
          <Button variant="outline" onClick={() => locationsQuery.refetch()}>
            <RefreshCw className="h-4 w-4" aria-hidden="true" />
            Refresh
          </Button>
        </div>
      </header>

      <section className="mx-auto grid max-w-6xl gap-6 px-6 py-8 lg:grid-cols-[0.8fr_1.2fr]">
        <form onSubmit={handleSubmit} className="rounded-md border border-border bg-surface p-5">
          <div className="flex items-center gap-3 border-b border-border pb-4">
            <MapPinned className="h-5 w-5 text-accent" aria-hidden="true" />
            <h2 className="text-base font-semibold">New Location</h2>
          </div>
          <div className="mt-5 grid gap-4">
            <label className="grid gap-2 text-sm font-medium">
              Tenant
              <select className="h-10 rounded-md border border-border bg-background px-3" value={tenantId ?? ""} onChange={(event) => setTenantId(event.target.value || undefined)} required>
                <option value="">Select tenant</option>
                {tenantsQuery.data?.map((tenant) => (
                  <option key={tenant.id} value={tenant.id}>{tenant.name}</option>
                ))}
              </select>
            </label>
            <label className="grid gap-2 text-sm font-medium">
              Name
              <input className="h-10 rounded-md border border-border bg-background px-3" value={name} onChange={(event) => setName(event.target.value)} required />
            </label>
            <label className="grid gap-2 text-sm font-medium">
              Code
              <input className="h-10 rounded-md border border-border bg-background px-3 uppercase" value={code} onChange={(event) => setCode(event.target.value)} required />
            </label>
            <label className="grid gap-2 text-sm font-medium">
              Address
              <input className="h-10 rounded-md border border-border bg-background px-3" value={address} onChange={(event) => setAddress(event.target.value)} required />
            </label>
            <label className="grid gap-2 text-sm font-medium">
              Phone
              <input className="h-10 rounded-md border border-border bg-background px-3" value={phone} onChange={(event) => setPhone(event.target.value)} required />
            </label>
            {createLocationMutation.isError ? (
              <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">Location could not be created.</p>
            ) : null}
            <Button type="submit" disabled={createLocationMutation.isPending || !tenantId}>
              {createLocationMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" /> : <Plus className="h-4 w-4" aria-hidden="true" />}
              Create
            </Button>
          </div>
        </form>

        <div className="rounded-md border border-border bg-surface">
          <div className="grid grid-cols-[0.9fr_0.4fr_1.1fr_0.6fr] border-b border-border px-4 py-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
            <span>Name</span>
            <span>Code</span>
            <span>Address</span>
            <span>Phone</span>
          </div>
          <div className="divide-y divide-border">
            {locationsQuery.isLoading ? (
              <div className="flex items-center gap-2 px-4 py-6 text-sm text-muted-foreground">
                <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
                Loading
              </div>
            ) : locationsQuery.data?.length ? (
              locationsQuery.data.map((location) => (
                <div key={location.id} className="grid grid-cols-[0.9fr_0.4fr_1.1fr_0.6fr] px-4 py-3 text-sm">
                  <span className="font-medium">{location.name}</span>
                  <span className="text-muted-foreground">{location.code}</span>
                  <span>{location.address}</span>
                  <span>{location.phone}</span>
                </div>
              ))
            ) : (
              <div className="px-4 py-6 text-sm text-muted-foreground">No locations</div>
            )}
          </div>
        </div>
      </section>
      </main>
    </ProtectedRoute>
  );
}
