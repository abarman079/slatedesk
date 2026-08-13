import {
  AppShell,
} from "@/components/app-shell";

import {
  AdminSettingsPage,
} from "@/features/admin/admin-settings";

export default function Page() {
  return (
    <AppShell
      role="Admin"
      title="Settings"
      subtitle="Institution preferences"
    >
      <AdminSettingsPage />
    </AppShell>
  );
}