"use client";

import { useMutation, useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { authApi } from "@/lib/auth/api";
import { logout } from "@/lib/auth/session";
import type { AppError } from "@/lib/errors";
import { formatDate, formatNumber } from "@/lib/utils";
import { hasRole, useAuthStatus } from "@/store/auth-store";
import { useAuthUIStore } from "@/store/auth-ui-store";
import type { ResendConfirmationResponse, UserDto } from "@/types/auth";
import styles from "@/styles/auth.module.css";
import { AuthErrorState } from "./AuthErrorState";
import { AuthLoading } from "./AuthLoading";
import { FormAlert } from "./FormAlert";
import { ProfileForm } from "./ProfileForm";
import { RoleBadge } from "./RoleBadge";
import { UserAvatar } from "./UserAvatar";

interface Shortcut {
  href: string;
  label: string;
  description: string;
}

/** Lối tắt theo vai trò (SRS 2.3: Admin có mọi quyền của Author). */
function shortcutsFor(user: UserDto): Shortcut[] {
  const isAdmin = hasRole(user, "Admin");
  const isAuthor = isAdmin || hasRole(user, "Author");
  const items: Shortcut[] = [];
  if (isAuthor) {
    items.push(
      { href: "/dashboard/recipes/new", label: "Viết công thức mới", description: "Soạn và lưu nháp công thức" },
      { href: "/dashboard/recipes", label: "Công thức của tôi", description: "Bản nháp, đã xuất bản, đã lưu trữ" },
      { href: "/dashboard", label: "Bảng điều khiển", description: "Tổng quan hoạt động của bạn" },
    );
  }
  if (isAdmin) {
    items.push(
      { href: "/dashboard/categories", label: "Quản lý danh mục", description: "Thêm, sửa, sắp xếp danh mục" },
      { href: "/admin/recipes/trash", label: "Thùng rác công thức", description: "Khôi phục hoặc xoá vĩnh viễn" },
    );
  }
  items.push({ href: "/recipes", label: "Khám phá công thức", description: "Xem các món mới nhất" });
  return items;
}

const DAY_MS = 24 * 60 * 60 * 1000;

function memberDays(createdAt: string): number {
  const created = new Date(createdAt).getTime();
  return Number.isNaN(created) ? 0 : Math.max(1, Math.ceil((Date.now() - created) / DAY_MS));
}

/** FR-AUTH-006: trang hồ sơ cá nhân (thông tin tài khoản, lối tắt, chỉnh sửa hồ sơ, bảo mật). */
export function ProfileView() {
  const status = useAuthStatus();
  const router = useRouter();
  const showToast = useAuthUIStore((s) => s.showToast);

  const meQuery = useQuery<UserDto, AppError>({
    queryKey: ["me"],
    queryFn: authApi.me,
    enabled: status === "authenticated",
  });

  const resendMutation = useMutation<ResendConfirmationResponse, AppError, string>({
    mutationFn: (email) => authApi.resendConfirmation(email),
    onSuccess: (r) => showToast("success", r.message),
    onError: (error) => showToast("error", error.message),
  });

  const logoutMutation = useMutation({
    mutationFn: logout,
    onSettled: () => router.replace("/"),
  });

  const me = meQuery.data;

  if (status !== "authenticated" || meQuery.isLoading) {
    return (
      <section className={styles.page}>
        <AuthLoading message="Đang tải hồ sơ..." />
      </section>
    );
  }

  if (meQuery.isError || !me) {
    return (
      <section className={styles.page}>
        <div className={styles.card}>
          <AuthErrorState
            title="Không tải được hồ sơ"
            message={meQuery.error?.message}
            onRetry={() => void meQuery.refetch()}
          />
        </div>
      </section>
    );
  }

  const resendButton = (
    <button
      type="button"
      className={`${styles.button} ${styles.buttonSmall}`}
      disabled={resendMutation.isPending}
      onClick={() => resendMutation.mutate(me.email)}
    >
      {resendMutation.isPending ? "Đang gửi..." : "Gửi lại email xác nhận"}
    </button>
  );

  return (
    <div className={styles.profile}>
      <header className={styles.hero}>
        <div className={styles.heroCover} />
        <div className={styles.heroBody}>
          <div className={styles.heroAvatar}>
            <UserAvatar name={me.displayName} src={me.avatarUrl} size="xl" />
          </div>
          <div className={styles.heroIdentity}>
            <h1 className={styles.heroName}>{me.displayName}</h1>
            <p className={styles.heroEmail}>{me.email}</p>
            <p className={styles.meta}>
              {me.roles.map((role) => (
                <RoleBadge key={role} role={role} />
              ))}
              <span>Tham gia {formatDate(me.createdAt)}</span>
            </p>
          </div>
        </div>
        <p className={me.bio ? styles.heroBio : `${styles.heroBio} ${styles.heroBioEmpty}`}>
          {me.bio || "Bạn chưa viết giới thiệu. Thêm vài dòng ở phần Thông tin cá nhân bên dưới nhé."}
        </p>
      </header>

      {!me.emailConfirmed && (
        <FormAlert variant="warning" action={resendButton}>
          Email chưa được xác nhận nên bạn chưa xuất bản được công thức.
        </FormAlert>
      )}

      <div className={styles.profileGrid}>
        <aside className={styles.profileAside}>
          <section className={styles.panel} aria-labelledby="account-heading">
            <h2 id="account-heading" className={styles.panelTitle}>
              Tài khoản
            </h2>
            <dl className={styles.infoList}>
              <div>
                <dt>Trạng thái email</dt>
                <dd>
                  <span className={me.emailConfirmed ? styles.chipSuccess : styles.chipWarning}>
                    {me.emailConfirmed ? "Đã xác nhận" : "Chưa xác nhận"}
                  </span>
                </dd>
              </div>
              <div>
                <dt>Vai trò</dt>
                <dd>{me.roles.length ? me.roles.map((role) => <RoleBadge key={role} role={role} />) : "Thành viên"}</dd>
              </div>
              <div>
                <dt>Ngày tham gia</dt>
                <dd>{formatDate(me.createdAt)}</dd>
              </div>
              <div>
                <dt>Thành viên được</dt>
                <dd>{formatNumber(memberDays(me.createdAt), 0)} ngày</dd>
              </div>
            </dl>
          </section>

          <nav className={styles.panel} aria-labelledby="shortcut-heading">
            <h2 id="shortcut-heading" className={styles.panelTitle}>
              Lối tắt
            </h2>
            <ul className={styles.shortcutList}>
              {shortcutsFor(me).map((item) => (
                <li key={item.href}>
                  <Link href={item.href} className={styles.shortcut}>
                    <span>
                      <strong>{item.label}</strong>
                      <small>{item.description}</small>
                    </span>
                    <span aria-hidden="true">→</span>
                  </Link>
                </li>
              ))}
            </ul>
          </nav>
        </aside>

        <div className={styles.profileMain}>
          <section className={styles.panel} aria-labelledby="info-heading">
            <h2 id="info-heading" className={styles.panelTitle}>
              Thông tin cá nhân
            </h2>
            <p className={styles.panelText}>Tên, ảnh và giới thiệu được hiển thị trên các công thức bạn đăng.</p>
            <ProfileForm me={me} />
          </section>

          <section className={styles.panel} aria-labelledby="security-heading">
            <h2 id="security-heading" className={styles.panelTitle}>
              Bảo mật &amp; phiên đăng nhập
            </h2>
            <ul className={styles.settingList}>
              <li>
                <div>
                  <strong>Xác nhận email</strong>
                  <small>
                    {me.emailConfirmed
                      ? "Email đã được xác nhận, bạn có thể xuất bản công thức."
                      : "Xác nhận email để mở khoá chức năng xuất bản công thức."}
                  </small>
                </div>
                {me.emailConfirmed ? <span className={styles.chipSuccess}>Đã xác nhận</span> : resendButton}
              </li>
              <li>
                <div>
                  <strong>Phiên đăng nhập</strong>
                  <small>Bạn đang đăng nhập trên trình duyệt này. Phiên tự gia hạn khi bạn còn sử dụng.</small>
                </div>
                <button
                  type="button"
                  className={`${styles.button} ${styles.buttonSmall} ${styles.buttonDanger}`}
                  disabled={logoutMutation.isPending}
                  onClick={() => logoutMutation.mutate()}
                >
                  {logoutMutation.isPending ? "Đang đăng xuất..." : "Đăng xuất"}
                </button>
              </li>
            </ul>
          </section>
        </div>
      </div>
    </div>
  );
}
