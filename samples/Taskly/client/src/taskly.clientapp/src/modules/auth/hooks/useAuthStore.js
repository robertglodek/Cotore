import useAuthStore from '../states/authState';

export default function useAuth() {
  return useAuthStore((state) => ({
    isLoggedIn: state.isLoggedIn,
    user: state.user,
    login: state.login,
    logout: state.logout,
  }));
}