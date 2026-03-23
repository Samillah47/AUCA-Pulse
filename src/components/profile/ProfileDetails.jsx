import React from 'react';
import { useForm } from 'react-hook-form';
import { useAuthStore } from '../../store/authStore';
import { toast } from 'react-hot-toast';
import Input from '../common/Input';
import Button from '../common/Button';
import Card from '../common/Card';
import { User, Mail, Phone } from 'lucide-react';
import { validationRules } from '../../utils/validators';
import { userService } from '../../services';

const ProfileDetails = () => {
    const { user, setUser } = useAuthStore();
    const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm({
        defaultValues: {
            name: user?.name,
            email: user?.email,
            phoneNumber: user?.phoneNumber,
        }
    });

    const onSubmit = async (data) => {
        try {
            const updatedUser = await userService.updateProfile(data);
            setUser(updatedUser);
            toast.success('Profile updated successfully!');
        } catch (error) {
            toast.error(error.message || 'Failed to update profile.');
        }
    };

    return (
        <Card>
            <Card.Header>
                <h3 className="text-lg font-semibold">Profile Details</h3>
            </Card.Header>
            <Card.Body>
                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <Input
                        label="Full Name"
                        id="name"
                        leftIcon={<User />}
                        error={errors.name?.message}
                        {...register('name', validationRules.name)}
                    />
                    <Input
                        label="Email"
                        id="email"
                        type="email"
                        leftIcon={<Mail />}
                        error={errors.email?.message}
                        {...register('email', validationRules.email)}
                        disabled // Don't allow email change for now
                    />
                     <Input
                        label="Phone Number"
                        id="phoneNumber"
                        type="tel"
                        leftIcon={<Phone />}
                        error={errors.phoneNumber?.message}
                        {...register('phoneNumber')}
                    />
                    <div className="flex justify-end">
                        <Button type="submit" loading={isSubmitting}>Save Changes</Button>
                    </div>
                </form>
            </Card.Body>
        </Card>
    );
};

export default ProfileDetails;
