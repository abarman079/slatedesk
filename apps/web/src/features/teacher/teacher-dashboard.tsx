"use client";

import Link from "next/link";

import {
  Plus,
} from "lucide-react";

import {
  useQuery,
} from "@tanstack/react-query";

import {
  Button,
  Card,
  EmptyState,
} from "@/components/ui";

import {
  AssignmentLedgerCard,
  TeacherError,
  TeacherPageHeading,
} from "@/features/teacher/teacher-shared";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import type {
  PagedResult,
  TeacherAssignment,
} from "@/types/teacher";

export function TeacherDashboardView() {
  const {
    request,
  } = useAuth();

  const query =
    useQuery({
      queryKey: [
        "teacher-dashboard",
      ],

      queryFn: () =>
        request<
          PagedResult<
            TeacherAssignment
          >
        >(
          "/api/v1/teacher/assignments?page=1&pageSize=100",
        ),
    });

  if (query.isLoading) {
    return (
      <p className="muted">
        Loading teaching workspace…
      </p>
    );
  }

  if (query.error) {
    return (
      <TeacherError
        message={
          query.error instanceof Error
            ? query.error.message
            : "Unable to load Teacher dashboard."
        }
      />
    );
  }

  const assignments =
    query.data?.items ?? [];

  const draftCount =
    assignments.filter(
      (item) =>
        item.status === "Draft",
    ).length;

  const publishedCount =
    assignments.filter(
      (item) =>
        item.status ===
        "Published",
    ).length;

  const closedCount =
    assignments.filter(
      (item) =>
        item.status === "Closed",
    ).length;

  const submissionCount =
    assignments.reduce(
      (total, item) =>
        total +
        item.submissionCount,
      0,
    );

  const recent =
    assignments.slice(0, 4);

  return (
    <>
      <TeacherPageHeading
        eyebrow="Teaching workspace"
        title="Assignments with context, not clutter."
        description="Create academic work, publish it deliberately, and move submissions from review to useful feedback."
        action={
          <Link href="/teacher/assignments/new">
            <Button>
              <Plus size={17} />
              New assignment
            </Button>
          </Link>
        }
      />

      <section className="teacher-stat-grid">
        <Card className="teacher-stat-card">
          <span className="overview-label">
            Drafts
          </span>

          <strong>
            {draftCount}
          </strong>
        </Card>

        <Card className="teacher-stat-card">
          <span className="overview-label">
            Published
          </span>

          <strong>
            {publishedCount}
          </strong>
        </Card>

        <Card className="teacher-stat-card">
          <span className="overview-label">
            Closed
          </span>

          <strong>
            {closedCount}
          </strong>
        </Card>

        <Card className="teacher-stat-card">
          <span className="overview-label">
            Submissions
          </span>

          <strong>
            {submissionCount}
          </strong>
        </Card>
      </section>

      <section
        style={{
          marginTop: 32,
        }}
      >
        <div
          style={{
            display: "flex",
            justifyContent:
              "space-between",
            gap: 18,
            alignItems: "center",
            marginBottom: 16,
          }}
        >
          <div>
            <p className="eyebrow">
              Recent work
            </p>

            <h2
              style={{
                fontFamily:
                  "var(--font-serif)",
                fontSize: "1.7rem",
                fontWeight: 580,
                margin:
                  "7px 0 0",
              }}
            >
              Assignment ledger
            </h2>
          </div>

          <Link href="/teacher/assignments">
            <Button
              variant="secondary"
              size="small"
            >
              View all
            </Button>
          </Link>
        </div>

        {recent.length === 0 ? (
          <EmptyState
            eyebrow="Assignment ledger"
            title="No assignments yet"
            description="Create your first draft assignment to begin the teaching workflow."
            action={
              <Link href="/teacher/assignments/new">
                <Button>
                  Create assignment
                </Button>
              </Link>
            }
          />
        ) : (
          <div className="assignment-ledger-grid">
            {recent.map(
              (assignment) => (
                <AssignmentLedgerCard
                  key={
                    assignment.id
                  }
                  assignment={
                    assignment
                  }
                  actionHref={`/teacher/assignments/${assignment.id}`}
                />
              ),
            )}
          </div>
        )}
      </section>
    </>
  );
}