/** auth-ui-store.ts — trạng thái giao diện của module Xác thực (menu tài khoản, thông báo nhanh). */
import { create } from "zustand";

export type AuthToastKind = "success" | "error";

interface AuthUIState {
  userMenuOpen: boolean;
  toast: { kind: AuthToastKind; message: string } | null;
  setUserMenuOpen: (open: boolean) => void;
  showToast: (kind: AuthToastKind, message: string) => void;
  clearToast: () => void;
}

export const useAuthUIStore = create<AuthUIState>((set) => ({
  userMenuOpen: false,
  toast: null,
  setUserMenuOpen: (open) => set({ userMenuOpen: open }),
  showToast: (kind, message) => set({ toast: { kind, message } }),
  clearToast: () => set({ toast: null }),
}));
