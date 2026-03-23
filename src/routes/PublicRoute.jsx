import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

/**
 * A route guard for public pages like Login and Signup.
 * If the user is already authenticated, it redirects them to the dashboard.
 * Otherwise, it renders the requested public page.
 */
const PublicRoute = () => {
  const { isAuthenticated } = useAuthStore();

  if (isAuthenticated) {
    // If the user is already logged in, redirect to their dashboard
    return <Navigate to="/dashboard" replace />;
  }

  // If not authenticated, render the public page (e.g., Login, Signup)
  return <Outlet />;
};

export default PublicRoute;
