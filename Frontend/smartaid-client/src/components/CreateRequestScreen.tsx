
import { useNavigate } from "react-router-dom";
import { useState, useEffect } from "react";
import { apiRequest } from "../services/api";
import "./CreateRequestScreen.css";

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
  suggestedEquipmentTypeIds: number[];
}

function CreateRequestScreen() {
  const navigate = useNavigate();

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [selectedImage, setSelectedImage] = useState<File | null>(null);

  const [analysis, setAnalysis] =
    useState<ImageAnalysisResult | null>(null);

  const [hasAnalyzed, setHasAnalyzed] = useState(false);

  const [helpTypes, setHelpTypes] = useState<HelpType[]>([]);
  const [helpTypeId, setHelpTypeId] = useState("");

  const [equipmentTypes, setEquipmentTypes] =
    useState<EquipmentType[]>([]);
  const [selectedEquipmentIds, setSelectedEquipmentIds] =
    useState<number[]>([]);

  const [latitude, setLatitude] = useState<number | null>(null);
  const [longitude, setLongitude] = useState<number | null>(null);
  const [locationStatus, setLocationStatus] = useState("");

  const [address, setAddress] = useState("");
  const [addressNotes, setAddressNotes] = useState("");

  const [isLoadingLocation, setIsLoadingLocation] = useState(false);
  const [isLoadingData, setIsLoadingData] = useState(true);
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [submitError, setSubmitError] = useState("");

  useEffect(() => {
    document.title = "בקשת עזרה חדשה | SmartAid";

    const loadData = async () => {
      try {
        const [helpTypesData, equipmentTypesData] = await Promise.all([
          apiRequest<HelpType[]>("/api/HelpTypes"),
          apiRequest<EquipmentType[]>("/api/EquipmentTypes")
        ]);

        setHelpTypes(helpTypesData);
        setEquipmentTypes(equipmentTypesData);
      } catch (error) {
        setSubmitError(
          error instanceof Error
            ? error.message
            : "לא ניתן לטעון את הנתונים הדרושים."
        );
      } finally {
        setIsLoadingData(false);
      }
    };

    loadData();
  }, []);

  const handleImageChange = (file: File | null) => {
    setSubmitError("");

    if (!file) {
      setSelectedImage(null);
      return;
    }

    const allowedTypes = [
      "image/jpeg",
      "image/png",
      "image/webp"
    ];

    if (!allowedTypes.includes(file.type.toLowerCase())) {
      setSubmitError(
        "אפשר להעלות תמונה מסוג JPEG, PNG או WebP בלבד."
      );
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      setSubmitError("גודל התמונה לא יכול לעלות על 5MB.");
      return;
    }

    setSelectedImage(file);

    // שינוי התמונה מחייב ניתוח מחדש.
    setAnalysis(null);
    setHasAnalyzed(false);
    setHelpTypeId("");
    setSelectedEquipmentIds([]);
  };

  const handleAnalyze = async () => {
    setSubmitError("");

    if (description.trim().length < 5) {
      setSubmitError(
        "יש לתאר את הבעיה בלפחות 5 תווים לפני הניתוח."
      );
      return;
    }

    setIsAnalyzing(true);

    try {
      const formData = new FormData();

      formData.append("description", description.trim());

      if (selectedImage) {
        formData.append("image", selectedImage);
      }

      const result = await apiRequest<ImageAnalysisResult>(
        "/api/ImageAnalysis/analyze",
        {
          method: "POST",
          body: formData
        }
      );

      setAnalysis(result);
      setTitle(result.problemName || "");

      setDescription(result.description || description.trim());

      const matchingHelpType = helpTypes.find(
        (item) => item.id === result.helpTypeId
      );

      setHelpTypeId(
        matchingHelpType ? String(matchingHelpType.id) : ""
      );

      const validEquipmentIds = (result.suggestedEquipmentTypeIds || [])
        .filter((id) =>
          equipmentTypes.some((equipment) => equipment.id === id)
        );

      setSelectedEquipmentIds([...new Set(validEquipmentIds)]);
      setHasAnalyzed(true);
    } catch (error) {
      setSubmitError(
        error instanceof Error
          ? error.message
          : "אירעה שגיאה בניתוח הבקשה באמצעות AI."
      );
    } finally {
      setIsAnalyzing(false);
    }
  };

  const handleEquipmentChange = (equipmentId: number) => {
    setSelectedEquipmentIds((previous) =>
      previous.includes(equipmentId)
        ? previous.filter((id) => id !== equipmentId)
        : [...previous, equipmentId]
    );
  };

  const handleGetLocation = () => {
    if (!navigator.geolocation) {
      setLocationStatus(
        "הדפדפן שלך אינו תומך בקבלת מיקום."
      );
      return;
    }

    setIsLoadingLocation(true);
    setLocationStatus("מנסה לקבל את המיקום שלך...");

    navigator.geolocation.getCurrentPosition(
      (position) => {
        setLatitude(position.coords.latitude);
        setLongitude(position.coords.longitude);
        setLocationStatus("המיקום שלך התקבל בהצלחה!");
        setIsLoadingLocation(false);
      },
      (error) => {
        setIsLoadingLocation(false);

        switch (error.code) {
          case error.PERMISSION_DENIED:
            setLocationStatus(
              "לא ניתנה הרשאה לגשת למיקום שלך."
            );
            break;
          case error.POSITION_UNAVAILABLE:
            setLocationStatus(
              "לא ניתן לאתר כרגע את המיקום שלך."
            );
            break;
          case error.TIMEOUT:
            setLocationStatus(
              "בקשת המיקום נמשכה זמן רב מדי. נסי שוב."
            );
            break;
          default:
            setLocationStatus(
              "אירעה שגיאה בקבלת המיקום."
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

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitError("");

    if (!hasAnalyzed || !analysis) {
      setSubmitError(
        "יש לבצע ניתוח AI לפני המעבר לשלב הבא."
      );
      return;
    }

    if (title.trim().length < 2) {
      setSubmitError(
        "יש להזין כותרת באורך של לפחות 2 תווים."
      );
      return;
    }

    if (description.trim().length < 5) {
      setSubmitError(
        "יש להזין תיאור באורך של לפחות 5 תווים."
      );
      return;
    }

    if (!helpTypeId) {
      setSubmitError("יש לבחור את סוג העזרה המבוקש.");
      return;
    }

    if (latitude === null || longitude === null) {
      setSubmitError(
        "יש לקבל את המיקום שלך לפני המשך."
      );
      return;
    }

    if (!address.trim()) {
      setSubmitError("יש להזין כתובת מדויקת.");
      return;
    }

    setIsSubmitting(true);

    navigate("/suggestions", {
      state: {
        analysis,
        draft: {
          title: title.trim(),
          description: description.trim(),
          helpTypeId,
          latitude,
          longitude,
          address: address.trim(),
          addressNotes: addressNotes.trim(),
          selectedImage,
          requiredEquipmentTypeIds: selectedEquipmentIds
        }
      }
    });
  };

  return (
    <div className="create-request-page" dir="rtl">
      <main className="create-request-container">
        <header className="create-request-header">
          <h1>בקשת עזרה חדשה</h1>
          <p>
            תארי את הבעיה וה-AI שלנו ינתח אותה
            ויציע את סוג העזרה והציוד המתאימים.
          </p>
        </header>

        <section className="create-request-card">
          {isLoadingData ? (
            <p>טוען נתונים...</p>
          ) : (
            <>
              {!hasAnalyzed && (
                <div className="create-request-form">
                  <h2>תיאור הבעיה</h2>

                  <div className="create-request-field">
                    <label htmlFor="initialDescription">
                      תארי מה קרה ומה הבעיה
                    </label>

                    <textarea
                      id="initialDescription"
                      value={description}
                      onChange={(e) =>
                        setDescription(e.target.value)
                      }
                      placeholder="לדוגמה: הרכב שלי אינו מניע ואני נמצאת בחניה..."
                      required
                      minLength={5}
                      maxLength={1000}
                    />
                  </div>

                  <div className="create-request-field">
                    <h3>העלאת תמונה (לא חובה)</h3>

                    <p>
                      אפשר לצרף תמונה כדי לעזור ל-AI
                      להבין טוב יותר את הבעיה.
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
                    onClick={handleAnalyze}
                    disabled={isAnalyzing}
                  >
                    {isAnalyzing
                      ? "ה-AI מנתח את הבקשה..."
                      : "נתח את הבקשה באמצעות AI"}
                  </button>
                </div>
              )}

              {hasAnalyzed && analysis && (
                <>
                  <div className="create-request-form">
                    <h2>תוצאות ניתוח ה-AI</h2>

                    <div className="solution-card">
                      <div className="solution-content">
                        <h3>{analysis.problemName}</h3>

                        <p>
                          <strong>קטגוריה:</strong>{" "}
                          {analysis.category}
                        </p>

                        <p>
                          <strong>רמת סיכון:</strong>{" "}
                          {analysis.riskLevel}
                        </p>

                        <p>
                          <strong>רמת ביטחון הניתוח:</strong>{" "}
                          {Math.round(
                            analysis.confidence * 100
                          )}%
                        </p>

                        <p>
                          <strong>המלצת המערכת:</strong>{" "}
                          {analysis.recommendation}
                        </p>

                        {analysis.safeActions?.length > 0 && (
                          <>
                            <h4>פעולות בטוחות אפשריות</h4>
                            <ul>
                              {analysis.safeActions.map(
                                (action, index) => (
                                  <li key={index}>{action}</li>
                                )
                              )}
                            </ul>
                          </>
                        )}
                      </div>
                    </div>

                    <p>
                      ה-AI מילא את הפרטים הבאים.
                      ניתן לערוך אותם לפני המשך.
                    </p>

                    <div className="create-request-field">
                      <label htmlFor="title">
                        כותרת הבעיה
                      </label>

                      <input
                        id="title"
                        type="text"
                        value={title}
                        onChange={(e) =>
                          setTitle(e.target.value)
                        }
                        placeholder="כותרת הבעיה"
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
                        required
                        minLength={5}
                        maxLength={1000}
                      />
                    </div>

                    <div className="create-request-field">
                      <label htmlFor="helpType">
                        סוג העזרה
                      </label>

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

                      {!helpTypeId && (
                        <p>
                          ה-AI לא זיהה סוג עזרה מתאים.
                          אנא בחרי אותו בעצמך.
                        </p>
                      )}
                    </div>

                    <div className="create-request-field">
                      <h3>ציוד נדרש</h3>

                      <p>
                        הציוד סומן לפי המלצת ה-AI.
                        ניתן לשנות את הבחירה.
                      </p>

                      {equipmentTypes.length === 0 ? (
                        <p>
                          לא הוגדרו סוגי ציוד במערכת.
                        </p>
                      ) : (
                        <div>
                          {equipmentTypes.map((equipment) => (
                            <label
                              key={equipment.id}
                              style={{
                                display: "block",
                                marginBottom: "8px"
                              }}
                            >
                              <input
                                type="checkbox"
                                checked={selectedEquipmentIds.includes(
                                  equipment.id
                                )}
                                onChange={() =>
                                  handleEquipmentChange(
                                    equipment.id
                                  )
                                }
                              />{" "}
                              {equipment.name}
                            </label>
                          ))}
                        </div>
                      )}
                    </div>

                    <button
                      type="button"
                      className="create-request-button"
                      onClick={() => {
                        setAnalysis(null);
                        setHasAnalyzed(false);
                        setTitle("");
                        setHelpTypeId("");
                        setSelectedEquipmentIds([]);
                      }}
                    >
                      חזרה לעריכת התיאור ולניתוח מחדש
                    </button>
                  </div>

                  <form
                    className="create-request-form"
                    onSubmit={handleSubmit}
                  >
                    <div className="create-request-location">
                      <h3>מיקום</h3>

                      <button
                        className="create-request-button"
                        type="button"
                        onClick={handleGetLocation}
                        disabled={isLoadingLocation}
                      >
                        {isLoadingLocation
                          ? "מקבל מיקום..."
                          : "קבלת המיקום הנוכחי שלי"}
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
                          onChange={(e) =>
                            setAddress(e.target.value)
                          }
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
                          onChange={(e) =>
                            setAddressNotes(e.target.value)
                          }
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
                        !helpTypeId ||
                        helpTypes.length === 0
                      }
                    >
                      {isSubmitting
                        ? "מעביר להצעות..."
                        : "המשך להצעות הפתרון"}
                    </button>
                  </form>
                </>
              )}
            </>
          )}
        </section>
      </main>
    </div>
  );
}

export default CreateRequestScreen;
