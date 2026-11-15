
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { apiRequest } from '../services/api';
import './RequestDetailsScreen.css';

interface HelpRequestPreview {
  id: number;
  title: string;
  description: string;
  createdAt: string;
  requiredEquipmentTypeIds: number[];
  requiredEquipmentNames: string[];
}

interface HelpRequestDetails extends HelpRequestPreview {
  userId: number;
  userFullName: string | null;
  imageName: string | null;
  latitude: number;
  longitude: number;
  address: string;
  addressNotes: string | null;
  status: number;
  assignedVolunteerId: number | null;
  assignedVolunteerName: string | null;
  rating: number | null;
}

function RequestDetailsScreen() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [preview, setPreview] = useState<HelpRequestPreview | null>(null);
  const [details, setDetails] = useState<HelpRequestDetails | null>(null);

  const [isLoading, setIsLoading] = useState(true);
  const [isAccepting, setIsAccepting] = useState(false);
  const [isUpdatingStatus, setIsUpdatingStatus] = useState(false);
  const [error, setError] = useState('');

  async function loadFullRequest() {
    if (!id) return;

    const fullRequest = await apiRequest<HelpRequestDetails>(
      `/api/HelpRequests/${id}`
    );

    setDetails(fullRequest);
    setPreview(null);
  }

  useEffect(() => {
    let isMounted = true;

    async function loadRequest() {
      if (!id) {
        setError('מזהה הבקשה אינו תקין.');
        setIsLoading(false);
        return;
      }

      try {
        setIsLoading(true);
        setError('');

        const openRequests = await apiRequest<HelpRequestPreview[]>(
          '/api/HelpRequests/open'
        );

        const requestPreview = openRequests.find(
          request => request.id === Number(id)
        );

        if (requestPreview) {
          if (isMounted) setPreview(requestPreview);
          return;
        }

        const fullRequest = await apiRequest<HelpRequestDetails>(
          `/api/HelpRequests/${id}`
        );

        if (isMounted) setDetails(fullRequest);
      } catch {
        if (isMounted) {
          setError(
            'לא ניתן לטעון את הבקשה. ייתכן שהיא כבר אינה זמינה או שאינה משויכת אליך.'
          );
        }
      } finally {
        if (isMounted) setIsLoading(false);
      }
    }

    loadRequest();

    return () => {
      isMounted = false;
    };
  }, [id]);

  async function handleAcceptRequest() {
    if (!id) return;

    try {
      setIsAccepting(true);
      setError('');

      await apiRequest<{ message: string }>(
        `/api/HelpRequests/${id}/accept`,
        { method: 'POST' }
      );

      await loadFullRequest();
    } catch (error) {
      setError(
        error instanceof Error
          ? error.message
          : 'אירעה שגיאה בקבלת הבקשה.'
      );
    } finally {
      setIsAccepting(false);
    }
  }

  async function handleMarkArrived() {
    if (!id) return;

    try {
      setIsUpdatingStatus(true);
      setError('');

      await apiRequest<{ message: string }>(
        `/api/HelpRequests/${id}/arrive`,
        { method: 'POST' }
      );

      await loadFullRequest();
    } catch (error) {
      setError(
        error instanceof Error
          ? error.message
          : 'אירעה שגיאה בסימון ההגעה.'
      );
    } finally {
      setIsUpdatingStatus(false);
    }
  }

  async function handleCompleteRequest() {
    if (!id) return;

    try {
      setIsUpdatingStatus(true);
      setError('');

      await apiRequest<{ message: string }>(
        `/api/HelpRequests/${id}/complete`,
        { method: 'POST' }
      );

      await loadFullRequest();
    } catch (error) {
      setError(
        error instanceof Error
          ? error.message
          : 'אירעה שגיאה בסיום הטיפול.'
      );
    } finally {
      setIsUpdatingStatus(false);
    }
  }

  function formatDate(date: string) {
    return new Date(date).toLocaleString('he-IL');
  }

  if (isLoading) {
    return (
      <main className="request-details-page" dir="rtl">
        <div className="request-details-state">
          <span className="loading-spinner" />
          <p>טוען פרטי בקשה...</p>
        </div>
      </main>
    );
  }

  if (!preview && !details) {
    return (
      <main className="request-details-page" dir="rtl">
        <div className="request-details-state">
          <span className="state-icon">!</span>
          <h2>לא ניתן להציג את הבקשה</h2>
          <p>{error || 'הבקשה אינה זמינה.'}</p>

          <button
            className="secondary-button"
            onClick={() => navigate('/volunteer')}
          >
            חזרה למסך המתנדב
          </button>
        </div>
      </main>
    );
  }

  const request = details || preview;
  const equipmentNames = request?.requiredEquipmentNames ?? [];

  const statusText = details
    ? details.status === 1
      ? 'הבקשה הוקצתה אליך'
      : details.status === 4
        ? 'הטיפול מתבצע'
        : details.status === 2
          ? 'הטיפול הושלם'
          : 'פרטי הבקשה'
    : 'בקשה פתוחה';

  return (
    <main className="request-details-page" dir="rtl">
      <div className="request-details-container">

        <header className="details-header">
          <button
            className="back-button"
            onClick={() => navigate(-1)}
          >
            ← חזרה
          </button>

          <div>
            <span className="details-eyebrow">SMART AID</span>
            <h1>פרטי בקשת עזרה</h1>
            <p>בקשה מספר {request?.id}</p>
          </div>

          <span className="status-badge">{statusText}</span>
        </header>

        {error && (
          <div className="details-error" role="alert">
            {error}
          </div>
        )}

        <section className="details-card">
          <div className="details-card-header">
            <div className="request-title-icon">✚</div>

            <div>
              <span className="section-label">בקשת עזרה</span>
              <h2>{request?.title}</h2>
            </div>
          </div>

          <div className="details-section">
            <h3>תיאור הבקשה</h3>
            <p className="description-text">
              {request?.description}
            </p>
          </div>

          <div className="details-section">
            <h3>ציוד נדרש</h3>

            {equipmentNames.length > 0 ? (
              <div className="equipment-list">
                {equipmentNames.map((equipmentName, index) => (
                  <span
                    className="equipment-chip"
                    key={`${equipmentName}-${index}`}
                  >
                    {equipmentName}
                  </span>
                ))}
              </div>
            ) : (
              <p className="muted-text">לא נדרש ציוד מיוחד.</p>
            )}
          </div>

          <div className="details-section">
            <h3>פרטי זמן</h3>
            <p className="detail-value">
              {formatDate(request!.createdAt)}
            </p>
          </div>

          {details && (
            <>
              <div className="full-details-divider" />

              <div className="full-details-heading">
                <span>🔓</span>
                <div>
                  <h3>פרטים מלאים</h3>
                  <p>הפרטים זמינים לאחר קבלת הבקשה</p>
                </div>
              </div>

              <div className="details-grid">
                <div className="detail-item">
                  <span>שם הפונה</span>
                  <strong>
                    {details.userFullName || 'לא צוין'}
                  </strong>
                </div>

                <div className="detail-item">
                  <span>כתובת</span>
                  <strong>{details.address || 'לא צוינה'}</strong>
                </div>

                <div className="detail-item">
                  <span>הערות מיקום</span>
                  <strong>
                    {details.addressNotes || 'אין הערות נוספות'}
                  </strong>
                </div>

                <div className="detail-item">
                  <span>מזהה הבקשה</span>
                  <strong>{details.id}</strong>
                </div>
              </div>
            </>
          )}

          {preview && (
            <div className="accept-section">
              <p>
                לאחר קבלת הבקשה, יוצגו לך פרטי הפונה והמיקום המלאים.
              </p>

              <button
                className="accept-request-button"
                onClick={handleAcceptRequest}
                disabled={isAccepting}
              >
                {isAccepting ? 'מקבל את הבקשה...' : 'קבלת הבקשה'}
              </button>
            </div>
          )}

          {details && details.status === 1 && (
            <div className="accept-section">
              <p>הבקשה משויכת אליך. לאחר הגעתך למיקום, סמן הגעה.</p>

              <button
                className="accept-request-button"
                onClick={handleMarkArrived}
                disabled={isUpdatingStatus}
              >
                {isUpdatingStatus ? 'מעדכן...' : 'הגעתי למיקום'}
              </button>
            </div>
          )}

          {details && details.status === 4 && (
            <div className="accept-section">
              <p>הגעת למיקום והטיפול מתבצע. בסיום הטיפול לחץ על הכפתור.</p>

              <button
                className="accept-request-button"
                onClick={handleCompleteRequest}
                disabled={isUpdatingStatus}
              >
                {isUpdatingStatus ? 'מעדכן...' : 'סיום טיפול'}
              </button>
            </div>
          )}

          {details && details.status === 2 && (
            <div className="accepted-message">
              ✓ הטיפול בבקשה הושלם בהצלחה.
            </div>
          )}
        </section>
      </div>
    </main>
  );
}

export default RequestDetailsScreen;
