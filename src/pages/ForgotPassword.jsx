import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'react-hot-toast';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Mail, CheckCircle, ArrowLeft, Shield, Lock, Clock } from 'lucide-react';
import authService from '../services/authService';
import Input from '../components/common/Input';
import Button from '../components/common/Button';
import Card from '../components/common/Card';
import { validationRules } from '../utils/validators';

const ForgotPassword = () => {
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm();
  const [isSubmitted, setIsSubmitted] = useState(false);

  const onSubmit = async (data) => {
    try {
      await authService.forgotPassword(data);
      toast.success('Your password reset request has been submitted for approval.');
      setIsSubmitted(true);
    } catch (error) {
      toast.error(error.message || 'Failed to submit request.');
    }
  };

  const securityFeatures = [
    {
      icon: <Shield className="w-5 h-5" />,
      text: 'Admin-approved process'
    },
    {
      icon: <Lock className="w-5 h-5" />,
      text: 'Secure token generation'
    },
    {
      icon: <Clock className="w-5 h-5" />,
      text: '1-hour validity window'
    }
  ];

  if (isSubmitted) {
    return (
      <div className="min-h-screen flex">
        {/* Left Side - Success Message */}
        <div className="flex-1 flex items-center justify-center p-4 sm:p-8 bg-gray-50 dark:bg-dark-950">
          <motion.div
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ duration: 0.5 }}
            className="w-full max-w-md"
          >
            <Card className="shadow-xl border-0">
              <Card.Body className="p-8 text-center">
                <motion.div
                  initial={{ scale: 0 }}
                  animate={{ scale: 1 }}
                  transition={{ duration: 0.5, delay: 0.2 }}
                  className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-green-100 dark:bg-green-900/30 mb-6"
                >
                  <CheckCircle className="w-12 h-12 text-green-600 dark:text-green-400" />
                </motion.div>

                <h1 className="text-h2 text-gray-900 dark:text-white mb-3">
                  Request Submitted! ✓
                </h1>
                
                <p className="text-gray-600 dark:text-gray-300 mb-8 leading-relaxed">
                  Your password reset request has been sent to the admin team for review. 
                  You'll receive an email with further instructions once approved.
                </p>

                <div className="bg-blue-50 dark:bg-blue-900/20 rounded-xl p-4 mb-6 text-left">
                  <div className="flex items-start gap-3">
                    <svg className="w-5 h-5 text-blue-600 dark:text-blue-400 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                    </svg>
                    <div className="text-sm text-blue-900 dark:text-blue-100">
                      <p className="font-semibold mb-1">What happens next?</p>
                      <ul className="space-y-1 text-blue-800 dark:text-blue-200">
                        <li>• Admin reviews your request (typically within 24 hours)</li>
                        <li>• You receive an approval email with reset link</li>
                        <li>• Link expires after 1 hour for security</li>
                      </ul>
                    </div>
                  </div>
                </div>

                <Link to="/login" className="block">
                  <button className="btn-primary-modern w-full flex items-center justify-center gap-2">
                    <ArrowLeft className="w-4 h-4" />
                    Back to Login
                  </button>
                </Link>
              </Card.Body>
            </Card>
          </motion.div>
        </div>

        {/* Right Side - Visual/Branding (same as login) */}
        <motion.div
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5, delay: 0.2 }}
          className="hidden lg:flex flex-1 relative overflow-hidden"
        >
          {/* Background Image */}
          <div className="absolute inset-0">
            <img 
              src="/assets/images/login-background.png" 
              alt="AUCA Campus"
              className="w-full h-full object-cover"
              onError={(e) => {
                e.target.style.display = 'none';
              }}
            />
            {/* Gradient Overlay */}
            <div className="absolute inset-0 bg-gradient-to-br from-primary-600/95 via-primary-700/90 to-secondary-700/95"></div>
          </div>
          
          {/* Background Pattern */}
          <div className="absolute inset-0 bg-grid-pattern opacity-10 z-10"></div>
          
          {/* Animated Circles */}
          <div className="absolute top-20 right-20 w-72 h-72 bg-white/10 rounded-full blur-3xl animate-pulse-slow"></div>
          <div className="absolute bottom-20 left-20 w-96 h-96 bg-secondary-500/20 rounded-full blur-3xl animate-pulse-slow" style={{ animationDelay: '1s' }}></div>

          {/* Content */}
          <div className="relative z-10 flex flex-col justify-center px-16 text-white">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.5, delay: 0.4 }}
            >
              <div className="inline-flex items-center justify-center w-20 h-20 rounded-2xl bg-white/10 backdrop-blur-sm mb-8">
                <CheckCircle className="w-12 h-12" />
              </div>

              <h2 className="text-display mb-6 leading-tight">
                Request
                <span className="block text-green-300">Submitted</span>
              </h2>
              
              <p className="text-xl text-gray-100 mb-8 max-w-md leading-relaxed">
                Your password reset request is being reviewed by our admin team.
              </p>

              <div className="text-sm text-gray-200">
                Check your email inbox for updates on your request status.
              </div>
            </motion.div>
          </div>
        </motion.div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex">
      {/* Left Side - Form */}
      <div className="flex-1 flex items-center justify-center p-4 sm:p-8 bg-gray-50 dark:bg-dark-950">
        <motion.div
          initial={{ opacity: 0, x: -20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5 }}
          className="w-full max-w-md"
        >
          {/* Back to Login Link */}
          <Link 
            to="/login" 
            className="inline-flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 hover:text-primary-600 dark:hover:text-primary-400 transition-colors mb-8 group"
          >
            <ArrowLeft className="w-4 h-4 group-hover:-translate-x-1 transition-transform" />
            Back to Login
          </Link>

          {/* Header */}
          <div className="mb-8">
            <div className="flex items-center gap-3 mb-6">
              <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-primary-600 to-secondary-600 flex items-center justify-center shadow-lg">
                <Lock className="w-7 h-7 text-white" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Reset Password</h1>
                <p className="text-sm text-gray-500 dark:text-gray-400">Secure recovery process</p>
              </div>
            </div>
            
            <h2 className="text-h3 text-gray-900 dark:text-white mb-2">
              Forgot your password?
            </h2>
            <p className="text-gray-600 dark:text-gray-300">
              No worries! Enter your email and we'll send a reset request to our admin team.
            </p>
          </div>

          {/* Form Card */}
          <Card className="shadow-xl border-0">
            <Card.Body className="p-8">
              <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                <Input
                  label="Email Address"
                  id="email"
                  type="email"
                  placeholder="your.email@auca.ac.rw"
                  leftIcon={<Mail className="w-5 h-5" />}
                  error={errors.email?.message}
                  {...register('email', validationRules.email)}
                />

                <button 
                  type="submit" 
                  disabled={isSubmitting}
                  className="btn-primary-modern w-full"
                >
                  {isSubmitting ? 'Submitting...' : 'Request Password Reset'}
                </button>
              </form>

              {/* Security Info */}
              <div className="mt-6 pt-6 border-t border-gray-200 dark:border-dark-600">
                <p className="text-xs text-gray-500 dark:text-gray-400 mb-3 font-semibold">
                  🔒 Security Features:
                </p>
                <div className="space-y-2">
                  {securityFeatures.map((feature, index) => (
                    <div key={index} className="flex items-center gap-2 text-xs text-gray-600 dark:text-gray-400">
                      <div className="text-primary-600 dark:text-primary-400">
                        {feature.icon}
                      </div>
                      <span>{feature.text}</span>
                    </div>
                  ))}
                </div>
              </div>
            </Card.Body>
          </Card>

          {/* Footer Help */}
          <div className="mt-6 text-center">
            <p className="text-sm text-gray-600 dark:text-gray-400">
              Remember your password?{' '}
              <Link
                to="/login"
                className="font-semibold text-primary-600 hover:text-primary-500 transition-colors"
              >
                Sign in
              </Link>
            </p>
          </div>
        </motion.div>
      </div>

      {/* Right Side - Visual/Branding */}
      <motion.div
        initial={{ opacity: 0, x: 20 }}
        animate={{ opacity: 1, x: 0 }}
        transition={{ duration: 0.5, delay: 0.2 }}
        className="hidden lg:flex flex-1 relative overflow-hidden"
      >
        {/* Background Image */}
        <div className="absolute inset-0">
          <img 
            src="/assets/images/login-background.png" 
            alt="AUCA Campus"
            className="w-full h-full object-cover"
            onError={(e) => {
              e.target.style.display = 'none';
            }}
          />
          {/* Gradient Overlay */}
          <div className="absolute inset-0 bg-gradient-to-br from-primary-600/95 via-primary-700/90 to-secondary-700/95"></div>
        </div>
        
        {/* Background Pattern */}
        <div className="absolute inset-0 bg-grid-pattern opacity-10 z-10"></div>
        
        {/* Animated Circles */}
        <div className="absolute top-20 right-20 w-72 h-72 bg-white/10 rounded-full blur-3xl animate-pulse-slow"></div>
        <div className="absolute bottom-20 left-20 w-96 h-96 bg-secondary-500/20 rounded-full blur-3xl animate-pulse-slow" style={{ animationDelay: '1s' }}></div>

        {/* Content */}
        <div className="relative z-10 flex flex-col justify-center px-16 text-white">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: 0.4 }}
          >
            <h2 className="text-display mb-6 leading-tight">
              Secure Password
              <span className="block text-yellow-300">Recovery</span>
            </h2>
            
            <p className="text-xl text-gray-100 mb-12 max-w-md leading-relaxed">
              We take security seriously. Your password reset request will be reviewed by an administrator before processing.
            </p>

            {/* Security Steps */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-100 mb-4">How it works:</h3>
              {[
                { num: 1, text: 'Submit your email address' },
                { num: 2, text: 'Admin reviews and approves request' },
                { num: 3, text: 'Receive secure reset link via email' },
                { num: 4, text: 'Create your new password' }
              ].map((step, index) => (
                <motion.div
                  key={index}
                  initial={{ opacity: 0, x: -20 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ duration: 0.5, delay: 0.6 + index * 0.1 }}
                  className="flex items-center gap-4 text-gray-100"
                >
                  <div className="flex-shrink-0 w-8 h-8 rounded-full bg-white/20 flex items-center justify-center text-sm font-bold">
                    {step.num}
                  </div>
                  <span className="text-lg">{step.text}</span>
                </motion.div>
              ))}
            </div>

            {/* Trust Badge */}
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 0.5, delay: 1 }}
              className="mt-12 p-6 bg-white/10 backdrop-blur-sm rounded-2xl border border-white/20"
            >
              <div className="flex items-start gap-3">
                <Shield className="w-6 h-6 text-green-300 flex-shrink-0" />
                <div>
                  <h4 className="font-semibold text-white mb-1">Protected Process</h4>
                  <p className="text-sm text-gray-200">
                    All password resets are manually approved to prevent unauthorized access to your account.
                  </p>
                </div>
              </div>
            </motion.div>
          </motion.div>
        </div>
      </motion.div>
    </div>
  );
};

export default ForgotPassword;
