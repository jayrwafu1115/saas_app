import { create } from "zustand";

type AppState = {
  tenantId?: string;
  setTenantId: (tenantId?: string) => void;
};

export const useAppStore = create<AppState>((set) => ({
  tenantId: undefined,
  setTenantId: (tenantId) => set({ tenantId }),
}));
