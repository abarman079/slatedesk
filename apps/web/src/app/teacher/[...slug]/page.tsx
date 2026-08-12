import {
  AppShell,
} from "@/components/app-shell";

import {
  EmptyState,
} from "@/components/ui";

export default function TeacherModulePage() {
  return (
    <AppShell
      role="Teacher"
      title="Teacher workspace"
      subtitle="Academic workflow"
    >
      <EmptyState
        eyebrow="Teacher module"
        title="The teaching interface is prepared."
        description="Assignment and review API workflows already exist. Their full working frontend screens arrive in the Teacher frontend phase."
      />
    </AppShell>
  );
}