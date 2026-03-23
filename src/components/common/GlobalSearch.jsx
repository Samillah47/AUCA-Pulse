import { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Search, X, DoorOpen, Users, Building2, TrendingUp, CornerDownLeft, ArrowRight } from 'lucide-react';
import { useDebounce } from '../../hooks';
import { cn } from '../../utils/helpers';
import { useUIStore } from '../../store';
import { searchService, roomService, lecturerService } from '../../services'; // Assuming searchService is available/updated
import { Badge, Avatar, LoadingSpinner, EmptyState } from './index';
import RoomModal from '../rooms/RoomModal';
import LecturerModal from '../lecturers/LecturerModal';

const GlobalSearch = () => {
  const navigate = useNavigate();
  const { isGlobalSearchOpen, closeGlobalSearch } = useUIStore();
  const [query, setQuery] = useState('');
  const [results, setResults] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [recentSearches, setRecentSearches] = useState([]);
  const [activeResult, setActiveResult] = useState(0);
  
  // Modal States
  const [selectedRoom, setSelectedRoom] = useState(null);
  const [selectedLecturer, setSelectedLecturer] = useState(null);

  const inputRef = useRef(null);
  const debouncedQuery = useDebounce(query, 300);

  // Load recent searches
  useEffect(() => {
    try {
      const saved = localStorage.getItem('recentSearches');
      setRecentSearches(saved ? JSON.parse(saved) : []);
    } catch (e) {
      console.error("Failed to parse recent searches from localStorage", e);
      setRecentSearches([]);
    }
  }, []);

  // Focus input on open
  useEffect(() => {
    if (isGlobalSearchOpen) {
      inputRef.current?.focus();
    }
  }, [isGlobalSearchOpen]);

  const addRecentSearch = (item) => {
    setRecentSearches(prev => {
      const newRecents = [item, ...prev.filter(r => r.id !== item.id)];
      const limitedRecents = newRecents.slice(0, 5);
      localStorage.setItem('recentSearches', JSON.stringify(limitedRecents));
      return limitedRecents;
    });
  };

  // Main search logic
  const performSearch = useCallback(async (searchQuery) => {
    if (!searchQuery || searchQuery.trim().length < 2) {
      setResults([]);
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    try {
      const data = await searchService.globalSearch(searchQuery);
      setResults(data);
      setActiveResult(0);
    } catch (error) {
      console.error("Global search failed:", error);
      setResults([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    performSearch(debouncedQuery);
  }, [debouncedQuery, performSearch]);

  const handleNavigation = async (item) => {
    if (!item) return;

    addRecentSearch(item);
    
    // Switch on Type
    if (item.type === 'Navigation') {
        navigate(item.payload); // Payload is URL
        closeGlobalSearch();
        setQuery('');
        setResults([]);
    } else if (item.type === 'Room') {
        // Fetch full room details for modal
        try {
            const roomData = await roomService.getRoomById(item.payload); 
            setSelectedRoom(roomData);
            // Don't close search yet, or maybe do? UX decision. 
            // Let's close search so modal is main focus.
            closeGlobalSearch(); 
        } catch (e) {
            console.error("Failed to fetch room", e);
            navigate(`/rooms/${item.payload}`); // Fallback
            closeGlobalSearch();
        }
    } else if (item.type === 'Lecturer') {
         try {
            const lecturerData = await lecturerService.getLecturerById(item.payload);
            setSelectedLecturer(lecturerData);
            closeGlobalSearch();
        } catch (e) {
             console.error("Failed to fetch lecturer", e);
             navigate(`/lecturers/${item.payload}`); // Fallback
             closeGlobalSearch();
        }
    }
  };
  
  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.key === 'ArrowDown') {
        e.preventDefault();
        setActiveResult(prev => Math.min(prev + 1, results.length - 1));
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        setActiveResult(prev => Math.max(prev - 1, 0));
      } else if (e.key === 'Enter') {
        e.preventDefault();
        const activeItem = results[activeResult];
        if (activeItem) {
          handleNavigation(activeItem);
        }
      } else if (e.key === 'Escape') {
        closeGlobalSearch();
      }
    };
    
    if (isGlobalSearchOpen) {
      window.addEventListener('keydown', handleKeyDown);
    }
    
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isGlobalSearchOpen, results, activeResult, closeGlobalSearch]);


  const renderItem = (item, index) => {
    const isActive = index === activeResult;
    const itemProps = {
      key: `${item.type}-${item.id}`,
      onClick: () => handleNavigation(item),
      onMouseEnter: () => setActiveResult(index),
      className: cn(
        'flex items-center justify-between p-3 rounded-lg cursor-pointer transition-colors',
        isActive ? 'bg-primary-100 dark:bg-primary-900/40' : 'hover:bg-gray-100 dark:hover:bg-dark-800'
      )
    };

    switch (item.type) {
      case 'Room':
        return (
          <div {...itemProps}>
            <div className="flex items-center gap-3">
              <DoorOpen className="w-5 h-5 text-gray-500" />
              <div className="flex flex-col">
                <span className="font-medium text-gray-900 dark:text-gray-100">{item.title}</span>
                <span className="text-xs text-gray-500">{item.subtitle}</span>
              </div>
            </div>
            <Badge color="blue">Room</Badge>
          </div>
        );
      case 'Lecturer':
        return (
          <div {...itemProps}>
            <div className="flex items-center gap-3">
              <Avatar name={item.title} size="sm" />
              <div className="flex flex-col">
                <span className="font-medium text-gray-900 dark:text-gray-100">{item.title}</span>
                <span className="text-xs text-gray-500">{item.subtitle}</span>
              </div>
            </div>
            <Badge color="purple">Lecturer</Badge>
          </div>
        );
      case 'Navigation':
          return (
            <div {...itemProps}>
                <div className="flex items-center gap-3">
                    <ArrowRight className="w-5 h-5 text-gray-400" />
                    <div className="flex flex-col">
                        <span className="font-medium text-gray-900 dark:text-gray-100">{item.title}</span>
                         <span className="text-xs text-gray-500">{item.subtitle}</span>
                    </div>
                </div>
                <div className="text-xs text-gray-400 bg-gray-100 dark:bg-dark-700 px-2 py-1 rounded">Jump to</div>
            </div>
          );
      default:
        return null;
    }
  };

  const SearchResults = () => {
    if (isLoading) {
      return <LoadingSpinner text="Searching..." />;
    }
    if (query.length > 1 && results.length === 0 && !isLoading) {
      return <EmptyState title="No results found" description={`Your search for "${query}" did not return any results.`} icon={Search} />;
    }
    if (results.length > 0) {
      return (
        <div className="space-y-1">
          {results.map(renderItem)}
        </div>
      );
    }
    if (recentSearches.length > 0) {
      return (
        <div className="space-y-2">
          <h4 className="px-3 text-xs font-semibold text-gray-500 uppercase">Recent Searches</h4>
          {recentSearches.map((item, index) => renderItem(item, index))}
        </div>
      );
    }
    return (
        <EmptyState title="Search for anything" description="Find rooms, lecturers, or settings..." icon={Search} />
    );
  };

  return (
    <>
        <AnimatePresence>
        {isGlobalSearchOpen && (
            <div className="fixed inset-0 z-50 flex items-start justify-center pt-16 sm:pt-24">
            <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="fixed inset-0 bg-black/60 backdrop-blur-sm"
                onClick={closeGlobalSearch}
            />
            <motion.div
                initial={{ opacity: 0, scale: 0.95, y: -20 }}
                animate={{ opacity: 1, scale: 1, y: 0 }}
                exit={{ opacity: 0, scale: 0.95, y: -20 }}
                className="relative w-full max-w-lg bg-white dark:bg-dark-900 rounded-xl shadow-2xl mx-4"
            >
                {/* Search Input */}
                <div className="relative">
                <div className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
                    <Search className="w-5 h-5" />
                </div>
                <input
                    ref={inputRef}
                    type="text"
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    placeholder="Search for 'Room 101', 'John', or 'Password'..."
                    className="w-full bg-transparent text-base pl-12 pr-12 py-4 border-0 focus:ring-0 text-gray-900 dark:text-gray-100 placeholder:text-gray-400"
                />
                {query && (
                    <button
                    onClick={() => setQuery('')}
                    className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                    >
                    <X className="w-5 h-5" />
                    </button>
                )}
                </div>

                {/* Results */}
                <div className="p-2 max-h-[60vh] overflow-y-auto custom-scrollbar border-t border-gray-200 dark:border-dark-700">
                <SearchResults />
                </div>
                
                {/* Footer */}
                <div className="px-4 py-2 text-xs text-gray-500 dark:text-gray-400 border-t border-gray-200 dark:border-dark-700 flex items-center justify-between">
                    <span>Tip: Use <kbd className="font-sans font-semibold">↑</kbd> <kbd className="font-sans font-semibold">↓</kbd> to navigate, <kbd className="font-sans font-semibold">↵</kbd> to select.</span>
                    <button onClick={closeGlobalSearch} className="font-semibold hover:text-gray-700 dark:hover:text-gray-200">ESC</button>
                </div>
            </motion.div>
            </div>
        )}
        </AnimatePresence>

        {/* Modals are rendered here, controlled by state */}
        <LecturerModal 
            isOpen={!!selectedLecturer} 
            lecturer={selectedLecturer} 
            onClose={() => setSelectedLecturer(null)} 
        />
        <RoomModal 
            isOpen={!!selectedRoom} 
            room={selectedRoom} 
            onClose={() => setSelectedRoom(null)} 
        />
    </>
  );
};

export default GlobalSearch;