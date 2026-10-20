import * as React from "react";
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/lib/utils";

const badgeVariants = cva(
  "inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2",
  {
    variants: {
      variant: {
        default:
          "border-transparent bg-[var(--primary)] text-white hover:bg-[var(--primary-hover)]",
        secondary:
          "border-transparent bg-[var(--secondary-light)] text-[var(--secondary)] hover:bg-[var(--secondary-subtle)]",
        destructive:
          "border-transparent bg-[var(--danger-light)] text-[var(--danger)] hover:bg-[var(--danger-hover)] hover:text-white",
        outline: "text-[var(--text-primary)] border-[var(--border-subtle)]",
        easy: "border-transparent bg-[#ecf7f0] text-[#2b7a4b] font-medium",
        medium: "border-transparent bg-[#fef3c7] text-[#d97706] font-medium",
        hard: "border-transparent bg-[#fef2f2] text-[#dc2626] font-medium",
        category: "border-transparent bg-white/90 text-[var(--text-primary)] shadow-sm backdrop-blur-sm",
        subtle: "border-transparent bg-[var(--primary-light)] text-[var(--primary)] font-medium",
        statusDraft: "border-transparent bg-amber-50 text-amber-700 font-medium",
        statusPublished: "border-transparent bg-emerald-50 text-emerald-700 font-medium",
        statusArchived: "border-transparent bg-stone-100 text-stone-600 font-medium",
      },
    },
    defaultVariants: {
      variant: "default",
    },
  }
);

export interface BadgeProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> {}

function Badge({ className, variant, ...props }: BadgeProps) {
  return (
    <div className={cn(badgeVariants({ variant }), className)} {...props} />
  );
}

export { Badge, badgeVariants };
