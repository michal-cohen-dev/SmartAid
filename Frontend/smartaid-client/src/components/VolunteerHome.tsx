
import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiRequest } from '../services/api';
import './VolunteerHome.css';
import { useAuth } from '../context/AuthContext';
import { createNotificationsConnection } from '../services/notifications';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { fetchOpenRequests } from '../store/requests/requestsThunks';

interface VolunteerLocation {
  latitude: number;
  longitude: number;
  accuracy: number;
}

function VolunteerHome() {
  const availabilityUpdatePendingRef = useRef(false);
  const navigate = useNavigate();
  const { token, isVolunteer, fullName } = useAuth();

  const dispatch = useAppDispatch();

  const {
    openRequests: requests,
    loading: isLoading,
    error: requestsError,
  } = useAppSelector(state => state.requests);

  const [isAvailable, setIsAvailable] = useState(true);
  const [location, setLocation] =
    useState<VolunteerLocation | null>(null);

  const [locationError, setLocationError] = useState('');
  const [locationSaveError, setLocationSaveError] = useState('');
  const [isGettingLocation, setIsGettingLocation] = useState(false);

  const [error, setError] = useState('');
  const [soundEnabled, setSoundEnabled] = useState(false);

  const audioContextRef = useRef<AudioContext | null>(null);

  // השמעת צליל התראה
  function playNotificationSound() {
    const audioContext = audioContextRef.current;

    if (!audioContext || audioContext.state !== 'running') {
      return;
    }

    const oscillator = audioContext.createOscillator();
    const gain = audioContext.createGain();

    oscillator.type = 'sine';
    oscillator.frequency.setValueAtTime(
      880,
      audioContext.currentTime
    );

    gain.gain.setValueAtTime(0.001, audioContext.currentTime);
    gain.gain.exponentialRampToValueAtTime(
      0.25,
      audioContext.currentTime + 0.03
    );
    gain.gain.exponentialRampToValueAtTime(
      0.001,
      audioContext.currentTime + 0.35
    );

    oscillator.connect(gain);
    gain.connect(audioContext.destination);

    oscillator.start();
    oscillator.stop(audioContext.currentTime + 0.35);
  }

  // התחברות להתראות ועדכוני בקשות בזמן אמת באמצעות SignalR
  useEffect(() => {
    if (!token || !isVolunteer) {
      return;
    }

    let isActive = true;

    const connection = createNotificationsConnection(token);

    const handleNewOffer = () => {
      if (isActive && soundEnabled) {
        playNotificationSound();
      }
    };

    const handleRequestUpdated = () => {
      if (isActive) {
        dispatch(fetchOpenRequests());
      }
    };

    connection.on('NewOffer', handleNewOffer);
    connection.on('RequestUpdated', handleRequestUpdated);

    connection.start().catch(error => {
      if (isActive) {
        console.error('שגיאה בהתחברות להתראות:', error);
      }
    });

    return () => {
      isActive = false;
      connection.off('NewOffer', handleNewOffer);
      connection.off('RequestUpdated', handleRequestUpdated);
      void connection.stop();
    };
  }, [token, isVolunteer, soundEnabled, dispatch]);

  // הפעלת התראות קוליות לאחר לחיצה של המשתמש
  async function handleEnableSound() {
    try {
      const AudioContextClass =
        window.AudioContext ||
        (window as typeof window & {
          webkitAudioContext?: typeof AudioContext;
        }).webkitAudioContext;

      if (!AudioContextClass) {
        setError('הדפדפן אינו תומך בהתראות קוליות.');
        return;
      }

      if (!audioContextRef.current) {
        audioContextRef.current = new AudioContextClass();
      }

      if (audioContextRef.current.state === 'suspended') {
        await audioContextRef.current.resume();
      }

      setSoundEnabled(true);

      // צליל קצר לבדיקת ההתראות
      const context = audioContextRef.current;
      const oscillator = context.createOscillator();
      const gain = context.createGain();

      oscillator.frequency.value = 880;
      gain.gain.setValueAtTime(0.15, context.currentTime);
      gain.gain.exponentialRampToValueAtTime(
        0.001,
        context.currentTime + 0.25
      );

      oscillator.connect(gain);
      gain.connect(context.destination);

      oscillator.start();
      oscillator.stop(context.currentTime + 0.25);
    } catch {
      setError('לא הצלחנו להפעיל את הצליל. נסי שוב.');
    }
  }

  // טעינת בקשות עזרה פתוחות ורענון אוטומטי באמצעות Redux
  useEffect(() => {
    dispatch(fetchOpenRequests());

    const intervalId = window.setInterval(() => {
      dispatch(fetchOpenRequests());
    }, 15000);

    return () => {
      window.clearInterval(intervalId);
    };
  }, [dispatch]);

  // מעקב רציף אחר מיקום המתנדב ושליחתו לשרת
  useEffect(() => {
    if (!isAvailable) {
      setLocation(null);
      setLocationError('');
      setLocationSaveError('');
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
    setLocationSaveError('');

    let lastLocationSentAt = 0;
    let isSavingLocation = false;
    let isActive = true;

    const watchId = navigator.geolocation.watchPosition(
      position => {
        const latitude = position.coords.latitude;
        const longitude = position.coords.longitude;

        setLocation({
          latitude,
          longitude,
          accuracy: position.coords.accuracy,
        });

        setLocationError('');
        setIsGettingLocation(false);

        const now = Date.now();

        // שליחת המיקום לשרת לכל היותר פעם ב-10 שניות
        if (
          now - lastLocationSentAt >= 10000 &&
          !isSavingLocation
        ) {
          lastLocationSentAt = now;
          isSavingLocation = true;

          apiRequest<{ message: string }>(
            '/api/Volunteers/me/location',
            {
              method: 'PUT',
              body: JSON.stringify({
                latitude,
                longitude,
              }),
            }
          )
            .then(async () => {
              if (!isActive) return;

              setLocationSaveError('');

              // לאחר שהמיקום נשמר, מעדכנים זמינות בשרת
              if (availabilityUpdatePendingRef.current) {
                await apiRequest<{ message: string }>(
                  '/api/Volunteers/me/availability',
                  {
                    method: 'PUT',
                    body: JSON.stringify({
                      isAvailable: true,
                    }),
                  }
                );

                availabilityUpdatePendingRef.current = false;
              }
            })
            .catch(error => {
              if (!isActive) return;

              availabilityUpdatePendingRef.current = false;
              setIsAvailable(false);

              setLocationSaveError(
                error instanceof Error
                  ? error.message
                  : 'עדכון הזמינות נכשל'
              );
            })
            .finally(() => {
              isSavingLocation = false;
            });
        }
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
      isActive = false;
      navigator.geolocation.clearWatch(watchId);
    };
  }, [isAvailable]);

  // סגירת משאב הצליל ביציאה מהעמוד
  useEffect(() => {
    return () => {
      audioContextRef.current?.close();
      audioContextRef.current = null;
    };
  }, []);

  async function handleToggle() {
    if (isAvailable) {
      try {
        await apiRequest<{ message: string }>(
          '/api/Volunteers/me/availability',
          {
            method: 'PUT',
            body: JSON.stringify({
              isAvailable: false,
            }),
          }
        );

        availabilityUpdatePendingRef.current = false;
        setIsAvailable(false);
        setLocation(null);
        setLocationError('');
        setLocationSaveError('');
      } catch (error) {
        setLocationSaveError(
          error instanceof Error
            ? error.message
            : 'לא הצלחנו לעדכן זמינות'
        );
      }
    } else {
      availabilityUpdatePendingRef.current = true;
      setIsAvailable(true);
    }
  }

  return (
    <div className="volunteer-page" dir="rtl">

      <header className="volunteer-header">
        <h1>SmartAid</h1>
        <p>
          שלום, {fullName || 'מתנדב'}! כאן תוכל לראות את בקשות העזרה הפתוחות.
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

              {locationSaveError && (
                <p role="alert">{locationSaveError}</p>
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

      <section className="volunteer-notifications">
        <h2>התראות על בקשות חדשות</h2>

        <p>
          {soundEnabled
            ? 'התראות קוליות מופעלות.'
            : 'כדי לשמוע צליל כאשר מגיעה בקשה חדשה, יש להפעיל התראות.'}
        </p>

        <button
          className="volunteer-button"
          type="button"
          onClick={handleEnableSound}
          disabled={soundEnabled}
        >
          {soundEnabled
            ? 'הצליל מופעל'
            : 'הפעלת צליל התראות'}
        </button>
      </section>

      <section className="volunteer-requests-section">

        <h2>בקשות עזרה פתוחות</h2>

        {isLoading && (
          <p className="volunteer-message">
            טוען בקשות מהשרת...
          </p>
        )}

        {requestsError && (
          <p className="volunteer-message" role="alert">
            {requestsError}
          </p>
        )}

        {error && (
          <p className="volunteer-message" role="alert">
            {error}
          </p>
        )}

        {!isLoading && !requestsError && !error && requests.length === 0 && (
          <p className="volunteer-message">
            אין כרגע בקשות עזרה פתוחות.
          </p>
        )}

        {!isLoading && !requestsError && !error && requests.length > 0 && (
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

    </div>
  );
}

export default VolunteerHome;
