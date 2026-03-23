import api from './mockApi';

const staffService = {
  /**
   * Updates the status of the staff member's office.
   * @param {object} statusData - The new status data.
   * @param {string} statusData.status - The new status (e.g., 'IN_OFFICE', 'AWAY').
   * @param {string} [statusData.message] - An optional message.
   * @returns {Promise<object>} The updated office status.
   */
  updateOfficeStatus: async (statusData) => {
    try {
      const response = await api.put('/staff/office/status', statusData);
      return response.data;
    } catch (error) {
      throw error.response.data || error;
    }
  },

  /**
   * Gets the details of the staff member's assigned office.
   * @returns {Promise<object>} The office details.
   */
  getOfficeDetails: async () => {
    try {
        const response = await api.get('/staff/office');
        return response.data;
    } catch (error) {
        throw error.response.data || error;
    }
  }
};

export default staffService;
