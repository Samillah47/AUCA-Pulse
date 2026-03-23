// mockUsers.js - Demo/mock data for users and verification requests
import { ROLES, VERIFICATION_STATUS } from '../utils/constants';

export const DEMO_USERS_LIST = [
  {
    id: 1,
    name: 'Demo Admin',
    email: 'admin@demo.com',
    role: ROLES.ADMIN,
    status: 'ACTIVE',
    createdAt: '2026-03-01',
  },
  {
    id: 2,
    name: 'Demo Lecturer',
    email: 'lecturer@demo.com',
    role: ROLES.LECTURER,
    status: 'ACTIVE',
    createdAt: '2026-03-02',
  },
  {
    id: 3,
    name: 'Demo Staff',
    email: 'staff@demo.com',
    role: ROLES.STAFF,
    status: 'ACTIVE',
    createdAt: '2026-03-03',
  },
  {
    id: 4,
    name: 'Demo Student',
    email: 'student@demo.com',
    role: ROLES.STUDENT,
    status: 'ACTIVE',
    createdAt: '2026-03-04',
  },
  // Pending approval
  {
    id: 5,
    name: 'Pending User 1',
    email: 'pending1@demo.com',
    role: ROLES.STUDENT,
    status: 'PENDING',
    createdAt: '2026-03-10',
  },
  {
    id: 6,
    name: 'Pending User 2',
    email: 'pending2@demo.com',
    role: ROLES.LECTURER,
    status: 'PENDING',
    createdAt: '2026-03-11',
  },
  // Rejected
  {
    id: 7,
    name: 'Rejected User',
    email: 'rejected@demo.com',
    role: ROLES.STAFF,
    status: 'REJECTED',
    createdAt: '2026-03-12',
  },
  // More active
  {
    id: 8,
    name: 'Active User 2',
    email: 'active2@demo.com',
    role: ROLES.STUDENT,
    status: 'ACTIVE',
    createdAt: '2026-03-13',
  },
];

export const DEMO_VERIFICATION_REQUESTS = [
  {
    id: 101,
    user: {
      id: 5,
      name: 'Pending User 1',
      email: 'pending1@demo.com',
      role: ROLES.STUDENT,
    },
    status: VERIFICATION_STATUS.PENDING,
    submittedAt: '2026-03-10',
  },
  {
    id: 102,
    user: {
      id: 6,
      name: 'Pending User 2',
      email: 'pending2@demo.com',
      role: ROLES.LECTURER,
    },
    status: VERIFICATION_STATUS.PENDING,
    submittedAt: '2026-03-11',
  },
  {
    id: 103,
    user: {
      id: 7,
      name: 'Rejected User',
      email: 'rejected@demo.com',
      role: ROLES.STAFF,
    },
    status: VERIFICATION_STATUS.REJECTED,
    submittedAt: '2026-03-12',
  },
];
