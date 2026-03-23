import React from 'react';
import ProfileHeader from '../components/profile/ProfileHeader';
import ProfileDetails from '../components/profile/ProfileDetails';
import ChangePassword from '../components/profile/ChangePassword';

const Profile = () => {
  return (
    <div className="space-y-6">
      <ProfileHeader />
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
            <ProfileDetails />
        </div>
        <div className="space-y-6">
            <ChangePassword />
        </div>
      </div>
    </div>
  );
};

export default Profile;
