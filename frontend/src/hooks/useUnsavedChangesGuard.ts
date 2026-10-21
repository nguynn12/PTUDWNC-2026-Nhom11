"use client";

import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";

/**
 * Chặn rời trang khi còn thay đổi chưa lưu:
 * - đóng tab, tải lại, sang trang ngoài: hộp thoại gốc của trình duyệt (beforeunload);
 * - bấm liên kết nội bộ (menu, logo, lối tắt...): chặn lại và trả về `pendingHref` để hiện hộp thoại xác nhận.
 * Nút Back của trình duyệt không chặn được trong App Router; phần này chỉ phủ các trường hợp trên.
 */
export function useUnsavedChangesGuard(isDirty: boolean) {
  const router = useRouter();
  const [pendingHref, setPendingHref] = useState<string | null>(null);

  useEffect(() => {
    if (!isDirty) return;

    const onBeforeUnload = (event: BeforeUnloadEvent) => {
      event.preventDefault();
      event.returnValue = "";
    };

    const onClick = (event: MouseEvent) => {
      if (event.defaultPrevented || event.button !== 0) return;
      if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
      const anchor = (event.target as Element | null)?.closest?.("a[href]") as HTMLAnchorElement | null;
      if (!anchor || (anchor.target && anchor.target !== "_self") || anchor.hasAttribute("download")) return;

      const url = new URL(anchor.href, window.location.href);
      if (url.origin !== window.location.origin) return; // trang ngoài: beforeunload lo
      if (url.pathname === window.location.pathname && url.search === window.location.search) return; // chuyển mục trong trang

      event.preventDefault();
      event.stopPropagation();
      setPendingHref(`${url.pathname}${url.search}${url.hash}`);
    };

    window.addEventListener("beforeunload", onBeforeUnload);
    document.addEventListener("click", onClick, true); // capture: chạy trước bộ xử lý của next/link
    return () => {
      window.removeEventListener("beforeunload", onBeforeUnload);
      document.removeEventListener("click", onClick, true);
    };
  }, [isDirty]);

  /** Ở lại trang và giữ nguyên dữ liệu đang nhập. */
  const stay = useCallback(() => setPendingHref(null), []);

  /** Bỏ thay đổi và đi tới liên kết đã bị chặn. */
  const leave = useCallback(() => {
    if (pendingHref) router.push(pendingHref);
    setPendingHref(null);
  }, [pendingHref, router]);

  return { pendingHref, stay, leave };
}
