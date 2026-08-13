import {
  AppShell,
} from "@/components/app-shell";

import {
  AdminUsersPage,
} from "@/features/admin/admin-setup-pages";

export default function Page() {
  return (
    <AppShell
      role="Admin"
      title="People"
      subtitle="Accounts and access"
    >
      <AdminUsersPage />
    </AppShell>
  );
}