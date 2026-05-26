import axios from "axios";

/**
 * Resolves the axios base URL at module-load time.
 *
 * Priority order:
 *  1. Electron (packaged)  – preload exposes `window.electronAPI.apiPort`
 *  2. Electron (dev)       – preload still sets apiPort = 5000, dev backend
 *                            should be running on that port when testing Electron
 *  3. Vite env variable    – set VITE_API_URL in .env for custom deployments
 *  4. Hardcoded default    – `http://localhost:5213/api` for regular `vite dev`
 */
function getBaseURL(): string {
  const w = typeof window !== "undefined" ? (window as any) : null;
  if (w?.electronAPI?.apiPort) {
    return `http://localhost:${w.electronAPI.apiPort}/api`;
  }
  return (import.meta.env.VITE_API_URL as string | undefined) ?? "http://localhost:5213/api";
}

const api = axios.create({
  baseURL: getBaseURL(),
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
