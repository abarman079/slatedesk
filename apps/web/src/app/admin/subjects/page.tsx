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
      title="Subjects"
      subtitle="Curriculum structure"
    >
      <AdminAcademicPage
        type="subjects"
      />
    </AppShell>
  );
}