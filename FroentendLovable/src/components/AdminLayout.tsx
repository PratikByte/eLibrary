import type { ReactNode } from "react";
import { useEffect, useState } from "react";
import { Link } from "@tanstack/react-router";
import { Sidebar, adminNav } from "./Sidebar";
import { Header } from "./Header";
import { getAuthSession, isAdminSession, type AuthSession } from "@/lib/api";

export function AdminLayout({
  title,
  subtitle,
  searchPlaceholder,
  children,
}: {
  title: string;
  subtitle?: string | undefined;
  searchPlaceholder?: string | undefined;
  children: ReactNode;
}) {
  const [session, setSession] = useState<AuthSession | null>(null);
  const [isCheckingRole, setIsCheckingRole] = useState(true);

  useEffect(() => {
    setSession(getAuthSession());
    setIsCheckingRole(false);
  }, []);

  if (isCheckingRole) {
    return <p className="p-6 text-sm text-muted-foreground">Checking permissions...</p>;
  }

  if (!isAdminSession(session)) {
    return (
      <main className="space-y-3 p-6">
        <h1 className="text-lg font-semibold">Admin access required</h1>
        <p className="text-sm text-muted-foreground">Your account does not have the Admin role.</p>
        <Link to="/" className="text-sm text-primary hover:underline">
          Return to dashboard
        </Link>
      </main>
    );
  }

  return (
    <div className="flex min-h-screen w-full bg-background">
      <Sidebar items={adminNav} />
      <div className="flex min-w-0 flex-1 flex-col">
        <Header
          title={title}
          subtitle={subtitle}
          navItems={adminNav}
          searchPlaceholder={searchPlaceholder}
          userName={session?.username}
          userRole={session?.role}
          profileTo="/admin/profile"
          settingsTo="/admin/settings"
        />

        <main className="flex-1 space-y-5 p-4 sm:p-6">{children}</main>
      </div>
    </div>
  );
}
