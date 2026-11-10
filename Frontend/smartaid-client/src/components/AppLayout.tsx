
import { Outlet, useLocation, useNavigate } from "react-router-dom";

function AppLayout() {
  const navigate = useNavigate();
  const location = useLocation();

  // בדיקה האם המשתמש הוא מתנדב לפי ה-Token שהתקבל מהשרת
  function checkIfVolunteer(): boolean {
    const token = sessionStorage.getItem("token");

    if (!token) {
      return false;
    }

    try {
      const payload = token.split(".")[1];

      if (!payload) {
        return false;
      }

      const base64 = payload
        .replace(/-/g, "+")
        .replace(/_/g, "/")
        .padEnd(Math.ceil(payload.length / 4) * 4, "=");

      const binary = atob(base64);
      const bytes = Uint8Array.from(
        binary,
        (character) => character.charCodeAt(0)
      );

      const decodedPayload = JSON.parse(
        new TextDecoder().decode(bytes)
      );

      return (
        decodedPayload.isVolunteer === true ||
        String(decodedPayload.isVolunteer).toLowerCase() === "true"
      );
    } catch {
      return false;
    }
  }

  const hasVolunteerRole = checkIfVolunteer();

  // בדיקה באיזה מסך המשתמש נמצא כרגע
  const isVolunteerPage =
    location.pathname === "/" ||
    location.pathname.startsWith("/volunteer") ||
    location.pathname.startsWith("/request-details/");

  function handleSwitchMode() {
    if (!hasVolunteerRole) {
      return;
    }

    if (isVolunteerPage) {
      navigate("/requester");
    } else {
      navigate("/volunteer");
    }
  }

  return (
    <div className="app-layout" dir="rtl">
      <header className="app-header">
        <div className="app-logo">
          <h1>SmartAid</h1>
          <p>עזרה חכמה. קהילה חזקה.</p>
        </div>

        <div className="app-navigation">
          <span className="current-mode">
            {isVolunteerPage ? "מצב מתנדב" : "מצב פונה"}
          </span>

          {/* רק מתנדב יוכל לראות את כפתור החלפת המצב */}
          {hasVolunteerRole && (
            <button
              type="button"
              className="switch-mode-button"
              onClick={handleSwitchMode}
            >
              {isVolunteerPage
                ? "מעבר למצב פונה"
                : "מעבר למצב מתנדב"}
            </button>
          )}
        </div>
      </header>

      <main className="app-content">
        <Outlet />
      </main>
    </div>
  );
}

export default AppLayout;
