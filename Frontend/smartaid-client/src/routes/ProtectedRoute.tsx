import { type ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

interface ProtectedRouteProps {
  children: ReactNode;
  requireVolunteer?: boolean;
  requireAdmin?: boolean;
}

function ProtectedRoute({
  children,
  requireVolunteer = false,
  requireAdmin = false,
}: ProtectedRouteProps) {
  const { isAuthenticated, isVolunteer, isAdmin } = useAuth();
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

  if (requireAdmin && !isAdmin) {
    return <Navigate to="/" replace />;
  }

  if (requireVolunteer && !isVolunteer) {
    return <Navigate to="/requester" replace />;
  }

  return <>{children}</>;
}

export default ProtectedRoute;