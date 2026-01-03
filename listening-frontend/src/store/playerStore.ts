// 实现音频播放的逻辑，功能：
// 所有播放控制都通过 store
//play()           // 播放
//pause()          // 暂停
//togglePlay()     // 切换播放/暂停
//seekTo(time)     // 跳转进度
//setVolume(vol)   // 设置音量



import { create } from 'zustand';
import type { PlayerState, Episode, SubtitleItem } from '@/types';

//设置插件
interface PlayerStore extends PlayerState {
  // Actions
  setCurrentEpisode: (episode: Episode | null) => void;
  setIsPlaying: (playing: boolean) => void;
  setCurrentTime: (time: number) => void;
  setDuration: (duration: number) => void;
  setVolume: (volume: number) => void;
  toggleSubtitles: () => void;
  setCurrentSubtitle: (subtitle: string) => void;
  
  // Audio element reference
  audioElement: HTMLAudioElement | null;
  setAudioElement: (element: HTMLAudioElement | null) => void;
  
  // Subtitles
  subtitles: SubtitleItem[];
  subtitlesLoaded: boolean;  // ← 添加这行：标记字幕是否已加载
  setSubtitles: (subtitles: SubtitleItem[]) => void;
  clearSubtitles: () => void;  // ← 添加这行：清空字幕的方法
  
  // Player controls
  play: () => void;
  pause: () => void;
  seekTo: (time: number) => void;
  togglePlay: () => void;
}

//使用了 Zustand 状态管理库的 create() 创建，以下所有方法/ 变量都属于全局状态，任何组件（首页，播放页，章节页，浮窗...）都可以通过 usePlayerStore() 访问
export const usePlayerStore = create<PlayerStore>((set, get) => ({
  // Initial state
  currentEpisode: null,
  isPlaying: false,
  currentTime: 0,
  duration: 0,
  volume: 0.8,
  showSubtitles: true,
  currentSubtitle: '',
  audioElement: null,
  subtitles: [],
  subtitlesLoaded: false, // 标记字幕已加载

  // Actions
  setCurrentEpisode: (episode) => set({ currentEpisode: episode }),
  setIsPlaying: (playing) => set({ isPlaying: playing }),
  setCurrentTime: (time) => {
    set({ currentTime: time });
    
    // Update current subtitle based on time
    const { subtitles } = get();
    const currentSubtitle = subtitles.find(
      (sub) => time >= sub.startTime && time <= sub.endTime
    );
    set({ currentSubtitle: currentSubtitle?.text || '' });
  },

  setDuration: (duration) => set({ duration }),

  setVolume: (volume) => {
    set({ volume });
    const { audioElement } = get();
    if (audioElement) {
      audioElement.volume = volume;
    }
  },

  //控制字幕的显示与隐藏
  toggleSubtitles: () => set((state) => ({ showSubtitles: !state.showSubtitles })),
  //设置当前字幕
  setCurrentSubtitle: (subtitle) => set({ currentSubtitle: subtitle }),

  //设置音频元素
  /*
1. 创建 <audio> 元素
   ↓
2. 通过 setAudioElement(element) 存到 store
   ↓
3. 其他组件从 store 读取 audioElement
   ↓
4. 调用 audioElement.play() / pause() 等方法
  */
  setAudioElement: (element) => {
    console.log('🔧 Store: Setting audio element:', element);
    set({ audioElement: element });
    console.log('🔧 Store: Audio element set, current state:', get().audioElement);
  },
  //设置字幕列表
  setSubtitles: (subtitles) => set({ 
    subtitles,
    subtitlesLoaded: subtitles.length > 0  // ← 修改这行：有字幕时标记为已加载
  }),
  
  // ← 添加清空字幕方法，目的是清除当前字幕，如未加载进来前，字幕是“暂无字幕”
  clearSubtitles: () => set({ 
    subtitles: [], 
    subtitlesLoaded: false,
    currentSubtitle: ''
  }),

  // Player controls
  play: () => {
    const { audioElement } = get();
    console.log('🎵 Store play called, audioElement:', audioElement);
    if (audioElement) {
      audioElement.play()
        .then(() => {
          console.log('✅ Audio play successful');
          set({ isPlaying: true });
        })
        .catch((error) => {
          console.error('❌ Audio play failed:', error);
          set({ isPlaying: false });
        });
    } else {
      console.error('❌ No audio element found in store');
    }
  },
  pause: () => {
    const { audioElement } = get();
    if (audioElement) {
      audioElement.pause();
      set({ isPlaying: false });
    }
  },
  //跳转播放位置
  seekTo: (time) => {
    const { audioElement } = get();
    if (audioElement) {
      audioElement.currentTime = time;
      set({ currentTime: time });
    }
  },
  //切换播放/暂停
  togglePlay: () => {
    const { isPlaying, play, pause } = get();
    if (isPlaying) {
      pause();
    } else {
      play();
    }
  },
}));
