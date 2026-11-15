
import { useNavigate } from 'react-router-dom';
import './RequesterHomeScreen.css';

function RequesterHomeScreen() {
  const navigate = useNavigate();

  function handleCreateRequest() {
    navigate('/request');
  }

  
  return (
    <div className="requester-home-page" dir="rtl">
      <main className="requester-home-container">
        <header className="requester-home-header">
          <h1>שלום, ברוכים הבאים ל־SmartAid</h1>

          <p>
            צריכים עזרה? אנחנו כאן כדי לחבר אתכם לאנשים
            שיוכלו לעזור לכם.
          </p>
        </header>

        <section className="requester-home-card">
          <h2>איך נוכל לעזור לכם היום?</h2>

          <p>
            תארו את הבעיה שלכם, וקבלו הצעות לפתרון
            או עזרה ממתנדב שנמצא בקרבתכם.
          </p>

          <button
            className="requester-create-button"
            type="button"
            onClick={handleCreateRequest}
          >
            פתיחת בקשת עזרה
          </button>
        </section>
      </main>
    </div>
  );

}

export default RequesterHomeScreen;
