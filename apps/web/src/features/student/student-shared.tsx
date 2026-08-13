"use client";

import {
  useEffect,
  useState,
} from "react";

import Link from "next/link";

import {
  ArrowRight,
  CalendarClock,
  GraduationCap,
  UserRound,
} from "lucide-react";

import {
  Badge,
  Button,
  Card,
} from "@/components/ui";

import type {
  StudentAssignment,
  SubmissionStatus,
} from "@/types/student";

export function formatStudentDate(
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

export function StudentPageHeading({
  eyebrow,
  title,
  description,
  action,
}: {
  eyebrow: string;
  title: string;
  description: string;
  action?: React.ReactNode;
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

export function StudentError({
  message,
}: {
  message: string;
}) {
  return (
    <Card className="student-error">
      <strong>
        Something needs attention
      </strong>

      <p>{message}</p>
    </Card>
  );
}

export function StudentSubmissionBadge({
  status,
}: {
  status: SubmissionStatus | null;
}) {
  if (!status) {
    return (
      <Badge tone="neutral">
        Not started
      </Badge>
    );
  }

  const tone =
    status === "Graded"
      ? "green"
      : status === "Late" ||
          status === "NeedsRevision" ||
          status === "UnderReview"
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

export function StudentDeadlineRail({
  publishedAtUtc,
  deadlineUtc,
  isPastDeadline,
  wouldBeLate,
}: {
  publishedAtUtc: string | null;
  deadlineUtc: string;
  isPastDeadline: boolean;
  wouldBeLate?: boolean;
}) {
  const [now, setNow] =
    useState<number | null>(null);

  useEffect(() => {
    const initialTimer =
      window.setTimeout(() => {
        setNow(Date.now());
      }, 0);

    const interval =
      window.setInterval(() => {
        setNow(Date.now());
      }, 60_000);

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

  const start =
    publishedAtUtc
      ? new Date(
          publishedAtUtc,
        ).getTime()
      : null;

  let progress = 0;

  if (
    now !== null &&
    start !== null
  ) {
    const duration =
      deadline - start;

    progress =
      duration <= 0
        ? 100
        : ((now - start) /
            duration) *
          100;
  }

  progress = Math.min(
    100,
    Math.max(0, progress),
  );

  return (
    <div className="student-deadline">
      <div className="student-deadline-track">
        <span
          className={`student-deadline-fill ${
            isPastDeadline
              ? "past"
              : ""
          }`}
          style={{
            width: `${progress}%`,
          }}
        />

        <span
          className="student-deadline-marker"
          style={{
            left: `${progress}%`,
          }}
        />
      </div>

      <div className="student-deadline-labels">
        <span>
          {publishedAtUtc
            ? "Published"
            : "Available"}
        </span>

        <span>
          {wouldBeLate
            ? "Late window"
            : isPastDeadline
              ? "Deadline passed"
              : "Today"}
        </span>

        <span>
          {formatStudentDate(
            deadlineUtc,
          )}
        </span>
      </div>
    </div>
  );
}

export function StudentAssignmentFolio({
  assignment,
}: {
  assignment: StudentAssignment;
}) {
  return (
    <Card className="student-folio">
      <div className="student-folio-edge">
        <span>
          {assignment.subjectCode}
        </span>
      </div>

      <div className="student-folio-body">
        <div className="student-folio-head">
          <span className="ledger-subject-code">
            {assignment.subjectCode}
          </span>

          <StudentSubmissionBadge
            status={
              assignment.submissionStatus
            }
          />
        </div>

        <h2>
          {assignment.title}
        </h2>

        <p>
          {assignment.description}
        </p>

        <div className="student-folio-meta">
          <span>
            <UserRound
              size={15}
              aria-hidden="true"
            />

            {assignment.teacherName}
          </span>

          <span>
            <GraduationCap
              size={15}
              aria-hidden="true"
            />

            {assignment.classCode}
          </span>

          <span>
            <CalendarClock
              size={15}
              aria-hidden="true"
            />

            {assignment.maximumMarks} marks
          </span>
        </div>

        <StudentDeadlineRail
          publishedAtUtc={
            assignment.publishedAtUtc
          }
          deadlineUtc={
            assignment.deadlineUtc
          }
          isPastDeadline={
            assignment.isPastDeadline
          }
          wouldBeLate={
            assignment.wouldBeLate
          }
        />

        <div className="student-folio-footer">
          <span>
            {assignment.canSubmit
              ? assignment.wouldBeLate
                ? "Late submission available"
                : "Submission available"
              : assignment.submissionStatus ===
                  "Graded"
                ? "Feedback available"
                : "Submission window closed"}
          </span>

          <Link
            href={`/student/assignments/${assignment.id}`}
          >
            <Button
              variant="secondary"
              size="small"
            >
              Open
              <ArrowRight size={15} />
            </Button>
          </Link>
        </div>
      </div>
    </Card>
  );
}

export function GradeSeal({
  marks,
  maximumMarks,
}: {
  marks: number;
  maximumMarks: number;
}) {
  const percentage =
    maximumMarks <= 0
      ? 0
      : Math.round(
          (marks /
            maximumMarks) *
            100,
        );

  return (
    <div
      className="grade-seal"
      aria-label={`${marks} out of ${maximumMarks}, graded`}
    >
      <div className="grade-seal-score">
        <strong>
          {marks}
        </strong>

        <span>
          / {maximumMarks}
        </span>
      </div>

      <div className="grade-seal-rule" />

      <span className="grade-seal-label">
        Graded · {percentage}%
      </span>
    </div>
  );
}