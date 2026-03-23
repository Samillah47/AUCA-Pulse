// mockRooms.js - Demo/mock data for rooms/classes

import { ROOM_STATUS } from '../utils/constants';

export const DEMO_ROOMS = [
  {
    id: 1,
    roomNumber: 'A101',
    roomName: 'Lecture Hall A101',
    roomType: 'LECTURE_HALL',
    building: 'Main',
    floor: '1',
    status: ROOM_STATUS.AVAILABLE,
  },
  {
    id: 2,
    roomNumber: 'B202',
    roomName: 'Lab B202',
    roomType: 'LAB',
    building: 'Science',
    floor: '2',
    status: ROOM_STATUS.OCCUPIED,
    occupiedBy: 'Demo Lecturer',
    occupiedUntil: '2026-03-23T15:00:00',
  },
  {
    id: 3,
    roomNumber: 'C303',
    roomName: 'Office C303',
    roomType: 'OFFICE',
    building: 'Admin',
    floor: '3',
    status: ROOM_STATUS.MAINTENANCE,
  },
  {
    id: 4,
    roomNumber: 'D404',
    roomName: 'Meeting Room D404',
    roomType: 'MEETING_ROOM',
    building: 'Library',
    floor: '4',
    status: ROOM_STATUS.OCCUPIED,
    occupiedBy: 'Demo Staff',
    occupiedUntil: '2026-03-23T16:00:00',
  },
  {
    id: 5,
    roomNumber: 'E505',
    roomName: 'Lecture Hall E505',
    roomType: 'LECTURE_HALL',
    building: 'Main',
    floor: '5',
    status: ROOM_STATUS.AVAILABLE,
  },
];
