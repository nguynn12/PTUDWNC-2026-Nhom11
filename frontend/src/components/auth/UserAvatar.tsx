import { initialsOf } from "@/lib/auth/utils";
import styles from "@/styles/auth.module.css";

interface UserAvatarProps {
  name: string | null | undefined;
  src?: string | null;
  size?: "sm" | "md" | "lg" | "xl";
}

/** Ảnh đại diện; chưa có ảnh thì hiện chữ cái đầu của tên. */
export function UserAvatar({ name, src, size = "md" }: UserAvatarProps) {
  const className = `${styles.avatar} ${styles[`avatar_${size}`]}`;
  if (src) {
    // Ảnh từ nhiều nguồn (Google, URL người dùng nhập) nên không dùng next/image.
    // eslint-disable-next-line @next/next/no-img-element
    return <img className={className} src={src} alt="" />;
  }
  return (
    <span className={className} aria-hidden="true">
      {initialsOf(name)}
    </span>
  );
}
