import {
  AppShell,
} from "@/components/app-shell";

import {
  TeacherReviewStack,
} from "@/features/teacher/teacher-review-stack";

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
      title="Review stack"
      subtitle="Submission review"
    >
      <TeacherReviewStack
        assignmentId={id}
      />
    </AppShell>
  );
}