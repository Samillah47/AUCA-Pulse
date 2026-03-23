import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { Building, Users, Edit } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { toast } from 'react-hot-toast';

import Card from '../common/Card';
import Button from '../common/Button';
import Modal from '../common/Modal';
import Select from '../common/Select';
import Input from '../common/Input';
import { useAuthStore } from '../../store/authStore';
import { staffService } from '../../services';
import { OFFICE_STATUSES } from '../../utils/constants';


const UpdateOfficeStatusModal = ({ isOpen, onClose, currentStatus }) => {
    const { register, handleSubmit, formState: { isSubmitting } } = useForm({
        defaultValues: {
            status: currentStatus?.status || OFFICE_STATUSES.IN_OFFICE,
            message: currentStatus?.message || '',
        }
    });
    const { setUser, user } = useAuthStore();

    const onSubmit = async (data) => {
        const updatedStatus = await staffService.updateOfficeStatus(data);
        setUser({ ...user, officeStatus: updatedStatus });
        toast.success('Office status updated successfully!');
        onClose();
    };

    return (
        <Modal isOpen={isOpen} onClose={onClose} title="Update Your Office Status">
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <Select
                    label="Current Status"
                    id="status"
                    {...register('status')}
                >
                    {Object.values(OFFICE_STATUSES).map(status => (
                        <option key={status} value={status}>{status}</option>
                    ))}
                </Select>

                <Input
                    label="Custom Message (Optional)"
                    id="message"
                    placeholder="e.g., Back at 2 PM"
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


const StaffDashboard = () => {
    const navigate = useNavigate();
    const { user } = useAuthStore();
    const [isModalOpen, setIsModalOpen] = useState(false);

  return (
    <>
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5 }}
      className="space-y-8"
    >
      <div className="flex justify-between items-center">
        <div>
            <h1 className="text-3xl font-bold tracking-tight">Welcome, {user?.name}!</h1>
            <p className="text-gray-500 dark:text-gray-400 mt-1">
                Manage your office status and find colleagues.
            </p>
        </div>
        <Button onClick={() => setIsModalOpen(true)}>
            <Edit className="mr-2 h-4 w-4" /> Update Office Status
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card className="flex flex-col items-center justify-center p-6 text-center hover:shadow-lg transition-shadow">
            <Building className="w-12 h-12 text-primary-500 mb-4"/>
            <h3 className="text-lg font-semibold mb-2">My Office</h3>
            <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
                View and update your office details and status.
            </p>
            <Button onClick={() => navigate('/office')} className="w-full">
                Go to My Office
            </Button>
        </Card>
        <Card className="flex flex-col items-center justify-center p-6 text-center hover:shadow-lg transition-shadow">
            <Users className="w-12 h-12 text-secondary-500 mb-4"/>
            <h3 className="text-lg font-semibold mb-2">Find Colleagues</h3>
            <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
                See the status of other lecturers and staff.
            </p>
            <Button onClick={() => navigate('/lecturers')} variant="secondary" className="w-full">
                Find Lecturers
            </Button>
        </Card>
      </div>
    </motion.div>

    <UpdateOfficeStatusModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        currentStatus={user?.officeStatus}
    />
    </>
  );
};

export default StaffDashboard;
