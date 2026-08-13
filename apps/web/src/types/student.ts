export type AssignmentStatus =
  | "Draft"
  | "Published"
  | "Closed"
  | "Archived";

export type SubmissionStatus =
  | "Draft"
  | "Submitted"
  | "Late"
  | "UnderReview"
  | "NeedsRevision"
  | "Graded";

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
};

export type StudentAssignment = {
  id: string;

  teacherName: string;

  academicClassId: string;
  className: string;
  classCode: string;

  subjectId: string;
  subjectName: string;
  subjectCode: string;

  title: string;
  description: string;
  instructions: string | null;

  deadlineUtc: string;
  maximumMarks: number;

  allowResubmission: boolean;
  allowLateSubmission: boolean;

  status: AssignmentStatus;
  publishedAtUtc: string | null;

  submissionStatus: SubmissionStatus | null;

  isPastDeadline: boolean;
  canSubmit: boolean;
  wouldBeLate: boolean;
};

export type StudentSubmission = {
  id: string;
  assignmentId: string;

  assignmentTitle: string;
  subjectCode: string;

  deadlineUtc: string;
  maximumMarks: number;

  answerText: string;

  submittedAtUtc: string | null;
  updatedAtUtc: string;

  status: SubmissionStatus;

  marksAwarded: number | null;
  teacherFeedback: string | null;
  gradedAtUtc: string | null;

  version: number;

  canEdit: boolean;
  canSubmit: boolean;
};

export type StudentResult = {
  submissionId: string;
  assignmentId: string;

  assignmentTitle: string;

  subjectName: string;
  subjectCode: string;

  marksAwarded: number;
  maximumMarks: number;

  teacherFeedback: string | null;
  gradedAtUtc: string | null;

  status: SubmissionStatus;
};