
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

function VolunteerCompletedRequestsScreen() {
  const navigate = useNavigate();

  const [requests, setRequests] = useState<HelpRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    document.title = "קריאות שטופלו כבר | SmartAid";

    const loadRequests = async () => {
      try {
        const data = await apiRequest<HelpRequest[]>(
          "/api/HelpRequests/volunteer/my"
        );

        const completedRequests = (Array.isArray(data) ? data : []).filter(
          (request) => request.status === 2
        );

        setRequests(completedRequests);
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : "אירעה שגיאה בטעינת הקריאות שהושלמו"
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
          <h1>קריאות שטופלו כבר</h1>
          <p>כאן אפשר לצפות בכל קריאות העזרה שהשלמת.</p>
        </div>
      </header>

      {loading && (
        <div className="my-requests-message">
          טוען את הקריאות שהושלמו...
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
          <h2>עדיין אין לך קריאות שהושלמו</h2>
          <p>קריאות שתסיים לטפל בהן יופיעו כאן.</p>
        </div>
      )}

      {!loading && !error && requests.length > 0 && (
        <>
          <div className="my-requests-count">
            נמצאו {requests.length} קריאות שהושלמו
          </div>

          <div className="my-requests-list">
            {requests.map((request) => (
              <article className="my-request-card" key={request.id}>
                <div className="my-request-card-top">
                  <span className="my-request-id">
                    קריאה #{request.id}
                  </span>

                  <span className="my-request-status completed">
                    הושלמה
                  </span>
                </div>

                <h2>{request.title}</h2>

                <p className="my-request-description">
                  {request.description}
                </p>

                <div className="my-request-meta">
                  <span>
                    📅 {formatDate(request.createdAt)}
                  </span>

                  {request.address && (
                    <span>📍 {request.address}</span>
                  )}
                </div>

                <button
                  className="my-requests-new-button"
                  onClick={() =>
                    navigate(`/request-details/${request.id}`)
                  }
                >
                  צפייה בפרטי הקריאה
                </button>
              </article>
            ))}
          </div>
        </>
      )}
    </main>
  );
}

export default VolunteerCompletedRequestsScreen;
