import {
  AppShell,
} from "@/components/app-shell";

import {
  StudentSubmissionHistory,
} from "@/features/student/student-submission-history";

export default function Page() {
  return (
    <AppShell
      role="Student"
      title="My work"
      subtitle="Submission history"
    >
      <StudentSubmissionHistory />
    </AppShell>
  );
}