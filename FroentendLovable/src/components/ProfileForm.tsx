import { useEffect, useState, type FormEvent } from "react";
import { Upload } from "lucide-react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { SectionCard } from "@/components/SectionCard";
import { getAuthSession, setAuthSession, updateUserProfile } from "@/lib/api";

/** Account details form shared by the member and librarian profile pages. */
export function ProfileForm({
  userId,
  name,
  email,
  role,
  profileImageUrl,
}: {
  userId: string;
  name: string;
  email: string;
  role: string;
  profileImageUrl?: string | null;
}) {
  const [username, setUsername] = useState(name);
  const [emailValue, setEmailValue] = useState(email);
  const [imageUrl, setImageUrl] = useState(profileImageUrl ?? "");
  const [photoFile, setPhotoFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    setUsername(name);
    setEmailValue(email);
    setImageUrl(profileImageUrl ?? "");
    setPhotoFile(null);
  }, [name, email, profileImageUrl]);

  const initials = username
    .split(" ")
    .map((part) => part[0])
    .join("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    setIsSaving(true);

    try {
      const updated = await updateUserProfile(userId, {
        username,
        email: emailValue,
        profileImage: photoFile,
      });
      setUsername(updated.username);
      setEmailValue(updated.email);
      if (updated.profileImageUrl) {
        setImageUrl(updated.profileImageUrl);
      }
      setPhotoFile(null);

      const session = getAuthSession();
      if (session && session.userId === userId) {
        setAuthSession({
          ...session,
          username: updated.username,
          email: updated.email,
        });
      }
      setSuccess("Profile updated.");
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Unable to update your profile.");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <form className="grid gap-5 lg:grid-cols-3" onSubmit={handleSubmit}>
      <SectionCard className="lg:col-span-1">
        <div className="flex flex-col items-center text-center">
          <Avatar className="h-20 w-20">
            {imageUrl ? <AvatarImage src={imageUrl} alt={username} /> : null}
            <AvatarFallback className="bg-accent text-xl text-accent-foreground">
              {initials}
            </AvatarFallback>
          </Avatar>
          <p className="mt-3 text-base font-semibold">{username}</p>
          <p className="text-sm text-muted-foreground">{emailValue}</p>
          <Badge className="mt-3" variant="secondary">
            {role}
          </Badge>
          <Button
            type="button"
            variant="outline"
            size="sm"
            className="mt-4"
            onClick={() => document.getElementById("profile-photo")?.click()}
          >
            <Upload className="h-4 w-4" /> Change photo
          </Button>
        </div>
      </SectionCard>

      <SectionCard
        title="Account details"
        description="Keep your contact information up to date"
        className="lg:col-span-2"
      >
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="grid gap-2">
            <Label htmlFor="profile-name">Username</Label>
            <Input
              id="profile-name"
              value={username}
              onChange={(event) => setUsername(event.target.value)}
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="profile-email">Email</Label>
            <Input
              id="profile-email"
              type="email"
              value={emailValue}
              onChange={(event) => setEmailValue(event.target.value)}
            />
          </div>
          <div className="grid gap-2 sm:col-span-2">
            <Label htmlFor="profile-photo">Profile picture</Label>
            <Input
              id="profile-photo"
              type="file"
              accept="image/*"
              onChange={(event) => {
                const file = event.target.files?.[0] ?? null;
                setPhotoFile(file);
                if (file) setImageUrl(URL.createObjectURL(file));
              }}
            />
          </div>
        </div>
        {error ? <p className="mt-4 text-sm text-destructive">{error}</p> : null}
        {success ? <p className="mt-4 text-sm text-muted-foreground">{success}</p> : null}
        <div className="mt-5 flex gap-2">
          <Button type="submit" disabled={isSaving}>
            {isSaving ? "Saving..." : "Save changes"}
          </Button>
          <Button
            type="button"
            variant="outline"
            onClick={() => {
              setUsername(name);
              setEmailValue(email);
              setImageUrl(profileImageUrl ?? "");
              setPhotoFile(null);
              setError(null);
              setSuccess(null);
            }}
          >
            Cancel
          </Button>
        </div>
      </SectionCard>
    </form>
  );
}
