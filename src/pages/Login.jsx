import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import LoginForm from '../components/auth/LoginForm';
import Card from '../components/common/Card';
import { ArrowLeft, Sparkles, Shield, Zap } from 'lucide-react';

const Login = () => {
  const features = [
    {
      icon: <Zap className="w-5 h-5" />,
      text: 'Real-time campus visibility'
    },
    {
      icon: <Shield className="w-5 h-5" />,
      text: 'Secure authentication'
    },
    {
      icon: <Sparkles className="w-5 h-5" />,
      text: 'Smart notifications'
    }
  ];

  return (
    <div className="min-h-screen flex">
      {/* Left Side - Login Form */}
      <div className="flex-1 flex items-center justify-center p-4 sm:p-8 bg-gray-50 dark:bg-dark-950">
        <motion.div
          initial={{ opacity: 0, x: -20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5 }}
          className="w-full max-w-md"
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
              Welcome back
            </h2>
            <p className="text-gray-600 dark:text-gray-300">
              Sign in to access your campus dashboard
            </p>
          </div>

          {/* Login Form Card */}
          <Card className="shadow-xl border-0">
            <Card.Body className="p-8">
              <LoginForm />
            </Card.Body>
          </Card>

          {/* Footer Links */}
          <div className="mt-6 text-center space-y-3">
            <p className="text-sm text-gray-600 dark:text-gray-400">
              Don't have an account?{' '}
              <Link
                to="/signup"
                className="font-semibold text-primary-600 hover:text-primary-500 transition-colors"
              >
                Request Access
              </Link>
            </p>
            <Link
              to="/forgot-password"
              className="block text-sm text-gray-500 hover:text-primary-600 dark:text-gray-400 dark:hover:text-primary-400 transition-colors"
            >
              Forgot your password?
            </Link>
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
              Feel the Pulse of
              <span className="block text-yellow-300">AUCA</span>
            </h2>
            
            <p className="text-xl text-gray-100 mb-12 max-w-md leading-relaxed">
              Your gateway to smart campus connectivity. Track lecturers, find rooms, and stay connected with the AUCA community.
            </p>

            {/* Features List */}
            <div className="space-y-4">
              {features.map((feature, index) => (
                <motion.div
                  key={index}
                  initial={{ opacity: 0, x: -20 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ duration: 0.5, delay: 0.6 + index * 0.1 }}
                  className="flex items-center gap-3 text-gray-100"
                >
                  <div className="w-10 h-10 rounded-xl bg-white/10 backdrop-blur-sm flex items-center justify-center">
                    {feature.icon}
                  </div>
                  <span className="text-lg">{feature.text}</span>
                </motion.div>
              ))}
            </div>

            {/* Trust Badge */}
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 0.5, delay: 1 }}
              className="mt-16 pt-8 border-t border-white/20"
            >
              <p className="text-sm text-gray-200">
                Trusted by the Adventist University of Central Africa community
              </p>
              <div className="flex items-center gap-2 mt-3">
                <div className="flex -space-x-2">
                  {[1, 2, 3, 4].map((i) => (
                    <div
                      key={i}
                      className="w-8 h-8 rounded-full bg-gradient-to-br from-yellow-400 to-orange-500 border-2 border-white flex items-center justify-center text-xs font-bold"
                    >
                      {i === 1 ? '👨' : i === 2 ? '👩' : i === 3 ? '👨‍🎓' : '👩‍🎓'}
                    </div>
                  ))}
                </div>
                <span className="text-sm text-gray-200 ml-2">Join 500+ active users</span>
              </div>
            </motion.div>
          </motion.div>
        </div>
      </motion.div>
    </div>
  );
};

export default Login;
