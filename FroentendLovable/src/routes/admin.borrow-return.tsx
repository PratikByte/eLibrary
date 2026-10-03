import { useState, type FormEvent } from "react";
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
import { borrowBook, returnBook } from "@/lib/api";

export const Route = createFileRoute("/admin/borrow-return")({
  head: () => ({
    meta: [
      { title: "eLibrary | Digital Library" },
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
  const [borrowError, setBorrowError] = useState<string | null>(null);
  const [borrowInfo, setBorrowInfo] = useState<string | null>(null);
  const [returnError, setReturnError] = useState<string | null>(null);
  const [returnInfo, setReturnInfo] = useState<string | null>(null);
  const [isBorrowing, setIsBorrowing] = useState(false);
  const [isReturning, setIsReturning] = useState(false);

  async function handleBorrow(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const formData = new FormData(form);
    setBorrowError(null);
    setBorrowInfo(null);
    setIsBorrowing(true);
    try {
      const record = await borrowBook(
        Number(formData.get("userId")),
        Number(formData.get("bookId")),
      );
      setBorrowInfo(
        `Issued "${record.bookName ?? "book"}" to ${record.userName ?? "member"}. Due ${record.dueDate}.`,
      );
      form.reset();
    } catch (requestError) {
      setBorrowError(requestError instanceof Error ? requestError.message : "Unable to issue this book.");
    } finally {
      setIsBorrowing(false);
    }
  }

  async function handleReturn(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const formData = new FormData(form);
    setReturnError(null);
    setReturnInfo(null);
    setIsReturning(true);
    try {
      const record = await returnBook(Number(formData.get("borrowId")));
      const fineText =
        record.fine && record.fine > 0 ? ` Fine ${record.fine}.` : "";
      setReturnInfo(`Returned "${record.bookName ?? "book"}".${fineText}`);
      form.reset();
    } catch (requestError) {
      setReturnError(requestError instanceof Error ? requestError.message : "Unable to record this return.");
    } finally {
      setIsReturning(false);
    }
  }

  return (
    <AdminLayout title="Borrow / Return" subtitle="Issue and receive books">
      <div className="grid gap-5 lg:grid-cols-2">
        <SectionCard title="Issue a book" description="Loans run for 7 days from the borrow date">
          <form className="grid gap-4" onSubmit={handleBorrow}>
            <div className="grid gap-2">
              <Label htmlFor="borrow-member">Member ID</Label>
              <Input id="borrow-member" name="userId" placeholder="e.g. 1024" required />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="borrow-book">Book ID</Label>
              <Input id="borrow-book" name="bookId" placeholder="e.g. 58" required />
            </div>
            {borrowError ? <p className="text-sm text-destructive">{borrowError}</p> : null}
            {borrowInfo ? <p className="text-sm text-muted-foreground">{borrowInfo}</p> : null}
            <Button className="w-full" type="submit" disabled={isBorrowing}>
              <ArrowLeftRight className="h-4 w-4" /> {isBorrowing ? "Issuing..." : "Issue book"}
            </Button>
          </form>
        </SectionCard>

        <SectionCard title="Return a book" description="Late returns are fined per overdue day">
          <form className="grid gap-4" onSubmit={handleReturn}>
            <div className="grid gap-2">
              <Label htmlFor="return-loan">Loan ID</Label>
              <Input id="return-loan" name="borrowId" placeholder="e.g. 341" required />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="return-date">Return date</Label>
              <Input id="return-date" type="date" disabled />
            </div>
            {returnError ? <p className="text-sm text-destructive">{returnError}</p> : null}
            {returnInfo ? <p className="text-sm text-muted-foreground">{returnInfo}</p> : null}
            <Button variant="outline" className="w-full" type="submit" disabled={isReturning}>
              <Repeat className="h-4 w-4" /> {isReturning ? "Recording..." : "Record return"}
            </Button>
          </form>
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
