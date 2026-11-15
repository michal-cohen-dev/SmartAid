
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

interface VolunteerLocation {
  latitude: number;
  longitude: number;
  accuracy: number;
}

function VolunteerHome() {
  const navigate = useNavigate();

  const [isAvailable, setIsAvailable] = useState(false);
  const [location, setLocation] =
    useState<VolunteerLocation | null>(null);

  const [locationError, setLocationError] = useState('');
  const [isGettingLocation, setIsGettingLocation] = useState(false);

  const [requests, setRequests] = useState<HelpRequest[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  // טעינת בקשות עזרה פתוחות ורענון אוטומטי
useEffect(() => {
  let isMounted = true;

  const loadRequests = async (showLoading = false) => {
    try {
      if (showLoading) {
        setIsLoading(true);
      }

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
      if (isMounted && showLoading) {
        setIsLoading(false);
      }
    }
  };

  loadRequests(true);

  const intervalId = window.setInterval(() => {
    loadRequests();
  }, 15000);

  return () => {
    isMounted = false;
    window.clearInterval(intervalId);
  };
}, []);

  // מעקב רציף אחר מיקום המתנדב
  useEffect(() => {
    if (!isAvailable) {
      setLocation(null);
      setLocationError('');
      setIsGettingLocation(false);
      return;
    }

    if (!navigator.geolocation) {
      setLocationError(
        'הדפדפן אינו תומך בקבלת מיקום.'
      );
      setIsAvailable(false);
      return;
    }

    setIsGettingLocation(true);
    setLocationError('');

    const watchId = navigator.geolocation.watchPosition(
      position => {
        setLocation({
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
          accuracy: position.coords.accuracy,
        });

        setLocationError('');
        setIsGettingLocation(false);
      },
      error => {
        setIsGettingLocation(false);

        if (error.code === error.PERMISSION_DENIED) {
          setLocationError(
            'לא ניתנה הרשאה לקבלת מיקום.'
          );
          setIsAvailable(false);
        } else {
          setLocationError(
            'לא הצלחנו לקבל את המיקום כרגע. ננסה שוב.'
          );
        }
      },
      {
        enableHighAccuracy: true,
        maximumAge: 0,
        timeout: 15000,
      }
    );

    return () => {
      navigator.geolocation.clearWatch(watchId);
    };
  }, [isAvailable]);

  function handleToggle() {
    setIsAvailable(previous => !previous);
  }

  return (
    <div className="volunteer-page" dir="rtl">

      <header className="volunteer-header">
        <h1>SmartAid</h1>
        <p>
          שלום, מתנדב! כאן תוכל לראות את בקשות העזרה הפתוחות.
        </p>
      </header>

      <section className="volunteer-availability">

        <div>
          <h2>הזמינות שלי</h2>

          <p>
            {isAvailable
              ? 'זמין לקבלת קריאות'
              : 'לא זמין'}
          </p>

          {isAvailable && (
            <div className="volunteer-location-status">

              {isGettingLocation && (
                <p>מאתר את המיקום שלך...</p>
              )}

              {locationError && (
                <p role="alert">{locationError}</p>
              )}

              {location && (
  <div className="volunteer-location-details">
    <p>המיקום שלך זוהה בהצלחה!</p>

    <p>
      קו רוחב: {location.latitude.toFixed(6)}
    </p>

    <p>
      קו אורך: {location.longitude.toFixed(6)}
    </p>

    <p>
      רמת דיוק משוערת: {Math.round(location.accuracy)} מטר
    </p>

    <div className="volunteer-map">
      <h3>המיקום הנוכחי שלך</h3>

      <iframe
        title="המיקום הנוכחי של המתנדב"
        src={`https://maps.google.com/maps?q=${location.latitude},${location.longitude}&z=16&output=embed`}
        width="100%"
        height="300"
        loading="lazy"
        referrerPolicy="no-referrer-when-downgrade"
        allowFullScreen
      />

      <a
        href={`https://www.google.com/maps/search/?api=1&query=${location.latitude},${location.longitude}`}
        target="_blank"
        rel="noopener noreferrer"
      >
        פתיחה ב־Google Maps
      </a>
    </div>
  </div>
)}

            </div>
          )}
        </div>

        <button
          className="volunteer-button"
          type="button"
          onClick={handleToggle}
        >
          {isAvailable
            ? 'הפסקת זמינות'
            : 'הפעלה וקבלת מיקום'}
        </button>

      </section>

      <section className="volunteer-requests-section">

        <h2>בקשות עזרה פתוחות</h2>

        {isLoading && (
          <p className="volunteer-message">
            טוען בקשות מהשרת...
          </p>
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

                {Array.isArray(request.requiredEquipmentNames) &&
  request.requiredEquipmentNames.length > 0 && (
    <div className="volunteer-request-equipment">
      <strong>ציוד נדרש:</strong>
      <ul>
        {request.requiredEquipmentNames.map((name, index) => (
          <li key={`${name}-${index}`}>{name}</li>
        ))}
      </ul>
    </div>
  )}

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
                  onClick={() =>
                    navigate(`/request-details/${request.id}`)
                  }
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
