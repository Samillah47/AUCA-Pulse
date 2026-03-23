import React from 'react';
import { Search } from 'lucide-react';
import Input from '../common/Input';
import Select from '../common/Select';
import { ROOM_STATUS } from '../../utils/constants';

const RoomSearch = ({ filters, setFilters }) => {
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFilters({
      [name]: value,
      page: 0 // Reset to first page when search or status filters change
    });
  };

  return (
    <div className="p-4 bg-white dark:bg-dark-800 rounded-lg shadow-md mb-6">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Input
          name="searchTerm"
          placeholder="Search by room name or location..."
          leftIcon={<Search className="w-5 h-5" />}
          value={filters.searchTerm}
          onChange={handleInputChange}
        />
        <Select
          name="status"
          value={filters.status}
          onChange={handleInputChange}
        >
          <option value="">All Statuses</option>
          <option value={ROOM_STATUS.AVAILABLE}>Available</option>
          <option value={ROOM_STATUS.OCCUPIED}>Occupied</option>
          <option value={ROOM_STATUS.MAINTENANCE}>Maintenance</option>
        </Select>
        {/* Add more filters like building or capacity if needed */}
      </div>
    </div>
  );
};

export default RoomSearch;
