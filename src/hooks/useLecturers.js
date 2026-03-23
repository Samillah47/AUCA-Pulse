import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import lecturerService from '../services/lecturerService';
import useDebounce from './useDebounce';

const useLecturers = (initialFilters = {}) => {
  const [filters, setFilters] = useState(initialFilters);
  const debouncedFilters = useDebounce(filters, 500);

  const { data, isLoading, error, isFetching } = useQuery({
    queryKey: ['lecturers', debouncedFilters],
    queryFn: () => lecturerService.getLecturers(debouncedFilters),
    keepPreviousData: true,
  });

  return {
    lecturers: data?.content || [],
    page: data,
    isLoading,
    isFetching,
    error,
    filters,
    setFilters,
  };
};

export default useLecturers;
