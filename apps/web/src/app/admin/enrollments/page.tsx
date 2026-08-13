import {
  AppShell,
} from "@/components/app-shell";

import {
  AdminEnrollmentsPage,
} from "@/features/admin/admin-setup-pages";

export default function Page() {
  return (
    <AppShell
      role="Admin"
      title="Enrollments"
      subtitle="Student structure"
    >
      <AdminEnrollmentsPage />
    </AppShell>
  );
}