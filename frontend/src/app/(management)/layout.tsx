import type { ReactNode } from "react";
import { RequireRole } from "@/components/auth/RequireRole";
import { ToastProvider } from "@/components/common/Toast";
import { ManagementShell } from "@/components/layout/ManagementShell";

export default function ManagementLayout({ children }: { children: ReactNode }) {
  return (
    <ToastProvider>
      <RequireRole role="Author">
        <ManagementShell>{children}</ManagementShell>
      </RequireRole>
    </ToastProvider>
  );
}
