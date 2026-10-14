import type { ReactNode } from "react";
import styles from "@/styles/auth.module.css";

const HIGHLIGHTS = [
  "Lưu và chia sẻ công thức của riêng bạn",
  "Xem thời gian nấu và dinh dưỡng từng món",
  "Khám phá món mới theo danh mục, nguyên liệu",
];

interface AuthShellProps {
  /** Hiện khối giới thiệu bên trái trên màn hình rộng (đăng nhập, đăng ký). */
  showIntro?: boolean;
  children: ReactNode;
}

/** Bố cục trang của module Xác thực: căn giữa thẻ form, kèm khối giới thiệu trên desktop. */
export function AuthShell({ showIntro = true, children }: AuthShellProps) {
  if (!showIntro) return <section className={styles.page}>{children}</section>;
  return (
    <section className={styles.shell}>
      <aside className={styles.intro} aria-hidden="true">
        <p className={styles.introEyebrow}>Culinary Blog</p>
        <h2 className={styles.introTitle}>Nấu ngon hơn mỗi ngày</h2>
        <p className={styles.introText}>
          Tham gia cộng đồng yêu bếp: ghi lại công thức, học hỏi món mới và chia sẻ hương vị của bạn.
        </p>
        <ul className={styles.introList}>
          {HIGHLIGHTS.map((item) => (
            <li key={item}>
              <svg viewBox="0 0 20 20" width="18" height="18" fill="currentColor" aria-hidden="true">
                <path d="M16.7 5.3a1 1 0 0 1 0 1.4l-8 8a1 1 0 0 1-1.4 0l-4-4a1 1 0 1 1 1.4-1.4L8 12.6l7.3-7.3a1 1 0 0 1 1.4 0Z" />
              </svg>
              {item}
            </li>
          ))}
        </ul>
      </aside>
      <div className={styles.shellMain}>{children}</div>
    </section>
  );
}
