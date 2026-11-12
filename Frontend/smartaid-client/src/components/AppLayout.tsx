
import { useEffect, useRef, useState } from "react";
import { Outlet, useLocation, useNavigate } from "react-router-dom";

function AppLayout() {
  const navigate = useNavigate();
  const location = useLocation();

  const [isProfileMenuOpen, setIsProfileMenuOpen] = useState(false);
  const profileMenuRef = useRef<HTMLDivElement>(null);

  function checkIfVolunteer(): boolean {
    const token = sessionStorage.getItem("token");

    if (!token) return false;

    try {
      const payload = token.split(".")[1];
      if (!payload) return false;

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

  const isVolunteerPage =
    location.pathname === "/" ||
    location.pathname.startsWith("/volunteer") ||
    location.pathname.startsWith("/request-details/");

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        profileMenuRef.current &&
        !profileMenuRef.current.contains(event.target as Node)
      ) {
        setIsProfileMenuOpen(false);
      }
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setIsProfileMenuOpen(false);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);
    document.addEventListener("keydown", handleEscape);

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      document.removeEventListener("keydown", handleEscape);
    };
  }, []);

  function handleSwitchMode() {
    if (!hasVolunteerRole) return;

    setIsProfileMenuOpen(false);
    navigate(isVolunteerPage ? "/requester" : "/volunteer");
  }

  function handleMenuNavigation(path: string) {
    setIsProfileMenuOpen(false);
    navigate(path);
  }

  return (
    <div className="app-layout" dir="rtl">
      <header className="app-header">
        <div
          className="app-logo"
          onClick={() => navigate("/")}
          role="button"
        >
          <div className="logo-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <path
                d="M12 21s-8-4.5-8-11a4.5 4.5 0 0 1 8-2.8A4.5 4.5 0 0 1 20 10c0 6.5-8 11-8 11Z"
                stroke="currentColor"
                strokeWidth="1.8"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
              <path
                d="M12 9v6M9 12h6"
                stroke="currentColor"
                strokeWidth="1.8"
                strokeLinecap="round"
              />
            </svg>
          </div>

          <div>
            <h1>SmartAid</h1>
            <p>עזרה חכמה. קהילה חזקה.</p>
          </div>
        </div>

        <div className="app-navigation">
          <div className="mode-indicator">
            <span className="mode-dot" />
            {isVolunteerPage ? "מצב מתנדב" : "מצב פונה"}
          </div>

          {hasVolunteerRole && (
            <button
              type="button"
              className="switch-mode-button"
              onClick={handleSwitchMode}
            >
              <svg viewBox="0 0 24 24" fill="none">
                <path
                  d="M20 7h-4l2-2M4 17h4l-2 2M18 5a7 7 0 0 0-12 3M6 19a7 7 0 0 0 12-3"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
              {isVolunteerPage ? "מעבר למצב פונה" : "מעבר למצב מתנדב"}
            </button>
          )}

          <div className="profile-menu-container" ref={profileMenuRef}>
            <button
              type="button"
              className={`profile-button ${
                isProfileMenuOpen ? "profile-button-active" : ""
              }`}
              aria-label="פתיחת תפריט אישי"
              aria-expanded={isProfileMenuOpen}
              onClick={() =>
                setIsProfileMenuOpen((previous) => !previous)
              }
            >
              <svg viewBox="0 0 24 24" fill="none">
                <circle
                  cx="12"
                  cy="8"
                  r="3.5"
                  stroke="currentColor"
                  strokeWidth="1.8"
                />
                <path
                  d="M5 20a7 7 0 0 1 14 0"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                />
              </svg>
              <span className="profile-button-arrow">
                {isProfileMenuOpen ? "▲" : "▼"}
              </span>
            </button>

            {isProfileMenuOpen && (
              <div className="profile-dropdown">
                <div className="profile-dropdown-header">
                  <div className="profile-avatar">
                    <svg viewBox="0 0 24 24" fill="none">
                      <circle
                        cx="12"
                        cy="8"
                        r="3.5"
                        stroke="currentColor"
                        strokeWidth="1.8"
                      />
                      <path
                        d="M5 20a7 7 0 0 1 14 0"
                        stroke="currentColor"
                        strokeWidth="1.8"
                      />
                    </svg>
                  </div>

                  <div>
                    <strong>האזור האישי שלי</strong>
                    <span>
                      {hasVolunteerRole ? "חשבון מתנדב" : "חשבון משתמש"}
                    </span>
                  </div>
                </div>

                <div className="profile-menu-section">
                  <span className="profile-section-title">
                    החשבון שלי
                  </span>

                  <button
                    type="button"
                    className="profile-menu-item"
                    onClick={() => handleMenuNavigation("/request")}
                  >
                    <span className="menu-item-icon blue">
                      <svg viewBox="0 0 24 24" fill="none">
                        <path
                          d="M12 5v14M5 12h14"
                          stroke="currentColor"
                          strokeWidth="2"
                          strokeLinecap="round"
                        />
                      </svg>
                    </span>
                    <span>פתיחת קריאה חדשה</span>
                    <span className="menu-item-arrow">‹</span>
                  </button>

                  <button
                    type="button"
                    className="profile-menu-item"
                    onClick={() => handleMenuNavigation("/my-requests")}
                  >
                    <span className="menu-item-icon turquoise">
                      <svg viewBox="0 0 24 24" fill="none">
                        <path
                          d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01"
                          stroke="currentColor"
                          strokeWidth="2"
                          strokeLinecap="round"
                        />
                      </svg>
                    </span>
                    <span>הקריאות הקודמות שלי</span>
                    <span className="menu-item-arrow">‹</span>
                  </button>

                  <button
                    type="button"
                    className="profile-menu-item"
                    onClick={() => handleMenuNavigation("/profile")}
                  >
                    <span className="menu-item-icon purple">
                      <svg viewBox="0 0 24 24" fill="none">
                        <path
                          d="M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z"
                          stroke="currentColor"
                          strokeWidth="1.8"
                        />
                        <path
                          d="m19.4 15 .1.1a1.7 1.7 0 0 1-2.4 2.4l-.1-.1a1.7 1.7 0 0 0-2.9 1.2v.2a1.7 1.7 0 0 1-3.4 0v-.2a1.7 1.7 0 0 0-2.9-1.2l-.1.1a1.7 1.7 0 0 1-2.4-2.4l.1-.1a1.7 1.7 0 0 0-1.2-2.9H4a1.7 1.7 0 0 1 0-3.4h.2a1.7 1.7 0 0 0 1.2-2.9l-.1-.1a1.7 1.7 0 0 1 2.4-2.4l.1.1a1.7 1.7 0 0 0 2.9-1.2V4a1.7 1.7 0 0 1 3.4 0v.2a1.7 1.7 0 0 0 2.9 1.2l.1-.1a1.7 1.7 0 0 1 2.4 2.4l-.1.1a1.7 1.7 0 0 0 1.2 2.9h.2a1.7 1.7 0 0 1 0 3.4h-.2a1.7 1.7 0 0 0-1.2 2.9Z"
                          stroke="currentColor"
                          strokeWidth="1.2"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                        />
                      </svg>
                    </span>
                    <span>עריכת הפרטים האישיים</span>
                    <span className="menu-item-arrow">‹</span>
                  </button>

                  {!hasVolunteerRole && (
                    <button
                      type="button"
                      className="profile-menu-item"
                      onClick={() =>
                        handleMenuNavigation("/volunteer-registration")
                      }
                    >
                      <span className="menu-item-icon green">
                        <svg viewBox="0 0 24 24" fill="none">
                          <path
                            d="M12 5v14M5 12h14"
                            stroke="currentColor"
                            strokeWidth="2"
                            strokeLinecap="round"
                          />
                          <circle
                            cx="12"
                            cy="12"
                            r="9"
                            stroke="currentColor"
                            strokeWidth="1.6"
                          />
                        </svg>
                      </span>
                      <span>הרשמה כמתנדב</span>
                      <span className="menu-item-arrow">‹</span>
                    </button>
                  )}
                </div>

                {hasVolunteerRole && (
                  <>
                    <div className="profile-menu-separator" />

                    <div className="profile-menu-section">
                      <span className="profile-section-title">
                        אזור המתנדב
                      </span>

                      <button
                        type="button"
                        className="profile-menu-item"
                        onClick={() =>
                          handleMenuNavigation("/volunteer/open-requests")
                        }
                      >
                        <span className="menu-item-icon orange">
                          <svg viewBox="0 0 24 24" fill="none">
                            <path
                              d="M12 3v3M12 18v3M4.2 4.2l2.1 2.1m11.4 11.4 2.1 2.1M3 12h3m12 0h3M4.2 19.8l2.1-2.1M17.7 6.3l2.1-2.1"
                              stroke="currentColor"
                              strokeWidth="1.8"
                              strokeLinecap="round"
                            />
                            <circle
                              cx="12"
                              cy="12"
                              r="4"
                              stroke="currentColor"
                              strokeWidth="1.8"
                            />
                          </svg>
                        </span>
                        <span>קריאות חדשות שמחכות למענה</span>
                        <span className="menu-item-arrow">‹</span>
                      </button>

                      <button
                        type="button"
                        className="profile-menu-item"
                        onClick={() =>
                          handleMenuNavigation("/volunteer/accepted-requests")
                        }
                      >
                        <span className="menu-item-icon blue">
                          <svg viewBox="0 0 24 24" fill="none">
                            <path
                              d="m5 12 4 4L19 6"
                              stroke="currentColor"
                              strokeWidth="2"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                            />
                          </svg>
                        </span>
                        <span>קריאות שנעניתי להן</span>
                        <span className="menu-item-arrow">‹</span>
                      </button>

                      <button
                        type="button"
                        className="profile-menu-item"
                        onClick={() =>
                          handleMenuNavigation("/volunteer/completed-requests")
                        }
                      >
                        <span className="menu-item-icon green">
                          <svg viewBox="0 0 24 24" fill="none">
                            <path
                              d="M5 12.5 9.5 17 19 7"
                              stroke="currentColor"
                              strokeWidth="2"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                            />
                            <circle
                              cx="12"
                              cy="12"
                              r="10"
                              stroke="currentColor"
                              strokeWidth="1.5"
                            />
                          </svg>
                        </span>
                        <span>קריאות שטופלו כבר</span>
                        <span className="menu-item-arrow">‹</span>
                      </button>
                    </div>
                  </>
                )}

                <div className="profile-dropdown-footer">
                  SmartAid · עזרה חכמה. קהילה חזקה.
                </div>
              </div>
            )}
          </div>
        </div>
      </header>

      <main className="app-content">
        <Outlet />
      </main>
    </div>
  );
}

export default AppLayout;
