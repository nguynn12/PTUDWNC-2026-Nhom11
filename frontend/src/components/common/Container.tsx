import React from "react";
import { cn } from "@/lib/utils";

export interface ContainerProps extends React.HTMLAttributes<HTMLDivElement> {
  as?: "div" | "section" | "main" | "article";
  size?: "default" | "narrow" | "wide" | "full";
  children: React.ReactNode;
}

export function Container({
  as: Component = "div",
  size = "default",
  className,
  children,
  ...props
}: ContainerProps) {
  const sizeClasses = {
    narrow: "max-w-3xl",
    default: "max-w-[1240px]",
    wide: "max-w-7xl",
    full: "max-w-full",
  };

  return (
    <Component
      className={cn(
        "mx-auto w-full px-4 sm:px-6 lg:px-8",
        sizeClasses[size],
        className
      )}
      {...props}
    >
      {children}
    </Component>
  );
}
