import axios from "axios";

// In Docker, VITE_API_BASE_URL is empty string - nginx proxies /api/* to backend
// In development, it's http://localhost:7001
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:7001";

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Request interceptor for logging
apiClient.interceptors.request.use(
  (config) => {
    console.log(`[API] ${config.method?.toUpperCase()} ${config.url}`);
    return config;
  },
  (error) => {
    console.error("[API] Request error:", error);
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    console.error("[API] Response error:", error.response?.data || error.message);
    return Promise.reject(error);
  }
);

