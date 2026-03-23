import { useAuthStore } from '../store';

const useAuth = () => {
  const { user, isAuthenticated, isLoading, error, login, register, logout, clearError } = useAuthStore();

  return {
    user,
    isAuthenticated,
    isLoading,
    error,
    login,
    register,
    logout,
    clearError,
  };
};

export default useAuth;