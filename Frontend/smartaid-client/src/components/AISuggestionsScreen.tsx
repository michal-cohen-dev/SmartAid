
import { useEffect, useState } from "react";
import { useNavigate, useParams, useLocation } from "react-router-dom";
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

interface ImageAnalysisResult {
  problemName: string;
  description: string;
  category: string;
  riskLevel: string;
  recommendation: string;
  safeActions: string[];
  volunteerRequired: boolean;
  confidence: number;
}

function AISuggestionsScreen() {
  const navigate = useNavigate();
  const location = useLocation();
  const { id } = useParams<{ id: string }>();

  const analysis = (
    location.state as { analysis?: ImageAnalysisResult | null } | null
  )?.analysis;

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

  const normalizedRisk = analysis?.riskLevel?.trim().toLowerCase();

  const isHighRisk =
    normalizedRisk === "high" ||
    normalizedRisk === "גבוה" ||
    normalizedRisk === "גבוהה";

  const isUnknownRisk =
    normalizedRisk === "unknown" ||
    normalizedRisk === "לא ידוע" ||
    normalizedRisk === "לא ידועה";

  const professionalRequired = isHighRisk || isUnknownRisk;

  return (
    <div dir="rtl" className="ai-page">
      <div className="ai-header">
        <h1>SmartAid</h1>
        <p>עזרה חכמה. קהילה חזקה.</p>
      </div>

      <div className="ai-container">
        <div className="ai-heading">
          <span className="ai-badge">ניתוח חכם</span>
          <h2>תוצאות בדיקת הבקשה</h2>
          <p>
            המערכת ניתחה את המידע שסיפקת כדי להציע את דרך הפעולה המתאימה.
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

        {analysis ? (
          <div className="solutions-section">
            <h2>תוצאות ניתוח התמונה</h2>

            <div className="solution-card">
              <div className="solution-content">
                <h3>{analysis.problemName}</h3>
                <p>{analysis.description}</p>

                <p>
                  <strong>קטגוריה:</strong> {analysis.category}
                </p>

                <p>
                  <strong>רמת סיכון:</strong> {analysis.riskLevel}
                </p>

                <p>
                  <strong>רמת ביטחון הניתוח:</strong>{" "}
                  {Math.round(analysis.confidence * 100)}%
                </p>
              </div>
            </div>

            <div className="solution-card">
              <div className="solution-content">
                <h3>המלצת המערכת</h3>
                <p>{analysis.recommendation}</p>
              </div>
            </div>

            {analysis.safeActions?.length > 0 && (
              <div className="solution-card">
                <div className="solution-content">
                  <h3>פעולות בטוחות אפשריות</h3>
                  <ul>
                    {analysis.safeActions.map((action, index) => (
                      <li key={index}>{action}</li>
                    ))}
                  </ul>
                </div>
              </div>
            )}

            {professionalRequired ? (
              <div className="solution-card">
                <div className="solution-content">
                  <h3>נדרשת בדיקה מקצועית</h3>
                  <p>
                    לא מומלץ לנסות לטפל בבעיה באופן עצמאי או להסתמך
                    על מתנדב. יש להתרחק מהאזור או מהגורם המסוכן,
                    ולהיעזר באיש מקצוע מוסמך.
                  </p>
                </div>
              </div>
            ) : analysis.volunteerRequired ? (
              <div className="volunteer-help">
                <div>
                  <h3>מומלץ להיעזר במתנדב</h3>
                  <p>
                    לפי הניתוח, כדאי לקבל עזרה ממתנדב מהקהילה.
                  </p>
                </div>

                <button
                  type="button"
                  className="volunteer-button"
                  onClick={() =>
                    navigate(`/request-tracking/${request.id}`)
                  }
                >
                  המשך לקבלת עזרת מתנדב
                </button>
              </div>
            ) : (
              <div className="solution-card">
                <div className="solution-content">
                  <h3>אפשר להתחיל בבדיקה עצמית בטוחה</h3>
                  <p>
                    לפי הניתוח, לא זוהה צורך מיידי במתנדב.
                    אם אינך בטוח כיצד לפעול, אל תבצע פעולה
                    שאינך מכיר ופנה לאיש מקצוע מתאים.
                  </p>
                </div>
              </div>
            )}

            <p>
              חשוב: הניתוח מבוסס על תמונה בלבד ואינו מהווה אבחון
              מקצועי. ייתכן שהמערכת לא זיהתה את כל פרטי הבעיה.
            </p>
          </div>
        ) : (
          <div className="solutions-section">
            <h2>הצעות לפתרון עצמי</h2>

            <div className="solution-card">
              <div className="solution-content">
                <h3>בדיקה ראשונית</h3>
                <p>
                  לא הועלתה תמונה לניתוח. חשוב לבדוק אם קיימת
                  פעולה פשוטה ובטוחה שניתן לבצע, בלי להסתכן.
                </p>
              </div>
            </div>

            <p>
              ההצעה המוצגת היא כללית ואינה אבחון מותאם אישית.
            </p>

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
        )}
      </div>
    </div>
  );
}

export default AISuggestionsScreen;
