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
  StudentError,
  StudentPageHeading,
  formatStudentDate,
} from "@/features/student/student-shared";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import type {
  PagedResult,
  StudentResult,
} from "@/types/student";

export function StudentResultsPage() {
  const {
    request,
  } = useAuth();

  const query =
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

  return (
    <>
      <StudentPageHeading
        eyebrow="Academic results"
        title="Results and feedback"
        description="Grades are shown together with the maximum marks and the Teacher feedback that gives them context."
      />

      {query.isLoading && (
        <p className="muted">
          Loading results…
        </p>
      )}

      {query.error && (
        <StudentError
          message={
            query.error instanceof Error
              ? query.error.message
              : "Unable to load results."
          }
        />
      )}

      {query.data &&
        query.data.items.length ===
          0 && (
          <EmptyState
            eyebrow="Results"
            title="No graded work yet"
            description="Once your Teacher grades a submission, the result and feedback will appear here."
          />
        )}

      <div className="student-results-list">
        {query.data?.items.map(
          (result) => (
            <Card
              key={
                result.submissionId
              }
              className="student-result-card"
            >
              <div>
                <p className="ledger-subject-code">
                  {
                    result.subjectCode
                  }{" "}
                  ·{" "}
                  {
                    result.subjectName
                  }
                </p>

                <h2>
                  {
                    result.assignmentTitle
                  }
                </h2>

                <p className="student-result-feedback">
                  {result.teacherFeedback ||
                    "No written feedback was added."}
                </p>

                <div className="student-work-meta">
                  <span>
                    Graded{" "}
                    {formatStudentDate(
                      result
                        .gradedAtUtc,
                    )}
                  </span>

                  <span>
                    Status:{" "}
                    {result.status}
                  </span>
                </div>

                <Link
                  href={`/student/assignments/${result.assignmentId}`}
                  style={{
                    display:
                      "inline-block",
                    marginTop: 16,
                  }}
                >
                  <Button
                    variant="secondary"
                    size="small"
                  >
                    Open assignment
                    <ArrowRight
                      size={15}
                    />
                  </Button>
                </Link>
              </div>

              <GradeSeal
                marks={
                  result.marksAwarded
                }
                maximumMarks={
                  result.maximumMarks
                }
              />
            </Card>
          ),
        )}
      </div>
    </>
  );
}