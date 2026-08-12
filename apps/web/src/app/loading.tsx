import {
  DashboardSkeleton,
} from "@/components/skeleton-patterns";

export default function Loading() {
  return (
    <main
      id="main-content"
      className="app-content"
    >
      <DashboardSkeleton />
    </main>
  );
}