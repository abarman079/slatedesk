"use client";

import {
  useEffect,
} from "react";

import {
  useRouter,
} from "next/navigation";

import {
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";

import {
  useForm,
} from "react-hook-form";

import {
  z,
} from "zod";

import {
  zodResolver,
} from "@hookform/resolvers/zod";

import {
  Button,
  Card,
  Input,
  Select,
  Textarea,
} from "@/components/ui";

import {
  TeacherError,
  TeacherPageHeading,
} from "@/features/teacher/teacher-shared";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import type {
  TeacherAllocationOption,
  TeacherAssignment,
} from "@/types/teacher";

const schema = z.object({
  allocationKey: z
    .string()
    .min(
      1,
      "Select a teaching allocation.",
    ),

  title: z
    .string()
    .trim()
    .min(
      2,
      "Assignment title is required.",
    )
    .max(200),

  description: z
    .string()
    .trim()
    .min(
      2,
      "Description is required.",
    )
    .max(2000),

  instructions:
    z.string().max(4000),

  deadlineLocal:
    z.string().min(
      1,
      "Deadline is required.",
    ),

  maximumMarks:
    z.number()
      .positive(
        "Maximum marks must be greater than zero.",
      )
      .max(1000000),

  allowResubmission:
    z.boolean(),

  allowLateSubmission:
    z.boolean(),
});

type AssignmentForm =
  z.infer<typeof schema>;

function toLocalInput(
  utcValue: string,
) {
  const date =
    new Date(utcValue);

  const adjusted =
    new Date(
      date.getTime() -
        date.getTimezoneOffset() *
          60_000,
    );

  return adjusted
    .toISOString()
    .slice(0, 16);
}

export function TeacherAssignmentEditor({
  assignmentId,
}: {
  assignmentId?: string;
}) {
  const router =
    useRouter();

  const queryClient =
    useQueryClient();

  const {
    request,
  } = useAuth();

  const editing =
    Boolean(assignmentId);

  const allocations =
    useQuery({
      queryKey: [
        "teacher-allocation-options",
      ],

      queryFn: () =>
        request<
          TeacherAllocationOption[]
        >(
          "/api/v1/teacher/assignments/allocation-options",
        ),
    });

  const assignment =
    useQuery({
      queryKey: [
        "teacher-assignment",
        assignmentId,
      ],

      enabled: editing,

      queryFn: () =>
        request<TeacherAssignment>(
          `/api/v1/teacher/assignments/${assignmentId}`,
        ),
    });

  const {
    register,
    handleSubmit,
    reset,
    formState: {
      errors,
    },
  } = useForm<AssignmentForm>({
    resolver:
      zodResolver(schema),

    defaultValues: {
      allocationKey: "",
      title: "",
      description: "",
      instructions: "",
      deadlineLocal: "",
      maximumMarks: 30,
      allowResubmission: true,
      allowLateSubmission: false,
    },
  });

  useEffect(() => {
    if (!assignment.data) {
      return;
    }

    reset({
      allocationKey:
        `${assignment.data.academicClassId}|${assignment.data.subjectId}`,

      title:
        assignment.data.title,

      description:
        assignment.data
          .description,

      instructions:
        assignment.data
          .instructions ?? "",

      deadlineLocal:
        toLocalInput(
          assignment.data
            .deadlineUtc,
        ),

      maximumMarks:
        assignment.data
          .maximumMarks,

      allowResubmission:
        assignment.data
          .allowResubmission,

      allowLateSubmission:
        assignment.data
          .allowLateSubmission,
    });
  }, [
    assignment.data,
    reset,
  ]);

  const mutation =
    useMutation({
      mutationFn: (
        values: AssignmentForm,
      ) => {
        const [
          academicClassId,
          subjectId,
        ] =
          values.allocationKey.split(
            "|",
          );

        const body = {
          academicClassId,
          subjectId,

          title:
            values.title.trim(),

          description:
            values.description.trim(),

          instructions:
            values.instructions
              .trim() || null,

          deadlineUtc:
            new Date(
              values.deadlineLocal,
            ).toISOString(),

          maximumMarks:
            values.maximumMarks,

          allowResubmission:
            values.allowResubmission,

          allowLateSubmission:
            values.allowLateSubmission,
        };

        return request<
          TeacherAssignment
        >(
          editing
            ? `/api/v1/teacher/assignments/${assignmentId}`
            : "/api/v1/teacher/assignments",
          {
            method:
              editing
                ? "PUT"
                : "POST",

            body:
              JSON.stringify(body),
          },
        );
      },

      onSuccess: async (
        result,
      ) => {
        await queryClient
          .invalidateQueries({
            queryKey: [
              "teacher-assignments",
            ],
          });

        await queryClient
          .invalidateQueries({
            queryKey: [
              "teacher-dashboard",
            ],
          });

        router.push(
          `/teacher/assignments/${result.id}`,
        );
      },
    });

  if (
    editing &&
    assignment.isLoading
  ) {
    return (
      <p className="muted">
        Loading assignment…
      </p>
    );
  }

  return (
    <>
      <TeacherPageHeading
        eyebrow={
          editing
            ? "Assignment editor"
            : "New academic work"
        }
        title={
          editing
            ? "Edit assignment"
            : "Create assignment"
        }
        description="Keep the brief precise, choose the correct teaching allocation, and set a deadline students can understand."
      />

      <div className="teacher-form-shell">
        <Card>
          <form
            className="teacher-editor"
            onSubmit={handleSubmit(
              (values) =>
                mutation.mutate(
                  values,
                ),
            )}
          >
            <div className="form-field teacher-form-wide">
              <label htmlFor="allocation">
                Teaching allocation
              </label>

              <Select
                id="allocation"
                {...register(
                  "allocationKey",
                )}
              >
                <option value="">
                  Select class and subject
                </option>

                {allocations.data?.map(
                  (option) => (
                    <option
                      key={`${option.academicClassId}|${option.subjectId}`}
                      value={`${option.academicClassId}|${option.subjectId}`}
                    >
                      {
                        option.classCode
                      }{" "}
                      ·{" "}
                      {
                        option.subjectCode
                      }{" "}
                      —{" "}
                      {
                        option.subjectName
                      }
                    </option>
                  ),
                )}
              </Select>

              {errors.allocationKey && (
                <p className="form-error">
                  {
                    errors
                      .allocationKey
                      .message
                  }
                </p>
              )}
            </div>

            <div className="form-field teacher-form-wide">
              <label htmlFor="title">
                Assignment title
              </label>

              <Input
                id="title"
                {...register("title")}
              />

              {errors.title && (
                <p className="form-error">
                  {
                    errors.title
                      .message
                  }
                </p>
              )}
            </div>

            <div className="form-field teacher-form-wide">
              <label htmlFor="description">
                Description
              </label>

              <Textarea
                id="description"
                {...register(
                  "description",
                )}
              />

              {errors.description && (
                <p className="form-error">
                  {
                    errors
                      .description
                      .message
                  }
                </p>
              )}
            </div>

            <div className="form-field teacher-form-wide">
              <label htmlFor="instructions">
                Instructions
              </label>

              <Textarea
                id="instructions"
                placeholder="Optional detailed instructions"
                {...register(
                  "instructions",
                )}
              />
            </div>

            <div className="teacher-editor-grid teacher-form-wide">
              <div className="form-field">
                <label htmlFor="deadline">
                  Deadline
                </label>

                <Input
                  id="deadline"
                  type="datetime-local"
                  {...register(
                    "deadlineLocal",
                  )}
                />

                {errors.deadlineLocal && (
                  <p className="form-error">
                    {
                      errors
                        .deadlineLocal
                        .message
                    }
                  </p>
                )}
              </div>

              <div className="form-field">
                <label htmlFor="marks">
                  Maximum marks
                </label>

                <Input
                  id="marks"
                  type="number"
                  min="0.01"
                  step="0.01"
                  {...register(
                    "maximumMarks",
                    {
                      valueAsNumber:
                        true,
                    },
                  )}
                />

                {errors.maximumMarks && (
                  <p className="form-error">
                    {
                      errors
                        .maximumMarks
                        .message
                    }
                  </p>
                )}
              </div>
            </div>

            <div className="teacher-checkboxes teacher-form-wide">
              <label className="teacher-check">
                <input
                  type="checkbox"
                  {...register(
                    "allowResubmission",
                  )}
                />

                <span>
                  <strong>
                    Allow resubmission
                  </strong>

                  <span>
                    Students may revise
                    submitted work while
                    the deadline rules
                    permit.
                  </span>
                </span>
              </label>

              <label className="teacher-check">
                <input
                  type="checkbox"
                  {...register(
                    "allowLateSubmission",
                  )}
                />

                <span>
                  <strong>
                    Allow late submission
                  </strong>

                  <span>
                    Work arriving after
                    the deadline is marked
                    Late.
                  </span>
                </span>
              </label>
            </div>

            {mutation.error && (
              <TeacherError
                message={
                  mutation.error
                    instanceof Error
                    ? mutation
                        .error.message
                    : "Unable to save assignment."
                }
              />
            )}

            <div className="admin-form-actions teacher-form-wide">
              <Button
                type="button"
                variant="secondary"
                onClick={() =>
                  router.back()
                }
              >
                Cancel
              </Button>

              <Button
                type="submit"
                disabled={
                  mutation.isPending
                }
              >
                {mutation.isPending
                  ? "Saving…"
                  : editing
                    ? "Save changes"
                    : "Save draft"}
              </Button>
            </div>
          </form>
        </Card>

        <Card className="teacher-editor-side">
          <p className="eyebrow">
            Publishing note
          </p>

          <h2>
            Draft first.
            Publish deliberately.
          </h2>

          <p>
            Creating an assignment does
            not expose it to Students.
            It begins as Draft and can
            be reviewed before
            publication.
          </p>

          <p>
            Publication requires a
            future deadline and
            positive maximum marks.
          </p>
        </Card>
      </div>
    </>
  );
}