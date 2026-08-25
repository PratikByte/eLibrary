import { Upload } from "lucide-react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { SectionCard } from "@/components/SectionCard";

/** Account details form shared by the member and librarian profile pages. */
export function ProfileForm({
  name,
  email,
  role,
  profileImageUrl,
}: {
  name: string;
  email: string;
  role: string;
  profileImageUrl?: string | null;
}) {
  const initials = name
    .split(" ")
    .map((part) => part[0])
    .join("");

  return (
    <div className="grid gap-5 lg:grid-cols-3">
      <SectionCard className="lg:col-span-1">
        <div className="flex flex-col items-center text-center">
          <Avatar className="h-20 w-20">
            {profileImageUrl ? <AvatarImage src={profileImageUrl} alt={name} /> : null}
            <AvatarFallback className="bg-accent text-xl text-accent-foreground">
              {initials}
            </AvatarFallback>
          </Avatar>
          <p className="mt-3 text-base font-semibold">{name}</p>
          <p className="text-sm text-muted-foreground">{email}</p>
          <Badge className="mt-3" variant="secondary">
            {role}
          </Badge>
          <Button variant="outline" size="sm" className="mt-4">
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
            <Input id="profile-name" defaultValue={name} />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="profile-email">Email</Label>
            <Input id="profile-email" type="email" defaultValue={email} />
          </div>
          <div className="grid gap-2 sm:col-span-2">
            <Label htmlFor="profile-photo">Profile picture</Label>
            <Input id="profile-photo" type="file" />
          </div>
        </div>
        <div className="mt-5 flex gap-2">
          <Button>Save changes</Button>
          <Button variant="outline">Cancel</Button>
        </div>
      </SectionCard>
    </div>
  );
}
