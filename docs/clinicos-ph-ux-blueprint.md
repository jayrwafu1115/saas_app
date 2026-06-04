# ClinicOS PH UX Blueprint

## 1. UX Design Strategy

ClinicOS PH is a premium multi-tenant clinic operations platform for Philippine clinics, diagnostic centers, dental practices, aesthetic clinics, specialty practices, and multi-branch healthcare groups. The product should feel calm, fast, trustworthy, and operational rather than decorative.

Core design principles:
- Put reception actions within 1-2 clicks: search patient, check in, queue, invoice, SMS reminder.
- Minimize doctor typing: reusable SOAP sections, prior-visit context, vitals summaries, AI drafts, prescription shortcuts.
- Make patient data scannable: MRN, age, gender, mobile, address, HMO, allergies, alerts, balance.
- Keep multi-tenant context visible: tenant switcher, location switcher, role-aware navigation.
- Keep AI separated from records until reviewed and saved by authorized staff.
- Make every table searchable, filterable, sortable, paginated, and export-ready.

## 2. Information Architecture

Primary navigation:
- Dashboard
- Appointments
- Queue / Check-in
- Patients
- Encounters
- Doctors
- Billing
- Documents
- Reports
- AI Reports
- Locations
- Users & Roles
- Settings

System-level navigation:
- Tenant switcher
- Location switcher
- Global search
- Notifications
- User profile
- Breadcrumb trail
- Mobile drawer

Settings sections:
- Clinic profile
- Locations
- Users
- Roles
- Permissions
- Billing settings
- Tax settings
- Queue settings
- AI settings
- Notification templates
- Audit logs

## 3. User Flows

Reception check-in:
1. Search patient by name, mobile, MRN, or appointment.
2. Select appointment or create walk-in.
3. Confirm patient details and HMO/payment flags.
4. Assign queue number.
5. Print/display queue slip and optionally send SMS.

Doctor consultation:
1. Open today queue.
2. Select patient.
3. Review summary, vitals, alerts, previous visits, documents.
4. Draft SOAP notes manually or with AI assistant.
5. Add diagnosis and prescription.
6. Review AI output before saving.
7. Complete consultation.

Billing:
1. Create invoice from appointment/encounter.
2. Apply HMO, senior citizen/PWD, VAT/non-VAT rules.
3. Record partial/full payment.
4. Generate OR/invoice number.
5. Print receipt.

Multi-location admin:
1. Select tenant or all locations.
2. Review dashboard KPIs.
3. Compare location performance.
4. Export reports.
5. Audit role and permission changes.

## 4. Page-by-Page Design Plan

Dashboard:
- KPI strip: total patients, today's appointments, waiting queue, completed consultations, revenue today, new patients this month.
- Charts: appointment trend, revenue, doctor utilization, location performance.
- Operational panels: recent activity, upcoming appointments, live queue.

Patient list:
- Data table with search, filters, sort, pagination, export.
- Filters: location, HMO, status, age group, balance.
- Row actions: view, book appointment, check in, create invoice.

Patient profile:
- Header: name, MRN, age, gender, contact, address, alerts.
- Tabs: timeline, documents, appointments, billing, encounters.
- Philippine fields: +63 mobile, province/city/barangay, HMO, PhilHealth, TIN, senior/PWD.
- Audit timeline for profile and medical record changes.

Appointments:
- Calendar, daily, and weekly views.
- Booking modal with doctor/location filters.
- Status badges: Scheduled, Confirmed, Checked In, Waiting, In Consultation, Completed, Cancelled, No Show.
- Reschedule/cancel confirmations.

Doctor workspace:
- Queue list, patient summary, vitals, SOAP notes, diagnosis, prescription, AI assistant, history, documents.

AI reports:
- Generate SOAP note, visit summary, patient-friendly summary, monthly report, revenue analysis, trend analysis, no-show insights.
- Editable output, source data, confidence/status, clinical disclaimer.

Billing:
- Invoice list, create invoice, payment recording, discounts, HMO coverage, partial payments, outstanding balance, printable invoice/receipt.

Reports:
- Owner, admin, doctor, and receptionist views.
- Revenue by location, appointment status, new/returning patients, doctor performance, no-show rate, demographics, HMO utilization.

## 5. Component List

Implemented sample component module:
- `AppShell`
- `Sidebar`
- `Topbar`
- `TenantSwitcher`
- `LocationSwitcher`
- `DataTable`
- `StatCard`
- `ChartCard`
- `StatusBadge`
- `PatientCard`
- `AppointmentCard`
- `EmptyState`
- `ConfirmDialog`
- `FormSection`
- `SearchCommand`
- `NotificationMenu`
- `AIResultPanel`
- `AuditTimeline`

Location:
- `frontend/src/components/clinicos/index.tsx`

## 6. Design Tokens

Color tokens:
- Background: `#f7faf9`
- Foreground: `#172033`
- Surface: `#ffffff`
- Muted: `#edf4f2`
- Border: `#d8e4e1`
- Primary: `#0f3f4a`
- Accent: `#0f766e`
- Success: `#16a34a`
- Warning: `#d97706`
- Danger: `#dc2626`
- Info: `#0284c7`

Shape and elevation:
- Cards: `rounded-md`
- Inputs/buttons: `rounded-md`
- Shadow: `0 12px 32px rgba(15, 63, 74, 0.08)`

Typography:
- Sans: Geist
- Headings: semibold, tight tracking
- Tables: compact 14px rows, uppercase 12px headers

## 7. Tailwind Theme Configuration

This project uses Tailwind CSS v4 token mapping in `frontend/src/app/globals.css`:

```css
:root {
  --background: #f7faf9;
  --foreground: #172033;
  --surface: #ffffff;
  --muted: #edf4f2;
  --border: #d8e4e1;
  --accent: #0f766e;
  --primary: #0f3f4a;
  --success: #16a34a;
  --warning: #d97706;
  --danger: #dc2626;
  --info: #0284c7;
}

@theme inline {
  --color-background: var(--background);
  --color-foreground: var(--foreground);
  --color-surface: var(--surface);
  --color-accent: var(--accent);
  --color-primary: var(--primary);
  --color-success: var(--success);
  --color-warning: var(--warning);
  --color-danger: var(--danger);
  --color-info: var(--info);
}
```

Dark mode is available through the `.dark` class token overrides.

## 8. Sample Next.js Page Structure

Recommended app structure:

```text
frontend/src/app/
  clinicos-ph/
    page.tsx
    patients/demo/page.tsx
  appointments/
  patients/
  encounters/
  billing/
  reports/

frontend/src/components/
  clinicos/index.tsx
  ui/button.tsx

frontend/src/lib/
  api.ts
  query-provider.tsx
```

## 9. Sample Dashboard UI Code

Implemented route:
- `frontend/src/app/clinicos-ph/page.tsx`

Highlights:
- SaaS shell with sidebar/topbar
- Tenant and location switchers
- KPI cards
- Trend and location performance panels
- Queue/check-in panel
- Recent activity
- Upcoming appointments table
- Philippine Peso revenue formatting

## 10. Sample Patient Profile UI Code

Implemented route:
- `frontend/src/app/clinicos-ph/patients/demo/page.tsx`

Highlights:
- Patient summary with +63 mobile and barangay/city address
- HMO, senior/PWD, allergy alerts
- PhilHealth and TIN fields
- Appointments, encounters, billing, and audit timeline sections
- AI result panel with clinical review disclaimer
