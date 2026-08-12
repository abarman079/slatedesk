"use client";

import {
  useEffect,
  useRef,
  type ReactNode,
} from "react";

import {
  X,
} from "lucide-react";

function getFocusableElements(
  root: HTMLElement,
) {
  return Array.from(
    root.querySelectorAll<HTMLElement>(
      [
        "a[href]",
        "button:not([disabled])",
        "input:not([disabled])",
        "select:not([disabled])",
        "textarea:not([disabled])",
        "[tabindex]:not([tabindex='-1'])",
      ].join(","),
    ),
  );
}

function OverlayShell({
  open,
  onClose,
  title,
  children,
  type,
}: {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  type: "dialog" | "drawer";
}) {
  const panelRef =
    useRef<HTMLDivElement>(null);

  const previousFocus =
    useRef<HTMLElement | null>(
      null,
    );

  useEffect(() => {
    if (!open) {
      return;
    }

    previousFocus.current =
      document.activeElement instanceof HTMLElement
        ? document.activeElement
        : null;

    const panel = panelRef.current;

    if (!panel) {
      return;
    }

    const focusable =
      getFocusableElements(panel);

    focusable[0]?.focus();

    const handleKeyDown = (
      event: KeyboardEvent,
    ) => {
      if (event.key === "Escape") {
        event.preventDefault();
        onClose();
        return;
      }

      if (event.key !== "Tab") {
        return;
      }

      const items =
        getFocusableElements(panel);

      if (items.length === 0) {
        return;
      }

      const first = items[0];
      const last =
        items[items.length - 1];

      if (
        event.shiftKey &&
        document.activeElement === first
      ) {
        event.preventDefault();
        last.focus();
      } else if (
        !event.shiftKey &&
        document.activeElement === last
      ) {
        event.preventDefault();
        first.focus();
      }
    };

    document.addEventListener(
      "keydown",
      handleKeyDown,
    );

    const previousOverflow =
      document.body.style.overflow;

    document.body.style.overflow =
      "hidden";

    return () => {
      document.removeEventListener(
        "keydown",
        handleKeyDown,
      );

      document.body.style.overflow =
        previousOverflow;

      previousFocus.current?.focus();
    };
  }, [open, onClose]);

  if (!open) {
    return null;
  }

  return (
    <div
      className="overlay-backdrop"
      role="presentation"
      onMouseDown={(event) => {
        if (
          event.target ===
          event.currentTarget
        ) {
          onClose();
        }
      }}
    >
      <div
        ref={panelRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby={`${type}-title`}
        className={
          type === "drawer"
            ? "overlay-panel overlay-drawer"
            : "overlay-panel overlay-dialog"
        }
      >
        <div className="overlay-header">
          <h2 id={`${type}-title`}>
            {title}
          </h2>

          <button
            type="button"
            className="icon-button"
            aria-label={`Close ${title}`}
            title="Close"
            onClick={onClose}
          >
            <X size={19} />
          </button>
        </div>

        <div className="overlay-content">
          {children}
        </div>
      </div>
    </div>
  );
}

export function Dialog(
  props: Omit<
    Parameters<typeof OverlayShell>[0],
    "type"
  >,
) {
  return (
    <OverlayShell
      {...props}
      type="dialog"
    />
  );
}

export function Drawer(
  props: Omit<
    Parameters<typeof OverlayShell>[0],
    "type"
  >,
) {
  return (
    <OverlayShell
      {...props}
      type="drawer"
    />
  );
}