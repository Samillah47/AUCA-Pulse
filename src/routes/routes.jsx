import { useRoutes, Navigate } from 'react-router-dom';
import ProtectedRoute from './ProtectedRoute';
import PublicRoute from './PublicRoute';

// Layouts
import MainLayout from '../components/layout/MainLayout';

// Pages
import LandingPage from '../pages/LandingPage';
import Login from '../pages/Login';
import Signup from '../pages/Signup';
import ForgotPassword from '../pages/ForgotPassword';
import ResetPassword from '../pages/ResetPassword';
import Dashboard from '../pages/Dashboard';
import Rooms from '../pages/Rooms';
import RoomDetails from '../components/rooms/RoomDetails'; // Added import
import Lecturers from '../pages/Lecturers';
import LecturerProfile from '../components/lecturers/LecturerProfile'; // Added import
import LecturerSchedule from '../pages/LecturerSchedule';
import Profile from '../pages/Profile';
import Notifications from '../pages/Notifications';
import NotFound from '../pages/NotFound';
import ComponentTest from '../pages/ComponentTest'; // For testing

// Role-specific pages
import VerificationRequests from '../components/admin/VerificationRequests';
import UserManagement from '../components/admin/UserManagement';
import PasswordResetRequests from '../components/admin/PasswordResetRequests';
import TwoFactorAuth from '../pages/TwoFactorAuth';

export const AppRouter = () => {
  const routes = useRoutes([
    // --- Public Routes ---
    // Unauthenticated users are directed here.
    {
      element: <PublicRoute />,
      children: [
        { path: '/', element: <LandingPage /> },
        { path: '/login', element: <Login /> },
        { path: '/signup', element: <Signup /> },
        { path: '/forgot-password', element: <ForgotPassword /> },
        { path: '/reset-password', element: <ResetPassword /> },
        { path: '/verify-otp', element: <TwoFactorAuth /> },
      ],
    },

    // --- Protected Routes ---
    // Authenticated users are directed here.
    {
      element: <ProtectedRoute />, // The gatekeeper for all authenticated routes
      children: [
        {
          element: <MainLayout />, // The UI layout for all pages inside
          children: [
            { path: 'dashboard', element: <Dashboard /> },
            { path: 'rooms', element: <Rooms /> },
            { path: 'rooms/:roomId', element: <RoomDetails /> }, // Added route
            { path: 'lecturers', element: <Lecturers /> },
            { path: 'lecturers/:lecturerId', element: <LecturerProfile /> }, // Added route
            { path: 'schedule', element: <LecturerSchedule /> },
            { path: 'profile', element: <Profile /> },
            { path: 'notifications', element: <Notifications /> },
            {
              path: 'admin',
              children: [
                { path: 'verifications', element: <VerificationRequests /> },
                { path: 'users', element: <UserManagement /> },
                { path: 'password-resets', element: <PasswordResetRequests /> },
              ],
            },
            // Redirect authenticated users from the root path to their dashboard
            { path: '/', element: <Navigate to="/dashboard" replace /> },
          ],
        },
      ],
    },

    // --- Other Routes ---
    { path: '/test', element: <ComponentTest /> }, // Component showcase page
    { path: '*', element: <NotFound /> }, // 404 Not Found page
  ]);

  return routes;
};
