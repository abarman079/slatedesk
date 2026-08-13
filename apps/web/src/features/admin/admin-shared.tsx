"use client";

import {
  AlertCircle,
  ChevronLeft,
  ChevronRight,
  Plus,
} from "lucide-react";

import {
  Badge,
  Button,
  Card,
} from "@/components/ui";

export function AdminPageHeading({
  eyebrow,
  title,
  description,
  actionLabel,
  onAction,
}: {
  eyebrow: string;
  title: string;
  description: string;
  actionLabel?: string;
  onAction?: () => void;
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

      {actionLabel && onAction && (
        <Button
          onClick={onAction}
        >
          <Plus size={17} />
          {actionLabel}
        </Button>
      )}
    </header>
  );
}

export function StatusBadge({
  active,
}: {
  active: boolean;
}) {
  return (
    <Badge
      tone={
        active
          ? "green"
          : "neutral"
      }
    >
      {active
        ? "Active"
        : "Inactive"}
    </Badge>
  );
}

export function WorkflowBadge({
  status,
}: {
  status: string;
}) {
  const tone =
    status === "Graded" ||
    status === "Published"
      ? "green"
      : status === "Late" ||
          status === "NeedsRevision" ||
          status === "UnderReview"
        ? "amber"
        : status === "Archived"
          ? "rose"
          : "neutral";

  return (
    <Badge tone={tone}>
      {status}
    </Badge>
  );
}

export function AdminError({
  message,
}: {
  message: string;
}) {
  return (
    <Card className="admin-error">
      <AlertCircle
        size={20}
        aria-hidden="true"
      />

      <div>
        <strong>
          Something needs attention
        </strong>

        <p>{message}</p>
      </div>
    </Card>
  );
}

export function Pager({
  page,
  totalPages,
  onChange,
}: {
  page: number;
  totalPages: number;
  onChange: (
    nextPage: number,
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
        <ChevronLeft size={16} />
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
        <ChevronRight size={16} />
      </Button>
    </div>
  );
}

export function formatDate(
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