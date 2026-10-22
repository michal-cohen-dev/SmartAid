import { useState } from "react";

function MainDashboard() {
  const [isAvailable, setIsAvailable] = useState(true);

  return (
    <div
      dir="rtl"
      style={{
        backgroundColor: "#0F172A", 
        color: "#F8FAFC",            
        minHeight: "100vh",
        padding: "24px",
        fontFamily: "Arial, sans-serif"
      }}
    >
      {}
      <header style={{ marginBottom: "24px", textAlign: "center" }}>
        <h1 style={{ color: "#38BDF8", fontSize: "28px", margin: "0 0 8px 0" }}>
          SmartAid
        </h1>
        <p style={{ color: "#94A3B8", fontSize: "16px", margin: 0 }}>
          עזרה חכמה. קהילה חזקה.
        </p>
      </header>

      {}
      <div
        style={{
          backgroundColor: "#1E293B", 
          padding: "16px",
          borderRadius: "12px",
          marginBottom: "20px",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          boxShadow: "0 4px 6px rgba(0, 0, 0, 0.2)"
        }}
      >
        <div>
          <h3 style={{ margin: "0 0 4px 0", fontSize: "18px" }}>הזמינות שלי</h3>
          <p style={{ margin: 0, fontSize: "14px", color: "#94A3B8" }}>
            {isAvailable ? "אתה זמין לקבלת קריאות בסביבתך" : "אתה במצב לא זמין"}
          </p>
        </div>
        
        <button
          onClick={() => setIsAvailable(!isAvailable)}
          style={{
            backgroundColor: isAvailable ? "#10B981" : "#64748B", 
            color: "#FFFFFF",
            border: "none",
            padding: "10px 16px",
            borderRadius: "8px",
            fontWeight: "bold",
            cursor: "pointer"
          }}
        >
          {isAvailable ? "פעיל" : "לא פעיל"}
        </button>
      </ div>

      {}
      <section style={{ marginBottom: "24px" }}>
        <h2 style={{ fontSize: "20px", marginBottom: "12px", color: "#E2E8F0" }}>
          קריאות פעילות בקרבת מקום
        </h2>

        {}
        <div
          style={{
            backgroundColor: "#1E293B",
            padding: "16px",
            borderRadius: "12px",
            borderRight: "4px solid #38BDF8", 
            boxShadow: "0 4px 6px rgba(0, 0, 0, 0.2)"
          }}
        >
          <h3 style={{ margin: "0 0 8px 0", fontSize: "18px" }}>עזרה בתיקון גלגל</h3>
          <p style={{ margin: "0 0 12px 0", color: "#94A3B8", fontSize: "14px" }}>
           הרכב נעצר בשולי הדרך עקב תקר בגלגל הקדמי, ונדרשת עזרה בהחלפת הגלגל הפגוע בגלגל חלופי (רזרבי).
          </p>
          <div style={{ display: "flex", justifyContent: "space-between", fontSize: "13px", color: "#38BDF8" }}>
            <span>מרחק: 1.2 ק״מ</span>
            <span>סטטוס: ממתין לטיפול</span>
          </div>
        </div>
      </section>

      {}
      <button
        type="button"
        style={{
          width: "100%",
          backgroundColor: "#0284C7", 
          color: "#FFFFFF",
          border: "none",
          padding: "14px",
          borderRadius: "10px",
          fontSize: "16px",
          fontWeight: "bold",
          cursor: "pointer",
          boxShadow: "0 4px 6px rgba(0, 2, 8, 0.3)"
        }}
      >
        פתיחת בקשת עזרה חדשה
      </button>
    </div>
  );
}

export default MainDashboard;