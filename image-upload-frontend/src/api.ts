import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5119/api';

// Token expiration tracking
let tokenExpirationTimer: NodeJS.Timeout | null = null;

// Function to decode JWT and check expiration
const isTokenExpired = (token: string): boolean => {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const expirationTime = payload.exp * 1000; // Convert to milliseconds
    return Date.now() >= expirationTime;
  } catch {
    return true;
  }
};

// Function to get time until token expires (in milliseconds)
const getTokenExpirationTime = (token: string): number | null => {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const expirationTime = payload.exp * 1000; // Convert to milliseconds
    return expirationTime - Date.now();
  } catch {
    return null;
  }
};

// Function to clear session
const clearSession = () => {
  localStorage.removeItem('authToken');
  localStorage.removeItem('user');
  if (tokenExpirationTimer) {
    clearTimeout(tokenExpirationTimer);
    tokenExpirationTimer = null;
  }
};

// Function to set up auto-logout on token expiration
const setupTokenExpiration = (token: string) => {
  if (tokenExpirationTimer) {
    clearTimeout(tokenExpirationTimer);
  }

  const timeUntilExpiration = getTokenExpirationTime(token);
  if (timeUntilExpiration && timeUntilExpiration > 0) {
    tokenExpirationTimer = setTimeout(() => {
      alert('Your session has expired. Please log in again.');
      clearSession();
      window.location.href = '/login';
    }, timeUntilExpiration);
  }
};

const api = axios.create({
  baseURL: API_BASE_URL,
});

// Add request interceptor to include auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      // Check if token is expired before making request
      if (isTokenExpired(token)) {
        clearSession();
        window.location.href = '/login';
        return Promise.reject(new Error('Token expired'));
      }
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Add response interceptor to handle auth errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      clearSession();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export interface LoginRequest {
  email: string;
  password: string;
  tenantSubdomain: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  email: string;
  userId: number;
  tenantName: string;
}

export interface User {
  id: number;
  username: string;
  email: string;
  tenantName: string;
  tenantSubdomain: string;
}

export interface ImageUploadResponse {
  id: number;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
  width: number;
  height: number;
  description?: string;
  tags?: string;
  uploadedAt: string;
  originalImageUrl: string;
  thumbnailUrl: string;
}

export interface ImageListResponse {
  id: number;
  originalFileName: string;
  description?: string;
  uploadedAt: string;
  thumbnailUrl: string;
}

export const authApi = {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await api.post('/auth/login', credentials);
    const data = response.data;
    
    // Set up automatic logout when token expires
    if (data.token) {
      setupTokenExpiration(data.token);
    }
    
    return data;
  },

  async getCurrentUser(): Promise<User> {
    const response = await api.get('/auth/me');
    return response.data;
  },
  
  logout() {
    clearSession();
    window.location.href = '/login';
  },
};

export const imageApi = {
  async uploadImage(file: File, description?: string, tags?: string): Promise<ImageUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    if (description) formData.append('description', description);
    if (tags) formData.append('tags', tags);

    const response = await api.post('/images/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  async getImages(): Promise<ImageListResponse[]> {
    const response = await api.get('/images');
    return response.data;
  },

  async getImageById(id: number): Promise<ImageUploadResponse> {
    const response = await api.get(`/images/${id}`);
    return response.data;
  },

  async deleteImage(id: number): Promise<void> {
    await api.delete(`/images/${id}`);
  },
};

export { setupTokenExpiration, clearSession, isTokenExpired };
export default api;