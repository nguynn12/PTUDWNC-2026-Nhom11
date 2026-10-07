import { create } from "zustand";

export type AuthToastKind = "success" | "error";

/** Trạng thái giao diện của phần Auth (menu tài khoản, thông báo nhanh). Tách khỏi useUIStore của nhóm. */
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
