import type { ReactNode } from "react";
import { RequireRole } from "@/components/auth/RequireRole";

export default function AdminLayout({ children }: { children: ReactNode }) {
  return <RequireRole role="Admin">{children}</RequireRole>;
}
