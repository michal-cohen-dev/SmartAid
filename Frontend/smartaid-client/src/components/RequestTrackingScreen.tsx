
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
}

function RequestTrackingScreen() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [request, setRequest] = useState<HelpRequest | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    document.title = "מעקב אחר בקשת עזרה | SmartAid";

    const loadRequest = async () => {
      if (!id || !Number.isInteger(Number(id)) || Number(id) <= 0) {
        setError("מזהה הבקשה אינו תקין.");
        setLoading(false);
        return;
      }

      try {
        const data = await apiRequest<HelpRequest>(
          `/api/HelpRequests/${id}`
        );

        setRequest(data);
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : "לא ניתן לטעון את פרטי הבקשה."
        );
      } finally {
        setLoading(false);
      }
    };

    loadRequest();
  }, [id]);

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
      title: "הטיפול הסתיים",
      description: "בקשת העזרה הושלמה בהצלחה.",
      icon: "✓",
    },
  ];

  const currentStep =
    request.status === 0 ? 1 :
    request.status === 1 ? 2 :
    request.status === 2 ? 3 : -1;

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
                  {index < currentStep ? "✓" : step.icon}
                </div>

                <div className="timeline-content">
                  <h3>{step.title}</h3>
                  <p>{step.description}</p>
                </div>
              </div>
            ))}
          </section>
        )}

        {request.assignedVolunteerName && (
          <div className="tracking-info">
            <span>🙋</span>
            <p>
              המתנדב המשויך לבקשה:{" "}
              <strong>{request.assignedVolunteerName}</strong>
            </p>
          </div>
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
