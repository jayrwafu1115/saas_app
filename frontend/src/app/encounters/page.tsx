"use client";

import Link from "next/link";
import { FormEvent, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, ClipboardList, Download, FileText, HeartPulse, Loader2, Plus, Printer, Search, Stethoscope } from "lucide-react";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { Button } from "@/components/ui/button";
import {
  addEncounterDiagnosis,
  addEncounterPrescription,
  addEncounterVital,
  createEncounter,
  getEncounter,
  getEncounterPdf,
  getEncounterPrintHtml,
  getPatientEncounterTimeline,
  searchPatients,
  signEncounter,
  type Patient,
} from "@/lib/api";
import { useAuthStore } from "@/store/auth-store";

export default function EncountersPage() {
  const queryClient = useQueryClient();
  const user = useAuthStore((state) => state.user);
  const [search, setSearch] = useState("");
  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null);
  const [selectedEncounterId, setSelectedEncounterId] = useState<string | null>(null);
  const [chiefComplaint, setChiefComplaint] = useState("");
  const [subjective, setSubjective] = useState("");
  const [objective, setObjective] = useState("");
  const [assessment, setAssessment] = useState("");
  const [plan, setPlan] = useState("");
  const [notes, setNotes] = useState("");
  const [temperature, setTemperature] = useState("");
  const [bloodPressure, setBloodPressure] = useState("");
  const [heartRate, setHeartRate] = useState("");
  const [oxygen, setOxygen] = useState("");
  const [diagnosisCode, setDiagnosisCode] = useState("");
  const [diagnosisDescription, setDiagnosisDescription] = useState("");
  const [prescriptionMedication, setPrescriptionMedication] = useState("");
  const [prescriptionDosage, setPrescriptionDosage] = useState("");
  const [prescriptionFrequency, setPrescriptionFrequency] = useState("");
  const [prescriptionDuration, setPrescriptionDuration] = useState("");

  const patientsQuery = useQuery({
    queryKey: ["encounter-patients", search],
    queryFn: () => searchPatients({ search, pageSize: 8 }),
  });
  const encounterQuery = useQuery({
    queryKey: ["encounter", selectedEncounterId],
    queryFn: () => getEncounter(selectedEncounterId!),
    enabled: Boolean(selectedEncounterId),
  });
  const timelineQuery = useQuery({
    queryKey: ["encounter-timeline", selectedPatient?.id],
    queryFn: () => getPatientEncounterTimeline(selectedPatient!.id),
    enabled: Boolean(selectedPatient),
  });

  const encounter = encounterQuery.data;
  const canCreate = selectedPatient && chiefComplaint.trim() && user?.id;
  const todayIso = useMemo(() => new Date().toISOString(), []);

  const createMutation = useMutation({
    mutationFn: () => createEncounter({
      tenantId: selectedPatient!.tenantId,
      locationId: selectedPatient!.locationId,
      patientId: selectedPatient!.id,
      clinicianUserId: user!.id,
      appointmentId: null,
      encounterDateUtc: todayIso,
      chiefComplaint,
      subjective,
      objective,
      assessment,
      plan,
      notes,
    }),
    onSuccess: (created) => {
      setSelectedEncounterId(created.id);
      queryClient.invalidateQueries({ queryKey: ["encounter-timeline", selectedPatient?.id] });
    },
  });

  const vitalMutation = useMutation({
    mutationFn: () => {
      const [systolic, diastolic] = bloodPressure.split("/").map((value) => Number(value.trim()));
      return addEncounterVital(selectedEncounterId!, {
        recordedAtUtc: new Date().toISOString(),
        temperatureCelsius: numberOrNull(temperature),
        systolicBloodPressure: Number.isFinite(systolic) ? systolic : null,
        diastolicBloodPressure: Number.isFinite(diastolic) ? diastolic : null,
        heartRate: integerOrNull(heartRate),
        respiratoryRate: null,
        oxygenSaturation: integerOrNull(oxygen),
        heightCm: null,
        weightKg: null,
        notes: "",
      });
    },
    onSuccess: () => {
      setTemperature("");
      setBloodPressure("");
      setHeartRate("");
      setOxygen("");
      refreshEncounter();
    },
  });

  const diagnosisMutation = useMutation({
    mutationFn: () => addEncounterDiagnosis(selectedEncounterId!, {
      code: diagnosisCode,
      description: diagnosisDescription,
      type: "Primary",
    }),
    onSuccess: () => {
      setDiagnosisCode("");
      setDiagnosisDescription("");
      refreshEncounter();
    },
  });

  const prescriptionMutation = useMutation({
    mutationFn: () => addEncounterPrescription(selectedEncounterId!, {
      medicationName: prescriptionMedication,
      dosage: prescriptionDosage,
      frequency: prescriptionFrequency,
      duration: prescriptionDuration,
      instructions: "",
    }),
    onSuccess: () => {
      setPrescriptionMedication("");
      setPrescriptionDosage("");
      setPrescriptionFrequency("");
      setPrescriptionDuration("");
      refreshEncounter();
    },
  });

  const signMutation = useMutation({
    mutationFn: () => signEncounter(selectedEncounterId!),
    onSuccess: refreshEncounter,
  });

  function refreshEncounter() {
    queryClient.invalidateQueries({ queryKey: ["encounter", selectedEncounterId] });
    queryClient.invalidateQueries({ queryKey: ["encounter-timeline", selectedPatient?.id] });
  }

  function selectPatient(patient: Patient) {
    setSelectedPatient(patient);
    setSelectedEncounterId(null);
    setChiefComplaint("");
    setSubjective("");
    setObjective("");
    setAssessment("");
    setPlan("");
    setNotes("");
  }

  function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (canCreate) {
      createMutation.mutate();
    }
  }

  async function openPrintView() {
    if (!selectedEncounterId) return;
    const html = await getEncounterPrintHtml(selectedEncounterId);
    const url = URL.createObjectURL(new Blob([html], { type: "text/html" }));
    window.open(url, "_blank", "noopener,noreferrer");
  }

  async function downloadPdf() {
    if (!selectedEncounterId) return;
    const blob = await getEncounterPdf(selectedEncounterId);
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = `encounter-${selectedEncounterId}.pdf`;
    anchor.click();
    URL.revokeObjectURL(url);
  }

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
                <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Clinical</p>
                <h1 className="text-xl font-semibold">Encounters</h1>
              </div>
            </div>
            <div className="flex gap-2">
              <Button variant="outline" size="icon" onClick={openPrintView} disabled={!selectedEncounterId} aria-label="Print">
                <Printer className="h-4 w-4" aria-hidden="true" />
              </Button>
              <Button variant="outline" size="icon" onClick={downloadPdf} disabled={!selectedEncounterId} aria-label="Download PDF">
                <Download className="h-4 w-4" aria-hidden="true" />
              </Button>
              <Button onClick={() => signMutation.mutate()} disabled={!selectedEncounterId || encounter?.status !== "Draft"}>
                <FileText className="h-4 w-4" aria-hidden="true" />
                Sign
              </Button>
            </div>
          </div>
        </header>

        <section className="mx-auto grid max-w-6xl gap-6 px-6 py-8 lg:grid-cols-[0.7fr_1.3fr]">
          <aside className="space-y-6">
            <section className="rounded-md border border-border bg-surface p-4">
              <div className="mb-3 flex items-center gap-2">
                <Search className="h-4 w-4 text-accent" aria-hidden="true" />
                <h2 className="text-sm font-semibold">Patient</h2>
              </div>
              <input className="h-10 w-full rounded-md border border-border bg-background px-3 text-sm" placeholder="Search patients" value={search} onChange={(event) => setSearch(event.target.value)} />
              <div className="mt-3 divide-y divide-border">
                {patientsQuery.data?.items.map((patient) => (
                  <button key={patient.id} className="block w-full px-2 py-3 text-left text-sm hover:bg-muted" type="button" onClick={() => selectPatient(patient)}>
                    <span className="block font-medium">{patient.firstName} {patient.lastName}</span>
                    <span className="text-muted-foreground">{patient.medicalRecordNumber}</span>
                  </button>
                ))}
              </div>
            </section>

            <section className="rounded-md border border-border bg-surface p-4">
              <div className="mb-3 flex items-center gap-2">
                <ClipboardList className="h-4 w-4 text-accent" aria-hidden="true" />
                <h2 className="text-sm font-semibold">Timeline</h2>
              </div>
              <div className="grid gap-2">
                {timelineQuery.data?.length ? timelineQuery.data.map((event) => (
                  <div key={`${event.type}-${event.occurredAtUtc}`} className="rounded-md bg-muted px-3 py-2 text-sm">
                    <p className="font-medium">{event.title}</p>
                    <p className="text-muted-foreground">{event.description}</p>
                  </div>
                )) : <p className="text-sm text-muted-foreground">No encounter events</p>}
              </div>
            </section>
          </aside>

          <div className="space-y-6">
            <form onSubmit={handleCreate} className="rounded-md border border-border bg-surface p-4">
              <div className="mb-4 flex items-center gap-2">
                <Stethoscope className="h-4 w-4 text-accent" aria-hidden="true" />
                <h2 className="text-sm font-semibold">{selectedPatient ? `${selectedPatient.firstName} ${selectedPatient.lastName}` : "SOAP Notes"}</h2>
              </div>
              <div className="grid gap-3">
                <input className="h-10 rounded-md border border-border bg-background px-3 text-sm" placeholder="Chief complaint" value={chiefComplaint} onChange={(event) => setChiefComplaint(event.target.value)} required />
                <textarea className="min-h-20 rounded-md border border-border bg-background px-3 py-2 text-sm" placeholder="Subjective" value={subjective} onChange={(event) => setSubjective(event.target.value)} />
                <textarea className="min-h-20 rounded-md border border-border bg-background px-3 py-2 text-sm" placeholder="Objective" value={objective} onChange={(event) => setObjective(event.target.value)} />
                <textarea className="min-h-20 rounded-md border border-border bg-background px-3 py-2 text-sm" placeholder="Assessment" value={assessment} onChange={(event) => setAssessment(event.target.value)} />
                <textarea className="min-h-20 rounded-md border border-border bg-background px-3 py-2 text-sm" placeholder="Plan" value={plan} onChange={(event) => setPlan(event.target.value)} />
                <textarea className="min-h-16 rounded-md border border-border bg-background px-3 py-2 text-sm" placeholder="Notes" value={notes} onChange={(event) => setNotes(event.target.value)} />
                <Button type="submit" disabled={!canCreate || createMutation.isPending}>
                  {createMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" /> : <Plus className="h-4 w-4" aria-hidden="true" />}
                  Create Encounter
                </Button>
              </div>
            </form>

            {encounter ? (
              <div className="grid gap-6 xl:grid-cols-3">
                <section className="rounded-md border border-border bg-surface p-4">
                  <div className="mb-3 flex items-center gap-2">
                    <HeartPulse className="h-4 w-4 text-accent" aria-hidden="true" />
                    <h2 className="text-sm font-semibold">Vitals</h2>
                  </div>
                  <div className="grid gap-2">
                    <input className="h-9 rounded-md border border-border bg-background px-3 text-sm" placeholder="Temp C" value={temperature} onChange={(event) => setTemperature(event.target.value)} />
                    <input className="h-9 rounded-md border border-border bg-background px-3 text-sm" placeholder="BP 120/80" value={bloodPressure} onChange={(event) => setBloodPressure(event.target.value)} />
                    <input className="h-9 rounded-md border border-border bg-background px-3 text-sm" placeholder="HR" value={heartRate} onChange={(event) => setHeartRate(event.target.value)} />
                    <input className="h-9 rounded-md border border-border bg-background px-3 text-sm" placeholder="SpO2" value={oxygen} onChange={(event) => setOxygen(event.target.value)} />
                    <Button type="button" onClick={() => vitalMutation.mutate()} disabled={!selectedEncounterId}>Add</Button>
                  </div>
                  <List items={encounter.vitals.map((vital) => `${vital.temperatureCelsius ?? "-"} C | ${vital.systolicBloodPressure ?? "-"}/${vital.diastolicBloodPressure ?? "-"} | HR ${vital.heartRate ?? "-"}`)} />
                </section>

                <section className="rounded-md border border-border bg-surface p-4">
                  <h2 className="mb-3 text-sm font-semibold">Diagnoses</h2>
                  <div className="grid gap-2">
                    <input className="h-9 rounded-md border border-border bg-background px-3 text-sm" placeholder="Code" value={diagnosisCode} onChange={(event) => setDiagnosisCode(event.target.value)} />
                    <input className="h-9 rounded-md border border-border bg-background px-3 text-sm" placeholder="Description" value={diagnosisDescription} onChange={(event) => setDiagnosisDescription(event.target.value)} />
                    <Button type="button" onClick={() => diagnosisMutation.mutate()} disabled={!diagnosisCode || !diagnosisDescription}>Add</Button>
                  </div>
                  <List items={encounter.diagnoses.map((diagnosis) => `${diagnosis.code} - ${diagnosis.description}`)} />
                </section>

                <section className="rounded-md border border-border bg-surface p-4">
                  <h2 className="mb-3 text-sm font-semibold">Prescriptions</h2>
                  <div className="grid gap-2">
                    <input className="h-9 rounded-md border border-border bg-background px-3 text-sm" placeholder="Medication" value={prescriptionMedication} onChange={(event) => setPrescriptionMedication(event.target.value)} />
                    <input className="h-9 rounded-md border border-border bg-background px-3 text-sm" placeholder="Dosage" value={prescriptionDosage} onChange={(event) => setPrescriptionDosage(event.target.value)} />
                    <input className="h-9 rounded-md border border-border bg-background px-3 text-sm" placeholder="Frequency" value={prescriptionFrequency} onChange={(event) => setPrescriptionFrequency(event.target.value)} />
                    <input className="h-9 rounded-md border border-border bg-background px-3 text-sm" placeholder="Duration" value={prescriptionDuration} onChange={(event) => setPrescriptionDuration(event.target.value)} />
                    <Button type="button" onClick={() => prescriptionMutation.mutate()} disabled={!prescriptionMedication || !prescriptionDosage || !prescriptionFrequency || !prescriptionDuration}>Add</Button>
                  </div>
                  <List items={encounter.prescriptions.map((prescription) => `${prescription.medicationName} ${prescription.dosage}`)} />
                </section>
              </div>
            ) : null}
          </div>
        </section>
      </main>
    </ProtectedRoute>
  );
}

function List({ items }: { items: string[] }) {
  return (
    <div className="mt-4 divide-y divide-border">
      {items.length ? items.map((item) => <p key={item} className="py-2 text-sm text-muted-foreground">{item}</p>) : <p className="py-2 text-sm text-muted-foreground">None</p>}
    </div>
  );
}

function numberOrNull(value: string) {
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

function integerOrNull(value: string) {
  const parsed = Number.parseInt(value, 10);
  return Number.isFinite(parsed) ? parsed : null;
}
