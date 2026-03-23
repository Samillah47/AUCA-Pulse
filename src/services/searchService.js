import api from './mockApi';

const searchService = {
  /**
   * Performs a global search across different entities.
   * @param {string} query - The search term.
   * @returns {Promise<Array<object>>} A list of search results.
   */
  globalSearch: async (query) => {
    if (!query) {
      return []; // Return empty array if query is empty
    }
    try {
      const response = await api.get('/search', {
        params: { q: query },
      });
      return response.data;
    } catch (error) {
      console.error('Global search failed:', error);
      // Re-throw to be handled by the calling component
      throw error.response?.data || error;
    }
  },
};

export default searchService;
