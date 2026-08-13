import {
  AppShell,
} from "@/components/app-shell";

import {
  StudentAssignmentList,
} from "@/features/student/student-assignment-list";

export default function Page() {
  return (
    <AppShell
      role="Student"
      title="Assignments"
      subtitle="Academic work"
    >
      <StudentAssignmentList />
    </AppShell>
  );
}