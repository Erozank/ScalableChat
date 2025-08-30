// src/utils/parseJwt.js
// Utilidad para decodificar un JWT
export function parseJwt(token) {
  if (!token) return null;
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(function(c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        })
        .join('')
    );
    console.log("Decoded JWT payload:", jsonPayload);
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}
