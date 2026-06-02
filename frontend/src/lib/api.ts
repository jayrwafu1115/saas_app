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
