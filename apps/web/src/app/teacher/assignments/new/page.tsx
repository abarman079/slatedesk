import {
  AppShell,
} from "@/components/app-shell";

import {
  TeacherAssignmentEditor,
} from "@/features/teacher/teacher-assignment-editor";

export default function Page() {
  return (
    <AppShell
      role="Teacher"
      title="New assignment"
      subtitle="Draft academic work"
    >
      <TeacherAssignmentEditor />
    </AppShell>
  );
}