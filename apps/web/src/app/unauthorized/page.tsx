"use client";

import {
  useRouter,
} from "next/navigation";

import {
  ShieldX,
} from "lucide-react";

import {
  Button,
  Card,
} from "@/components/ui";

import {
  useAuth,
} from "@/features/auth/auth-provider";

export default function UnauthorizedPage() {
  const router = useRouter();

  const {
    user,
  } = useAuth();

  function returnHome() {
    if (
      user?.roles.includes("Admin")
    ) {
      router.push("/admin");
      return;
    }

    if (
      user?.roles.includes(
        "Teacher",
      )
    ) {
      router.push("/teacher");
      return;
    }

    if (
      user?.roles.includes(
        "Student",
      )
    ) {
      router.push("/student");
      return;
    }

    router.push("/login");
  }

  return (
    <main
      id="main-content"
      style={{
        minHeight: "100svh",
        display: "grid",
        placeItems: "center",
        padding: 24,
      }}
    >
      <Card
        style={{
          width: "min(100%, 520px)",
          padding: 36,
          textAlign: "center",
        }}
      >
        <ShieldX
          size={34}
          color="var(--rose)"
        />

        <p
          className="eyebrow"
          style={{
            marginTop: 20,
          }}
        >
          Permission required
        </p>

        <h1
          style={{
            fontFamily:
              "var(--font-serif)",
            fontSize: "2.4rem",
            fontWeight: 560,
            letterSpacing:
              "-0.04em",
            margin:
              "10px 0 12px",
          }}
        >
          This workspace is not
          assigned to your account.
        </h1>

        <p className="muted">
          Your account is active,
          but your current role does
          not permit access to this
          area.
        </p>

        <Button
          style={{
            marginTop: 24,
          }}
          onClick={
            returnHome
          }
        >
          Return to my workspace
        </Button>
      </Card>
    </main>
  );
}