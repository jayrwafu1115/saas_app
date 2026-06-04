import axios from "axios";

export const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080",
  timeout: 10000,
});

api.interceptors.request.use((config) => {
  if (typeof window === "undefined") {
    return config;
  }

  const token = window.localStorage.getItem("clinic-auth-access-token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

export type Tenant = {
  id: string;
  name: string;
  slug: string;
  status: string;
  settingsJson: string;
};

export type Location = {
  id: string;
  tenantId: string;
  name: string;
  code: string;
  address: string;
  phone: string;
};

export type CreateTenantPayload = {
  name: string;
  slug: string;
  status: string;
  settingsJson: string;
};

export type CreateLocationPayload = {
  tenantId: string;
  name: string;
  code: string;
  address: string;
  phone: string;
};

export type UserProfile = {
  id: string;
  tenantId?: string | null;
  email: string;
  displayName: string;
  roles: string[];
  permissions: string[];
};

export type AuthResponse = {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  user: UserProfile;
};

export type RegisterResponse = {
  userId: string;
  email: string;
  emailVerificationToken: string;
};

export type Patient = {
  id: string;
  tenantId: string;
  locationId: string;
  medicalRecordNumber: string;
  firstName: string;
  middleName: string;
  lastName: string;
  birthDate: string;
  gender: string;
  email: string;
  phone: string;
  address: string;
};

export type PatientContact = {
  id: string;
  patientId: string;
  name: string;
  relationship: string;
  email: string;
  phone: string;
  isPrimary: boolean;
};

export type PatientDocument = {
  id: string;
  patientId: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  objectKey: string;
  uploadedAtUtc: string;
};

export type PatientDetail = Patient & {
  contacts: PatientContact[];
  documents: PatientDocument[];
};

export type PatientTimelineEvent = {
  occurredAtUtc: string;
  type: string;
  title: string;
  description: string;
};

export type PagedResult<T> = {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type PatientPayload = Omit<Patient, "id">;

export type AppointmentStatus = "Scheduled" | "CheckedIn" | "CheckedOut" | "Cancelled";

export type Appointment = {
  id: string;
  tenantId: string;
  locationId: string;
  patientId: string;
  doctorUserId: string;
  startsAtUtc: string;
  endsAtUtc: string;
  reason: string;
  notes: string;
  status: AppointmentStatus;
  checkedInAtUtc?: string | null;
  checkedOutAtUtc?: string | null;
  cancelledAtUtc?: string | null;
};

export type AppointmentPayload = {
  tenantId: string;
  locationId: string;
  patientId: string;
  doctorUserId: string;
  startsAtUtc: string;
  endsAtUtc: string;
  reason: string;
  notes?: string;
};

export type EncounterStatus = "Draft" | "Signed" | "Voided";

export type Encounter = {
  id: string;
  tenantId: string;
  locationId: string;
  patientId: string;
  clinicianUserId: string;
  appointmentId?: string | null;
  encounterDateUtc: string;
  chiefComplaint: string;
  subjective: string;
  objective: string;
  assessment: string;
  plan: string;
  notes: string;
  status: EncounterStatus;
  signedAtUtc?: string | null;
};

export type Vital = {
  id: string;
  encounterId: string;
  recordedAtUtc: string;
  temperatureCelsius?: number | null;
  systolicBloodPressure?: number | null;
  diastolicBloodPressure?: number | null;
  heartRate?: number | null;
  respiratoryRate?: number | null;
  oxygenSaturation?: number | null;
  heightCm?: number | null;
  weightKg?: number | null;
  notes: string;
};

export type Diagnosis = {
  id: string;
  encounterId: string;
  code: string;
  description: string;
  type: string;
};

export type Prescription = {
  id: string;
  encounterId: string;
  medicationName: string;
  dosage: string;
  frequency: string;
  duration: string;
  instructions: string;
};

export type EncounterAuditLog = {
  id: string;
  encounterId: string;
  occurredAtUtc: string;
  action: string;
  summary: string;
  actorUserId: string;
};

export type EncounterDetail = Encounter & {
  vitals: Vital[];
  diagnoses: Diagnosis[];
  prescriptions: Prescription[];
  auditLogs: EncounterAuditLog[];
};

export type EncounterPayload = Omit<Encounter, "id" | "status" | "signedAtUtc">;

export type VitalPayload = Omit<Vital, "id" | "encounterId">;
export type DiagnosisPayload = Omit<Diagnosis, "id" | "encounterId">;
export type PrescriptionPayload = Omit<Prescription, "id" | "encounterId">;

export type EncounterTimelineEvent = {
  occurredAtUtc: string;
  type: string;
  title: string;
  description: string;
};

export type DashboardKpis = {
  totalPatients: number;
  newPatients: number;
  appointments: number;
  revenue: number;
  activeDoctors: number;
};

export type DailyVisit = { date: string; visits: number };
export type MonthlyRevenue = { year: number; month: number; revenue: number };
export type DoctorPerformance = { doctorUserId: string; appointments: number; completedVisits: number; revenue: number };
export type LocationPerformance = { locationId: string; locationName: string; appointments: number; completedVisits: number; revenue: number };

export type ReportingCharts = {
  dailyVisits: DailyVisit[];
  monthlyRevenue: MonthlyRevenue[];
  doctorPerformance: DoctorPerformance[];
  locationPerformance: LocationPerformance[];
};

export type ReportingDashboard = {
  kpis: DashboardKpis;
  charts: ReportingCharts;
};

export type SubscriptionPlan = {
  id: string;
  name: string;
  code: string;
  monthlyPricePhp: number;
  maxUsers: number;
  maxDoctors: number;
  maxLocations: number;
  maxPatients: number;
  trialDays: number;
  featuresJson: string;
};

export type TenantSubscription = {
  id: string;
  tenantId: string;
  planId: string;
  planName: string;
  status: string;
  trialEndsAtUtc: string;
  currentPeriodEndUtc: string;
  isRestricted: boolean;
};

export type SubscriptionUsage = {
  tenantId: string;
  metric: string;
  quantity: number;
  limit: number;
  period: string;
  isOverLimit: boolean;
};

export type BillingOverview = {
  subscriptions: TenantSubscription[];
  usage: SubscriptionUsage[];
};

export type BillingCheckout = {
  paymentId: string;
  provider: "GCash" | "Maya";
  amountPhp: number;
  checkoutUrl: string;
  providerReference: string;
};

export async function getTenants() {
  const response = await api.get<Tenant[]>("/api/tenants");
  return response.data;
}

export async function createTenant(payload: CreateTenantPayload) {
  const response = await api.post<Tenant>("/api/tenants", payload);
  return response.data;
}

export async function getLocations(tenantId?: string) {
  const response = await api.get<Location[]>("/api/locations", {
    headers: tenantId ? { "X-Tenant-Id": tenantId } : undefined,
  });
  return response.data;
}

export async function createLocation(payload: CreateLocationPayload) {
  const response = await api.post<Location>("/api/locations", payload);
  return response.data;
}

export async function login(payload: { email: string; password: string }) {
  const response = await api.post<AuthResponse>("/api/auth/login", payload);
  return response.data;
}

export async function register(payload: {
  email: string;
  password: string;
  displayName: string;
  tenantId?: string | null;
}) {
  const response = await api.post<RegisterResponse>("/api/auth/register", payload);
  return response.data;
}

export async function confirmEmail(payload: { userId: string; token: string }) {
  const response = await api.post("/api/auth/confirm-email", payload);
  return response.data;
}

export async function forgotPassword(payload: { email: string }) {
  const response = await api.post<{ resetToken?: string | null }>("/api/auth/forgot-password", payload);
  return response.data;
}

export async function resetPassword(payload: { email: string; token: string; newPassword: string }) {
  const response = await api.post("/api/auth/reset-password", payload);
  return response.data;
}

export async function getMe() {
  const response = await api.get<UserProfile>("/api/auth/me");
  return response.data;
}

export async function searchPatients(params: {
  search?: string;
  tenantId?: string;
  locationId?: string;
  pageNumber?: number;
  pageSize?: number;
}) {
  const response = await api.get<PagedResult<Patient>>("/api/patients", { params });
  return response.data;
}

export async function getPatient(id: string) {
  const response = await api.get<PatientDetail>(`/api/patients/${id}`);
  return response.data;
}

export async function createPatient(payload: PatientPayload) {
  const response = await api.post<Patient>("/api/patients", payload);
  return response.data;
}

export async function updatePatient(id: string, payload: Omit<PatientPayload, "tenantId">) {
  const response = await api.put<Patient>(`/api/patients/${id}`, payload);
  return response.data;
}

export async function deletePatient(id: string) {
  await api.delete(`/api/patients/${id}`);
}

export async function createPatientContact(
  patientId: string,
  payload: Omit<PatientContact, "id" | "patientId">,
) {
  const response = await api.post<PatientContact>(`/api/patients/${patientId}/contacts`, payload);
  return response.data;
}

export async function uploadPatientDocument(patientId: string, file: File) {
  const form = new FormData();
  form.append("file", file);
  const response = await api.post<PatientDocument>(`/api/patients/${patientId}/documents`, form);
  return response.data;
}

export async function getPatientTimeline(patientId: string) {
  const response = await api.get<PatientTimelineEvent[]>(`/api/patients/${patientId}/timeline`);
  return response.data;
}

export async function getAppointmentCalendar(params: {
  tenantId?: string;
  locationId?: string;
  doctorUserId?: string;
  view?: "daily" | "weekly" | "monthly";
  date?: string;
}) {
  const response = await api.get<Appointment[]>("/api/appointments/calendar", { params });
  return response.data;
}

export async function createAppointment(payload: AppointmentPayload) {
  const response = await api.post<Appointment>("/api/appointments", payload);
  return response.data;
}

export async function rescheduleAppointment(id: string, payload: Pick<AppointmentPayload, "locationId" | "doctorUserId" | "startsAtUtc" | "endsAtUtc">) {
  const response = await api.post<Appointment>(`/api/appointments/${id}/reschedule`, payload);
  return response.data;
}

export async function cancelAppointment(id: string) {
  const response = await api.post<Appointment>(`/api/appointments/${id}/cancel`);
  return response.data;
}

export async function checkInAppointment(id: string) {
  const response = await api.post<Appointment>(`/api/appointments/${id}/check-in`);
  return response.data;
}

export async function checkOutAppointment(id: string) {
  const response = await api.post<Appointment>(`/api/appointments/${id}/check-out`);
  return response.data;
}

export async function createEncounter(payload: EncounterPayload) {
  const response = await api.post<Encounter>("/api/encounters", payload);
  return response.data;
}

export async function getEncounter(id: string) {
  const response = await api.get<EncounterDetail>(`/api/encounters/${id}`);
  return response.data;
}

export async function updateEncounterSoap(id: string, payload: Omit<EncounterPayload, "tenantId" | "patientId" | "appointmentId">) {
  const response = await api.put<Encounter>(`/api/encounters/${id}/soap`, payload);
  return response.data;
}

export async function addEncounterVital(encounterId: string, payload: VitalPayload) {
  const response = await api.post<Vital>(`/api/encounters/${encounterId}/vitals`, payload);
  return response.data;
}

export async function addEncounterDiagnosis(encounterId: string, payload: DiagnosisPayload) {
  const response = await api.post<Diagnosis>(`/api/encounters/${encounterId}/diagnoses`, payload);
  return response.data;
}

export async function addEncounterPrescription(encounterId: string, payload: PrescriptionPayload) {
  const response = await api.post<Prescription>(`/api/encounters/${encounterId}/prescriptions`, payload);
  return response.data;
}

export async function signEncounter(id: string) {
  const response = await api.post<Encounter>(`/api/encounters/${id}/sign`);
  return response.data;
}

export async function getPatientEncounterTimeline(patientId: string) {
  const response = await api.get<EncounterTimelineEvent[]>(`/api/encounters/patients/${patientId}/timeline`);
  return response.data;
}

export function getEncounterPrintUrl(id: string) {
  return `${api.defaults.baseURL}/api/encounters/${id}/print`;
}

export function getEncounterPdfUrl(id: string) {
  return `${api.defaults.baseURL}/api/encounters/${id}/pdf`;
}

export async function getEncounterPrintHtml(id: string) {
  const response = await api.get<string>(`/api/encounters/${id}/print`, { responseType: "text" });
  return response.data;
}

export async function getEncounterPdf(id: string) {
  const response = await api.get<Blob>(`/api/encounters/${id}/pdf`, { responseType: "blob" });
  return response.data;
}

export async function getReportingDashboard(params: { tenantId?: string; from?: string; to?: string }) {
  const response = await api.get<ReportingDashboard>("/api/reports/dashboard", { params });
  return response.data;
}

export async function getReportingCharts(params: { tenantId?: string; from?: string; to?: string }) {
  const response = await api.get<ReportingCharts>("/api/reports/charts", { params });
  return response.data;
}

export async function getReportExcel(params: { tenantId?: string; from?: string; to?: string }) {
  const response = await api.get<Blob>("/api/reports/export/excel", { params, responseType: "blob" });
  return response.data;
}

export async function getReportPdf(params: { tenantId?: string; from?: string; to?: string }) {
  const response = await api.get<Blob>("/api/reports/export/pdf", { params, responseType: "blob" });
  return response.data;
}

export async function getSubscriptionPlans() {
  const response = await api.get<SubscriptionPlan[]>("/api/billing/plans");
  return response.data;
}

export async function startSubscriptionTrial(payload: { tenantId: string; planCode: string }) {
  const response = await api.post<TenantSubscription>("/api/billing/trial", payload);
  return response.data;
}

export async function createBillingCheckout(payload: { tenantId: string; planCode: string; provider: "GCash" | "Maya" }) {
  const response = await api.post<BillingCheckout>("/api/billing/checkout", payload);
  return response.data;
}

export async function getBillingOverview(tenantId?: string) {
  const response = await api.get<BillingOverview>("/api/billing/overview", { params: { tenantId } });
  return response.data;
}
