import { useState } from "react";
import { BarChart3, Download, Table as TableIcon } from "lucide-react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  LabelList,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { SectionCard } from "./SectionCard";
import { Button } from "./ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "./ui/table";
import { exportToExcel } from "@/lib/export-excel";

export type ReportColumn<T> = {
  key: keyof T & string;
  label: string;
  /** Optional custom cell rendering (table view only). */
  render?: (row: T) => React.ReactNode;
};

/**
 * Report block with a Table / Chart switch and an Excel export button.
 * Data is passed in by the page — no backend calls happen here.
 */
export function ReportSection<T extends Record<string, string | number>>({
  title,
  description,
  rows,
  columns,
  chartLabelKey,
  chartValueKey,
  fileName,
  summary,
}: {
  title: string;
  description?: string | undefined;
  rows: T[];
  columns: ReportColumn<T>[];
  chartLabelKey: keyof T & string;
  chartValueKey: keyof T & string;
  fileName: string;
  summary?: Array<{ label: string; value: string }> | undefined;
}) {
  const [view, setView] = useState<"table" | "chart">("table");

  const chartData = rows.map((row) => ({
    name: String(row[chartLabelKey]),
    value: Number(row[chartValueKey]) || 0,
  }));

  function handleExport() {
    exportToExcel(
      rows.map((row) => {
        const record: Record<string, string | number> = {};
        for (const column of columns) record[column.label] = row[column.key] ?? "";
        return record;
      }),
      fileName,
      title,
    );
  }

  return (
    <SectionCard
      title={title}
      description={description}
      action={
        <div className="flex items-center gap-2">
          <div className="flex rounded-lg border border-border p-0.5">
            <Button
              variant={view === "table" ? "default" : "ghost"}
              size="sm"
              className="h-8"
              onClick={() => setView("table")}
            >
              <TableIcon className="h-4 w-4" /> Table
            </Button>
            <Button
              variant={view === "chart" ? "default" : "ghost"}
              size="sm"
              className="h-8"
              onClick={() => setView("chart")}
            >
              <BarChart3 className="h-4 w-4" /> Chart
            </Button>
          </div>
          <Button variant="outline" size="sm" className="h-8" onClick={handleExport}>
            <Download className="h-4 w-4" /> Export
          </Button>
        </div>
      }
    >
      {view === "table" ? (
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                {columns.map((column) => (
                  <TableHead key={column.key}>{column.label}</TableHead>
                ))}
              </TableRow>
            </TableHeader>
            <TableBody>
              {rows.map((row, index) => (
                <TableRow key={index}>
                  {columns.map((column) => (
                    <TableCell key={column.key}>
                      {column.render ? column.render(row) : row[column.key]}
                    </TableCell>
                  ))}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      ) : (
        <div className="h-72 w-full">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={chartData} margin={{ top: 20, right: 12, bottom: 40, left: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" vertical={false} />
              <XAxis
                dataKey="name"
                interval={0}
                angle={-25}
                textAnchor="end"
                height={60}
                tick={{ fontSize: 12 }}
                className="fill-muted-foreground"
              />
              <YAxis allowDecimals={false} tick={{ fontSize: 12 }} className="fill-muted-foreground" />
              <Tooltip
                cursor={{ fillOpacity: 0.08 }}
                contentStyle={{
                  borderRadius: "0.5rem",
                  border: "1px solid var(--border)",
                }}
              />
              <Bar dataKey="value" fill="#0f8b8d" radius={[4, 4, 0, 0]} maxBarSize={48}>
                <LabelList dataKey="value" position="top" className="fill-foreground text-xs" />
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      {summary ? (
        <div className="mt-4 grid gap-3 rounded-lg bg-muted/50 p-4 sm:grid-cols-3">
          {summary.map((item) => (
            <div key={item.label}>
              <p className="text-xs text-muted-foreground">{item.label}</p>
              <p className="text-sm font-semibold">{item.value}</p>
            </div>
          ))}
        </div>
      ) : null}
    </SectionCard>
  );
}
