"use client";

import Link from "next/link";

import {
  Edit3,
  Send,
  Square,
  Trash2,
  Users,
} from "lucide-react";

import {
  useRouter,
} from "next/navigation";

import {
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";

import {
  Button,
  Card,
} from "@/components/ui";

import {
  AssignmentStatusBadge,
  DeadlineRail,
  TeacherError,
  formatTeacherDate,
} from "@/features/teacher/teacher-shared";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import type {
  TeacherAssignment,
} from "@/types/teacher";

export function TeacherAssignmentDetail({
  assignmentId,
}: {
  assignmentId: string;
}) {
  const router =
    useRouter();

  const queryClient =
    useQueryClient();

  const {
    request,
  } = useAuth();

  const query =
    useQuery({
      queryKey: [
        "teacher-assignment",
        assignmentId,
      ],

      queryFn: () =>
        request<TeacherAssignment>(
          `/api/v1/teacher/assignments/${assignmentId}`,
        ),
    });

  const mutation =
    useMutation({
      mutationFn: async (
        action:
          | "publish"
          | "close"
          | "delete",
      ) => {
        if (action === "delete") {
          return request<void>(
            `/api/v1/teacher/assignments/${assignmentId}`,
            {
              method: "DELETE",
            },
          );
        }

        return request<
          TeacherAssignment
        >(
          `/api/v1/teacher/assignments/${assignmentId}/${action}`,
          {
            method: "POST",
          },
        );
      },

      onSuccess: async (
        _data,
        action,
      ) => {
        await queryClient.invalidateQueries({
          queryKey: [
            "teacher-assignment",
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

        if (action === "delete") {
          router.push(
            "/teacher/assignments",
          );
        }
      },
    });

  if (query.isLoading) {
    return (
      <p className="muted">
        Loading assignment…
      </p>
    );
  }

  if (
    query.error ||
    !query.data
  ) {
    return (
      <TeacherError
        message={
          query.error instanceof Error
            ? query.error.message
            : "Assignment was not found."
        }
      />
    );
  }

  const assignment =
    query.data;

  return (
    <>
      <div className="teacher-detail-grid">
        <div className="teacher-detail-main">
          <Card>
            <div className="ledger-card-top">
              <span className="ledger-subject-code">
                {
                  assignment
                    .subjectCode
                }
              </span>

              <AssignmentStatusBadge
                status={
                  assignment.status
                }
              />
            </div>

            <h1>
              {assignment.title}
            </h1>

            <p className="teacher-detail-copy">
              {
                assignment
                  .description
              }
            </p>

            {assignment.instructions && (
              <>
                <p
                  className="eyebrow"
                  style={{
                    marginTop: 28,
                  }}
                >
                  Instructions
                </p>

                <p className="teacher-detail-copy">
                  {
                    assignment
                      .instructions
                  }
                </p>
              </>
            )}

            <DeadlineRail
              publishedAtUtc={
                assignment
                  .publishedAtUtc
              }
              deadlineUtc={
                assignment
                  .deadlineUtc
              }
              isPastDeadline={
                assignment
                  .isPastDeadline
              }
            />
          </Card>

          <Card>
            <p className="eyebrow">
              Assignment record
            </p>

            <div className="teacher-detail-meta">
              <div className="teacher-meta-row">
                <span>
                  Class
                </span>

                <strong>
                  {
                    assignment
                      .classCode
                  }{" "}
                  ·{" "}
                  {
                    assignment
                      .className
                  }
                </strong>
              </div>

              <div className="teacher-meta-row">
                <span>
                  Subject
                </span>

                <strong>
                  {
                    assignment
                      .subjectName
                  }
                </strong>
              </div>

              <div className="teacher-meta-row">
                <span>
                  Deadline
                </span>

                <strong>
                  {formatTeacherDate(
                    assignment
                      .deadlineUtc,
                  )}
                </strong>
              </div>

              <div className="teacher-meta-row">
                <span>
                  Maximum marks
                </span>

                <strong>
                  {
                    assignment
                      .maximumMarks
                  }
                </strong>
              </div>

              <div className="teacher-meta-row">
                <span>
                  Resubmission
                </span>

                <strong>
                  {assignment
                    .allowResubmission
                    ? "Allowed"
                    : "Disabled"}
                </strong>
              </div>

              <div className="teacher-meta-row">
                <span>
                  Late work
                </span>

                <strong>
                  {assignment
                    .allowLateSubmission
                    ? "Allowed"
                    : "Disabled"}
                </strong>
              </div>
            </div>
          </Card>
        </div>

        <div>
          <Card>
            <p className="eyebrow">
              Workflow
            </p>

            <div
              className="admin-stat-value"
              style={{
                marginBottom: 4,
              }}
            >
              {
                assignment
                  .submissionCount
              }
            </div>

            <p className="muted">
              Student submissions
            </p>

            <div className="teacher-detail-actions">
              <Link
                href={`/teacher/assignments/${assignment.id}/submissions`}
              >
                <Button
                  variant="secondary"
                  style={{
                    width: "100%",
                  }}
                >
                  <Users size={17} />
                  Open submissions
                </Button>
              </Link>

              {assignment.status !==
                "Closed" && (
                <Link
                  href={`/teacher/assignments/${assignment.id}/edit`}
                >
                  <Button
                    variant="secondary"
                    style={{
                      width: "100%",
                    }}
                  >
                    <Edit3 size={17} />
                    Edit assignment
                  </Button>
                </Link>
              )}

              {assignment.status ===
                "Draft" && (
                <Button
                  style={{
                    width: "100%",
                  }}
                  disabled={
                    mutation.isPending
                  }
                  onClick={() =>
                    mutation.mutate(
                      "publish",
                    )
                  }
                >
                  <Send size={17} />
                  Publish
                </Button>
              )}

              {assignment.status ===
                "Published" && (
                <Button
                  variant="secondary"
                  style={{
                    width: "100%",
                  }}
                  disabled={
                    mutation.isPending
                  }
                  onClick={() =>
                    mutation.mutate(
                      "close",
                    )
                  }
                >
                  <Square size={16} />
                  Close assignment
                </Button>
              )}

              <Button
                variant="danger"
                style={{
                  width: "100%",
                }}
                disabled={
                  mutation.isPending
                }
                onClick={() => {
                  if (
                    window.confirm(
                      "Delete or archive this assignment?",
                    )
                  ) {
                    mutation.mutate(
                      "delete",
                    );
                  }
                }}
              >
                <Trash2 size={17} />
                Delete / archive
              </Button>
            </div>
          </Card>

          {mutation.error && (
            <div
              style={{
                marginTop: 14,
              }}
            >
              <TeacherError
                message={
                  mutation.error
                    instanceof Error
                    ? mutation
                        .error.message
                    : "Unable to update assignment."
                }
              />
            </div>
          )}
        </div>
      </div>
    </>
  );
}