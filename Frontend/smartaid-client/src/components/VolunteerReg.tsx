
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { apiRequest } from "../services/api";
import "./VolunteerReg.css";

interface LookupItem {
  id: number;
  name: string;
}

function VolunteerReg() {
  const navigate = useNavigate();

  const [equipmentTypes, setEquipmentTypes] = useState<LookupItem[]>([]);
  const [helpTypes, setHelpTypes] = useState<LookupItem[]>([]);

  const [selectedEquipmentIds, setSelectedEquipmentIds] = useState<number[]>([]);
  const [selectedHelpTypeIds, setSelectedHelpTypeIds] = useState<number[]>([]);

  const [loadingData, setLoadingData] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    async function loadOptions() {
      try {
        setLoadingData(true);
        setError("");

        const [equipment, help] = await Promise.all([
          apiRequest<LookupItem[]>("/api/EquipmentTypes"),
          apiRequest<LookupItem[]>("/api/HelpTypes"),
        ]);

        setEquipmentTypes(equipment);
        setHelpTypes(help);
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : "לא ניתן לטעון את אפשרויות ההרשמה."
        );
      } finally {
        setLoadingData(false);
      }
    }

    loadOptions();
  }, []);

  
function toggleSelection(
  id: number,
  setSelectedIds: React.Dispatch<React.SetStateAction<number[]>>
) {
  setSelectedIds((previous) =>
    previous.includes(id)
      ? previous.filter((item) => item !== id)
      : [...previous, id]
  );
}


  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");

    if (selectedHelpTypeIds.length === 0) {
      setError("יש לבחור לפחות סוג עזרה אחד.");
      return;
    }

    try {
      setSubmitting(true);

      await apiRequest("/api/Volunteers", {
        method: "POST",
        body: JSON.stringify({
          equipmentTypeIds: selectedEquipmentIds,
          helpTypeIds: selectedHelpTypeIds,
        }),
      });

      setSuccess(true);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "אירעה שגיאה בהרשמה כמתנדב."
      );
    } finally {
      setSubmitting(false);
    }
  }

  function handleLoginAgain() {
    sessionStorage.removeItem("token");
    navigate("/login");
  }

  if (success) {
    return (
      <div className="volunteer-reg-page" dir="rtl">
        <div className="volunteer-reg-card volunteer-reg-success">
          <div className="volunteer-reg-success-icon">✓</div>
          <h1>נרשמת בהצלחה כמתנדב!</h1>
          <p>
            פרופיל המתנדב שלך נוצר בהצלחה.
            כדי לעדכן את הרשאות החשבון, יש להתחבר מחדש.
          </p>
          <button
            type="button"
            className="volunteer-reg-submit"
            onClick={handleLoginAgain}
          >
            התחברות מחדש
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="volunteer-reg-page" dir="rtl">
      <div className="volunteer-reg-card">
        <header className="volunteer-reg-header">
          <h1>הצטרפות לקהילת המתנדבים</h1>
          <p>
            ספר לנו באילו תחומים תוכל לעזור ואיזה ציוד עומד לרשותך.
          </p>
        </header>

        {loadingData ? (
          <p className="volunteer-reg-info">טוען אפשרויות הרשמה...</p>
        ) : (
          <form onSubmit={handleSubmit}>
            <section className="volunteer-reg-section">
              <h2>באילו תחומים אוכל לעזור?</h2>
              <p>יש לבחור לפחות תחום אחד.</p>

              <div className="volunteer-reg-options">
                {helpTypes.map((item) => (
                  <label
                    key={item.id}
                    className="volunteer-reg-option"
                  >
                    <input
                      type="checkbox"
                      checked={selectedHelpTypeIds.includes(item.id)}
                     onChange={() =>
  toggleSelection(item.id, setSelectedHelpTypeIds)
}
                    />
                    <span>{item.name}</span>
                  </label>
                ))}
              </div>

              {helpTypes.length === 0 && (
                <p className="volunteer-reg-info">
                  לא נמצאו תחומי עזרה במערכת.
                </p>
              )}
            </section>

            <section className="volunteer-reg-section">
              <h2>איזה ציוד עומד לרשותי?</h2>
              <p>אפשר לבחור כמה אפשרויות, או להשאיר ללא בחירה.</p>

              <div className="volunteer-reg-options">
                {equipmentTypes.map((item) => (
                  <label
                    key={item.id}
                    className="volunteer-reg-option"
                  >
                    <input
                      type="checkbox"
                      checked={selectedEquipmentIds.includes(item.id)}
                     onChange={() =>
  toggleSelection(item.id, setSelectedEquipmentIds)
}
                    />
                    <span>{item.name}</span>
                  </label>
                ))}
              </div>

              {equipmentTypes.length === 0 && (
                <p className="volunteer-reg-info">
                  לא הוגדרו סוגי ציוד במערכת.
                </p>
              )}
            </section>

            {error && (
              <p className="volunteer-reg-error" role="alert">
                {error}
              </p>
            )}

            <button
              type="submit"
              className="volunteer-reg-submit"
              disabled={submitting || helpTypes.length === 0}
            >
              {submitting ? "שולח הרשמה..." : "השלמת הרשמה כמתנדב"}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}

export default VolunteerReg;
