import {
  AppShell,
} from "@/components/app-shell";

import {
  TeacherDashboardView,
} from "@/features/teacher/teacher-dashboard";

export default function Page() {
  return (
    <AppShell
      role="Teacher"
      title="Teacher workspace"
      subtitle="Assignments and review"
    >
      <TeacherDashboardView />
    </AppShell>
  );
}