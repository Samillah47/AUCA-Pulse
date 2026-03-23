// mockApi.js - Simulates backend API for demo purposes
// Persists users and data in localStorage for the session

const DEMO_USERS = [
  {
    id: 1,
    email: 'admin@demo.com',
    password: 'admin123',
    name: 'Demo Admin',
    role: 'ADMIN',
    token: 'admin-token',
  },
  {
    id: 2,
    email: 'lecturer@demo.com',
    password: 'lecturer123',
    name: 'Demo Lecturer',
    role: 'LECTURER',
    token: 'lecturer-token',
  },
  {
    id: 3,
    email: 'staff@demo.com',
    password: 'staff123',
    name: 'Demo Staff',
    role: 'STAFF',
    token: 'staff-token',
  },
  {
    id: 4,
    email: 'student@demo.com',
    password: 'student123',
    name: 'Demo Student',
    role: 'STUDENT',
    token: 'student-token',
  },
];

function getUsers() {
  const users = JSON.parse(localStorage.getItem('demo_users'));
  if (Array.isArray(users) && users.length > 0) return users;
  localStorage.setItem('demo_users', JSON.stringify(DEMO_USERS));
  return DEMO_USERS;
}

function saveUsers(users) {
  localStorage.setItem('demo_users', JSON.stringify(users));
}

const mockApi = {
  post: async (url, data) => {
    if (url === '/auth/login') {
      const users = getUsers();
      const user = users.find(u => u.email === data.email && u.password === data.password);
      if (!user) throw { response: { data: { message: 'Invalid credentials' } } };
      return { data: { ...user } };
    }
    if (url === '/auth/register') {
      const users = getUsers();
      if (users.some(u => u.email === data.email)) {
        throw { response: { data: { message: 'Email already registered' } } };
      }
      const newUser = {
        id: users.length + 1,
        ...data,
        role: data.role || 'STUDENT',
        token: `${data.email}-token`,
      };
      users.push(newUser);
      saveUsers(users);
      return { data: { ...newUser } };
    }
    // Add more POST endpoints as needed
    throw { response: { data: { message: 'Not implemented' } } };
  },
  get: async (url) => {
            // Admin: password reset requests
            if (url.startsWith('/admin/password-reset-requests')) {
              // Demo: 2 requests, one pending, one approved, with ISO date strings
              return {
                data: [
                  {
                    id: 201,
                    user: { id: 4, name: 'Demo Student', email: 'student@demo.com' },
                    status: 'PENDING',
                    requestedAt: '2026-03-20T10:30:00Z',
                    createdAt: '2026-03-20T10:30:00Z',
                  },
                  {
                    id: 202,
                    user: { id: 3, name: 'Demo Staff', email: 'staff@demo.com' },
                    status: 'APPROVED',
                    requestedAt: '2026-03-19T14:00:00Z',
                    createdAt: '2026-03-19T14:00:00Z',
                  },
                ]
              };
            }

            // Lecturers page: paginated lecturers list
            if (url.startsWith('/lecturers?')) {
              const { DEMO_LECTURERS } = await import('./mockLecturers');
              const params = new URLSearchParams(url.split('?')[1]);
              let filtered = [...DEMO_LECTURERS];
              if (params.get('search')) {
                const search = params.get('search').toLowerCase();
                filtered = filtered.filter(l => l.name.toLowerCase().includes(search) || l.email.toLowerCase().includes(search) || (l.department && l.department.toLowerCase().includes(search)));
              }
              if (params.get('status')) {
                filtered = filtered.filter(l => l.status === params.get('status'));
              }
              const page = parseInt(params.get('page') || '0', 10);
              const size = parseInt(params.get('size') || '9', 10);
              const start = page * size;
              const end = start + size;
              const paged = filtered.slice(start, end);
              return {
                data: {
                  content: paged,
                  totalElements: filtered.length,
                  totalPages: Math.ceil(filtered.length / size),
                  number: page,
                  size: size,
                }
              };
            }
        // Admin: user management table (paginated)
        if (url.startsWith('/admin/users?')) {
          const { DEMO_USERS_LIST } = await import('./mockUsers');
          const params = new URLSearchParams(url.split('?')[1]);
          let filtered = [...DEMO_USERS_LIST];
          if (params.get('search')) {
            const search = params.get('search').toLowerCase();
            filtered = filtered.filter(u => u.name.toLowerCase().includes(search) || u.email.toLowerCase().includes(search));
          }
          if (params.get('role')) {
            filtered = filtered.filter(u => u.role === params.get('role'));
          }
          const page = parseInt(params.get('page') || '0', 10);
          const size = parseInt(params.get('size') || '10', 10);
          const start = page * size;
          const end = start + size;
          const paged = filtered.slice(start, end);
          return {
            data: {
              content: paged,
              totalElements: filtered.length,
              totalPages: Math.ceil(filtered.length / size),
              pageNumber: page,
              pageSize: size,
            }
          };
        }

        // Admin: verification requests (filtered by status)
        if (url.startsWith('/admin/verification-requests')) {
          const { DEMO_VERIFICATION_REQUESTS } = await import('./mockUsers');
          // Only filter by status if present
          const statusMatch = url.match(/status=([^&]+)/);
          let filtered = [...DEMO_VERIFICATION_REQUESTS];
          if (statusMatch) {
            filtered = filtered.filter(r => r.status === decodeURIComponent(statusMatch[1]));
          }
          return { data: filtered };
        }
    // Admin stats
    if (url.startsWith('/admin/stats')) {
      return { data: { users: 4, rooms: 5, lecturers: 2, staff: 1, students: 1 } };
    }

    // Mock rooms list (paginated)
    if (url.startsWith('/rooms?')) {
      const { DEMO_ROOMS } = await import('./mockRooms');
      // Simulate pagination
      const params = new URLSearchParams(url.split('?')[1]);
      const page = parseInt(params.get('page') || '0', 10);
      const size = parseInt(params.get('size') || '9', 10);
      let filtered = [...DEMO_ROOMS];
      if (params.get('search')) {
        const search = params.get('search').toLowerCase();
        filtered = filtered.filter(r => r.roomName.toLowerCase().includes(search) || r.roomNumber.toLowerCase().includes(search));
      }
      if (params.get('status')) {
        filtered = filtered.filter(r => r.status === params.get('status'));
      }
      const start = page * size;
      const end = start + size;
      const paged = filtered.slice(start, end);
      return {
        data: {
          content: paged,
          totalElements: filtered.length,
          totalPages: Math.ceil(filtered.length / size),
          pageNumber: page,
          pageSize: size,
        }
      };
    }

    // Mock room by ID
    const roomIdMatch = url.match(/^\/rooms\/(\d+)/);
    if (roomIdMatch) {
      const { DEMO_ROOMS } = await import('./mockRooms');
      const room = DEMO_ROOMS.find(r => r.id === Number(roomIdMatch[1]));
      if (!room) throw { response: { data: { message: 'Room not found' } } };
      return { data: room };
    }

    // Add more GET endpoints as needed
    throw { response: { data: { message: 'Not implemented' } } };
  },
  put: async (url, data) => {
    // Implement as needed for profile updates, etc.
    throw { response: { data: { message: 'Not implemented' } } };
  }
};

export default mockApi;
