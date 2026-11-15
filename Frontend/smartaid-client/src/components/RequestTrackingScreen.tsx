
import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { apiRequest } from "../services/api";
import "./RequestTrackingScreen.css";

interface HelpRequest {
  id: number;
  title: string;
  description: string;
  createdAt: string;
  status: number;
  address: string;
  assignedVolunteerName?: string | null;
  rating?: number | null;
}

function RequestTrackingScreen() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [request, setRequest] = useState<HelpRequest | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [selectedRating, setSelectedRating] = useState(0);
  const [hoveredRating, setHoveredRating] = useState(0);
  const [ratingLoading, setRatingLoading] = useState(false);
  const [ratingMessage, setRatingMessage] = useState("");

useEffect(() => {
  document.title = "מעקב אחר בקשת עזרה | SmartAid";

  if (!id || !Number.isInteger(Number(id)) || Number(id) <= 0) {
    setError("מזהה הבקשה אינו תקין.");
    setLoading(false);
    return;
  }

  let isMounted = true;
  let intervalId: number | undefined;

  const loadRequest = async (showLoading = false) => {
    if (showLoading) {
      setLoading(true);
    }

    try {
      const data = await apiRequest<HelpRequest>(
        `/api/HelpRequests/${id}`
      );

      if (isMounted) {
        setRequest(data);
        setError("");

        // מפסיקים לרענן כשהקריאה הסתיימה או בוטלה
        if (data.status === 2 || data.status === 3) {
          if (intervalId !== undefined) {
            window.clearInterval(intervalId);
          }
        }
      }
    } catch (err) {
      if (isMounted) {
        setError(
          err instanceof Error
            ? err.message
            : "לא ניתן לטעון את פרטי הבקשה."
        );
      }
    } finally {
      if (isMounted && showLoading) {
        setLoading(false);
      }
    }
  };

  loadRequest(true);

  intervalId = window.setInterval(() => {
    loadRequest();
  }, 5000);

  return () => {
    isMounted = false;

    if (intervalId !== undefined) {
      window.clearInterval(intervalId);
    }
  };
}, [id]);

  const submitRating = async () => {
    if (!id || selectedRating < 1 || selectedRating > 5) {
      return;
    }

    setRatingLoading(true);
    setRatingMessage("");

    try {
      await apiRequest(`/api/HelpRequests/${id}/rate`, {
        method: "POST",
        body: JSON.stringify({
          rating: selectedRating,
        }),
      });

      setRequest((previous) =>
        previous
          ? { ...previous, rating: selectedRating }
          : previous
      );

      setRatingMessage("תודה! הדירוג שלך נשמר בהצלחה.");
    } catch (err) {
      setRatingMessage(
        err instanceof Error
          ? err.message
          : "לא ניתן לשמור את הדירוג."
      );
    } finally {
      setRatingLoading(false);
    }
  };

  const statusInfo: Record<
    number,
    { title: string; description: string; icon: string }
  > = {
    0: {
      title: "מחפשים עבורך מתנדב",
      description: "בקשת העזרה התקבלה במערכת ומחפשים עבורך מתנדב מתאים.",
      icon: "🔍",
    },
    1: {
      title: "המתנדב בדרך אליך",
      description: "המתנדב אישר את הקריאה והוא בדרך אליך.",
      icon: "🚗",
    },
    2: {
      title: "הטיפול הסתיים",
      description: "בקשת העזרה שלך הושלמה בהצלחה.",
      icon: "✓",
    },
    3: {
      title: "הבקשה בוטלה",
      description: "בקשת העזרה הזו בוטלה.",
      icon: "✕",
    },
    4: {
      title: "המתנדב הגיע ומטפל בבקשה",
      description: "המתנדב הגיע למקום והחל בטיפול בבקשת העזרה שלך.",
      icon: "🙋",
    },
  };

  if (loading) {
    return (
      <main className="tracking-page" dir="rtl">
        <div className="tracking-container">
          <p>טוען את פרטי הבקשה...</p>
        </div>
      </main>
    );
  }

  if (error || !request) {
    return (
      <main className="tracking-page" dir="rtl">
        <div className="tracking-container">
          <h2>לא ניתן להציג את המעקב</h2>
          <p>{error || "הבקשה לא נמצאה."}</p>
          <button onClick={() => navigate("/my-requests")}>
            חזרה לקריאות שלי
          </button>
        </div>
      </main>
    );
  }

  const currentStatus = statusInfo[request.status] ?? {
    title: "סטטוס לא ידוע",
    description: "לא ניתן לזהות את מצב הבקשה.",
    icon: "ℹ️",
  };

  const isCancelled = request.status === 3;

  const timelineSteps = [
    {
      title: "הבקשה נשלחה בהצלחה",
      description: "בקשת העזרה שלך התקבלה במערכת.",
      icon: "📩",
    },
    {
      title: "מחפשים עבורך מתנדב",
      description: "המערכת מחפשת מתנדב מתאים לבקשה שלך.",
      icon: "🔍",
    },
    {
      title: "המתנדב בדרך אליך",
      description: "המתנדב אישר את הקריאה והוא בדרכו אליך.",
      icon: "🚗",
    },
    {
      title: "המתנדב הגיע ומטפל בבקשה",
      description: "המתנדב הגיע למקום והחל בטיפול.",
      icon: "🙋",
    },
    {
      title: "הטיפול הסתיים",
      description: "בקשת העזרה הושלמה בהצלחה.",
      icon: "✓",
    },
  ];

  const currentStep =
    request.status === 0 ? 1 :
    request.status === 1 ? 2 :
    request.status === 4 ? 3 :
    request.status === 2 ? 4 : -1;

  return (
    <div dir="rtl" className="tracking-page">
      <header className="tracking-header">
        <h1>SmartAid</h1>
        <p>עזרה חכמה. קהילה חזקה.</p>
      </header>

      <main className="tracking-container">
        <div className="tracking-heading">
          <span className="tracking-badge">
            מעקב אחר בקשת עזרה
          </span>
          <h2>מעקב אחר הבקשה שלך</h2>
          <p>
            כאן ניתן לראות את מצב הטיפול בבקשת העזרה שלך.
          </p>
        </div>

        <div className="tracking-request-card">
          <div className="tracking-request-icon">📋</div>
          <div>
            <h3>{request.title}</h3>
            <p>בקשה מספר {request.id}</p>
            {request.address && (
              <span>📍 {request.address}</span>
            )}
          </div>
        </div>

        <section className="current-status-card">
          <span className="current-status-icon">
            {currentStatus.icon}
          </span>

          <div>
            <span className="status-label">
              הסטטוס הנוכחי
            </span>
            <h2>{currentStatus.title}</h2>
            <p>{currentStatus.description}</p>
          </div>
        </section>

        {!isCancelled && (
          <section className="tracking-timeline">
            <h2>התקדמות הבקשה</h2>

            {timelineSteps.map((step, index) => (
              <div
                className={`timeline-item ${
                  request.status === 2 || index < currentStep
                    ? "completed"
                    : index === currentStep
                    ? "active"
                    : "pending"
                }`}
                key={step.title}
              >
                <div className="timeline-icon">
                  {index < currentStep || request.status === 2
                    ? "✓"
                    : step.icon}
                </div>

                <div className="timeline-content">
                  <h3>{step.title}</h3>
                  <p>{step.description}</p>
                </div>
              </div>
            ))}
          </section>
        )}

        {request.status === 2 && (
          <section className="rating-card">
            <h2>איך היה השירות שקיבלת?</h2>

            {request.rating != null ? (
              <>
                <p>הדירוג שלך נשמר בהצלחה:</p>
                <div className="rating-stars">
                  {"★".repeat(request.rating)}
                  {"☆".repeat(5 - request.rating)}
                </div>
              </>
            ) : (
              <>
                <p>נשמח שתדרג את המתנדב שעזר לך.</p>

                <div
                  className="rating-stars"
                  onMouseLeave={() => setHoveredRating(0)}
                >
                  {[1, 2, 3, 4, 5].map((rating) => (
                    <button
                      key={rating}
                      type="button"
                      className={
                        rating <= (hoveredRating || selectedRating)
                          ? "rating-star selected"
                          : "rating-star"
                      }
                      onMouseEnter={() => setHoveredRating(rating)}
                      onFocus={() => setHoveredRating(rating)}
                      onBlur={() => setHoveredRating(0)}
                      onClick={() => setSelectedRating(rating)}
                      disabled={ratingLoading}
                      aria-label={`דירוג ${rating} מתוך 5`}
                      aria-pressed={selectedRating === rating}
                    >
                      ★
                    </button>
                  ))}
                </div>

                <button
                  type="button"
                  className="tracking-back-button"
                  onClick={submitRating}
                  disabled={selectedRating === 0 || ratingLoading}
                >
                  {ratingLoading ? "שומר דירוג..." : "שליחת דירוג"}
                </button>

                {ratingMessage && (
                  <p className="rating-message">{ratingMessage}</p>
                )}
              </>
            )}
          </section>
        )}

        {request.assignedVolunteerName && (
  <section className="volunteer-info-card">
    <div className="volunteer-info-icon">🙋</div>

    <div className="volunteer-info-content">
      <span className="volunteer-info-label">
        המתנדב שמטפל בבקשה שלך
      </span>

      <h3>{request.assignedVolunteerName}</h3>

      <p>
        {request.status === 1
          ? "המתנדב אישר את הבקשה ונמצא בדרכו אליך."
          : request.status === 4
          ? "המתנדב הגיע אליך ומטפל בבקשה."
          : request.status === 2
          ? "המתנדב השלים את הטיפול בבקשה."
          : "המתנדב משויך לבקשת העזרה שלך."}
      </p>
    </div>
  </section>
)}

        {isCancelled && (
          <div className="tracking-info">
            <span>ℹ️</span>
            <p>הבקשה בוטלה. ניתן לפתוח בקשת עזרה חדשה במידת הצורך.</p>
          </div>
        )}

        <button
          className="tracking-back-button"
          onClick={() => navigate("/my-requests")}
        >
          חזרה לקריאות שלי
        </button>
      </main>
    </div>
  );
}

export default RequestTrackingScreen;
