import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";

export type RecipesViewMode = "grid" | "list";

interface UIState {
  // Mobile navigation
  isMobileMenuOpen: boolean;
  toggleMobileMenu: () => void;
  setMobileMenuOpen: (open: boolean) => void;

  // Filter Drawer on mobile/tablet
  isFilterDrawerOpen: boolean;
  toggleFilterDrawer: () => void;
  setFilterDrawerOpen: (open: boolean) => void;

  // Recipe display mode (persisted to localStorage)
  recipesViewMode: RecipesViewMode;
  setRecipesViewMode: (mode: RecipesViewMode) => void;

  // Modal manager
  activeModal: string | null;
  modalData: unknown;
  openModal: (modalId: string, data?: unknown) => void;
  closeModal: () => void;
}

export const useUIStore = create<UIState>()(
  persist(
    (set) => ({
      isMobileMenuOpen: false,
      toggleMobileMenu: () =>
        set((state) => ({ isMobileMenuOpen: !state.isMobileMenuOpen })),
      setMobileMenuOpen: (open) => set({ isMobileMenuOpen: open }),

      isFilterDrawerOpen: false,
      toggleFilterDrawer: () =>
        set((state) => ({ isFilterDrawerOpen: !state.isFilterDrawerOpen })),
      setFilterDrawerOpen: (open) => set({ isFilterDrawerOpen: open }),

      recipesViewMode: "grid",
      setRecipesViewMode: (mode) => set({ recipesViewMode: mode }),

      activeModal: null,
      modalData: null,
      openModal: (modalId, data = null) =>
        set({ activeModal: modalId, modalData: data }),
      closeModal: () => set({ activeModal: null, modalData: null }),
    }),
    {
      name: "culinary-blog-ui-store",
      storage: createJSONStorage(() =>
        typeof window !== "undefined" ? localStorage : {
          getItem: () => null,
          setItem: () => {},
          removeItem: () => {},
        }
      ),
      // Chỉ lưu trữ chế độ xem vào localStorage, không persist các cờ modal/menu
      partialize: (state) => ({
        recipesViewMode: state.recipesViewMode,
      }),
    }
  )
);
