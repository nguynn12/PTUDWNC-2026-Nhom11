import type { Metadata } from "next";
import { Suspense } from "react";
import { ConfirmEmail } from "@/components/auth/ConfirmEmail";

export const metadata: Metadata = { title: "Xác nhận email | Culinary Blog" };

export default function ConfirmEmailPage() {
  return (
    <Suspense>
      <ConfirmEmail />
    </Suspense>
  );
}
