import {
  AppShell,
} from "@/components/app-shell";

import {
  StudentDashboardView,
} from "@/features/student/student-dashboard";

export default function Page() {
  return (
    <AppShell
      role="Student"
      title="Student workspace"
      subtitle="Assignments and progress"
    >
      <StudentDashboardView />
    </AppShell>
  );
}