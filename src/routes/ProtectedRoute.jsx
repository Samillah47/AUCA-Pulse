import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

/**
 * A route guard that checks for user authentication.
 * If the user is authenticated, it renders an <Outlet /> to display the nested child routes.
 * Otherwise, it redirects the user to the login page.
 * This is the idiomatic way to handle protected routes in React Router v6.
 */
const ProtectedRoute = () => {
  const { isAuthenticated } = useAuthStore.getState(); // Use getState for components outside of React's render cycle if needed, but direct use is fine here.

  if (!isAuthenticated) {
    // If not authenticated, redirect to the login page, preserving the intended location
    return <Navigate to="/login" replace />;
  }

  // If authenticated, render the child routes
  return <Outlet />;
};

export default ProtectedRoute;
