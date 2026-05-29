import { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import type { AuthResponse } from '../types/auth.types';
import { STORAGE_KEYS } from '../api/client/apiConfig';

interface AuthContextType {
  user: AuthResponse['user'] | null;
  token: string | null;
  login: (credentials: { email: string; password: string }) => Promise<void>;
  register: (credentials: { email: string; password: string; displayName: string }) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<AuthResponse['user'] | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const storedToken = localStorage.getItem(STORAGE_KEYS.AUTH_TOKEN);
    const storedUser = localStorage.getItem(STORAGE_KEYS.AUTH_USER);

    if (storedToken && storedUser) {
      try {
        setToken(storedToken);
        setUser(JSON.parse(storedUser));
      } catch {
        localStorage.removeItem(STORAGE_KEYS.AUTH_TOKEN);
        localStorage.removeItem(STORAGE_KEYS.AUTH_USER);
      }
    }
    
    setIsLoading(false);
  }, []);

  const login = async (credentials: { email: string; password: string }) => {
    setIsLoading(true);
    await new Promise(resolve => setTimeout(resolve, 500));
    
    const mockUser: AuthResponse['user'] = {
      id: 'user-' + Date.now(),
      email: credentials.email,
      displayName: credentials.email.split('@')[0],
      role: 'parent',
      hasCompletedOnboarding: false,
    };
    
    const mockToken = 'mock-token-' + Date.now();
    
    setToken(mockToken);
    setUser(mockUser);
    localStorage.setItem(STORAGE_KEYS.AUTH_TOKEN, mockToken);
    localStorage.setItem(STORAGE_KEYS.AUTH_USER, JSON.stringify(mockUser));
    setIsLoading(false);
  };

  const register = async (credentials: { email: string; password: string; displayName: string }) => {
    setIsLoading(true);
    await new Promise(resolve => setTimeout(resolve, 500));
    
    const mockUser: AuthResponse['user'] = {
      id: 'user-' + Date.now(),
      email: credentials.email,
      displayName: credentials.displayName,
      role: 'parent',
      hasCompletedOnboarding: false,
    };
    
    const mockToken = 'mock-token-' + Date.now();
    
    setToken(mockToken);
    setUser(mockUser);
    localStorage.setItem(STORAGE_KEYS.AUTH_TOKEN, mockToken);
    localStorage.setItem(STORAGE_KEYS.AUTH_USER, JSON.stringify(mockUser));
    setIsLoading(false);
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    localStorage.removeItem(STORAGE_KEYS.AUTH_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.AUTH_USER);
  };

  const value: AuthContextType = {
    user,
    token,
    login,
    register,
    logout,
    isAuthenticated: !!token && !!user,
    isLoading,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
