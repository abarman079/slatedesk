import {
  AppShell,
} from "@/components/app-shell";

import {
  TeacherReviewQueue,
} from "@/features/teacher/teacher-review-queue";

export default function Page() {
  return (
    <AppShell
      role="Teacher"
      title="Review queue"
      subtitle="Student submissions"
    >
      <TeacherReviewQueue />
    </AppShell>
  );
}