"use client";

import Link from "next/link";
import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Loader2, Plus, Search, Trash2, UsersRound } from "lucide-react";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { Button } from "@/components/ui/button";
import { deletePatient, searchPatients } from "@/lib/api";

export default function PatientListPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const patientsQuery = useQuery({
    queryKey: ["patients", search, pageNumber],
    queryFn: () => searchPatients({ search, pageNumber, pageSize: 10 }),
  });
  const deleteMutation = useMutation({
    mutationFn: deletePatient,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["patients"] }),
  });

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
                <h1 className="text-xl font-semibold">Patients</h1>
              </div>
            </div>
            <Button asChild>
              <Link href="/patients/new">
                <Plus className="h-4 w-4" aria-hidden="true" />
                New
              </Link>
            </Button>
          </div>
        </header>

        <section className="mx-auto max-w-6xl px-6 py-8">
          <div className="mb-4 flex items-center gap-3 rounded-md border border-border bg-surface px-3 py-2">
            <Search className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
            <input
              className="h-9 flex-1 bg-transparent text-sm outline-none"
              value={search}
              onChange={(event) => {
                setSearch(event.target.value);
                setPageNumber(1);
              }}
              placeholder="Search by MRN, name, email, or phone"
            />
          </div>

          <div className="rounded-md border border-border bg-surface">
            <div className="grid grid-cols-[0.7fr_1fr_0.8fr_0.8fr_0.3fr] border-b border-border px-4 py-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
              <span>MRN</span>
              <span>Name</span>
              <span>Email</span>
              <span>Phone</span>
              <span />
            </div>
            <div className="divide-y divide-border">
              {patientsQuery.isLoading ? (
                <div className="flex items-center gap-2 px-4 py-6 text-sm text-muted-foreground">
                  <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
                  Loading
                </div>
              ) : patientsQuery.data?.items.length ? (
                patientsQuery.data.items.map((patient) => (
                  <div key={patient.id} className="grid grid-cols-[0.7fr_1fr_0.8fr_0.8fr_0.3fr] items-center px-4 py-3 text-sm">
                    <Link className="font-medium text-accent" href={`/patients/${patient.id}`}>{patient.medicalRecordNumber}</Link>
                    <span>{patient.firstName} {patient.lastName}</span>
                    <span className="text-muted-foreground">{patient.email}</span>
                    <span>{patient.phone}</span>
                    <Button variant="ghost" size="icon" onClick={() => deleteMutation.mutate(patient.id)} aria-label="Delete patient">
                      <Trash2 className="h-4 w-4" aria-hidden="true" />
                    </Button>
                  </div>
                ))
              ) : (
                <div className="flex items-center gap-2 px-4 py-6 text-sm text-muted-foreground">
                  <UsersRound className="h-4 w-4" aria-hidden="true" />
                  No patients
                </div>
              )}
            </div>
          </div>

          <div className="mt-4 flex items-center justify-between text-sm text-muted-foreground">
            <span>{patientsQuery.data?.totalCount ?? 0} records</span>
            <div className="flex gap-2">
              <Button variant="outline" disabled={pageNumber <= 1} onClick={() => setPageNumber((page) => page - 1)}>Previous</Button>
              <Button variant="outline" disabled={!patientsQuery.data || pageNumber >= patientsQuery.data.totalPages} onClick={() => setPageNumber((page) => page + 1)}>Next</Button>
            </div>
          </div>
        </section>
      </main>
    </ProtectedRoute>
  );
}
