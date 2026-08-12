export type AppRole = "Admin" | "Teacher" | "Student";

export type AuthenticatedUser = {
  id: string;
  fullName: string;
  email: string;
  roles: AppRole[];
};

export type AuthenticationResponse = {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  user: AuthenticatedUser;
};