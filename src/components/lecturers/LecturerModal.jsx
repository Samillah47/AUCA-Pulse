import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Mail, Phone, MapPin, Building, X } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { Avatar, Badge, Button } from '../common';

const LecturerModal = ({ lecturer, isOpen, onClose }) => {
  const navigate = useNavigate();

  if (!isOpen || !lecturer) return null;

  const handleViewProfile = () => {
    onClose();
    navigate(`/lecturers/${lecturer.id}`);
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
                className="absolute top-4 right-4 p-1 rounded-full hover:bg-gray-100 dark:hover:bg-dark-700 transition"
            >
                <X className="w-5 h-5 text-gray-500" />
            </button>

            <div className="p-6 text-center">
                <Avatar src={lecturer.profilePicture} name={lecturer.name} size="xl" className="mx-auto mb-4" />
                <h2 className="text-xl font-bold text-gray-900 dark:text-gray-100">{lecturer.name}</h2>
                <p className="text-sm text-gray-500 dark:text-gray-400 mb-2">{lecturer.department}</p>
                <div className="flex justify-center gap-2 mb-6">
                     <Badge status={lecturer.status?.status}>{lecturer.status?.status}</Badge>
                </div>

                <div className="space-y-3 text-left bg-gray-50 dark:bg-dark-900/50 p-4 rounded-xl mb-6">
                    <div className="flex items-center gap-3 text-sm">
                        <Mail className="w-4 h-4 text-gray-400" />
                        <span className="text-gray-700 dark:text-gray-300">{lecturer.email}</span>
                    </div>
                    {lecturer.office && (
                        <div className="flex items-center gap-3 text-sm">
                            <Building className="w-4 h-4 text-gray-400" />
                            <span className="text-gray-700 dark:text-gray-300">Office: {lecturer.office.name}</span>
                        </div>
                    )}
                </div>

                <Button onClick={handleViewProfile} className="w-full">
                    View Full Profile
                </Button>
            </div>
        </motion.div>
      </div>
    </AnimatePresence>
  );
};

export default LecturerModal;
