
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
  address: string;
  assignedVolunteerName?: string | null;
}

const statusDetails: Record<number, { label: string; className: string }> = {
  0: { label: "פתוחה", className: "open" },
  1: { label: "שויך מתנדב", className: "assigned" },
  2: { label: "הושלמה", className: "completed" },
  3: { label: "בוטלה", className: "cancelled" },
};

function MyRequestsScreen() {
  const navigate = useNavigate();

  const [requests, setRequests] = useState<HelpRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    document.title = "הקריאות הקודמות שלי | SmartAid";

    const loadRequests = async () => {
      try {
        const data = await apiRequest<HelpRequest[]>("/api/HelpRequests/my");
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
    };

    loadRequests();
  }, []);

  const formatDate = (date: string) =>
    new Date(date).toLocaleDateString("he-IL", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    });

  return (
    <main className="my-requests-screen" dir="rtl">
      <header className="my-requests-header">
        <div>
          <span className="my-requests-eyebrow">SmartAid</span>
          <h1>הקריאות הקודמות שלי</h1>
          <p>כאן אפשר לצפות בכל בקשות העזרה שפתחת.</p>
        </div>

        <button
          className="my-requests-new-button"
          onClick={() => navigate("/request")}
        >
          + בקשת עזרה חדשה
        </button>
      </header>

      {loading && (
        <div className="my-requests-message">
          טוען את הקריאות שלך...
        </div>
      )}

      {!loading && error && (
        <div className="my-requests-message error">
          {error}
          <button onClick={() => window.location.reload()}>
            נסי שוב
          </button>
        </div>
      )}

      {!loading && !error && requests.length === 0 && (
        <div className="my-requests-empty">
          <div className="my-requests-empty-icon">📋</div>
          <h2>עדיין אין לך קריאות</h2>
          <p>כשתפתחי בקשת עזרה, היא תופיע כאן.</p>
          <button onClick={() => navigate("/request")}>
            פתיחת בקשת עזרה
          </button>
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
                <article
                  className="my-request-card"
                  key={request.id}
                >
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
  onClick={() => navigate(`/request-tracking/${request.id}`)}
>
  מעקב אחר הבקשה
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

export default MyRequestsScreen;
