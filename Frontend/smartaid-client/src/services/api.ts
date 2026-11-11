
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL?.replace(/\/+$/, "");

export interface AuthResponse {
  message: string;
  token: string;
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

  // הגדרת JSON רק כאשר הבקשה מכילה גוף שאינו FormData
  if (
    options.body &&
    !(options.body instanceof FormData) &&
    !headers.has("Content-Type")
  ) {
    headers.set("Content-Type", "application/json");
  }

  // הוספת JWT לבקשות של משתמש מחובר
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  // קריאת התשובה מהשרת
  const responseText = await response.text();

  let data: any = null;

  if (responseText) {
    try {
      data = JSON.parse(responseText);
    } catch {
      data = responseText;
    }
  }

  // טיפול בשגיאות שהתקבלו מהשרת
  if (!response.ok) {
    const errorMessage =
      data?.title ||
      data?.message ||
      (typeof data === "string" ? data : null) ||
      `שגיאת שרת: ${response.status}`;

    throw new Error(errorMessage);
  }

  return data as T;
}
