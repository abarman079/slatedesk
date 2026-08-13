"use client";

import { useMemo, useState } from "react";

import { CheckCircle2, RefreshCw, Save, Send } from "lucide-react";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { Button, Card, Textarea } from "@/components/ui";

import { Dialog } from "@/components/ui/overlay";

import {
  GradeSeal,
  StudentDeadlineRail,
  StudentError,
  StudentPageHeading,
  StudentSubmissionBadge,
  formatStudentDate,
} from "@/features/student/student-shared";

import { useAuth } from "@/features/auth/auth-provider";

import { ApiError } from "@/lib/api-client";

import type {
  PagedResult,
  StudentAssignment,
  StudentSubmission,
} from "@/types/student";

export function StudentAssignmentDetail({
  assignmentId,
}: {
  assignmentId: string;
}) {
  const { request } = useAuth();

  const queryClient = useQueryClient();

  const [answerDraft, setAnswerDraft] = useState<{
    assignmentId: string;
    value: string;
  } | null>(null);

  const [confirmOpen, setConfirmOpen] = useState(false);

  const assignment = useQuery({
    queryKey: ["student-assignment", assignmentId],

    queryFn: () =>
      request<StudentAssignment>(`/api/v1/student/assignments/${assignmentId}`),
  });

  const submissions = useQuery({
    queryKey: ["student-submissions"],

    queryFn: () =>
      request<PagedResult<StudentSubmission>>(
        "/api/v1/student/submissions?page=1&pageSize=100",
      ),
  });

  const existing = useMemo(
    () =>
      submissions.data?.items.find(
        (item) => item.assignmentId === assignmentId,
      ) ?? null,
    [submissions.data, assignmentId],
  );

  const answer =
    answerDraft?.assignmentId === assignmentId
      ? answerDraft.value
      : (existing?.answerText ?? "");

  const refreshAll = async () => {
    await Promise.all([
      queryClient.invalidateQueries({
        queryKey: ["student-submissions"],
      }),

      queryClient.invalidateQueries({
        queryKey: ["student-assignment", assignmentId],
      }),

      queryClient.invalidateQueries({
        queryKey: ["student-assignment-list"],
      }),

      queryClient.invalidateQueries({
        queryKey: ["student-assignments"],
      }),

      queryClient.invalidateQueries({
        queryKey: ["student-results"],
      }),
    ]);
  };

  const saveMutation = useMutation({
    mutationFn: async () => {
      if (existing) {
        return request<StudentSubmission>(
          `/api/v1/student/submissions/${existing.id}`,
          {
            method: "PUT",

            body: JSON.stringify({
              answerText: answer,
              version: existing.version,
            }),
          },
        );
      }

      return request<StudentSubmission>(
        `/api/v1/student/assignments/${assignmentId}/submissions`,
        {
          method: "POST",

          body: JSON.stringify({
            answerText: answer,
          }),
        },
      );
    },

    onSuccess: async () => {
      await refreshAll();

      setAnswerDraft(null);
    },
  });

  const submitMutation = useMutation({
    mutationFn: async () => {
      let submission = existing;

      if (!submission) {
        submission = await request<StudentSubmission>(
          `/api/v1/student/assignments/${assignmentId}/submissions`,
          {
            method: "POST",

            body: JSON.stringify({
              answerText: answer,
            }),
          },
        );
      } else if (answer !== submission.answerText) {
        submission = await request<StudentSubmission>(
          `/api/v1/student/submissions/${submission.id}`,
          {
            method: "PUT",

            body: JSON.stringify({
              answerText: answer,

              version: submission.version,
            }),
          },
        );
      }

      if (!answer.trim()) {
        throw new Error("Write an answer before submitting.");
      }

      return request<StudentSubmission>(
        `/api/v1/student/submissions/${submission.id}/submit`,
        {
          method: "POST",

          body: JSON.stringify({
            version: submission.version,
          }),
        },
      );
    },

    onSuccess: async () => {
      setConfirmOpen(false);

      await refreshAll();

      setAnswerDraft(null);
    },
  });

  const error = saveMutation.error ?? submitMutation.error;

  const concurrencyError = error instanceof ApiError && error.status === 409;

  if (assignment.isLoading || submissions.isLoading) {
    return <p className="muted">Loading assignment…</p>;
  }

  if (assignment.error || !assignment.data) {
    return (
      <StudentError
        message={
          assignment.error instanceof Error
            ? assignment.error.message
            : "Assignment was not found."
        }
      />
    );
  }

  const item = assignment.data;

  const canEdit = existing ? existing.canEdit : item.canSubmit;

  const canSubmit = existing ? existing.canSubmit : item.canSubmit;

  const graded = existing?.status === "Graded";

  const activity = [
    item.publishedAtUtc
      ? {
          label: "Assignment published",
          date: item.publishedAtUtc,
        }
      : null,

    existing
      ? {
          label: "Draft last updated",
          date: existing.updatedAtUtc,
        }
      : null,

    existing?.submittedAtUtc
      ? {
          label: existing.status === "Late" ? "Submitted late" : "Submitted",
          date: existing.submittedAtUtc,
        }
      : null,

    existing?.gradedAtUtc
      ? {
          label: "Feedback and grade released",
          date: existing.gradedAtUtc,
        }
      : null,
  ].filter(
    (
      value,
    ): value is {
      label: string;
      date: string;
    } => Boolean(value),
  );

  return (
    <>
      <StudentPageHeading
        eyebrow={item.subjectCode}
        title={item.title}
        description={`${item.subjectName} · ${item.teacherName}`}
      />

      {error && (
        <div
          style={{
            marginBottom: 16,
          }}
        >
          <StudentError
            message={
              concurrencyError
                ? "Your submission changed after this page loaded. Reload the latest version before saving again."
                : error instanceof Error
                  ? error.message
                  : "Unable to update submission."
            }
          />

          {concurrencyError && (
            <Button
              variant="secondary"
              size="small"
              style={{
                marginTop: 10,
              }}
              onClick={() => {
                setAnswerDraft(null);

                void submissions.refetch();
                void assignment.refetch();
              }}
            >
              <RefreshCw size={16} />
              Reload latest data
            </Button>
          )}
        </div>
      )}

      <div className="student-detail-grid">
        <div className="student-detail-main">
          <Card className="student-assignment-heading">
            <div className="student-folio-head">
              <span className="ledger-subject-code">{item.subjectCode}</span>

              <StudentSubmissionBadge
                status={existing?.status ?? item.submissionStatus}
              />
            </div>

            <h1>{item.title}</h1>

            <p className="student-assignment-copy">{item.description}</p>

            {item.instructions && (
              <>
                <p
                  className="eyebrow"
                  style={{
                    marginTop: 28,
                  }}
                >
                  Instructions
                </p>

                <p className="student-assignment-copy">{item.instructions}</p>
              </>
            )}

            <StudentDeadlineRail
              publishedAtUtc={item.publishedAtUtc}
              deadlineUtc={item.deadlineUtc}
              isPastDeadline={item.isPastDeadline}
              wouldBeLate={item.wouldBeLate}
            />
          </Card>

          {graded && existing?.marksAwarded !== null ? (
            <Card className="student-feedback-card">
              <GradeSeal
                marks={existing.marksAwarded}
                maximumMarks={existing.maximumMarks}
              />

              <div>
                <p className="eyebrow">Teacher feedback</p>

                <h2>Your result is ready.</h2>

                <p>
                  {existing.teacherFeedback || "No written feedback was added."}
                </p>

                <p
                  className="muted"
                  style={{
                    marginTop: 10,
                    fontSize: "0.75rem",
                  }}
                >
                  Graded {formatStudentDate(existing.gradedAtUtc)}
                </p>
              </div>
            </Card>
          ) : (
            <Card className="student-submission-editor">
              <div>
                <p className="eyebrow">Your submission</p>

                <h2>{existing ? "Continue your work" : "Start your answer"}</h2>
              </div>

              {existing?.status === "UnderReview" && (
                <div className="student-status-callout">
                  <strong>Under review</strong>

                  <p>
                    Your Teacher is reviewing this submission. Editing is
                    temporarily locked.
                  </p>
                </div>
              )}

              {existing?.status === "NeedsRevision" && (
                <div className="student-status-callout amber">
                  <strong>Revision requested</strong>

                  <p>
                    Update your response and submit it again if the assignment
                    policy permits.
                  </p>
                </div>
              )}

              {item.wouldBeLate && canSubmit && (
                <div className="student-status-callout amber">
                  <strong>Late submission</strong>

                  <p>
                    The original deadline has passed. If you submit now, the
                    work will be recorded as Late.
                  </p>
                </div>
              )}

              <div className="form-field">
                <label htmlFor="answer">Answer</label>

                <Textarea
                  id="answer"
                  value={answer}
                  disabled={!canEdit}
                  placeholder="Write your assignment response here…"
                  onChange={(event) =>
                    setAnswerDraft({
                      assignmentId,
                      value: event.target.value,
                    })
                  }
                />
              </div>

              <div className="student-editor-actions">
                <span className="muted">
                  {existing ? `Status: ${existing.status}` : "Not saved yet"}
                </span>

                <div>
                  <Button
                    variant="secondary"
                    disabled={!canEdit || saveMutation.isPending}
                    onClick={() => saveMutation.mutate()}
                  >
                    <Save size={17} />

                    {saveMutation.isPending
                      ? "Saving…"
                      : existing
                        ? "Save changes"
                        : "Save draft"}
                  </Button>

                  <Button
                    disabled={!canSubmit || submitMutation.isPending}
                    onClick={() => setConfirmOpen(true)}
                  >
                    <Send size={17} />

                    {existing?.submittedAtUtc ? "Resubmit" : "Submit answer"}
                  </Button>
                </div>
              </div>
            </Card>
          )}
        </div>

        <div>
          <Card>
            <p className="eyebrow">Assignment record</p>

            <div className="student-policy-note">
              <div className="student-policy-row">
                <span>Teacher</span>

                <strong>{item.teacherName}</strong>
              </div>

              <div className="student-policy-row">
                <span>Class</span>

                <strong>{item.classCode}</strong>
              </div>

              <div className="student-policy-row">
                <span>Maximum marks</span>

                <strong>{item.maximumMarks}</strong>
              </div>

              <div className="student-policy-row">
                <span>Deadline</span>

                <strong>{formatStudentDate(item.deadlineUtc)}</strong>
              </div>

              <div className="student-policy-row">
                <span>Resubmission</span>

                <strong>
                  {item.allowResubmission ? "Allowed" : "Not allowed"}
                </strong>
              </div>

              <div className="student-policy-row">
                <span>Late work</span>

                <strong>
                  {item.allowLateSubmission ? "Allowed" : "Not allowed"}
                </strong>
              </div>
            </div>
          </Card>

          <Card
            style={{
              marginTop: 18,
            }}
          >
            <p className="eyebrow">Activity</p>

            <div className="student-activity-thread">
              {activity.map((event) => (
                <div
                  key={`${event.label}-${event.date}`}
                  className="student-activity-item"
                >
                  <span className="student-activity-dot" />

                  <div className="student-activity-copy">
                    <strong>{event.label}</strong>

                    <span>{formatStudentDate(event.date)}</span>
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </div>
      </div>

      <Dialog
        open={confirmOpen}
        onClose={() => setConfirmOpen(false)}
        title="Submit assignment?"
      >
        <div className="admin-form">
          <div className="student-status-callout">
            <CheckCircle2 size={20} color="var(--cobalt)" />

            <strong>
              {item.wouldBeLate
                ? "This will be recorded as a late submission."
                : "Your latest answer will be submitted for review."}
            </strong>
          </div>

          <p className="muted">
            Deadline: {formatStudentDate(item.deadlineUtc)}
          </p>

          <div className="admin-form-actions">
            <Button variant="secondary" onClick={() => setConfirmOpen(false)}>
              Keep editing
            </Button>

            <Button
              disabled={submitMutation.isPending}
              onClick={() => submitMutation.mutate()}
            >
              {submitMutation.isPending
                ? "Submitting…"
                : existing?.submittedAtUtc
                  ? "Confirm resubmission"
                  : "Confirm submission"}
            </Button>
          </div>
        </div>
      </Dialog>
    </>
  );
}
