import {
  AppShell,
} from "@/components/app-shell";

import {
  AdminAssignmentsOverview,
} from "@/features/admin/admin-overview-pages";

export default function Page() {
  return (
    <AppShell
      role="Admin"
      title="Assignments"
      subtitle="Institution overview"
    >
      <AdminAssignmentsOverview />
    </AppShell>
  );
}