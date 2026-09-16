import { Link } from "@tanstack/react-router";
import { Menu, Search, Bell, LogOut, UserCircle, Settings, KeyRound } from "lucide-react";
import { Sheet, SheetContent, SheetTrigger, SheetTitle } from "@/components/ui/sheet";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { SidebarNav, SidebarLogo, type NavItem } from "./Sidebar";
import { clearAuthSession } from "@/lib/api";
import { API_BASE_URL } from "@/lib/api-config";

type HeaderProps = {
  title: string;
  subtitle?: string | undefined;
  navItems: NavItem[];
  /** Show the search box only on pages where searching makes sense. */
  searchPlaceholder?: string | undefined;
  /** Signed-in user name. When missing, a Sign in button is shown instead. */
  userName?: string | undefined;
  userRole?: string | undefined;
  /** Where the "View profile" menu item points. */
  profileTo?: string | undefined;
  /** Admin / super admin only: show the Settings entry in the profile menu. */
  settingsTo?: string | undefined;
};

export function Header({
  title,
  subtitle,
  navItems,
  searchPlaceholder,
  userName,
  userRole,
  profileTo = "/profile",
  settingsTo,
}: HeaderProps) {
  const initials = userName
    ? userName
        .split(" ")
        .map((part) => part[0])
        .join("")
        .slice(0, 2)
        .toUpperCase()
    : "";

  return (
    <header className="sticky top-0 z-10 flex items-center gap-3 border-b border-border bg-card px-4 py-3 sm:px-6">
      {/* Mobile-only navigation drawer */}
      <Sheet>
        <SheetTrigger asChild className="md:hidden">
          <Button variant="ghost" size="icon" aria-label="Open menu">
            <Menu className="h-5 w-5" />
          </Button>
        </SheetTrigger>
        <SheetContent side="left" className="flex w-64 flex-col p-0">
          <SheetTitle className="px-5 py-5">
            <SidebarLogo />
          </SheetTitle>
          <div className="flex-1 px-3">
            <SidebarNav items={navItems} />
          </div>
        </SheetContent>
      </Sheet>

      <div className="min-w-0 flex-1">
        <h1 className="truncate text-lg font-semibold">{title}</h1>
        {subtitle ? <p className="truncate text-sm text-muted-foreground">{subtitle}</p> : null}
      </div>

      {searchPlaceholder ? (
        <div className="relative hidden lg:block lg:w-72">
          <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input className="pl-9" placeholder={searchPlaceholder} aria-label={searchPlaceholder} />
        </div>
      ) : null}

      {userName ? (
        <div className="flex shrink-0 items-center gap-2">
          <Button variant="ghost" size="icon" aria-label="Notifications" className="hidden sm:flex">
            <Bell className="h-5 w-5" />
          </Button>

          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <button
                type="button"
                className="flex items-center gap-2 rounded-lg px-1 py-1 transition-colors hover:bg-muted"
                aria-label="Account menu"
              >
                <Avatar className="h-9 w-9">
                  <AvatarImage
                    src={`${API_BASE_URL}/profile-images/DefaultUser.svg`}
                    alt={`${userName} profile`}
                  />
                  <AvatarFallback className="bg-accent text-accent-foreground">
                    {initials}
                  </AvatarFallback>
                </Avatar>
                <div className="hidden text-left text-sm leading-tight sm:block">
                  <p className="font-medium">{userName}</p>
                  <p className="text-xs text-muted-foreground">{userRole}</p>
                </div>
              </button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-52">
              <DropdownMenuLabel className="font-normal">
                <p className="text-sm font-medium">{userName}</p>
                <p className="text-xs text-muted-foreground">{userRole}</p>
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem asChild>
                <Link to={profileTo}>
                  <UserCircle className="h-4 w-4" /> View profile
                </Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link to="/forgot-password">
                  <KeyRound className="h-4 w-4" /> Change password
                </Link>
              </DropdownMenuItem>
              {settingsTo ? (
                <DropdownMenuItem asChild>
                  <Link to={settingsTo}>
                    <Settings className="h-4 w-4" /> Settings
                  </Link>
                </DropdownMenuItem>
              ) : null}
              <DropdownMenuSeparator />
              <DropdownMenuItem
                className="text-destructive focus:text-destructive"
                onClick={() => {
                  clearAuthSession();
                  window.location.assign("/login");
                }}
              >
                <LogOut className="h-4 w-4" /> Logout
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ) : (
        <Button asChild size="sm" className="shrink-0">
          <Link to="/login">Sign in</Link>
        </Button>
      )}
    </header>
  );
}
