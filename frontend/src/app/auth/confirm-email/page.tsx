import type { Metadata } from "next";
import { Suspense } from "react";
import { AuthShell } from "@/components/auth/AuthShell";
import { ConfirmEmail } from "@/components/auth/ConfirmEmail";

export const metadata: Metadata = { title: "Xác nhận email" };

export default function ConfirmEmailPage() {
  return (
    <AuthShell showIntro={false}>
      <Suspense>
        <ConfirmEmail />
      </Suspense>
    </AuthShell>
  );
}
