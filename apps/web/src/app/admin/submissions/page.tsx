import {
  AppShell,
} from "@/components/app-shell";

import {
  AdminSubmissionsOverview,
} from "@/features/admin/admin-overview-pages";

export default function Page() {
  return (
    <AppShell
      role="Admin"
      title="Submissions"
      subtitle="Institution overview"
    >
      <AdminSubmissionsOverview />
    </AppShell>
  );
}