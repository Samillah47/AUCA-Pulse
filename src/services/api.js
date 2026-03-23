import axios from 'axios';
import { useAuthStore } from '../store/authStore';

// Base URL for the API
const API_URL = 'http://localhost:8080/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor
// This runs before each request is sent
api.interceptors.request.use(
  (config) => {
    // Get the token from the Zustand store
    const token = useAuthStore.getState().token;

    if (token) {
      // If the token exists, add it to the Authorization header
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    // Handle request error
    return Promise.reject(error);
  }
);

// Response Interceptor
// This runs after a response is received
api.interceptors.response.use(
  (response) => {
    // Any status code that lie within the range of 2xx cause this function to trigger
    return response;
  },
  (error) => {
    // Any status codes that falls outside the range of 2xx cause this function to trigger
    if (error.response && error.response.status === 401) {
      // If the error is 401 (Unauthorized), it means the token is invalid or expired
      // Log the user out by clearing the token and user info from the store
      useAuthStore.getState().logout();
      // Optionally, redirect to the login page
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
