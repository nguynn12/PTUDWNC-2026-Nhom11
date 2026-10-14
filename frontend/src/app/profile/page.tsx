import type { Metadata } from "next";
import { ProfileForm } from "@/components/auth/ProfileForm";
import { RequireAuth } from "@/components/auth/RequireAuth";
import styles from "@/styles/auth.module.css";

export const metadata: Metadata = { title: "Hồ sơ cá nhân" };

export default function ProfilePage() {
  return (
    <RequireAuth>
      <section className={styles.page}>
        <ProfileForm />
      </section>
    </RequireAuth>
  );
}
