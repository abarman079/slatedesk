"use client";

import {
  QueryClient,
  QueryClientProvider,
} from "@tanstack/react-query";

import {
  useState,
  type ReactNode,
} from "react";

import {
  AuthProvider,
} from "@/features/auth/auth-provider";

import {
  ThemeProvider,
} from "@/features/theme/theme-provider";

export function Providers({
  children,
}: {
  children: ReactNode;
}) {
  const [queryClient] =
    useState(
      () =>
        new QueryClient({
          defaultOptions: {
            queries: {
              staleTime: 30_000,
              refetchOnWindowFocus:
                false,
              retry: 1,
            },
            mutations: {
              retry: 0,
            },
          },
        }),
    );

  return (
    <QueryClientProvider
      client={queryClient}
    >
      <ThemeProvider>
        <AuthProvider>
          {children}
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}