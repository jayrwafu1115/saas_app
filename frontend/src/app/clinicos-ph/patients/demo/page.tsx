import {
  AIResultPanel,
  AppShell,
  AppointmentCard,
  AuditTimeline,
  ChartCard,
  DataTable,
  FormSection,
  PatientCard,
  StatusBadge,
} from "@/components/clinicos";

export default function ClinicOSPatientProfilePage() {
  return (
    <AppShell breadcrumb="Patients / MRN-2026-0142">
      <div className="grid gap-6 xl:grid-cols-[0.9fr_1.1fr]">
        <div className="grid gap-6">
          <PatientCard
            name="Maria Dela Cruz"
            meta="42F · +63 917 555 0188 · Barangay Bel-Air, Makati City"
            alerts={["HMO: Maxicare", "Senior/PWD: No", "Allergy: Penicillin"]}
          />

          <FormSection title="Philippines Patient Details">
            <dl className="grid gap-3 text-sm sm:grid-cols-2">
              <Info label="Medical Record Number" value="MRN-2026-0142" />
              <Info label="Outstanding Balance" value="₱2,450" />
              <Info label="Province / City" value="Metro Manila / Makati" />
              <Info label="Barangay" value="Bel-Air" />
              <Info label="PhilHealth" value="Optional · Not provided" />
              <Info label="TIN" value="Optional · 123-456-789" />
            </dl>
          </FormSection>

          <AIResultPanel title="AI Visit Summary Draft" status="Needs Review">
            Patient reports intermittent cough and throat irritation. Vitals are stable. Previous visit history shows similar allergic rhinitis episode.
          </AIResultPanel>
        </div>

        <div className="grid gap-6">
          <ChartCard title="Appointments">
            <div className="grid gap-3">
              <AppointmentCard time="09:30" patient="Maria Dela Cruz" doctor="Dra. Santos" status="In Consultation" />
              <AppointmentCard time="Jul 02" patient="Follow-up" doctor="Dra. Santos" status="Scheduled" />
            </div>
          </ChartCard>

          <ChartCard title="Encounters">
            <DataTable
              columns={["Date", "Complaint", "Doctor", "Status"]}
              rows={[
                ["Jun 04, 2026", "Cough", "Dra. Santos", <StatusBadge key="draft" status="Draft" />],
                ["May 12, 2026", "Allergic rhinitis", "Dr. Lim", <StatusBadge key="signed" status="Completed" />],
              ]}
            />
          </ChartCard>

          <ChartCard title="Billing">
            <DataTable
              columns={["Invoice", "Amount", "Coverage", "Status"]}
              rows={[
                ["OR-10482", "₱1,850", "HMO partial", <StatusBadge key="paid" status="Paid" />],
                ["INV-10491", "₱2,450", "Self-pay", <StatusBadge key="due" status="Waiting" />],
              ]}
            />
          </ChartCard>

          <ChartCard title="Audit Timeline">
            <AuditTimeline
              events={[
                { title: "Document uploaded", detail: "Lab result attached", time: "Today" },
                { title: "Profile updated", detail: "Barangay address verified", time: "Yesterday" },
                { title: "Invoice printed", detail: "VAT receipt OR-10482", time: "May 12" },
              ]}
            />
          </ChartCard>
        </div>
      </div>
    </AppShell>
  );
}

function Info({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-muted-foreground">{label}</dt>
      <dd className="mt-1 font-medium">{value}</dd>
    </div>
  );
}
