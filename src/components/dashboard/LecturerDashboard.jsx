import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { DoorOpen, Calendar, Edit, MapPin, Clock, Building, LogOut, Bell, Users, BookOpen } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { toast } from 'react-hot-toast';

import Card from '../common/Card';
import Button from '../common/Button';
import Modal from '../common/Modal';
import Select from '../common/Select';
import Input from '../common/Input';
import { useAuthStore } from '../../store/authStore';
import { lecturerService, roomService, notificationService } from '../../services';
import { LECTURER_STATUS } from '../../utils/constants';

const UpdateStatusModal = ({ isOpen, onClose, currentStatus }) => {
  const { register, handleSubmit, formState: { isSubmitting } } = useForm({
    defaultValues: {
      status: currentStatus?.status || LECTURER_STATUS.AVAILABLE,
      message: currentStatus?.message || '',
    }
  });
  const { setUser, user } = useAuthStore();

  const onSubmit = async (data) => {
    try {
      const lecturerId = user?.userId || user?.id;
      const updatedStatus = await lecturerService.updateStatus(lecturerId, data);
      // Update user in store to reflect new status
      setUser({ ...user, status: updatedStatus });
      toast.success('Status updated successfully!');
      onClose();
    } catch (error) {
      toast.error(error.message || 'Failed to update status.');
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Update Your Status">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Select
          label="Current Status"
          id="status"
          {...register('status')}
        >
          {Object.values(LECTURER_STATUS).map(status => (
            <option key={status} value={status}>{status}</option>
          ))}
        </Select>

        <Input
          label="Custom Message (Optional)"
          id="message"
          placeholder="e.g., In a meeting until 3 PM"
          {...register('message')}
        />

        <Modal.Footer
          onCancel={onClose}
          onConfirm={handleSubmit(onSubmit)}
          confirmText="Update Status"
          loading={isSubmitting}
        />
      </form>
    </Modal>
  );
};

const getStatusBadgeClass = (status) => {
  const statusMap = {
    [LECTURER_STATUS.AVAILABLE]: 'badge-available',
    [LECTURER_STATUS.TEACHING]: 'badge-teaching',
    [LECTURER_STATUS.AWAY]: 'badge-away',
    [LECTURER_STATUS.OCCUPIED]: 'badge-occupied',
  };
  return statusMap[status] || 'badge-available';
};

const getStatusText = (status) => {
  return status || LECTURER_STATUS.AVAILABLE;
};

const BookedRoomCard = ({ status, onRelease, onExtend, isReleasing, isExtending }) => {
  // Show card if *either* office (name) OR roomNumber exists
  if (!status?.office && !status?.roomNumber) return null;

  const displayName = status.office || `Room ${status.roomNumber}`;

  return (
    <div className="card-premium border-l-4 border-green-500">
      <div className="p-6">
        <div className="flex justify-between items-start mb-4">
          <div className="flex items-center gap-3">
            <div className="icon-container-lg bg-gradient-to-br from-green-500 to-green-600">
              <DoorOpen className="w-8 h-8 text-white" />
            </div>
            <div>
              <h3 className="text-lg font-bold text-gray-900 dark:text-gray-100">Current Room</h3>
              <span className="badge badge-available">Active Session</span>
            </div>
          </div>
        </div>

        <div className="space-y-3 mb-6">
          <div className="flex items-center gap-2">
            <span className="text-2xl font-bold text-gray-900 dark:text-white">{displayName}</span>
            <span className="text-sm px-2 py-1 bg-gray-100 dark:bg-dark-700 rounded text-gray-600 dark:text-gray-300">
              #{status.roomNumber || 'N/A'}
            </span>
          </div>
          
          <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400 text-sm">
            <Building className="w-4 h-4" />
            <span>{status.building || 'Unknown Building'}, Floor {status.floor || 'G'}</span>
          </div>

          <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400 text-sm">
            <Clock className="w-4 h-4" />
            <span>Occupied until {status.occupiedUntil ? new Date(status.occupiedUntil).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : 'Unknown'}</span>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <button 
            onClick={onExtend}
            disabled={isExtending || isReleasing}
            className="btn-primary-modern flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <Clock className="w-4 h-4" />
            {isExtending ? 'Extending...' : 'Extend (+30m)'}
          </button>
          
          <button 
            onClick={onRelease} 
            disabled={isReleasing || isExtending}
            className="btn-danger-modern flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <LogOut className="w-4 h-4" />
            {isReleasing ? 'Releasing...' : 'Release'}
          </button>
        </div>
      </div>
    </div>
  );
};

const LecturerDashboard = () => {
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const queryClient = useQueryClient();
  const [isModalOpen, setIsModalOpen] = useState(false);

  const lecturerId = user?.id || user?.userId;

  const { data: statusData } = useQuery({
    queryKey: ['lecturerStatus', lecturerId],
    queryFn: () => lecturerService.getStatus(lecturerId),
    enabled: !!lecturerId,
    refetchInterval: 30000, // Refresh every 30s
  });

  // Fetch unread notification count
  const { data: unreadCount = 0 } = useQuery({
    queryKey: ['unreadNotifications', lecturerId],
    queryFn: () => notificationService.getUnreadCount(lecturerId),
    enabled: !!lecturerId,
    refetchInterval: 30000, // Refresh every 30 seconds
  });

  // Room release mutation
  const releaseMutation = useMutation({
    mutationFn: async (roomId) => {
      return await roomService.releaseRoom(roomId);
    },
    onSuccess: () => {
      toast.success('Room released successfully!');
      // Refetch lecturer status to update UI
      queryClient.invalidateQueries(['lecturerStatus', lecturerId]);
    },
    onError: (error) => {
      toast.error(error.message || 'Failed to release room');
    }
  });

  // Room extend mutation
  const extendMutation = useMutation({
    mutationFn: async ({ roomId, duration }) => {
      return await roomService.extendRoom(roomId, { duration });
    },
    onSuccess: () => {
      toast.success('Booking extended by 30 minutes!');
      // Refetch lecturer status to update UI
      queryClient.invalidateQueries(['lecturerStatus', lecturerId]);
    },
    onError: (error) => {
      toast.error(error.message || 'Failed to extend booking');
    }
  });

  const quickActions = [
    {
      icon: <Calendar className="w-8 h-8 text-primary-600" />,
      title: 'My Schedule',
      description: 'View your teaching schedule for the semester.',
      action: () => navigate('/schedule'),
      buttonText: 'View Schedule',
      variant: 'primary',
      gradient: 'from-primary-500 to-primary-600'
    },
    {
      icon: <DoorOpen className="w-8 h-8 text-secondary-600" />,
      title: 'Room Availability',
      description: 'Check if a room is available for consultation or meeting.',
      action: () => navigate('/rooms'),
      buttonText: 'Occupy Room',
      variant: 'secondary',
      gradient: 'from-secondary-500 to-secondary-600'
    },
    {
      icon: <Users className="w-8 h-8 text-accent-600" />,
      title: 'My Students',
      description: 'View student information and office hour requests.',
      action: () => navigate('/students'),
      buttonText: 'View Students',
      variant: 'secondary',
      gradient: 'from-accent-500 to-accent-600'
    }
  ];



  return (
    <>
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="space-y-8"
      >
        {/* Welcome Header */}
        <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
          <div>
            <h1 className="text-h1 bg-gradient-to-r from-primary-600 to-secondary-600 bg-clip-text text-transparent">
              Welcome, {user?.name}! 👨‍🏫
            </h1>
            <p className="text-body-lg text-gray-600 dark:text-gray-300 mt-2">
              Manage your status and schedule here
            </p>
          </div>
          <button onClick={() => setIsModalOpen(true)} className="btn-primary-modern flex items-center gap-2">
            <Edit className="w-4 h-4" />
            Update Status
          </button>
        </div>

        {/* Current Status Card */}
        <div className="stat-card bg-gradient-to-br from-primary-50 to-secondary-50 dark:from-primary-900/10 dark:to-secondary-900/10 border-primary-200 dark:border-primary-800">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <div className="icon-container-lg">
                <BookOpen className="w-8 h-8 text-primary-600" />
              </div>
              <div>
                <div className="stat-label">Current Status</div>
                <div className="flex items-center gap-3 mt-1">
                  <span className={`badge ${getStatusBadgeClass(user?.status?.status)}`}>
                    {getStatusText(user?.status?.status)}
                  </span>
                  {user?.status?.message && (
                    <span className="text-sm text-gray-600 dark:text-gray-400">
                      "{user.status.message}"
                    </span>
                  )}
                </div>
              </div>
            </div>
            <button onClick={() => navigate('/notifications')} className="btn-secondary-modern flex items-center gap-2">
              <Bell className="w-4 h-4" />
              Notifications
              {unreadCount > 0 && (
                <span className="badge badge-pending ml-2">{unreadCount}</span>
              )}
            </button>
          </div>
        </div>

        {/* Booked Room + Quick Actions */}
        <div>
          <div className="section-header">
            <h2 className="section-title">
              <DoorOpen className="w-6 h-6 text-primary-600" />
              Quick Actions
            </h2>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {/* Booked Room Card - component handles its own visibility */}
            <BookedRoomCard 
              status={statusData} 
              onRelease={() => {
                if (statusData?.roomId) {
                  releaseMutation.mutate(statusData.roomId);
                }
              }}
              onExtend={() => {
                if (statusData?.roomId) {
                  extendMutation.mutate({ roomId: statusData.roomId, duration: 30 });
                }
              }}
              isReleasing={releaseMutation.isPending}
              isExtending={extendMutation.isPending}
            />

            {quickActions.map((action, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5, delay: index * 0.1 }}
              >
                <div className="card-interactive group">
                  <div className="p-6 flex flex-col items-center text-center">
                    {/* Icon with gradient background */}
                    <div className={`icon-container-lg mb-4 bg-gradient-to-br ${action.gradient} bg-opacity-10 group-hover:scale-110 transition-transform duration-300`}>
                      {action.icon}
                    </div>

                    <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                      {action.title}
                    </h3>
                    
                    <p className="text-sm text-gray-600 dark:text-gray-300 mb-6 flex-grow">
                      {action.description}
                    </p>

                    <button
                      onClick={action.action}
                      className={`btn-${action.variant}-modern w-full`}
                    >
                      {action.buttonText}
                    </button>
                  </div>
                </div>
              </motion.div>
            ))}
          </div>
        </div>

        {/* Teaching Tip */}
        <div className="card-premium p-6 bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-blue-900/10 dark:to-indigo-900/10 border-blue-200 dark:border-blue-800">
          <div className="flex items-start gap-4">
            <div className="icon-container flex-shrink-0">
              <svg className="w-6 h-6 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="flex-grow">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                💡 Teaching Tip
              </h3>
              <p className="text-gray-700 dark:text-gray-300">
                Remember to update your status when you're in a meeting or unavailable. This helps students know when they can reach you for consultations.
              </p>
            </div>
          </div>
        </div>
      </motion.div>

      <UpdateStatusModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        currentStatus={user?.status}
      />
    </>
  );
};

export default LecturerDashboard;
