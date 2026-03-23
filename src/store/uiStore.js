import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { useEffect } from 'react';

// Function to check the system's preferred color scheme
const prefersDarkMode = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;

export const useUIStore = create(
  persist(
    (set, get) => ({
      // --- STATE ---
      isSidebarCollapsed: false,
      theme: prefersDarkMode ? 'dark' : 'light', // Default to system preference
      isGlobalSearchOpen: false, // For the global search modal

      // --- ACTIONS ---

      // Toggle the sidebar's collapsed state
      toggleSidebar: () => set((state) => ({ isSidebarCollapsed: !state.isSidebarCollapsed })),

      // Set a specific state for the sidebar
      setSidebarCollapsed: (isCollapsed) => set({ isSidebarCollapsed: isCollapsed }),

      // Toggle the theme between 'light' and 'dark'
      toggleTheme: () => {
        const newTheme = get().theme === 'light' ? 'dark' : 'light';
        set({ theme: newTheme });
      },

      // Set a specific theme
      setTheme: (theme) => set({ theme }),

      // Global Search Modal Actions
      openGlobalSearch: () => set({ isGlobalSearchOpen: true }),
      closeGlobalSearch: () => set({ isGlobalSearchOpen: false }),
      toggleGlobalSearch: () => set((state) => ({ isGlobalSearchOpen: !state.isGlobalSearchOpen })),
    }),
    {
      name: 'ui-storage', // name of the item in the storage (must be unique)
      // Only persist the 'theme' property
      partialize: (state) => ({ theme: state.theme, isSidebarCollapsed: state.isSidebarCollapsed }),
    }
  )
);

// Custom hook to apply the theme to the DOM
export const useTheme = () => {
  const theme = useUIStore((state) => state.theme);

  useEffect(() => {
    // Apply the theme class to the root HTML element
    // This is what Tailwind's 'darkMode: "class"' uses
    const root = document.documentElement;
    root.classList.remove('light', 'dark');
    root.classList.add(theme);
  }, [theme]); // Rerun the effect only when the theme changes

  return theme;
};

export default useUIStore;
