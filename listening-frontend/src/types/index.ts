// API响应类型
export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data?: T;
  errorCode?: string;
}

// 用户相关类型
export interface User {
  id: string;
  userName: string;
  roles: string[];
}

export interface LoginRequest {
  userNameOrEmail: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

// 音频文件相关类型
export interface Episode {
  id: string;
  title: string;
  name: string;        // Add this for compatibility
  subtitle?: string;   // Add this property
  description?: string;
  audioUrl?: string;
  coverUrl?: string;   // Change from coverImageUrl
  coverImageUrl?: string;
  subtitleUrl?: string;
  duration: number;
  durationInSecond: number;  // Add this property
  durationDisplay: string;
  durationTimeFormat: string;
  createTime: string;
}

export interface EpisodeDetail extends Episode {
  sentences: Sentence[];
}

export interface Sentence {
  id: string;
  content: string;
  startTime: string; // TimeSpan格式
  endTime: string;   // TimeSpan格式
}

// 播放器状态
export interface PlayerState {
  currentEpisode: Episode | null;
  isPlaying: boolean;
  currentTime: number;
  duration: number;
  volume: number;
  showSubtitles: boolean;
  currentSubtitle: string;
}

// 字幕解析
export interface SubtitleItem {
  index: number;
  startTime: number; // 秒
  endTime: number;   // 秒
  text: string;
}
