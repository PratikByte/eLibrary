import { useCallback, useEffect, useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { Eye, EyeOff, Pencil, Plus, Trash2 } from "lucide-react";
import { AdminLayout } from "@/components/AdminLayout";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { MemberDetailsDialog, type MemberDialogMode } from "@/components/MemberDetailsDialog";
import { SectionCard } from "@/components/SectionCard";
import { FilterBar, FilterField } from "@/components/FilterBar";
import { ListPagination } from "@/components/list-pagination";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { Member } from "@/lib/placeholder-data";
import {
  DEFAULT_PAGE_SIZE,
  deleteUser,
  getUsersPaged,
  mapUserDto,
  registerUser,
  updateUserProfile,
} from "@/lib/api";

export const Route = createFileRoute("/admin/users")({
  head: () => ({
    meta: [
      { title: "Members — eLibrary Admin" },
      { name: "description", content: "View library members, their loans and account details." },
      { property: "og:title", content: "Members — eLibrary Admin" },
      {
        property: "og:description",
        content: "View library members, their loans and account details.",
      },
    ],
  }),
  component: AdminUsersPage,
});

function AdminUsersPage() {
  const [members, setMembers] = useState<Member[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalRecords, setTotalRecords] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [memberToDelete, setMemberToDelete] = useState<Member | null>(null);
  const [selectedMember, setSelectedMember] = useState<Member | null>(null);
  const [dialogMode, setDialogMode] = useState<MemberDialogMode>("view");
  const [addMemberOpen, setAddMemberOpen] = useState(false);

  const loadUsers = useCallback(async (pageNumber = 1) => {
    setError(null);
    try {
      const response = await getUsersPaged(pageNumber, DEFAULT_PAGE_SIZE);
      setMembers((response.data ?? []).map(mapUserDto));
      setPage(response.pageNumber || pageNumber);
      setTotalPages(response.totalPages || 1);
      setTotalRecords(response.totalRecords || (response.data ?? []).length);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Unable to load members.");
      setMembers([]);
    }
  }, []);

  useEffect(() => {
    void loadUsers(1);
  }, [loadUsers]);

  const openMember = (member: Member, mode: MemberDialogMode) => {
    setSelectedMember(member);
    setDialogMode(mode);
  };

  const handleSave = async (updated: Member, profileImage?: File | null) => {
    try {
      await updateUserProfile(updated.id, {
        username: updated.name,
        email: updated.email,
        profileImage: profileImage ?? null,
      });
      setInfo(`Updated ${updated.name}.`);
      setSelectedMember(null);
      await loadUsers(page);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Unable to update this member.");
    }
  };

  const handleDelete = async () => {
    if (!memberToDelete) return;
    try {
      await deleteUser(memberToDelete.id);
      setInfo(`Removed ${memberToDelete.name}.`);
      setMemberToDelete(null);
      await loadUsers(page);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Unable to remove this member.");
      setMemberToDelete(null);
    }
  };

  const handleAddMember = async (username: string, email: string, password: string) => {
    await registerUser(username, email, password);
    setInfo(`Added ${username}.`);
    await loadUsers(1);
  };

  return (
    <AdminLayout title="Users" subtitle="Manage library members" searchPlaceholder="Search members…">
      <FilterBar searchPlaceholder="Search by name, email or member ID…">
        <FilterField label="Role">
          <Select defaultValue="all">
            <SelectTrigger aria-label="Role">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All roles</SelectItem>
              <SelectItem value="admin">Admin</SelectItem>
              <SelectItem value="member">Member</SelectItem>
            </SelectContent>
          </Select>
        </FilterField>
        <FilterField label="Active loans">
          <Select defaultValue="all">
            <SelectTrigger aria-label="Active loans">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Any</SelectItem>
              <SelectItem value="with">With loans</SelectItem>
              <SelectItem value="without">No loans</SelectItem>
            </SelectContent>
          </Select>
        </FilterField>
        <FilterField label="Joined from" htmlFor="member-from">
          <Input id="member-from" type="date" />
        </FilterField>
        <FilterField label="Joined to" htmlFor="member-to">
          <Input id="member-to" type="date" />
        </FilterField>
      </FilterBar>

      {error ? <p className="text-sm text-destructive">{error}</p> : null}
      {info ? <p className="text-sm text-muted-foreground">{info}</p> : null}

      <SectionCard
        title="Members"
        description={`${totalRecords} registered accounts`}
        action={
          <Button size="sm" onClick={() => setAddMemberOpen(true)}>
            <Plus className="h-4 w-4" /> Add member
          </Button>
        }
      >
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Member</TableHead>
                <TableHead>Email</TableHead>
                <TableHead>Role</TableHead>
                <TableHead>Active loans</TableHead>
                <TableHead>Joined</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {members.map((member) => (
                <TableRow key={member.id}>
                  <TableCell>
                    <div className="flex items-center gap-3">
                      <Avatar className="h-9 w-9">
                        {member.avatar ? (
                          <AvatarImage src={member.avatar} alt={member.name} />
                        ) : null}
                        <AvatarFallback className="bg-accent text-accent-foreground">
                          {member.name
                            .split(" ")
                            .map((part) => part[0])
                            .join("")}
                        </AvatarFallback>
                      </Avatar>
                      <span className="font-medium">{member.name}</span>
                    </div>
                  </TableCell>
                  <TableCell className="text-muted-foreground">{member.email}</TableCell>
                  <TableCell>
                    <Badge variant={member.role === "Admin" ? "default" : "secondary"}>
                      {member.role}
                    </Badge>
                  </TableCell>
                  <TableCell>—</TableCell>
                  <TableCell className="text-muted-foreground">—</TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-1">
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label="View member"
                        className="text-primary hover:bg-accent hover:text-primary"
                        onClick={() => openMember(member, "view")}
                      >
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label="Edit member"
                        className="text-foreground hover:bg-accent hover:text-foreground"
                        onClick={() => openMember(member, "edit")}
                      >
                        <Pencil className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label="Remove member"
                        className="text-destructive hover:bg-destructive/10 hover:text-destructive"
                        onClick={() => setMemberToDelete(member)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
        <ListPagination page={page} totalPages={totalPages} onPageChange={(next) => void loadUsers(next)} />
      </SectionCard>

      <MemberDetailsDialog
        member={selectedMember}
        mode={dialogMode}
        open={selectedMember !== null}
        onOpenChange={(open) => {
          if (!open) setSelectedMember(null);
        }}
        onSave={(updated, profileImage) => void handleSave(updated, profileImage)}
      />

      <AddMemberDialog
        open={addMemberOpen}
        onOpenChange={setAddMemberOpen}
        onCreated={handleAddMember}
        onError={setError}
      />

      <ConfirmDialog
        open={memberToDelete !== null}
        onOpenChange={(open) => {
          if (!open) setMemberToDelete(null);
        }}
        title="Remove member"
        description={
          memberToDelete
            ? `Are you sure you want to remove "${memberToDelete.name}"? This action cannot be undone.`
            : "Are you sure you want to remove this member? This action cannot be undone."
        }
        confirmLabel="Remove"
        cancelLabel="Cancel"
        variant="destructive"
        onConfirm={() => void handleDelete()}
      />
    </AdminLayout>
  );
}

function AddMemberDialog({
  open,
  onOpenChange,
  onCreated,
  onError,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onCreated: (username: string, email: string, password: string) => Promise<void>;
  onError: (message: string) => void;
}) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setIsSubmitting(true);
    onError("");

    const formData = new FormData(event.currentTarget);
    try {
      await onCreated(
        String(formData.get("username") ?? "").trim(),
        String(formData.get("email") ?? "").trim(),
        String(formData.get("password") ?? ""),
      );
      event.currentTarget.reset();
      onOpenChange(false);
    } catch (requestError) {
      onError(requestError instanceof Error ? requestError.message : "Unable to add this member.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Add member</DialogTitle>
          <DialogDescription>Create a new User account for the library.</DialogDescription>
        </DialogHeader>
        <form className="space-y-4" onSubmit={handleSubmit}>
          <div className="space-y-2">
            <Label htmlFor="admin-add-username">Username</Label>
            <Input id="admin-add-username" name="username" autoComplete="username" required />
          </div>
          <div className="space-y-2">
            <Label htmlFor="admin-add-email">Email</Label>
            <Input id="admin-add-email" name="email" type="email" autoComplete="email" required />
          </div>
          <div className="space-y-2">
            <Label htmlFor="admin-add-password">Password</Label>
            <div className="relative">
              <Input
                id="admin-add-password"
                name="password"
                type={showPassword ? "text" : "password"}
                autoComplete="new-password"
                className="pr-10"
                minLength={6}
                required
              />
              <button
                type="button"
                className="absolute inset-y-0 right-0 flex w-10 items-center justify-center text-foreground hover:text-primary"
                onClick={() => setShowPassword((visible) => !visible)}
                aria-label={showPassword ? "Hide password" : "Show password"}
                title={showPassword ? "Hide password" : "Show password"}
              >
                {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
              </button>
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Adding..." : "Add member"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
