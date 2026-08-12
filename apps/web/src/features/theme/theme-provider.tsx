"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
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
  const [mode, setModeState] =
    useState<ThemeMode>("light");

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
    const stored =
      window.localStorage.getItem(
        STORAGE_KEY,
      ) as ThemeMode | null;

    const initialMode =
      stored === "light" ||
      stored === "dark" ||
      stored === "system"
        ? stored
        : "light";

    setModeState(initialMode);
    applyTheme(initialMode);
  }, [applyTheme]);

  useEffect(() => {
    if (mode !== "system") {
      return;
    }

    const media = window.matchMedia(
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
  }, [mode, applyTheme]);

  const setMode = useCallback(
    (nextMode: ThemeMode) => {
      setModeState(nextMode);

      window.localStorage.setItem(
        STORAGE_KEY,
        nextMode,
      );

      applyTheme(nextMode);
    },
    [applyTheme],
  );

  const value = useMemo(
    () => ({
      mode,
      setMode,
    }),
    [mode, setMode],
  );

  return (
    <ThemeContext.Provider value={value}>
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