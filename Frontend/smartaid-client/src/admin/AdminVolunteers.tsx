
import { useEffect, useMemo, useState } from "react";
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
  equipmentTypeNames: string[];
  helpTypeNames: string[];
}

export default function AdminVolunteers() {
  const [volunteers, setVolunteers] = useState<Volunteer[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [availability, setAvailability] = useState("all");

  useEffect(() => {
    loadVolunteers();
  }, []);

  async function loadVolunteers() {
    try {
      setLoading(true);
      setError("");

      const data = await apiRequest<Volunteer[]>("/api/Volunteers");
      setVolunteers(data);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בטעינת המתנדבים"
      );
    } finally {
      setLoading(false);
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

  const activeCount = volunteers.filter(v => v.isActive).length;

  if (loading) {
    return <div className="admin-volunteers-message">טוען מתנדבים...</div>;
  }

  return (
    <div className="admin-volunteers" dir="rtl">
      <div className="volunteers-heading">
        <div>
          <h2>ניהול מתנדבים</h2>
          <p>צפייה ומעקב אחר המתנדבים במערכת</p>
        </div>

        <button onClick={loadVolunteers} className="refresh-button">
          רענון נתונים
        </button>
      </div>

      {error && (
        <div className="volunteers-error">
          {error}
        </div>
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
              </tr>
            ))}

            {filteredVolunteers.length === 0 && (
              <tr>
                <td colSpan={8} className="empty-volunteers">
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
