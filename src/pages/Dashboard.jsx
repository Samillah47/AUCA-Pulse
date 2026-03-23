import React from 'react';
import { useAuthStore } from '../store/authStore';
import { ROLES } from '../utils/constants';

import AdminDashboard from '../components/dashboard/AdminDashboard';
import LecturerDashboard from '../components/dashboard/LecturerDashboard';
import StaffDashboard from '../components/dashboard/StaffDashboard';
import StudentDashboard from '../components/dashboard/StudentDashboard';
import LoadingSpinner from '../components/common/LoadingSpinner';

const Dashboard = () => {
  const { user } = useAuthStore();

  if (!user) {
    return (
      <div className="flex items-center justify-center h-full">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  const renderDashboard = () => {
    switch (user.role) {
      case ROLES.ADMIN:
        return <AdminDashboard />;
      case ROLES.LECTURER:
        return <LecturerDashboard />;
      case ROLES.STAFF:
        return <StaffDashboard />;
      case ROLES.STUDENT:
        return <StudentDashboard />;
      default:
        return (
          <div className="text-center">
            <h2 className="text-2xl font-bold">Welcome!</h2>
            <p className="text-gray-500">Your dashboard is not yet configured.</p>
          </div>
        );
    }
  };

  return (
    <div>
      {renderDashboard()}
    </div>
  );
};

export default Dashboard;
