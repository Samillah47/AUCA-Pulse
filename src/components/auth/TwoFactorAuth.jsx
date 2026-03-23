import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store';
import authService from '../../services/authService';
import { Input, Button } from '../common';
import { toast } from 'react-hot-toast';

const TwoFactorAuth = () => {
  const [otp, setOtp] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const loginSuccess = useAuthStore((state) => state.loginSuccess);
  
  // Email should be passed from the login page via navigation state
  const email = location.state?.email;

  // Redirect to login if email is not available, as it's required for OTP verification
  if (!email) {
    // Using a simple effect to navigate after render
    React.useEffect(() => {
        navigate('/login');
        toast.error('An error occurred. Please try logging in again.');
    }, [navigate]);
    return null; // Render nothing while redirecting
  }

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!otp || otp.length !== 6) {
      toast.error('Please enter a valid 6-digit OTP.');
      return;
    }
    setLoading(true);
    try {
      const response = await authService.verifyOtp({ email, otp });
      loginSuccess(response);
      toast.success('Login successful! Redirecting to dashboard...');
      navigate('/dashboard');
    } catch (error) {
      toast.error(error.message || 'Invalid or expired OTP. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gray-50 dark:bg-gray-900 p-4">
      <div className="w-full max-w-md p-8 space-y-6 bg-white rounded-lg shadow-md dark:bg-gray-800">
        <div className="text-center">
            <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Two-Factor Authentication</h1>
            <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
                A 6-digit verification code has been sent to <strong>{email}</strong>.
            </p>
        </div>
        <form className="space-y-6" onSubmit={handleSubmit}>
          <Input
            id="otp"
            label="Verification Code"
            name="otp"
            type="text"
            required
            placeholder="Enter your 6-digit OTP"
            value={otp}
            onChange={(e) => setOtp(e.target.value.replace(/\D/g, ''))} // Allow only digits
            maxLength="6"
            pattern="\d{6}"
            inputMode="numeric"
            autoComplete="one-time-code"
            autoFocus
          />
          <Button type="submit" disabled={loading} className="w-full">
            {loading ? 'Verifying...' : 'Verify & Log In'}
          </Button>
        </form>
        <div className="text-center text-sm">
            <button 
                onClick={() => navigate('/login')} 
                className="font-medium text-indigo-600 hover:text-indigo-500 dark:text-indigo-400 dark:hover:text-indigo-300"
            >
                Back to Login
            </button>
        </div>
      </div>
    </div>
  );
};

export default TwoFactorAuth;
