import { useState } from "react";

function RequestDetailsScreen() {
  const [showAllDetails, setShowAllDetails] = useState(true);

  return (
    <div dir="rtl" className="request-page">
      <header className="request-header">
        <h1>SmartAid</h1>
        <p>פרטי בקשת עזרה</p>
      </header>

      <main className="request-card">
        <div className="request-top">
          <span className="request-icon">🚗</span>
          <div>
            <span className="request-status">בקשה חדשה</span>
            <h2>הרכב אינו מניע</h2>
            <p>בקשה מספר 1</p>
          </div>
        </div>

        <div className="request-info">
          <div>
            <span>שם הפונה</span>
            <strong>דנה לוי</strong>
          </div>

          <div>
            <span>קטגוריה</span>
            <strong>סיוע בדרכים</strong>
          </div>

          <div>
            <span>מרחק ממך</span>
            <strong>3.6 ק״מ</strong>
          </div>

          <div>
            <span>זמן הגעה משוער</span>
            <strong>12 דקות</strong>
          </div>
        </div>

        <div className="request-section">
          <h3>תיאור הבעיה</h3>
          <p>הרכב לא מניע לאחר שהחנה אותו. נראה שיש בעיה במצבר ונדרשת עזרה בהתנעה.</p>
        </div>

        <div className="request-section">
          <h3>מיקום הבקשה</h3>
          <p>📍 רחוב הרצל 12, חיפה</p>
          <div className="map-placeholder">
            <span>📍</span>
            <p>תצוגת מפה לדוגמה</p>
          </div>
        </div>

        <div className="request-section">
          <h3>ציוד נדרש</h3>
          <div className="equipment-row">
            <span>✓</span> כבלים להתנעת רכב
          </div>
          <div className="equipment-row">
            <span>✓</span> ערכת כלי עבודה
          </div>
        </div>

        <div className="request-section">
          <button
            type="button"
            className="details-toggle"
            onClick={() => setShowAllDetails(!showAllDetails)}
          >
            {showAllDetails ? "הסתרת פרטים נוספים" : "הצגת פרטים נוספים"}
          </button>

          {showAllDetails && (
            <p className="request-time">
              זמן יצירת הבקשה: לפני 5 דקות
            </p>
          )}
        </div>

        <div className="request-actions">
          <button type="button" className="accept-button">
            קבלת בקשה
          </button>

          <button type="button" className="reject-button">
            דחייה
          </button>
        </div>
      </main>
    </div>
  );
}

export default RequestDetailsScreen;