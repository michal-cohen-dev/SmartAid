
import { Outlet, NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import "./AdminLayout.css";

const menuItems = [
  { title: "לוח בקרה", path: "/admin", icon: "📊" },
  { title: "ניהול משתמשים", path: "/admin/users", icon: "👥" },
  { title: "ניהול מתנדבים", path: "/admin/volunteers", icon: "🤝" },
  { title: "ניהול קריאות", path: "/admin/requests", icon: "📋" },
  { title: "ניהול ציוד", path: "/admin/equipment", icon: "🧰" },
  { title: "ניהול סוגי עזרה", path: "/admin/help-types", icon: "🆘" },
];

export default function AdminLayout() {
  const { logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/login", { replace: true });
  }

  return (
    <div className="admin-layout" dir="rtl">
      <aside className="admin-sidebar">
        <h2>SmartAid</h2>
        <p>מערכת ניהול</p>

        <nav>
          {menuItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.path === "/admin"}
              className={({ isActive }) =>
                isActive ? "admin-nav-link active" : "admin-nav-link"
              }
            >
              <span>{item.icon}</span>
              {item.title}
            </NavLink>
          ))}
        </nav>
      </aside>

      <main className="admin-main">
        <header className="admin-header">
          <h1>שלום, מנהל 👋</h1>
          <span>SmartAid | מערכת ניהול</span>

          <button
            type="button"
            className="admin-logout-button"
            onClick={handleLogout}
          >
            התנתקות
          </button>
        </header>

        <section className="admin-content">
          <Outlet />
        </section>
      </main>
    </div>
  );
}
