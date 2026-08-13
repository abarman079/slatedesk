export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
};

export type AdminUser = {
  id: string;
  fullName: string;
  email: string;
  roles: string[];
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
};

export type AcademicClass = {
  id: string;
  name: string;
  code: string;
  academicYear: string;
  description: string | null;
  isActive: boolean;
  createdAtUtc: string;
};

export type Subject = {
  id: string;
  name: string;
  code: string;
  description: string | null;
  isActive: boolean;
  createdAtUtc: string;
};

export type TeacherAllocation = {
  id: string;
  teacherId: string;
  teacherName: string;
  teacherEmail: string;
  academicClassId: string;
  className: string;
  classCode: string;
  subjectId: string;
  subjectName: string;
  subjectCode: string;
  isActive: boolean;
  assignedAtUtc: string;
};

export type StudentEnrollment = {
  id: string;
  studentId: string;
  studentName: string;
  studentEmail: string;
  academicClassId: string;
  className: string;
  classCode: string;
  isActive: boolean;
  enrolledAtUtc: string;
};

export type AuditLog = {
  id: string;
  userId: string | null;
  action: string;
  entityType: string;
  entityId: string;
  description: string;
  createdAtUtc: string;
};

export type AdminDashboard = {
  activeTeachers: number;
  activeStudents: number;
  activeClasses: number;
  activeSubjects: number;
  publishedAssignments: number;
  totalSubmissions: number;
  recentActivity: AuditLog[];
};

export type AdminAssignmentOverview = {
  id: string;
  title: string;
  teacherName: string;
  className: string;
  classCode: string;
  subjectName: string;
  subjectCode: string;
  deadlineUtc: string;
  maximumMarks: number;
  status: string;
  submissionCount: number;
  isArchived: boolean;
};

export type AdminSubmissionOverview = {
  id: string;
  assignmentId: string;
  assignmentTitle: string;
  studentName: string;
  teacherName: string;
  submittedAtUtc: string | null;
  status: string;
  marksAwarded: number | null;
  maximumMarks: number;
  gradedAtUtc: string | null;
};

export type AppSetting = {
  id: string;
  key: string;
  value: string;
  description: string | null;
  updatedAtUtc: string;
};