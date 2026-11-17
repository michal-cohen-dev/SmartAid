
import {
  createContext,
  useContext,
  useState,
  type ReactNode,
} from "react";

interface AuthContextType {
  token: string | null;
  isAuthenticated: boolean;
  isVolunteer: boolean;
  login: (token: string) => void;
  logout: () => void;
  isAdmin: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(
  undefined
);

function getVolunteerStatus(token: string | null): boolean {
  if (!token) return false;

  try {
    const payload = token.split(".")[1];
    if (!payload) return false;

    const base64 = payload
      .replace(/-/g, "+")
      .replace(/_/g, "/")
      .padEnd(Math.ceil(payload.length / 4) * 4, "=");

    const binary = atob(base64);
    const bytes = Uint8Array.from(
      binary,
      (character) => character.charCodeAt(0)
    );

    const decoded = JSON.parse(
      new TextDecoder().decode(bytes)
    );

    return (
      decoded.isVolunteer === true ||
      String(decoded.isVolunteer).toLowerCase() === "true"
    );
  } catch {
    return false;
  }
}

function getUserRole(token: string | null): string | null {
  if (!token) return null;

  try {
    const payload = token.split(".")[1];
    const base64 = payload
      .replace(/-/g, "+")
      .replace(/_/g, "/")
      .padEnd(Math.ceil(payload.length / 4) * 4, "=");

    const binary = atob(base64);
    const bytes = Uint8Array.from(
      binary,
      (character) => character.charCodeAt(0)
    );

    const decoded = JSON.parse(
      new TextDecoder().decode(bytes)
    );

    return decoded.role ??
      decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
      null;
  } catch {
    return null;
  }
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
