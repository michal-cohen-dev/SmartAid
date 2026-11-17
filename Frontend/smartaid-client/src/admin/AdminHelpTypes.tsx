
import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { apiRequest } from "../services/api";
import "./AdminHelpTypes.css";

interface HelpType {
  id: number;
  name: string;
}

export default function AdminHelpTypes() {
  const [helpTypes, setHelpTypes] = useState<HelpType[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [name, setName] = useState("");
  const [editingId, setEditingId] = useState<number | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadHelpTypes();
  }, []);

  async function loadHelpTypes() {
    try {
      setLoading(true);
      setError("");

      const data = await apiRequest<HelpType[]>("/api/HelpTypes");
      setHelpTypes(data);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בטעינת סוגי העזרה"
      );
    } finally {
      setLoading(false);
    }
  }

  function resetForm() {
    setName("");
    setEditingId(null);
  }

  function startEditing(item: HelpType) {
    setName(item.name);
    setEditingId(item.id);
    setError("");
    setSuccessMessage("");
  }

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();

    const normalizedName = name.trim();

    if (normalizedName.length < 2 || normalizedName.length > 100) {
      setError("שם סוג העזרה חייב להכיל בין 2 ל-100 תווים.");
      return;
    }

    try {
      setSaving(true);
      setError("");
      setSuccessMessage("");

      if (editingId === null) {
        await apiRequest("/api/HelpTypes", {
          method: "POST",
          body: JSON.stringify({
            name: normalizedName,
          }),
        });

        setSuccessMessage("סוג העזרה נוסף בהצלחה.");
      } else {
        await apiRequest(`/api/HelpTypes/${editingId}`, {
          method: "PUT",
          body: JSON.stringify({
            id: editingId,
            name: normalizedName,
          }),
        });

        setSuccessMessage("סוג העזרה עודכן בהצלחה.");
      }

      resetForm();
      await loadHelpTypes();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "לא ניתן לשמור את סוג העזרה"
      );
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(item: HelpType) {
    const confirmed = window.confirm(
      `האם למחוק את סוג העזרה "${item.name}"?`
    );

    if (!confirmed) return;

    try {
      setError("");
      setSuccessMessage("");

      await apiRequest(`/api/HelpTypes/${item.id}`, {
        method: "DELETE",
      });

      setSuccessMessage("סוג העזרה נמחק בהצלחה.");
      await loadHelpTypes();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "לא ניתן למחוק את סוג העזרה. ייתכן שהוא נמצא בשימוש."
      );
    }
  }

  if (loading) {
    return (
      <div className="admin-help-types-message">
        טוען סוגי עזרה...
      </div>
    );
  }

  return (
    <div className="admin-help-types" dir="rtl">
      <div className="help-types-heading">
        <div>
          <h2>ניהול סוגי עזרה</h2>
          <p>ניהול סוגי העזרה הזמינים במערכת</p>
        </div>

        <button
          type="button"
          className="help-types-refresh-button"
          onClick={loadHelpTypes}
        >
          רענון נתונים
        </button>
      </div>

      {error && (
        <div className="help-types-error">
          {error}
        </div>
      )}

      {successMessage && (
        <div className="help-types-success">
          {successMessage}
        </div>
      )}

      <div className="help-types-stat-card">
        <span>סה״כ סוגי עזרה</span>
        <strong>{helpTypes.length}</strong>
      </div>

      <form className="help-types-form" onSubmit={handleSubmit}>
        <h3>
          {editingId === null
            ? "הוספת סוג עזרה"
            : "עריכת סוג עזרה"}
        </h3>

        <div className="help-types-form-row">
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="לדוגמה: סיוע בתיקון רכב"
            minLength={2}
            maxLength={100}
            required
          />

          <button type="submit" disabled={saving}>
            {saving
              ? "שומר..."
              : editingId === null
                ? "הוספה"
                : "שמירת שינויים"}
          </button>

          {editingId !== null && (
            <button
              type="button"
              className="help-types-cancel-button"
              onClick={resetForm}
              disabled={saving}
            >
              ביטול
            </button>
          )}
        </div>
      </form>

      <div className="help-types-table-wrapper">
        <table className="help-types-table">
          <thead>
            <tr>
              <th>מזהה</th>
              <th>שם סוג העזרה</th>
              <th>פעולות</th>
            </tr>
          </thead>

          <tbody>
            {helpTypes.map((item) => (
              <tr key={item.id}>
                <td>#{item.id}</td>
                <td>{item.name}</td>

                <td className="help-types-actions">
                  <button
                    type="button"
                    onClick={() => startEditing(item)}
                  >
                    עריכה
                  </button>

                  <button
                    type="button"
                    className="help-types-delete-button"
                    onClick={() => handleDelete(item)}
                  >
                    מחיקה
                  </button>
                </td>
              </tr>
            ))}

            {helpTypes.length === 0 && (
              <tr>
                <td colSpan={3} className="empty-help-types">
                  לא הוגדרו סוגי עזרה במערכת
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
