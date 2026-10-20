import type { ReactNode } from "react";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";

interface AuthCardProps {
  title?: ReactNode;
  subtitle?: ReactNode;
  footer?: ReactNode;
  /** Thẻ rộng hơn (trang hồ sơ, thông báo). */
  wide?: boolean;
  className?: string;
  children: ReactNode;
}

/** Khung thẻ (shadcn Card) dùng chung cho đăng nhập, đăng ký, xác nhận email. */
export function AuthCard({ title, subtitle, footer, wide, className, children }: AuthCardProps) {
  return (
    <Card className={cn("w-full max-w-md gap-5 shadow-md", wide && "max-w-2xl", className)}>
      {(title || subtitle) && (
        <CardHeader>
          {title && (
            <CardTitle>
              <h1 className="font-serif text-2xl font-bold tracking-tight">{title}</h1>
            </CardTitle>
          )}
          {subtitle && <CardDescription>{subtitle}</CardDescription>}
        </CardHeader>
      )}
      <CardContent className="flex flex-col gap-4">{children}</CardContent>
      {footer && (
        <CardFooter className="justify-center border-t pt-5 text-sm text-muted-foreground [&_a]:font-semibold [&_a]:text-primary [&_a:hover]:underline">
          <p>{footer}</p>
        </CardFooter>
      )}
    </Card>
  );
}
