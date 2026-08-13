"use client";

import {
  useState,
} from "react";

import {
  useQuery,
} from "@tanstack/react-query";

import {
  Input,
  Select,
  Table,
} from "@/components/ui";

import {
  AdminError,
  AdminPageHeading,
  Pager,
  WorkflowBadge,
  formatDate,
} from "@/features/admin/admin-shared";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import type {
  AdminAssignmentOverview,
  AdminSubmissionOverview,
  PagedResult,
} from "@/types/admin";

export function AdminAssignmentsOverview() {
  const {
    request,
  } = useAuth();

  const [page, setPage] =
    useState(1);

  const [search, setSearch] =
    useState("");

  const [status, setStatus] =
    useState("");

  const query =
    useQuery({
      queryKey: [
        "admin-assignment-overview",
        page,
        search,
        status,
      ],

      queryFn: () =>
        request<
          PagedResult<
            AdminAssignmentOverview
          >
        >(
          `/api/v1/admin/assignments?page=${page}&pageSize=20&search=${encodeURIComponent(
            search,
          )}${
            status
              ? `&status=${status}`
              : ""
          }`,
        ),
    });

  return (
    <>
      <AdminPageHeading
        eyebrow="Academic oversight"
        title="Assignments"
        description="A read-only institution-wide view of assignment activity across all Teachers and classes."
      />

      <div className="admin-toolbar">
        <Input
          className="admin-search"
          placeholder="Search assignments"
          value={search}
          onChange={(event) => {
            setPage(1);
            setSearch(
              event.target.value,
            );
          }}
        />

        <Select
          value={status}
          onChange={(event) => {
            setPage(1);
            setStatus(
              event.target.value,
            );
          }}
        >
          <option value="">
            All statuses
          </option>

          <option value="Draft">
            Draft
          </option>

          <option value="Published">
            Published
          </option>

          <option value="Closed">
            Closed
          </option>

          <option value="Archived">
            Archived
          </option>
        </Select>
      </div>

      {query.error && (
        <AdminError
          message={
            query.error instanceof Error
              ? query.error.message
              : "Unable to load assignments."
          }
        />
      )}

      {query.data && (
        <>
          <Table>
            <thead>
              <tr>
                <th>Assignment</th>
                <th>Teacher</th>
                <th>Class</th>
                <th>Status</th>
                <th>Deadline</th>
                <th>Submissions</th>
              </tr>
            </thead>

            <tbody>
              {query.data.items.map(
                (item) => (
                  <tr key={item.id}>
                    <td data-label="Assignment">
                      <span className="table-primary">
                        {item.title}
                      </span>

                      <span className="table-secondary">
                        {
                          item.subjectCode
                        }{" "}
                        ·{" "}
                        {
                          item.maximumMarks
                        }{" "}
                        marks
                      </span>
                    </td>

                    <td data-label="Teacher">
                      {item.teacherName}
                    </td>

                    <td data-label="Class">
                      {item.classCode}
                    </td>

                    <td data-label="Status">
                      <WorkflowBadge
                        status={
                          item.status
                        }
                      />
                    </td>

                    <td data-label="Deadline">
                      {formatDate(
                        item.deadlineUtc,
                      )}
                    </td>

                    <td data-label="Submissions">
                      {
                        item.submissionCount
                      }
                    </td>
                  </tr>
                ),
              )}
            </tbody>
          </Table>

          <Pager
            page={page}
            totalPages={
              query.data.totalPages
            }
            onChange={setPage}
          />
        </>
      )}
    </>
  );
}

export function AdminSubmissionsOverview() {
  const {
    request,
  } = useAuth();

  const [page, setPage] =
    useState(1);

  const [search, setSearch] =
    useState("");

  const [status, setStatus] =
    useState("");

  const query =
    useQuery({
      queryKey: [
        "admin-submission-overview",
        page,
        search,
        status,
      ],

      queryFn: () =>
        request<
          PagedResult<
            AdminSubmissionOverview
          >
        >(
          `/api/v1/admin/submissions?page=${page}&pageSize=20&search=${encodeURIComponent(
            search,
          )}${
            status
              ? `&status=${status}`
              : ""
          }`,
        ),
    });

  return (
    <>
      <AdminPageHeading
        eyebrow="Institution review"
        title="Submissions"
        description="Monitor submission and grading activity across the institution without entering the Teacher grading workflow."
      />

      <div className="admin-toolbar">
        <Input
          className="admin-search"
          placeholder="Search submissions"
          value={search}
          onChange={(event) => {
            setPage(1);
            setSearch(
              event.target.value,
            );
          }}
        />

        <Select
          value={status}
          onChange={(event) => {
            setPage(1);
            setStatus(
              event.target.value,
            );
          }}
        >
          <option value="">
            All statuses
          </option>

          <option value="Draft">
            Draft
          </option>

          <option value="Submitted">
            Submitted
          </option>

          <option value="Late">
            Late
          </option>

          <option value="UnderReview">
            Under review
          </option>

          <option value="NeedsRevision">
            Needs revision
          </option>

          <option value="Graded">
            Graded
          </option>
        </Select>
      </div>

      {query.error && (
        <AdminError
          message={
            query.error instanceof Error
              ? query.error.message
              : "Unable to load submissions."
          }
        />
      )}

      {query.data && (
        <>
          <Table>
            <thead>
              <tr>
                <th>Student</th>
                <th>Assignment</th>
                <th>Teacher</th>
                <th>Status</th>
                <th>Submitted</th>
                <th>Grade</th>
              </tr>
            </thead>

            <tbody>
              {query.data.items.map(
                (item) => (
                  <tr key={item.id}>
                    <td data-label="Student">
                      {
                        item.studentName
                      }
                    </td>

                    <td data-label="Assignment">
                      <span className="table-primary">
                        {
                          item.assignmentTitle
                        }
                      </span>
                    </td>

                    <td data-label="Teacher">
                      {
                        item.teacherName
                      }
                    </td>

                    <td data-label="Status">
                      <WorkflowBadge
                        status={
                          item.status
                        }
                      />
                    </td>

                    <td data-label="Submitted">
                      {formatDate(
                        item.submittedAtUtc,
                      )}
                    </td>

                    <td data-label="Grade">
                      {item.marksAwarded ===
                      null
                        ? "—"
                        : `${item.marksAwarded} / ${item.maximumMarks}`}
                    </td>
                  </tr>
                ),
              )}
            </tbody>
          </Table>

          <Pager
            page={page}
            totalPages={
              query.data.totalPages
            }
            onChange={setPage}
          />
        </>
      )}
    </>
  );
}