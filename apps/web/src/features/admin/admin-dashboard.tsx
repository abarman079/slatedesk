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
} from "@/components/ui";

import {
  AdminError,
  formatDate,
} from "@/features/admin/admin-shared";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import type {
  AdminDashboard,
} from "@/types/admin";

export function AdminDashboardView() {
  const {
    request,
  } = useAuth();

  const query =
    useQuery({
      queryKey: [
        "admin-dashboard",
      ],

      queryFn: () =>
        request<AdminDashboard>(
          "/api/v1/admin/dashboard",
        ),
    });

  if (query.isLoading) {
    return (
      <p className="muted">
        Loading institution overview…
      </p>
    );
  }

  if (query.error) {
    return (
      <AdminError
        message={
          query.error instanceof Error
            ? query.error.message
            : "Unable to load dashboard."
        }
      />
    );
  }

  if (!query.data) {
    return null;
  }

  const stats = [
    [
      "Teachers",
      query.data.activeTeachers,
    ],
    [
      "Students",
      query.data.activeStudents,
    ],
    [
      "Classes",
      query.data.activeClasses,
    ],
    [
      "Subjects",
      query.data.activeSubjects,
    ],
    [
      "Published work",
      query.data.publishedAssignments,
    ],
    [
      "Submissions",
      query.data.totalSubmissions,
    ],
  ] as const;

  return (
    <>
      <header className="page-heading">
        <div>
          <p className="eyebrow">
            Institution control
          </p>

          <h1>
            Academic operations,
            at a glance.
          </h1>

          <p>
            People, structure,
            assignments, submissions,
            and recent administrative
            activity in one workspace.
          </p>
        </div>

        <Link href="/admin/users">
          <Button>
            Manage people
            <ArrowRight size={17} />
          </Button>
        </Link>
      </header>

      <section
        className="admin-dashboard-grid"
        aria-label="Institution summary"
      >
        {stats.map(
          ([label, value]) => (
            <Card
              className="admin-stat"
              key={label}
            >
              <div className="overview-label">
                {label}
              </div>

              <div className="admin-stat-value">
                {value}
              </div>
            </Card>
          ),
        )}
      </section>

      <Card
        style={{
          marginTop: 18,
        }}
      >
        <p className="eyebrow">
          Recent activity
        </p>

        <div className="activity-list">
          {query.data.recentActivity.map(
            (activity) => (
              <div
                key={activity.id}
                className="activity-row"
              >
                <span
                  className="activity-dot"
                  aria-hidden="true"
                />

                <div>
                  <p className="table-primary">
                    {activity.action}
                  </p>

                  <p className="activity-description">
                    {
                      activity.description
                    }
                  </p>
                </div>

                <span className="activity-time">
                  {formatDate(
                    activity.createdAtUtc,
                  )}
                </span>
              </div>
            ),
          )}
        </div>
      </Card>
    </>
  );
}