
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { apiRequest } from "../services/api";
import "./MyRequestsScreen.css";

interface HelpRequest {
  id: number;
  title: string;
  description: string;
  createdAt: string;
  status: number;
  address?: string;
}

const statusDetails: Record<number, { label: string; className: string }> = {
  0: { label: "פתוחה", className: "open" },
  1: { label: "שויך אליך", className: "assigned" },
  2: { label: "הושלמה", className: "completed" },
  3: { label: "בוטלה", className: "cancelled" },
  4: { label: "בטיפול", className: "assigned" },
};

function VolunteerHistoryScreen() {
  const navigate = useNavigate();

  const [requests, setRequests] = useState<HelpRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    document.title = "היסטוריית ההתנדבות שלי | SmartAid";

    const loadRequests = async () => {
      try {
        const data = await apiRequest<HelpRequest[]>(
          "/api/HelpRequests/volunteer/my"
        );
        setRequests(data);
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : "אירעה שגיאה בטעינת היסטוריית ההתנדבות"
        );
      } finally {
        setLoading(false);
      }
    };

    loadRequests();
  }, []);

  const formatDate = (date: string) =>
    new Date(date).toLocaleDateString("he-IL");

  return (
    <main className="my-requests-screen" dir="rtl">
      <header className="my-requests-header">
        <div>
          <span className="my-requests-eyebrow">SmartAid</span>
          <h1>היסטוריית ההתנדבות שלי</h1>
          <p>כאן אפשר לצפות בכל בקשות העזרה ששויכו אליך.</p>
        </div>
      </header>

      {loading && (
        <div className="my-requests-message">
          טוען את היסטוריית ההתנדבות...
        </div>
      )}

      {!loading && error && (
        <div className="my-requests-message error">
          {error}
        </div>
      )}

      {!loading && !error && requests.length === 0 && (
        <div className="my-requests-empty">
          <div className="my-requests-empty-icon">📋</div>
          <h2>עדיין אין לך קריאות</h2>
          <p>בקשות שתקבל יופיעו כאן.</p>
        </div>
      )}

      {!loading && !error && requests.length > 0 && (
        <>
          <div className="my-requests-count">
            נמצאו {requests.length} קריאות
          </div>

          <div className="my-requests-list">
            {requests.map((request) => {
              const status = statusDetails[request.status] ?? {
                label: "לא ידוע",
                className: "unknown",
              };

              return (
                <article className="my-request-card" key={request.id}>
                  <div className="my-request-card-top">
                    <span className="my-request-id">
                      קריאה #{request.id}
                    </span>

                    <span className={`my-request-status ${status.className}`}>
                      {status.label}
                    </span>
                  </div>

                  <h2>{request.title}</h2>

                  <p className="my-request-description">
                    {request.description}
                  </p>

                  <div className="my-request-meta">
                    <span>📅 {formatDate(request.createdAt)}</span>
                    {request.address && (
                      <span>📍 {request.address}</span>
                    )}
                  </div>

                  <button
                    className="my-request-track-button"
                    onClick={() =>
                      navigate(`/request-details/${request.id}`)
                    }
                  >
                    צפייה בפרטי הבקשה
                  </button>
                </article>
              );
            })}
          </div>
        </>
      )}
    </main>
  );
}

export default VolunteerHistoryScreen;
