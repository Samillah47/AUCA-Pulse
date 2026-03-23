/**
 * Form validation rules
 */

export const validationRules = {
  email: {
    required: 'Email is required',
    pattern: {
      value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
      message: 'Invalid email format',
    },
  },
  
  password: {
    required: 'Password is required',
    minLength: {
      value: 8,
      message: 'Password must be at least 8 characters',
    },
  },
  
  name: {
    required: 'Name is required',
    minLength: {
      value: 2,
      message: 'Name must be at least 2 characters',
    },
  },
  
  phone: {
    pattern: {
      value: /^\+?[0-9]{10,}$/,
      message: 'Invalid phone number',
    },
  },
  
  identificationNumber: {
    required: 'Identification number is required',
  },
  
  roomNumber: {
    required: 'Room number is required',
  },
};

export default validationRules;