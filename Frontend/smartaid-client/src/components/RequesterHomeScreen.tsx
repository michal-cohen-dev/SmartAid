
import { useNavigate } from 'react-router-dom';

function RequesterHomeScreen() {
  const navigate = useNavigate();

  function handleCreateRequest() {
    navigate('/request');
  }

  return (
    <div dir="rtl">
      <h1>שלום, ברוכים הבאים ל־SmartAid</h1>

      <p>
        צריכים עזרה? אנחנו כאן כדי לחבר אתכם לאנשים
        שיוכלו לעזור לכם.
      </p>

      <section>
        <h2>איך נוכל לעזור לכם היום?</h2>

        <p>
          תארו את הבעיה שלכם, וקבלו הצעות לפתרון
          או עזרה ממתנדב שנמצא בקרבתכם.
        </p>

        <button
          type="button"
          onClick={handleCreateRequest}
        >
          פתיחת בקשת עזרה
        </button>
      </section>
    </div>
  );
}

export default RequesterHomeScreen;
