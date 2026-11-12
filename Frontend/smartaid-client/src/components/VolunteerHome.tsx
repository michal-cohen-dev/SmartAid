
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiRequest } from '../services/api';

interface HelpRequest {
  id: number;
  userId: number;
  title: string;
  description: string;
  userFullName: string | null;
  imageName: string | null;
  latitude: number;
  longitude: number;
  address: string;
  addressNotes: string | null;
  status: number;
  assignedVolunteerId: number | null;
  assignedVolunteerName: string | null;
  createdAt: string;
  rating: number | null;
  requiredEquipmentTypeIds: number[];
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
          // הצגת בקשות שיש להן כותרת ותיאור
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
    <div dir="rtl">
      <h1>SmartAid</h1>
      <h2>שלום, מתנדב!</h2>

      <div>
        <h3>הזמינות שלי</h3>

        <p>
          {isAvailable
            ? 'זמין לקבלת קריאות'
            : 'לא זמין'}
        </p>

        <button type="button" onClick={handleToggle}>
          שינוי סטטוס זמינות
        </button>
      </div>

      <div>
        <h2>בקשות עזרה פתוחות</h2>

        {isLoading && (
          <p>טוען בקשות מהשרת...</p>
        )}

        {error && (
          <p role="alert">{error}</p>
        )}

        {!isLoading && !error && requests.length === 0 && (
          <p>אין כרגע בקשות עזרה פתוחות.</p>
        )}

        {!isLoading && !error && requests.map(request => (
          <div key={request.id}>
            <h3>{request.title}</h3>

            <p>{request.description}</p>

            <p>
              פונה: {request.userFullName || 'לא צוין'}
            </p>

            <p>
              מיקום: {request.address}
            </p>

            {request.addressNotes && (
              <p>
                הערות מיקום: {request.addressNotes}
              </p>
            )}

            <p>
              תאריך יצירה:{' '}
              {new Date(request.createdAt).toLocaleString('he-IL')}
            </p>

            <p>סטטוס: בקשה חדשה</p>

            <button
              type="button"
              onClick={() => navigate(`/request-details/${request.id}`)}
            >
              צפייה בפרטי הבקשה
            </button>
          </div>
        ))}
      </div>

      <button
        type="button"
        onClick={() => navigate('/request')}
      >
        פתיחת בקשת עזרה
      </button>
    </div>
  );
}

export default VolunteerHome;
