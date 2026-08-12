const API_URL =
  process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7049";

export class ApiError extends Error {
  readonly status: number;
  readonly body: unknown;

  constructor(
    status: number,
    message: string,
    body?: unknown,
  ) {
    super(message);

    this.name = "ApiError";
    this.status = status;
    this.body = body;
  }
}

type ApiRequestOptions = RequestInit & {
  accessToken?: string | null;
};

export async function apiRequest<T>(
  path: string,
  options: ApiRequestOptions = {},
): Promise<T> {
  const headers = new Headers(options.headers);

  if (
    options.body &&
    !(options.body instanceof FormData) &&
    !headers.has("Content-Type")
  ) {
    headers.set("Content-Type", "application/json");
  }

  if (options.accessToken) {
    headers.set(
      "Authorization",
      `Bearer ${options.accessToken}`,
    );
  }

  const response = await fetch(
    `${API_URL}${path}`,
    {
      ...options,
      headers,
      credentials: "include",
    },
  );

  if (response.status === 204) {
    return undefined as T;
  }

  const contentType =
    response.headers.get("content-type");

  const body =
    contentType?.includes("application/json")
      ? await response.json()
      : await response.text();

  if (!response.ok) {
    const message =
      typeof body === "object" &&
      body !== null &&
      "detail" in body &&
      typeof body.detail === "string"
        ? body.detail
        : `Request failed with status ${response.status}.`;

    throw new ApiError(
      response.status,
      message,
      body,
    );
  }

  return body as T;
}