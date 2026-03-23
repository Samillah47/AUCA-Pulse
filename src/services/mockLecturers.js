// mockLecturers.js - Demo/mock data for lecturers
import { LECTURER_STATUS } from '../utils/constants';

export const DEMO_LECTURERS = [
  {
    id: 2,
    name: 'Demo Lecturer',
    email: 'lecturer@demo.com',
    department: 'Computer Science',
    status: LECTURER_STATUS.AVAILABLE,
    office: 'C303',
    phone: '0788000000',
  },
  {
    id: 6,
    name: 'Pending User 2',
    email: 'pending2@demo.com',
    department: 'Mathematics',
    status: LECTURER_STATUS.TEACHING,
    office: 'B202',
    phone: '0788111111',
  },
  {
    id: 9,
    name: 'Lecturer Jane',
    email: 'jane.lecturer@demo.com',
    department: 'Physics',
    status: LECTURER_STATUS.IN_OFFICE,
    office: 'A101',
    phone: '0788222222',
  },
  {
    id: 10,
    name: 'Lecturer John',
    email: 'john.lecturer@demo.com',
    department: 'Chemistry',
    status: LECTURER_STATUS.AWAY,
    office: 'D404',
    phone: '0788333333',
  },
];
