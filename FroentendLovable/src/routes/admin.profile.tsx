import { createFileRoute } from "@tanstack/react-router";
import { AdminLayout } from "@/components/AdminLayout";
import { ProfileForm } from "@/components/ProfileForm";
import { getAuthSession, type AuthSession } from "@/lib/api";
import { useEffect, useState } from "react";

export const Route = createFileRoute("/admin/profile")({
  head: () => ({
    meta: [
      { title: "Librarian Profile — eLibrary Admin" },
      {
        name: "description",
        content: "Update your librarian account name, email and profile picture.",
      },
      { property: "og:title", content: "Librarian Profile — eLibrary Admin" },
      {
        property: "og:description",
        content: "Update your librarian account name, email and profile picture.",
      },
    ],
  }),
  component: AdminProfilePage,
});

function AdminProfilePage() {
  const [session, setSession] = useState<AuthSession | null>(null);

  useEffect(() => {
    setSession(getAuthSession());
  }, []);

  return (
    <AdminLayout title="Profile" subtitle="Your librarian account">
      {session ? (
        <ProfileForm name={session.username} email={session.email} role={session.role} />
      ) : (
        <p className="text-sm text-muted-foreground">Loading your profile...</p>
      )}
    </AdminLayout>
  );
}
