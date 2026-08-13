"use client";

import {
  useState,
} from "react";

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
  EmptyState,
  Input,
  Select,
  Table,
  Toast,
} from "@/components/ui";

import {
  Drawer,
} from "@/components/ui/overlay";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import {
  AdminError,
  AdminPageHeading,
  Pager,
  StatusBadge,
} from "@/features/admin/admin-shared";

import type {
  AcademicClass,
  AdminUser,
  PagedResult,
  StudentEnrollment,
  Subject,
  TeacherAllocation,
} from "@/types/admin";

/* =========================================================
   USERS
   ========================================================= */

const userSchema = z.object({
  fullName: z
    .string()
    .trim()
    .min(2, "Full name is required.")
    .max(150),

  email: z
    .string()
    .trim()
    .email("Enter a valid email."),

  role: z.enum([
    "Teacher",
    "Student",
  ]),

  password: z
    .string()
    .min(
      8,
      "Password must contain at least 8 characters.",
    ),
});

type UserForm =
  z.infer<typeof userSchema>;

export function AdminUsersPage() {
  const {
    request,
  } = useAuth();

  const queryClient =
    useQueryClient();

  const [page, setPage] =
    useState(1);

  const [search, setSearch] =
    useState("");

  const [role, setRole] =
    useState("");

  const [drawerOpen, setDrawerOpen] =
    useState(false);

  const [notice, setNotice] =
    useState<string | null>(null);

  const query =
    useQuery({
      queryKey: [
        "admin-users",
        page,
        search,
        role,
      ],

      queryFn: () =>
        request<
          PagedResult<AdminUser>
        >(
          `/api/v1/admin/users?page=${page}&pageSize=10&search=${encodeURIComponent(
            search,
          )}${
            role
              ? `&role=${role}`
              : ""
          }`,
        ),
    });

  const {
    register,
    handleSubmit,
    reset,
    formState: {
      errors,
      isSubmitting,
    },
  } = useForm<UserForm>({
    resolver:
      zodResolver(userSchema),

    defaultValues: {
      role: "Teacher",
    },
  });

  const createMutation =
    useMutation({
      mutationFn: (
        values: UserForm,
      ) =>
        request<AdminUser>(
          "/api/v1/admin/users",
          {
            method: "POST",
            body:
              JSON.stringify(
                values,
              ),
          },
        ),

      onSuccess: async () => {
        await queryClient.invalidateQueries({
          queryKey: [
            "admin-users",
          ],
        });

        await queryClient.invalidateQueries({
          queryKey: [
            "admin-dashboard",
          ],
        });

        setDrawerOpen(false);
        reset();
        setNotice(
          "Account created successfully.",
        );
      },
    });

  const statusMutation =
    useMutation({
      mutationFn: ({
        id,
        isActive,
      }: {
        id: string;
        isActive: boolean;
      }) =>
        request<void>(
          `/api/v1/admin/users/${id}/status`,
          {
            method: "PATCH",
            body: JSON.stringify({
              isActive,
            }),
          },
        ),

      onSuccess: () =>
        queryClient.invalidateQueries({
          queryKey: [
            "admin-users",
          ],
        }),
    });

  async function submit(
    values: UserForm,
  ) {
    await createMutation.mutateAsync(
      values,
    );
  }

  return (
    <>
      <AdminPageHeading
        eyebrow="People directory"
        title="People"
        description="Create and manage the Teacher and Student accounts that make up the institution."
        actionLabel="Create account"
        onAction={() =>
          setDrawerOpen(true)
        }
      />

      <div className="admin-toolbar">
        <Input
          className="admin-search"
          placeholder="Search name or email"
          value={search}
          onChange={(event) => {
            setPage(1);
            setSearch(
              event.target.value,
            );
          }}
        />

        <Select
          value={role}
          onChange={(event) => {
            setPage(1);
            setRole(
              event.target.value,
            );
          }}
          aria-label="Filter by role"
        >
          <option value="">
            All roles
          </option>

          <option value="Teacher">
            Teachers
          </option>

          <option value="Student">
            Students
          </option>
        </Select>
      </div>

      {query.isLoading && (
        <p className="muted">
          Loading people…
        </p>
      )}

      {query.error && (
        <AdminError
          message={
            query.error instanceof
            Error
              ? query.error.message
              : "Unable to load people."
          }
        />
      )}

      {query.data &&
        query.data.items.length ===
          0 && (
          <EmptyState
            eyebrow="People"
            title="No accounts found"
            description="Create the first Teacher or Student account for this view."
          />
        )}

      {query.data &&
        query.data.items.length >
          0 && (
          <>
            <Table>
              <thead>
                <tr>
                  <th>Person</th>
                  <th>Role</th>
                  <th>Status</th>
                  <th>Created</th>
                  <th />
                </tr>
              </thead>

              <tbody>
                {query.data.items.map(
                  (user) => (
                    <tr key={user.id}>
                      <td data-label="Person">
                        <span className="table-primary">
                          {
                            user.fullName
                          }
                        </span>

                        <span className="table-secondary">
                          {user.email}
                        </span>
                      </td>

                      <td data-label="Role">
                        {user.roles.join(
                          ", ",
                        )}
                      </td>

                      <td data-label="Status">
                        <StatusBadge
                          active={
                            user.isActive
                          }
                        />
                      </td>

                      <td data-label="Created">
                        {new Date(
                          user.createdAtUtc,
                        ).toLocaleDateString()}
                      </td>

                      <td data-label="Actions">
                        <div className="table-actions">
                          <Button
                            variant="secondary"
                            size="small"
                            disabled={
                              statusMutation.isPending
                            }
                            onClick={() =>
                              statusMutation.mutate(
                                {
                                  id: user.id,
                                  isActive:
                                    !user.isActive,
                                },
                              )
                            }
                          >
                            {user.isActive
                              ? "Deactivate"
                              : "Activate"}
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ),
                )}
              </tbody>
            </Table>

            <Pager
              page={page}
              totalPages={
                query.data
                  .totalPages
              }
              onChange={setPage}
            />
          </>
        )}

      <Drawer
        open={drawerOpen}
        onClose={() =>
          setDrawerOpen(false)
        }
        title="Create account"
      >
        <form
          className="admin-form"
          onSubmit={handleSubmit(
            submit,
          )}
        >
          <div className="form-field">
            <label htmlFor="fullName">
              Full name
            </label>

            <Input
              id="fullName"
              {...register(
                "fullName",
              )}
            />

            {errors.fullName && (
              <p className="form-error">
                {
                  errors.fullName
                    .message
                }
              </p>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="email">
              Email
            </label>

            <Input
              id="email"
              type="email"
              {...register("email")}
            />

            {errors.email && (
              <p className="form-error">
                {
                  errors.email
                    .message
                }
              </p>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="role">
              Role
            </label>

            <Select
              id="role"
              {...register("role")}
            >
              <option value="Teacher">
                Teacher
              </option>

              <option value="Student">
                Student
              </option>
            </Select>
          </div>

          <div className="form-field">
            <label htmlFor="password">
              Temporary password
            </label>

            <Input
              id="password"
              type="password"
              {...register(
                "password",
              )}
            />

            {errors.password && (
              <p className="form-error">
                {
                  errors.password
                    .message
                }
              </p>
            )}
          </div>

          {createMutation.error && (
            <AdminError
              message={
                createMutation.error
                  instanceof Error
                  ? createMutation
                      .error.message
                  : "Unable to create account."
              }
            />
          )}

          <div className="admin-form-actions">
            <Button
              type="button"
              variant="secondary"
              onClick={() =>
                setDrawerOpen(false)
              }
            >
              Cancel
            </Button>

            <Button
              type="submit"
              disabled={
                isSubmitting ||
                createMutation.isPending
              }
            >
              Create account
            </Button>
          </div>
        </form>
      </Drawer>

      {notice && (
        <Toast message={notice} />
      )}
    </>
  );
}

/* =========================================================
   CLASSES / SUBJECTS
   ========================================================= */

const referenceSchema =
  z.object({
    name: z
      .string()
      .trim()
      .min(
        2,
        "Name is required.",
      ),

    code: z
      .string()
      .trim()
      .min(
        2,
        "Code is required.",
      ),

    academicYear:
      z.string().optional(),

    description:
      z.string().optional(),
  });

type ReferenceForm =
  z.infer<typeof referenceSchema>;

export function AdminAcademicPage({
  type,
}: {
  type: "classes" | "subjects";
}) {
  const {
    request,
  } = useAuth();

  const queryClient =
    useQueryClient();

  const [page, setPage] =
    useState(1);

  const [search, setSearch] =
    useState("");

  const [drawerOpen, setDrawerOpen] =
    useState(false);

  const [notice, setNotice] =
    useState<string | null>(null);

  const isClass =
    type === "classes";

  const query =
    useQuery({
      queryKey: [
        `admin-${type}`,
        page,
        search,
      ],

      queryFn: () =>
        request<
          PagedResult<
            AcademicClass | Subject
          >
        >(
          `/api/v1/admin/${type}?page=${page}&pageSize=10&search=${encodeURIComponent(
            search,
          )}`,
        ),
    });

  const {
    register,
    handleSubmit,
    reset,
    formState: {
      errors,
    },
  } = useForm<ReferenceForm>({
    resolver:
      zodResolver(
        referenceSchema,
      ),
  });

  const createMutation =
    useMutation({
      mutationFn: (
        values: ReferenceForm,
      ) => {
        const body = isClass
          ? {
              name:
                values.name,
              code:
                values.code,
              academicYear:
                values.academicYear,
              description:
                values.description,
            }
          : {
              name:
                values.name,
              code:
                values.code,
              description:
                values.description,
            };

        return request(
          `/api/v1/admin/${type}`,
          {
            method: "POST",
            body:
              JSON.stringify(body),
          },
        );
      },

      onSuccess: async () => {
        await queryClient.invalidateQueries({
          queryKey: [
            `admin-${type}`,
          ],
        });

        await queryClient.invalidateQueries({
          queryKey: [
            "admin-dashboard",
          ],
        });

        reset();
        setDrawerOpen(false);

        setNotice(
          isClass
            ? "Class created successfully."
            : "Subject created successfully.",
        );
      },
    });

  const statusMutation =
    useMutation({
      mutationFn: ({
        id,
        isActive,
      }: {
        id: string;
        isActive: boolean;
      }) =>
        request<void>(
          `/api/v1/admin/${type}/${id}/status`,
          {
            method: "PATCH",
            body: JSON.stringify({
              isActive,
            }),
          },
        ),

      onSuccess: () =>
        queryClient.invalidateQueries({
          queryKey: [
            `admin-${type}`,
          ],
        }),
    });

  async function submit(
    values: ReferenceForm,
  ) {
    if (
      isClass &&
      !values.academicYear?.trim()
    ) {
      return;
    }

    await createMutation.mutateAsync(
      values,
    );
  }

  return (
    <>
      <AdminPageHeading
        eyebrow={
          isClass
            ? "Academic structure"
            : "Curriculum"
        }
        title={
          isClass
            ? "Classes"
            : "Subjects"
        }
        description={
          isClass
            ? "Create and maintain the cohorts and class groups used throughout SlateDesk."
            : "Manage the subjects that connect Teachers, classes, assignments, and results."
        }
        actionLabel={
          isClass
            ? "Create class"
            : "Create subject"
        }
        onAction={() =>
          setDrawerOpen(true)
        }
      />

      <div className="admin-toolbar">
        <Input
          className="admin-search"
          placeholder={`Search ${type}`}
          value={search}
          onChange={(event) => {
            setPage(1);
            setSearch(
              event.target.value,
            );
          }}
        />
      </div>

      {query.isLoading && (
        <p className="muted">
          Loading {type}…
        </p>
      )}

      {query.error && (
        <AdminError
          message={
            query.error instanceof
            Error
              ? query.error.message
              : `Unable to load ${type}.`
          }
        />
      )}

      {query.data &&
        query.data.items.length >
          0 && (
          <>
            <Table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Code</th>

                  {isClass && (
                    <th>
                      Academic year
                    </th>
                  )}

                  <th>Status</th>
                  <th />
                </tr>
              </thead>

              <tbody>
                {query.data.items.map(
                  (item) => (
                    <tr key={item.id}>
                      <td data-label="Name">
                        <span className="table-primary">
                          {item.name}
                        </span>

                        {item.description && (
                          <span className="table-secondary">
                            {
                              item.description
                            }
                          </span>
                        )}
                      </td>

                      <td data-label="Code">
                        {item.code}
                      </td>

                      {isClass && (
                        <td data-label="Academic year">
                          {
                            (
                              item as AcademicClass
                            )
                              .academicYear
                          }
                        </td>
                      )}

                      <td data-label="Status">
                        <StatusBadge
                          active={
                            item.isActive
                          }
                        />
                      </td>

                      <td data-label="Actions">
                        <div className="table-actions">
                          <Button
                            variant="secondary"
                            size="small"
                            onClick={() =>
                              statusMutation.mutate(
                                {
                                  id: item.id,
                                  isActive:
                                    !item.isActive,
                                },
                              )
                            }
                          >
                            {item.isActive
                              ? "Deactivate"
                              : "Activate"}
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ),
                )}
              </tbody>
            </Table>

            <Pager
              page={page}
              totalPages={
                query.data
                  .totalPages
              }
              onChange={setPage}
            />
          </>
        )}

      <Drawer
        open={drawerOpen}
        onClose={() =>
          setDrawerOpen(false)
        }
        title={
          isClass
            ? "Create class"
            : "Create subject"
        }
      >
        <form
          className="admin-form"
          onSubmit={handleSubmit(
            submit,
          )}
        >
          <div className="form-field">
            <label>
              Name
            </label>

            <Input
              {...register("name")}
            />

            {errors.name && (
              <p className="form-error">
                {
                  errors.name.message
                }
              </p>
            )}
          </div>

          <div className="form-field">
            <label>
              Code
            </label>

            <Input
              {...register("code")}
            />

            {errors.code && (
              <p className="form-error">
                {
                  errors.code.message
                }
              </p>
            )}
          </div>

          {isClass && (
            <div className="form-field">
              <label>
                Academic year
              </label>

              <Input
                placeholder="2026"
                {...register(
                  "academicYear",
                )}
              />
            </div>
          )}

          <div className="form-field">
            <label>
              Description
            </label>

            <Input
              {...register(
                "description",
              )}
            />
          </div>

          {createMutation.error && (
            <AdminError
              message={
                createMutation.error
                  instanceof Error
                  ? createMutation
                      .error.message
                  : "Unable to save."
              }
            />
          )}

          <div className="admin-form-actions">
            <Button
              type="button"
              variant="secondary"
              onClick={() =>
                setDrawerOpen(false)
              }
            >
              Cancel
            </Button>

            <Button type="submit">
              Create
            </Button>
          </div>
        </form>
      </Drawer>

      {notice && (
        <Toast message={notice} />
      )}
    </>
  );
}

/* =========================================================
   ALLOCATIONS
   ========================================================= */

const allocationSchema =
  z.object({
    teacherId:
      z.string().min(1),
    academicClassId:
      z.string().min(1),
    subjectId:
      z.string().min(1),
  });

type AllocationForm =
  z.infer<
    typeof allocationSchema
  >;

export function AdminAllocationsPage() {
  const {
    request,
  } = useAuth();

  const queryClient =
    useQueryClient();

  const [drawerOpen, setDrawerOpen] =
    useState(false);

  const allocations =
    useQuery({
      queryKey: [
        "admin-allocations",
      ],

      queryFn: () =>
        request<
          PagedResult<
            TeacherAllocation
          >
        >(
          "/api/v1/admin/teacher-allocations?page=1&pageSize=100",
        ),
    });

  const teachers =
    useQuery({
      queryKey: [
        "admin-allocation-teachers",
      ],

      queryFn: () =>
        request<
          PagedResult<AdminUser>
        >(
          "/api/v1/admin/users?page=1&pageSize=100&role=Teacher&isActive=true",
        ),
    });

  const classes =
    useQuery({
      queryKey: [
        "admin-allocation-classes",
      ],

      queryFn: () =>
        request<
          PagedResult<AcademicClass>
        >(
          "/api/v1/admin/classes?page=1&pageSize=100&isActive=true",
        ),
    });

  const subjects =
    useQuery({
      queryKey: [
        "admin-allocation-subjects",
      ],

      queryFn: () =>
        request<
          PagedResult<Subject>
        >(
          "/api/v1/admin/subjects?page=1&pageSize=100&isActive=true",
        ),
    });

  const {
    register,
    handleSubmit,
    reset,
  } = useForm<AllocationForm>({
    resolver:
      zodResolver(
        allocationSchema,
      ),
  });

  const createMutation =
    useMutation({
      mutationFn: (
        values: AllocationForm,
      ) =>
        request<TeacherAllocation>(
          "/api/v1/admin/teacher-allocations",
          {
            method: "POST",
            body:
              JSON.stringify(
                values,
              ),
          },
        ),

      onSuccess: async () => {
        await queryClient.invalidateQueries({
          queryKey: [
            "admin-allocations",
          ],
        });

        reset();
        setDrawerOpen(false);
      },
    });

  const deleteMutation =
    useMutation({
      mutationFn: (
        id: string,
      ) =>
        request<void>(
          `/api/v1/admin/teacher-allocations/${id}`,
          {
            method: "DELETE",
          },
        ),

      onSuccess: () =>
        queryClient.invalidateQueries({
          queryKey: [
            "admin-allocations",
          ],
        }),
    });

  return (
    <>
      <AdminPageHeading
        eyebrow="Teaching structure"
        title="Teacher allocations"
        description="Connect an active Teacher with the class and subject they are responsible for."
        actionLabel="Create allocation"
        onAction={() =>
          setDrawerOpen(true)
        }
      />

      {allocations.error && (
        <AdminError
          message={
            allocations.error
              instanceof Error
              ? allocations.error.message
              : "Unable to load allocations."
          }
        />
      )}

      {allocations.data && (
        <Table>
          <thead>
            <tr>
              <th>Teacher</th>
              <th>Class</th>
              <th>Subject</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>

          <tbody>
            {allocations.data.items.map(
              (item) => (
                <tr key={item.id}>
                  <td data-label="Teacher">
                    <span className="table-primary">
                      {
                        item.teacherName
                      }
                    </span>

                    <span className="table-secondary">
                      {
                        item.teacherEmail
                      }
                    </span>
                  </td>

                  <td data-label="Class">
                    {item.classCode}
                  </td>

                  <td data-label="Subject">
                    {item.subjectCode}
                  </td>

                  <td data-label="Status">
                    <StatusBadge
                      active={
                        item.isActive
                      }
                    />
                  </td>

                  <td data-label="Actions">
                    <div className="table-actions">
                      <Button
                        variant="danger"
                        size="small"
                        onClick={() => {
                          if (
                            window.confirm(
                              "Remove this allocation?",
                            )
                          ) {
                            deleteMutation.mutate(
                              item.id,
                            );
                          }
                        }}
                      >
                        Remove
                      </Button>
                    </div>
                  </td>
                </tr>
              ),
            )}
          </tbody>
        </Table>
      )}

      <Drawer
        open={drawerOpen}
        onClose={() =>
          setDrawerOpen(false)
        }
        title="Create allocation"
      >
        <form
          className="admin-form"
          onSubmit={handleSubmit(
            (values) =>
              createMutation.mutate(
                values,
              ),
          )}
        >
          <div className="form-field">
            <label>
              Teacher
            </label>

            <Select
              {...register(
                "teacherId",
              )}
            >
              <option value="">
                Select Teacher
              </option>

              {teachers.data?.items.map(
                (teacher) => (
                  <option
                    key={teacher.id}
                    value={teacher.id}
                  >
                    {teacher.fullName}
                  </option>
                ),
              )}
            </Select>
          </div>

          <div className="form-field">
            <label>
              Class
            </label>

            <Select
              {...register(
                "academicClassId",
              )}
            >
              <option value="">
                Select class
              </option>

              {classes.data?.items.map(
                (item) => (
                  <option
                    key={item.id}
                    value={item.id}
                  >
                    {item.code} —{" "}
                    {item.name}
                  </option>
                ),
              )}
            </Select>
          </div>

          <div className="form-field">
            <label>
              Subject
            </label>

            <Select
              {...register(
                "subjectId",
              )}
            >
              <option value="">
                Select subject
              </option>

              {subjects.data?.items.map(
                (item) => (
                  <option
                    key={item.id}
                    value={item.id}
                  >
                    {item.code} —{" "}
                    {item.name}
                  </option>
                ),
              )}
            </Select>
          </div>

          {createMutation.error && (
            <AdminError
              message={
                createMutation.error
                  instanceof Error
                  ? createMutation
                      .error.message
                  : "Unable to create allocation."
              }
            />
          )}

          <div className="admin-form-actions">
            <Button
              type="button"
              variant="secondary"
              onClick={() =>
                setDrawerOpen(false)
              }
            >
              Cancel
            </Button>

            <Button type="submit">
              Allocate Teacher
            </Button>
          </div>
        </form>
      </Drawer>
    </>
  );
}

/* =========================================================
   ENROLLMENTS
   ========================================================= */

const enrollmentSchema =
  z.object({
    studentId:
      z.string().min(1),

    academicClassId:
      z.string().min(1),
  });

type EnrollmentForm =
  z.infer<
    typeof enrollmentSchema
  >;

export function AdminEnrollmentsPage() {
  const {
    request,
  } = useAuth();

  const queryClient =
    useQueryClient();

  const [drawerOpen, setDrawerOpen] =
    useState(false);

  const enrollments =
    useQuery({
      queryKey: [
        "admin-enrollments",
      ],

      queryFn: () =>
        request<
          PagedResult<
            StudentEnrollment
          >
        >(
          "/api/v1/admin/enrollments?page=1&pageSize=100",
        ),
    });

  const students =
    useQuery({
      queryKey: [
        "admin-enrollment-students",
      ],

      queryFn: () =>
        request<
          PagedResult<AdminUser>
        >(
          "/api/v1/admin/users?page=1&pageSize=100&role=Student&isActive=true",
        ),
    });

  const classes =
    useQuery({
      queryKey: [
        "admin-enrollment-classes",
      ],

      queryFn: () =>
        request<
          PagedResult<AcademicClass>
        >(
          "/api/v1/admin/classes?page=1&pageSize=100&isActive=true",
        ),
    });

  const {
    register,
    handleSubmit,
    reset,
  } = useForm<EnrollmentForm>({
    resolver:
      zodResolver(
        enrollmentSchema,
      ),
  });

  const createMutation =
    useMutation({
      mutationFn: (
        values: EnrollmentForm,
      ) =>
        request<StudentEnrollment>(
          "/api/v1/admin/enrollments",
          {
            method: "POST",
            body:
              JSON.stringify(
                values,
              ),
          },
        ),

      onSuccess: async () => {
        await queryClient.invalidateQueries({
          queryKey: [
            "admin-enrollments",
          ],
        });

        reset();
        setDrawerOpen(false);
      },
    });

  const statusMutation =
    useMutation({
      mutationFn: ({
        id,
        isActive,
      }: {
        id: string;
        isActive: boolean;
      }) =>
        request<void>(
          `/api/v1/admin/enrollments/${id}/status`,
          {
            method: "PATCH",
            body: JSON.stringify({
              isActive,
            }),
          },
        ),

      onSuccess: () =>
        queryClient.invalidateQueries({
          queryKey: [
            "admin-enrollments",
          ],
        }),
    });

  return (
    <>
      <AdminPageHeading
        eyebrow="Student structure"
        title="Enrollments"
        description="Place each Student into their active academic class."
        actionLabel="Enroll Student"
        onAction={() =>
          setDrawerOpen(true)
        }
      />

      {enrollments.error && (
        <AdminError
          message={
            enrollments.error
              instanceof Error
              ? enrollments.error.message
              : "Unable to load enrollments."
          }
        />
      )}

      {enrollments.data && (
        <Table>
          <thead>
            <tr>
              <th>Student</th>
              <th>Class</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>

          <tbody>
            {enrollments.data.items.map(
              (item) => (
                <tr key={item.id}>
                  <td data-label="Student">
                    <span className="table-primary">
                      {
                        item.studentName
                      }
                    </span>

                    <span className="table-secondary">
                      {
                        item.studentEmail
                      }
                    </span>
                  </td>

                  <td data-label="Class">
                    {item.classCode}
                  </td>

                  <td data-label="Status">
                    <StatusBadge
                      active={
                        item.isActive
                      }
                    />
                  </td>

                  <td data-label="Actions">
                    <div className="table-actions">
                      <Button
                        variant="secondary"
                        size="small"
                        onClick={() =>
                          statusMutation.mutate(
                            {
                              id: item.id,
                              isActive:
                                !item.isActive,
                            },
                          )
                        }
                      >
                        {item.isActive
                          ? "Deactivate"
                          : "Activate"}
                      </Button>
                    </div>
                  </td>
                </tr>
              ),
            )}
          </tbody>
        </Table>
      )}

      <Drawer
        open={drawerOpen}
        onClose={() =>
          setDrawerOpen(false)
        }
        title="Enroll Student"
      >
        <form
          className="admin-form"
          onSubmit={handleSubmit(
            (values) =>
              createMutation.mutate(
                values,
              ),
          )}
        >
          <div className="form-field">
            <label>
              Student
            </label>

            <Select
              {...register(
                "studentId",
              )}
            >
              <option value="">
                Select Student
              </option>

              {students.data?.items.map(
                (student) => (
                  <option
                    key={student.id}
                    value={student.id}
                  >
                    {student.fullName}
                  </option>
                ),
              )}
            </Select>
          </div>

          <div className="form-field">
            <label>
              Class
            </label>

            <Select
              {...register(
                "academicClassId",
              )}
            >
              <option value="">
                Select class
              </option>

              {classes.data?.items.map(
                (item) => (
                  <option
                    key={item.id}
                    value={item.id}
                  >
                    {item.code} —{" "}
                    {item.name}
                  </option>
                ),
              )}
            </Select>
          </div>

          {createMutation.error && (
            <AdminError
              message={
                createMutation.error
                  instanceof Error
                  ? createMutation
                      .error.message
                  : "Unable to enroll Student."
              }
            />
          )}

          <div className="admin-form-actions">
            <Button
              type="button"
              variant="secondary"
              onClick={() =>
                setDrawerOpen(false)
              }
            >
              Cancel
            </Button>

            <Button type="submit">
              Enroll Student
            </Button>
          </div>
        </form>
      </Drawer>
    </>
  );
}