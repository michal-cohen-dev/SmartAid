
import { useEffect, useState } from "react";
import { apiRequest } from "../services/api";
import "./ProfileScreen.css";

interface UserProfile {
  id: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  createdAt: string;
}

function getUserIdFromToken(): number | null {
  const token = sessionStorage.getItem("token");

  if (!token) return null;

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

    const userId =
      decoded[
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
      ] ?? decoded.nameid ?? decoded.sub;

    const parsedId = Number(userId);

    return Number.isInteger(parsedId) && parsedId > 0
      ? parsedId
      : null;
  } catch {
    return null;
  }
}

function ProfileScreen() {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  const userId = getUserIdFromToken();

  useEffect(() => {
    async function loadProfile() {
      if (!userId) {
        setError("לא ניתן לזהות את המשתמש המחובר. יש להתחבר מחדש.");
        setLoading(false);
        return;
      }

      try {
        const data = await apiRequest<UserProfile>(
          `/api/Users/${userId}`
        );

        setProfile(data);
        setFullName(data.fullName);
        setEmail(data.email);
        setPhoneNumber(data.phoneNumber ?? "");
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : "טעינת הפרופיל נכשלה."
        );
      } finally {
        setLoading(false);
      }
    }

    loadProfile();
  }, [userId]);

  async function handleSubmit(
    event: React.FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    if (!userId || saving) return;

    setError("");
    setMessage("");

    try {
      setSaving(true);

      await apiRequest(`/api/Users/${userId}`, {
        method: "PUT",
        body: JSON.stringify({
          fullName: fullName.trim(),
          email: email.trim(),
          phoneNumber: phoneNumber.trim(),
        }),
      });

      setProfile((previous) =>
        previous
          ? {
              ...previous,
              fullName: fullName.trim(),
              email: email.trim(),
              phoneNumber: phoneNumber.trim(),
            }
          : previous
      );

      setMessage("הפרטים האישיים עודכנו בהצלחה.");
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "שמירת הפרטים נכשלה."
      );
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return <div className="profile-container">טוען פרופיל...</div>;
  }

  return (
    <div className="profile-container" dir="rtl">
      <div className="profile-card">
        <h1>הפרופיל האישי שלי</h1>
        <p className="profile-subtitle">
          צפייה ועדכון של הפרטים האישיים שלך
        </p>

        {error && <p className="profile-error">{error}</p>}
        {message && <p className="profile-success">{message}</p>}

        {profile && (
          <form onSubmit={handleSubmit}>
            <div className="profile-field">
              <label htmlFor="profileFullName">שם מלא</label>
              <input
                id="profileFullName"
                value={fullName}
                onChange={(event) => setFullName(event.target.value)}
                required
                minLength={2}
                maxLength={100}
              />
            </div>

           <div className="profile-field">
  <label htmlFor="profileEmail">
    כתובת דוא"ל (משמשת להתחברות)
  </label>
  <input
    id="profileEmail"
    type="email"
    value={email}
    readOnly
  />
  <small>לשינוי כתובת הדוא"ל נדרש תהליך אימות.</small>
</div>

            <div className="profile-field">
              <label htmlFor="profilePhone">מספר טלפון</label>
              <input
                id="profilePhone"
                type="tel"
                value={phoneNumber}
                onChange={(event) => setPhoneNumber(event.target.value)}
                required
                minLength={9}
                maxLength={20}
              />
            </div>

            <button type="submit" disabled={saving}>
              {saving ? "שומר..." : "שמירת שינויים"}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}

export default ProfileScreen;
