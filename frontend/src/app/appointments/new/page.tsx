"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { ArrowLeft, Loader2, Save } from "lucide-react";
import { useRouter } from "next/navigation";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { Button } from "@/components/ui/button";
import { createAppointment, getLocations, getTenants, searchPatients } from "@/lib/api";
import { useAppStore } from "@/store/app-store";
import { useAuthStore } from "@/store/auth-store";

export default function NewAppointmentPage() {
  const router = useRouter();
  const tenantId = useAppStore((state) => state.tenantId);
  const setTenantId = useAppStore((state) => state.setTenantId);
  const user = useAuthStore((state) => state.user);
  const tenantsQuery = useQuery({ queryKey: ["tenants"], queryFn: getTenants });
  const locationsQuery = useQuery({ queryKey: ["locations", tenantId], queryFn: () => getLocations(tenantId), enabled: Boolean(tenantId) });
  const patientsQuery = useQuery({ queryKey: ["patients", tenantId], queryFn: () => searchPatients({ tenantId, pageSize: 100 }), enabled: Boolean(tenantId) });
  const [locationId, setLocationId] = useState("");
  const [patientId, setPatientId] = useState("");
  const [doctorUserId, setDoctorUserId] = useState("");
  const [date, setDate] = useState(new Date().toISOString().slice(0, 10));
  const [startTime, setStartTime] = useState("09:00");
  const [durationMinutes, setDurationMinutes] = useState("30");
  const [reason, setReason] = useState("");
  const [notes, setNotes] = useState("");

  useEffect(() => {
    if (!tenantId && tenantsQuery.data?.[0]) setTenantId(tenantsQuery.data[0].id);
  }, [setTenantId, tenantId, tenantsQuery.data]);

  const selectedLocationId = locationId || locationsQuery.data?.[0]?.id || "";
  const selectedPatientId = patientId || patientsQuery.data?.items[0]?.id || "";
  const selectedDoctorUserId = doctorUserId || user?.id || "";
  const mutation = useMutation({
    mutationFn: createAppointment,
    onSuccess: () => router.push("/appointments"),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!tenantId || !selectedLocationId || !selectedPatientId || !isGuid(selectedDoctorUserId)) return;
    const startsAtUtc = new Date(`${date}T${startTime}:00Z`);
    const endsAtUtc = new Date(startsAtUtc.getTime() + Number(durationMinutes) * 60 * 1000);
    mutation.mutate({
      tenantId,
      locationId: selectedLocationId,
      patientId: selectedPatientId,
      doctorUserId: selectedDoctorUserId,
      startsAtUtc: startsAtUtc.toISOString(),
      endsAtUtc: endsAtUtc.toISOString(),
      reason,
      notes,
    });
  }

  return (
    <ProtectedRoute>
      <main className="min-h-screen bg-background text-foreground">
        <header className="border-b border-border bg-surface">
          <div className="mx-auto flex max-w-4xl items-center gap-3 px-6 py-4">
            <Button variant="ghost" size="icon" asChild aria-label="Back">
              <Link href="/appointments"><ArrowLeft className="h-4 w-4" aria-hidden="true" /></Link>
            </Button>
            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Appointment Form</p>
              <h1 className="text-xl font-semibold">New Appointment</h1>
            </div>
          </div>
        </header>
        <form onSubmit={handleSubmit} className="mx-auto grid max-w-4xl gap-4 px-6 py-8 sm:grid-cols-2">
          <label className="grid gap-2 text-sm font-medium">
            Tenant
            <select className="h-10 rounded-md border border-border bg-surface px-3" value={tenantId ?? ""} onChange={(event) => setTenantId(event.target.value || undefined)} required>
              <option value="">Select tenant</option>
              {tenantsQuery.data?.map((tenant) => <option key={tenant.id} value={tenant.id}>{tenant.name}</option>)}
            </select>
          </label>
          <label className="grid gap-2 text-sm font-medium">
            Location
            <select className="h-10 rounded-md border border-border bg-surface px-3" value={selectedLocationId} onChange={(event) => setLocationId(event.target.value)} required>
              {locationsQuery.data?.map((location) => <option key={location.id} value={location.id}>{location.name}</option>)}
            </select>
          </label>
          <label className="grid gap-2 text-sm font-medium">
            Patient
            <select className="h-10 rounded-md border border-border bg-surface px-3" value={selectedPatientId} onChange={(event) => setPatientId(event.target.value)} required>
              {patientsQuery.data?.items.map((patient) => <option key={patient.id} value={patient.id}>{patient.firstName} {patient.lastName}</option>)}
            </select>
          </label>
          <Field label="Doctor User Id" value={selectedDoctorUserId} onChange={setDoctorUserId} pattern={guidPattern} />
          <Field label="Date" value={date} onChange={setDate} type="date" />
          <Field label="Start Time" value={startTime} onChange={setStartTime} type="time" />
          <Field label="Duration Minutes" value={durationMinutes} onChange={setDurationMinutes} type="number" />
          <Field label="Reason" value={reason} onChange={setReason} />
          <label className="grid gap-2 text-sm font-medium sm:col-span-2">
            Notes
            <textarea className="min-h-24 rounded-md border border-border bg-surface px-3 py-2" value={notes} onChange={(event) => setNotes(event.target.value)} />
          </label>
          {!isGuid(selectedDoctorUserId) ? (
            <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700 sm:col-span-2">Doctor user ID must be a valid GUID.</p>
          ) : null}
          {mutation.isError ? <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700 sm:col-span-2">Appointment could not be saved.</p> : null}
          <Button className="sm:col-span-2" type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" /> : <Save className="h-4 w-4" aria-hidden="true" />}
            Save
          </Button>
        </form>
      </main>
    </ProtectedRoute>
  );
}

const guidPattern = "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";

function isGuid(value: string) {
  return new RegExp(guidPattern).test(value);
}

function Field({ label, value, onChange, type = "text", pattern }: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  type?: string;
  pattern?: string;
}) {
  return (
    <label className="grid gap-2 text-sm font-medium">
      {label}
      <input className="h-10 rounded-md border border-border bg-surface px-3" value={value} onChange={(event) => onChange(event.target.value)} type={type} pattern={pattern} required />
    </label>
  );
}
