import {
  AppShell,
} from "@/components/app-shell";

import {
  StudentResultsPage,
} from "@/features/student/student-results";

export default function Page() {
  return (
    <AppShell
      role="Student"
      title="Results"
      subtitle="Marks and feedback"
    >
      <StudentResultsPage />
    </AppShell>
  );
}