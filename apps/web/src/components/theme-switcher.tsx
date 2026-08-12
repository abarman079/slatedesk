"use client";

import {
  Monitor,
  Moon,
  Sun,
} from "lucide-react";

import {
  useTheme,
  type ThemeMode,
} from "@/features/theme/theme-provider";

const options: {
  mode: ThemeMode;
  label: string;
  icon: typeof Sun;
}[] = [
  {
    mode: "light",
    label: "Light theme",
    icon: Sun,
  },
  {
    mode: "system",
    label: "System theme",
    icon: Monitor,
  },
  {
    mode: "dark",
    label: "Dark theme",
    icon: Moon,
  },
];

export function ThemeSwitcher() {
  const {
    mode,
    setMode,
  } = useTheme();

  return (
    <div
      className="theme-switcher"
      aria-label="Theme preference"
    >
      {options.map(
        ({
          mode: optionMode,
          label,
          icon: Icon,
        }) => (
          <button
            key={optionMode}
            type="button"
            className={`theme-option ${
              mode === optionMode
                ? "active"
                : ""
            }`}
            onClick={() =>
              setMode(optionMode)
            }
            aria-label={label}
            title={label}
            aria-pressed={
              mode === optionMode
            }
          >
            <Icon size={16} />
          </button>
        ),
      )}
    </div>
  );
}