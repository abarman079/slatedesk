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
  StudentError,
  StudentPageHeading,
  StudentSubmissionBadge,
  formatStudentDate,
} from "@/features/student/student-shared";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import type {
  PagedResult,
  StudentSubmission,
} from "@/types/student";

export function StudentSubmissionHistory() {
  const {
    request,
  } = useAuth();

  const query =
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

  return (
    <>
      <StudentPageHeading
        eyebrow="My work"
        title="Submission history"
        description="See what is still a draft, what has been submitted, and what is currently under review."
      />

      {query.isLoading && (
        <p className="muted">
          Loading your work…
        </p>
      )}

      {query.error && (
        <StudentError
          message={
            query.error instanceof Error
              ? query.error.message
              : "Unable to load submission history."
          }
        />
      )}

      {query.data &&
        query.data.items.length ===
          0 && (
          <EmptyState
            eyebrow="My work"
            title="No saved work yet"
            description="Save a draft or submit an assignment and it will appear here."
          />
        )}

      <div className="student-work-list">
        {query.data?.items.map(
          (submission) => (
            <Card
              key={
                submission.id
              }
              className="student-work-card"
            >
              <div>
                <div className="ledger-card-top">
                  <span className="ledger-subject-code">
                    {
                      submission
                        .subjectCode
                    }
                  </span>

                  <StudentSubmissionBadge
                    status={
                      submission.status
                    }
                  />
                </div>

                <h2>
                  {
                    submission
                      .assignmentTitle
                  }
                </h2>

                <div className="student-work-meta">
                  <span>
                    Updated{" "}
                    {formatStudentDate(
                      submission
                        .updatedAtUtc,
                    )}
                  </span>

                  <span>
                    Submitted{" "}
                    {formatStudentDate(
                      submission
                        .submittedAtUtc,
                    )}
                  </span>

                  <span>
                    {
                      submission
                        .maximumMarks
                    }{" "}
                    marks
                  </span>
                </div>
              </div>

              <Link
                href={`/student/assignments/${submission.assignmentId}`}
              >
                <Button
                  variant="secondary"
                  size="small"
                >
                  Open work
                  <ArrowRight
                    size={15}
                  />
                </Button>
              </Link>
            </Card>
          ),
        )}
      </div>
    </>
  );
}