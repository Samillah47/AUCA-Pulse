import api from './mockApi';

const userService = {
  /**
   * Updates the current user's profile information.
   * @param {object} userData - The data to update (e.g., name, phoneNumber).
   * @returns {Promise<object>} The updated user profile.
   */
  updateProfile: async (userData) => {
    try {
      const response = await api.put('/users/profile', userData);
      return response.data;
    } catch (error) {
      throw error.response.data || error;
    }
  },

  /**
   * Changes the current user's password.
   * @param {object} passwordData - The password data (oldPassword, newPassword).
   * @returns {Promise<object>} Success message.
   */
  changePassword: async (passwordData) => {
    try {
      const response = await api.put('/users/profile/password', passwordData);
      return response.data;
    } catch (error) {
      throw error.response.data || error;
    }
  },

    /**
   * Requests verification for a user.
   * @param {object} verificationData - Data required for verification (e.g., reason, identificationNumber).
   * @returns {Promise<object>} Success message.
   */
  requestVerification: async (verificationData) => {
    try {
      const response = await api.post('/users/profile/request-verification', verificationData);
      return response.data;
    } catch (error) {
      throw error.response.data || error;
    }
  },

  /**
   * Get user's verification status.
   * @returns {Promise<object>} Verification status.
   */
  getVerificationStatus: async () => {
    try {
      const response = await api.get('/users/profile/verification-status');
      return response.data;
    } catch (error) {
      throw error.response.data || error;
    }
  },
};

export default userService;
