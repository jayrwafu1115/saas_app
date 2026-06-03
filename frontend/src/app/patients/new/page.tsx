"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { ArrowLeft, Loader2, Save } from "lucide-react";
import { useRouter } from "next/navigation";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { Button } from "@/components/ui/button";
import { createPatient, getLocations, getTenants } from "@/lib/api";
import { useAppStore } from "@/store/app-store";

export default function NewPatientPage() {
  const router = useRouter();
  const tenantId = useAppStore((state) => state.tenantId);
  const setTenantId = useAppStore((state) => state.setTenantId);
  const tenantsQuery = useQuery({ queryKey: ["tenants"], queryFn: getTenants });
  const locationsQuery = useQuery({ queryKey: ["locations", tenantId], queryFn: () => getLocations(tenantId), enabled: Boolean(tenantId) });
  const [locationId, setLocationId] = useState("");
  const [medicalRecordNumber, setMedicalRecordNumber] = useState("");
  const [firstName, setFirstName] = useState("");
  const [middleName, setMiddleName] = useState("");
  const [lastName, setLastName] = useState("");
  const [birthDate, setBirthDate] = useState("");
  const [gender, setGender] = useState("Female");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [address, setAddress] = useState("");

  useEffect(() => {
    if (!tenantId && tenantsQuery.data?.[0]) {
      setTenantId(tenantsQuery.data[0].id);
    }
  }, [setTenantId, tenantId, tenantsQuery.data]);

  const selectedLocationId = locationId || locationsQuery.data?.[0]?.id || "";

  const mutation = useMutation({
    mutationFn: createPatient,
    onSuccess: (patient) => router.push(`/patients/${patient.id}`),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!tenantId || !selectedLocationId) {
      return;
    }

    mutation.mutate({
      tenantId,
      locationId: selectedLocationId,
      medicalRecordNumber,
      firstName,
      middleName,
      lastName,
      birthDate,
      gender,
      email,
      phone,
      address,
    });
  }

  return (
    <ProtectedRoute>
      <main className="min-h-screen bg-background text-foreground">
        <header className="border-b border-border bg-surface">
          <div className="mx-auto flex max-w-4xl items-center gap-3 px-6 py-4">
            <Button variant="ghost" size="icon" asChild aria-label="Back">
              <Link href="/patients"><ArrowLeft className="h-4 w-4" aria-hidden="true" /></Link>
            </Button>
            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Patient Form</p>
              <h1 className="text-xl font-semibold">New Patient</h1>
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
              <option value="">Select location</option>
              {locationsQuery.data?.map((location) => <option key={location.id} value={location.id}>{location.name}</option>)}
            </select>
          </label>
          <Field label="Medical Record Number" value={medicalRecordNumber} onChange={setMedicalRecordNumber} />
          <Field label="First Name" value={firstName} onChange={setFirstName} />
          <Field label="Middle Name" value={middleName} onChange={setMiddleName} required={false} />
          <Field label="Last Name" value={lastName} onChange={setLastName} />
          <Field label="Birth Date" value={birthDate} onChange={setBirthDate} type="date" />
          <label className="grid gap-2 text-sm font-medium">
            Gender
            <select className="h-10 rounded-md border border-border bg-surface px-3" value={gender} onChange={(event) => setGender(event.target.value)}>
              <option>Female</option>
              <option>Male</option>
              <option>Other</option>
              <option>Unknown</option>
            </select>
          </label>
          <Field label="Email" value={email} onChange={setEmail} type="email" />
          <Field label="Phone" value={phone} onChange={setPhone} />
          <label className="grid gap-2 text-sm font-medium sm:col-span-2">
            Address
            <input className="h-10 rounded-md border border-border bg-surface px-3" value={address} onChange={(event) => setAddress(event.target.value)} required />
          </label>
          {mutation.isError ? <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700 sm:col-span-2">Patient could not be saved.</p> : null}
          <Button className="sm:col-span-2" type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" /> : <Save className="h-4 w-4" aria-hidden="true" />}
            Save
          </Button>
        </form>
      </main>
    </ProtectedRoute>
  );
}

function Field({ label, value, onChange, type = "text", required = true }: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  type?: string;
  required?: boolean;
}) {
  return (
    <label className="grid gap-2 text-sm font-medium">
      {label}
      <input className="h-10 rounded-md border border-border bg-surface px-3" value={value} onChange={(event) => onChange(event.target.value)} type={type} required={required} />
    </label>
  );
}
