import {
  AppShell,
  AppointmentCard,
  AuditTimeline,
  CalendarDays,
  ChartCard,
  CreditCard,
  DataTable,
  MiniBars,
  StatCard,
  StatusBadge,
  Stethoscope,
  UsersRound,
} from "@/components/clinicos";

const appointments = [
  ["08:30", "Maria Dela Cruz", "Dra. Santos", "Checked In"],
  ["09:00", "Juan Reyes", "Dr. Lim", "Waiting"],
  ["09:30", "Ana Garcia", "Dra. Santos", "In Consultation"],
  ["10:00", "Roberto Cruz", "Dr. Lim", "Confirmed"],
];

export default function ClinicOSDashboardPage() {
  return (
    <AppShell breadcrumb="Dashboard">
      <div className="grid gap-6">
        <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          <StatCard label="Total Patients" value="12,840" detail="+184 this month" icon={UsersRound} />
          <StatCard label="Today's Appointments" value="86" detail="14 waiting, 22 completed" icon={CalendarDays} tone="info" />
          <StatCard label="Revenue Today" value="₱128,450" detail="VAT clinic · OR ready" icon={CreditCard} tone="success" />
          <StatCard label="Doctor Utilization" value="78%" detail="Across 3 active branches" icon={Stethoscope} tone="warning" />
        </section>

        <section className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
          <ChartCard title="Appointment Trend">
            <MiniBars values={[24, 31, 28, 44, 38, 52, 48, 61, 57, 68, 63, 72]} />
          </ChartCard>
          <ChartCard title="Location Performance">
            <DataTable
              columns={["Location", "Visits", "Revenue", "Queue"]}
              rows={[
                ["BGC Branch", "124", "₱248,000", <StatusBadge key="ok" status="On Track" />],
                ["Makati", "98", "₱190,500", <StatusBadge key="wait" status="Waiting" />],
                ["Quezon City", "76", "₱142,200", <StatusBadge key="ok2" status="On Track" />],
              ]}
            />
          </ChartCard>
        </section>

        <section className="grid gap-6 xl:grid-cols-[0.9fr_1.1fr]">
          <ChartCard title="Queue / Check-in" action={<StatusBadge status="Live" />}>
            <div className="grid gap-3">
              {appointments.map(([time, patient, doctor, status]) => (
                <AppointmentCard key={`${time}-${patient}`} time={time} patient={patient} doctor={doctor} status={status} />
              ))}
            </div>
          </ChartCard>

          <ChartCard title="Recent Activity">
            <AuditTimeline
              events={[
                { title: "Patient checked in", detail: "Maria Dela Cruz assigned Q-018", time: "08:24 AM" },
                { title: "Invoice paid", detail: "GCash payment ₱1,850 · OR-10482", time: "08:19 AM" },
                { title: "SOAP note signed", detail: "Dra. Santos completed consult", time: "08:12 AM" },
                { title: "SMS reminder queued", detail: "+63 917 555 0188", time: "08:02 AM" },
              ]}
            />
          </ChartCard>
        </section>

        <ChartCard title="Upcoming Appointments">
          <DataTable
            columns={["Time", "Patient", "Doctor", "Location", "Status"]}
            rows={appointments.map(([time, patient, doctor, status]) => [
              time,
              patient,
              doctor,
              "BGC Branch",
              <StatusBadge key={status} status={status} />,
            ])}
          />
        </ChartCard>
      </div>
    </AppShell>
  );
}
