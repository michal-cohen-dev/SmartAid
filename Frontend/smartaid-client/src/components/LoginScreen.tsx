
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { apiRequest, type AuthResponse } from "../services/api";
import { useAuth } from "../context/AuthContext";
import "./LoginScreen.css";
import { validateEmail } from "../utils/validation";

function getUserRole(token: string): string | null {
  try {
    const payload = token.split(".")[1];
    if (!payload) return null;

    const base64 = payload
      .replace(/-/g, "+")
      .replace(/_/g, "/")
      .padEnd(Math.ceil(payload.length / 4) * 4, "=");

    const binary = atob(base64);
    const bytes = Uint8Array.from(
      binary,
      (character) => character.charCodeAt(0)
    );

    const decoded = JSON.parse(
      new TextDecoder().decode(bytes)
    );

    return (
      decoded.role ??
      decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
      null
    );
  } catch {
    return null;
  }
}

function LoginScreen() {
  const navigate = useNavigate();
  const { login } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(
    event: React.FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    if (loading) return;

    setError("");
    
const emailError = validateEmail(email);

if (emailError) {
  setError(emailError);
  return;
}

if (!password) {
  setError("יש להזין סיסמה.");
  return;
}

    try {
      setLoading(true);

      const response = await apiRequest<AuthResponse>(
        "/api/Auth/login",
        {
          method: "POST",
          body: JSON.stringify({
            email: email.trim(),
            password,
          }),
        }
      );

      if (!response.token) {
        throw new Error("לא התקבל Token מהשרת.");
      }

      // שמירת הטוקן דרך מנגנון האימות המרכזי
      login(response.token);

      // זיהוי סוג המשתמש מתוך AuthContext
      const decodedIsVolunteer =
        (() => {
          try {
            const payload = response.token.split(".")[1];
            if (!payload) return false;

            const base64 = payload
              .replace(/-/g, "+")
              .replace(/_/g, "/")
              .padEnd(
                Math.ceil(payload.length / 4) * 4,
                "="
              );

            const binary = atob(base64);
            const bytes = Uint8Array.from(
              binary,
              (character) => character.charCodeAt(0)
            );

            const decoded = JSON.parse(
              new TextDecoder().decode(bytes)
            );

            return (
              decoded.isVolunteer === true ||
              String(decoded.isVolunteer).toLowerCase() === "true"
            );
          } catch {
            return false;
          }
        })();

const role = getUserRole(response.token);

if (role?.toLowerCase() === "admin") {
  navigate("/admin");
} else if (decodedIsVolunteer) {
  navigate("/volunteer");
} else {
  navigate("/requester");
}

    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בהתחברות. נסי שוב."
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-page" dir="rtl">
      <div className="login-card">
        <header className="login-header">
          <h1>ברוכים הבאים ל־SmartAid</h1>
          <p>עזרה חכמה. קהילה חזקה.</p>
        </header>

        <form className="login-form" onSubmit={handleSubmit}>
          <div className="login-field">
            <label htmlFor="loginEmail">אימייל</label>
            <input
              id="loginEmail"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              placeholder="הזיני כתובת אימייל"
              required
            />
          </div>

          <div className="login-field">
            <label htmlFor="loginPassword">סיסמה</label>
            <input
              id="loginPassword"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="הזיני סיסמה"
              required
            />
          </div>

          {error && (
            <p className="login-error">{error}</p>
          )}

          <button
            className="login-submit"
            type="submit"
            disabled={loading}
          >
            {loading ? "מתחבר..." : "התחברות"}
          </button>
        </form>

        <p className="login-register">
          עדיין אין לך חשבון?{" "}
          <Link to="/register">הירשמי עכשיו</Link>
        </p>
      </div>
    </div>
  );
}

export default LoginScreen;
