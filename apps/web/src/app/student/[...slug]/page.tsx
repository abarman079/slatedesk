import {
  AppShell,
} from "@/components/app-shell";

import {
  EmptyState,
} from "@/components/ui";

export default function StudentModulePage() {
  return (
    <AppShell
      role="Student"
      title="Student workspace"
      subtitle="Academic progress"
    >
      <EmptyState
        eyebrow="Student module"
        title="The student interface is prepared."
        description="Assignment, submission, grading, and result APIs are already complete. The next frontend phase will connect those workflows here."
      />
    </AppShell>
  );
}