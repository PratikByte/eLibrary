import { useEffect, useState } from "react";
import { ImagePreviewDialog } from "@/components/ImagePreviewDialog";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { Member } from "@/lib/placeholder-data";

export type MemberDialogMode = "view" | "edit";

/**
 * Member profile card shown in a dialog (admin only).
 * - mode "view": read-only details
 * - mode "edit": same card with editable fields + Save / Cancel
 *
 * Saving is local only for now (visual placeholder) until the API is connected.
 */
export function MemberDetailsDialog({
  member,
  mode,
  open,
  onOpenChange,
  onSave,
}: {
  member: Member | null;
  mode: MemberDialogMode;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSave?: (member: Member) => void;
}) {
  const [draft, setDraft] = useState<Member | null>(member);
  const [photoPreviewOpen, setPhotoPreviewOpen] = useState(false);

  useEffect(() => {
    setDraft(member);
  }, [member, mode, open]);

  if (!member || !draft) return null;

  const isEdit = mode === "edit";
  const initials = draft.name
    .split(" ")
    .map((part) => part[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  const update = (patch: Partial<Member>) =>
    setDraft((current) => (current ? { ...current, ...patch } : current));

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{isEdit ? "Edit member" : "Member details"}</DialogTitle>
          <DialogDescription>
            {isEdit
              ? "Update the member information and save your changes."
              : "Full account information for this member."}
          </DialogDescription>
        </DialogHeader>

        <div className="rounded-lg border bg-card p-4">
          <div className="flex items-start gap-3">
            <div className="flex flex-col items-center gap-2">
              {isEdit ? (
                <button
                  type="button"
                  aria-label="Enlarge profile photo"
                  className="rounded-full ring-offset-background transition hover:opacity-90 focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:outline-none"
                  onClick={() => setPhotoPreviewOpen(true)}
                >
                  <Avatar className="h-16 w-16">
                    {draft.avatar ? <AvatarImage src={draft.avatar} alt={draft.name} /> : null}
                    <AvatarFallback className="bg-accent text-accent-foreground">
                      {initials}
                    </AvatarFallback>
                  </Avatar>
                </button>
              ) : (
                <Avatar className="h-16 w-16">
                  {draft.avatar ? <AvatarImage src={draft.avatar} alt={draft.name} /> : null}
                  <AvatarFallback className="bg-accent text-accent-foreground">
                    {initials}
                  </AvatarFallback>
                </Avatar>
              )}
            </div>
            <div className="min-w-0">
              <p className="truncate text-base font-semibold">{draft.name}</p>
              <p className="truncate text-sm text-muted-foreground">{draft.email}</p>
              <div className="mt-2">
                <Badge variant={draft.role === "Admin" ? "default" : "secondary"}>
                  {draft.role}
                </Badge>
              </div>
            </div>
          </div>

          {isEdit ? (
            <div className="mt-5 grid gap-4 sm:grid-cols-2">
              <div className="grid gap-2 sm:col-span-2">
                <Label htmlFor="member-edit-name">Full name</Label>
                <Input
                  id="member-edit-name"
                  value={draft.name}
                  onChange={(event) => update({ name: event.target.value })}
                />
              </div>
              <div className="grid gap-2 sm:col-span-2">
                <Label htmlFor="member-edit-email">Email</Label>
                <Input
                  id="member-edit-email"
                  type="email"
                  value={draft.email}
                  onChange={(event) => update({ email: event.target.value })}
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="member-edit-role">Role</Label>
                <Select
                  value={draft.role}
                  onValueChange={(value) => update({ role: value as Member["role"] })}
                >
                  <SelectTrigger id="member-edit-role">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Member">Member</SelectItem>
                    <SelectItem value="Admin">Admin</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="grid gap-2">
                <Label htmlFor="member-edit-loans">Active loans</Label>
                <Input
                  id="member-edit-loans"
                  type="number"
                  value={draft.loans}
                  onChange={(event) => update({ loans: Number(event.target.value) })}
                />
              </div>
            </div>
          ) : (
            <dl className="mt-5 grid gap-4 sm:grid-cols-2">
              <div>
                <dt className="text-xs text-muted-foreground">Email</dt>
                <dd className="text-sm font-medium">{draft.email}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">Role</dt>
                <dd className="text-sm font-medium">{draft.role}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">Active loans</dt>
                <dd className="text-sm font-medium">{draft.loans}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">Joined</dt>
                <dd className="text-sm font-medium">{draft.joined}</dd>
              </div>
            </dl>
          )}
        </div>

        <DialogFooter>
          {isEdit ? (
            <>
              <Button variant="outline" onClick={() => onOpenChange(false)}>
                Cancel
              </Button>
              <Button
                onClick={() => {
                  onSave?.(draft);
                  onOpenChange(false);
                }}
              >
                Save changes
              </Button>
            </>
          ) : (
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Close
            </Button>
          )}
        </DialogFooter>

        <ImagePreviewDialog
          open={photoPreviewOpen}
          onOpenChange={setPhotoPreviewOpen}
          title={draft.name}
          imageUrl={draft.avatar}
          allowChange={isEdit}
          onImageSelected={(previewUrl) => update({ avatar: previewUrl })}
        />
      </DialogContent>
    </Dialog>
  );
}
