"use client";

import {
  useEffect,
  useState,
  type ReactNode,
} from "react";

import Link from "next/link";

import {
  ArrowRight,
  CalendarDays,
  FileText,
} from "lucide-react";

import {
  Badge,
  Button,
  Card,
} from "@/components/ui";

import type {
  AssignmentStatus,
  TeacherAssignment,
} from "@/types/teacher";

export function formatTeacherDate(
  value: string | null,
) {
  if (!value) {
    return "—";
  }

  return new Intl.DateTimeFormat(
    undefined,
    {
      dateStyle: "medium",
      timeStyle: "short",
    },
  ).format(new Date(value));
}

export function AssignmentStatusBadge({
  status,
}: {
  status: AssignmentStatus;
}) {
  const tone =
    status === "Published"
      ? "green"
      : status === "Draft"
        ? "blue"
        : status === "Closed"
          ? "amber"
          : "rose";

  return (
    <Badge tone={tone}>
      {status}
    </Badge>
  );
}

export function SubmissionStatusBadge({
  status,
}: {
  status: string;
}) {
  const tone =
    status === "Graded"
      ? "green"
      : status === "Late" ||
          status === "UnderReview" ||
          status === "NeedsRevision"
        ? "amber"
        : status === "Submitted"
          ? "blue"
          : "neutral";

  return (
    <Badge tone={tone}>
      {status}
    </Badge>
  );
}

export function TeacherError({
  message,
}: {
  message: string;
}) {
  return (
    <Card className="teacher-error">
      <strong>
        Something needs attention
      </strong>

      <p>{message}</p>
    </Card>
  );
}

export function TeacherPageHeading({
  eyebrow,
  title,
  description,
  action,
}: {
  eyebrow: string;
  title: string;
  description: string;
  action?: ReactNode;
}) {
  return (
    <header className="page-heading">
      <div>
        <p className="eyebrow">
          {eyebrow}
        </p>

        <h1>{title}</h1>

        <p>{description}</p>
      </div>

      {action}
    </header>
  );
}

export function DeadlineRail({
  publishedAtUtc,
  deadlineUtc,
  isPastDeadline,
}: {
  publishedAtUtc: string | null;
  deadlineUtc: string;
  isPastDeadline: boolean;
}) {
  const [now, setNow] =
    useState<number | null>(null);

  useEffect(() => {
    const updateNow = () => {
      setNow(Date.now());
    };

    const initialTimer =
      window.setTimeout(
        updateNow,
        0,
      );

    const interval =
      window.setInterval(
        updateNow,
        60_000,
      );

    return () => {
      window.clearTimeout(
        initialTimer,
      );

      window.clearInterval(
        interval,
      );
    };
  }, []);

  const deadline =
    new Date(deadlineUtc).getTime();

  const published =
    publishedAtUtc
      ? new Date(
          publishedAtUtc,
        ).getTime()
      : null;

  let progress = 0;

  if (
    published !== null &&
    now !== null
  ) {
    const duration =
      deadline - published;

    progress =
      duration <= 0
        ? 100
        : ((now - published) /
            duration) *
          100;
  }

  progress = Math.min(
    100,
    Math.max(0, progress),
  );

  return (
    <div className="teacher-deadline">
      <div className="teacher-deadline-track">
        <span
          className={
            isPastDeadline
              ? "teacher-deadline-fill overdue"
              : "teacher-deadline-fill"
          }
          style={{
            width: `${progress}%`,
          }}
        />

        <span
          className="teacher-deadline-now"
          style={{
            left: `${progress}%`,
          }}
        />
      </div>

      <div className="teacher-deadline-labels">
        <span>
          {publishedAtUtc
            ? "Published"
            : "Draft"}
        </span>

        <span>
          {isPastDeadline
            ? "Deadline passed"
            : "In progress"}
        </span>

        <span>
          {formatTeacherDate(
            deadlineUtc,
          )}
        </span>
      </div>
    </div>
  );
}

export function AssignmentLedgerCard({
  assignment,
  actionHref,
  actionLabel = "Open assignment",
}: {
  assignment: TeacherAssignment;
  actionHref: string;
  actionLabel?: string;
}) {
  return (
    <Card className="assignment-ledger-card">
      <div className="ledger-card-top">
        <span className="ledger-subject-code">
          {assignment.subjectCode}
        </span>

        <AssignmentStatusBadge
          status={assignment.status}
        />
      </div>

      <h2>
        {assignment.title}
      </h2>

      <p className="ledger-description">
        {assignment.description}
      </p>

      <div className="ledger-information">
        <span>
          <FileText
            size={15}
            aria-hidden="true"
          />

          {assignment.classCode}
        </span>

        <span>
          <CalendarDays
            size={15}
            aria-hidden="true"
          />

          {formatTeacherDate(
            assignment.deadlineUtc,
          )}
        </span>
      </div>

      <DeadlineRail
        publishedAtUtc={
          assignment.publishedAtUtc
        }
        deadlineUtc={
          assignment.deadlineUtc
        }
        isPastDeadline={
          assignment.isPastDeadline
        }
      />

      <div className="ledger-card-footer">
        <div>
          <strong>
            {
              assignment
                .submissionCount
            }
          </strong>

          <span>
            submissions ·{" "}
            {
              assignment.maximumMarks
            }{" "}
            marks
          </span>
        </div>

        <Link href={actionHref}>
          <Button
            variant="secondary"
            size="small"
          >
            {actionLabel}
            <ArrowRight size={15} />
          </Button>
        </Link>
      </div>
    </Card>
  );
}

export function TeacherPager({
  page,
  totalPages,
  onChange,
}: {
  page: number;
  totalPages: number;
  onChange: (
    page: number,
  ) => void;
}) {
  if (totalPages <= 1) {
    return null;
  }

  return (
    <div className="pager">
      <Button
        variant="secondary"
        size="small"
        disabled={page <= 1}
        onClick={() =>
          onChange(page - 1)
        }
      >
        Previous
      </Button>

      <span>
        Page {page} of{" "}
        {totalPages}
      </span>

      <Button
        variant="secondary"
        size="small"
        disabled={
          page >= totalPages
        }
        onClick={() =>
          onChange(page + 1)
        }
      >
        Next
      </Button>
    </div>
  );
}