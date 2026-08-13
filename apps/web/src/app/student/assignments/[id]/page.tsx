import {
  AppShell,
} from "@/components/app-shell";

import {
  StudentAssignmentDetail,
} from "@/features/student/student-assignment-detail";

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
      role="Student"
      title="Assignment"
      subtitle="Assignment and submission"
    >
      <StudentAssignmentDetail
        assignmentId={id}
      />
    </AppShell>
  );
}