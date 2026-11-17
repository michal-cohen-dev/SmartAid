
import { useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";
import { apiRequest } from "../services/api";
import "./AdminVolunteers.css";

interface Volunteer {
  id: number;
  fullName: string;
  phoneNumber: string;
  latitude: number | null;
  longitude: number | null;
  isActive: boolean;
  totalOffersReceived: number;
  totalOffersAccepted: number;
  totalCompleted: number;
  reliabilityScore: number;
  equipmentTypeIds: number[];
  equipmentTypeNames: string[];
  helpTypeIds: number[];
  helpTypeNames: string[];
}

interface EquipmentType {
  id: number;
  name: string;
}

interface HelpType {
  id: number;
  name: string;
}

export default function AdminVolunteers() {
  const [volunteers, setVolunteers] = useState<Volunteer[]>([]);
  const [equipmentTypes, setEquipmentTypes] = useState<EquipmentType[]>([]);
  const [helpTypes, setHelpTypes] = useState<HelpType[]>([]);

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const [search, setSearch] = useState("");
  const [availability, setAvailability] = useState("all");

  const [editingVolunteer, setEditingVolunteer] =
    useState<Volunteer | null>(null);

  const [fullName, setFullName] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [selectedEquipmentIds, setSelectedEquipmentIds] =
    useState<number[]>([]);
  const [selectedHelpIds, setSelectedHelpIds] =
    useState<number[]>([]);

  useEffect(() => {
    loadData();
  }, []);

  async function loadData() {
    try {
      setLoading(true);
      setError("");

      const [volunteerData, equipmentData, helpData] =
        await Promise.all([
          apiRequest<Volunteer[]>("/api/Volunteers"),
          apiRequest<EquipmentType[]>("/api/EquipmentTypes"),
          apiRequest<HelpType[]>("/api/HelpTypes"),
        ]);

      setVolunteers(volunteerData);
      setEquipmentTypes(equipmentData);
      setHelpTypes(helpData);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בטעינת הנתונים"
      );
    } finally {
      setLoading(false);
    }
  }

  function startEditing(volunteer: Volunteer) {
    setEditingVolunteer(volunteer);
    setFullName(volunteer.fullName);
    setPhoneNumber(volunteer.phoneNumber);
    setSelectedEquipmentIds([...volunteer.equipmentTypeIds]);
    setSelectedHelpIds([...volunteer.helpTypeIds]);
    setError("");
    setSuccessMessage("");
  }

  function cancelEditing() {
    setEditingVolunteer(null);
    setFullName("");
    setPhoneNumber("");
    setSelectedEquipmentIds([]);
    setSelectedHelpIds([]);
  }

  function toggleEquipment(id: number) {
    setSelectedEquipmentIds((current) =>
      current.includes(id)
        ? current.filter((itemId) => itemId !== id)
        : [...current, id]
    );
  }

  function toggleHelpType(id: number) {
    setSelectedHelpIds((current) =>
      current.includes(id)
        ? current.filter((itemId) => itemId !== id)
        : [...current, id]
    );
  }

  async function handleSave(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();

    if (!editingVolunteer) return;

    const normalizedName = fullName.trim();
    const normalizedPhone = phoneNumber.trim();

    if (normalizedName.length < 2 || normalizedName.length > 100) {
      setError("השם חייב להכיל בין 2 ל-100 תווים.");
      return;
    }

    if (normalizedPhone.length < 9 || normalizedPhone.length > 20) {
      setError("מספר הטלפון חייב להכיל בין 9 ל-20 תווים.");
      return;
    }

    if (selectedHelpIds.length === 0) {
      setError("יש לבחור לפחות סוג עזרה אחד.");
      return;
    }

    try {
      setSaving(true);
      setError("");
      setSuccessMessage("");

      await apiRequest(
        `/api/Volunteers/admin/${editingVolunteer.id}`,
        {
          method: "PUT",
          body: JSON.stringify({
            fullName: normalizedName,
            phoneNumber: normalizedPhone,
            equipmentTypeIds: selectedEquipmentIds,
            helpTypeIds: selectedHelpIds,
          }),
        }
      );

      setSuccessMessage("פרטי המתנדב עודכנו בהצלחה.");

      cancelEditing();
      await loadData();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בשמירת פרטי המתנדב"
      );
    } finally {
      setSaving(false);
    }
  }

  const filteredVolunteers = useMemo(() => {
    return volunteers.filter((volunteer) => {
      const matchesSearch =
        volunteer.fullName.toLowerCase().includes(search.toLowerCase()) ||
        volunteer.phoneNumber.includes(search);

      const matchesAvailability =
        availability === "all" ||
        (availability === "active" && volunteer.isActive) ||
        (availability === "inactive" && !volunteer.isActive);

      return matchesSearch && matchesAvailability;
    });
  }, [volunteers, search, availability]);

  const activeCount = volunteers.filter(
    (v) => v.isActive
  ).length;

  if (loading) {
    return (
      <div className="admin-volunteers-message">
        טוען מתנדבים...
      </div>
    );
  }

  return (
    <div className="admin-volunteers" dir="rtl">
      <div className="volunteers-heading">
        <div>
          <h2>ניהול מתנדבים</h2>
          <p>צפייה, מעקב ועריכת פרטי המתנדבים במערכת</p>
        </div>

        <button onClick={loadData} className="refresh-button">
          רענון נתונים
        </button>
      </div>

      {error && (
        <div className="volunteers-error">
          {error}
        </div>
      )}

      {successMessage && (
        <div className="volunteers-success">
          {successMessage}
        </div>
      )}

      {editingVolunteer && (
        <form className="volunteer-edit-form" onSubmit={handleSave}>
          <h3>עריכת מתנדב: {editingVolunteer.fullName}</h3>

          <div className="volunteer-edit-fields">
            <label>
              שם מלא
              <input
                type="text"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                minLength={2}
                maxLength={100}
                required
              />
            </label>

            <label>
              מספר טלפון
              <input
                type="tel"
                value={phoneNumber}
                onChange={(e) => setPhoneNumber(e.target.value)}
                minLength={9}
                maxLength={20}
                required
              />
            </label>
          </div>

          <div className="volunteer-edit-section">
            <h4>סוגי עזרה</h4>

            <div className="volunteer-checkbox-list">
              {helpTypes.map((type) => (
                <label key={type.id}>
                  <input
                    type="checkbox"
                    checked={selectedHelpIds.includes(type.id)}
                    onChange={() => toggleHelpType(type.id)}
                  />
                  {type.name}
                </label>
              ))}

              {helpTypes.length === 0 && (
                <p>לא הוגדרו סוגי עזרה במערכת.</p>
              )}
            </div>
          </div>

          <div className="volunteer-edit-section">
            <h4>סוגי ציוד</h4>

            <div className="volunteer-checkbox-list">
              {equipmentTypes.map((type) => (
                <label key={type.id}>
                  <input
                    type="checkbox"
                    checked={selectedEquipmentIds.includes(type.id)}
                    onChange={() => toggleEquipment(type.id)}
                  />
                  {type.name}
                </label>
              ))}

              {equipmentTypes.length === 0 && (
                <p>לא הוגדרו סוגי ציוד במערכת.</p>
              )}
            </div>
          </div>

          <div className="volunteer-edit-actions">
            <button type="submit" disabled={saving}>
              {saving ? "שומר..." : "שמירת שינויים"}
            </button>

            <button
              type="button"
              className="volunteer-edit-cancel"
              onClick={cancelEditing}
              disabled={saving}
            >
              ביטול
            </button>
          </div>
        </form>
      )}

      <div className="volunteers-stats">
        <div className="volunteer-stat-card">
          <span>סה״כ מתנדבים</span>
          <strong>{volunteers.length}</strong>
        </div>

        <div className="volunteer-stat-card">
          <span>מתנדבים זמינים</span>
          <strong>{activeCount}</strong>
        </div>

        <div className="volunteer-stat-card">
          <span>לא זמינים</span>
          <strong>{volunteers.length - activeCount}</strong>
        </div>
      </div>

      <div className="volunteers-filters">
        <input
          type="text"
          placeholder="חיפוש לפי שם או טלפון..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />

        <select
          value={availability}
          onChange={(e) => setAvailability(e.target.value)}
        >
          <option value="all">כל המתנדבים</option>
          <option value="active">זמינים בלבד</option>
          <option value="inactive">לא זמינים בלבד</option>
        </select>
      </div>

      <div className="volunteers-table-wrapper">
        <table className="volunteers-table">
          <thead>
            <tr>
              <th>שם המתנדב</th>
              <th>טלפון</th>
              <th>זמינות</th>
              <th>הצעות שהתקבלו</th>
              <th>הצעות שאושרו</th>
              <th>קריאות שהושלמו</th>
              <th>ציון אמינות</th>
              <th>מיקום</th>
              <th>פעולות</th>
            </tr>
          </thead>

          <tbody>
            {filteredVolunteers.map((volunteer) => (
              <tr key={volunteer.id}>
                <td>{volunteer.fullName}</td>
                <td>{volunteer.phoneNumber}</td>

                <td>
                  <span
                    className={
                      volunteer.isActive
                        ? "availability-badge active"
                        : "availability-badge inactive"
                    }
                  >
                    {volunteer.isActive ? "זמין" : "לא זמין"}
                  </span>
                </td>

                <td>{volunteer.totalOffersReceived}</td>
                <td>{volunteer.totalOffersAccepted}</td>
                <td>{volunteer.totalCompleted}</td>
                <td>{volunteer.reliabilityScore.toFixed(1)}%</td>

                <td>
                  {volunteer.latitude != null &&
                  volunteer.longitude != null
                    ? "קיים מיקום"
                    : "לא דווח מיקום"}
                </td>

                <td>
                  <button
                    type="button"
                    className="volunteer-edit-button"
                    onClick={() => startEditing(volunteer)}
                  >
                    עריכה
                  </button>
                </td>
              </tr>
            ))}

            {filteredVolunteers.length === 0 && (
              <tr>
                <td colSpan={9} className="empty-volunteers">
                  לא נמצאו מתנדבים להצגה
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
