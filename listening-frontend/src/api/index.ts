import axios from 'axios';
import type { AxiosResponse } from 'axios';
import type { ApiResponse, LoginRequest, LoginResponse, Episode, EpisodeDetail } from '@/types';
import { storage } from '@/utils';

// 创建axios实例
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',  // 使用环境变量
  timeout: 10000,
});

// 请求拦截器 - 添加token
api.interceptors.request.use((config) => {
  const token = storage.get('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 响应拦截器 - 处理错误
api.interceptors.response.use(
  (response: AxiosResponse<ApiResponse>) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token过期，清除本地存储并跳转到首页
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      // 跳转到首页而不是不存在的 /login 路由
      window.location.href = '/';
    }
    return Promise.reject(error);
  }
);

// 认证API
export const authApi = {
  login: (data: LoginRequest): Promise<AxiosResponse<ApiResponse<LoginResponse>>> =>
    api.post('/Auth/login', data),
  
  logout: (): Promise<AxiosResponse<ApiResponse>> =>
    api.post('/Auth/logout'),
};

// 听力内容API
export const listeningApi = {
  getEpisodes: (): Promise<AxiosResponse<ApiResponse<Episode[]>>> =>
    api.get('/listening/episodes'),
  
  getEpisodeDetail: (id: string): Promise<AxiosResponse<ApiResponse<EpisodeDetail>>> =>
    api.get(`/listening/episodes/${id}`),
};

// 文件API
export const fileApi = {
  getFileInfo: (id: string): Promise<AxiosResponse<ApiResponse<any>>> =>
    api.get(`/file/info/${id}`),
  
  downloadFile: (id: string): string =>
    `/api/file/download/${id}`,
};


// 管理员 API
export const adminApi = {
  // 获取所有音频（管理员视角）
  getEpisodes: (): Promise<AxiosResponse<ApiResponse<any[]>>> =>
    api.get('/episodes'),  // 注意：这里调用 Admin.WebAPI 的接口
  
  // 上传新音频
  uploadEpisode: (formData: FormData, onProgress?: (percent: number) => void) =>
    api.post('/episodes', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (progressEvent) => {
        if (onProgress && progressEvent.total) {
          const percent = Math.round((progressEvent.loaded * 100) / progressEvent.total);
          onProgress(percent);
        }
      },
    }),
  
  // 删除音频
  deleteEpisode: (id: string): Promise<AxiosResponse<ApiResponse>> =>
    api.delete(`/episodes/${id}`),
  
  // 更新音频信息
  updateEpisode: (id: string, data: any): Promise<AxiosResponse<ApiResponse>> =>
    api.put(`/episodes/${id}`, data),
};

export default api;
