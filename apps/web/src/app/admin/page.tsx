import {
  AppShell,
} from "@/components/app-shell";

import {
  RoleDashboard,
} from "@/components/role-dashboard";

export default function AdminPage() {
  return (
    <AppShell
      role="Admin"
      title="Admin workspace"
      subtitle="Institution overview"
    >
      <RoleDashboard
        role="Admin"
      />
    </AppShell>
  );
}