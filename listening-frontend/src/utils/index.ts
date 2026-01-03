import type { SubtitleItem } from '@/types';

// 时间格式化工具
export const formatTime = (seconds: number): string => {
  // 🔥 添加数值验证
  if (!seconds || isNaN(seconds) || seconds < 0) {
    return '00:00';
  }
  
  const mins = Math.floor(seconds / 60);
  const secs = Math.floor(seconds % 60);
  return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
};

// 解析SRT字幕文件 - 改进版，兼容不同换行符
export const parseSRT = (srtContent: string): SubtitleItem[] => {
  const subtitles: SubtitleItem[] = [];
  
  // 先统一换行符为 \n，兼容 Windows (\r\n) 和 Unix (\n)
  const normalizedContent = srtContent.replace(/\r\n/g, '\n').replace(/\r/g, '\n');
  
  // 按双换行符分割字幕块（支持多个连续换行）
  const blocks = normalizedContent.trim().split(/\n\n+/);

  blocks.forEach((block) => {
    const lines = block.trim().split('\n');
    if (lines.length >= 3) {
      const index = parseInt(lines[0]);
      const timeMatch = lines[1].match(/(\d{2}):(\d{2}):(\d{2}),(\d{3}) --> (\d{2}):(\d{2}):(\d{2}),(\d{3})/);
      
      if (timeMatch) {
        const startTime = 
          parseInt(timeMatch[1]) * 3600 + 
          parseInt(timeMatch[2]) * 60 + 
          parseInt(timeMatch[3]) + 
          parseInt(timeMatch[4]) / 1000;
          
        const endTime = 
          parseInt(timeMatch[5]) * 3600 + 
          parseInt(timeMatch[6]) * 60 + 
          parseInt(timeMatch[7]) + 
          parseInt(timeMatch[8]) / 1000;

        // 获取字幕文本（从第3行开始的所有内容），并清理首尾空格
        const text = lines.slice(2).join('\n').trim();

        subtitles.push({
          index,
          startTime,
          endTime,
          text,
        });
      }
    }
  });

  return subtitles;
};

// 从URL获取文件名
export const getFileNameFromUrl = (url: string): string => {
  return url.split('/').pop() || 'Unknown';
};

// 本地存储工具
export const storage = {
  get: (key: string) => {
    try {
      const item = localStorage.getItem(key);
      return item ? JSON.parse(item) : null;
    } catch {
      return null;
    }
  },
  set: (key: string, value: any) => {
    try {
      localStorage.setItem(key, JSON.stringify(value));
    } catch (error) {
      console.error('Failed to save to localStorage:', error);
    }
  },
  remove: (key: string) => {
    localStorage.removeItem(key);
  },
};
