import {
  AppShell,
} from "@/components/app-shell";

import {
  TeacherAssignmentDetail,
} from "@/features/teacher/teacher-assignment-detail";

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
      title="Assignment"
      subtitle="Assignment record"
    >
      <TeacherAssignmentDetail
        assignmentId={id}
      />
    </AppShell>
  );
}