import React from 'react';
import { Search } from 'lucide-react';
import Input from '../common/Input';
import Select from '../common/Select';
import { LECTURER_STATUS } from '../../utils/constants';

const LecturerSearch = ({ filters, setFilters }) => {
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFilters({
      [name]: value,
    });
  };

  return (
    <div className="p-4 bg-white dark:bg-dark-800 rounded-lg shadow-md mb-6">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Input
          name="searchTerm"
          placeholder="Search by name or department..."
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
          <option value={LECTURER_STATUS.AVAILABLE}>Available</option>
          <option value={LECTURER_STATUS.IN_OFFICE}>In Office</option>
          <option value={LECTURER_STATUS.TEACHING}>Teaching</option>
          <option value={LECTURER_STATUS.AWAY}>Away</option>
        </Select>
        {/* Add more filters like department if needed */}
      </div>
    </div>
  );
};

export default LecturerSearch;
