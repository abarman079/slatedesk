"use client";

import {
  forwardRef,
  type ButtonHTMLAttributes,
  type HTMLAttributes,
  type InputHTMLAttributes,
  type SelectHTMLAttributes,
  type TextareaHTMLAttributes,
} from "react";

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: "primary" | "secondary" | "ghost" | "danger";
  size?: "default" | "small";
};

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  function Button(
    { className = "", variant = "primary", size = "default", ...props },
    ref,
  ) {
    return (
      <button
        ref={ref}
        className={[
          "ui-button",
          `ui-button-${variant}`,
          `ui-button-${size}`,
          className,
        ].join(" ")}
        {...props}
      />
    );
  },
);

export const Input = forwardRef<
  HTMLInputElement,
  InputHTMLAttributes<HTMLInputElement>
>(function Input({ className = "", ...props }, ref) {
  return <input ref={ref} className={`ui-input ${className}`} {...props} />;
});

export const Select = forwardRef<
  HTMLSelectElement,
  SelectHTMLAttributes<HTMLSelectElement>
>(function Select({ className = "", ...props }, ref) {
  return <select ref={ref} className={`ui-input ${className}`} {...props} />;
});

export const Textarea = forwardRef<
  HTMLTextAreaElement,
  TextareaHTMLAttributes<HTMLTextAreaElement>
>(function Textarea({ className = "", ...props }, ref) {
  return (
    <textarea
      ref={ref}
      className={`ui-input ui-textarea ${className}`}
      {...props}
    />
  );
});

export function Card({
  className = "",
  ...props
}: HTMLAttributes<HTMLDivElement>) {
  return <div className={`ui-card ${className}`} {...props} />;
}

export function Badge({
  children,
  tone = "neutral",
}: {
  children: React.ReactNode;
  tone?: "neutral" | "blue" | "green" | "amber" | "rose";
}) {
  return <span className={`ui-badge ui-badge-${tone}`}>{children}</span>;
}

export function Skeleton({ className = "" }: { className?: string }) {
  return <span aria-hidden="true" className={`ui-skeleton ${className}`} />;
}

export function EmptyState({
  eyebrow,
  title,
  description,
  action,
}: {
  eyebrow?: string;
  title: string;
  description: string;
  action?: React.ReactNode;
}) {
  return (
    <div className="empty-state">
      <div className="empty-state-mark" aria-hidden="true">
        <span />
        <span />
        <span />
      </div>

      {eyebrow && <p className="eyebrow">{eyebrow}</p>}

      <h2>{title}</h2>

      <p>{description}</p>

      {action && <div className="empty-state-action">{action}</div>}
    </div>
  );
}

export function Toast({
  message,
  tone = "success",
}: {
  message: string;
  tone?: "success" | "error";
}) {
  return (
    <div role="status" aria-live="polite" className={`toast toast-${tone}`}>
      {message}
    </div>
  );
}

export function Table({ children }: { children: React.ReactNode }) {
  return (
    <div className="table-shell">
      <table className="data-table">{children}</table>
    </div>
  );
}
