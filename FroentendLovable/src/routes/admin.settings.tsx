import { createFileRoute } from "@tanstack/react-router";
import { Save, ShieldCheck } from "lucide-react";
import { AdminLayout } from "@/components/AdminLayout";
import { SectionCard } from "@/components/SectionCard";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

export const Route = createFileRoute("/admin/settings")({
  head: () => ({
    meta: [
      { title: "eLibrary | Digital Library" },
      {
        name: "description",
        content: "Admin-only settings: library rules, loan limits, fines and account access.",
      },
      { property: "og:title", content: "Settings — eLibrary Admin" },
      {
        property: "og:description",
        content: "Admin-only settings: library rules, loan limits, fines and account access.",
      },
    ],
  }),
  component: AdminSettingsPage,
});

/** Visual placeholder toggle row. Wiring comes with the backend integration. */
function ToggleRow({
  id,
  label,
  description,
  defaultChecked,
}: {
  id: string;
  label: string;
  description: string;
  defaultChecked?: boolean | undefined;
}) {
  return (
    <div className="flex items-start justify-between gap-4 border-b border-border py-3 last:border-0">
      <div className="min-w-0">
        <Label htmlFor={id} className="text-sm font-medium">
          {label}
        </Label>
        <p className="mt-1 text-sm text-muted-foreground">{description}</p>
      </div>
      <Switch id={id} defaultChecked={defaultChecked ?? false} />
    </div>
  );
}

function AdminSettingsPage() {
  return (
    <AdminLayout title="Settings" subtitle="Admin-only configuration for your library">
      <SectionCard
        title="Access level"
        description="These settings are visible to Admin and Super admin accounts only"
        action={
          <Badge variant="secondary" className="gap-1">
            <ShieldCheck className="h-3.5 w-3.5" /> Admin only
          </Badge>
        }
      />

      <SectionCard title="Library details" description="Shown across the app and on receipts">
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="grid gap-2">
            <Label htmlFor="library-name">Library name</Label>
            <Input id="library-name" defaultValue="eLibrary" />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="library-email">Contact email</Label>
            <Input id="library-email" type="email" defaultValue="support@elibrary.test" />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="library-phone">Contact phone</Label>
            <Input id="library-phone" defaultValue="+91 00000 00000" />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="library-timezone">Time zone</Label>
            <Select defaultValue="ist">
              <SelectTrigger id="library-timezone">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="ist">Asia/Kolkata (IST)</SelectItem>
                <SelectItem value="utc">UTC</SelectItem>
                <SelectItem value="est">America/New_York</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </SectionCard>

      <SectionCard title="Borrowing rules" description="Loan limits and fine calculation">
        <div className="grid gap-4 sm:grid-cols-3">
          <div className="grid gap-2">
            <Label htmlFor="loan-days">Loan period (days)</Label>
            <Input id="loan-days" type="number" defaultValue={14} />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="max-books">Max books per member</Label>
            <Input id="max-books" type="number" defaultValue={5} />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="fine-rate">Fine per overdue day</Label>
            <Input id="fine-rate" type="number" defaultValue={10} />
          </div>
        </div>
        <div className="mt-4">
          <ToggleRow
            id="allow-renewals"
            label="Allow renewals"
            description="Members can extend a loan once if the book is not reserved."
            defaultChecked
          />
          <ToggleRow
            id="block-overdue"
            label="Block borrowing when overdue"
            description="Members with an overdue book cannot borrow new titles."
            defaultChecked
          />
        </div>
      </SectionCard>

      <SectionCard title="Users & roles" description="Who can sign in and what they can manage">
        <ToggleRow
          id="self-registration"
          label="Allow self registration"
          description="New members can create their own account from the sign-up page."
          defaultChecked
        />
        <ToggleRow
          id="require-approval"
          label="Require admin approval"
          description="New accounts stay inactive until an admin approves them."
        />
        <ToggleRow
          id="allow-role-change"
          label="Allow admins to change roles"
          description="Only Super admins can promote a member to Admin when this is off."
        />
      </SectionCard>

      <SectionCard title="Security" description="Account protection for staff logins">
        <ToggleRow
          id="two-factor"
          label="Two-factor authentication"
          description="Require a one-time code for Admin and Super admin sign-ins."
        />
        <ToggleRow
          id="force-password-reset"
          label="Password expiry"
          description="Ask staff accounts to change their password every 90 days."
        />
        <div className="mt-4 grid gap-2 sm:max-w-xs">
          <Label htmlFor="session-timeout">Session timeout (minutes)</Label>
          <Input id="session-timeout" type="number" defaultValue={60} />
        </div>
      </SectionCard>

      <SectionCard title="Notifications" description="Automatic emails sent to members">
        <ToggleRow
          id="due-reminder"
          label="Due-date reminder"
          description="Send a reminder two days before a book is due."
          defaultChecked
        />
        <ToggleRow
          id="overdue-notice"
          label="Overdue notice"
          description="Notify members when a loan becomes overdue."
          defaultChecked
        />
      </SectionCard>

      <div className="flex gap-2">
        <Button>
          <Save className="h-4 w-4" /> Save settings
        </Button>
        <Button variant="outline">Cancel</Button>
      </div>
    </AdminLayout>
  );
}
