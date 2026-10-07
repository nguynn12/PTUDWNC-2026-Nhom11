import type { Metadata } from "next";
import { ProfileForm } from "@/components/auth/ProfileForm";
import { RequireAuth } from "@/components/auth/RequireAuth";

export const metadata: Metadata = { title: "Hồ sơ cá nhân | Culinary Blog" };

export default function ProfilePage() {
  return (
    <RequireAuth>
      <ProfileForm />
    </RequireAuth>
  );
}
