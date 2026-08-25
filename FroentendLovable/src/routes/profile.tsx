import { createFileRoute } from "@tanstack/react-router";
import { UserLayout } from "@/components/UserLayout";
import { ProfileForm } from "@/components/ProfileForm";
import { apiRequest, getAuthSession } from "@/lib/api";
import { useEffect, useState } from "react";

type UserProfile = {
  userId: number;
  username: string;
  email: string;
  role: string;
  profileImageUrl?: string | null;
};

type ApiResponse<T> = {
  success: boolean;
  message?: string | null;
  data?: T | null;
};

export const Route = createFileRoute("/profile")({
  head: () => ({
    meta: [
      { title: "My Profile — eLibrary" },
      { name: "description", content: "Update your eLibrary email, username and profile picture." },
      { property: "og:title", content: "My Profile — eLibrary" },
      {
        property: "og:description",
        content: "Update your eLibrary email, username and profile picture.",
      },
    ],
  }),
  component: ProfilePage,
});

function ProfilePage() {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const session = getAuthSession();
    if (!session) {
      setError("Please sign in to view your profile.");
      return;
    }

    let cancelled = false;
    apiRequest<ApiResponse<UserProfile>>(`/api/UserProfile/${session.userId}`)
      .then((response) => {
        if (cancelled) return;
        if (!response.success || !response.data) {
          setError(response.message ?? "Unable to load your profile.");
          return;
        }
        setProfile(response.data);
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
    <UserLayout title="Profile" subtitle="Your account details">
      {error ? (
        <p className="text-sm text-destructive">{error}</p>
      ) : profile ? (
        <ProfileForm
          name={profile.username}
          email={profile.email}
          role={profile.role}
          profileImageUrl={profile.profileImageUrl}
        />
      ) : (
        <p className="text-sm text-muted-foreground">Loading your profile...</p>
      )}
    </UserLayout>
  );
}
