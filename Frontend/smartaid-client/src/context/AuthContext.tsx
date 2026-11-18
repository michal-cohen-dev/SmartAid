
import {
  createContext,
  useContext,
  useState,
  type ReactNode,
} from "react";

interface AuthContextType {
  token: string | null;
  fullName: string | null;
  isAuthenticated: boolean;
  isVolunteer: boolean;
  login: (token: string) => void;
  logout: () => void;
  isAdmin: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(
  undefined
);

function decodeToken(token: string | null): any {
  if (!token) return null;

  try {
    const payload = token.split(".")[1];
    if (!payload) return null;

    const base64 = payload
      .replace(/-/g, "+")
      .replace(/_/g, "/")
      .padEnd(Math.ceil(payload.length / 4) * 4, "=");

    const binary = atob(base64);

    const bytes = Uint8Array.from(
      binary,
      (character) => character.charCodeAt(0)
    );

    return JSON.parse(
      new TextDecoder().decode(bytes)
    );
  } catch {
    return null;
  }
}

function getVolunteerStatus(token: string | null): boolean {
  const decoded = decodeToken(token);

  return (
    decoded?.isVolunteer === true ||
    String(decoded?.isVolunteer).toLowerCase() === "true"
  );
}

function getUserRole(token: string | null): string | null {
  const decoded = decodeToken(token);

  return (
    decoded?.role ??
    decoded?.[
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ] ??
    null
  );
}

function getFullName(token: string | null): string | null {
  const decoded = decodeToken(token);

  return decoded?.fullName || null;
}

export function AuthProvider({
  children,
}: {
  children: ReactNode;
}) {
  const [token, setToken] = useState<string | null>(
    () => sessionStorage.getItem("token")
  );

  function login(newToken: string) {
    sessionStorage.setItem("token", newToken);
    setToken(newToken);
  }

  function logout() {
    sessionStorage.removeItem("token");
    setToken(null);
  }

  const value: AuthContextType = {
    token,
    fullName: getFullName(token),
    isAuthenticated: !!token,
    isVolunteer: getVolunteerStatus(token),
    login,
    logout,
    isAdmin: getUserRole(token) === "Admin",
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error(
      "useAuth must be used inside AuthProvider"
    );
  }

  return context;
}
