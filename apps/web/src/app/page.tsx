"use client";

import {
  useEffect,
} from "react";

import {
  useRouter,
} from "next/navigation";

import {
  useAuth,
} from "@/features/auth/auth-provider";

export default function HomePage() {
  const router = useRouter();

  const {
    user,
    isLoading,
  } = useAuth();

  useEffect(() => {
    if (isLoading) {
      return;
    }

    if (!user) {
      router.replace("/login");
      return;
    }

    if (
      user.roles.includes("Admin")
    ) {
      router.replace("/admin");
      return;
    }

    if (
      user.roles.includes("Teacher")
    ) {
      router.replace("/teacher");
      return;
    }

    router.replace("/student");
  }, [
    user,
    isLoading,
    router,
  ]);

  return (
    <main
      id="main-content"
      style={{
        minHeight: "100svh",
        display: "grid",
        placeItems: "center",
      }}
    >
      <p className="muted">
        Preparing SlateDesk…
      </p>
    </main>
  );
}