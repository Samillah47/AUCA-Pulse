import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'react-hot-toast';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { Lock, CheckCircle } from 'lucide-react';
import authService from '../services/authService';
import Input from '../components/common/Input';
import Button from '../components/common/Button';
import { Modal } from '../components/common';
import { validationRules } from '../utils/validators';

const ResetPassword = () => {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const token = searchParams.get('token');
    const [isSuccess, setIsSuccess] = useState(false);

    const { register, handleSubmit, formState: { errors, isSubmitting }, watch } = useForm();
    const password = watch('password');

    const onSubmit = async (data) => {
        try {
            await authService.resetPassword({ token, password: data.password });
            toast.success('Password has been reset successfully!');
            setIsSuccess(true);
        } catch (error) {
            toast.error(error.message || 'Failed to reset password. The link may be invalid or expired.');
            navigate('/forgot-password');
        }
    };

    useEffect(() => {
        if (!token) {
            toast.error('Invalid or missing password reset token.');
            navigate('/forgot-password');
        }
    }, [token, navigate]);
    
    if (isSuccess) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-dark-950 p-4">
                <Modal isOpen={true} onClose={() => navigate('/login')} title="Success!">
                    <div className="text-center p-4">
                        <CheckCircle className="w-16 h-16 text-green-500 mx-auto mb-4" />
                        <h2 className="text-xl font-bold mb-2">Password Reset Successful</h2>
                        <p className="text-gray-600 dark:text-gray-400 mb-6">You can now log in with your new password.</p>
                        <Link to="/login">
                            <Button variant="primary">Go to Login</Button>
                        </Link>
                    </div>
                </Modal>
            </div>
        );
    }

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-dark-950 p-4">
            <Modal isOpen={true} onClose={() => navigate('/login')} title="Reset Your Password">
                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                        Choose a new, strong password for your account.
                    </p>
                    <Input
                        label="New Password"
                        id="password"
                        type="password"
                        leftIcon={<Lock />}
                        error={errors.password?.message}
                        {...register('password', validationRules.password)}
                    />
                    <Input
                        label="Confirm New Password"
                        id="confirmPassword"
                        type="password"
                        leftIcon={<Lock />}
                        error={errors.confirmPassword?.message}
                        {...register('confirmPassword', {
                            ...validationRules.password,
                            validate: value => value === password || "Passwords do not match"
                        })}
                    />
                    <Button type="submit" variant="primary" className="w-full" loading={isSubmitting}>
                        Set New Password
                    </Button>
                </form>
            </Modal>
        </div>
    );
};

export default ResetPassword;
