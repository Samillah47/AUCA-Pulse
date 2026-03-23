import React from 'react';
import { Link } from 'react-router-dom';
import { Frown } from 'lucide-react';
import Button from '../components/common/Button';

const NotFound = () => {
  return (
    <div className="flex flex-col items-center justify-center h-full text-center">
        <Frown className="w-24 h-24 text-primary-500 mb-4" />
      <h1 className="text-6xl font-bold">404</h1>
      <p className="text-2xl text-gray-500 dark:text-gray-400 mt-2">Page Not Found</p>
      <p className="mt-4 max-w-md">
        Sorry, the page you are looking for does not exist. You might have mistyped the address or the page may have moved.
      </p>
      <Link to="/dashboard">
        <Button className="mt-6">
            Go to Dashboard
        </Button>
      </Link>
    </div>
  );
};

export default NotFound;
