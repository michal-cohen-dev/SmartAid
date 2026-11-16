
import { useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { apiRequest } from '../services/api';
import './CreateRequestScreen.css';

interface HelpType {
  id: number;
  name: string;
}

interface EquipmentType {
  id: number;
  name: string;
}

interface ImageAnalysisResult {
  problemName: string;
  description: string;
  category: string;
  helpTypeId: number | null;
  riskLevel: string;
  recommendation: string;
  safeActions: string[];
  volunteerRequired: boolean;
  confidence: number;
}

function CreateRequestScreen() {
  const navigate = useNavigate();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [selectedImage, setSelectedImage] = useState<File | null>(null);

  const [analysis, setAnalysis] =
    useState<ImageAnalysisResult | null>(null);

  const [isManualMode, setIsManualMode] = useState(false);
  const [hasAnalyzed, setHasAnalyzed] = useState(false);

  const [helpTypes, setHelpTypes] = useState<HelpType[]>([]);
  const [helpTypeId, setHelpTypeId] = useState('');

  const [equipmentTypes, setEquipmentTypes] =
    useState<EquipmentType[]>([]);
  const [selectedEquipmentIds, setSelectedEquipmentIds] =
    useState<number[]>([]);

  const [latitude, setLatitude] = useState<number | null>(null);
  const [longitude, setLongitude] = useState<number | null>(null);
  const [locationStatus, setLocationStatus] = useState('');

  const [address, setAddress] = useState('');
  const [addressNotes, setAddressNotes] = useState('');

  const [isLoadingLocation, setIsLoadingLocation] = useState(false);
  const [isLoadingHelpTypes, setIsLoadingHelpTypes] = useState(true);
  const [isLoadingEquipmentTypes, setIsLoadingEquipmentTypes] =
    useState(true);
  const [isAnalyzing, setIsAnalyzing] = useState(false);
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

    const loadEquipmentTypes = async () => {
      try {
        const data = await apiRequest<EquipmentType[]>(
          '/api/EquipmentTypes'
        );
        setEquipmentTypes(data);
      } catch (error) {
        setSubmitError(
          error instanceof Error
            ? error.message
            : 'לא ניתן לטעון את סוגי הציוד.'
        );
      } finally {
        setIsLoadingEquipmentTypes(false);
      }
    };

    loadHelpTypes();
    loadEquipmentTypes();
  }, []);

  const handleEquipmentChange = (equipmentId: number) => {
    setSelectedEquipmentIds(previous =>
      previous.includes(equipmentId)
        ? previous.filter(id => id !== equipmentId)
        : [...previous, equipmentId]
    );
  };

  const handleImageChange = (file: File | null) => {
    setSubmitError('');
    setAnalysis(null);
    setHasAnalyzed(false);
    setHelpTypeId('');

    if (!file) {
      setSelectedImage(null);
      return;
    }

    const allowedTypes = [
      'image/jpeg',
      'image/png',
      'image/webp'
    ];

    if (!allowedTypes.includes(file.type.toLowerCase())) {
      setSelectedImage(null);
      setSubmitError(
        'אפשר להעלות תמונה מסוג JPEG, PNG או WebP בלבד.'
      );
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      setSelectedImage(null);
      setSubmitError('גודל התמונה לא יכול לעלות על 5MB.');
      return;
    }

    setSelectedImage(file);
  };

  const handleAnalyzeImage = async () => {
    if (!selectedImage) {
      setSubmitError('יש לבחור תמונה לפני ביצוע הניתוח.');
      return;
    }

    setSubmitError('');
    setIsAnalyzing(true);

    try {
      const imageFormData = new FormData();
      imageFormData.append('image', selectedImage);

      const result = await apiRequest<ImageAnalysisResult>(
        '/api/ImageAnalysis/analyze',
        {
          method: 'POST',
          body: imageFormData
        }
      );

      setAnalysis(result);
      setTitle(result.problemName || '');
      setDescription(result.description || '');

      if (
        result.helpTypeId !== null &&
        result.helpTypeId !== undefined &&
        result.helpTypeId >= 1 &&
        result.helpTypeId <= 11
      ) {
        setHelpTypeId(String(result.helpTypeId));
      } else {
        setHelpTypeId('');
      }

      setHasAnalyzed(true);
      setIsManualMode(false);
    } catch (error) {
      setSubmitError(
        error instanceof Error
          ? error.message
          : 'אירעה שגיאה בניתוח התמונה.'
      );
    } finally {
      setIsAnalyzing(false);
    }
  };

  const handleManualMode = () => {
    setIsManualMode(true);
    setAnalysis(null);
    setHasAnalyzed(false);
    setTitle('');
    setDescription('');
    setHelpTypeId('');
    setSubmitError('');
  };

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
            setLocationStatus(
              'לא ניתנה הרשאה לגשת למיקום שלך.'
            );
            break;

          case error.POSITION_UNAVAILABLE:
            setLocationStatus(
              'לא ניתן לאתר כרגע את המיקום שלך.'
            );
            break;

          case error.TIMEOUT:
            setLocationStatus(
              'בקשת המיקום נמשכה זמן רב מדי. נסי שוב.'
            );
            break;

          default:
            setLocationStatus(
              'אירעה שגיאה בקבלת המיקום.'
            );
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
      setSubmitError(
        'יש להזין כותרת באורך של לפחות 2 תווים.'
      );
      return;
    }

    if (!description.trim() || description.trim().length < 5) {
      setSubmitError(
        'יש להזין תיאור באורך של לפחות 5 תווים.'
      );
      return;
    }

    if (!helpTypeId) {
      setSubmitError('יש לבחור את סוג העזרה המבוקש.');
      return;
    }

    if (selectedEquipmentIds.length > 20) {
      setSubmitError('ניתן לבחור עד 20 סוגי ציוד.');
      return;
    }

    if (latitude === null || longitude === null) {
      setSubmitError(
        'יש לקבל את המיקום שלך לפני שליחת הבקשה.'
      );
      return;
    }

    setIsSubmitting(true);

    try {
      const formData = new FormData();

      if (selectedImage) {
        formData.append('ImageFile', selectedImage);
      }

      formData.append('Title', title.trim());
      formData.append('Description', description.trim());
      formData.append('Latitude', String(latitude));
      formData.append('Longitude', String(longitude));
      formData.append('Address', address.trim());

      if (addressNotes.trim()) {
        formData.append('AddressNotes', addressNotes.trim());
      }

      formData.append('HelpTypeId', helpTypeId);

      selectedEquipmentIds.forEach(id => {
        formData.append('RequiredEquipmentTypeIds', String(id));
      });

      const response = await apiRequest<{
        message: string;
        id: number;
      }>('/api/HelpRequests', {
        method: 'POST',
        body: formData
      });

      navigate(`/suggestions/${response.id}`, {
        state: { analysis }
      });
    } catch (error) {
      setSubmitError(
        error instanceof Error
          ? error.message
          : 'אירעה שגיאה בשליחת הבקשה.'
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
            לא יודעת איך לתאר את הבעיה? ה-AI שלנו יעזור לך!
          </p>
        </header>

        <section className="create-request-card">

          {!hasAnalyzed && !isManualMode && (
            <div className="create-request-form">

              <div className="create-request-field">
                <h2>נתחיל מהתמונה</h2>
                <p>
                  צלמי או העלי תמונה של הבעיה,
                  וה-AI ינסה לזהות אותה ולהציע תיאור מתאים.
                </p>

                <label htmlFor="requestImage">
                  בחירת תמונה
                </label>

                <input
                  id="requestImage"
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  onChange={(e) =>
                    handleImageChange(
                      e.target.files?.[0] || null
                    )
                  }
                />

                {selectedImage && (
                  <p>
                    התמונה שנבחרה: {selectedImage.name}
                  </p>
                )}
              </div>

              {submitError && (
                <p
                  className="create-request-error"
                  role="alert"
                >
                  {submitError}
                </p>
              )}

              <button
                type="button"
                className="create-request-submit"
                onClick={handleAnalyzeImage}
                disabled={!selectedImage || isAnalyzing}
              >
                {isAnalyzing
                  ? 'ה-AI מנתח את התמונה...'
                  : 'נתח את התמונה באמצעות AI'}
              </button>

              <button
                type="button"
                className="create-request-button"
                onClick={handleManualMode}
              >
                אני מעדיפה למלא את הבקשה בעצמי
              </button>

            </div>
          )}

          {hasAnalyzed && analysis && (
            <div className="create-request-form">

              <h2>ה-AI זיהה את הבעיה!</h2>

              <div className="solution-card">
                <div className="solution-content">
                  <h3>{analysis.problemName}</h3>

                  <p>{analysis.description}</p>

                  <p>
                    <strong>קטגוריה:</strong>{' '}
                    {analysis.category}
                  </p>

                  <p>
                    <strong>רמת סיכון:</strong>{' '}
                    {analysis.riskLevel}
                  </p>

                  <p>
                    <strong>המלצת המערכת:</strong>{' '}
                    {analysis.recommendation}
                  </p>
                </div>
              </div>

              <p>
                הפרטים הבאים מולאו אוטומטית.
                אפשר לערוך אותם לפני שליחת הבקשה.
              </p>

              <button
                type="button"
                className="create-request-button"
                onClick={() => {
                  setHasAnalyzed(false);
                  setAnalysis(null);
                  setSelectedImage(null);
                  setTitle('');
                  setDescription('');
                  setHelpTypeId('');
                }}
              >
                העלאת תמונה אחרת
              </button>

            </div>
          )}

          {(hasAnalyzed || isManualMode) && (
            <form
              className="create-request-form"
              onSubmit={handleSubmit}
            >

              <div className="create-request-field">
                <label htmlFor="title">
                  כותרת הבעיה
                </label>

                <input
                  id="title"
                  type="text"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  placeholder="לדוגמה: הרכב אינו מניע"
                  required
                  minLength={2}
                  maxLength={100}
                />
              </div>

              <div className="create-request-field">
                <label htmlFor="description">
                  תיאור הבעיה
                </label>

                <textarea
                  id="description"
                  value={description}
                  onChange={(e) =>
                    setDescription(e.target.value)
                  }
                  placeholder="תיאור הבעיה..."
                  required
                  minLength={5}
                  maxLength={1000}
                />
              </div>

              <div className="create-request-field">
                <label htmlFor="helpType">
                  סוג העזרה
                </label>

                {isLoadingHelpTypes ? (
                  <p>טוען סוגי עזרה...</p>
                ) : (
                  <select
                    id="helpType"
                    value={helpTypeId}
                    onChange={(e) =>
                      setHelpTypeId(e.target.value)
                    }
                    required
                  >
                    <option value="">
                      בחרי סוג עזרה
                    </option>

                    {helpTypes.map((helpType) => (
                      <option
                        key={helpType.id}
                        value={helpType.id}
                      >
                        {helpType.name}
                      </option>
                    ))}
                  </select>
                )}

                {!isLoadingHelpTypes &&
                  helpTypes.length === 0 && (
                    <p>
                      לא נמצאו סוגי עזרה במערכת.
                    </p>
                  )}

                {hasAnalyzed && !helpTypeId && (
                  <p>
                    ה-AI לא הצליח לזהות סוג עזרה מתאים.
                    אנא בחרי את סוג העזרה באופן ידני.
                  </p>
                )}
              </div>

              <div className="create-request-field">
                <h3>ציוד נדרש (לא חובה)</h3>

                <p>
                  סמני את הציוד שהמתנדב צריך להביא איתו.
                  אפשר לבחור יותר מסוג ציוד אחד.
                </p>

                {isLoadingEquipmentTypes ? (
                  <p>טוען סוגי ציוד...</p>
                ) : equipmentTypes.length === 0 ? (
                  <p>לא נמצאו סוגי ציוד במערכת.</p>
                ) : (
                  <div className="create-request-equipment-list">
                    {equipmentTypes.map(equipment => (
                      <label
                        key={equipment.id}
                        className="create-request-equipment-option"
                      >
                        <input
                          type="checkbox"
                          checked={selectedEquipmentIds.includes(
                            equipment.id
                          )}
                          onChange={() =>
                            handleEquipmentChange(equipment.id)
                          }
                          disabled={
                            !selectedEquipmentIds.includes(equipment.id) &&
                            selectedEquipmentIds.length >= 20
                          }
                        />
                        <span>{equipment.name}</span>
                      </label>
                    ))}
                  </div>
                )}

                <p>
                  נבחרו {selectedEquipmentIds.length} סוגי ציוד.
                </p>
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
                  <p
                    className="create-request-status"
                    role="status"
                  >
                    {locationStatus}
                  </p>
                )}

                <div className="create-request-field">
                  <label htmlFor="address">
                    כתובת מדויקת
                  </label>

                  <input
                    id="address"
                    type="text"
                    value={address}
                    onChange={(e) => setAddress(e.target.value)}
                    placeholder="לדוגמה: הרצל 10, פתח תקווה"
                    maxLength={300}
                    required
                  />
                </div>

                <div className="create-request-field">
                  <label htmlFor="addressNotes">
                    הערות למתנדב לגבי המיקום (לא חובה)
                  </label>

                  <textarea
                    id="addressNotes"
                    value={addressNotes}
                    onChange={(e) => setAddressNotes(e.target.value)}
                    placeholder="לדוגמה: קומה 2, דירה 5, כניסה מהחצר"
                    maxLength={300}
                  />
                </div>
              </div>

              {submitError && (
                <p
                  className="create-request-error"
                  role="alert"
                >
                  {submitError}
                </p>
              )}

              <button
                className="create-request-submit"
                type="submit"
                disabled={
                  isSubmitting ||
                  isLoadingHelpTypes ||
                  isLoadingEquipmentTypes ||
                  helpTypes.length === 0
                }
              >
                {isSubmitting
                  ? 'שולח בקשה...'
                  : 'המשך לקבלת עזרה'}
              </button>

            </form>
          )}

        </section>
      </main>
    </div>
  );
}

export default CreateRequestScreen;
