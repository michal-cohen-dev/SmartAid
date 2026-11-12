
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiRequest } from '../services/api';
import './VolunteerHome.css';

interface HelpRequest {
  id: number;
  title: string;
  description: string;
  createdAt: string;
  requiredEquipmentTypeIds: number[];
  requiredEquipmentNames: string[];
}

function VolunteerHome() {
  const navigate = useNavigate();

  const [isAvailable, setIsAvailable] = useState(true);
  const [requests, setRequests] = useState<HelpRequest[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let isMounted = true;

    const loadRequests = async () => {
      try {
        setIsLoading(true);
        setError('');

        const data = await apiRequest<HelpRequest[]>(
          '/api/HelpRequests/open'
        );

        if (isMounted) {
          const validRequests = data.filter(
            request =>
              request.title?.trim() &&
              request.description?.trim()
          );

          setRequests(validRequests);
        }
      } catch (error) {
        if (isMounted) {
          setError(
            error instanceof Error
              ? error.message
              : 'אירעה שגיאה בטעינת בקשות העזרה.'
          );
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    loadRequests();

    return () => {
      isMounted = false;
    };
  }, []);

  function handleToggle() {
    setIsAvailable(previous => !previous);
  }

  
  return (
    <div className="volunteer-page" dir="rtl">
      <header className="volunteer-header">
        <h1>SmartAid</h1>
        <p>שלום, מתנדב! כאן תוכל לראות את בקשות העזרה הפתוחות.</p>
      </header>

      <section className="volunteer-availability">
        <div>
          <h2>הזמינות שלי</h2>
          <p>
            {isAvailable ? 'זמין לקבלת קריאות' : 'לא זמין'}
          </p>
        </div>

        <button
          className="volunteer-button"
          type="button"
          onClick={handleToggle}
        >
          שינוי סטטוס זמינות
        </button>
      </section>

      <section className="volunteer-requests-section">
        <h2>בקשות עזרה פתוחות</h2>

        {isLoading && (
          <p className="volunteer-message">טוען בקשות מהשרת...</p>
        )}

        {error && (
          <p className="volunteer-message" role="alert">
            {error}
          </p>
        )}

        {!isLoading && !error && requests.length === 0 && (
          <p className="volunteer-message">
            אין כרגע בקשות עזרה פתוחות.
          </p>
        )}

        {!isLoading && !error && requests.length > 0 && (
          <div className="volunteer-requests-grid">
            {requests.map(request => (
              <article
                className="volunteer-request-card"
                key={request.id}
              >
                <h3>{request.title}</h3>

                <p className="volunteer-request-description">
                  {request.description}
                </p>

                <p className="volunteer-request-meta">
                  תאריך יצירה:{' '}
                  {new Date(request.createdAt).toLocaleString('he-IL')}
                </p>

                <p className="volunteer-request-meta">
                  סטטוס: בקשה חדשה
                </p>

                <button
                  className="volunteer-button"
                  type="button"
                  onClick={() => navigate(`/request-details/${request.id}`)}
                >
                  צפייה בפרטי הבקשה
                </button>
              </article>
            ))}
          </div>
        )}
      </section>

      <div className="volunteer-create-request">
        <button
          className="volunteer-button"
          type="button"
          onClick={() => navigate('/request')}
        >
          פתיחת בקשת עזרה
        </button>
      </div>
    </div>
  );

}

export default VolunteerHome;
