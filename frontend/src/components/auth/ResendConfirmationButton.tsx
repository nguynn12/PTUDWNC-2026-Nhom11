"use client";

import { Loader2, Mail } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useResendConfirmation } from "@/hooks/useProfileMutations";
import { cn } from "@/lib/utils";
import { outlineButtonClass } from "./profile-styles";

/** Nút "Gửi lại email xác nhận" (FR-AUTH-009). */
export function ResendConfirmationButton({ email, className }: { email: string; className?: string }) {
  const resend = useResendConfirmation();
  return (
    <Button
      type="button"
      className={cn(outlineButtonClass, "h-10", className)}
      disabled={resend.isPending}
      aria-busy={resend.isPending || undefined}
      onClick={() => resend.mutate(email)}
    >
      {resend.isPending ? <Loader2 className="animate-spin" aria-hidden="true" /> : <Mail aria-hidden="true" />}
      {resend.isPending ? "Đang gửi..." : "Gửi lại email xác nhận"}
    </Button>
  );
}
