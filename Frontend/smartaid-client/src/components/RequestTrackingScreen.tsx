
import { useEffect } from "react";
import './RequestTrackingScreen.css';

function RequestTrackingScreen() {
  useEffect(() => {
    document.title = "מעקב אחר בקשת עזרה | SmartAid";
  }, []);

  // שלב נוכחי בתהליך – כרגע נתון הדגמה
  const currentStep = 1;

  const statusSteps = [
    {
      title: "הבקשה נשלחה בהצלחה",
      description: "בקשת העזרה שלך התקבלה במערכת.",
      icon: "📩"
    },
    {
      title: "מחפשים עבורך מתנדב",
      description: "המערכת מאתרת מתנדבים מתאימים באזור שלך.",
      icon: "🔍"
    },
    {
      title: "נמצא מתנדב – ממתינים לאישור",
      description: "נמצא מתנדב מתאים. ממתינים לאישור שלו.",
      icon: "🙋"
    },
    {
      title: "המתנדב בדרך אליך",
      description: "המתנדב אישר את הבקשה והוא בדרכו אליך.",
      icon: "🚗"
    },
    {
      title: "המתנדב הגיע",
      description: "המתנדב הגיע למיקום שלך.",
      icon: "📍"
    },
    {
      title: "הטיפול הסתיים",
      description: "בקשת העזרה הושלמה בהצלחה.",
      icon: "✓"
    }
  ];

  const currentStatus = statusSteps[currentStep];

  return (
    <div dir="rtl" className="tracking-page">

      <header className="tracking-header">
        <h1>SmartAid</h1>
        <p>עזרה חכמה. קהילה חזקה.</p>
      </header>

      <main className="tracking-container">

        <div className="tracking-heading">
          <span className="tracking-badge">מעקב אחר בקשת עזרה</span>
          <h2>אנחנו מטפלים בבקשה שלך!</h2>
          <p>
            ניתן לעקוב כאן אחר התקדמות הבקשה
            עד להגעת המתנדב וסיום הטיפול.
          </p>
        </div>

        <div className="tracking-request-card">
          <div className="tracking-request-icon">🚗</div>
          <div>
            <h3>הרכב אינו מניע</h3>
            <p>בקשה מספר 1</p>
            <span>קטגוריה: סיוע בדרכים</span>
          </div>
        </div>

        <section className="current-status-card">
          <span className="current-status-icon">
            {currentStatus.icon}
          </span>

          <div>
            <span className="status-label">הסטטוס הנוכחי</span>
            <h2>{currentStatus.title}</h2>
            <p>{currentStatus.description}</p>
          </div>
        </section>

        <section className="tracking-timeline">
          <h2>התקדמות הבקשה</h2>

          {statusSteps.map((step, index) => (
            <div
              className={`timeline-item ${
                index < currentStep
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

        <div className="tracking-info">
          <span>ℹ️</span>
          <p>
            אין צורך לבצע פעולה נוספת.
            המערכת תעדכן את סטטוס הבקשה
            בהתאם להתקדמות התהליך.
          </p>
        </div>

      </main>
    </div>
  );
}

export default RequestTrackingScreen;
