import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { authApi, LoginRequest, LoginResponse, User, setupTokenExpiration, isTokenExpired, clearSession } from '../api';

interface AuthContextType {
  user: User | null;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
  loading: boolean;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const initAuth = async () => {
      const token = localStorage.getItem('authToken');
      if (token) {
        // Check if token is expired
        if (isTokenExpired(token)) {
          clearSession();
          setLoading(false);
          return;
        }
        
        try {
          // Set up automatic logout timer
          setupTokenExpiration(token);
          
          const userData = await authApi.getCurrentUser();
          setUser(userData);
        } catch (error) {
          clearSession();
        }
      }
      setLoading(false);
    };

    initAuth();
  }, []);

  const login = async (credentials: LoginRequest): Promise<void> => {
    try {
      const response: LoginResponse = await authApi.login(credentials);
      localStorage.setItem('authToken', response.token);
      
      const userData: User = {
        id: response.userId,
        username: response.username,
        email: response.email,
        tenantName: response.tenantName,
        tenantSubdomain: credentials.tenantSubdomain,
      };
      
      setUser(userData);
      localStorage.setItem('user', JSON.stringify(userData));
    } catch (error) {
      throw error;
    }
  };

  const logout = () => {
    clearSession();
    setUser(null);
  };

  const value: AuthContextType = {
    user,
    login,
    logout,
    loading,
    isAuthenticated: !!user,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};