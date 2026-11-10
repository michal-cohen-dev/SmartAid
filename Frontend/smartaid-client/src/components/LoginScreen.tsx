
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { apiRequest, type AuthResponse } from "../services/api";

function LoginScreen() {
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  function getIsVolunteer(token: string): boolean {
    const parts = token.split(".");
    const payload = parts[1];

    if (!payload) {
      throw new Error("התקבל Token לא תקין מהשרת.");
    }

    // המרת Base64URL ל-Base64 והשלמת Padding
    const base64 = payload
      .replace(/-/g, "+")
      .replace(/_/g, "/")
      .padEnd(Math.ceil(payload.length / 4) * 4, "=");

    // פענוח תקין גם כאשר ה-Token מכיל תווים בעברית
    const binary = atob(base64);
    const bytes = Uint8Array.from(
      binary,
      (character) => character.charCodeAt(0)
    );

    const decodedPayload = JSON.parse(
      new TextDecoder().decode(bytes)
    );

    const isVolunteer = decodedPayload.isVolunteer;

    return (
      isVolunteer === true ||
      String(isVolunteer).toLowerCase() === "true"
    );
  }

  async function handleSubmit(
    event: React.FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    if (loading) return;

    setError("");

    if (!email.trim() || !password) {
      setError("יש למלא כתובת אימייל וסיסמה.");
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

      // זיהוי סוג המשתמש לפי ה-Token שהשרת החזיר
      const isVolunteer = getIsVolunteer(response.token);

      // שמירת ה-Token לאחר פענוח מוצלח
      sessionStorage.setItem("token", response.token);

      // ניווט בהתאם לסוג המשתמש
      if (isVolunteer) {
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

      sessionStorage.removeItem("token");

    } finally {
      setLoading(false);
    }
  }

  return (
    <div dir="rtl">
      <div>
        <h1>ברוכים הבאים ל־SmartAid</h1>
        <p>עזרה חכמה. קהילה חזקה.</p>

        <form onSubmit={handleSubmit}>
          <div>
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

          <div>
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
            <p style={{ color: "red" }}>
              {error}
            </p>
          )}

          <button type="submit" disabled={loading}>
            {loading ? "מתחבר..." : "התחברות"}
          </button>
        </form>

        <p>
          עדיין אין לך חשבון?{" "}
          <Link to="/register">הירשמי עכשיו</Link>
        </p>
      </div>
    </div>
  );
}

export default LoginScreen;
