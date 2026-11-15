import { type ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

interface ProtectedRouteProps {
  children: ReactNode;
  requireVolunteer?: boolean;
}

function ProtectedRoute({
  children,
  requireVolunteer = false,
}: ProtectedRouteProps) {
  const { isAuthenticated, isVolunteer } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return (
      <Navigate
        to="/login"
        replace
        state={{ from: location }}
      />
    );
  }

  if (requireVolunteer && !isVolunteer) {
    return <Navigate to="/requester" replace />;
  }

  return <>{children}</>;
}

export default ProtectedRoute;