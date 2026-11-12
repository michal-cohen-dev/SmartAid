
import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { apiRequest } from "../services/api";
import "./AISuggestionsScreen.css";

interface HelpRequest {
  id: number;
  title: string;
  description: string;
  createdAt: string;
  status: number;
  address: string;
  assignedVolunteerName?: string | null;
}

function AISuggestionsScreen() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [request, setRequest] = useState<HelpRequest | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    document.title = "הצעות לפתרון עצמי | SmartAid";

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

  if (loading) {
    return (
      <main className="ai-page" dir="rtl">
        <div className="ai-container">
          <p>טוען את פרטי הבקשה...</p>
        </div>
      </main>
    );
  }

  if (error || !request) {
    return (
      <main className="ai-page" dir="rtl">
        <div className="ai-container">
          <h2>לא ניתן להציג את ההצעות</h2>
          <p>{error || "הבקשה לא נמצאה."}</p>
          <button
            type="button"
            className="volunteer-button"
            onClick={() => navigate("/my-requests")}
          >
            חזרה לקריאות שלי
          </button>
        </div>
      </main>
    );
  }

  return (
    <div dir="rtl" className="ai-page">
      <div className="ai-header">
        <h1>SmartAid</h1>
        <p>עזרה חכמה. קהילה חזקה.</p>
      </div>

      <div className="ai-container">
        <div className="ai-heading">
          <span className="ai-badge">הצעות לפתרון עצמי</span>
          <h2>אולי אפשר לפתור את הבעיה בעצמך?</h2>
          <p>
            ריכזנו עבורך את פרטי הבקשה. אפשר להיעזר בהצעות
            כלליות או להמשיך לקבלת עזרה ממתנדב.
          </p>
        </div>

        <div className="problem-card">
          <h3>הבעיה שתיארת</h3>
          <h2>{request.title}</h2>
          <p>{request.description}</p>

          {request.address && (
            <span className="problem-category">
              📍 {request.address}
            </span>
          )}
        </div>

        <div className="solutions-section">
          <h2>הצעות לפתרון עצמי</h2>

          <div className="solution-card">
            <div className="solution-icon">💡</div>
            <div className="solution-content">
              <h3>בדיקה ראשונית</h3>
              <p>
                נסי לבדוק אם יש לך מידע נוסף על הבעיה,
                ואם קיימת פעולה פשוטה ובטוחה שכבר ניסית.
                חשוב לא לבצע פעולות שאינך בטוחה כיצד לבצע.
              </p>
              <div className="solution-meta">
                <span>הצעה כללית</span>
              </div>
            </div>
          </div>

          <p>
            ההצעה המוצגת היא כללית ואינה אבחון מותאם אישית
            לבעיה שתיארת.
          </p>
        </div>

        <div className="volunteer-help">
          <div>
            <h3>עדיין זקוקה לעזרה?</h3>
            <p>אפשר להיעזר במתנדב מהקהילה.</p>
          </div>

          <button
            type="button"
            className="volunteer-button"
            onClick={() =>
              navigate(`/request-tracking/${request.id}`)
            }
          >
            אני צריכה מתנדב
          </button>
        </div>
      </div>
    </div>
  );
}

export default AISuggestionsScreen;
