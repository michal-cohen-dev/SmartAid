
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

function VolunteerAcceptedRequestsScreen() {
  const navigate = useNavigate();

  const [requests, setRequests] = useState<HelpRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    document.title = "קריאות שנעניתי להן | SmartAid";

    const loadRequests = async () => {
      try {
        const data = await apiRequest<HelpRequest[]>(
          "/api/HelpRequests/volunteer/my"
        );

        setRequests(data.filter(request => request.status === 1 || request.status === 4));
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : "אירעה שגיאה בטעינת הקריאות."
        );
      } finally {
        setLoading(false);
      }
    };

    loadRequests();
  }, []);

  return (
    <main className="my-requests-screen" dir="rtl">
      <header className="my-requests-header">
        <div>
          <span className="my-requests-eyebrow">SmartAid</span>
          <h1>קריאות שנעניתי להן</h1>
          <p>כאן מופיעות הקריאות שקיבלת ואתה עדיין מטפל בהן או בדרך אליהן.</p>
        </div>
      </header>

      {loading && (
        <div className="my-requests-message">
          טוען קריאות...
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
          <h2>אין כרגע קריאות שנענית להן</h2>
          <p>קריאות שתקבל יופיעו כאן.</p>
        </div>
      )}

      {!loading && !error && requests.length > 0 && (
        <>
          <div className="my-requests-count">
            נמצאו {requests.length} קריאות
          </div>

          <div className="my-requests-list">
            {requests.map(request => (
              <article className="my-request-card" key={request.id}>
                <div className="my-request-card-top">
                  <span className="my-request-id">
                    קריאה #{request.id}
                  </span>
                  <span className="my-request-status assigned">
                    {request.status === 4 ? "בטיפול" : "שויך אליך"}
                  </span>
                </div>

                <h2>{request.title}</h2>
                <p className="my-request-description">
                  {request.description}
                </p>

                <div className="my-request-meta">
                  <span>
                    📅 {new Date(request.createdAt).toLocaleDateString("he-IL")}
                  </span>
                  {request.address && (
                    <span>📍 {request.address}</span>
                  )}
                </div>

                <button
                  className="my-request-track-button"
                  onClick={() => navigate(`/request-details/${request.id}`)}
                >
                  צפייה בפרטי הבקשה
                </button>
              </article>
            ))}
          </div>
        </>
      )}
    </main>
  );
}

export default VolunteerAcceptedRequestsScreen;
