
import { Outlet, useLocation, useNavigate } from "react-router-dom";

function AppLayout() {
  const navigate = useNavigate();
  const location = useLocation();

  const isVolunteer = location.pathname.startsWith("/volunteer");

  function handleSwitchMode() {
    if (isVolunteer) {
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
            {isVolunteer ? "מצב מתנדב" : "מצב פונה"}
          </span>

          <button
            type="button"
            className="switch-mode-button"
            onClick={handleSwitchMode}
          >
            {isVolunteer
              ? "מעבר למצב פונה"
              : "מעבר למצב מתנדב"}
          </button>
        </div>
      </header>

      <main className="app-content">
        <Outlet />
      </main>
    </div>
  );
}

export default AppLayout;
