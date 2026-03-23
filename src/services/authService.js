import api from './mockApi';

/**
 * Authentication service for handling user login, registration, and logout.
 */
const authService = {
  /**
   * Logs in a user.
   * @param {object} credentials - The user's login credentials.
   * @param {string} credentials.email - The user's email.
   * @param {string} credentials.password - The user's password.
   * @returns {Promise<object>} The response data, including the token and user info.
   */
  login: async (credentials) => {
    try {
      const response = await api.post('/auth/login', credentials);
      return response.data;
    } catch (error) {
      // Handle network errors
      if (!error.response) {
        if (error.request) {
          // Request made but no response received
          throw new Error('No response from server. Please check your network connection or ensure the backend is running.');
        } else {
          // Error in request setup
          throw new Error(error.message || 'An error occurred while setting up the request.');
        }
      }
      
      // Handle HTTP error responses
      const errorData = error.response.data;
      const errorMessage = errorData?.message || errorData?.error || 'Login failed. Please try again.';
      throw new Error(errorMessage);
    }
  },

  /**
   * Verifies the two-factor authentication OTP.
   * @param {object} data - The OTP verification data.
   * @param {string} data.email - The user's email.
   * @param {string} data.otp - The one-time password.
   * @returns {Promise<object>} The response data, including the final JWT.
   */
  verifyOtp: async (data) => {
    try {
      const response = await api.post('/auth/verify-otp', data);
      return response.data;
    } catch (error) {
      if (!error.response) {
        if (error.request) {
          throw new Error('No response from server. Check network or backend status.');
        } else {
          throw new Error(error.message || 'Error setting up OTP verification request.');
        }
      }
      
      const errorData = error.response.data;
      const errorMessage = errorData?.message || errorData?.error || 'OTP verification failed.';
      throw new Error(errorMessage);
    }
  },

  /**
   * Registers a new user.
   * @param {object} userData - The data for the new user.
   * @returns {Promise<object>} The response data from the server.
   */
  register: async (userData) => {
    try {
      const response = await api.post('/auth/register', userData);
      return response.data;
    } catch (error) {
      if (!error.response) {
        if (error.request) {
          throw new Error('No response from server. Please check your network connection or ensure the backend is running.');
        } else {
          throw new Error(error.message || 'An error occurred while setting up the request.');
        }
      }
      
      const errorData = error.response.data;
      const errorMessage = errorData?.message || errorData?.error || 'Registration failed. Please try again.';
      throw new Error(errorMessage);
    }
  },

  /**
   * Sends a password reset request.
   * @param {string} email - The user's email address.
   * @returns {Promise<object>} The response data from the server.
   */
  forgotPassword: async (data) => {
    try {
      const response = await api.post('/auth/forgot-password', data);
      return response.data;
    } catch (error) {
      if (!error.response) {
        if (error.request) {
          throw new Error('No response from server. Please check your network connection.');
        } else {
          throw new Error(error.message || 'An error occurred.');
        }
      }
      
      const errorData = error.response.data;
      throw new Error(errorData?.message || 'Failed to send reset link.');
    }
  },

  /**
   * Resets the user's password using a token.
   * @param {string} token - The password reset token from the email.
   * @param {string} newPassword - The new password.
   * @returns {Promise<object>} The response data from the server.
   */
  resetPassword: async (data) => {
    try {
      const response = await api.post('/auth/reset-password', data);
      return response.data;
    } catch (error) {
      if (!error.response) {
        if (error.request) {
          throw new Error('No response from server. Please check your network connection.');
        } else {
          throw new Error(error.message || 'An error occurred.');
        }
      }
      
      const errorData = error.response.data;
      throw new Error(errorData?.message || 'Failed to reset password.');
    }
  },

  /**
   * Fetches the current user's profile information.
   * @returns {Promise<object>} The user's profile data.
   */
  getProfile: async () => {
    try {
      const response = await api.get('/profile');
      return response.data;
    } catch (error) {
      if (!error.response) {
        if (error.request) {
          throw new Error('No response from server. Please check your network connection.');
        } else {
          throw new Error(error.message || 'An error occurred.');
        }
      }
      
      const errorData = error.response.data;
      throw new Error(errorData?.message || 'Failed to fetch profile.');
    }
  },
};

export default authService;
