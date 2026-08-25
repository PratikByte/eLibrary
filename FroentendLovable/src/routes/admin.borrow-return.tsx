import { createFileRoute } from "@tanstack/react-router";
import { ArrowLeftRight, Repeat } from "lucide-react";
import { AdminLayout } from "@/components/AdminLayout";
import { SectionCard } from "@/components/SectionCard";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { sampleLoans } from "@/lib/placeholder-data";

export const Route = createFileRoute("/admin/borrow-return")({
  head: () => ({
    meta: [
      { title: "Borrow & Return — eLibrary Admin" },
      { name: "description", content: "Issue books to members and record returns with due dates and fines." },
      { property: "og:title", content: "Borrow & Return — eLibrary Admin" },
      {
        property: "og:description",
        content: "Issue books to members and record returns with due dates and fines.",
      },
    ],
  }),
  component: AdminBorrowReturnPage,
});

function AdminBorrowReturnPage() {
  return (
    <AdminLayout title="Borrow / Return" subtitle="Issue and receive books">
      <div className="grid gap-5 lg:grid-cols-2">
        <SectionCard title="Issue a book" description="Loans run for 7 days from the borrow date">
          <div className="grid gap-4">
            <div className="grid gap-2">
              <Label htmlFor="borrow-member">Member ID</Label>
              <Input id="borrow-member" placeholder="e.g. 1024" />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="borrow-book">Book ID</Label>
              <Input id="borrow-book" placeholder="e.g. 58" />
            </div>
            <Button className="w-full">
              <ArrowLeftRight className="h-4 w-4" /> Issue book
            </Button>
          </div>
        </SectionCard>

        <SectionCard title="Return a book" description="Late returns are fined per overdue day">
          <div className="grid gap-4">
            <div className="grid gap-2">
              <Label htmlFor="return-loan">Loan ID</Label>
              <Input id="return-loan" placeholder="e.g. 341" />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="return-date">Return date</Label>
              <Input id="return-date" type="date" />
            </div>
            <Button variant="outline" className="w-full">
              <Repeat className="h-4 w-4" /> Record return
            </Button>
          </div>
        </SectionCard>
      </div>

      <SectionCard title="Recent transactions" description="Latest borrow and return records">
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Book</TableHead>
                <TableHead>Member</TableHead>
                <TableHead>Borrowed</TableHead>
                <TableHead>Due</TableHead>
                <TableHead>Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {sampleLoans.map((loan) => (
                <TableRow key={loan.id}>
                  <TableCell className="font-medium">{loan.title}</TableCell>
                  <TableCell className="text-muted-foreground">{loan.member}</TableCell>
                  <TableCell>{loan.borrowed}</TableCell>
                  <TableCell>{loan.due}</TableCell>
                  <TableCell>
                    <LoanStatus status={loan.status} />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </SectionCard>
    </AdminLayout>
  );
}

function LoanStatus({ status }: { status: "Active" | "Overdue" | "Returned" }) {
  if (status === "Overdue") return <Badge variant="destructive">Overdue</Badge>;
  if (status === "Returned") return <Badge variant="outline">Returned</Badge>;
  return <Badge variant="secondary">Active</Badge>;
}
