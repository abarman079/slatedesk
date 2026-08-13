import {
  AppShell,
} from "@/components/app-shell";

import {
  TeacherAssignmentList,
} from "@/features/teacher/teacher-assignment-list";

export default function Page() {
  return (
    <AppShell
      role="Teacher"
      title="Assignments"
      subtitle="Academic work"
    >
      <TeacherAssignmentList />
    </AppShell>
  );
}