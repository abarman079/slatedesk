"use client";

import Link from "next/link";

import {
  ArrowRight,
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
  GradeSeal,
  StudentAssignmentFolio,
  StudentError,
  StudentPageHeading,
} from "@/features/student/student-shared";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import type {
  PagedResult,
  StudentAssignment,
  StudentResult,
  StudentSubmission,
} from "@/types/student";

export function StudentDashboardView() {
  const {
    request,
  } = useAuth();

  const assignments =
    useQuery({
      queryKey: [
        "student-assignments",
        "dashboard",
      ],

      queryFn: () =>
        request<
          PagedResult<
            StudentAssignment
          >
        >(
          "/api/v1/student/assignments?page=1&pageSize=100",
        ),
    });

  const submissions =
    useQuery({
      queryKey: [
        "student-submissions",
      ],

      queryFn: () =>
        request<
          PagedResult<
            StudentSubmission
          >
        >(
          "/api/v1/student/submissions?page=1&pageSize=100",
        ),
    });

  const results =
    useQuery({
      queryKey: [
        "student-results",
      ],

      queryFn: () =>
        request<
          PagedResult<
            StudentResult
          >
        >(
          "/api/v1/student/results?page=1&pageSize=100",
        ),
    });

  const error =
    assignments.error ??
    submissions.error ??
    results.error;

  if (
    assignments.isLoading ||
    submissions.isLoading ||
    results.isLoading
  ) {
    return (
      <p className="muted">
        Loading your academic workspace…
      </p>
    );
  }

  if (error) {
    return (
      <StudentError
        message={
          error instanceof Error
            ? error.message
            : "Unable to load Student dashboard."
        }
      />
    );
  }

  const assignmentItems =
    assignments.data?.items ?? [];

  const submissionItems =
    submissions.data?.items ?? [];

  const resultItems =
    results.data?.items ?? [];

  const submittedIds =
    new Set(
      submissionItems
        .filter(
          (item) =>
            item.status !==
            "Draft",
        )
        .map(
          (item) =>
            item.assignmentId,
        ),
    );

  const dueSoon =
    assignmentItems
      .filter(
        (item) =>
          !item.isPastDeadline &&
          item.submissionStatus !==
            "Graded",
      )
      .sort(
        (a, b) =>
          new Date(
            a.deadlineUtc,
          ).getTime() -
          new Date(
            b.deadlineUtc,
          ).getTime(),
      );

  const pastDeadline =
    assignmentItems.filter(
      (item) =>
        item.isPastDeadline &&
        item.submissionStatus !==
          "Graded",
    ).length;

  const progress =
    assignmentItems.length ===
    0
      ? 0
      : Math.round(
          (submittedIds.size /
            assignmentItems.length) *
            100,
        );

  const latestResult =
    resultItems[0] ?? null;

  return (
    <>
      <StudentPageHeading
        eyebrow="Student workspace"
        title="Know what is due and what comes next."
        description="Assignments, saved work, submission status, and Teacher feedback stay together without administrative clutter."
        action={
          <Link href="/student/assignments">
            <Button>
              View assignments
              <ArrowRight
                size={17}
              />
            </Button>
          </Link>
        }
      />

      <section className="student-stat-grid">
        <Card className="student-stat">
          <span className="overview-label">
            Due soon
          </span>

          <strong>
            {dueSoon.length}
          </strong>
        </Card>

        <Card className="student-stat">
          <span className="overview-label">
            Past deadline
          </span>

          <strong>
            {pastDeadline}
          </strong>
        </Card>

        <Card className="student-stat">
          <span className="overview-label">
            Submitted
          </span>

          <strong>
            {submittedIds.size}
          </strong>
        </Card>

        <Card className="student-stat">
          <span className="overview-label">
            Graded
          </span>

          <strong>
            {resultItems.length}
          </strong>
        </Card>
      </section>

      <section className="student-dashboard-grid">
        <Card className="student-progress-card">
          <p className="eyebrow">
            Progress overview
          </p>

          <div className="student-progress-value">
            {progress}%
          </div>

          <p className="muted">
            of currently visible
            assignments have a submitted
            response.
          </p>

          <div
            className="student-progress-track"
            aria-label={`${progress}% submission progress`}
          >
            <div
              className="student-progress-fill"
              style={{
                width: `${progress}%`,
              }}
            />
          </div>

          <p className="student-progress-note">
            Progress reflects submitted
            academic work, not a final
            course grade.
          </p>
        </Card>

        <Card>
          <p className="eyebrow">
            Latest result
          </p>

          {latestResult ? (
            <>
              <div
                style={{
                  marginTop: 18,
                }}
              >
                <GradeSeal
                  marks={
                    latestResult
                      .marksAwarded
                  }
                  maximumMarks={
                    latestResult
                      .maximumMarks
                  }
                />
              </div>

              <p
                style={{
                  margin:
                    "16px 0 0",
                  fontWeight: 740,
                }}
              >
                {
                  latestResult
                    .assignmentTitle
                }
              </p>
            </>
          ) : (
            <p
              className="muted"
              style={{
                marginTop: 16,
              }}
            >
              Graded work will appear
              here.
            </p>
          )}
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
            alignItems: "center",
            gap: 16,
            marginBottom: 16,
          }}
        >
          <div>
            <p className="eyebrow">
              Next in your ledger
            </p>

            <h2
              style={{
                margin:
                  "7px 0 0",
                fontFamily:
                  "var(--font-serif)",
                fontSize:
                  "1.75rem",
                fontWeight: 580,
              }}
            >
              Upcoming assignments
            </h2>
          </div>

          <Link href="/student/assignments">
            <Button
              variant="secondary"
              size="small"
            >
              View all
            </Button>
          </Link>
        </div>

        {dueSoon.length === 0 ? (
          <EmptyState
            eyebrow="Assignment ledger"
            title="Nothing urgent right now"
            description="Published assignments for your active class will appear here."
          />
        ) : (
          <div className="student-folio-grid">
            {dueSoon
              .slice(0, 4)
              .map(
                (assignment) => (
                  <StudentAssignmentFolio
                    key={
                      assignment.id
                    }
                    assignment={
                      assignment
                    }
                  />
                ),
              )}
          </div>
        )}
      </section>
    </>
  );
}