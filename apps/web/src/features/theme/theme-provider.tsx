"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useSyncExternalStore,
  type ReactNode,
} from "react";

export type ThemeMode =
  | "light"
  | "dark"
  | "system";

type ThemeContextValue = {
  mode: ThemeMode;
  setMode: (mode: ThemeMode) => void;
};

const ThemeContext =
  createContext<ThemeContextValue | null>(
    null,
  );

const STORAGE_KEY = "slatedesk-theme";

const THEME_CHANGE_EVENT =
  "slatedesk-theme-change";

function isThemeMode(
  value: string | null,
): value is ThemeMode {
  return (
    value === "light" ||
    value === "dark" ||
    value === "system"
  );
}

function getThemeSnapshot(): ThemeMode {
  const stored =
    window.localStorage.getItem(
      STORAGE_KEY,
    );

  return isThemeMode(stored)
    ? stored
    : "light";
}

function getServerThemeSnapshot():
  ThemeMode {
  return "light";
}

function subscribeToTheme(
  callback: () => void,
) {
  function handleStorage(
    event: StorageEvent,
  ) {
    if (
      event.key === STORAGE_KEY
    ) {
      callback();
    }
  }

  function handleThemeChange() {
    callback();
  }

  window.addEventListener(
    "storage",
    handleStorage,
  );

  window.addEventListener(
    THEME_CHANGE_EVENT,
    handleThemeChange,
  );

  return () => {
    window.removeEventListener(
      "storage",
      handleStorage,
    );

    window.removeEventListener(
      THEME_CHANGE_EVENT,
      handleThemeChange,
    );
  };
}

function resolveTheme(
  mode: ThemeMode,
): "light" | "dark" {
  if (mode !== "system") {
    return mode;
  }

  return window.matchMedia(
    "(prefers-color-scheme: dark)",
  ).matches
    ? "dark"
    : "light";
}

export function ThemeProvider({
  children,
}: {
  children: ReactNode;
}) {
  const mode =
    useSyncExternalStore(
      subscribeToTheme,
      getThemeSnapshot,
      getServerThemeSnapshot,
    );

  const applyTheme = useCallback(
    (nextMode: ThemeMode) => {
      const resolved =
        resolveTheme(nextMode);

      document.documentElement.dataset.theme =
        resolved;

      document.documentElement.dataset.themeMode =
        nextMode;
    },
    [],
  );

  useEffect(() => {
    applyTheme(mode);

    if (mode !== "system") {
      return;
    }

    const media =
      window.matchMedia(
        "(prefers-color-scheme: dark)",
      );

    const listener = () => {
      applyTheme("system");
    };

    media.addEventListener(
      "change",
      listener,
    );

    return () =>
      media.removeEventListener(
        "change",
        listener,
      );
  }, [
    mode,
    applyTheme,
  ]);

  const setMode = useCallback(
    (nextMode: ThemeMode) => {
      window.localStorage.setItem(
        STORAGE_KEY,
        nextMode,
      );

      applyTheme(nextMode);

      window.dispatchEvent(
        new Event(
          THEME_CHANGE_EVENT,
        ),
      );
    },
    [applyTheme],
  );

  const value = useMemo(
    () => ({
      mode,
      setMode,
    }),
    [
      mode,
      setMode,
    ],
  );

  return (
    <ThemeContext.Provider
      value={value}
    >
      {children}
    </ThemeContext.Provider>
  );
}

export function useTheme() {
  const context =
    useContext(ThemeContext);

  if (!context) {
    throw new Error(
      "useTheme must be used inside ThemeProvider.",
    );
  }

  return context;
}