import React from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'react-hot-toast';
import Input from '../common/Input';
import Button from '../common/Button';
import Card from '../common/Card';
import { Lock } from 'lucide-react';
import { validationRules } from '../../utils/validators';
import { userService } from '../../services';

const ChangePassword = () => {
    const { register, handleSubmit, formState: { errors, isSubmitting }, reset } = useForm();

    const onSubmit = async (data) => {
        if (data.newPassword !== data.confirmPassword) {
            toast.error('New passwords do not match.');
            return;
        }
        try {
            await userService.changePassword({ oldPassword: data.oldPassword, newPassword: data.newPassword });
            toast.success('Password changed successfully!');
            reset();
        } catch (error) {
            toast.error(error.message || 'Failed to change password.');
        }
    };

    return (
        <Card>
            <Card.Header>
                <h3 className="text-lg font-semibold">Change Password</h3>
            </Card.Header>
            <Card.Body>
                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <Input
                        label="Old Password"
                        id="oldPassword"
                        type="password"
                        leftIcon={<Lock />}
                        error={errors.oldPassword?.message}
                        {...register('oldPassword', validationRules.password)}
                    />
                    <Input
                        label="New Password"
                        id="newPassword"
                        type="password"
                        leftIcon={<Lock />}
                        error={errors.newPassword?.message}
                        {...register('newPassword', validationRules.password)}
                    />
                    <Input
                        label="Confirm New Password"
                        id="confirmPassword"
                        type="password"
                        leftIcon={<Lock />}
                        error={errors.confirmPassword?.message}
                        {...register('confirmPassword', validationRules.password)}
                    />
                    <div className="flex justify-end">
                        <Button type="submit" loading={isSubmitting}>Update Password</Button>
                    </div>
                </form>
            </Card.Body>
        </Card>
    );
};

export default ChangePassword;
