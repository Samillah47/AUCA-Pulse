import React from 'react';
import Avatar from '../common/Avatar';
import Badge from '../common/Badge';
import { useAuthStore } from '../../store/authStore';

const ProfileHeader = () => {
    const { user } = useAuthStore();
  return (
    <div className="flex flex-col items-center md:flex-row md:items-center gap-6 p-6 bg-white dark:bg-dark-800 rounded-lg shadow-md">
      <Avatar name={user?.name} size="xl" />
      <div className="text-center md:text-left">
        <h2 className="text-2xl font-bold">{user?.name}</h2>
        <p className="text-gray-500 dark:text-gray-400">{user?.email}</p>
        <div className="mt-2">
            <Badge variant="primary">{user?.role}</Badge>
        </div>
      </div>
    </div>
  );
};

export default ProfileHeader;
