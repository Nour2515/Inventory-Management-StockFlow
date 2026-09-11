const accessTokenKey = "stockflow.accessToken";
const refreshTokenKey = "stockflow.refreshToken";

export function getAccessToken() {
  return localStorage.getItem(accessTokenKey);
}

export function getRefreshToken() {
  return localStorage.getItem(refreshTokenKey);
}

export function saveTokens(accessToken: string, refreshToken: string) {
  localStorage.setItem(accessTokenKey, accessToken);
  localStorage.setItem(refreshTokenKey, refreshToken);
  window.dispatchEvent(new Event("stockflow:auth-changed"));
}

export function clearTokens() {
  localStorage.removeItem(accessTokenKey);
  localStorage.removeItem(refreshTokenKey);
  window.dispatchEvent(new Event("stockflow:auth-changed"));
}
