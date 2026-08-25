import { createFileRoute } from "@tanstack/react-router";
import { Repeat } from "lucide-react";
import { UserLayout } from "@/components/UserLayout";
import { SectionCard } from "@/components/SectionCard";
import { Button } from "@/components/ui/button";
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

export const Route = createFileRoute("/my-loans")({
  head: () => ({
    meta: [
      { title: "My Loans — eLibrary" },
      { name: "description", content: "Track your borrowed books, due dates, returns and fines." },
      { property: "og:title", content: "My Loans — eLibrary" },
      {
        property: "og:description",
        content: "Track your borrowed books, due dates, returns and fines.",
      },
    ],
  }),
  component: MyLoansPage,
});

function MyLoansPage() {
  const current = sampleLoans.filter((loan) => loan.status !== "Returned");
  const history = sampleLoans.filter((loan) => loan.status === "Returned");

  return (
    <UserLayout title="My loans" subtitle="Books you have borrowed">
      <SectionCard title="Current loans" description="Return within 7 days to avoid fines">
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Book</TableHead>
                <TableHead>Borrowed</TableHead>
                <TableHead>Due</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Action</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {current.map((loan) => (
                <TableRow key={loan.id}>
                  <TableCell className="font-medium">{loan.title}</TableCell>
                  <TableCell>{loan.borrowed}</TableCell>
                  <TableCell>{loan.due}</TableCell>
                  <TableCell>
                    {loan.status === "Overdue" ? (
                      <Badge variant="destructive">Overdue · fine {loan.fine}</Badge>
                    ) : (
                      <Badge variant="secondary">Active</Badge>
                    )}
                  </TableCell>
                  <TableCell className="text-right">
                    <Button size="sm" variant="outline">
                      <Repeat className="h-4 w-4" /> Return
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </SectionCard>

      <SectionCard title="History" description="Books you have already returned">
        <ul className="divide-y divide-border">
          {history.map((loan) => (
            <li key={loan.id} className="flex items-center justify-between gap-3 py-3 text-sm">
              <span className="truncate font-medium">{loan.title}</span>
              <span className="shrink-0 text-xs text-muted-foreground">
                Returned · borrowed {loan.borrowed}
              </span>
            </li>
          ))}
        </ul>
      </SectionCard>
    </UserLayout>
  );
}
