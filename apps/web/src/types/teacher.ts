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

export type TeacherAllocationOption = {
  academicClassId: string;
  className: string;
  classCode: string;
  subjectId: string;
  subjectName: string;
  subjectCode: string;
};

export type TeacherAssignment = {
  id: string;

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
  createdAtUtc: string;
  updatedAtUtc: string | null;

  submissionCount: number;
  isPastDeadline: boolean;
};

export type TeacherSubmission = {
  id: string;
  assignmentId: string;
  assignmentTitle: string;

  studentId: string;
  studentName: string;
  studentEmail: string;

  answerText: string;

  submittedAtUtc: string | null;
  updatedAtUtc: string;

  status: SubmissionStatus;

  marksAwarded: number | null;
  teacherFeedback: string | null;
  gradedAtUtc: string | null;

  version: number;

  isLate: boolean;
};