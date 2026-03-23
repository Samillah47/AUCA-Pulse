import React from 'react';

const Footer = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="w-full bg-white dark:bg-dark-900 border-t border-gray-200 dark:border-dark-700">
      <div className="max-w-7xl mx-auto py-4 px-4 sm:px-6 lg:px-8 text-center text-sm text-gray-500 dark:text-gray-400">
        <p>
          &copy; {currentYear} ACPulse - Adventist University of Central Africa. All rights reserved.
        </p>
      </div>
    </footer>
  );
};

export default Footer;
