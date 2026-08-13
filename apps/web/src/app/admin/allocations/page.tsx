import {
  AppShell,
} from "@/components/app-shell";

import {
  AdminAllocationsPage,
} from "@/features/admin/admin-setup-pages";

export default function Page() {
  return (
    <AppShell
      role="Admin"
      title="Teacher allocations"
      subtitle="Teaching structure"
    >
      <AdminAllocationsPage />
    </AppShell>
  );
}