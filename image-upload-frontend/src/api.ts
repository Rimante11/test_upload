import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5119/api';

const api = axios.create({
  baseURL: API_BASE_URL,
});

// Add request interceptor to include auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
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
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');
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
    return response.data;
  },

  async getCurrentUser(): Promise<User> {
    const response = await api.get('/auth/me');
    return response.data;
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

export default api;