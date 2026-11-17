
import { useEffect, useState } from "react";
import { apiRequest } from "../services/api";
import "./AdminEquipment.css";

interface EquipmentType {
  id: number;
  name: string;
}

export default function AdminEquipment() {
  const [equipmentTypes, setEquipmentTypes] = useState<EquipmentType[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [name, setName] = useState("");
  const [editingId, setEditingId] = useState<number | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadEquipmentTypes();
  }, []);

  async function loadEquipmentTypes() {
    try {
      setLoading(true);
      setError("");

      const data = await apiRequest<EquipmentType[]>(
        "/api/EquipmentTypes"
      );

      setEquipmentTypes(data);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בטעינת סוגי הציוד"
      );
    } finally {
      setLoading(false);
    }
  }

  function resetForm() {
    setName("");
    setEditingId(null);
  }

  function startEditing(item: EquipmentType) {
    setName(item.name);
    setEditingId(item.id);
  }

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();

    const normalizedName = name.trim();

    if (normalizedName.length < 2 || normalizedName.length > 100) {
      setError("שם הציוד חייב להכיל בין 2 ל-100 תווים.");
      return;
    }

    try {
      setSaving(true);
      setError("");

      if (editingId === null) {
        await apiRequest("/api/EquipmentTypes", {
          method: "POST",
          body: JSON.stringify({ name: normalizedName }),
        });
      } else {
        await apiRequest(`/api/EquipmentTypes/${editingId}`, {
          method: "PUT",
          body: JSON.stringify({
            id: editingId,
            name: normalizedName,
          }),
        });
      }

      resetForm();
      await loadEquipmentTypes();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "לא ניתן לשמור את סוג הציוד"
      );
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(item: EquipmentType) {
    const confirmed = window.confirm(
      `האם למחוק את סוג הציוד "${item.name}"?`
    );

    if (!confirmed) return;

    try {
      setError("");

      await apiRequest(`/api/EquipmentTypes/${item.id}`, {
        method: "DELETE",
      });

      await loadEquipmentTypes();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "לא ניתן למחוק את סוג הציוד"
      );
    }
  }

  if (loading) {
    return (
      <div className="admin-equipment-message">
        טוען סוגי ציוד...
      </div>
    );
  }

  return (
    <div className="admin-equipment" dir="rtl">
      <div className="equipment-heading">
        <div>
          <h2>ניהול ציוד</h2>
          <p>ניהול סוגי הציוד הזמינים במערכת</p>
        </div>

        <button
          className="equipment-refresh-button"
          onClick={loadEquipmentTypes}
        >
          רענון נתונים
        </button>
      </div>

      {error && (
        <div className="equipment-error">
          {error}
        </div>
      )}

      <div className="equipment-stat-card">
        <span>סה״כ סוגי ציוד</span>
        <strong>{equipmentTypes.length}</strong>
      </div>

      <form className="equipment-form" onSubmit={handleSubmit}>
        <h3>
          {editingId === null ? "הוספת סוג ציוד" : "עריכת סוג ציוד"}
        </h3>

        <div className="equipment-form-row">
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="לדוגמה: מגבה לרכב"
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
              className="equipment-cancel-button"
              onClick={resetForm}
            >
              ביטול
            </button>
          )}
        </div>
      </form>

      <div className="equipment-table-wrapper">
        <table className="equipment-table">
          <thead>
            <tr>
              <th>מזהה</th>
              <th>שם הציוד</th>
              <th>פעולות</th>
            </tr>
          </thead>

          <tbody>
            {equipmentTypes.map((item) => (
              <tr key={item.id}>
                <td>#{item.id}</td>
                <td>{item.name}</td>
                <td className="equipment-actions">
                  <button onClick={() => startEditing(item)}>
                    עריכה
                  </button>

                  <button
                    className="equipment-delete-button"
                    onClick={() => handleDelete(item)}
                  >
                    מחיקה
                  </button>
                </td>
              </tr>
            ))}

            {equipmentTypes.length === 0 && (
              <tr>
                <td colSpan={3} className="empty-equipment">
                  לא הוגדרו סוגי ציוד במערכת
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
