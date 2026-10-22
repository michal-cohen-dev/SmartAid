import { useState } from "react";

function VolunteerHome() {
  const [isAvailable, setIsAvailable] = useState(true);

  function handleToggle() {
    setIsAvailable(!isAvailable);
  }

  return (
    <div dir="rtl">
      <h1>SmartAid</h1>
      <h2>שלום, מתנדב!</h2>

      <div>
        <h3>הזמינות שלי</h3>
        <p>{isAvailable ? "זמין לקבלת קריאות" : "לא זמין"}</p>
        <button type="button" onClick={handleToggle}>
          שינוי סטטוס זמינות
        </button>
      </div>

      <div>
        <h2>קריאות פעילות</h2>

       
        <div>
          <h3>עזרה בתיקון מחשב</h3>
          <p>המחשב אינו נדלק</p>
          <p>מרחק: 1.2 ק״מ</p>
          <p>סטטוס: ממתין לטיפול</p>
        </div>

        
        <div>
          <h3>סיוע בהעברת ציוד</h3>
          <p>נדרשת עזרה בהעברת ארגזים</p>
          <p>מרחק: 2.5 ק״מ</p>
          <p>סטטוס: בקשה חדשה</p>
        </div>
      </div>

      <button type="button">
        פתיחת בקשת עזרה
      </button>
    </div>
  );
}

export default VolunteerHome;