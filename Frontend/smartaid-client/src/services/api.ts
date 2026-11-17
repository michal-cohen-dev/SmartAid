
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL?.replace(/\/+$/, "");

export interface AuthResponse {
  message: string;
  token: string;
}


export class ApiError extends Error {
  public readonly status: number;
  public readonly details?: unknown;

  constructor(
    message: string,
    status: number,
    details?: unknown
  ) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.details = details;
  }
}



function getErrorMessage(data: unknown, status: number): string {
  if (typeof data === "string" && data.trim()) {
    return data;
  }

  if (data && typeof data === "object") {
    const body = data as Record<string, unknown>;

    // קודם בודקים שגיאות ולידציה לפי שדות
    if (body.errors && typeof body.errors === "object") {
      const errors = Object.values(
        body.errors as Record<string, unknown>
      ).flatMap((value) =>
        Array.isArray(value) ? value : [value]
      );

      const messages = errors.filter(
        (value): value is string =>
          typeof value === "string" && value.trim().length > 0
      );

      if (messages.length > 0) {
        return [...new Set(messages)].join("\n");
      }
    }

    if (typeof body.message === "string" && body.message.trim()) {
      return body.message;
    }

    if (typeof body.title === "string" && body.title.trim()) {
      return body.title;
    }
  }

  return `שגיאת שרת: ${status}`;
}


export async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  if (!API_BASE_URL) {
    throw new Error(
      "כתובת השרת לא הוגדרה. בדקי את קובץ .env.local"
    );
  }

  const token = sessionStorage.getItem("token");
  const headers = new Headers(options.headers);

  if (
    options.body &&
    !(options.body instanceof FormData) &&
    !headers.has("Content-Type")
  ) {
    headers.set("Content-Type", "application/json");
  }

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  let response: Response;

  try {
    response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers,
    });
  } catch (error) {
    if (error instanceof Error && error.name === "AbortError") {
      throw error;
    }

    throw new ApiError(
      "לא ניתן להתחבר לשרת. בדקי את החיבור לאינטרנט ונסי שוב.",
      0
    );
  }

  const responseText = await response.text();

  let data: unknown = null;

  if (responseText) {
    try {
      data = JSON.parse(responseText);
    } catch {
      data = responseText;
    }
  }

  if (!response.ok) {
    throw new ApiError(
      getErrorMessage(data, response.status),
      response.status,
      data
    );
  }

  return data as T;
}
