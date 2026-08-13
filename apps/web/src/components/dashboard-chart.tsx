"use client";

import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

import {
  useReducedMotion,
} from "@/hooks/use-reduced-motion";

export type DashboardChartDatum = {
  label: string;
  value: number;
};

export function DashboardChart({
  data,
  ariaLabel,
}: {
  data: DashboardChartDatum[];
  ariaLabel: string;
}) {
  const reducedMotion =
    useReducedMotion();

  const summary =
    data
      .map(
        (item) =>
          `${item.label}: ${item.value}`,
      )
      .join(", ");

  return (
    <figure
      className="dashboard-chart-shell"
      aria-label={ariaLabel}
    >
      <figcaption className="sr-only">
        {summary}
      </figcaption>

      <ResponsiveContainer
        width="100%"
        height="100%"
      >
        <BarChart
          data={data}
          accessibilityLayer
          margin={{
            top: 8,
            right: 8,
            bottom: 0,
            left: -12,
          }}
        >
          <CartesianGrid
            vertical={false}
            stroke="var(--border-soft)"
            strokeDasharray="3 3"
          />

          <XAxis
            dataKey="label"
            axisLine={false}
            tickLine={false}
            tick={{
              fill: "var(--ink-soft)",
              fontSize: 11,
            }}
          />

          <YAxis
            allowDecimals={false}
            axisLine={false}
            tickLine={false}
            width={34}
            tick={{
              fill: "var(--ink-soft)",
              fontSize: 11,
            }}
          />

          <Tooltip
            cursor={{
              fill: "var(--cobalt-soft)",
            }}
            contentStyle={{
              border:
                "1px solid var(--border)",
              borderRadius: "10px",
              background:
                "var(--surface)",
              color: "var(--ink)",
              boxShadow:
                "0 10px 24px rgba(20, 32, 51, 0.08)",
            }}
          />

          <Bar
            dataKey="value"
            fill="var(--cobalt)"
            maxBarSize={46}
            radius={[
              7,
              7,
              2,
              2,
            ]}
            isAnimationActive={
              !reducedMotion
            }
            animationDuration={240}
          />
        </BarChart>
      </ResponsiveContainer>
    </figure>
  );
}