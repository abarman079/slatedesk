import {
  AppShell,
} from "@/components/app-shell";

import {
  RoleDashboard,
} from "@/components/role-dashboard";

export default function TeacherPage() {
  return (
    <AppShell
      role="Teacher"
      title="Teacher workspace"
      subtitle="Assignments and review"
    >
      <RoleDashboard
        role="Teacher"
      />
    </AppShell>
  );
}