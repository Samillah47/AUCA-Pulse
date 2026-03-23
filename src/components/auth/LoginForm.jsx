import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store';
import authService from '../../services/authService';
import { validationRules } from '../../utils/validators';
import { toast } from 'react-hot-toast';
import { Mail, Lock } from 'lucide-react';
import Button from '../common/Button';
import Input from '../common/Input';

const LoginForm = () => {
  const navigate = useNavigate();
  const { loginSuccess, loginOtpPending } = useAuthStore();
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    setError,
  } = useForm();

  const onSubmit = async (data) => {
    try {
      const response = await authService.login(data);
      
      if (response.status === 'PENDING_OTP') {
        // Handle the OTP flow
        loginOtpPending(data.email);
        toast.success(response.message || 'OTP has been sent to your email.');
        navigate('/verify-otp', { state: { email: data.email } });
      } else {
        // This case would be for direct login if 2FA is disabled for a user, for example.
        loginSuccess(response);
        toast.success(`Welcome back, ${response.name}!`);
        navigate('/dashboard');
      }
    } catch (error) {
      console.error('Login failed with error:', error);
      const errorMessage = error.message || 'Login failed. Please check your credentials.';
      toast.error(errorMessage);
      setError('root.serverError', {
        type: 'manual',
        message: errorMessage,
      });
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <Input
        label="Email"
        id="email"
        type="email"
        placeholder="your.email@auca.ac.rw"
        leftIcon={<Mail className="w-5 h-5" />}
        error={errors.email?.message}
        {...register('email', validationRules.email)}
      />

      <Input
        label="Password"
        id="password"
        type="password"
        placeholder="********"
        leftIcon={<Lock className="w-5 h-5" />}
        error={errors.password?.message}
        {...register('password', validationRules.password)}
      />

      {errors.root?.serverError && (
        <p className="text-sm text-red-500">{errors.root.serverError.message}</p>
      )}

      <Button type="submit" variant="primary" className="w-full" loading={isSubmitting}>
        Sign In
      </Button>
    </form>
  );
};

export default LoginForm;

