import { useEffect, useState } from "react";
import { clearTokens, getAccessToken, getRefreshToken } from "../api/tokenStore";

type TokenPayload = {
  nameid?: string;
  sub?: string;
  email?: string;
  unique_name?: string;
  name?: string;
  role?: string | string[];
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: string | string[];
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"?: string;
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"?: string;
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"?: string;
};

function decodePayload(token: string): TokenPayload | null {
  try {
    const payload = token.split(".")[1];
    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    return JSON.parse(window.atob(normalized)) as TokenPayload;
  } catch {
    return null;
  }
}

export function useAuth() {
  const [accessToken, setAccessToken] = useState(() => getAccessToken());
  const refreshToken = getRefreshToken();
  const payload = accessToken ? decodePayload(accessToken) : null;
  const roleClaim =
    payload?.role ??
    payload?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
  const roles = Array.isArray(roleClaim) ? roleClaim : roleClaim ? [roleClaim] : [];
  const userId =
    payload?.nameid ??
    payload?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ??
    payload?.sub;

  useEffect(() => {
    function onStorage() {
      setAccessToken(getAccessToken());
    }

    window.addEventListener("storage", onStorage);
    window.addEventListener("stockflow:auth-changed", onStorage);
    return () => {
      window.removeEventListener("storage", onStorage);
      window.removeEventListener("stockflow:auth-changed", onStorage);
    };
  }, []);

  function logout() {
    clearTokens();
    setAccessToken(null);
    window.dispatchEvent(new Event("stockflow:auth-changed"));
  }

  const email =
    payload?.email ??
    payload?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"];
  const displayName =
    payload?.name ??
    payload?.unique_name ??
    payload?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];

  return {
    accessToken,
    refreshToken,
    isAuthenticated: Boolean(accessToken),
    roles,
    email,
    displayName,
    userId: userId ? Number(userId) : undefined,
    logout,
  };
}
