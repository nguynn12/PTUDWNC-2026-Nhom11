import type { Metadata } from "next";
import { ProfileView } from "@/components/auth/ProfileView";
import { RequireAuth } from "@/components/auth/RequireAuth";

export const metadata: Metadata = { title: "Hồ sơ cá nhân" };

export default function ProfilePage() {
  return (
    <RequireAuth>
      <ProfileView />
    </RequireAuth>
  );
}
