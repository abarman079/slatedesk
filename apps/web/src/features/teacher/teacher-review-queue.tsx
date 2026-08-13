"use client";

import {
  useQuery,
} from "@tanstack/react-query";

import {
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

export function TeacherReviewQueue() {
  const {
    request,
  } = useAuth();

  const query =
    useQuery({
      queryKey: [
        "teacher-review-queue",
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
        Loading review queue…
      </p>
    );
  }

  if (query.error) {
    return (
      <TeacherError
        message={
          query.error instanceof Error
            ? query.error.message
            : "Unable to load review queue."
        }
      />
    );
  }

  const assignments =
    (query.data?.items ?? [])
      .filter(
        (assignment) =>
          assignment
            .submissionCount > 0,
      );

  return (
    <>
      <TeacherPageHeading
        eyebrow="Review queue"
        title="Work waiting for your attention."
        description="Open an assignment to review Student answers, move work through review states, and return marks with useful feedback."
      />

      {assignments.length === 0 ? (
        <EmptyState
          eyebrow="Review queue"
          title="Nothing to review yet"
          description="Assignments with Student submissions will appear here automatically."
        />
      ) : (
        <div className="assignment-ledger-grid">
          {assignments.map(
            (assignment) => (
              <AssignmentLedgerCard
                key={
                  assignment.id
                }
                assignment={
                  assignment
                }
                actionLabel="Open review stack"
                actionHref={`/teacher/assignments/${assignment.id}/submissions`}
              />
            ),
          )}
        </div>
      )}
    </>
  );
}