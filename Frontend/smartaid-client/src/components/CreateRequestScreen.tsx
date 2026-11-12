
import { useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { apiRequest } from '../services/api';
import './CreateRequestScreen.css';

interface HelpType {
  id: number;
  name: string;
}

function CreateRequestScreen() {
  const navigate = useNavigate();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [helpTypes, setHelpTypes] = useState<HelpType[]>([]);
  const [helpTypeId, setHelpTypeId] = useState('');

  const [latitude, setLatitude] = useState<number | null>(null);
  const [longitude, setLongitude] = useState<number | null>(null);
  const [locationStatus, setLocationStatus] = useState('');
  const [isLoadingLocation, setIsLoadingLocation] = useState(false);
  const [isLoadingHelpTypes, setIsLoadingHelpTypes] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState('');

  useEffect(() => {
    document.title = 'בקשת עזרה חדשה | SmartAid';

    const loadHelpTypes = async () => {
      try {
        const data = await apiRequest<HelpType[]>('/api/HelpTypes');
        setHelpTypes(data);
      } catch (error) {
        setSubmitError(
          error instanceof Error
            ? error.message
            : 'לא ניתן לטעון את סוגי העזרה.'
        );
      } finally {
        setIsLoadingHelpTypes(false);
      }
    };

    loadHelpTypes();
  }, []);

  const handleGetLocation = () => {
    if (!navigator.geolocation) {
      setLocationStatus('הדפדפן שלך אינו תומך בקבלת מיקום.');
      return;
    }

    setIsLoadingLocation(true);
    setLocationStatus('מנסה לקבל את המיקום שלך...');

    navigator.geolocation.getCurrentPosition(
      (position) => {
        setLatitude(position.coords.latitude);
        setLongitude(position.coords.longitude);
        setLocationStatus('המיקום שלך התקבל בהצלחה!');
        setIsLoadingLocation(false);
      },
      (error) => {
        setIsLoadingLocation(false);

        switch (error.code) {
          case error.PERMISSION_DENIED:
            setLocationStatus('לא ניתנה הרשאה לגשת למיקום שלך.');
            break;
          case error.POSITION_UNAVAILABLE:
            setLocationStatus('לא ניתן לאתר כרגע את המיקום שלך.');
            break;
          case error.TIMEOUT:
            setLocationStatus('בקשת המיקום נמשכה זמן רב מדי. נסי שוב.');
            break;
          default:
            setLocationStatus('אירעה שגיאה בקבלת המיקום.');
        }
      },
      {
        enableHighAccuracy: true,
        timeout: 15000,
        maximumAge: 0
      }
    );
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitError('');

    if (!title.trim() || title.trim().length < 2) {
      setSubmitError('יש להזין כותרת באורך של לפחות 2 תווים.');
      return;
    }

    if (!description.trim() || description.trim().length < 5) {
      setSubmitError('יש להזין תיאור באורך של לפחות 5 תווים.');
      return;
    }

    if (!helpTypeId) {
      setSubmitError('יש לבחור את סוג העזרה המבוקש.');
      return;
    }

    if (latitude === null || longitude === null) {
      setSubmitError('יש לקבל את המיקום שלך לפני שליחת הבקשה.');
      return;
    }

    setIsSubmitting(true);

    try {
      const formData = new FormData();

      formData.append('Title', title.trim());
      formData.append('Description', description.trim());
      formData.append('Latitude', String(latitude));
      formData.append('Longitude', String(longitude));
      formData.append('Address', 'מיקום לפי קואורדינטות GPS');
      formData.append('HelpTypeId', helpTypeId);

      await apiRequest<{ message: string }>('/api/HelpRequests', {
        method: 'POST',
        body: formData
      });

      navigate('/suggestions');
    } catch (error) {
      setSubmitError(
        error instanceof Error
          ? error.message
          : 'אירעה שגיאה בשליחת בקשת העזרה.'
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  
  return (
    <div className="create-request-page" dir="rtl">
      <main className="create-request-container">
        <header className="create-request-header">
          <h1>בקשת עזרה חדשה</h1>
          <p>
            תארי את הבעיה כדי שנוכל למצוא עבורך עזרה מתאימה
          </p>
        </header>

        <section className="create-request-card">
          <form className="create-request-form" onSubmit={handleSubmit}>
            <div className="create-request-field">
              <label htmlFor="title">כותרת הבקשה</label>
              <input
                id="title"
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="לדוגמה: תקר בגלגל"
                required
                minLength={2}
                maxLength={100}
              />
            </div>

            <div className="create-request-field">
              <label htmlFor="description">תיאור הבעיה</label>
              <textarea
                id="description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="תארי מה קרה ואיזו עזרה נדרשת"
                required
                minLength={5}
                maxLength={1000}
              />
            </div>

            <div className="create-request-field">
              <label htmlFor="helpType">סוג העזרה</label>

              {isLoadingHelpTypes ? (
                <p>טוען סוגי עזרה...</p>
              ) : (
                <select
                  id="helpType"
                  value={helpTypeId}
                  onChange={(e) => setHelpTypeId(e.target.value)}
                  required
                >
                  <option value="">בחרי סוג עזרה</option>
                  {helpTypes.map((helpType) => (
                    <option key={helpType.id} value={helpType.id}>
                      {helpType.name}
                    </option>
                  ))}
                </select>
              )}

              {!isLoadingHelpTypes && helpTypes.length === 0 && (
                <p>לא נמצאו סוגי עזרה במערכת.</p>
              )}
            </div>

            <div className="create-request-location">
              <label>מיקום</label>

              <button
                className="create-request-button"
                type="button"
                onClick={handleGetLocation}
                disabled={isLoadingLocation}
              >
                {isLoadingLocation
                  ? 'מקבל מיקום...'
                  : 'קבלת המיקום הנוכחי שלי'}
              </button>

              {locationStatus && (
                <p className="create-request-status" role="status">
                  {locationStatus}
                </p>
              )}
            </div>

            {submitError && (
              <p className="create-request-error" role="alert">
                {submitError}
              </p>
            )}

            <button
              className="create-request-submit"
              type="submit"
              disabled={
                isSubmitting ||
                isLoadingHelpTypes ||
                helpTypes.length === 0
              }
            >
              {isSubmitting ? 'שולח בקשה...' : 'שליחת בקשת עזרה'}
            </button>
          </form>
        </section>
      </main>
    </div>
  );

}

export default CreateRequestScreen;
