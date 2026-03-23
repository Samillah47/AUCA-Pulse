import React from 'react';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-hot-toast';
import { Mail, Phone, MapPin, Building, Briefcase, Calendar } from 'lucide-react';
import { format } from 'date-fns';

import { lecturerService } from '../../services';
import { Card, Avatar, Badge, LoadingSpinner, EmptyState, Button } from '../common';
import { LECTURER_STATUS } from '../../utils/constants';
import ScheduleModal from './ScheduleModal';

const LecturerProfile = () => {
    const { lecturerId } = useParams();

    const { data: lecturer, isLoading, error } = useQuery({
        queryKey: ['lecturer', lecturerId],
        queryFn: () => lecturerService.getLecturerById(lecturerId),
        onError: (err) => {
            toast.error(err.message || 'Failed to fetch lecturer details.');
        },
    });

    if (isLoading) return <LoadingSpinner fullScreen text="Loading Lecturer Profile..." />;
    if (error) return <EmptyState title="Error" description={error.message} />;
    if (!lecturer) return <EmptyState title="Lecturer Not Found" />;

    return (
        <div className="max-w-4xl mx-auto space-y-6">
            {/* Header */}
            <Card>
                <Card.Body>
                    <div className="flex flex-col md:flex-row items-center gap-6">
                        <Avatar src={lecturer.profilePicture} name={lecturer.name} size="2xl" />
                        <div className="flex-1 text-center md:text-left">
                            <div className="flex items-center justify-center md:justify-start gap-2">
                                <h1 className="text-3xl font-bold">{lecturer.name}</h1>
                                <Badge status={lecturer.status?.status}>{lecturer.status?.status}</Badge>
                            </div>
                            <p className="text-gray-500 dark:text-gray-400 text-lg mt-1">{lecturer.department}</p>
                            {lecturer.status?.message && (
                                <p className="text-sm text-gray-600 dark:text-gray-300 mt-2 p-2 bg-gray-50 dark:bg-dark-800 rounded-md">
                                    Status Update: {lecturer.status.message}
                                </p>
                            )}
                        </div>
                    </div>
                </Card.Body>
            </Card>

            {/* Contact Information */}
            <Card>
                <Card.Header>
                    <Card.Title>Contact Information</Card.Title>
                </Card.Header>
                <Card.Body>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <InfoItem icon={<Mail />} label="Email" value={lecturer.email} />
                        {lecturer.phoneNumber && <InfoItem icon={<Phone />} label="Phone" value={lecturer.phoneNumber} />}
                        {lecturer.office && <InfoItem icon={<Building />} label="Office" value={lecturer.office.name} />}
                        {lecturer.location && <InfoItem icon={<MapPin />} label="Location" value={lecturer.location.name} />}
                    </div>
                </Card.Body>
            </Card>

            {/* Schedule */}
            <LecturerScheduleList lecturerId={lecturerId} />
        </div>
    );
};

const LecturerScheduleList = ({ lecturerId }) => {
    const queryClient = useQueryClient();
    const [isModalOpen, setIsModalOpen] = React.useState(false);
    const [selectedSlot, setSelectedSlot] = React.useState(null);
    
    // Auth Check: Is the current user the owner of this profile?
    // Assuming auth info is stored in localStorage or Context. For now, checking simplistic Role/ID match would require context. 
    // Let's assume for this task we just show the controls if the user has role LECTURER and IDs match 
    // OR we can pass a prop 'canEdit'.
    // Better approach: Read from localStorage first since we didn't setup a full context provider in this file view.
    const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
    const canEdit = currentUser?.roles?.includes('LECTURER') && currentUser.id === parseInt(lecturerId); 

    const { data: schedule, isLoading, error } = useQuery({
        queryKey: ['lecturerSchedule', lecturerId],
        queryFn: () => lecturerService.getLecturerSchedule(lecturerId),
    });

    const deleteMutation = useMutation({
        mutationFn: lecturerService.deleteLecturerScheduleEntry,
        onSuccess: () => {
            toast.success('Schedule deleted successfully');
            queryClient.invalidateQueries(['lecturerSchedule', lecturerId]);
        },
        onError: (err) => toast.error('Failed to delete schedule')
    });

    const handleEdit = (slot) => {
        setSelectedSlot(slot);
        setIsModalOpen(true);
    };

    const handleDelete = (id) => {
        if(window.confirm('Are you sure you want to delete this class?')) {
            deleteMutation.mutate(id);
        }
    };

    const handleAdd = () => {
        setSelectedSlot(null);
        setIsModalOpen(true);
    }

    if (isLoading) return <LoadingSpinner />;
    if (error) return <div className="text-red-500">Failed to load schedule.</div>;

    return (
        <Card>
            <Card.Header>
                 <div className="flex justify-between items-center">
                    <Card.Title>Weekly Schedule</Card.Title>
                    {canEdit && (
                        <Button size="sm" onClick={handleAdd}>
                            + Add Class
                        </Button>
                    )}
                </div>
            </Card.Header>
            <Card.Body>
                 {(schedule && schedule.length > 0) ? (
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200 dark:divide-dark-700">
                        <thead className="bg-gray-50 dark:bg-dark-800">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Day</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Time</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Course/Detail</th>
                                {canEdit && <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>}
                            </tr>
                        </thead>
                        <tbody className="bg-white dark:bg-dark-900 divide-y divide-gray-200 dark:divide-dark-700">
                            {schedule.map((slot) => (
                                <tr key={slot.id}>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-gray-100">
                                        <Badge color="blue">{slot.dayOfWeek}</Badge>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                                        {format(new Date(`2000-01-01T${slot.startTime}`), 'h:mm a')} - {format(new Date(`2000-01-01T${slot.endTime}`), 'h:mm a')}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                                        {slot.courseName}
                                    </td>
                                    {canEdit && (
                                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                            <button onClick={() => handleEdit(slot)} className="text-indigo-600 hover:text-indigo-900 dark:text-indigo-400 dark:hover:text-indigo-300 mr-4">Edit</button>
                                            <button onClick={() => handleDelete(slot.id)} className="text-red-600 hover:text-red-900 dark:text-red-400 dark:hover:text-red-300">Delete</button>
                                        </td>
                                    )}
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
                 ) : (
                    <EmptyState
                        icon={Calendar}
                        title="No Schedule Available"
                        description="This lecturer has no scheduled classes yet."
                    />
                )}
            </Card.Body>
            {isModalOpen && (
                <ScheduleModal
                    isOpen={isModalOpen}
                    onClose={() => setIsModalOpen(false)}
                    scheduleItem={selectedSlot}
                    lecturerId={lecturerId}
                />
            )}
        </Card>
    );
};

const InfoItem = ({ icon, label, value }) => (
    <div className="flex items-start gap-4">
        <div className="p-2 bg-gray-100 dark:bg-dark-800 rounded-lg text-gray-600 dark:text-gray-300">
            {icon}
        </div>
        <div>
            <p className="text-sm text-gray-500 dark:text-gray-400">{label}</p>
            <p className="font-semibold">{value}</p>
        </div>
    </div>
);

export default LecturerProfile;
