"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { ArrowLeft, Loader2, Save } from "lucide-react";
import { useParams, useRouter } from "next/navigation";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { Button } from "@/components/ui/button";
import { getLocations, getPatient, updatePatient, type PatientDetail } from "@/lib/api";

export default function EditPatientPage() {
  const params = useParams<{ id: string }>();
  const patientId = params.id;
  const patientQuery = useQuery({ queryKey: ["patient", patientId], queryFn: () => getPatient(patientId) });
  const patient = patientQuery.data;

  return (
    <ProtectedRoute>
      <main className="min-h-screen bg-background text-foreground">
        <header className="border-b border-border bg-surface">
          <div className="mx-auto flex max-w-4xl items-center gap-3 px-6 py-4">
            <Button variant="ghost" size="icon" asChild aria-label="Back">
              <Link href={`/patients/${patientId}`}><ArrowLeft className="h-4 w-4" aria-hidden="true" /></Link>
            </Button>
            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Patient Form</p>
              <h1 className="text-xl font-semibold">Edit Patient</h1>
            </div>
          </div>
        </header>
        {patient ? (
          <EditPatientForm key={patient.id} patient={patient} />
        ) : (
          <div className="mx-auto flex max-w-4xl items-center gap-2 px-6 py-8 text-sm text-muted-foreground">
            <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
            Loading
          </div>
        )}
      </main>
    </ProtectedRoute>
  );
}

function EditPatientForm({ patient }: { patient: PatientDetail }) {
  const router = useRouter();
  const locationsQuery = useQuery({
    queryKey: ["locations", patient.tenantId],
    queryFn: () => getLocations(patient.tenantId),
  });
  const [locationId, setLocationId] = useState(patient.locationId);
  const [medicalRecordNumber, setMedicalRecordNumber] = useState(patient.medicalRecordNumber);
  const [firstName, setFirstName] = useState(patient.firstName);
  const [middleName, setMiddleName] = useState(patient.middleName);
  const [lastName, setLastName] = useState(patient.lastName);
  const [birthDate, setBirthDate] = useState(patient.birthDate);
  const [gender, setGender] = useState(patient.gender);
  const [email, setEmail] = useState(patient.email);
  const [phone, setPhone] = useState(patient.phone);
  const [address, setAddress] = useState(patient.address);
  const mutation = useMutation({
    mutationFn: () => updatePatient(patient.id, {
      locationId,
      medicalRecordNumber,
      firstName,
      middleName,
      lastName,
      birthDate,
      gender,
      email,
      phone,
      address,
    }),
    onSuccess: () => router.push(`/patients/${patient.id}`),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    mutation.mutate();
  }

  return (
    <form onSubmit={handleSubmit} className="mx-auto grid max-w-4xl gap-4 px-6 py-8 sm:grid-cols-2">
          <label className="grid gap-2 text-sm font-medium">
            Location
            <select className="h-10 rounded-md border border-border bg-surface px-3" value={locationId} onChange={(event) => setLocationId(event.target.value)} required>
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
          <Button className="sm:col-span-2" type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" /> : <Save className="h-4 w-4" aria-hidden="true" />}
            Save
          </Button>
    </form>
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
