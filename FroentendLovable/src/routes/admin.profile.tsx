import { createFileRoute } from "@tanstack/react-router";
import { AdminLayout } from "@/components/AdminLayout";
import { Loader } from "@/components/Loader";
import { ProfileForm } from "@/components/ProfileForm";
import { getAuthSession, getUserProfile } from "@/lib/api";
import { useEffect, useState } from "react";

type UserProfile = {
  userId: string;
  username: string;
  email: string;
  role: string;
  profileImageUrl?: string | null;
};

export const Route = createFileRoute("/admin/profile")({
  head: () => ({
    meta: [
      { title: "eLibrary | Digital Library" },
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
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const session = getAuthSession();
    if (!session) {
      setError("Please sign in to view your profile.");
      return;
    }

    let cancelled = false;
    getUserProfile(session.userId)
      .then((response) => {
        if (cancelled) return;
        setProfile({
          userId: response.userId,
          username: response.username,
          email: response.email,
          role: response.role,
          profileImageUrl: response.profileImageUrl,
        });
      })
      .catch((requestError) => {
        if (!cancelled) {
          setError(
            requestError instanceof Error ? requestError.message : "Unable to load your profile.",
          );
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <AdminLayout title="Profile" subtitle="Your librarian account">
      {error ? (
        <p className="text-sm text-destructive">{error}</p>
      ) : profile ? (
        <ProfileForm
          userId={profile.userId}
          name={profile.username}
          email={profile.email}
          role={profile.role}
          profileImageUrl={profile.profileImageUrl ?? null}
        />
      ) : (
        <Loader label="Loading your profile..." />
      )}
    </AdminLayout>
  );
}
