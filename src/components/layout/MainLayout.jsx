import { useState, useEffect } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { useUIStore, useAuthStore } from '../../store';
import { cn } from '../../utils/helpers';
import Sidebar from './Sidebar';
import TopBar from './TopBar';
import Footer from './Footer';
import { GlobalSearch } from '../common';

const MainLayout = () => {
  const { isSidebarCollapsed, setSidebarCollapsed } = useUIStore();
  const { user } = useAuthStore();
  const [isMobileSidebarOpen, setMobileSidebarOpen] = useState(false);
  const location = useLocation();

  const toggleMobileSidebar = () => {
    setMobileSidebarOpen(!isMobileSidebarOpen);
  };
  
  // Close mobile sidebar on route change
  useEffect(() => {
    setMobileSidebarOpen(false);
  }, [location]);

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-dark-950 text-gray-800 dark:text-gray-200">
      <GlobalSearch />

      {/* Mobile Sidebar */}
      <div className={cn(
        'fixed inset-0 z-50 lg:hidden',
        isMobileSidebarOpen ? 'block' : 'hidden'
      )}>
        {/* Backdrop */}
        <div 
            className="absolute inset-0 bg-black/50"
            onClick={toggleMobileSidebar}
        ></div>
        {/* Sidebar */}
        <div className="relative w-72 h-full">
            <Sidebar
                isCollapsed={false}
                setIsCollapsed={() => {}} // Not used in mobile
                user={user}
            />
        </div>
      </div>
      
      {/* Desktop Sidebar */}
      <div className="hidden lg:block">
        <Sidebar
            isCollapsed={isSidebarCollapsed}
            setIsCollapsed={setSidebarCollapsed}
            user={user}
        />
      </div>

      <div
        className={cn(
          'flex flex-col flex-1 min-h-screen transition-all duration-300',
          isSidebarCollapsed ? 'lg:ml-20' : 'lg:ml-72'
        )}
      >
        <TopBar toggleMobileSidebar={toggleMobileSidebar} />
        <main className="flex-1 p-4 sm:p-6 lg:p-8">
            <Outlet />
        </main>
        <Footer />
      </div>
    </div>
  );
};

export default MainLayout;
