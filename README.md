# ACPulse - Smart Campus Management System

A modern, feature-rich web application for managing campus operations at Adventist University of Central Africa (AUCA). ACPulse streamlines room management, lecturer availability tracking, staff coordination, and student engagement through an intuitive, responsive interface.

## Overview

ACPulse is built as a comprehensive solution for intelligent campus management, providing role-based access, real-time status tracking, verification workflows, and seamless collaboration across multiple user types.

## Features

### Core Functionality
- **Role-Based Access Control**: Dedicated dashboards and workflows for Admin, Lecturer, Staff, and Student roles
- **Room Management**: Intelligent room allocation, occupancy tracking, and extension management
- **Lecturer Directory**: Searchable lecturer profiles with real-time availability status
- **User Verification**: Streamlined verification request workflow for lecturers and staff
- **Notification System**: Real-time notifications and updates across the platform
- **Profile Management**: Secure user profile updates and password management

### User Dashboards
- **Admin Dashboard**: System statistics, user management, and verification request handling
- **Lecturer Dashboard**: Profile management, availability status updates, and room bookings
- **Staff Dashboard**: Office status management and assignment tracking
- **Student Dashboard**: Room search, booking, and lecturer discovery

### Technical Excellence
- **Dark Mode Support**: Full light/dark theme implementation with system preference detection
- **Responsive Design**: Mobile-first approach with Tailwind CSS, fully functional on all screen sizes
- **Real-Time State Management**: Zustand store for efficient global state handling
- **Advanced Caching**: React Query with TanStack for optimized data fetching and caching
- **Form Management**: React Hook Form for robust form handling and validation
- **Component Library**: Reusable, well-documented UI components

## Tech Stack

### Frontend Framework
- **React 19.2.0** - Modern React with latest features
- **Vite 7.2** - Lightning-fast build tool and dev server
- **React Router DOM 6** - Client-side routing

### State & Data Management
- **Zustand 5.0.9** - Lightweight state management
- **TanStack React Query 5.90** - Powerful async state management and caching
- **Axios 1.13** - HTTP client with interceptor support

### UI & Styling
- **Tailwind CSS 3** - Utility-first CSS framework
- **Tailwind Forms** - Form component styling plugin
- **Lucide React 0.555** - Icon library with 555+ icons
- **Framer Motion 12.23** - Animation and motion library
- **CLSX 2.1.1** - Conditional className utility

### Form & Validation
- **React Hook Form 7** - Efficient form state management
- **Custom validators** - Application-specific validation rules

### Utilities
- **date-fns 4.1** - Modern date manipulation
- **Fuse.js 7.1** - Fuzzy search library
- **React Hot Toast** - Toast notifications

### Development
- **ESLint** - Code linting and quality assurance
- **PostCSS** - CSS processing and autoprefixing

## Project Structure

```
acpulse-frontend/
├── src/
│   ├── components/
│   │   ├── admin/              # Admin-specific components
│   │   ├── auth/               # Authentication forms
│   │   ├── common/             # Reusable UI components
│   │   ├── dashboard/          # Role-specific dashboards
│   │   ├── layout/             # Layout components
│   │   ├── lecturers/          # Lecturer-related components
│   │   ├── profile/            # User profile components
│   │   └── rooms/              # Room management components
│   ├── hooks/                  # Custom React hooks
│   ├── pages/                  # Page components
│   ├── routes/                 # Routing configuration
│   ├── services/               # API service layer
│   ├── store/                  # Zustand stores
│   ├── utils/                  # Helper functions and constants
│   ├── App.jsx                 # Root component
│   ├── index.css               # Global styles
│   └── main.jsx                # Entry point
├── public/                     # Static assets
├── index.html                  # HTML template
├── vite.config.js              # Vite configuration
├── tailwind.config.js          # Tailwind configuration
├── postcss.config.js           # PostCSS configuration
└── package.json                # Dependencies and scripts
```

## Getting Started

### Prerequisites
- Node.js 16+ and npm/yarn
- Backend API running on `http://localhost:8080/api`

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd acpulse-frontend
```

2. Install dependencies:
```bash
npm install
```

3. Configure environment variables:
```bash
cp .env.example .env
# Update VITE_API_BASE_URL if necessary
```

### Development

Start the development server:
```bash
npm run dev
```

The application will be available at `http://localhost:5173` (or next available port)

### Building

Build for production:
```bash
npm run build
```

Preview production build:
```bash
npm run preview
```

### Linting

Run ESLint to check code quality:
```bash
npm run lint
```

## Environment Configuration

Create a `.env` file in the root directory:

```env
VITE_API_BASE_URL=http://localhost:8080/api
VITE_APP_NAME=ACPulse
VITE_APP_VERSION=1.0.0
```

## Authentication Flow

1. Users access the application and are directed to the login page
2. Upon successful authentication, users are redirected to their role-specific dashboard
3. JWT tokens are stored securely and included in API requests via axios interceptors
4. Token refresh and logout functionality managed through the auth store

## State Management

### Auth Store (`useAuthStore`)
- Manages user authentication state and session
- Persists auth data to localStorage
- Provides login/logout actions

### UI Store (`useUIStore`)
- Manages theme (light/dark mode)
- Manages sidebar state
- Persists UI preferences to localStorage

## API Integration

The application communicates with a RESTful API through the axios client configured in `services/api.js`. All API calls are made through specialized service modules:

- `authService.js` - Authentication endpoints
- `userService.js` - User profile and verification
- `lecturerService.js` - Lecturer data and status
- `roomService.js` - Room management
- `adminService.js` - Admin operations
- `staffService.js` - Staff operations
- `notificationService.js` - Notifications
- `locationService.js` - Location hierarchy data

## Component Architecture

### Reusable Components
Located in `components/common/`, these components follow a consistent API:

- **Button** - Multi-variant button component
- **Input** - Text input with validation support
- **Card** - Container with customizable styling
- **Modal** - Dialog component with configurable actions
- **Avatar** - User avatar with fallback and status
- **Badge** - Status and category badges
- **Table** - Data table with search and sort
- **Pagination** - Page navigation
- **LoadingSpinner** - Loading indicator
- **Toast** - Toast notifications
- **GlobalSearch** - Application-wide search functionality

## Performance Optimizations

- **Code Splitting**: Route-based code splitting for faster initial load
- **Image Optimization**: Lazy loading for images
- **Caching Strategy**: React Query manages API response caching
- **CSS Optimization**: Tailwind purges unused styles in production
- **Debouncing**: Search and filter inputs debounced to reduce API calls

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Contributing Guidelines

1. Follow the existing code structure and naming conventions
2. Maintain component reusability and modularity
3. Use ESLint standards for code quality
4. Document complex logic with comments
5. Test all features before submitting changes

## Known Limitations & Future Enhancements

- TypeScript integration planned for improved type safety
- Unit and integration tests to be added
- Accessibility (a11y) improvements in progress
- Advanced filtering and reporting features planned

## Troubleshooting

### Port Already in Use
If port 5173 is already in use, Vite automatically tries the next available port.

### API Connection Issues
- Verify backend API is running on the configured URL
- Check network requests in browser DevTools
- Review console for detailed error messages

### Module Resolution Errors
Clear node_modules and reinstall:
```bash
rm -rf node_modules && npm install
```

### Build Failures
Ensure all environment variables are set correctly and all dependencies are installed.

## Support & Contact

For issues, feature requests, or technical questions, please contact the development team or open an issue in the project repository.

## License

© 2025 Adventist University of Central Africa. All rights reserved.

---

**Version**: 1.0.0  
**Last Updated**: December 2025
