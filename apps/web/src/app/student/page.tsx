import {
  AppShell,
} from "@/components/app-shell";

import {
  RoleDashboard,
} from "@/components/role-dashboard";

export default function StudentPage() {
  return (
    <AppShell
      role="Student"
      title="Student workspace"
      subtitle="Assignments and results"
    >
      <RoleDashboard
        role="Student"
      />
    </AppShell>
  );
}