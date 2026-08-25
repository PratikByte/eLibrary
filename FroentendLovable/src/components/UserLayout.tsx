import type { ReactNode } from "react";
import { useEffect, useState } from "react";
import { Sidebar, userNav } from "./Sidebar";
import { Header } from "./Header";
import { getAuthSession, type AuthSession } from "@/lib/api";

export function UserLayout({
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

  useEffect(() => {
    setSession(getAuthSession());
  }, []);

  return (
    <div className="flex min-h-screen w-full bg-background">
      <Sidebar items={userNav} />
      <div className="flex min-w-0 flex-1 flex-col">
        <Header
          title={title}
          subtitle={subtitle}
          navItems={userNav}
          searchPlaceholder={searchPlaceholder}
          userName={session?.username}
          userRole={session?.role}
        />
        <main className="flex-1 space-y-5 p-4 sm:p-6">{children}</main>
      </div>
    </div>
  );
}
