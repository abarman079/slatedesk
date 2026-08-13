import {
  AppShell,
} from "@/components/app-shell";

import {
  TeacherAssignmentEditor,
} from "@/features/teacher/teacher-assignment-editor";

export default async function Page({
  params,
}: {
  params: Promise<{
    id: string;
  }>;
}) {
  const {
    id,
  } = await params;

  return (
    <AppShell
      role="Teacher"
      title="Edit assignment"
      subtitle="Assignment editor"
    >
      <TeacherAssignmentEditor
        assignmentId={id}
      />
    </AppShell>
  );
}