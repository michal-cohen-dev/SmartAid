
import { Link } from 'react-router-dom';
import './MainDashboard.css';
function MainDashboard() {
  return (
    <div className="dashboard-page" dir="rtl">
      <header className="dashboard-header">
        <div className="dashboard-brand">
          <h1>SmartAid</h1>
          <span>עזרה חכמה. קהילה חזקה.</span>
        </div>
      </header>

      <main className="dashboard-main">
        <section className="dashboard-welcome">
          <h2>עוזרים אחד לשני, בקלות ובחכמה</h2>

          <p>
            SmartAid מחברת בין אנשים שזקוקים לעזרה לבין מתנדבים
            שרוצים להושיט יד וליצור קהילה חזקה יותר.
          </p>

          <div className="dashboard-actions">
            <Link to="/login" className="dashboard-button dashboard-button-primary">
              התחברות
            </Link>

            <Link to="/register" className="dashboard-button dashboard-button-secondary">
              הרשמה
            </Link>
          </div>
        </section>
      </main>
    </div>
  );
}

export default MainDashboard;
