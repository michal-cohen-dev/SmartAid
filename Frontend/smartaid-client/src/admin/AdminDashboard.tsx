
import { useEffect, useMemo, useState } from "react";
import {
  MapContainer,
  TileLayer,
  CircleMarker,
  Popup,
  useMap,
} from "react-leaflet";
import "leaflet/dist/leaflet.css";
import { apiRequest } from "../services/api";
import "./AdminDashboard.css";

interface AdminUser {
  id: number;
  fullName: string;
}

interface AdminVolunteer {
  id: number;
  fullName: string;
  latitude: number | null;
  longitude: number | null;
  isActive: boolean;
}

interface AdminHelpRequest {
  id: number;
  title: string;
  address: string;
  latitude: number;
  longitude: number;
  status: number;
  helpTypeName?: string | null;
}

interface MapPoint {
  id: string;
  name: string;
  description: string;
  latitude: number;
  longitude: number;
  type: "request" | "volunteer";
}

function MapBounds({ points }: { points: MapPoint[] }) {
  const map = useMap();

  useEffect(() => {
    if (points.length === 0) return;

    const bounds = points.map(
      (point) => [point.latitude, point.longitude] as [number, number]
    );

    map.fitBounds(bounds, { padding: [40, 40], maxZoom: 13 });
  }, [map, points]);

  return null;
}

function getRequestStatus(status: number): string {
  switch (status) {
    case 0:
      return "פתוחה";
    case 1:
      return "שויכה למתנדב";
    case 2:
      return "הושלמה";
    case 3:
      return "בוטלה";
    case 4:
      return "בטיפול";
    default:
      return "לא ידוע";
  }
}

export default function AdminDashboard() {
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [volunteers, setVolunteers] = useState<AdminVolunteer[]>([]);
  const [requests, setRequests] = useState<AdminHelpRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let isMounted = true;

    async function loadDashboard() {
      setLoading(true);
      setError("");

      try {
        const [usersData, volunteersData, requestsData] =
          await Promise.all([
            apiRequest<AdminUser[]>("/api/Users/admin"),
            apiRequest<AdminVolunteer[]>("/api/Volunteers"),
            apiRequest<AdminHelpRequest[]>("/api/HelpRequests"),
          ]);

        if (!isMounted) return;

        setUsers(usersData);
        setVolunteers(volunteersData);
        setRequests(requestsData);
      } catch (err) {
        if (!isMounted) return;

        setError(
          err instanceof Error
            ? err.message
            : "טעינת נתוני לוח הבקרה נכשלה."
        );
      } finally {
        if (isMounted) setLoading(false);
      }
    }

    loadDashboard();

    return () => {
      isMounted = false;
    };
  }, []);

  const activeVolunteers = volunteers.filter(
    (volunteer) => volunteer.isActive
  ).length;

  const openRequests = requests.filter(
    (request) => request.status === 0
  ).length;

  const points = useMemo<MapPoint[]>(() => {
    const requestPoints: MapPoint[] = requests
      .filter(
        (request) =>
          Number.isFinite(request.latitude) &&
          Number.isFinite(request.longitude) &&
          request.latitude >= -90 &&
          request.latitude <= 90 &&
          request.longitude >= -180 &&
          request.longitude <= 180
      )
      .map((request) => ({
        id: `request-${request.id}`,
        name: request.title,
        description: [
          request.address,
          `סטטוס: ${getRequestStatus(request.status)}`,
          request.helpTypeName
            ? `סוג עזרה: ${request.helpTypeName}`
            : "",
        ]
          .filter(Boolean)
          .join(" | "),
        latitude: request.latitude,
        longitude: request.longitude,
        type: "request",
      }));

    const volunteerPoints: MapPoint[] = volunteers
      .filter(
        (volunteer) =>
          volunteer.latitude !== null &&
          volunteer.longitude !== null &&
          Number.isFinite(volunteer.latitude) &&
          Number.isFinite(volunteer.longitude) &&
          volunteer.latitude >= -90 &&
          volunteer.latitude <= 90 &&
          volunteer.longitude >= -180 &&
          volunteer.longitude <= 180
      )
      .map((volunteer) => ({
        id: `volunteer-${volunteer.id}`,
        name: volunteer.fullName,
        description: volunteer.isActive ? "מתנדב פעיל" : "מתנדב לא פעיל",
        latitude: volunteer.latitude!,
        longitude: volunteer.longitude!,
        type: "volunteer",
      }));

    return [...requestPoints, ...volunteerPoints];
  }, [requests, volunteers]);

  const mapCenter: [number, number] =
    points.length > 0
      ? [points[0].latitude, points[0].longitude]
      : [31.5, 34.8];

  if (loading) {
    return (
      <div className="admin-dashboard" dir="rtl">
        <h2>לוח הבקרה</h2>
        <p>טוען נתונים...</p>
      </div>
    );
  }

  return (
    <div className="admin-dashboard" dir="rtl">
      <div className="dashboard-heading">
        <div>
          <h2>לוח הבקרה</h2>
          <p>תמונת מצב עדכנית של מערכת SmartAid</p>
        </div>
      </div>

      {error && (
        <div className="dashboard-error" role="alert">
          {error}
        </div>
      )}

      {!error && (
        <>
          <div className="dashboard-stats">
            <div className="dashboard-stat-card">
              <span className="dashboard-stat-icon">👥</span>
              <div>
                <p>משתמשים במערכת</p>
                <strong>{users.length}</strong>
              </div>
            </div>

            <div className="dashboard-stat-card">
              <span className="dashboard-stat-icon">🤝</span>
              <div>
                <p>מתנדבים פעילים</p>
                <strong>{activeVolunteers}</strong>
              </div>
            </div>

            <div className="dashboard-stat-card">
              <span className="dashboard-stat-icon">📋</span>
              <div>
                <p>סך כל הקריאות</p>
                <strong>{requests.length}</strong>
              </div>
            </div>

            <div className="dashboard-stat-card">
              <span className="dashboard-stat-icon">⏳</span>
              <div>
                <p>קריאות פתוחות</p>
                <strong>{openRequests}</strong>
              </div>
            </div>
          </div>

          <section className="dashboard-map-section">
            <div className="dashboard-section-heading">
              <div>
                <h3>מפת המערכת</h3>
                <p>מיקומי קריאות ומתנדבים</p>
              </div>

              <div className="dashboard-map-legend">
                <span>
                  <i className="legend-request" />
                  קריאות
                </span>
                <span>
                  <i className="legend-volunteer" />
                  מתנדבים
                </span>
              </div>
            </div>

            {points.length === 0 ? (
              <div className="dashboard-empty-map">
                אין כרגע נתוני מיקום להצגה במפה.
              </div>
            ) : (
              <MapContainer
                center={mapCenter}
                zoom={8}
                scrollWheelZoom
                className="dashboard-map"
              >
                <TileLayer
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />

                <MapBounds points={points} />

                {points.map((point) => (
                  <CircleMarker
                    key={point.id}
                    center={[point.latitude, point.longitude]}
                    radius={9}
                    pathOptions={{
                      color:
                        point.type === "request" ? "#dc6b35" : "#27865a",
                      fillColor:
                        point.type === "request" ? "#f28b50" : "#43b581",
                      fillOpacity: 0.85,
                    }}
                  >
                    <Popup>
                      <strong>{point.name}</strong>
                      <br />
                      {point.description}
                    </Popup>
                  </CircleMarker>
                ))}
              </MapContainer>
            )}
          </section>

          <p className="dashboard-map-note">
            המפה מציגה רק קריאות ומתנדבים שעבורם קיימים נתוני מיקום תקינים.
          </p>
        </>
      )}
    </div>
  );
}
