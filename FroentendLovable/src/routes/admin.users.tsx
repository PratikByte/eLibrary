import { useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { Trash2, Eye, Plus, Pencil } from "lucide-react";
import { AdminLayout } from "@/components/AdminLayout";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { MemberDetailsDialog, type MemberDialogMode } from "@/components/MemberDetailsDialog";
import { SectionCard } from "@/components/SectionCard";
import { FilterBar, FilterField } from "@/components/FilterBar";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
import { sampleMembers, type Member } from "@/lib/placeholder-data";

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
  const [members, setMembers] = useState<Member[]>(sampleMembers);
  const [memberToDelete, setMemberToDelete] = useState<Member | null>(null);
  const [selectedMember, setSelectedMember] = useState<Member | null>(null);
  const [dialogMode, setDialogMode] = useState<MemberDialogMode>("view");

  const openMember = (member: Member, mode: MemberDialogMode) => {
    setSelectedMember(member);
    setDialogMode(mode);
  };

  const handleSave = (updated: Member) => {
    setMembers((current) => current.map((item) => (item.id === updated.id ? updated : item)));
    setSelectedMember(null);
  };

  const handleDelete = () => {
    if (!memberToDelete) return;
    setMembers((current) => current.filter((item) => item.id !== memberToDelete.id));
    setMemberToDelete(null);
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

      <SectionCard

        title="Members"
        description={`${members.length} registered accounts`}
        action={
          <Button size="sm">
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
                  <TableCell>{member.loans}</TableCell>
                  <TableCell className="text-muted-foreground">{member.joined}</TableCell>
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
      </SectionCard>

      <MemberDetailsDialog
        member={selectedMember}
        mode={dialogMode}
        open={selectedMember !== null}
        onOpenChange={(open) => {
          if (!open) setSelectedMember(null);
        }}
        onSave={handleSave}
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
        onConfirm={handleDelete}
      />
    </AdminLayout>
  );
}
