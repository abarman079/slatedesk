import Link from "next/link";

import {
  ArrowRight,
  ShieldCheck,
} from "lucide-react";

import {
  Badge,
  Button,
  Card,
} from "@/components/ui";

import type {
  AppRole,
} from "@/types/auth";

const copy: Record<
  AppRole,
  {
    eyebrow: string;
    heading: string;
    description: string;
    action: string;
    actionHref: string;
    roleNote: string;
  }
> = {
  Admin: {
    eyebrow: "Institution control",
    heading:
      "Build the academic structure with confidence.",
    description:
      "Organize people, classes, subjects, allocations, and enrollments from one deliberate workspace.",
    action: "Manage people",
    actionHref: "/admin/users",
    roleNote:
      "Administrative actions are protected by backend-enforced role policies.",
  },

  Teacher: {
    eyebrow: "Teaching workspace",
    heading:
      "Assignments move from idea to feedback in one clear flow.",
    description:
      "Prepare work, publish at the right moment, review submissions, and return useful feedback.",
    action: "View assignments",
    actionHref:
      "/teacher/assignments",
    roleNote:
      "Only allocations owned by your account can be used for assignment workflows.",
  },

  Student: {
    eyebrow: "Student workspace",
    heading:
      "Know what is due, what is submitted, and what comes next.",
    description:
      "Keep assignments, drafts, submissions, deadlines, and feedback visible without administrative clutter.",
    action: "View assignments",
    actionHref:
      "/student/assignments",
    roleNote:
      "Only assignments belonging to your active class are available here.",
  },
};

export function RoleDashboard({
  role,
}: {
  role: AppRole;
}) {
  const content =
    copy[role];

  return (
    <>
      <header className="page-heading">
        <div>
          <p className="eyebrow">
            {content.eyebrow}
          </p>

          <h1>
            {content.heading}
          </h1>

          <p>
            {
              content.description
            }
          </p>
        </div>

        <Badge tone="green">
          Connected
        </Badge>
      </header>

      <section
        className="overview-grid"
        aria-label="Workspace status"
      >
        <Card className="overview-card">
          <div className="overview-label">
            Account role
          </div>

          <div className="overview-value">
            {role}
          </div>

          <span className="muted">
            Verified session
          </span>
        </Card>

        <Card className="overview-card">
          <div className="overview-label">
            API session
          </div>

          <div className="overview-value">
            Live
          </div>

          <span className="muted">
            Secure refresh enabled
          </span>
        </Card>

        <Card className="overview-card">
          <div className="overview-label">
            Workspace
          </div>

          <div className="overview-value">
            Ready
          </div>

          <span className="muted">
            Role-aware navigation
          </span>
        </Card>
      </section>

      <section className="workspace-panel">
        <Card className="ledger-preview">
          <div className="ledger-preview-head">
            <div>
              <p className="eyebrow">
                Deadline rail
              </p>

              <h2
                style={{
                  margin:
                    "8px 0 0",
                  fontFamily:
                    "var(--font-serif)",
                  fontSize:
                    "1.65rem",
                  fontWeight: 580,
                }}
              >
                Academic work,
                visibly organized.
              </h2>
            </div>

            <Badge tone="blue">
              Preview
            </Badge>
          </div>

          <div
            className="deadline-rail"
            aria-hidden="true"
          >
            <div className="deadline-rail-fill" />
            <div className="deadline-rail-dot" />
          </div>

          <div className="deadline-meta">
            <span>Published</span>
            <span>Today</span>
            <span>Deadline</span>
          </div>
        </Card>

        <Card className="role-note">
          <ShieldCheck
            size={24}
            color="var(--cobalt)"
          />

          <h2>
            Purposeful access
          </h2>

          <p>
            {content.roleNote}
          </p>

          <Link
            href={
              content.actionHref
            }
            style={{
              marginTop: 24,
            }}
          >
            <Button
              style={{
                width: "100%",
              }}
            >
              {content.action}

              <ArrowRight
                size={17}
              />
            </Button>
          </Link>
        </Card>
      </section>
    </>
  );
}