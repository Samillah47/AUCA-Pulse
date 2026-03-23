/**
 * Application-wide constants
 */

// User roles
export const ROLES = {
  ADMIN: 'ADMIN',
  LECTURER: 'LECTURER',
  STAFF: 'STAFF',
  STUDENT: 'STUDENT',
};

// Room statuses
export const ROOM_STATUS = {
  AVAILABLE: 'AVAILABLE',
  OCCUPIED: 'OCCUPIED',
  MAINTENANCE: 'MAINTENANCE',
};

// Lecturer statuses
export const LECTURER_STATUS = {
  IN_OFFICE: 'IN_OFFICE',
  TEACHING: 'TEACHING',
  AWAY: 'AWAY',
  AVAILABLE: 'AVAILABLE',
};

// Office statuses
export const OFFICE_STATUSES = {
  IN_OFFICE: 'IN_OFFICE',
  OUT_OF_OFFICE: 'OUT_OF_OFFICE',
  BUSY: 'BUSY',
  AVAILABLE: 'AVAILABLE',
};

// Verification request statuses
export const VERIFICATION_STATUS = {
  PENDING: 'PENDING',
  APPROVED: 'APPROVED',
  REJECTED: 'REJECTED',
};

// Base URL for the API
export const API_BASE_URL = 'http://localhost:8080/api';
