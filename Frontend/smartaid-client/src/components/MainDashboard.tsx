
import { Link } from 'react-router-dom';

function MainDashboard() {
  return (
    <div dir="rtl">
      <h1>SmartAid</h1>
      <p>עזרה חכמה. קהילה חזקה.</p>

      <nav>
        <Link to="/login">התחברות</Link>
        {' | '}
        <Link to="/register">הרשמה</Link>
      </nav>
    </div>
  );
}

export default MainDashboard;
