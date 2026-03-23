import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-hot-toast';
import { CalendarDays, Plus, Trash2, Edit, MapPin, BookOpen } from 'lucide-react';
import { motion } from 'framer-motion';

import Card from '../components/common/Card';
import Button from '../components/common/Button';
import LoadingSpinner from '../components/common/LoadingSpinner';
import EmptyState from '../components/common/EmptyState';
import { useAuthStore } from '../store/authStore';
import lecturerService from '../services/lecturerService';
import ScheduleModal from '../components/lecturers/ScheduleModal';

// Helper to format time for display/input
const formatTime = (timeString) => {
  if (!timeString) return '';
  // Assuming timeString is HH:mm:ss, return HH:mm
  return timeString.substring(0, 5);
};

// Represents a single schedule entry
const ScheduleEntry = ({ entry, onEdit, onDelete }) => (
  <div className="flex flex-col md:flex-row justify-between items-start md:items-center p-4 border-b last:border-b-0 dark:border-dark-700 hover:bg-gray-50 dark:hover:bg-dark-800 transition-colors">
    <div className="flex-1">
        <div className="flex items-center gap-2 mb-1">
            <span className="font-bold text-gray-800 dark:text-gray-200 w-24">{entry.dayOfWeek}</span>
             <span className="text-gray-600 dark:text-gray-400 font-mono text-sm bg-gray-100 dark:bg-dark-900 px-2 py-0.5 rounded">
                {formatTime(entry.startTime)} - {formatTime(entry.endTime)}
            </span>
        </div>
        <div className="flex items-center gap-4 ml-0 md:ml-24 mt-2 md:mt-0">
             <div className="flex items-center gap-1.5 text-blue-600 dark:text-blue-400">
                <BookOpen className="w-4 h-4" />
                <span className="font-medium">{entry.courseName || 'Untitled Class'}</span>
            </div>
            {entry.roomName && (
                <div className="flex items-center gap-1.5 text-gray-500 dark:text-gray-400 text-sm">
                    <MapPin className="w-4 h-4" />
                    <span>{entry.roomName}</span>
                </div>
            )}
        </div>
    </div>
    
    <div className="flex gap-2 mt-4 md:mt-0 ml-0 md:ml-4">
      <Button variant="ghost" size="sm" onClick={() => onEdit(entry)}>
        <Edit className="w-4 h-4" />
      </Button>
      <Button variant="danger" size="sm" onClick={() => onDelete(entry.id)}>
        <Trash2 className="w-4 h-4" />
      </Button>
    </div>
  </div>
);

const LecturerSchedule = () => {
  const queryClient = useQueryClient();
  const { user } = useAuthStore();
  const lecturerId = user?.userId; // Ensure userId is correct property
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedSchedule, setSelectedSchedule] = useState(null);

  // Fetch lecturer's schedule
  const { data: schedule, isLoading, error } = useQuery({
    queryKey: ['lecturerSchedule', lecturerId],
    queryFn: () => lecturerService.getLecturerSchedule(lecturerId),
    enabled: !!lecturerId,
    staleTime: 5 * 60 * 1000, 
  });

  // Mutation for deleting a schedule entry
  const deleteScheduleMutation = useMutation({
    mutationFn: (scheduleId) => lecturerService.deleteLecturerScheduleEntry(scheduleId),
    onSuccess: () => {
      toast.success('Schedule entry deleted successfully!');
      queryClient.invalidateQueries(['lecturerSchedule', lecturerId]);
    },
    onError: (err) => {
      toast.error(err.message || 'Failed to delete schedule entry.');
    },
  });

  const handleEdit = (entry) => {
    setSelectedSchedule(entry);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setSelectedSchedule(null);
    setIsModalOpen(true);
  }

  if (isLoading) return <LoadingSpinner fullScreen />;
  if (error) return <EmptyState icon={CalendarDays} title="Error Loading Schedule" message={error.message} />;

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5 }}
      className="space-y-6"
    >
      <div className="flex justify-between items-center">
        <div>
            <h1 className="text-3xl font-bold tracking-tight">My Schedule</h1>
            <p className="text-gray-500 dark:text-gray-400">Manage your weekly classes and recurring bookings.</p>
        </div>
        <Button onClick={handleAddNew}>
            <Plus className="w-4 h-4 mr-2" /> Add Class
        </Button>
      </div>

      <Card>
        <Card.Body className="p-0">
          {schedule?.length === 0 ? (
            <div className="p-8">
                <EmptyState
                icon={CalendarDays}
                title="No Classes Scheduled"
                message="You haven't added any classes to your schedule yet."
                action={<Button onClick={handleAddNew}>Add Your First Class</Button>}
                />
            </div>
          ) : (
            <div className="divide-y divide-gray-100 dark:divide-dark-700">
              {schedule?.map((entry) => (
                <ScheduleEntry
                  key={entry.id}
                  entry={entry}
                  onEdit={handleEdit}
                  onDelete={deleteScheduleMutation.mutate}
                />
              ))}
            </div>
          )}
        </Card.Body>
      </Card>

      {/* Modal */}
      {isModalOpen && (
        <ScheduleModal 
            isOpen={isModalOpen} 
            onClose={() => setIsModalOpen(false)} 
            scheduleItem={selectedSchedule}
            lecturerId={lecturerId}
        />
      )}

    </motion.div>
  );
};

export default LecturerSchedule;