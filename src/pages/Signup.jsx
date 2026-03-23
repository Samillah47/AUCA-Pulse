import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import SignupForm from '../components/auth/SignupForm';
import Card from '../components/common/Card';
import { ArrowLeft, UserPlus, CheckCircle, Clock } from 'lucide-react';

const Signup = () => {
  const steps = [
    {
      icon: <UserPlus className="w-5 h-5" />,
      text: 'Create your account'
    },
    {
      icon: <Clock className="w-5 h-5" />,
      text: 'Wait for admin approval'
    },
    {
      icon: <CheckCircle className="w-5 h-5" />,
      text: 'Access your dashboard'
    }
  ];

  return (
    <div className="min-h-screen flex">
      {/* Left Side - Signup Form */}
      <div className="flex-1 flex items-center justify-center p-4 sm:p-8 bg-gray-50 dark:bg-dark-950">
        <motion.div
          initial={{ opacity: 0, x: -20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5 }}
          className="w-full max-w-2xl"
        >
          {/* Back to Landing Link */}
          <Link 
            to="/" 
            className="inline-flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 hover:text-primary-600 dark:hover:text-primary-400 transition-colors mb-8 group"
          >
            <ArrowLeft className="w-4 h-4 group-hover:-translate-x-1 transition-transform" />
            Back to Home
          </Link>

          {/* Header */}
          <div className="mb-8">
            <div className="flex items-center gap-3 mb-6">
              <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-primary-600 to-secondary-600 flex items-center justify-center shadow-lg">
                <span className="text-white font-bold text-2xl">⚡</span>
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-900 dark:text-white">ACPulse</h1>
                <p className="text-sm text-gray-500 dark:text-gray-400">AUCA Smart Campus</p>
              </div>
            </div>
            
            <h2 className="text-h2 text-gray-900 dark:text-white mb-2">
              Request Access
            </h2>
            <p className="text-gray-600 dark:text-gray-300">
              Join the AUCA Smart Campus community
            </p>
          </div>

          {/* Signup Form Card */}
          <Card className="shadow-xl border-0">
            <Card.Body className="p-8">
              <SignupForm />
            </Card.Body>
          </Card>

          {/* Footer Links */}
          <div className="mt-6 text-center">
            <p className="text-sm text-gray-600 dark:text-gray-400">
              Already have an account?{' '}
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
            src="/assets/images/signup-background.jpg" 
            alt="AUCA Students"
            className="w-full h-full object-cover"
            onError={(e) => {
              e.target.style.display = 'none';
            }}
          />
          {/* Gradient Overlay */}
          <div className="absolute inset-0 bg-gradient-to-br from-secondary-600/95 via-secondary-700/90 to-accent-700/95"></div>
        </div>
        
        {/* Background Pattern */}
        <div className="absolute inset-0 bg-grid-pattern opacity-10 z-10"></div>
        
        {/* Animated Circles */}
        <div className="absolute top-20 left-20 w-72 h-72 bg-white/10 rounded-full blur-3xl animate-pulse-slow"></div>
        <div className="absolute bottom-20 right-20 w-96 h-96 bg-accent-500/20 rounded-full blur-3xl animate-pulse-slow" style={{ animationDelay: '1s' }}></div>

        {/* Content */}
        <div className="relative z-10 flex flex-col justify-center px-16 text-white">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: 0.4 }}
          >
            <h2 className="text-display mb-6 leading-tight">
              Join the
              <span className="block text-yellow-300">Smart Campus</span>
            </h2>
            
            <p className="text-xl text-gray-100 mb-12 max-w-md leading-relaxed">
              Get instant access to campus resources, connect with lecturers, and stay informed about everything happening at AUCA.
            </p>

            {/* Steps List */}
            <div className="space-y-6">
              <h3 className="text-lg font-semibold text-gray-100 mb-4">How it works:</h3>
              {steps.map((step, index) => (
                <motion.div
                  key={index}
                  initial={{ opacity: 0, x: -20 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ duration: 0.5, delay: 0.6 + index * 0.1 }}
                  className="flex items-center gap-4 text-gray-100"
                >
                  <div className="flex-shrink-0 w-10 h-10 rounded-xl bg-white/10 backdrop-blur-sm flex items-center justify-center">
                    {step.icon}
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="flex-shrink-0 w-8 h-8 rounded-full bg-white/20 flex items-center justify-center text-sm font-bold">
                      {index + 1}
                    </span>
                    <span className="text-lg">{step.text}</span>
                  </div>
                </motion.div>
              ))}
            </div>

            {/* Info Box */}
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 0.5, delay: 1 }}
              className="mt-12 p-6 bg-white/10 backdrop-blur-sm rounded-2xl border border-white/20"
            >
              <div className="flex items-start gap-3">
                <svg className="w-6 h-6 text-yellow-300 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                </svg>
                <div>
                  <h4 className="font-semibold text-white mb-1">Account Approval</h4>
                  <p className="text-sm text-gray-200">
                    Your account will be reviewed by an administrator. You'll receive an email once approved, typically within 24 hours.
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

export default Signup;
