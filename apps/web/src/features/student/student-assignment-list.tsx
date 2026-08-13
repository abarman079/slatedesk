"use client";

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
} from "@/components/ui";

import {
  StudentAssignmentFolio,
  StudentError,
  StudentPageHeading,
} from "@/features/student/student-shared";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import type {
  PagedResult,
  StudentAssignment,
} from "@/types/student";

export function StudentAssignmentList() {
  const {
    request,
  } = useAuth();

  const [page, setPage] =
    useState(1);

  const [search, setSearch] =
    useState("");

  const query =
    useQuery({
      queryKey: [
        "student-assignment-list",
        page,
        search,
      ],

      queryFn: () =>
        request<
          PagedResult<
            StudentAssignment
          >
        >(
          `/api/v1/student/assignments?page=${page}&pageSize=12&search=${encodeURIComponent(
            search,
          )}`,
        ),
    });

  return (
    <>
      <StudentPageHeading
        eyebrow="Assignment ledger"
        title="Assignments"
        description="Published academic work for your active class, ordered around the deadlines that matter."
      />

      <div className="admin-toolbar">
        <Input
          className="admin-search"
          placeholder="Search title or subject"
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
          Loading assignments…
        </p>
      )}

      {query.error && (
        <StudentError
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
            title="No assignments found"
            description="There are no published assignments matching this view."
          />
        )}

      {query.data &&
        query.data.items.length >
          0 && (
          <>
            <div className="student-folio-grid">
              {query.data.items.map(
                (assignment) => (
                  <StudentAssignmentFolio
                    key={
                      assignment.id
                    }
                    assignment={
                      assignment
                    }
                  />
                ),
              )}
            </div>

            {query.data.totalPages >
              1 && (
              <div className="pager">
                <Button
                  variant="secondary"
                  size="small"
                  disabled={
                    page <= 1
                  }
                  onClick={() =>
                    setPage(
                      (value) =>
                        value - 1,
                    )
                  }
                >
                  Previous
                </Button>

                <span>
                  Page {page} of{" "}
                  {
                    query.data
                      .totalPages
                  }
                </span>

                <Button
                  variant="secondary"
                  size="small"
                  disabled={
                    page >=
                    query.data
                      .totalPages
                  }
                  onClick={() =>
                    setPage(
                      (value) =>
                        value + 1,
                    )
                  }
                >
                  Next
                </Button>
              </div>
            )}
          </>
        )}
    </>
  );
}