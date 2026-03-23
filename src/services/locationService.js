import api from './mockApi';

/**
 * Service for fetching location data.
 */
const locationService = {
  /**
   * Fetches locations by type (e.g., PROVINCE, DISTRICT).
   * @param {string} type - The type of location to fetch.
   * @returns {Promise<Array>} A list of locations.
   */
  getLocationsByType: async (type) => {
    try {
      const response = await api.get(`/locations?type=${type}`);
      return response.data;
    } catch (error) {
      console.error(`Failed to fetch locations of type ${type}`, error);
      throw error.response.data || error;
    }
  },

  /**
   * Fetches the children of a specific location.
   * @param {number} parentId - The ID of the parent location.
   * @returns {Promise<Array>} A list of child locations.
   */
  getChildLocations: async (parentId) => {
    if (!parentId) return [];
    try {
      const response = await api.get(`/locations/${parentId}/children`);
      return response.data;
    } catch (error) {
      console.error(`Failed to fetch child locations for parent ${parentId}`, error);
      throw error.response.data || error;
    }
  },
};

export default locationService;
