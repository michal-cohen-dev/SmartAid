import { useState } from "react";

function AISuggestionsScreen() {
  const [isSolutionClicked, setIsSolutionClicked] = useState(false);
  const [showHelpMessage, setShowHelpMessage] = useState(false);

  function handleSolutionClick() {
    setIsSolutionClicked(true);
    setShowHelpMessage(true);
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
            ריכזנו עבורך כמה הצעות ראשוניות. אם הן לא עוזרות,
            אפשר להיעזר במתנדב.
          </p>
        </div>

        <div className="problem-card">
          <h3>הבעיה שתיארת</h3>
          <h2>הרכב אינו מניע</h2>
          <p>הרכב לא מניע לאחר החניה. ייתכן שהבעיה קשורה למצבר.</p>
          <span className="problem-category">סיוע בדרכים</span>
        </div>

        <div className="solutions-section">
          <h2>הצעות שיכולות לעזור</h2>

          <div className={`solution-card ${isSolutionClicked ? "selected" : ""}`}>
            <div className="solution-icon">🔋</div>
            <div className="solution-content">
              <h3>בדיקת המצבר</h3>
              <p>בדקי אם אורות לוח המחוונים נדלקים ואם יש סימנים לכך שהמצבר חלש.</p>
              <div className="solution-meta">
                <span>⏱ 2 דקות</span>
                <span>רמת קושי: קל</span>
              </div>
              <button
                type="button"
                className={isSolutionClicked ? "solution-selected" : "solution-button"}
                onClick={handleSolutionClick}
              >
                {isSolutionClicked ? "נבחר כפתרון שעזר ✓" : "הפתרון עזר לי"}
              </button>
            </div>
          </div>
        </div>

        {showHelpMessage && (
          <div className="ai-feedback">
            <strong>תודה על העדכון!</strong>
            <p>נשמח אם תשתפי אותנו אם הפתרון עזר או אם את עדיין זקוקה לעזרה.</p>
          </div>
        )}

        <div className="volunteer-help">
          <div>
            <h3>עדיין זקוקה לעזרה?</h3>
            <p>אפשר להיעזר במתנדב מהקהילה.</p>
          </div>

          <button type="button" className="volunteer-button">
            אני צריכה מתנדב
          </button>
        </div>
      </div>
    </div>
  );
}

export default AISuggestionsScreen;