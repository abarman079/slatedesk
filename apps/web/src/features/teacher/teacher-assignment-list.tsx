"use client";

import Link from "next/link";

import {
  Plus,
} from "lucide-react";

import {
  useState,
} from "react";

import {
  useQuery,
} from "@tanstack/react-query";

import {
  Button,
  EmptyState,
  Input,
  Select,
} from "@/components/ui";

import {
  AssignmentLedgerCard,
  TeacherError,
  TeacherPageHeading,
  TeacherPager,
} from "@/features/teacher/teacher-shared";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import type {
  PagedResult,
  TeacherAssignment,
} from "@/types/teacher";

export function TeacherAssignmentList() {
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
        "teacher-assignments",
        page,
        search,
        status,
      ],

      queryFn: () =>
        request<
          PagedResult<
            TeacherAssignment
          >
        >(
          `/api/v1/teacher/assignments?page=${page}&pageSize=12&search=${encodeURIComponent(
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
      <TeacherPageHeading
        eyebrow="Assignment ledger"
        title="Assignments"
        description="Draft, publish, close, and review the academic work attached to your active teaching allocations."
        action={
          <Link href="/teacher/assignments/new">
            <Button>
              <Plus size={17} />
              New assignment
            </Button>
          </Link>
        }
      />

      <div className="admin-toolbar">
        <Input
          className="admin-search"
          placeholder="Search title, subject, or class"
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
          aria-label="Filter assignment status"
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
        </Select>
      </div>

      {query.isLoading && (
        <p className="muted">
          Loading assignments…
        </p>
      )}

      {query.error && (
        <TeacherError
          message={
            query.error instanceof Error
              ? query.error.message
              : "Unable to load assignments."
          }
        />
      )}

      {query.data &&
        query.data.items.length ===
          0 && (
          <EmptyState
            eyebrow="Assignment ledger"
            title="Nothing matches this view"
            description="Adjust the filter or create a new assignment."
          />
        )}

      {query.data &&
        query.data.items.length >
          0 && (
          <>
            <div className="assignment-ledger-grid">
              {query.data.items.map(
                (assignment) => (
                  <AssignmentLedgerCard
                    key={
                      assignment.id
                    }
                    assignment={
                      assignment
                    }
                    actionHref={`/teacher/assignments/${assignment.id}`}
                  />
                ),
              )}
            </div>

            <TeacherPager
              page={page}
              totalPages={
                query.data
                  .totalPages
              }
              onChange={setPage}
            />
          </>
        )}
    </>
  );
}