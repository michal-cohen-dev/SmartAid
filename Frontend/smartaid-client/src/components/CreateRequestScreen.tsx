
import { useNavigate } from 'react-router-dom';
import { useState, useEffect } from "react";
import { apiRequest } from "../services/api";

function CreateRequestScreen() {
  const navigate = useNavigate();

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");

  const [latitude, setLatitude] = useState<number | null>(null);
  const [longitude, setLongitude] = useState<number | null>(null);
  const [locationStatus, setLocationStatus] = useState("");
  const [isLoadingLocation, setIsLoadingLocation] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState("");

  useEffect(() => {
    document.title = "בקשת עזרה חדשה | SmartAid";
  }, []);

  const handleGetLocation = () => {
    if (!navigator.geolocation) {
      setLocationStatus("הדפדפן שלך אינו תומך בקבלת מיקום.");
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
            setLocationStatus("לא ניתנה הרשאה לגשת למיקום שלך.");
            break;

          case error.POSITION_UNAVAILABLE:
            setLocationStatus("לא ניתן לאתר כרגע את המיקום שלך.");
            break;

          case error.TIMEOUT:
            setLocationStatus("בקשת המיקום נמשכה זמן רב מדי. נסי שוב.");
            break;

          default:
            setLocationStatus("אירעה שגיאה בקבלת המיקום.");
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
    setSubmitError("");

    if (latitude === null || longitude === null) {
      setSubmitError("יש לקבל את המיקום שלך לפני שליחת הבקשה.");
      return;
    }

    setIsSubmitting(true);

    try {
      const formData = new FormData();

      formData.append("Latitude", String(latitude));
      formData.append("Longitude", String(longitude));
      formData.append("Address", "");

      await apiRequest<{ message: string }>("/api/HelpRequests", {
        method: "POST",
        body: formData
      });

      navigate("/suggestions");
    } catch (error) {
      setSubmitError(
        error instanceof Error
          ? error.message
          : "אירעה שגיאה בשליחת בקשת העזרה."
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div dir="rtl">
      <h1>בקשת עזרה חדשה</h1>
      <p>תארי את הבעיה כדי שנוכל למצוא עבורך עזרה מתאימה</p>

      <form onSubmit={handleSubmit}>
        <div>
          <label>כותרת הבקשה</label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="לדוגמה: תקר בגלגל"
          />
        </div>

        <div>
          <label>תיאור הבעיה</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="תארי מה קרה ואיזו עזרה נדרשת"
          />
        </div>

        <div>
          <label>מיקום</label>

          <button
            type="button"
            onClick={handleGetLocation}
            disabled={isLoadingLocation}
          >
            {isLoadingLocation
              ? "מקבל מיקום..."
              : "קבלת המיקום הנוכחי שלי"}
          </button>

          {locationStatus && (
            <p role="status">{locationStatus}</p>
          )}
        </div>

        {submitError && (
          <p role="alert">{submitError}</p>
        )}

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "שולח בקשה..." : "שליחת בקשת עזרה"}
        </button>
      </form>
    </div>
  );
}

export default CreateRequestScreen;
