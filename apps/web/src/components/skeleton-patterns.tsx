import {
  Card,
  Skeleton,
} from "@/components/ui";

export function AssignmentLedgerSkeleton() {
  return (
    <Card className="ledger-skeleton">
      <Skeleton className="sk-code" />

      <Skeleton className="sk-title" />

      <div className="sk-meta-row">
        <Skeleton className="sk-meta" />
        <Skeleton className="sk-meta" />
      </div>

      <Skeleton className="sk-rail" />

      <div className="sk-footer">
        <Skeleton className="sk-pill" />
        <Skeleton className="sk-button" />
      </div>
    </Card>
  );
}

export function ReviewStackSkeleton() {
  return (
    <Card className="review-skeleton">
      <div className="review-skeleton-list">
        {[1, 2, 3].map((item) => (
          <div
            key={item}
            className="review-skeleton-person"
          >
            <Skeleton className="sk-avatar" />

            <div>
              <Skeleton className="sk-person-line" />
              <Skeleton className="sk-person-line short" />
            </div>
          </div>
        ))}
      </div>

      <div className="review-skeleton-detail">
        <Skeleton className="sk-review-title" />

        {[1, 2, 3, 4, 5, 6].map(
          (item) => (
            <Skeleton
              key={item}
              className="sk-answer-line"
            />
          ),
        )}

        <Skeleton className="sk-feedback" />
      </div>
    </Card>
  );
}

export function DashboardSkeleton() {
  return (
    <div className="dashboard-skeleton">
      <div className="dashboard-skeleton-heading">
        <Skeleton className="sk-heading" />
        <Skeleton className="sk-subheading" />
      </div>

      <div className="dashboard-skeleton-grid">
        {[1, 2, 3].map((item) => (
          <Card key={item}>
            <Skeleton className="sk-stat-label" />
            <Skeleton className="sk-stat-value" />
          </Card>
        ))}
      </div>

      <AssignmentLedgerSkeleton />
    </div>
  );
}