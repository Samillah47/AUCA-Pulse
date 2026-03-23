import { useQuery } from '@tanstack/react-query';
import roomService from '../services/roomService';
import useDebounce from './useDebounce'; // Re-enabled

const useRooms = (filters = {}) => {
  const debouncedFilters = useDebounce(filters, 500); // Re-enabled
  // console.log('useRooms - debouncedFilters:', debouncedFilters); // Removed debug log

  const { data, isLoading, error, isFetching } = useQuery({
    queryKey: ['rooms', { ...debouncedFilters }], // Use debounced filters
    queryFn: () => roomService.getRooms(debouncedFilters),
    keepPreviousData: true, // Re-enabled
  });

  return {
    rooms: data?.content || [],
    page: data || null,
    isLoading,
    isFetching,
    error,
  };
};

export default useRooms;

