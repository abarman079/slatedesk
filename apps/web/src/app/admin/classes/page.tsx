import {
  AppShell,
} from "@/components/app-shell";

import {
  AdminAcademicPage,
} from "@/features/admin/admin-setup-pages";

export default function Page() {
  return (
    <AppShell
      role="Admin"
      title="Classes"
      subtitle="Academic structure"
    >
      <AdminAcademicPage
        type="classes"
      />
    </AppShell>
  );
}