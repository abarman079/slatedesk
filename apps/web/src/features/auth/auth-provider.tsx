"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";

import { ApiError, apiRequest } from "@/lib/api-client";

import type { AuthenticatedUser, AuthenticationResponse } from "@/types/auth";

type AuthContextValue = {
  user: AuthenticatedUser | null;
  accessToken: string | null;
  isLoading: boolean;

  login: (email: string, password: string) => Promise<AuthenticatedUser>;

  logout: () => Promise<void>;

  refreshSession: () => Promise<boolean>;

  request: <T>(path: string, options?: RequestInit) => Promise<T>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

let bootstrapPromise: Promise<AuthenticationResponse | null> | null = null;

let refreshPromise: Promise<AuthenticationResponse> | null = null;

async function bootstrapSession() {
  if (!bootstrapPromise) {
    bootstrapPromise = apiRequest<AuthenticationResponse>(
      "/api/v1/auth/refresh",
      {
        method: "POST",
      },
    ).catch(() => null);
  }

  return bootstrapPromise;
}

async function rotateSession() {
  if (!refreshPromise) {
    refreshPromise = apiRequest<AuthenticationResponse>(
      "/api/v1/auth/refresh",
      {
        method: "POST",
      },
    ).finally(() => {
      refreshPromise = null;
    });
  }

  return refreshPromise;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthenticatedUser | null>(null);

  const [accessToken, setAccessToken] = useState<string | null>(null);

  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function initialize() {
      const session = await bootstrapSession();

      if (cancelled) {
        return;
      }

      if (session) {
        setUser(session.user);
        setAccessToken(session.accessToken);
      }

      setIsLoading(false);
    }

    void initialize();

    return () => {
      cancelled = true;
    };
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const response = await apiRequest<AuthenticationResponse>(
      "/api/v1/auth/login",
      {
        method: "POST",
        body: JSON.stringify({
          email,
          password,
        }),
      },
    );

    bootstrapPromise = Promise.resolve(response);

    setUser(response.user);

    setAccessToken(response.accessToken);

    return response.user;
  }, []);

  const refreshSession = useCallback(async () => {
    try {
      const response = await rotateSession();

      setUser(response.user);

      setAccessToken(response.accessToken);

      return true;
    } catch {
      setUser(null);
      setAccessToken(null);

      return false;
    }
  }, []);

  const logout = useCallback(async () => {
    try {
      await apiRequest<void>("/api/v1/auth/logout", {
        method: "POST",
        accessToken,
      });
    } finally {
      bootstrapPromise = null;
      refreshPromise = null;

      setUser(null);
      setAccessToken(null);
    }
  }, [accessToken]);

  const request = useCallback(
    async <T,>(path: string, options: RequestInit = {}): Promise<T> => {
      try {
        return await apiRequest<T>(path, {
          ...options,
          accessToken,
        });
      } catch (error) {
        if (!(error instanceof ApiError) || error.status !== 401) {
          throw error;
        }

        let refreshedResponse: AuthenticationResponse;

        try {
          refreshedResponse = await rotateSession();
        } catch {
          setUser(null);
          setAccessToken(null);

          throw error;
        }

        setUser(refreshedResponse.user);

        setAccessToken(refreshedResponse.accessToken);

        return apiRequest<T>(path, {
          ...options,
          accessToken: refreshedResponse.accessToken,
        });
      }
    },
    [accessToken],
  );

  const value = useMemo(
    () => ({
      user,
      accessToken,
      isLoading,
      login,
      logout,
      refreshSession,
      request,
    }),
    [user, accessToken, isLoading, login, logout, refreshSession, request],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return context;
}
