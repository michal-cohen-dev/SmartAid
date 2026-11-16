
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
  const [actionError, setActionError] = useState("");
  const [cancellingId, setCancellingId] = useState<number | null>(null);

  useEffect(() => {
    document.title = "קריאות שנעניתי להן | SmartAid";

    let isMounted = true;

    const loadRequests = async (showLoading = false) => {
      try {
        if (showLoading && isMounted) {
          setLoading(true);
        }

        const data = await apiRequest<HelpRequest[]>(
          "/api/HelpRequests/volunteer/my"
        );

        if (isMounted) {
          setRequests(
            data.filter(
              request => request.status === 1 || request.status === 4
            )
          );
          setError("");
        }
      } catch (err) {
        if (isMounted) {
          setError(
            err instanceof Error
              ? err.message
              : "אירעה שגיאה בטעינת הקריאות."
          );
        }
      } finally {
        if (isMounted && showLoading) {
          setLoading(false);
        }
      }
    };

    loadRequests(true);

    const intervalId = window.setInterval(() => {
      loadRequests();
    }, 15000);

    return () => {
      isMounted = false;
      window.clearInterval(intervalId);
    };
  }, []);

  const handleCancelRequest = async (requestId: number) => {
    const confirmed = window.confirm(
      "האם את/ה בטוח/ה שברצונך לבטל את ההתנדבות לקריאה זו?"
    );

    if (!confirmed) {
      return;
    }

    try {
      setCancellingId(requestId);
      setActionError("");

      await apiRequest(
        `/api/HelpRequests/${requestId}/cancel`,
        {
          method: "POST",
        }
      );

      setRequests(current =>
        current.filter(request => request.id !== requestId)
      );
    } catch (err) {
      setActionError(
        err instanceof Error
          ? err.message
          : "לא ניתן לבטל את ההתנדבות כרגע. נסה/י שוב."
      );
    } finally {
      setCancellingId(null);
    }
  };

  return (
    <main className="my-requests-screen" dir="rtl">
      <header className="my-requests-header">
        <div>
          <span className="my-requests-eyebrow">SmartAid</span>
          <h1>קריאות שנעניתי להן</h1>
          <p>
            כאן מופיעות הקריאות שקיבלת ואתה עדיין מטפל בהן או בדרך אליהן.
          </p>
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

      {actionError && (
        <div className="my-requests-message error">
          {actionError}
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

                <div
                  style={{
                    display: "flex",
                    gap: "10px",
                    flexWrap: "wrap",
                    marginTop: "18px",
                  }}
                >
                  <button
                    className="my-request-track-button"
                    onClick={() =>
                      navigate(`/request-details/${request.id}`)
                    }
                  >
                    צפייה בפרטי הבקשה
                  </button>

                  <button
                    type="button"
                    onClick={() => handleCancelRequest(request.id)}
                    disabled={cancellingId === request.id}
                    style={{
                      border: "1px solid #d9534f",
                      borderRadius: "10px",
                      padding: "10px 16px",
                      background:
                        cancellingId === request.id ? "#f5b7b5" : "#fff",
                      color: "#c0392b",
                      fontWeight: 700,
                      cursor:
                        cancellingId === request.id
                          ? "not-allowed"
                          : "pointer",
                    }}
                  >
                    {cancellingId === request.id
                      ? "מבטל..."
                      : "ביטול התנדבות"}
                  </button>
                </div>
              </article>
            ))}
          </div>
        </>
      )}
    </main>
  );
}

export default VolunteerAcceptedRequestsScreen;
