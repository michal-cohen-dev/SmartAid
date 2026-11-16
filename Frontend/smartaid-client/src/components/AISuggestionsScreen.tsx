
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

interface DraftRequest {
  title: string;
  description: string;
  helpTypeId: string;
  latitude: number;
  longitude: number;
  address: string;
  addressNotes: string;
  selectedImage: File | null;
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

  const navigationState = location.state as {
    analysis?: ImageAnalysisResult | null;
    draft?: DraftRequest;
  } | null;

  const analysis = navigationState?.analysis;
  const draft = navigationState?.draft;

  const [request, setRequest] = useState<HelpRequest | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    document.title = "הצעות לפתרון עצמי | SmartAid";

    const loadRequest = async () => {
      // בקשה חדשה שעדיין לא נשלחה לשרת
      if (draft) {
        setRequest({
          id: 0,
          title: draft.title,
          description: draft.description,
          createdAt: new Date().toISOString(),
          status: 0,
          address: draft.address
        });

        setLoading(false);
        return;
      }

      // תמיכה גם בכתובות ישנות שבהן כבר קיים מזהה בקשה
      if (!id || !Number.isInteger(Number(id)) || Number(id) <= 0) {
        setError("לא נמצאו פרטי בקשה תקינים.");
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
  }, [id, draft]);

  // יצירת הקריאה מתבצעת רק לאחר לחיצה מפורשת על הזמנת מתנדב
  const handleVolunteerRequest = async () => {
    if (!draft) {
      if (request?.id) {
        navigate(`/request-tracking/${request.id}`);
      }
      return;
    }

    if (isSubmitting) return;

    setIsSubmitting(true);
    setError("");

    try {
      const formData = new FormData();

      if (draft.selectedImage) {
        formData.append("ImageFile", draft.selectedImage);
      }

      formData.append("Title", draft.title);
      formData.append("Description", draft.description);
      formData.append("Latitude", String(draft.latitude));
      formData.append("Longitude", String(draft.longitude));
      formData.append("Address", draft.address);
      formData.append("HelpTypeId", draft.helpTypeId);

      if (draft.addressNotes) {
        formData.append("AddressNotes", draft.addressNotes);
      }

      const response = await apiRequest<{
        message: string;
        id: number;
      }>("/api/HelpRequests", {
        method: "POST",
        body: formData
      });

      navigate(`/request-tracking/${response.id}`, {
        replace: true
      });
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בשליחת הבקשה למתנדבים."
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  if (loading) {
    return (
      <main className="ai-page" dir="rtl">
        <div className="ai-container">
          <p>טוען את פרטי הבקשה...</p>
        </div>
      </main>
    );
  }

  if (error && !request) {
    return (
      <main className="ai-page" dir="rtl">
        <div className="ai-container">
          <h2>לא ניתן להציג את ההצעות</h2>
          <p>{error}</p>
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

  if (!request) return null;

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

  const showVolunteerButton =
    !professionalRequired &&
    (!analysis || analysis.volunteerRequired);

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
            המערכת ניתחה את המידע שסיפקת כדי להציע
            את דרך הפעולה המתאימה.
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
                    לא מומלץ לנסות לטפל בבעיה באופן עצמאי
                    או להסתמך על מתנדב. יש להתרחק מהאזור
                    או מהגורם המסוכן ולהיעזר באיש מקצוע מוסמך.
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
                  onClick={handleVolunteerRequest}
                  disabled={isSubmitting}
                >
                  {isSubmitting
                    ? "שולח בקשה..."
                    : "אני צריכה מתנדב"}
                </button>
              </div>
            ) : (
              <div className="solution-card">
                <div className="solution-content">
                  <h3>אפשר להתחיל בבדיקה עצמית בטוחה</h3>
                  <p>
                    לפי הניתוח, לא זוהה צורך מיידי במתנדב.
                    אם אינך בטוחה כיצד לפעול, אל תבצעי
                    פעולה שאינך מכירה ופני לאיש מקצוע מתאים.
                  </p>
                </div>
              </div>
            )}

            <p>
              חשוב: הניתוח מבוסס על תמונה בלבד ואינו
              מהווה אבחון מקצועי. ייתכן שהמערכת לא זיהתה
              את כל פרטי הבעיה.
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
                onClick={handleVolunteerRequest}
                disabled={isSubmitting}
              >
                {isSubmitting
                  ? "שולח בקשה..."
                  : "אני צריכה מתנדב"}
              </button>
            </div>
          </div>
        )}

        {error && (
          <p className="create-request-error" role="alert">
            {error}
          </p>
        )}
      </div>
    </div>
  );
}

export default AISuggestionsScreen;
