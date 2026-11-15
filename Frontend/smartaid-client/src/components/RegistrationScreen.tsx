
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { apiRequest, type AuthResponse } from "../services/api";
import "./RegistrationScreen.css";

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
    <div className="registration-page" dir="rtl">
      <div className="registration-card">
        <header className="registration-header">
          <h1>יצירת חשבון</h1>
          <p>הצטרפי לקהילת SmartAid</p>
        </header>

        <form className="registration-form" onSubmit={handleSubmit}>
          <div className="registration-field">
            <label htmlFor="fullName">שם מלא</label>
            <input
              id="fullName"
              type="text"
              value={fullName}
              onChange={(event) => setFullName(event.target.value)}
              required
              minLength={2}
              maxLength={100}
              placeholder="הזיני שם מלא"
            />
          </div>

          <div className="registration-field">
            <label htmlFor="phone">טלפון</label>
            <input
              id="phone"
              type="tel"
              value={phone}
              onChange={(event) => setPhone(event.target.value)}
              required
              placeholder="הזיני מספר טלפון"
            />
          </div>

          <div className="registration-field">
            <label htmlFor="email">אימייל</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
              placeholder="name@example.com"
            />
          </div>

          <div className="registration-field">
            <label htmlFor="password">סיסמה</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              minLength={8}
              maxLength={100}
              placeholder="בחרי סיסמה"
            />
          </div>

          <div className="registration-field">
            <label htmlFor="confirmPassword">אישור סיסמה</label>
            <input
              id="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
              required
              placeholder="הקלידי שוב את הסיסמה"
            />
          </div>

          {error && (
            <p className="registration-error" role="alert">
              {error}
            </p>
          )}

          <button
            className="registration-submit"
            type="submit"
            disabled={loading}
          >
            {loading ? "נרשמת..." : "הרשמה"}
          </button>
        </form>

        <p className="registration-login">
          כבר יש לך חשבון?{" "}
          <Link to="/login">התחברי עכשיו</Link>
        </p>
      </div>
    </div>
  );

}

export default RegistrationScreen;
