
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { apiRequest, type AuthResponse } from "../services/api";

function RegistrationScreen() {
  const navigate = useNavigate();

  const [fullName, setFullName] = useState("");
  const [phone, setPhone] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(
    event: React.FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();
    setError("");

    if (loading) return;

    if (
      !fullName.trim() ||
      !phone.trim() ||
      !email.trim() ||
      !password
    ) {
      setError("יש למלא את כל השדות.");
      return;
    }

    if (password !== confirmPassword) {
      setError("הסיסמאות אינן תואמות.");
      return;
    }

    try {
      setLoading(true);

      const response = await apiRequest<AuthResponse>(
        "/api/Auth/register",
        {
          method: "POST",
          body: JSON.stringify({
            fullName: fullName.trim(),
            email: email.trim(),
            phoneNumber: phone.trim(),
            password
          })
        }
      );

      sessionStorage.setItem("token", response.token);

      navigate("/requester");
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בהרשמה. נסי שוב."
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <div dir="rtl">
      <div>
        <h1>יצירת חשבון</h1>
        <p>הצטרפי לקהילת SmartAid</p>

        <form onSubmit={handleSubmit}>
          <div>
            <label htmlFor="fullName">שם מלא</label>
            <input
              id="fullName"
              type="text"
              value={fullName}
              onChange={(event) =>
                setFullName(event.target.value)
              }
              required
              minLength={2}
              maxLength={100}
              placeholder="הזיני שם מלא"
            />
          </div>

          <div>
            <label htmlFor="phone">טלפון</label>
            <input
              id="phone"
              type="tel"
              value={phone}
              onChange={(event) =>
                setPhone(event.target.value)
              }
              required
              placeholder="הזיני מספר טלפון"
            />
          </div>

          <div>
            <label htmlFor="email">אימייל</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(event) =>
                setEmail(event.target.value)
              }
              required
              placeholder="name@example.com"
            />
          </div>

          <div>
            <label htmlFor="password">סיסמה</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(event) =>
                setPassword(event.target.value)
              }
              required
              minLength={8}
              maxLength={100}
              placeholder="בחרי סיסמה"
            />
          </div>

          <div>
            <label htmlFor="confirmPassword">
              אישור סיסמה
            </label>
            <input
              id="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(event) =>
                setConfirmPassword(event.target.value)
              }
              required
              placeholder="הקלידי שוב את הסיסמה"
            />
          </div>

          {error && <p role="alert">{error}</p>}

          <button type="submit" disabled={loading}>
            {loading ? "נרשמת..." : "הרשמה"}
          </button>
        </form>

        <p>
          כבר יש לך חשבון?{" "}
          <Link to="/login">התחברי עכשיו</Link>
        </p>
      </div>
    </div>
  );
}

export default RegistrationScreen;
