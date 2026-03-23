import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { toast } from 'react-hot-toast';
import { MapPin, Users, Clock, User, X } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { Badge, Button, Input, Select, Modal } from '../common';
import { ROOM_STATUS } from '../../utils/constants';
import { roomService } from '../../services';

const RoomModal = ({ room, isOpen, onClose }) => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [showBookingForm, setShowBookingForm] = useState(false);

  const { register, handleSubmit, reset } = useForm({
    defaultValues: {
      duration: 60,
      occupiedFor: ''
    }
  });

  const bookingMutation = useMutation({
    mutationFn: roomService.occupyRoom,
    onSuccess: () => {
      toast.success('Room booked successfully!');
      queryClient.invalidateQueries(['rooms']);
      queryClient.invalidateQueries(['room', room?.id]);
      reset();
      setShowBookingForm(false);
      onClose();
    },
    onError: (err) => {
      toast.error(err.message || 'Failed to book room');
    }
  });

  if (!isOpen || !room) return null;

  const handleViewDetails = () => {
    onClose();
    navigate(`/rooms/${room.id}`);
  };

  const handleBookRoom = () => {
    setShowBookingForm(true);
  };

  const onSubmitBooking = (data) => {
    bookingMutation.mutate({
      ...data,
      roomNumber: room.roomNumber
    });
  };

  return (
    <AnimatePresence>
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="fixed inset-0 bg-black/60 backdrop-blur-sm"
          onClick={onClose}
        />
        <motion.div
          initial={{ opacity: 0, scale: 0.95, y: 20 }}
          animate={{ opacity: 1, scale: 1, y: 0 }}
          exit={{ opacity: 0, scale: 0.95, y: 20 }}
          className="relative w-full max-w-md bg-white dark:bg-dark-800 rounded-2xl shadow-xl overflow-hidden"
        >
             <button 
                onClick={onClose}
                className="absolute top-4 right-4 p-1 rounded-full hover:bg-gray-100 dark:hover:bg-dark-700 transition z-10"
            >
                <X className="w-5 h-5 text-gray-500" />
            </button>

            <div className="p-6">
                <div className="flex justify-between items-start mb-4">
                    <div>
                        <h2 className="text-2xl font-bold text-gray-900 dark:text-gray-100">{room.roomName}</h2>
                        <div className="flex items-center gap-2 text-gray-500 text-sm mt-1">
                            <MapPin className="w-4 h-4" />
                            <span>{room.building}, Floor {room.floor}</span>
                        </div>
                    </div>
                    <Badge status={room.status}>{room.status}</Badge>
                </div>

                {!showBookingForm ? (
                  <>
                    <div className="space-y-4 mb-6">
                        <div className="flex items-center justify-between p-3 bg-gray-50 dark:bg-dark-900/50 rounded-lg">
                            <div className="flex items-center gap-2">
                                <Users className="w-4 h-4 text-gray-500" />
                                <span className="text-sm font-medium">Capacity</span>
                            </div>
                            <span className="font-bold">{room.capacity} People</span>
                        </div>

                        {room.status === ROOM_STATUS.OCCUPIED && (
                            <div className="p-4 bg-red-50 dark:bg-red-900/10 border border-red-100 dark:border-red-900/20 rounded-lg space-y-3">
                                <div className="flex items-center gap-2 text-red-700 dark:text-red-400">
                                    <User className="w-4 h-4" />
                                    <span className="font-medium">Occupied By</span>
                                </div>
                                <p className="pl-6 font-semibold">{room.currentLecturerName}</p>
                                
                                {room.occupiedUntil && (
                                    <div className="flex items-center gap-2 text-red-600 dark:text-red-400 text-sm pt-2 border-t border-red-200 dark:border-red-800/30">
                                        <Clock className="w-4 h-4" />
                                        <span>Free at {new Date(room.occupiedUntil).toLocaleTimeString()}</span>
                                    </div>
                                )}
                            </div>
                        )}
                    </div>

                    <div className="flex gap-3">
                        <Button onClick={handleViewDetails} variant="outline" className="flex-1">
                            View Details
                        </Button>
                        {room.status === ROOM_STATUS.AVAILABLE && (
                            <Button onClick={handleBookRoom} className="flex-1">
                                Book Room
                            </Button>
                        )}
                    </div>
                  </>
                ) : (
                  <form onSubmit={handleSubmit(onSubmitBooking)} className="space-y-4">
                    <Input 
                      label="Purpose" 
                      {...register('occupiedFor', { required: 'Purpose is required' })} 
                      placeholder="e.g., Student Consultation" 
                    />
                    <Select label="Duration" {...register('duration', { valueAsNumber: true, required: true })}>
                      <option value={30}>30 minutes</option>
                      <option value={60}>1 hour</option>
                      <option value={90}>1.5 hours</option>
                      <option value={120}>2 hours</option>
                    </Select>
                    <div className="flex gap-3 pt-2">
                      <Button 
                        type="button" 
                        onClick={() => setShowBookingForm(false)} 
                        variant="outline" 
                        className="flex-1"
                      >
                        Cancel
                      </Button>
                      <Button 
                        type="submit" 
                        className="flex-1"
                        disabled={bookingMutation.isLoading}
                      >
                        {bookingMutation.isLoading ? 'Booking...' : 'Confirm Booking'}
                      </Button>
                    </div>
                  </form>
                )}
            </div>
        </motion.div>
      </div>
    </AnimatePresence>
  );
};

export default RoomModal;
