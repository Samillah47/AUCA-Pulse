import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowRight, Search, MapPin, Bell, Users, Shield, BookOpen, ChevronRight } from 'lucide-react';

const LandingPage = () => {
  const navigate = useNavigate();
  const [scrollY, setScrollY] = useState(0);

  useEffect(() => {
    const handleScroll = () => setScrollY(window.scrollY);
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const features = [
    {
      icon: <Search className="w-12 h-12 text-primary-500" />,
      title: 'Find Lecturers in Real-Time',
      description: 'Instantly locate lecturers across campus and check their availability status.',
      image: '/assets/images/feature-lecturer.png' // AUCA lecturer image
    },
    {
      icon: <MapPin className="w-12 h-12 text-secondary-500" />,
      title: 'Discover Available Rooms',
      description: 'View real-time room availability for study sessions, meetings, or consultations.',
      image: '/assets/images/feature-rooms.jpg' // Placeholder for AUCA classroom image
    },
    {
      icon: <BookOpen className="w-12 h-12 text-accent-500" />,
      title: 'Know Which Offices Are Open',
      description: 'Check office hours and lecturer locations before heading to campus.',
      image: '/assets/images/feature-office.jpg' // Placeholder for AUCA office image
    },
    {
      icon: <Bell className="w-12 h-12 text-warning-500" />,
      title: 'Smart Notifications',
      description: 'Get instant updates on room bookings, status changes, and important announcements.',
      image: '/assets/images/feature-notifications.jpg' // Placeholder for notification concept
    }
  ];

  const audiences = [
    {
      icon: <Users className="w-16 h-16 text-primary-500" />,
      title: 'Students',
      description: 'Find lecturers, check room availability, and view office hours—all in one place.',
      features: ['Real-time lecturer tracking', 'Room availability search', 'Office hours visibility'],
      image: '/assets/images/students.jpg' // Placeholder for AUCA students
    },
    {
      icon: <BookOpen className="w-16 h-16 text-secondary-500" />,
      title: 'Lecturers & Staff',
      description: 'Manage your status, book rooms, and organize your schedule efficiently.',
      features: ['Update availability status', 'Book rooms instantly', 'Manage teaching schedules'],
      image: '/assets/images/lecturers.png' // AUCA lecturers image
    },
    {
      icon: <Shield className="w-16 h-16 text-accent-500" />,
      title: 'Administrators',
      description: 'Oversee campus operations with comprehensive management tools.',
      features: ['Approve user registrations', 'Manage rooms and offices', 'Monitor system activity'],
      image: '/assets/images/admin.jpg' // Placeholder for admin concept
    }
  ];

  return (
    <div className="min-h-screen bg-white dark:bg-dark-900">
      {/* Hero Section with Background Image */}
      <section className="relative overflow-hidden min-h-screen flex items-center">
        {/* Background Image with Parallax Effect */}
        <div 
          className="absolute inset-0 z-0"
          style={{ transform: `translateY(${scrollY * 0.5}px)` }}
        >
          <div className="absolute inset-0 bg-gradient-to-br from-primary-900/90 via-primary-800/85 to-secondary-900/90 z-10"></div>
          <img 
            src="/assets/images/auca-campus-hero.png" 
            alt="AUCA Campus"
            className="w-full h-full object-cover"
            onError={(e) => {
              e.target.style.display = 'none';
              e.target.parentElement.style.background = 'linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%)';
            }}
          />
          {/* Animated Grid Pattern Overlay */}
          <div className="absolute inset-0 bg-grid-pattern opacity-10 z-20 animate-pulse-slow"></div>
        </div>
        
        <div className="relative z-30 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
          {/* Navigation Bar */}
          <div className="absolute top-8 right-8 flex gap-4 animate-fade-in">
            <button
              onClick={() => navigate('/login')}
              className="px-6 py-2 bg-white/10 backdrop-blur-md text-white border border-white/20 rounded-lg font-medium hover:bg-white/20 transition-all duration-300 flex items-center gap-2"
            >
              Login
              <ChevronRight className="w-4 h-4" />
            </button>
          </div>

          <div className="text-center text-white animate-slide-up">
            {/* Logo/Brand */}
            <div className="flex justify-center mb-8">
              <div className="inline-flex items-center gap-3 px-6 py-3 bg-white/10 backdrop-blur-md rounded-full border border-white/20">
                <span className="text-4xl">⚡</span>
                <span className="text-xl font-bold">ACPulse</span>
              </div>
            </div>

            {/* Headline */}
            <h1 className="text-5xl sm:text-6xl lg:text-7xl font-extrabold mb-6 leading-tight">
              Feel the Pulse of
              <span className="block text-transparent bg-clip-text bg-gradient-to-r from-yellow-300 to-orange-300 mt-2">
                AUCA
              </span>
            </h1>

            {/* Subtext */}
            <p className="max-w-2xl mx-auto text-xl sm:text-2xl text-gray-100 mb-10 leading-relaxed">
              Smart campus visibility for students, lecturers, and staff. 
              Know who's where, when, and how to reach them.
            </p>

            {/* CTA Buttons */}
            <div className="flex flex-col sm:flex-row gap-4 justify-center items-center">
              <button
                onClick={() => navigate('/login')}
                className="group px-8 py-4 bg-white text-primary-600 rounded-xl font-semibold text-lg shadow-2xl hover:shadow-white/20 transform hover:-translate-y-1 transition-all duration-300 flex items-center gap-2"
              >
                Get Started
                <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
              </button>
              <button
                onClick={() => navigate('/signup')}
                className="px-8 py-4 bg-white/10 backdrop-blur-md text-white border-2 border-white/30 rounded-xl font-semibold text-lg hover:bg-white/20 transition-all duration-300"
              >
                Request Access
              </button>
            </div>

            {/* Trust Badge */}
            <p className="mt-8 text-sm text-gray-200">
              Trusted by the Adventist University of Central Africa community
            </p>
          </div>

          {/* Scroll Indicator */}
          <div className="absolute bottom-8 left-1/2 transform -translate-x-1/2 animate-bounce">
            <div className="w-6 h-10 border-2 border-white/50 rounded-full flex justify-center">
              <div className="w-1 h-3 bg-white/50 rounded-full mt-2 animate-pulse"></div>
            </div>
          </div>
        </div>
      </section>

      {/* Feature Highlights with Images */}
      <section className="py-20 bg-gray-50 dark:bg-dark-800">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16 animate-fade-in">
            <h2 className="text-3xl sm:text-4xl font-bold text-gray-900 dark:text-white mb-4">
              Everything You Need, One Platform
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
              ACPulse brings campus visibility to your fingertips with powerful, easy-to-use features.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {features.map((feature, index) => (
              <div
                key={index}
                className="group relative overflow-hidden bg-white dark:bg-dark-700 rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-500 hover:-translate-y-2"
                style={{ animationDelay: `${index * 100}ms` }}
              >
                {/* Background Image */}
                <div className="absolute inset-0 overflow-hidden">
                  <img 
                    src={feature.image}
                    alt={feature.title}
                    className="w-full h-full object-cover opacity-10 group-hover:opacity-20 group-hover:scale-110 transition-all duration-700"
                    onError={(e) => {
                      e.target.style.display = 'none';
                    }}
                  />
                </div>

                <div className="relative p-8">
                  <div className="mb-4 transform group-hover:scale-110 transition-transform duration-300">
                    {feature.icon}
                  </div>
                  <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-3">
                    {feature.title}
                  </h3>
                  <p className="text-gray-600 dark:text-gray-300">
                    {feature.description}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Audience Sections with Images */}
      <section className="py-20 bg-white dark:bg-dark-900 relative overflow-hidden">
        {/* Subtle Background Pattern */}
        <div className="absolute inset-0 bg-grid-pattern opacity-5"></div>
        
        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16 animate-fade-in">
            <h2 className="text-3xl sm:text-4xl font-bold text-gray-900 dark:text-white mb-4">
              Built for Everyone on Campus
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
              Whether you're a student, lecturer, or administrator, ACPulse has tools designed for you.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {audiences.map((audience, index) => (
              <div
                key={index}
                className="group relative overflow-hidden bg-gradient-to-br from-gray-50 to-white dark:from-dark-800 dark:to-dark-700 rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-500"
              >
                {/* Background Image with Overlay */}
                <div className="absolute top-0 left-0 right-0 h-48 overflow-hidden">
                  <div className="absolute inset-0 bg-gradient-to-b from-transparent via-transparent to-white dark:to-dark-700 z-10"></div>
                  <img 
                    src={audience.image}
                    alt={audience.title}
                    className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-700"
                    onError={(e) => {
                      e.target.style.display = 'none';
                      e.target.parentElement.style.background = 'linear-gradient(135deg, #3b82f6 0%, #8b5cf6 100%)';
                    }}
                  />
                </div>

                <div className="relative pt-56 p-8">
                  <div className="mb-6 flex justify-center transform -translate-y-20">
                    <div className="p-4 bg-white dark:bg-dark-800 rounded-full shadow-xl">
                      {audience.icon}
                    </div>
                  </div>
                  <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-4 text-center -mt-12">
                    {audience.title}
                  </h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-6 text-center">
                    {audience.description}
                  </p>
                  <ul className="space-y-3">
                    {audience.features.map((feature, idx) => (
                      <li key={idx} className="flex items-start gap-3">
                        <svg className="w-5 h-5 text-green-500 mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                          <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                        </svg>
                        <span className="text-gray-700 dark:text-gray-300">{feature}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-gray-900 dark:bg-dark-950 text-white py-12 relative overflow-hidden">
        {/* Subtle Background */}
        <div className="absolute inset-0 bg-gradient-to-br from-primary-900/20 to-secondary-900/20"></div>
        
        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-8">
            {/* Brand */}
            <div className="col-span-1 md:col-span-2">
              <div className="flex items-center gap-2 mb-4">
                <span className="text-3xl">⚡</span>
                <span className="text-2xl font-bold">ACPulse</span>
              </div>
              <p className="text-gray-400 mb-4">
                Smart campus visibility for the Adventist University of Central Africa community.
              </p>
              <p className="text-sm text-gray-500">
                © 2025 ACPulse. All rights reserved.
              </p>
            </div>

            {/* Links */}
            <div>
              <h4 className="font-semibold mb-4">Platform</h4>
              <ul className="space-y-2 text-gray-400">
                <li><button onClick={() => navigate('/login')} className="hover:text-white transition-colors">Login</button></li>
                <li><button onClick={() => navigate('/signup')} className="hover:text-white transition-colors">Sign Up</button></li>
                <li><a href="#features" className="hover:text-white transition-colors">Features</a></li>
              </ul>
            </div>

            <div>
              <h4 className="font-semibold mb-4">Legal</h4>
              <ul className="space-y-2 text-gray-400">
                <li><a href="#" className="hover:text-white transition-colors">Privacy Policy</a></li>
                <li><a href="#" className="hover:text-white transition-colors">Terms of Service</a></li>
                <li><a href="#" className="hover:text-white transition-colors">Contact</a></li>
              </ul>
            </div>
          </div>

          <div className="border-t border-gray-800 pt-8 text-center text-gray-500 text-sm">
            <p>Adventist University of Central Africa</p>
            <p className="mt-2">Building the future of campus connectivity</p>
          </div>
        </div>
      </footer>
    </div>
  );
};

export default LandingPage;
