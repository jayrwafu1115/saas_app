"use client";

import Link from "next/link";
import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, CreditCard, Gauge, Play, WalletCards } from "lucide-react";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { Button } from "@/components/ui/button";
import { createBillingCheckout, getBillingOverview, getSubscriptionPlans, getTenants, startSubscriptionTrial } from "@/lib/api";

export default function BillingPage() {
  const queryClient = useQueryClient();
  const [tenantId, setTenantId] = useState("");
  const [planCode, setPlanCode] = useState("starter");
  const [provider, setProvider] = useState<"GCash" | "Maya">("GCash");
  const tenantsQuery = useQuery({ queryKey: ["tenants"], queryFn: getTenants });
  const plansQuery = useQuery({ queryKey: ["subscription-plans"], queryFn: getSubscriptionPlans });
  const overviewQuery = useQuery({ queryKey: ["billing-overview", tenantId], queryFn: () => getBillingOverview(tenantId || undefined) });
  const refresh = () => queryClient.invalidateQueries({ queryKey: ["billing-overview"] });
  const trialMutation = useMutation({ mutationFn: () => startSubscriptionTrial({ tenantId, planCode }), onSuccess: refresh });
  const checkoutMutation = useMutation({ mutationFn: () => createBillingCheckout({ tenantId, planCode, provider }) });

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
                <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">SaaS Billing</p>
                <h1 className="text-xl font-semibold">Subscriptions</h1>
              </div>
            </div>
          </div>
        </header>

        <section className="mx-auto grid max-w-6xl gap-6 px-6 py-8 lg:grid-cols-[0.8fr_1.2fr]">
          <aside className="rounded-md border border-border bg-surface p-5">
            <div className="mb-4 flex items-center gap-2">
              <WalletCards className="h-4 w-4 text-accent" aria-hidden="true" />
              <h2 className="text-sm font-semibold">Manage Tenant</h2>
            </div>
            <div className="grid gap-3">
              <select className="h-10 rounded-md border border-border bg-background px-3 text-sm" value={tenantId} onChange={(event) => setTenantId(event.target.value)}>
                <option value="">Select tenant</option>
                {tenantsQuery.data?.map((tenant) => <option key={tenant.id} value={tenant.id}>{tenant.name}</option>)}
              </select>
              <select className="h-10 rounded-md border border-border bg-background px-3 text-sm" value={planCode} onChange={(event) => setPlanCode(event.target.value)}>
                {plansQuery.data?.map((plan) => <option key={plan.id} value={plan.code}>{plan.name} - PHP {plan.monthlyPricePhp}</option>)}
              </select>
              <div className="flex rounded-md border border-border bg-background p-1">
                {(["GCash", "Maya"] as const).map((item) => (
                  <button key={item} type="button" className={`h-9 flex-1 rounded text-sm ${provider === item ? "bg-foreground text-background" : "text-muted-foreground"}`} onClick={() => setProvider(item)}>
                    {item}
                  </button>
                ))}
              </div>
              <Button type="button" disabled={!tenantId} onClick={() => trialMutation.mutate()}>
                <Play className="h-4 w-4" aria-hidden="true" />
                Start Trial
              </Button>
              <Button type="button" variant="outline" disabled={!tenantId} onClick={() => checkoutMutation.mutate()}>
                <CreditCard className="h-4 w-4" aria-hidden="true" />
                Create Checkout
              </Button>
              {checkoutMutation.data ? (
                <a className="rounded-md bg-muted px-3 py-2 text-sm text-accent" href={checkoutMutation.data.checkoutUrl} target="_blank" rel="noreferrer">
                  {checkoutMutation.data.providerReference}
                </a>
              ) : null}
            </div>
          </aside>

          <div className="space-y-6">
            <section className="rounded-md border border-border bg-surface p-5">
              <h2 className="mb-4 text-sm font-semibold">Subscription Overview</h2>
              <div className="divide-y divide-border">
                {overviewQuery.data?.subscriptions.length ? overviewQuery.data.subscriptions.map((subscription) => (
                  <div key={subscription.id} className="grid grid-cols-[1fr_8rem_8rem] gap-3 py-3 text-sm">
                    <span className="font-medium">{subscription.tenantId.slice(0, 8)} - {subscription.planName}</span>
                    <span className="text-muted-foreground">{subscription.status}</span>
                    <span className={subscription.isRestricted ? "text-destructive" : "text-accent"}>{subscription.isRestricted ? "Restricted" : "Allowed"}</span>
                  </div>
                )) : <p className="text-sm text-muted-foreground">No subscriptions</p>}
              </div>
            </section>

            <section className="rounded-md border border-border bg-surface p-5">
              <div className="mb-4 flex items-center gap-2">
                <Gauge className="h-4 w-4 text-accent" aria-hidden="true" />
                <h2 className="text-sm font-semibold">Tenant Usage</h2>
              </div>
              <div className="divide-y divide-border">
                {overviewQuery.data?.usage.length ? overviewQuery.data.usage.map((usage) => (
                  <div key={`${usage.tenantId}-${usage.metric}`} className="grid grid-cols-[1fr_6rem_6rem] gap-3 py-3 text-sm">
                    <span className="font-medium">{usage.metric}</span>
                    <span className="text-muted-foreground">{usage.quantity} / {usage.limit}</span>
                    <span className={usage.isOverLimit ? "text-destructive" : "text-accent"}>{usage.isOverLimit ? "Over" : "OK"}</span>
                  </div>
                )) : <p className="text-sm text-muted-foreground">No usage yet</p>}
              </div>
            </section>
          </div>
        </section>
      </main>
    </ProtectedRoute>
  );
}
