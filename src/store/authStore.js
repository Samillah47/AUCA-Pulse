import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export const useAuthStore = create(
  persist(
    (set) => ({
      user: null,
      token: null,
      isAuthenticated: false,
      isOtpPending: false, // To track if the UI should show the OTP screen
      otpEmail: '', // To store the email while OTP is being verified

      // --- ACTIONS ---

      /**
       * Sets the state to indicate that OTP verification is required.
       * @param {string} email - The email to which the OTP was sent.
       */
      loginOtpPending: (email) => {
        set({
          isOtpPending: true,
          otpEmail: email,
          isAuthenticated: false, // Not fully authenticated yet
        });
      },

      /**
       * Sets user and token after successful login (or OTP verification).
       * @param {object} authResponse - The final auth response from the backend.
       */
      loginSuccess: (authResponse) => {
        const { token, ...userData } = authResponse;
        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify(userData));
        set({
          user: userData,
          token: token,
          isAuthenticated: true,
          isOtpPending: false, // Clear OTP pending state
          otpEmail: '',
        });
      },

      // Clear all auth state on logout
      logout: () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        set({
          user: null,
          token: null,
          isAuthenticated: false,
          isOtpPending: false,
          otpEmail: '',
        });
      },

      // Update user information (e.g., after profile update)
      setUser: (userData) => {
        const currentUser = JSON.parse(localStorage.getItem('user'));
        const updatedUser = { ...currentUser, ...userData };
        localStorage.setItem('user', JSON.stringify(updatedUser));
        set({ user: updatedUser });
      },
    }),
    {
      name: 'auth-storage', // name of the item in the storage (must be unique)
      getStorage: () => localStorage, // explicitly use localStorage
    }
  )
);

// Selector to get the current user
export const useCurrentUser = () => useAuthStore((state) => state.user);

// Selector to get the authentication status
export const useIsAuthenticated = () => useAuthStore((state) => state.isAuthenticated);

export default useAuthStore;

