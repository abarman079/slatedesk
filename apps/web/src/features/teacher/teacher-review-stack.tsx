"use client";

import {
  useState,
} from "react";

import {
  RefreshCw,
} from "lucide-react";

import {
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";

import {
  Button,
  EmptyState,
  Input,
  Textarea,
} from "@/components/ui";

import {
  SubmissionStatusBadge,
  TeacherError,
  TeacherPageHeading,
  formatTeacherDate,
} from "@/features/teacher/teacher-shared";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import {
  ApiError,
} from "@/lib/api-client";

import type {
  PagedResult,
  TeacherSubmission,
} from "@/types/teacher";

function initials(
  value: string,
) {
  return value
    .split(/\s+/)
    .slice(0, 2)
    .map((part) =>
      part.charAt(0),
    )
    .join("")
    .toUpperCase();
}

function ReviewPanel({
  submission,
  statusPending,
  gradePending,
  onStatus,
  onGrade,
}: {
  submission: TeacherSubmission;

  statusPending: boolean;
  gradePending: boolean;

  onStatus: (
    status:
      | "UnderReview"
      | "NeedsRevision",
    version: number,
  ) => void;

  onGrade: (
    marks: number,
    feedback: string,
    version: number,
  ) => void;
}) {
  const [marks, setMarks] =
    useState(
      submission.marksAwarded
        ?.toString() ?? "",
    );

  const [
    feedback,
    setFeedback,
  ] = useState(
    submission
      .teacherFeedback ?? "",
  );

  const numericMarks =
    Number(marks);

  const invalidMarks =
    marks.trim() === "" ||
    Number.isNaN(
      numericMarks,
    ) ||
    numericMarks < 0;

  return (
    <div className="review-stack-detail">
      <div className="ledger-card-top">
        <div>
          <p className="eyebrow">
            Student response
          </p>

          <h2
            style={{
              margin:
                "8px 0 3px",
              fontFamily:
                "var(--font-serif)",
              fontSize:
                "1.8rem",
              fontWeight: 580,
            }}
          >
            {
              submission.studentName
            }
          </h2>

          <p className="muted">
            {
              submission.studentEmail
            }
          </p>
        </div>

        <SubmissionStatusBadge
          status={
            submission.status
          }
        />
      </div>

      <div
        className="teacher-detail-meta"
        style={{
          marginTop: 22,
        }}
      >
        <div className="teacher-meta-row">
          <span>
            Submitted
          </span>

          <strong>
            {formatTeacherDate(
              submission
                .submittedAtUtc,
            )}
          </strong>
        </div>

        <div className="teacher-meta-row">
          <span>
            Last update
          </span>

          <strong>
            {formatTeacherDate(
              submission
                .updatedAtUtc,
            )}
          </strong>
        </div>

        <div className="teacher-meta-row">
          <span>
            Submission type
          </span>

          <strong>
            {submission.isLate
              ? "Late submission"
              : "On-time submission"}
          </strong>
        </div>
      </div>

      <div className="review-answer">
        {submission.answerText}
      </div>

      {submission.status !==
        "Draft" &&
        submission.status !==
          "Graded" && (
          <div
            className="review-actions"
            style={{
              marginTop: 18,
            }}
          >
            <Button
              variant="secondary"
              size="small"
              disabled={
                statusPending
              }
              onClick={() =>
                onStatus(
                  "UnderReview",
                  submission.version,
                )
              }
            >
              Mark under review
            </Button>

            <Button
              variant="secondary"
              size="small"
              disabled={
                statusPending
              }
              onClick={() =>
                onStatus(
                  "NeedsRevision",
                  submission.version,
                )
              }
            >
              Request revision
            </Button>
          </div>
        )}

      {submission.status !==
        "Draft" && (
        <div className="review-grading">
          <div className="form-field">
            <label htmlFor="grade-marks">
              Marks
            </label>

            <Input
              id="grade-marks"
              type="number"
              min="0"
              step="0.01"
              value={marks}
              onChange={(event) =>
                setMarks(
                  event.target.value,
                )
              }
            />
          </div>

          <div className="form-field review-feedback">
            <label htmlFor="grade-feedback">
              Teacher feedback
            </label>

            <Textarea
              id="grade-feedback"
              value={feedback}
              placeholder="Write concise, useful academic feedback."
              onChange={(event) =>
                setFeedback(
                  event.target.value,
                )
              }
            />
          </div>

          <div className="review-actions">
            <Button
              disabled={
                invalidMarks ||
                gradePending
              }
              onClick={() =>
                onGrade(
                  numericMarks,
                  feedback,
                  submission.version,
                )
              }
            >
              {gradePending
                ? "Saving grade…"
                : submission.status ===
                    "Graded"
                  ? "Update grade"
                  : "Save grade"}
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}

export function TeacherReviewStack({
  assignmentId,
}: {
  assignmentId: string;
}) {
  const {
    request,
  } = useAuth();

  const queryClient =
    useQueryClient();

  const [
    selectedId,
    setSelectedId,
  ] = useState<
    string | null
  >(null);

  const query =
    useQuery({
      queryKey: [
        "teacher-submissions",
        assignmentId,
      ],

      queryFn: () =>
        request<
          PagedResult<
            TeacherSubmission
          >
        >(
          `/api/v1/teacher/assignments/${assignmentId}/submissions?page=1&pageSize=100`,
        ),
    });

  const submissions =
    query.data?.items ?? [];

  const effectiveId =
    selectedId ??
    submissions[0]?.id ??
    null;

  const selected =
    submissions.find(
      (item) =>
        item.id === effectiveId,
    ) ?? null;

  const statusMutation =
    useMutation({
      mutationFn: ({
        submissionId,
        status,
        version,
      }: {
        submissionId: string;
        status:
          | "UnderReview"
          | "NeedsRevision";
        version: number;
      }) =>
        request<TeacherSubmission>(
          `/api/v1/teacher/submissions/${submissionId}/review-status`,
          {
            method: "PUT",
            body:
              JSON.stringify({
                status,
                version,
              }),
          },
        ),

      onSuccess: () =>
        queryClient.invalidateQueries({
          queryKey: [
            "teacher-submissions",
            assignmentId,
          ],
        }),
    });

  const gradeMutation =
    useMutation({
      mutationFn: ({
        submissionId,
        marksAwarded,
        teacherFeedback,
        version,
      }: {
        submissionId: string;
        marksAwarded: number;
        teacherFeedback: string;
        version: number;
      }) =>
        request<TeacherSubmission>(
          `/api/v1/teacher/submissions/${submissionId}/grade`,
          {
            method: "PUT",
            body:
              JSON.stringify({
                marksAwarded,
                teacherFeedback:
                  teacherFeedback.trim() ||
                  null,
                version,
              }),
          },
        ),

      onSuccess: async () => {
        await queryClient.invalidateQueries({
          queryKey: [
            "teacher-submissions",
            assignmentId,
          ],
        });

        await queryClient.invalidateQueries({
          queryKey: [
            "teacher-assignments",
          ],
        });

        await queryClient.invalidateQueries({
          queryKey: [
            "teacher-dashboard",
          ],
        });
      },
    });

  const error =
    statusMutation.error ??
    gradeMutation.error;

  const concurrencyError =
    error instanceof ApiError &&
    error.status === 409;

  return (
    <>
      <TeacherPageHeading
        eyebrow="Review stack"
        title="Review Student work"
        description="Read the answer in context, move it through review, and return a grade without losing the Student's latest update."
        action={
          concurrencyError ? (
            <Button
              variant="secondary"
              onClick={() =>
                void query.refetch()
              }
            >
              <RefreshCw
                size={16}
              />
              Reload latest data
            </Button>
          ) : undefined
        }
      />

      {query.isLoading && (
        <p className="muted">
          Loading submissions…
        </p>
      )}

      {query.error && (
        <TeacherError
          message={
            query.error instanceof Error
              ? query.error.message
              : "Unable to load submissions."
          }
        />
      )}

      {error && (
        <div
          style={{
            marginBottom: 16,
          }}
        >
          <TeacherError
            message={
              concurrencyError
                ? "This submission changed after it was loaded. Reload the latest data before grading again."
                : error instanceof Error
                  ? error.message
                  : "Unable to update submission."
            }
          />
        </div>
      )}

      {!query.isLoading &&
        submissions.length ===
          0 && (
          <EmptyState
            eyebrow="Review stack"
            title="No submissions yet"
            description="Student submissions will appear here once work is saved and submitted."
          />
        )}

      {submissions.length >
        0 && (
        <div className="review-stack">
          <aside className="review-stack-list">
            <div className="review-stack-list-head">
              <h2>
                Students
              </h2>

              <span className="muted">
                {
                  submissions.length
                }{" "}
                submissions
              </span>
            </div>

            {submissions.map(
              (submission) => (
                <button
                  key={
                    submission.id
                  }
                  type="button"
                  className={`review-person-button ${
                    submission.id ===
                    effectiveId
                      ? "active"
                      : ""
                  }`}
                  onClick={() =>
                    setSelectedId(
                      submission.id,
                    )
                  }
                >
                  <span className="review-avatar">
                    {initials(
                      submission
                        .studentName,
                    )}
                  </span>

                  <span>
                    <span className="review-person-name">
                      {
                        submission
                          .studentName
                      }
                    </span>

                    <span className="review-person-email">
                      {
                        submission
                          .studentEmail
                      }
                    </span>
                  </span>

                  <SubmissionStatusBadge
                    status={
                      submission
                        .status
                    }
                  />
                </button>
              ),
            )}
          </aside>

          {selected ? (
            <ReviewPanel
              key={`${selected.id}-${selected.version}`}
              submission={
                selected
              }
              statusPending={
                statusMutation
                  .isPending
              }
              gradePending={
                gradeMutation
                  .isPending
              }
              onStatus={(
                status,
                version,
              ) =>
                statusMutation.mutate(
                  {
                    submissionId:
                      selected.id,
                    status,
                    version,
                  },
                )
              }
              onGrade={(
                marksAwarded,
                teacherFeedback,
                version,
              ) =>
                gradeMutation.mutate(
                  {
                    submissionId:
                      selected.id,
                    marksAwarded,
                    teacherFeedback,
                    version,
                  },
                )
              }
            />
          ) : (
            <div className="teacher-review-empty">
              Select a submission.
            </div>
          )}
        </div>
      )}
    </>
  );
}