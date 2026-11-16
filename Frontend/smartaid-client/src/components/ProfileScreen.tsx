
import { useEffect, useState } from "react";
import { apiRequest, ApiError } from "../services/api";
import "./ProfileScreen.css";

interface UserProfile {
  id: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  createdAt: string;
}

interface VolunteerProfile {
  id: number;
  fullName: string;
  phoneNumber: string;
  equipmentTypeIds: number[];
  equipmentTypeNames: string[];
  helpTypeIds: number[];
  helpTypeNames: string[];
}

interface LookupItem {
  id: number;
  name: string;
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

    const decoded = JSON.parse(new TextDecoder().decode(bytes));

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
  const [volunteer, setVolunteer] = useState<VolunteerProfile | null>(null);

  const [equipmentTypes, setEquipmentTypes] = useState<LookupItem[]>([]);
  const [helpTypes, setHelpTypes] = useState<LookupItem[]>([]);

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");

  const [selectedEquipmentIds, setSelectedEquipmentIds] = useState<number[]>([]);
  const [selectedHelpIds, setSelectedHelpIds] = useState<number[]>([]);

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [savingQualifications, setSavingQualifications] = useState(false);

  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [qualificationError, setQualificationError] = useState("");
  const [qualificationMessage, setQualificationMessage] = useState("");

  const userId = getUserIdFromToken();

  useEffect(() => {
    async function loadProfile() {
      if (!userId) {
        setError("לא ניתן לזהות את המשתמש המחובר. יש להתחבר מחדש.");
        setLoading(false);
        return;
      }

      try {
        const userData = await apiRequest<UserProfile>(
          `/api/Users/${userId}`
        );

        setProfile(userData);
        setFullName(userData.fullName);
        setEmail(userData.email);
        setPhoneNumber(userData.phoneNumber ?? "");

        try {
          const volunteerData = await apiRequest<VolunteerProfile>(
            "/api/Volunteers/me"
          );

          setVolunteer(volunteerData);
          setSelectedEquipmentIds(volunteerData.equipmentTypeIds ?? []);
          setSelectedHelpIds(volunteerData.helpTypeIds ?? []);

          const [equipmentData, helpData] = await Promise.all([
            apiRequest<LookupItem[]>("/api/EquipmentTypes"),
            apiRequest<LookupItem[]>("/api/HelpTypes"),
          ]);

          setEquipmentTypes(equipmentData);
          setHelpTypes(helpData);
        } catch (volunteerError) {
          if (
            !(volunteerError instanceof ApiError) ||
            volunteerError.status !== 404
          ) {
            throw volunteerError;
          }
        }
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

  function toggleEquipment(id: number) {
    setSelectedEquipmentIds((previous) =>
      previous.includes(id)
        ? previous.filter((item) => item !== id)
        : [...previous, id]
    );
  }

  function toggleHelpType(id: number) {
    setSelectedHelpIds((previous) =>
      previous.includes(id)
        ? previous.filter((item) => item !== id)
        : [...previous, id]
    );
  }

  async function saveQualifications(
    event: React.FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    if (!volunteer || savingQualifications) return;

    setQualificationError("");
    setQualificationMessage("");

    if (selectedHelpIds.length === 0) {
      setQualificationError("יש לבחור לפחות סוג עזרה אחד.");
      return;
    }

    try {
      setSavingQualifications(true);

      await apiRequest(`/api/Volunteers/${volunteer.id}`, {
        method: "PUT",
        body: JSON.stringify({
          id: volunteer.id,
          fullName: fullName.trim(),
          phoneNumber: phoneNumber.trim(),
          equipmentTypeIds: selectedEquipmentIds,
          helpTypeIds: selectedHelpIds,
        }),
      });

      setVolunteer((previous) =>
        previous
          ? {
              ...previous,
              fullName: fullName.trim(),
              phoneNumber: phoneNumber.trim(),
              equipmentTypeIds: [...selectedEquipmentIds],
              helpTypeIds: [...selectedHelpIds],
              equipmentTypeNames: equipmentTypes
                .filter((item) => selectedEquipmentIds.includes(item.id))
                .map((item) => item.name),
              helpTypeNames: helpTypes
                .filter((item) => selectedHelpIds.includes(item.id))
                .map((item) => item.name),
            }
          : previous
      );

      setQualificationMessage("פרטי ההתנדבות עודכנו בהצלחה.");
    } catch (err) {
      setQualificationError(
        err instanceof Error
          ? err.message
          : "עדכון פרטי ההתנדבות נכשל."
      );
    } finally {
      setSavingQualifications(false);
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

      {volunteer && (
        <div className="profile-card volunteer-profile-card">
          <h2>פרטי ההתנדבות שלי</h2>
          <p className="profile-subtitle">
            כאן אפשר לעדכן את סוגי הציוד וסוגי העזרה שבהם ניתן לסייע.
          </p>

          {qualificationError && (
            <p className="profile-error">{qualificationError}</p>
          )}

          {qualificationMessage && (
            <p className="profile-success">{qualificationMessage}</p>
          )}

          <form onSubmit={saveQualifications}>
            <div className="profile-qualification-section">
              <h3>סוגי ציוד שברשותי</h3>

              {equipmentTypes.length === 0 ? (
                <p>לא הוגדרו סוגי ציוד במערכת.</p>
              ) : (
                <div className="profile-options-grid">
                  {equipmentTypes.map((item) => (
                    <label
                      className="profile-option"
                      key={item.id}
                    >
                      <input
                        type="checkbox"
                        checked={selectedEquipmentIds.includes(item.id)}
                        onChange={() => toggleEquipment(item.id)}
                      />
                      <span>{item.name}</span>
                    </label>
                  ))}
                </div>
              )}
            </div>

            <div className="profile-qualification-section">
              <h3>סוגי עזרה שאני יכול להציע</h3>

              {helpTypes.length === 0 ? (
                <p>לא הוגדרו סוגי עזרה במערכת.</p>
              ) : (
                <div className="profile-options-grid">
                  {helpTypes.map((item) => (
                    <label
                      className="profile-option"
                      key={item.id}
                    >
                      <input
                        type="checkbox"
                        checked={selectedHelpIds.includes(item.id)}
                        onChange={() => toggleHelpType(item.id)}
                      />
                      <span>{item.name}</span>
                    </label>
                  ))}
                </div>
              )}
            </div>

            <button
              type="submit"
              disabled={savingQualifications}
            >
              {savingQualifications
                ? "שומר..."
                : "שמירת פרטי ההתנדבות"}
            </button>
          </form>
        </div>
      )}
    </div>
  );
}

export default ProfileScreen;
