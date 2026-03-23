import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Briefcase, Phone } from 'lucide-react';
import Card from '../common/Card';
import Badge from '../common/Badge';
import Avatar from '../common/Avatar';
import { LECTURER_STATUS } from '../../utils/constants';

const LecturerCard = ({ lecturer, onClick }) => {
  if (!lecturer) return null; // Defensive rendering

  const navigate = useNavigate();

  const getStatusBadge = (status) => {
    switch (status) {
      case LECTURER_STATUS.AVAILABLE:
        return <Badge color="green">Available</Badge>;
      case LECTURER_STATUS.IN_OFFICE:
          return <Badge color="blue">In Office</Badge>;
      case LECTURER_STATUS.TEACHING:
        return <Badge color="purple">Teaching</Badge>;
      case LECTURER_STATUS.AWAY:
        return <Badge color="gray">Away</Badge>;
      default:
        return <Badge color="gray">Unknown</Badge>;
    }
  };

  const handleCardClick = () => {
    if (onClick) {
        onClick(lecturer);
    } else {
        navigate(`/lecturers/${lecturer.id}`);
    }
  };

  return (
    <Card
      onClick={handleCardClick}
      className="hover:shadow-lg transition-shadow cursor-pointer"
    >
      <Card.Body>
        <div className="flex items-center gap-4">
          <Avatar name={lecturer.name} size="lg" />
          <div className="flex-1">
            <div className="flex justify-between items-start">
              <h3 className="text-lg font-bold">{lecturer.name}</h3>
              {getStatusBadge(lecturer.status)}
            </div>
            <p className="text-sm text-gray-500 dark:text-gray-400">{lecturer.department}</p>
          </div>
        </div>
        <div className="mt-4 border-t border-gray-200 dark:border-dark-700 pt-4 space-y-2 text-sm">
          <div className="flex items-center gap-2">
            <Briefcase className="w-4 h-4 text-gray-400" />
            <span>{lecturer.office?.name || 'No office assigned'}</span>
          </div>
          {lecturer.user?.phoneNumber && (
             <div className="flex items-center gap-2">
                <Phone className="w-4 h-4 text-gray-400" />
                <span>{lecturer.user.phoneNumber}</span>
            </div>
          )}
        </div>
      </Card.Body>
    </Card>
  );
};

export default LecturerCard;
