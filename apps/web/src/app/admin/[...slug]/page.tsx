import {
  AppShell,
} from "@/components/app-shell";

import {
  EmptyState,
} from "@/components/ui";

export default function AdminModulePage() {
  return (
    <AppShell
      role="Admin"
      title="Admin workspace"
      subtitle="Academic operations"
    >
      <EmptyState
        eyebrow="Admin module"
        title="This workspace is ready for its live data."
        description="The shared shell, navigation, authentication, and design system are complete. This screen will be connected to the existing Admin API in the next phase."
      />
    </AppShell>
  );
}