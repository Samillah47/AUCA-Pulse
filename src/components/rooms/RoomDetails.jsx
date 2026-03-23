import React, { useState } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { toast } from 'react-hot-toast';
import { format } from 'date-fns';
import { MapPin, Users, Clock, User, ShieldCheck, AlertTriangle } from 'lucide-react';

import { roomService } from '../../services';
import { useAuthStore } from '../../store';
import { ROLES, ROOM_STATUS } from '../../utils/constants';

import { Card, Button, Badge, LoadingSpinner, EmptyState, Modal, Input, Select } from '../common';

const RoomDetails = () => {
    const { roomId } = useParams();
    const queryClient = useQueryClient();
    const { user } = useAuthStore();
    
    const [occupyModalOpen, setOccupyModalOpen] = useState(false);
    const [extendModalOpen, setExtendModalOpen] = useState(false);
    const [releaseModalOpen, setReleaseModalOpen] = useState(false);
    const [editModalOpen, setEditModalOpen] = useState(false); // Added state

    const { data: room, isLoading, error } = useQuery({
        queryKey: ['room', roomId],
        queryFn: () => roomService.getRoomById(roomId)
    });

    // --- Mutations ---
    const occupyMutation = useMutation({
        mutationFn: roomService.occupyRoom,
        onSuccess: () => {
            toast.success('Room occupied successfully!');
            queryClient.invalidateQueries(['room', roomId]);
            queryClient.invalidateQueries(['rooms']);
            setOccupyModalOpen(false);
        },
        onError: (err) => toast.error(err.message || 'Failed to occupy room.'),
    });

    const extendMutation = useMutation({
        mutationFn: roomService.extendRoom,
        onSuccess: () => {
            toast.success('Occupation extended successfully!');
            queryClient.invalidateQueries(['room', roomId]);
            queryClient.invalidateQueries(['rooms']);
            setExtendModalOpen(false);
        },
        onError: (err) => toast.error(err.message || 'Failed to extend occupation.'),
    });
    
    const releaseMutation = useMutation({
        mutationFn: roomService.releaseRoom,
        onSuccess: () => {
            toast.success('Room released successfully!');
            queryClient.invalidateQueries(['room', roomId]);
            queryClient.invalidateQueries(['rooms']);
            setReleaseModalOpen(false);
        },
        onError: (err) => toast.error(err.message || 'Failed to release room.'),
    });

    const updateMutation = useMutation({
        mutationFn: (data) => roomService.updateRoom(roomId, data),
        onSuccess: () => {
             toast.success('Room updated successfully!');
             queryClient.invalidateQueries(['room', roomId]);
             setEditModalOpen(false);
        },
        onError: (err) => toast.error(err.message || 'Failed to update room.'),
    });


    // --- Render Logic ---
    if (isLoading) return <LoadingSpinner fullScreen text="Loading Room Details..." />;
    if (error) return <EmptyState title="Error" description={error.message} icon={AlertTriangle} />;
    if (!room) return <EmptyState title="Room Not Found" />;

    const isLecturer = user?.role === ROLES.LECTURER;
    const canOccupy = isLecturer && room.status === ROOM_STATUS.AVAILABLE;
    const canManage = isLecturer && room.occupiedBy?.id === user?.id;

    return (
        <>
            <div className="space-y-6">
                {/* Header */}
                <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                    <div>
                        <div className="flex items-center gap-3">
                            <h1 className="text-4xl font-bold tracking-tight">{room.roomName}</h1>
                            <Badge status={room.status} size="lg">{room.status}</Badge>
                        </div>
                        <div className="flex items-center gap-4 text-gray-500 dark:text-gray-400 mt-2">
                            <div className="flex items-center gap-2">
                                <MapPin className="w-5 h-5" />
                                <span>{room.building}, Floor {room.floor}</span>
                            </div>
                            <div className="flex items-center gap-2">
                                <span className="text-sm font-mono bg-gray-100 dark:bg-dark-800 px-2 py-1 rounded">
                                    #{room.roomNumber}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Action Buttons */}
                <div className="flex gap-2">
                    {isLecturer && (
                        <>
                            {canOccupy && <Button onClick={() => setOccupyModalOpen(true)}>Occupy Room</Button>}
                            {canManage && <Button onClick={() => setExtendModalOpen(true)} variant="outline">Extend Occupation</Button>}
                            {canManage && <Button onClick={() => setReleaseModalOpen(true)} variant="danger">Release Room</Button>}
                        </>
                    )}
                    {user?.role === ROLES.ADMIN && (
                        <Button onClick={() => setEditModalOpen(true)} variant="secondary">Edit Room</Button>
                    )}
                </div>


                {/* Details Card */}
                <Card>
                    <Card.Body className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <InfoItem icon={<Users />} label="Capacity" value={`${room.capacity} People`} />
                        <InfoItem icon={<ShieldCheck />} label="Type" value={room.roomType} />
                        <InfoItem icon={<MapPin />} label="Building" value={room.building} />
                        <InfoItem icon={<MapPin />} label="Floor" value={`Floor ${room.floor}`} />
                        {room.officeOwnerName && (
                            <InfoItem icon={<User className="text-blue-500" />} label="Office Owner" value={room.officeOwnerName} />
                        )}
                        
                        {room.status === ROOM_STATUS.OCCUPIED && room.currentLecturerName && (
                            <>
                                <InfoItem icon={<User />} label="Occupied By" value={room.currentLecturerName} />
                                {room.occupiedUntil && (
                                    <>
                                        <InfoItem 
                                            icon={<Clock />} 
                                            label="Occupied Until" 
                                            value={format(new Date(room.occupiedUntil), 'p, MMM d')} 
                                        />
                                        <div className="md:col-span-2">
                                            <TimeRemaining until={room.occupiedUntil} />
                                        </div>
                                    </>
                                )}
                                {room.occupationMessage && (
                                    <div className="md:col-span-2">
                                        <InfoItem icon={<AlertTriangle />} label="Purpose" value={room.occupationMessage} />
                                    </div>
                                )}
                            </>
                        )}
                    </Card.Body>
                </Card>
                
                {/* Add room schedule component here later if needed */}
            </div>

            {/* Modals */}
            {canOccupy && <OccupyModal isOpen={occupyModalOpen} onClose={() => setOccupyModalOpen(false)} onSubmit={occupyMutation.mutate} isLoading={occupyMutation.isLoading} roomNumber={room.roomNumber} />}
            {canManage && <ExtendModal isOpen={extendModalOpen} onClose={() => setExtendModalOpen(false)} onSubmit={(data) => extendMutation.mutate({roomId: room.id, data})} isLoading={extendMutation.isLoading} />}
            {canManage && <ReleaseModal isOpen={releaseModalOpen} onClose={() => setReleaseModalOpen(false)} onConfirm={() => releaseMutation.mutate(room.id)} isLoading={releaseMutation.isLoading} />}
            {user?.role === ROLES.ADMIN && <EditRoomModal isOpen={editModalOpen} onClose={() => setEditModalOpen(false)} room={room} onSubmit={updateMutation.mutate} isLoading={updateMutation.isLoading} />}

        </>
    );
};

const TimeRemaining = ({ until }) => {
    const [timeLeft, setTimeLeft] = React.useState('');
    
    React.useEffect(() => {
        const updateTime = () => {
            const now = new Date();
            const end = new Date(until);
            const diff = end - now;
            
            if (diff <= 0) {
                setTimeLeft('Expired');
                return;
            }
            
            const hours = Math.floor(diff / (1000 * 60 * 60));
            const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
            
            if (hours > 0) {
                setTimeLeft(`${hours}h ${minutes}m remaining`);
            } else {
                setTimeLeft(`${minutes}m remaining`);
            }
        };
        
        updateTime();
        const interval = setInterval(updateTime, 60000); // Update every minute
        
        return () => clearInterval(interval);
    }, [until]);
    
    return (
        <div className="flex items-center gap-2 p-3 bg-amber-50 dark:bg-amber-900/10 border border-amber-200 dark:border-amber-900/20 rounded-lg">
            <Clock className="w-5 h-5 text-amber-600 dark:text-amber-400" />
            <span className="font-semibold text-amber-700 dark:text-amber-300">{timeLeft}</span>
        </div>
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


// --- Modals ---
const OccupyModal = ({ isOpen, onClose, onSubmit, isLoading, roomNumber }) => {
    const { register, handleSubmit } = useForm();
    
    const handleFormSubmit = (data) => {
        onSubmit({ ...data, roomNumber });
    };

    return (
        <Modal isOpen={isOpen} onClose={onClose} title="Occupy Room">
            <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
                <Input label="Purpose" id="occupiedFor" {...register('occupiedFor', { required: 'Purpose is required.'})} placeholder="e.g., Student Consultation" />
                <Select label="Duration (in minutes)" id="duration" {...register('duration', { valueAsNumber: true, required: true })}>
                    <option value={30}>30 minutes</option>
                    <option value={60}>1 hour</option>
                    <option value={90}>1.5 hours</option>
                    <option value={120}>2 hours</option>
                </Select>
                <Modal.Footer onCancel={onClose} onConfirm={handleSubmit(handleFormSubmit)} confirmText="Occupy" loading={isLoading} />
            </form>
        </Modal>
    );
};

const ExtendModal = ({ isOpen, onClose, onSubmit, isLoading }) => {
    const { register, handleSubmit } = useForm();

    return (
        <Modal isOpen={isOpen} onClose={onClose} title="Extend Occupation">
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <Select label="Extend by (in minutes)" id="duration" {...register('duration', { valueAsNumber: true, required: true })}>
                    <option value={15}>15 minutes</option>
                    <option value={30}>30 minutes</option>
                    <option value={45}>45 minutes</option>
                    <option value={60}>1 hour</option>
                </Select>
                <Modal.Footer onCancel={onClose} onConfirm={handleSubmit(onSubmit)} confirmText="Extend" loading={isLoading} />
            </form>
        </Modal>
    );
};

const ReleaseModal = ({ isOpen, onClose, onConfirm, isLoading }) => (
    <Modal isOpen={isOpen} onClose={onClose} title="Release Room" description="Are you sure you want to release this room? This action cannot be undone.">
        <Modal.Footer
            onCancel={onClose}
            onConfirm={onConfirm}
            confirmText="Yes, Release It"
            loading={isLoading}
        />
    </Modal>
);

const EditRoomModal = ({ isOpen, onClose, room, onSubmit, isLoading }) => {
    const { register, handleSubmit, watch } = useForm({
        defaultValues: {
            roomName: room.roomName,
            roomNumber: room.roomNumber,
            capacity: room.capacity,
            building: room.building,
            floor: room.floor,
            roomType: room.roomType,
            officeOwnerId: room.officeOwnerId || ''
        }
    });

    const watchRoomType = watch('roomType');

    // Fetch lecturers for assignment
    const { data: lecturers } = useQuery({
        queryKey: ['lecturers', 'all'],
        queryFn: () => import('../../services').then(m => m.lecturerService.getLecturers({ size: 100 })),
        enabled: isOpen && watchRoomType === 'OFFICE'
    });

    return (
        <Modal isOpen={isOpen} onClose={onClose} title="Edit Room Details">
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <Input label="Room Name" id="roomName" {...register('roomName', { required: true })} />
                <Input label="Room Number" id="roomNumber" {...register('roomNumber', { required: true })} />
                <div className="grid grid-cols-2 gap-4">
                     <Input label="Building" id="building" {...register('building', { required: true })} />
                     <Input label="Floor" id="floor" {...register('floor', { required: true })} />
                </div>
                <div className="grid grid-cols-2 gap-4">
                    <Input label="Capacity" id="capacity" type="number" {...register('capacity', { required: true, valueAsNumber: true })} />
                    <Select label="Type" id="roomType" {...register('roomType', { required: true })}>
                         <option value="LECTURE_HALL">Lecture Hall</option>
                         <option value="LAB">Lab</option>
                         <option value="OFFICE">Office</option>
                         <option value="MEETING_ROOM">Meeting Room</option>
                    </Select>
                </div>
                
                {watchRoomType === 'OFFICE' && (
                     <Select label="Assign Office Owner" id="officeOwnerId" {...register('officeOwnerId', { valueAsNumber: true })}>
                        <option value="">-- Select Owner --</option>
                        {lecturers?.content?.map(l => (
                            <option key={l.id} value={l.id}>{l.name} ({l.department})</option>
                        ))}
                    </Select>
                )}
                <Modal.Footer onCancel={onClose} onConfirm={handleSubmit(onSubmit)} confirmText="Save Changes" loading={isLoading} />
            </form>
        </Modal>
    );
};

export default RoomDetails;