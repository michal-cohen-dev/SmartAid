
import { useEffect, useMemo, useState } from "react";
import { apiRequest } from "../services/api";
import "./AdminUsers.css";

interface User {
  id: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  createdAt: string;
}

interface UserUpdate {
  fullName: string;
  email: string;
  phoneNumber: string;
}

export default function AdminUsers() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState<UserUpdate>({
    fullName: "",
    email: "",
    phoneNumber: "",
  });

  useEffect(() => {
    loadUsers();
  }, []);

  async function loadUsers() {
    try {
      setLoading(true);
      setError("");

      const data = await apiRequest<User[]>("/api/Users/admin");
      setUsers(data);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בטעינת המשתמשים"
      );
    } finally {
      setLoading(false);
    }
  }

  const filteredUsers = useMemo(() => {
    const value = search.trim().toLowerCase();

    if (!value) return users;

    return users.filter(
      (user) =>
        user.fullName.toLowerCase().includes(value) ||
        user.email.toLowerCase().includes(value) ||
        user.phoneNumber.includes(value) ||
        user.id.toString().includes(value)
    );
  }, [users, search]);

  function startEditing(user: User) {
    setEditingUser(user);
    setForm({
      fullName: user.fullName,
      email: user.email,
      phoneNumber: user.phoneNumber,
    });
    setError("");
  }

  function cancelEditing() {
    setEditingUser(null);
    setForm({
      fullName: "",
      email: "",
      phoneNumber: "",
    });
  }

  async function saveUser(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!editingUser) return;

    try {
      setSaving(true);
      setError("");

      await apiRequest(
        `/api/Users/admin/${editingUser.id}`,
        {
          method: "PUT",
          body: JSON.stringify(form),
        }
      );

      cancelEditing();
      await loadUsers();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בעדכון המשתמש"
      );
    } finally {
      setSaving(false);
    }
  }

  async function deleteUser(user: User) {
    const confirmed = window.confirm(
      `האם את בטוחה שברצונך למחוק את המשתמש ${user.fullName}?`
    );

    if (!confirmed) return;

    try {
      setError("");

      await apiRequest(
        `/api/Users/admin/${user.id}`,
        { method: "DELETE" }
      );

      if (editingUser?.id === user.id) {
        cancelEditing();
      }

      await loadUsers();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה במחיקת המשתמש"
      );
    }
  }

  if (loading) {
    return (
      <div className="admin-users-message">
        טוען משתמשים...
      </div>
    );
  }

  return (
    <div className="admin-users" dir="rtl">
      <div className="users-heading">
        <div>
          <h2>ניהול משתמשים</h2>
          <p>צפייה, חיפוש ועדכון פרטי המשתמשים במערכת</p>
        </div>

        <button
          onClick={loadUsers}
          className="users-refresh-button"
        >
          רענון נתונים
        </button>
      </div>

      {error && (
        <div className="users-error">
          {error}
        </div>
      )}

      <div className="users-stats">
        <div className="user-stat-card">
          <span>סה״כ משתמשים</span>
          <strong>{users.length}</strong>
        </div>
      </div>

      <div className="users-filters">
        <input
          type="text"
          placeholder="חיפוש לפי שם, דוא״ל, טלפון או מזהה..."
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
      </div>

      {editingUser && (
        <form className="user-edit-form" onSubmit={saveUser}>
          <h3>עריכת פרטי משתמש</h3>

          <label>
            שם מלא
            <input
              type="text"
              required
              minLength={2}
              maxLength={100}
              value={form.fullName}
              onChange={(event) =>
                setForm({ ...form, fullName: event.target.value })
              }
            />
          </label>

          <label>
            דוא״ל
            <input
              type="email"
              required
              maxLength={254}
              value={form.email}
              onChange={(event) =>
                setForm({ ...form, email: event.target.value })
              }
            />
          </label>

          <label>
            טלפון
            <input
              type="tel"
              required
              minLength={9}
              maxLength={20}
              value={form.phoneNumber}
              onChange={(event) =>
                setForm({ ...form, phoneNumber: event.target.value })
              }
            />
          </label>

          <div className="user-edit-actions">
            <button type="submit" disabled={saving}>
              {saving ? "שומר..." : "שמירת שינויים"}
            </button>

            <button
              type="button"
              className="cancel-edit-button"
              onClick={cancelEditing}
              disabled={saving}
            >
              ביטול
            </button>
          </div>
        </form>
      )}

      <div className="users-table-wrapper">
        <table className="users-table">
          <thead>
            <tr>
              <th>מזהה</th>
              <th>שם מלא</th>
              <th>דוא״ל</th>
              <th>טלפון</th>
              <th>תאריך הרשמה</th>
              <th>פעולות</th>
            </tr>
          </thead>

          <tbody>
            {filteredUsers.map((user) => (
              <tr key={user.id}>
                <td>#{user.id}</td>
                <td>{user.fullName}</td>
                <td>{user.email}</td>
                <td>{user.phoneNumber}</td>
                <td>
                  {new Date(user.createdAt).toLocaleDateString("he-IL")}
                </td>
                <td>
                  <div className="user-actions">
                    <button
                      className="edit-user-button"
                      onClick={() => startEditing(user)}
                    >
                      עריכה
                    </button>

                    <button
                      className="delete-user-button"
                      onClick={() => deleteUser(user)}
                    >
                      מחיקה
                    </button>
                  </div>
                </td>
              </tr>
            ))}

            {filteredUsers.length === 0 && (
              <tr>
                <td colSpan={6} className="empty-users">
                  לא נמצאו משתמשים להצגה
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
