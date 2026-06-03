"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, ClipboardList, FileUp, Loader2, Pencil, Plus, UserRound } from "lucide-react";
import { useParams } from "next/navigation";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { Button } from "@/components/ui/button";
import { createPatientContact, getPatient, getPatientTimeline, uploadPatientDocument } from "@/lib/api";

export default function PatientProfilePage() {
  const params = useParams<{ id: string }>();
  const patientId = params.id;
  const queryClient = useQueryClient();
  const patientQuery = useQuery({ queryKey: ["patient", patientId], queryFn: () => getPatient(patientId) });
  const timelineQuery = useQuery({ queryKey: ["patient-timeline", patientId], queryFn: () => getPatientTimeline(patientId) });
  const [contactName, setContactName] = useState("");
  const [relationship, setRelationship] = useState("");
  const [contactEmail, setContactEmail] = useState("");
  const [contactPhone, setContactPhone] = useState("");
  const contactMutation = useMutation({
    mutationFn: () => createPatientContact(patientId, { name: contactName, relationship, email: contactEmail, phone: contactPhone, isPrimary: false }),
    onSuccess: () => {
      setContactName("");
      setRelationship("");
      setContactEmail("");
      setContactPhone("");
      queryClient.invalidateQueries({ queryKey: ["patient", patientId] });
      queryClient.invalidateQueries({ queryKey: ["patient-timeline", patientId] });
    },
  });
  const uploadMutation = useMutation({
    mutationFn: (file: File) => uploadPatientDocument(patientId, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["patient", patientId] });
      queryClient.invalidateQueries({ queryKey: ["patient-timeline", patientId] });
    },
  });

  function handleContactSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    contactMutation.mutate();
  }

  const patient = patientQuery.data;

  return (
    <ProtectedRoute>
      <main className="min-h-screen bg-background text-foreground">
        <header className="border-b border-border bg-surface">
          <div className="mx-auto flex max-w-6xl items-center gap-3 px-6 py-4">
            <Button variant="ghost" size="icon" asChild aria-label="Back">
              <Link href="/patients"><ArrowLeft className="h-4 w-4" aria-hidden="true" /></Link>
            </Button>
            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Patient Profile</p>
              <h1 className="text-xl font-semibold">{patient ? `${patient.firstName} ${patient.lastName}` : "Loading"}</h1>
            </div>
            <Button className="ml-auto" variant="outline" asChild>
              <Link href="/encounters">
                <ClipboardList className="h-4 w-4" aria-hidden="true" />
                Encounters
              </Link>
            </Button>
            <Button variant="outline" asChild>
              <Link href={`/patients/${patientId}/edit`}>
                <Pencil className="h-4 w-4" aria-hidden="true" />
                Edit
              </Link>
            </Button>
          </div>
        </header>

        {patientQuery.isLoading ? (
          <div className="mx-auto flex max-w-6xl items-center gap-2 px-6 py-8 text-sm text-muted-foreground">
            <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
            Loading
          </div>
        ) : patient ? (
          <section className="mx-auto grid max-w-6xl gap-6 px-6 py-8 lg:grid-cols-[1fr_0.8fr]">
            <div className="space-y-6">
              <article className="rounded-md border border-border bg-surface p-5">
                <div className="flex items-center gap-3 border-b border-border pb-4">
                  <UserRound className="h-5 w-5 text-accent" aria-hidden="true" />
                  <h2 className="text-base font-semibold">{patient.medicalRecordNumber}</h2>
                </div>
                <dl className="mt-4 grid gap-3 text-sm sm:grid-cols-2">
                  <Info label="Birth Date" value={patient.birthDate} />
                  <Info label="Gender" value={patient.gender} />
                  <Info label="Email" value={patient.email} />
                  <Info label="Phone" value={patient.phone} />
                  <Info label="Address" value={patient.address} wide />
                </dl>
              </article>

              <article className="rounded-md border border-border bg-surface p-5">
                <h2 className="border-b border-border pb-4 text-base font-semibold">Documents</h2>
                <label className="mt-4 flex cursor-pointer items-center justify-center gap-2 rounded-md border border-dashed border-border px-4 py-6 text-sm text-muted-foreground">
                  <FileUp className="h-4 w-4" aria-hidden="true" />
                  Upload
                  <input className="hidden" type="file" onChange={(event) => {
                    const file = event.target.files?.[0];
                    if (file) uploadMutation.mutate(file);
                  }} />
                </label>
                <div className="mt-4 divide-y divide-border">
                  {patient.documents.map((document) => (
                    <div key={document.id} className="py-3 text-sm">
                      <p className="font-medium">{document.fileName}</p>
                      <p className="text-muted-foreground">{document.contentType} · {document.sizeBytes} bytes</p>
                    </div>
                  ))}
                </div>
              </article>
            </div>

            <aside className="space-y-6">
              <article className="rounded-md border border-border bg-surface p-5">
                <h2 className="border-b border-border pb-4 text-base font-semibold">Contacts</h2>
                <form onSubmit={handleContactSubmit} className="mt-4 grid gap-3">
                  <input className="h-10 rounded-md border border-border bg-background px-3 text-sm" placeholder="Name" value={contactName} onChange={(event) => setContactName(event.target.value)} required />
                  <input className="h-10 rounded-md border border-border bg-background px-3 text-sm" placeholder="Relationship" value={relationship} onChange={(event) => setRelationship(event.target.value)} required />
                  <input className="h-10 rounded-md border border-border bg-background px-3 text-sm" placeholder="Email" value={contactEmail} onChange={(event) => setContactEmail(event.target.value)} />
                  <input className="h-10 rounded-md border border-border bg-background px-3 text-sm" placeholder="Phone" value={contactPhone} onChange={(event) => setContactPhone(event.target.value)} required />
                  <Button type="submit" disabled={contactMutation.isPending}>
                    {contactMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" /> : <Plus className="h-4 w-4" aria-hidden="true" />}
                    Add
                  </Button>
                </form>
                <div className="mt-4 divide-y divide-border">
                  {patient.contacts.map((contact) => (
                    <div key={contact.id} className="py-3 text-sm">
                      <p className="font-medium">{contact.name}</p>
                      <p className="text-muted-foreground">{contact.relationship} · {contact.phone}</p>
                    </div>
                  ))}
                </div>
              </article>

              <article className="rounded-md border border-border bg-surface p-5">
                <h2 className="border-b border-border pb-4 text-base font-semibold">Timeline</h2>
                <div className="mt-4 grid gap-3">
                  {timelineQuery.data?.map((event) => (
                    <div key={`${event.type}-${event.occurredAtUtc}`} className="rounded-md bg-muted px-3 py-2 text-sm">
                      <p className="font-medium">{event.title}</p>
                      <p className="text-muted-foreground">{event.description}</p>
                    </div>
                  ))}
                </div>
              </article>
            </aside>
          </section>
        ) : null}
      </main>
    </ProtectedRoute>
  );
}

function Info({ label, value, wide = false }: { label: string; value: string; wide?: boolean }) {
  return (
    <div className={wide ? "sm:col-span-2" : undefined}>
      <dt className="text-muted-foreground">{label}</dt>
      <dd className="mt-1 font-medium">{value}</dd>
    </div>
  );
}
