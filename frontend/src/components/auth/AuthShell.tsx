import { Check, ChefHat } from "lucide-react";
import type { ReactNode } from "react";

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
  if (!showIntro) {
    return <section className="mx-auto flex w-full max-w-6xl justify-center px-4 py-12 sm:py-16">{children}</section>;
  }
  return (
    <section className="mx-auto grid w-full max-w-5xl items-stretch gap-8 px-4 py-10 sm:py-14 lg:grid-cols-[1fr_28rem]">
      <aside
        aria-hidden="true"
        className="relative hidden flex-col justify-center gap-5 overflow-hidden rounded-2xl bg-gradient-to-br from-primary to-[#963e0b] p-10 text-primary-foreground shadow-lg lg:flex"
      >
        <div className="pointer-events-none absolute -top-16 -right-16 size-56 rounded-full bg-white/10" />
        <div className="pointer-events-none absolute -bottom-20 -left-10 size-48 rounded-full bg-white/5" />
        <span className="flex size-12 items-center justify-center rounded-xl bg-white/15">
          <ChefHat className="size-6" />
        </span>
        <p className="text-xs font-bold tracking-[0.2em] text-white/80 uppercase">Culinary Blog</p>
        <h2 className="font-serif text-4xl leading-tight font-bold text-white">Nấu ngon hơn mỗi ngày</h2>
        <p className="leading-relaxed text-white/90">
          Tham gia cộng đồng yêu bếp: ghi lại công thức, học hỏi món mới và chia sẻ hương vị của bạn.
        </p>
        <ul className="mt-2 grid gap-3">
          {HIGHLIGHTS.map((item) => (
            <li key={item} className="flex items-center gap-3 font-medium">
              <span className="flex size-6 shrink-0 items-center justify-center rounded-full bg-white/20">
                <Check className="size-3.5" />
              </span>
              {item}
            </li>
          ))}
        </ul>
      </aside>
      <div className="flex items-center justify-center">{children}</div>
    </section>
  );
}
