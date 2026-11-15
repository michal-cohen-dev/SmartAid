
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { apiRequest } from "../services/api";
import "./MyRequestsScreen.css";

interface HelpRequest {
  id: number;
  title: string;
  description: string;
  createdAt: string;
  status: number;
  address?: string;
}

function VolunteerCompletedRequestsScreen() {
  const [requests, setRequests] = useState<HelpRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    async function loadRequests() {
      try {
        const data = await apiRequest("/api/HelpRequests/volunteer/my");
        const completed = (Array.isArray(data) ? data : [])
          .filter((request: HelpRequest) => request.status === 2);

        setRequests(completed);
      } catch (error) {
        console.error("Error loading completed requests:", error);
      } finally {
        setLoading(false);
      }
    }

    loadRequests();
  }, []);

  if (loading) {
    return <div className="my-requests-container">טוען קריאות...</div>;
  }

  return (
    <div className="my-requests-container">
      <h1>קריאות שטופלו כבר</h1>

      {requests.length === 0 ? (
        <p>עדיין אין לך קריאות שהושלמו.</p>
      ) : (
        <div className="requests-list">
          {requests.map((request) => (
            <div className="request-card" key={request.id}>
              <h3>{request.title}</h3>
              <p>{request.description}</p>

              {request.address && <p>כתובת: {request.address}</p>}

              <p>
                תאריך יצירה:{" "}
                {new Date(request.createdAt).toLocaleDateString("he-IL")}
              </p>

              <span className="status-badge">הושלמה</span>

              <button
                onClick={() => navigate(`/request-details/${request.id}`)}
              >
                צפייה בפרטי הקריאה
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default VolunteerCompletedRequestsScreen;
