"use client";

import {
  useEffect,
  useState,
  type ReactNode,
} from "react";

import Link from "next/link";

import {
  usePathname,
  useRouter,
} from "next/navigation";

import {
  BadgeCheck,
  BookOpen,
  ClipboardList,
  GraduationCap,
  Inbox,
  LayoutDashboard,
  Link2,
  LogOut,
  Menu,
  School,
  Send,
  Users,
  type LucideIcon,
} from "lucide-react";

import {
  Button,
} from "@/components/ui";

import {
  Drawer,
} from "@/components/ui/overlay";

import {
  ThemeSwitcher,
} from "@/components/theme-switcher";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import type {
  AppRole,
} from "@/types/auth";

type NavItem = {
  href: string;
  label: string;
  icon: LucideIcon;
};

const navigation:
  Record<AppRole, NavItem[]> = {
  Admin: [
    {
      href: "/admin",
      label: "Overview",
      icon: LayoutDashboard,
    },
    {
      href: "/admin/users",
      label: "People",
      icon: Users,
    },
    {
      href: "/admin/classes",
      label: "Classes",
      icon: School,
    },
    {
      href: "/admin/subjects",
      label: "Subjects",
      icon: BookOpen,
    },
    {
      href: "/admin/allocations",
      label: "Allocations",
      icon: Link2,
    },
    {
      href: "/admin/enrollments",
      label: "Enrollments",
      icon: GraduationCap,
    },
  ],

  Teacher: [
    {
      href: "/teacher",
      label: "Overview",
      icon: LayoutDashboard,
    },
    {
      href: "/teacher/assignments",
      label: "Assignments",
      icon: ClipboardList,
    },
    {
      href: "/teacher/submissions",
      label: "Review queue",
      icon: Inbox,
    },
  ],

  Student: [
    {
      href: "/student",
      label: "Overview",
      icon: LayoutDashboard,
    },
    {
      href: "/student/assignments",
      label: "Assignments",
      icon: ClipboardList,
    },
    {
      href: "/student/submissions",
      label: "My work",
      icon: Send,
    },
    {
      href: "/student/results",
      label: "Results",
      icon: BadgeCheck,
    },
  ],
};

function pathMatches(
  pathname: string,
  href: string,
) {
  if (pathname === href) {
    return true;
  }

  if (
    href.split("/").length > 2
  ) {
    return pathname.startsWith(
      `${href}/`,
    );
  }

  return false;
}

function Brand() {
  return (
    <Link
      href="/"
      className="brand-lockup"
      aria-label="SlateDesk home"
    >
      <span
        className="brand-mark"
        aria-hidden="true"
      />

      <span className="brand-word">
        SlateDesk
      </span>
    </Link>
  );
}

function Navigation({
  role,
  pathname,
  onNavigate,
}: {
  role: AppRole;
  pathname: string;
  onNavigate?: () => void;
}) {
  return (
    <>
      <p className="sidebar-role">
        {role} workspace
      </p>

      <nav
        className="primary-nav"
        aria-label="Primary navigation"
      >
        {navigation[role].map(
          (
            item,
            index,
          ) => {
            const active =
              pathMatches(
                pathname,
                item.href,
              );

            const Icon =
              item.icon;

            return (
              <Link
                key={item.href}
                href={item.href}
                className={`nav-link ${
                  active
                    ? "active"
                    : ""
                }`}
                aria-current={
                  active
                    ? "page"
                    : undefined
                }
                onClick={
                  onNavigate
                }
              >
                <Icon
                  size={18}
                  strokeWidth={1.8}
                />

                <span>
                  {item.label}
                </span>

                <span className="nav-index">
                  {String(
                    index + 1,
                  ).padStart(
                    2,
                    "0",
                  )}
                </span>
              </Link>
            );
          },
        )}
      </nav>
    </>
  );
}

export function AppShell({
  role,
  children,
  title,
  subtitle,
}: {
  role: AppRole;
  children: ReactNode;
  title: string;
  subtitle: string;
}) {
  const pathname =
    usePathname();

  const router =
    useRouter();

  const {
    user,
    isLoading,
    logout,
  } = useAuth();

  const [
    mobileOpen,
    setMobileOpen,
  ] = useState(false);

  useEffect(() => {
    if (isLoading) {
      return;
    }

    if (!user) {
      router.replace("/login");
      return;
    }

    if (
      !user.roles.includes(role)
    ) {
      router.replace(
        "/unauthorized",
      );
    }
  }, [
    user,
    isLoading,
    role,
    router,
  ]);

  async function handleLogout() {
    await logout();

    router.replace("/login");
  }

  if (
    isLoading ||
    !user ||
    !user.roles.includes(role)
  ) {
    return (
      <main
        id="main-content"
        className="app-content"
      >
        <p className="muted">
          Preparing your workspace…
        </p>
      </main>
    );
  }

  const initials =
    user.fullName
      .split(/\s+/)
      .slice(0, 2)
      .map((part) =>
        part.charAt(0),
      )
      .join("")
      .toUpperCase();

  return (
    <div className="app-shell">
      <aside className="app-sidebar">
        <div className="sidebar-head">
          <Brand />
        </div>

        <Navigation
          role={role}
          pathname={pathname}
        />

        <div className="sidebar-footer">
          <div className="user-chip">
            <span className="user-avatar">
              {initials}
            </span>

            <div>
              <div className="user-name">
                {user.fullName}
              </div>

              <div className="user-email">
                {user.email}
              </div>
            </div>
          </div>

          <Button
            variant="ghost"
            className="sidebar-logout"
            onClick={
              handleLogout
            }
          >
            <LogOut size={17} />

            Sign out
          </Button>
        </div>
      </aside>

      <div className="app-main-column">
        <header className="app-topbar">
          <button
            type="button"
            className="icon-button mobile-menu-trigger"
            aria-label="Open navigation"
            title="Open navigation"
            onClick={() =>
              setMobileOpen(true)
            }
          >
            <Menu size={19} />
          </button>

          <div className="topbar-heading">
            <strong>
              {title}
            </strong>

            <span>
              {subtitle}
            </span>
          </div>

          <div className="topbar-actions">
            <ThemeSwitcher />
          </div>
        </header>

        <main
          id="main-content"
          className="app-content"
        >
          {children}
        </main>
      </div>

      <Drawer
        open={mobileOpen}
        onClose={() =>
          setMobileOpen(false)
        }
        title="Navigation"
      >
        <Brand />

        <Navigation
          role={role}
          pathname={pathname}
          onNavigate={() =>
            setMobileOpen(false)
          }
        />

        <div
          style={{
            marginTop: 24,
          }}
        >
          <Button
            variant="secondary"
            style={{
              width: "100%",
            }}
            onClick={
              handleLogout
            }
          >
            <LogOut size={17} />
            Sign out
          </Button>
        </div>
      </Drawer>
    </div>
  );
}