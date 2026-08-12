"use client";

import {
  useEffect,
  useState,
} from "react";

import {
  useRouter,
} from "next/navigation";

import {
  LockKeyhole,
  LogIn,
} from "lucide-react";

import {
  useForm,
} from "react-hook-form";

import {
  z,
} from "zod";

import {
  zodResolver,
} from "@hookform/resolvers/zod";

import {
  Button,
  Input,
} from "@/components/ui";

import {
  useAuth,
} from "@/features/auth/auth-provider";

import {
  ApiError,
} from "@/lib/api-client";

import type {
  AuthenticatedUser,
} from "@/types/auth";

const schema = z.object({
  email: z
    .string()
    .trim()
    .email(
      "Enter a valid email address.",
    ),

  password: z
    .string()
    .min(
      8,
      "Password must contain at least 8 characters.",
    ),
});

type FormValues =
  z.infer<typeof schema>;

function dashboardFor(
  user: AuthenticatedUser,
) {
  if (user.roles.includes("Admin")) {
    return "/admin";
  }

  if (
    user.roles.includes("Teacher")
  ) {
    return "/teacher";
  }

  return "/student";
}

function AcademicLedgerMotif() {
  return (
    <div
      className="ledger-motif"
      aria-hidden="true"
    >
      <div className="ledger-line" />

      <article className="ledger-folio">
        <span className="folio-code">
          CSE-401
        </span>

        <div className="folio-title">
          REST API Design
        </div>

        <div className="folio-meta">
          <span>Due 14 Aug</span>
          <span>Published</span>
        </div>
      </article>

      <article className="ledger-folio">
        <span className="folio-code">
          MAT-318
        </span>

        <div className="folio-title">
          Structural Methods
        </div>

        <div className="folio-meta">
          <span>Due Friday</span>
          <span>Review</span>
        </div>
      </article>

      <article className="ledger-folio">
        <span className="folio-code">
          ENG-220
        </span>

        <div className="folio-title">
          Technical Report
        </div>

        <div className="folio-meta">
          <span>26 / 30</span>
          <span>Graded</span>
        </div>
      </article>
    </div>
  );
}

export default function LoginPage() {
  const router = useRouter();

  const {
    login,
    user,
    isLoading,
  } = useAuth();

  const [apiError, setApiError] =
    useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: {
      errors,
      isSubmitting,
    },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  useEffect(() => {
    if (
      !isLoading &&
      user
    ) {
      router.replace(
        dashboardFor(user),
      );
    }
  }, [
    user,
    isLoading,
    router,
  ]);

  async function onSubmit(
    values: FormValues,
  ) {
    setApiError(null);

    try {
      const authenticatedUser =
        await login(
          values.email,
          values.password,
        );

      router.replace(
        dashboardFor(
          authenticatedUser,
        ),
      );
    } catch (error) {
      setApiError(
        error instanceof ApiError
          ? error.message
          : "Unable to sign in. Please try again.",
      );
    }
  }

  return (
    <main className="login-page">
      <section
        className="login-editorial"
        aria-label="SlateDesk introduction"
      >
        <div className="brand-lockup">
          <span
            className="brand-mark"
            aria-hidden="true"
          />

          <span className="brand-word">
            SlateDesk
          </span>
        </div>

        <div className="login-copy">
          <p className="eyebrow">
            Academic workspace
          </p>

          <h1 className="editorial-heading">
            Work with clarity.
            <br />
            Teach with context.
          </h1>

          <p>
            One precise workspace for
            assignments, submissions,
            review, and academic
            progress.
          </p>

          <AcademicLedgerMotif />
        </div>

        <p className="muted">
          Academic work, clearly
          organized.
        </p>
      </section>

      <section
        className="login-form-panel"
        aria-labelledby="login-title"
      >
        <div className="login-form-wrap">
          <p className="eyebrow">
            Secure access
          </p>

          <h2 id="login-title">
            Welcome back
          </h2>

          <p>
            Sign in with your SlateDesk
            account.
          </p>

          <form
            className="login-form"
            onSubmit={handleSubmit(
              onSubmit,
            )}
            noValidate
          >
            {apiError && (
              <div
                role="alert"
                className="form-error"
              >
                {apiError}
              </div>
            )}

            <div className="form-field">
              <label htmlFor="email">
                Email address
              </label>

              <Input
                id="email"
                type="email"
                autoComplete="email"
                placeholder="name@slatedesk.local"
                aria-invalid={
                  Boolean(errors.email)
                }
                aria-describedby={
                  errors.email
                    ? "email-error"
                    : undefined
                }
                {...register("email")}
              />

              {errors.email && (
                <p
                  id="email-error"
                  className="form-error"
                >
                  {
                    errors.email
                      .message
                  }
                </p>
              )}
            </div>

            <div className="form-field">
              <label htmlFor="password">
                Password
              </label>

              <Input
                id="password"
                type="password"
                autoComplete="current-password"
                placeholder="Your password"
                aria-invalid={
                  Boolean(
                    errors.password,
                  )
                }
                aria-describedby={
                  errors.password
                    ? "password-error"
                    : undefined
                }
                {...register(
                  "password",
                )}
              />

              {errors.password && (
                <p
                  id="password-error"
                  className="form-error"
                >
                  {
                    errors.password
                      .message
                  }
                </p>
              )}
            </div>

            <Button
              className="login-submit"
              type="submit"
              disabled={isSubmitting}
            >
              {isSubmitting
                ? "Signing in…"
                : "Enter workspace"}

              {!isSubmitting && (
                <LogIn size={18} />
              )}
            </Button>
          </form>

          <div className="security-note">
            <LockKeyhole
              size={15}
              aria-hidden="true"
            />

            <span>
              Protected by role-based
              access and secure session
              refresh.
            </span>
          </div>
        </div>
      </section>
    </main>
  );
}