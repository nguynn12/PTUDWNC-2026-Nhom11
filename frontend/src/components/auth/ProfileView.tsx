"use client";

import { useQuery } from "@tanstack/react-query";
import { useCallback, useState, useSyncExternalStore } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { useUnsavedChangesGuard } from "@/hooks/useUnsavedChangesGuard";
import { authApi } from "@/lib/auth/api";
import type { AppError } from "@/lib/errors";
import { cn } from "@/lib/utils";
import { useAuthStatus } from "@/store/auth-store";
import type { UserDto } from "@/types/auth";
import { AuthErrorState } from "./AuthErrorState";
import { AuthLoading } from "./AuthLoading";
import { ConfirmActionDialog } from "./ConfirmActionDialog";
import { FormAlert } from "./FormAlert";
import { PROFILE_THEME_CLASS } from "./profile-theme";
import { sectionTitleClass, surfaceClass } from "./profile-styles";
import { ProfileForm } from "./ProfileForm";
import { ProfileHeader } from "./ProfileHeader";
import { ProfileSidebar, SECTION_ANCHORS, type ProfileSection } from "./ProfileSidebar";
import { ResendConfirmationButton } from "./ResendConfirmationButton";
import { SecuritySection } from "./SecuritySection";

// --- Mục đang xem lưu trên URL hash (#thong-tin-ca-nhan | #bao-mat) để liên kết được và nút Back hoạt động.
//     "#doi-mat-khau" là hash cũ của mục đổi mật khẩu nên vẫn mở mục Bảo mật.
const hashListeners = new Set<() => void>();

function subscribeHash(listener: () => void) {
  hashListeners.add(listener);
  window.addEventListener("hashchange", listener);
  return () => {
    hashListeners.delete(listener);
    window.removeEventListener("hashchange", listener);
  };
}

function sectionFromHash(hash: string): ProfileSection {
  return hash === `#${SECTION_ANCHORS.security}` || hash === "#doi-mat-khau" ? "security" : "profile";
}

function useActiveSection(): [ProfileSection, (section: ProfileSection) => void] {
  const section = useSyncExternalStore(
    subscribeHash,
    () => sectionFromHash(window.location.hash),
    () => "profile" as const,
  );
  const select = useCallback((next: ProfileSection) => {
    window.history.replaceState(null, "", `#${SECTION_ANCHORS[next]}`);
    hashListeners.forEach((listener) => listener());
    // Đưa focus về tiêu đề mục mới để trình đọc màn hình đọc đúng nội dung vừa chuyển tới.
    setTimeout(() => {
      const heading = document.getElementById(`${SECTION_ANCHORS[next]}-title`);
      heading?.focus({ preventScroll: true });
      heading?.scrollIntoView({ block: "start" });
    }, 0);
  }, []);
  return [section, select];
}

function Page({ children }: { children: React.ReactNode }) {
  return (
    <div className={cn(PROFILE_THEME_CLASS, "min-h-[calc(100vh-var(--header-height))]")}>
      <div className="mx-auto w-full max-w-4xl px-4 py-8 sm:px-6 sm:py-10">{children}</div>
    </div>
  );
}

/** FR-AUTH-006: trang hồ sơ cá nhân (ảnh đại diện & thông tin, bảo mật & phiên đăng nhập). */
export function ProfileView() {
  const status = useAuthStatus();
  const [section, selectSection] = useActiveSection();
  const [dirty, setDirty] = useState(false);
  const guard = useUnsavedChangesGuard(dirty);

  const meQuery = useQuery<UserDto, AppError>({
    queryKey: ["me"],
    queryFn: authApi.me,
    enabled: status === "authenticated",
  });
  const me = meQuery.data;

  if (status !== "authenticated" || meQuery.isLoading) {
    return (
      <Page>
        <AuthLoading message="Đang tải hồ sơ..." className="py-16" />
      </Page>
    );
  }

  if (meQuery.isError || !me) {
    return (
      <Page>
        <Card className="mx-auto w-full max-w-md">
          <CardContent>
            <AuthErrorState
              title="Không tải được hồ sơ"
              message={meQuery.error?.message}
              onRetry={() => void meQuery.refetch()}
            />
          </CardContent>
        </Card>
      </Page>
    );
  }

  const editProfile = () => {
    selectSection("profile");
    setTimeout(() => document.getElementById("displayName")?.focus(), 0);
  };

  return (
    <Page>
      <div className="grid gap-8">
        <ProfileHeader me={me} onEdit={editProfile} />

        {!me.emailConfirmed && (
          <FormAlert
            variant="warning"
            title="Email chưa được xác nhận"
            action={<ResendConfirmationButton email={me.email} className="mt-1" />}
          >
            Bạn chưa xuất bản được công thức cho đến khi xác nhận email.
          </FormAlert>
        )}

        <div className="grid items-start gap-6 lg:grid-cols-[14.5rem_minmax(0,1fr)] lg:gap-10">
          <ProfileSidebar user={me} activeSection={section} onSelectSection={selectSection} />

          <div className="min-w-0">
            {/* Cả hai mục luôn được mount (chỉ ẩn mục không xem) để dữ liệu đang nhập không mất khi chuyển mục. */}
            <section
              id={SECTION_ANCHORS.profile}
              aria-labelledby={`${SECTION_ANCHORS.profile}-title`}
              hidden={section !== "profile"}
              className={cn(surfaceClass, "scroll-mt-24 p-5 sm:p-8")}
            >
              <h2 id={`${SECTION_ANCHORS.profile}-title`} tabIndex={-1} className={cn(sectionTitleClass, "outline-none")}>
                Thông tin cá nhân
              </h2>
              <p className="mt-1 mb-7 text-sm text-muted-foreground">
                Tên, ảnh và giới thiệu được hiển thị trên các công thức bạn đăng.
              </p>
              <ProfileForm key={me.id} me={me} onDirtyChange={setDirty} />
            </section>

            <section
              id={SECTION_ANCHORS.security}
              aria-labelledby={`${SECTION_ANCHORS.security}-title`}
              hidden={section !== "security"}
              className={cn(surfaceClass, "scroll-mt-24 p-5 sm:p-8")}
            >
              <h2 id={`${SECTION_ANCHORS.security}-title`} tabIndex={-1} className={cn(sectionTitleClass, "outline-none")}>
                Bảo mật &amp; phiên đăng nhập
              </h2>
              <p className="mt-1 mb-7 text-sm text-muted-foreground">
                Xác nhận email, mật khẩu và phiên đăng nhập trên trình duyệt này.
              </p>
              <SecuritySection me={me} hasUnsavedChanges={dirty} />
            </section>
          </div>
        </div>
      </div>

      <ConfirmActionDialog
        open={guard.pendingHref !== null}
        onOpenChange={(open) => !open && guard.stay()}
        title="Rời trang và bỏ thay đổi?"
        description="Hồ sơ của bạn có thay đổi chưa lưu. Nếu rời trang bây giờ, những thay đổi này sẽ bị mất."
        confirmLabel="Rời trang"
        cancelLabel="Ở lại chỉnh sửa"
        onConfirm={guard.leave}
      />
    </Page>
  );
}
