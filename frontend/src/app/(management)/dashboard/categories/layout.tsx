import type { ReactNode } from "react";
import { RequireRole } from "@/components/auth/RequireRole";

export default function CategoryManagementLayout({ children }: { children: ReactNode }) {
  return <RequireRole role="Admin">{children}</RequireRole>;
}
