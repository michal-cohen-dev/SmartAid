
import { useEffect, useMemo, useState } from "react";
import { apiRequest } from "../services/api";
import "./AdminRequests.css";

interface HelpRequest {
  id: number;
  title: string;
  description: string;
  userFullName: string | null;
  address: string;
  status: number;
  assignedVolunteerName: string | null;
  createdAt: string;
  rating: number | null;
}

const statusLabels: Record<number, string> = {
  0: "פתוחה",
  1: "שויכה למתנדב",
  2: "הושלמה",
  3: "בוטלה",
  4: "בטיפול",
};

export default function AdminRequests() {
  const [requests, setRequests] = useState<HelpRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [cancellingId, setCancellingId] = useState<number | null>(null);

  useEffect(() => {
    loadRequests();
  }, []);

  async function loadRequests() {
    try {
      setLoading(true);
      setError("");

      const data = await apiRequest<HelpRequest[]>("/api/HelpRequests");
      setRequests(data);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בטעינת הקריאות"
      );
    } finally {
      setLoading(false);
    }
  }

  async function cancelRequest(request: HelpRequest) {
    const confirmed = window.confirm(
      `האם את בטוחה שברצונך לבטל את קריאה מספר ${request.id}?`
    );

    if (!confirmed) {
      return;
    }

    try {
      setCancellingId(request.id);
      setError("");
      setSuccessMessage("");

      await apiRequest(
        `/api/HelpRequests/${request.id}/admin-cancel`,
        {
          method: "POST",
        }
      );

      setSuccessMessage(
        `קריאה מספר ${request.id} בוטלה בהצלחה.`
      );

      await loadRequests();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בביטול הקריאה"
      );
    } finally {
      setCancellingId(null);
    }
  }

  const filteredRequests = useMemo(() => {
    return requests.filter((request) => {
      const searchText = search.toLowerCase();

      const matchesSearch =
        request.title.toLowerCase().includes(searchText) ||
        (request.userFullName ?? "").toLowerCase().includes(searchText) ||
        request.address.toLowerCase().includes(searchText);

      const matchesStatus =
        statusFilter === "all" ||
        request.status.toString() === statusFilter;

      return matchesSearch && matchesStatus;
    });
  }, [requests, search, statusFilter]);

  const countByStatus = (status: number) =>
    requests.filter((request) => request.status === status).length;

  if (loading) {
    return <div className="admin-requests-message">טוען קריאות...</div>;
  }

  return (
    <div className="admin-requests" dir="rtl">
      <div className="requests-heading">
        <div>
          <h2>ניהול קריאות</h2>
          <p>צפייה ומעקב אחר קריאות העזרה במערכת</p>
        </div>

        <button onClick={loadRequests} className="requests-refresh-button">
          רענון נתונים
        </button>
      </div>

      {error && <div className="requests-error">{error}</div>}

      {successMessage && (
        <div className="requests-success">
          {successMessage}
        </div>
      )}

      <div className="requests-stats">
        <div className="request-stat-card">
          <span>סה״כ קריאות</span>
          <strong>{requests.length}</strong>
        </div>

        <div className="request-stat-card">
          <span>קריאות פתוחות</span>
          <strong>{countByStatus(0)}</strong>
        </div>

        <div className="request-stat-card">
          <span>בטיפול</span>
          <strong>{countByStatus(4) + countByStatus(1)}</strong>
        </div>

        <div className="request-stat-card">
          <span>קריאות שהושלמו</span>
          <strong>{countByStatus(2)}</strong>
        </div>
      </div>

      <div className="requests-filters">
        <input
          type="text"
          placeholder="חיפוש לפי כותרת, שם או כתובת..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />

        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          <option value="all">כל הקריאות</option>
          <option value="0">פתוחות</option>
          <option value="1">שויכו למתנדב</option>
          <option value="4">בטיפול</option>
          <option value="2">הושלמו</option>
          <option value="3">בוטלו</option>
        </select>
      </div>

      <div className="requests-table-wrapper">
        <table className="requests-table">
          <thead>
            <tr>
              <th>מספר קריאה</th>
              <th>כותרת</th>
              <th>פותח הקריאה</th>
              <th>כתובת</th>
              <th>מתנדב משויך</th>
              <th>סטטוס</th>
              <th>תאריך יצירה</th>
              <th>פעולות</th>
            </tr>
          </thead>

          <tbody>
            {filteredRequests.map((request) => {
              const canCancel = [0, 1, 4].includes(request.status);

              return (
                <tr key={request.id}>
                  <td>#{request.id}</td>
                  <td>{request.title}</td>
                  <td>{request.userFullName || "לא צוין"}</td>
                  <td>{request.address || "לא צוינה כתובת"}</td>
                  <td>{request.assignedVolunteerName || "טרם שובץ"}</td>

                  <td>
                    <span className={`request-status status-${request.status}`}>
                      {statusLabels[request.status] ?? "לא ידוע"}
                    </span>
                  </td>

                  <td>
                    {new Date(request.createdAt).toLocaleDateString("he-IL")}
                  </td>

                  <td>
                    {canCancel ? (
                      <button
                        type="button"
                        className="requests-refresh-button requests-cancel-button"
                        onClick={() => cancelRequest(request)}
                        disabled={cancellingId !== null}
                      >
                        {cancellingId === request.id
                          ? "מבטל..."
                          : "ביטול קריאה"}
                      </button>
                    ) : (
                      <span>אין פעולות</span>
                    )}
                  </td>
                </tr>
              );
            })}

            {filteredRequests.length === 0 && (
              <tr>
                <td colSpan={8} className="empty-requests">
                  לא נמצאו קריאות להצגה
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
