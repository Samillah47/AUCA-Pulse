import React, { useState } from 'react';
import useRooms from '../hooks/useRooms';
import RoomSearch from '../components/rooms/RoomSearch';
import RoomCard from '../components/rooms/RoomCard';
import LoadingSpinner from '../components/common/LoadingSpinner';
import EmptyState from '../components/common/EmptyState';
import Pagination from '../components/common/Pagination';
import RoomModal from '../components/rooms/RoomModal';
import { DoorOpen } from 'lucide-react';

const Rooms = () => {
  const [filters, setFilters] = useState({
    searchTerm: '',
    status: '',
    page: 0,
    size: 9,
  });
  const [selectedRoom, setSelectedRoom] = useState(null);

  const { rooms, page, isLoading, isFetching, error } = useRooms(filters);

  const handlePageChange = (newPage) => {
    setFilters((prevFilters) => ({
      ...prevFilters,
      page: newPage - 1, // Adjust to 0-based index for backend
    }));
  };

  const handleFilterChange = (newFilters) => {
    setFilters({
      ...filters,
      ...newFilters,
      page: 0, // Reset to first page on filter change
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold tracking-tight">Rooms</h1>
      </div>
      
      <RoomSearch filters={filters} setFilters={handleFilterChange} />

      {isLoading && (
        <div className="flex justify-center p-8">
          <LoadingSpinner size="lg" />
        </div>
      )}

      {!isLoading && error && (
        <EmptyState
          icon={DoorOpen}
          title="An Error Occurred"
          description={`Failed to fetch rooms: ${error.message}`}
        />
      )}

      {!isLoading && !error && rooms.length === 0 && (
        <EmptyState
          icon={DoorOpen}
          title={filters.searchTerm ? "No Matching Rooms Found" : "No Rooms Available"}
          description={filters.searchTerm ? `No rooms match your search for "${filters.searchTerm}". Try adjusting your filters.` : "There are no rooms currently available in the system."}
        />
      )}

      {!isLoading && !error && rooms.length > 0 && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {rooms.map((room) => (
              <RoomCard 
                key={room.id} 
                room={room} 
                onClick={(r) => setSelectedRoom(r)} 
              />
            ))}
          </div>
          {page && (
            <Pagination
              currentPage={page.number + 1} // Display 1-based page number
              totalPages={page?.totalPages}
              onPageChange={handlePageChange}
            />
          )}
        </>
      )}

      <RoomModal 
          isOpen={!!selectedRoom}
          room={selectedRoom}
          onClose={() => setSelectedRoom(null)}
      />
    </div>
  );
};

export default Rooms;
