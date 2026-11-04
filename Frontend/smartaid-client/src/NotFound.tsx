import { Link } from "react-router-dom";

function NotFound() {
  return (
    <div dir="rtl">
      <h1>העמוד לא נמצא</h1>
      <p>הכתובת שהזנת אינה קיימת במערכת.</p>
      <Link to="/">חזרה לדף הבית</Link>
    </div>
  );
}

export default NotFound;