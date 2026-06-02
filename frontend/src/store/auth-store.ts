import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { AuthResponse, UserProfile } from "@/lib/api";

type AuthState = {
  accessToken?: string;
  refreshToken?: string;
  user?: UserProfile;
  hasHydrated: boolean;
  setHasHydrated: (hasHydrated: boolean) => void;
  setAuth: (auth: AuthResponse) => void;
  clearAuth: () => void;
};

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: undefined,
      refreshToken: undefined,
      user: undefined,
      hasHydrated: false,
      setHasHydrated: (hasHydrated) => set({ hasHydrated }),
      setAuth: (auth) => {
        if (typeof window !== "undefined") {
          window.localStorage.setItem("clinic-auth-access-token", auth.accessToken);
        }

        set({
          accessToken: auth.accessToken,
          refreshToken: auth.refreshToken,
          user: auth.user,
        });
      },
      clearAuth: () => {
        if (typeof window !== "undefined") {
          window.localStorage.removeItem("clinic-auth-access-token");
        }

        set({ accessToken: undefined, refreshToken: undefined, user: undefined });
      },
    }),
    {
      name: "clinic-auth",
      partialize: (state) => ({
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        user: state.user,
      }),
      onRehydrateStorage: () => (state) => {
        if (typeof window !== "undefined" && state?.accessToken) {
          window.localStorage.setItem("clinic-auth-access-token", state.accessToken);
        }

        state?.setHasHydrated(true);
      },
    },
  ),
);
