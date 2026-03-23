import api from './mockApi';

const roomService = {
  // Search room by number
  searchRoom: async (roomNumber) => {
    const response = await api.get(`/rooms/search?roomNumber=${roomNumber}`);
    return response.data;
  },

  // Get all rooms (backend returns a simple array)
  getRooms: async (filters = {}) => {
    const { searchTerm = '', status = '', page = 0, size = 9 } = filters;
    
    const params = new URLSearchParams();
    if (searchTerm) params.append('search', searchTerm);
    if (status) params.append('status', status);
    params.append('page', page); // Ensure page is sent
    params.append('size', size); // Ensure size is sent

    const response = await api.get(`/rooms?${params.toString()}`);
    
    return response.data;
  },

  getRoomById: async (roomId) => {
    const response = await api.get(`/rooms/${roomId}`);
    return response.data;
  },

  // Occupy room (Lecturer only)
  occupyRoom: async (data) => {
    const userStr = localStorage.getItem('user');
    if (!userStr) {
      throw new Error('User not authenticated. Please log in again.');
    }
    
    const user = JSON.parse(userStr);
    const userId = user.userId || user.id; // Support both property names
    
    if (!userId) {
      throw new Error('Invalid user data. Please log in again.');
    }
    
    // Convert duration (minutes) to endTime (LocalDateTime)
    // We explicitly format to YYYY-MM-DDTHH:mm:ss to match Java LocalDateTime
    const now = new Date();
    const endTime = new Date(now.getTime() + (data.duration || 60) * 60000);
    
    // Format to local ISO string (YYYY-MM-DDTHH:mm:ss) ignoring timezone offset issues for simplicity
    // or better, use a library, but here we just manually format to ensure compatibility
    const year = endTime.getFullYear();
    const month = String(endTime.getMonth() + 1).padStart(2, '0');
    const day = String(endTime.getDate()).padStart(2, '0');
    const hours = String(endTime.getHours()).padStart(2, '0');
    const minutes = String(endTime.getMinutes()).padStart(2, '0');
    const seconds = String(endTime.getSeconds()).padStart(2, '0');
    
    const endTimeString = `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
    
    const payload = {
      roomNumber: data.roomNumber,
      endTime: endTimeString,
      customMessage: data.occupiedFor || data.customMessage
    };
    
    const response = await api.post(`/lecturer/occupy-room?lecturerId=${userId}`, payload);
    return response.data;
  },

  // Extend room occupation
  extendRoom: async ({ roomId, data }) => {
    const userStr = localStorage.getItem('user');
    if (!userStr) {
      throw new Error('User not authenticated. Please log in again.');
    }
    
    const user = JSON.parse(userStr);
    const userId = user.userId || user.id;
    
    if (!userId) {
      throw new Error('Invalid user data. Please log in again.');
    }
    
    // Convert duration (minutes) to new endTime
    const newEndTime = new Date();
    newEndTime.setMinutes(newEndTime.getMinutes() + (data.duration || 30));
    
    // Fix: Send LOCAL time ISO string (not UTC) because backend deserializes to LocalDateTime literal
    // This handles the timezone offset (e.g. UTC+2) correctly so backend sees '13:00' not '11:00'
    const offsetMs = newEndTime.getTimezoneOffset() * 60000;
    const localISOTime = new Date(newEndTime.getTime() - offsetMs).toISOString().slice(0, -1);

    const payload = {
      newEndTime: localISOTime
    };
    
    const response = await api.put(`/lecturer/extend-room/${roomId}?lecturerId=${userId}`, payload);
    return response.data;
  },

  // Release room
  releaseRoom: async (roomId) => {
    const userStr = localStorage.getItem('user');
    if (!userStr) {
      throw new Error('User not authenticated. Please log in again.');
    }
    
    const user = JSON.parse(userStr);
    const userId = user.userId || user.id;
    
    if (!userId) {
      throw new Error('Invalid user data. Please log in again.');
    }
    
    const response = await api.post(`/lecturer/release-room/${roomId}?lecturerId=${userId}`);
    return response.data;
  },

  // --- Admin Methods ---
  createRoom: async (data) => {
    const response = await api.post('/rooms', data);
    return response.data;
  },

  updateRoom: async (roomId, data) => {
    const response = await api.put(`/rooms/${roomId}`, data);
    return response.data;
  },

  deleteRoom: async (roomId) => {
    await api.delete(`/rooms/${roomId}`);
  }
};

export default roomService;