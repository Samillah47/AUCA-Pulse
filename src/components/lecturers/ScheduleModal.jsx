import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-hot-toast';
import { lecturerService, roomService } from '../../services';
import { Modal, Input, Select, Button, LoadingSpinner } from '../common';

const ScheduleModal = ({ isOpen, onClose, scheduleItem, lecturerId }) => {
    const queryClient = useQueryClient();
    const isEditing = !!scheduleItem;

    const { register, handleSubmit, reset, setValue, control } = useForm({
        defaultValues: {
            courseName: '',
            dayOfWeek: 'MONDAY',
            startTime: '',
            endTime: '',
            roomId: ''
        }
    });

    // Fetch Rooms
    const { data: rooms, isLoading: isLoadingRooms } = useQuery({
        queryKey: ['rooms'],
        queryFn: roomService.getAllRooms,
        enabled: isOpen,
        staleTime: 5 * 60 * 1000
    });

    useEffect(() => {
        if (isOpen && scheduleItem) {
            setValue('courseName', scheduleItem.courseName);
            setValue('dayOfWeek', scheduleItem.dayOfWeek);
            setValue('startTime', scheduleItem.startTime);
            setValue('endTime', scheduleItem.endTime);
            setValue('roomId', scheduleItem.roomId || '');
            setValue('id', scheduleItem.id);
        } else if (isOpen) {
            reset({
                 courseName: '',
                 dayOfWeek: 'MONDAY',
                 startTime: '',
                 endTime: '',
                 roomId: ''
            });
        }
    }, [isOpen, scheduleItem, setValue, reset]);

    const mutation = useMutation({
        mutationFn: (data) => lecturerService.setLecturerSchedule(lecturerId, data),
        onSuccess: () => {
            toast.success(isEditing ? 'Schedule updated successfully' : 'Schedule added successfully');
            queryClient.invalidateQueries(['lecturerSchedule', lecturerId]);
            onClose();
        },
        onError: (error) => {
            toast.error(error.message || 'Failed to save schedule');
        }
    });

    const onSubmit = (data) => {
        const payload = { ...data };
        // Handle optional roomId conversion
        if (Number.isNaN(payload.roomId) || payload.roomId === '') {
            payload.roomId = null; 
        }
        if (isEditing) {
            payload.id = scheduleItem.id;
        }
        mutation.mutate(payload);
    };

    const days = ['MONDAY', 'TUESDAY', 'WEDNESDAY', 'THURSDAY', 'FRIDAY', 'SATURDAY', 'SUNDAY'];

    return (
        <Modal isOpen={isOpen} onClose={onClose} title={isEditing ? 'Edit Schedule' : 'Add New Class'}>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <Input
                    label="Course / Lesson Name"
                    {...register('courseName', { required: 'Course name is required' })}
                    placeholder="e.g. Intro to Web Tech"
                />
                
                <Select label="Day of Week" {...register('dayOfWeek')}>
                    {days.map(day => (
                        <option key={day} value={day}>{day}</option>
                    ))}
                </Select>

                <div className="grid grid-cols-2 gap-4">
                    <Input
                        label="Start Time"
                        type="time"
                        {...register('startTime', { required: 'Start time is required' })}
                    />
                    <Input
                        label="End Time"
                        type="time"
                        {...register('endTime', { required: 'End time is required' })}
                    />
                </div>

                <Select label="Assigned Room (Optional)" {...register('roomId', { valueAsNumber: true })}>
                    <option value="">No Room Assigned</option>
                    {rooms?.content?.map(room => (
                        <option key={room.id} value={room.id}>
                            {room.roomName} ({room.roomNumber})
                        </option>
                    )) || rooms?.map(room => ( // Handle if response is array or page
                        <option key={room.id} value={room.id}>
                            {room.roomName} ({room.roomNumber})
                        </option>
                    ))}
                </Select>

                <Modal.Footer
                    onCancel={onClose}
                    onConfirm={handleSubmit(onSubmit)}
                    confirmText={isEditing ? 'Update' : 'Add Class'}
                    loading={mutation.isLoading}
                />
            </form>
        </Modal>
    );
};

export default ScheduleModal;
