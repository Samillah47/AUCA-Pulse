import React from 'react';
import { useNavigate } from 'react-router-dom';
import { MapPin, Clock, User, Users, MonitorPlay, FlaskConical, Briefcase } from 'lucide-react';
import Card from '../common/Card';
import { ROOM_STATUS } from '../../utils/constants';

const RoomCard = ({ room }) => {
  const navigate = useNavigate();

  const handleCardClick = () => {
    navigate(`/rooms/${room.id}`);
  };

  const getRoomIcon = (type) => {
    switch (type) {
      case 'LAB': return <FlaskConical className="w-5 h-5 text-purple-500" />;
      case 'OFFICE': return <Briefcase className="w-5 h-5 text-blue-500" />;
      case 'MEETING_ROOM': return <Users className="w-5 h-5 text-orange-500" />;
      case 'LECTURE_HALL':
      default: return <MonitorPlay className="w-5 h-5 text-gray-500" />;
    }
  };

  const getStatusBadge = (status) => {
    const styles = {
      AVAILABLE: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
      OCCUPIED: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300',
      MAINTENANCE: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300',
    };
    return (
      <span className={`px-2 py-1 rounded-full text-xs font-medium ${styles[status] || styles.AVAILABLE}`}>
        {status}
      </span>
    );
  };

  return (
    <Card
      onClick={handleCardClick}
      className="hover:shadow-lg transition-shadow cursor-pointer"
    >
      <Card.Body>
        <div className="flex justify-between items-start">
          <div className="flex items-center gap-2">
            {getRoomIcon(room?.roomType)}
            <div>
                 <h3 className="text-lg font-bold text-gray-900 dark:text-white">{room?.roomName}</h3>
                 <span className="text-xs text-gray-400 font-mono">{room?.roomType?.replace('_', ' ')}</span>
            </div>
          </div>
          {getStatusBadge(room?.status)}
        </div>
        
        <div className="text-sm text-gray-500 dark:text-gray-400 mt-2">
          <div className="flex items-center gap-2">
            <MapPin className="w-4 h-4" />
            <span>{room?.building} - {room?.floor}</span>
          </div>
        </div>

        <div className="mt-4 border-t border-gray-200 dark:border-dark-700 pt-4">
          {room?.status === ROOM_STATUS.OCCUPIED && room?.currentLecturerName ? (
            <div className="flex items-center gap-2 text-sm">
              <User className="w-4 h-4 text-red-500" />
              <span className="text-gray-600 dark:text-gray-300">Occupied by <strong>{room.currentLecturerName}</strong></span>
            </div>
          ) : (
            <div className="flex items-center gap-2 text-sm text-green-600 dark:text-green-400">
              <Users className="w-4 h-4" />
              <span>Capacity: {room?.capacity}</span>
            </div>
          )}
          {room?.occupiedUntil && (
            <div className="flex items-center gap-2 text-sm mt-2 text-orange-600 dark:text-orange-400">
              <Clock className="w-4 h-4" />
              <span>Until {new Date(room.occupiedUntil).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</span>
            </div>
          )}
        </div>
      </Card.Body>
    </Card>
  );
};

export default RoomCard;
