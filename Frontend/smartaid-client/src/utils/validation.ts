
export function validateEmail(email: string): string | null {
  const value = email.trim();

  if (!value) {
    return "יש להזין כתובת אימייל.";
  }

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  if (!emailRegex.test(value)) {
    return "כתובת האימייל אינה תקינה.";
  }

  if (value.length > 254) {
    return "כתובת האימייל ארוכה מדי.";
  }

  return null;
}

export function validatePassword(password: string): string | null {
  if (!password) {
    return "יש להזין סיסמה.";
  }

  if (password.length < 8) {
    return "הסיסמה חייבת להכיל לפחות 8 תווים.";
  }

  if (password.length > 100) {
    return "הסיסמה ארוכה מדי.";
  }

  return null;
}

export function validateFullName(name: string): string | null {
  const value = name.trim();

  if (!value) {
    return "יש להזין שם מלא.";
  }

  if (value.length < 2 || value.length > 100) {
    return "השם חייב להכיל בין 2 ל־100 תווים.";
  }

  return null;
}

export function validatePhone(phone: string): string | null {
  const value = phone.trim();

  if (!value) {
    return "יש להזין מספר טלפון.";
  }

  if (!/^[0-9+\-\s()]+$/.test(value)) {
    return "מספר הטלפון מכיל תווים לא תקינים.";
  }

  const digits = value.replace(/\D/g, "");

  if (digits.length < 9 || digits.length > 15) {
    return "מספר הטלפון חייב להכיל בין 9 ל־15 ספרות.";
  }

  return null;
}

export function validatePasswordMatch(
  password: string,
  confirmPassword: string
): string | null {
  if (!confirmPassword) {
    return "יש לאשר את הסיסמה.";
  }

  if (password !== confirmPassword) {
    return "הסיסמאות אינן תואמות.";
  }

  return null;
}
