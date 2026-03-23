import React, { useState, useEffect } from 'react';
import LecturerSearch from '../components/lecturers/LecturerSearch';
import LecturerCard from '../components/lecturers/LecturerCard';
import LoadingSpinner from '../components/common/LoadingSpinner';
import EmptyState from '../components/common/EmptyState';
import Pagination from '../components/common/Pagination';
import { Users } from 'lucide-react';
import lecturerService from '../services/lecturerService'; // Corrected import
import LecturerModal from '../components/lecturers/LecturerModal';

const Lecturers = () => {
  const [lecturers, setLecturers] = useState([]);
  const [pageData, setPageData] = useState(null); // New state for pagination data
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    searchTerm: '',
    status: '',
    page: 0,
    size: 9,
  });
  const [selectedLecturer, setSelectedLecturer] = useState(null);

  // Fetch lecturers data
  const fetchLecturers = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await lecturerService.getLecturers(filters); // `response` is now a Page object
      setLecturers(response.content || []); // Use response.content
      setPageData({
        number: response.number,
        totalPages: response.totalPages,
        totalElements: response.totalElements,
        size: response.size,
      }); // Set page data
    } catch (err) {
      console.error('Error fetching lecturers:', err);
      setError(err);
      setLecturers([]); // Ensure lecturers array is empty on error
      setPageData(null); // Clear page data on error
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchLecturers();
  }, [filters]); // Refetch when filters change

  const handlePageChange = (newPage) => {
    setFilters((prevFilters) => ({
      ...prevFilters,
      page: newPage - 1, // Adjust to 0-based index for backend
    }));
  };

  const handleFilterChange = (newFilters) => {
    setFilters((prevFilters) => ({
      ...prevFilters,
      ...newFilters,
      page: 0, // Reset to first page on filter change
    }));
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold tracking-tight">Lecturers</h1>
      </div>

      <LecturerSearch filters={filters} setFilters={handleFilterChange} />

      {isLoading && (
        <div className="flex justify-center p-8">
          <LoadingSpinner size="lg" />
        </div>
      )}

      {!isLoading && error && (
        <EmptyState
          icon={Users}
          title="An Error Occurred"
          message={`Failed to fetch lecturers: ${error.message}`}
        />
      )}

      {!isLoading && !error && lecturers.length === 0 && (
        <EmptyState
          icon={Users}
          title="No Lecturers Found"
          message="There are no lecturers registered in the system."
        />
      )}

      {!isLoading && !error && lecturers.length > 0 && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {lecturers.map((lecturer) => (
              <LecturerCard 
                key={lecturer.id} 
                lecturer={lecturer} 
                onClick={(l) => setSelectedLecturer(l)}
              />
            ))}
          </div>
          {pageData && ( // Render pagination only if pageData exists
            <Pagination
              currentPage={pageData.number + 1} // Display 1-based page number
              totalPages={pageData.totalPages}
              onPageChange={handlePageChange}
            />
          )}
        </>
      )}

      <LecturerModal 
        isOpen={!!selectedLecturer}
        lecturer={selectedLecturer}
        onClose={() => setSelectedLecturer(null)}
      />
    </div>
  );
};

export default Lecturers;