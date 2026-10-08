import { useCallback, useEffect, useRef, useState, type FormEvent } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { Download, Eye, EyeOff, Pencil, Plus, Trash2 } from "lucide-react";
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
import { exportToExcel } from "@/lib/export-excel";
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
      { title: "eLibrary | Digital Library" },
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
  const [searchTerm, setSearchTerm] = useState("");
  const [roleFilter, setRoleFilter] = useState("all");
  const [loanFilter, setLoanFilter] = useState("all");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalRecords, setTotalRecords] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [memberToDelete, setMemberToDelete] = useState<Member | null>(null);
  const [selectedMember, setSelectedMember] = useState<Member | null>(null);
  const [dialogMode, setDialogMode] = useState<MemberDialogMode>("view");
  const [addMemberOpen, setAddMemberOpen] = useState(false);
  const usersRequestId = useRef(0);

  const loadUsers = useCallback(async (pageNumber = 1, query = "", role = "all") => {
    const requestId = ++usersRequestId.current;
    setError(null);
    try {
      const response = await getUsersPaged(pageNumber, DEFAULT_PAGE_SIZE, query, role);
      if (requestId !== usersRequestId.current) return;
      setMembers((response.data ?? []).map(mapUserDto));
      setPage(response.pageNumber || pageNumber);
      setTotalPages(response.totalPages || 1);
      setTotalRecords(response.totalRecords || (response.data ?? []).length);
    } catch (requestError) {
      if (requestId !== usersRequestId.current) return;
      setError(requestError instanceof Error ? requestError.message : "Unable to load members.");
      setMembers([]);
    }
  }, []);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => void loadUsers(1, searchTerm, roleFilter), 250);
    return () => window.clearTimeout(timeoutId);
  }, [loadUsers, searchTerm, roleFilter]);

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
      await loadUsers(page, searchTerm, roleFilter);
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
      await loadUsers(page, searchTerm, roleFilter);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Unable to remove this member.");
      setMemberToDelete(null);
    }
  };

  const handleAddMember = async (username: string, email: string, password: string) => {
    await registerUser(username, email, password);
    setInfo(`Added ${username}.`);
    await loadUsers(1, searchTerm, roleFilter);
  };

  const handleExport = async () => {
    setError(null);
    try {
      const firstPage = await getUsersPaged(1, 100, searchTerm, roleFilter);
      const exportMembers = (firstPage.data ?? []).map(mapUserDto);
      for (let nextPage = 2; nextPage <= firstPage.totalPages; nextPage += 1) {
        const response = await getUsersPaged(nextPage, 100, searchTerm, roleFilter);
        exportMembers.push(...(response.data ?? []).map(mapUserDto));
      }

      exportToExcel(
        exportMembers.map((member) => ({
          ID: member.id,
          Username: member.name,
          Email: member.email,
          Role: member.role,
        })),
        "elibrary-users",
        "Users",
      );
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Unable to export users.");
    }
  };

  return (
    <AdminLayout title="Users" subtitle="Manage library members">
      <FilterBar
        inline
        searchPlaceholder="Search by username or email..."
        searchValue={searchTerm}
        onSearchChange={setSearchTerm}
        onClear={() => {
          setSearchTerm("");
          setRoleFilter("all");
          setLoanFilter("all");
        }}
      >
        <FilterField label="Role">
          <Select value={roleFilter} onValueChange={setRoleFilter}>
            <SelectTrigger aria-label="Role">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All roles</SelectItem>
              <SelectItem value="Admin">Admin</SelectItem>
              <SelectItem value="User">User</SelectItem>
            </SelectContent>
          </Select>
        </FilterField>
        <FilterField label="Active loans">
          <Select value={loanFilter} onValueChange={setLoanFilter}>
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
      </FilterBar>

      {error ? <p className="text-sm text-destructive">{error}</p> : null}
      {info ? <p className="text-sm text-muted-foreground">{info}</p> : null}

      <SectionCard
        title="Members"
        description={`${totalRecords} registered accounts`}
        action={
          <div className="flex items-center gap-2">
            <Button size="sm" onClick={() => setAddMemberOpen(true)}>
              <Plus className="h-4 w-4" /> Add member
            </Button>
            <Button variant="outline" size="sm" onClick={() => void handleExport()}>
              <Download className="h-4 w-4" /> Export
            </Button>
          </div>
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
        <ListPagination
          page={page}
          totalPages={totalPages}
          onPageChange={(next) => void loadUsers(next, searchTerm, roleFilter)}
        />
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
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [pendingMember, setPendingMember] = useState<{
    username: string;
    email: string;
    password: string;
  } | null>(null);
  const formRef = useRef<HTMLFormElement | null>(null);

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    onError("");

    const formData = new FormData(event.currentTarget);
    setPendingMember({
      username: String(formData.get("username") ?? "").trim(),
      email: String(formData.get("email") ?? "").trim(),
      password: String(formData.get("password") ?? ""),
    });
    setConfirmOpen(true);
  };

  const handleConfirmedAdd = async () => {
    if (!pendingMember) return;
    setIsSubmitting(true);
    try {
      await onCreated(pendingMember.username, pendingMember.email, pendingMember.password);
      formRef.current?.reset();
      onOpenChange(false);
      setPendingMember(null);
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
        <form ref={formRef} className="space-y-4" onSubmit={handleSubmit}>
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
        <ConfirmDialog
          open={confirmOpen}
          onOpenChange={setConfirmOpen}
          title="Add member"
          description={
            pendingMember
              ? `Are you sure you want to add "${pendingMember.username}"?`
              : "Are you sure you want to add this member?"
          }
          confirmLabel="Add member"
          variant="default"
          onConfirm={() => void handleConfirmedAdd()}
        />
      </DialogContent>
    </Dialog>
  );
}
