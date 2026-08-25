import { Link } from "@tanstack/react-router";
import {
  LayoutDashboard,
  BookOpen,
  Library,
  ArrowLeftRight,
  Users,
  BarChart3,
  BookMarked,
  Settings,
  type LucideIcon,
} from "lucide-react";

export type NavItem = {
  label: string;
  to: string;
  icon: LucideIcon;
};

export const adminNav: NavItem[] = [
  { label: "Dashboard", to: "/admin", icon: LayoutDashboard },
  { label: "Books", to: "/admin/books", icon: BookOpen },
  { label: "Borrow / Return", to: "/admin/borrow-return", icon: ArrowLeftRight },
  { label: "Users", to: "/admin/users", icon: Users },
  { label: "Reports", to: "/admin/reports", icon: BarChart3 },
  // Admin-only area: visible to Admin / Super admin roles.
  { label: "Settings", to: "/admin/settings", icon: Settings },
];

export const userNav: NavItem[] = [
  { label: "Dashboard", to: "/", icon: LayoutDashboard },
  { label: "Catalog", to: "/catalog", icon: Library },
  { label: "My Loans", to: "/my-loans", icon: BookMarked },
];


const linkClass =
  "flex items-center gap-3 rounded-lg px-3 py-2 text-sm text-sidebar-foreground transition-colors hover:bg-sidebar-accent";
const activeClass = "bg-primary font-medium text-primary-foreground hover:bg-primary";

/** Navigation links, shared by the desktop sidebar and the mobile drawer. */
export function SidebarNav({ items, onNavigate }: { items: NavItem[]; onNavigate?: () => void }) {
  return (
    <nav className="space-y-1">
      {items.map((item) => (
        <Link
          key={item.to}
          to={item.to}
          activeOptions={{ exact: item.to === "/" || item.to === "/admin" }}
          className={linkClass}
          activeProps={{ className: activeClass }}
          onClick={onNavigate}
        >
          <item.icon className="h-4 w-4 shrink-0" />
          <span className="truncate">{item.label}</span>
        </Link>
      ))}
    </nav>
  );
}

export function SidebarLogo() {
  return (
    <div className="flex items-center gap-2">
      <span className="grid h-9 w-9 shrink-0 place-items-center rounded-lg bg-primary text-base font-bold text-primary-foreground">
        e
      </span>
      <div className="min-w-0">
        <p className="truncate text-base font-semibold text-sidebar-foreground">eLibrary</p>
        <p className="truncate text-xs text-muted-foreground">Library management</p>
      </div>
    </div>
  );
}

export function Sidebar({ items }: { items: NavItem[] }) {
  return (
    <aside className="hidden w-64 shrink-0 flex-col border-r border-sidebar-border bg-sidebar md:flex">
      <div className="px-5 py-5">
        <SidebarLogo />
      </div>

      <div className="flex-1 px-3 pb-5">
        <SidebarNav items={items} />
      </div>
    </aside>

  );
}
