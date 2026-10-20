import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { initialsOf } from "@/lib/auth/utils";
import { cn } from "@/lib/utils";

const SIZES = {
  sm: { root: "size-8", text: "text-xs" },
  md: { root: "size-10", text: "text-sm" },
  lg: { root: "size-16", text: "text-lg" },
  xl: { root: "size-24", text: "text-2xl" },
} as const;

interface UserAvatarProps {
  name: string | null | undefined;
  src?: string | null;
  size?: keyof typeof SIZES;
  className?: string;
}

/** Ảnh đại diện (shadcn Avatar); ảnh lỗi hoặc chưa có thì hiện chữ cái đầu của tên. */
export function UserAvatar({ name, src, size = "md", className }: UserAvatarProps) {
  return (
    <Avatar className={cn(SIZES[size].root, className)}>
      {src && <AvatarImage src={src} alt={name ? `Ảnh đại diện của ${name}` : ""} />}
      <AvatarFallback className={cn("bg-primary/10 font-semibold text-primary", SIZES[size].text)}>
        {initialsOf(name)}
      </AvatarFallback>
    </Avatar>
  );
}
