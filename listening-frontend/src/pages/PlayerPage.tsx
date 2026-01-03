import React, { useState, useEffect, useRef, useCallback, useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Card, 
  Button, 
  Slider, 
  Typography, 
  Row, 
  Col, 
  Spin, 
  Alert,
  Switch,
  Tooltip
} from 'antd';
import {
  PlayCircleOutlined,
  PauseCircleOutlined,
  SoundOutlined,
  FontSizeOutlined,
  DownloadOutlined,
  ArrowLeftOutlined
} from '@ant-design/icons';
import { usePlayerStore } from '@/store/playerStore';
import { listeningApi } from '@/api';
import { formatTime, parseSRT } from '@/utils';
import type { EpisodeDetail } from '@/types';

//从 Ant Design 的 Typography 组件中提取 Title 和 Text 子组件
//如：<Title level={3}>{episode.title}</Title>  // 显示音频标题
const { Title, Text } = Typography;


// React.FC 是 React.FunctionComponent 的简写，告诉 TypeScript：这是一个 React 函数组件
const PlayerPage: React.FC = () => {

  // 🔧 修复：正确的URL转换函数
  const convertToProxyUrl = (originalUrl?: string) => {
    if (!originalUrl) return '';
    
    console.log('🔧 Converting URL:', originalUrl);
    
    // 如果已经是代理URL，直接返回
    if (originalUrl.startsWith('/api/file/')) {
      const result = originalUrl.replace('/download', '/content');
      console.log('🔧 Already proxy URL, converted to:', result);
      return result;
    }
    
    // 处理完整的后端URL：http(s)://localhost:7292/api/File/{id}/download
    // 这里可以使用https和http两种协议
    if (originalUrl.includes('://localhost:7292/api/File/')) {
      const result = originalUrl
        .replace(/https?:\/\/localhost:7292\/api\/File\//i, '/api/file/')// ← 使用正则匹配 不区分大小写（File 或 file 都能匹配） 同时匹配 http:// 和 https://
        .replace('/download', '/content');
      console.log('🔧 Full URL converted to:', result);
      return result;
    }
    
    // 处理相对路径：/api/File/{id}/download  
    if (originalUrl.startsWith('/api/File/')) {
      const result = originalUrl
        .replace('/api/File/', '/api/file/')
        .replace('/download', '/content');
      console.log('🔧 Relative URL converted to:', result);
      return result;
    }
    
    console.log('🔧 URL not converted:', originalUrl);
    return originalUrl;
  };
  

// react 函数组件的常用钩子函数
// 钩子函数（Hook Function）是一种在特定事件（如点击..）发生时被自动调用的函数
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const audioRef = useRef<HTMLAudioElement>(null);
  
  const [episode, setEpisode] = useState<EpisodeDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);



  const {
    isPlaying,
    currentTime,
    duration,
    volume,
    showSubtitles,
    currentSubtitle,
    subtitlesLoaded,  // ← 标记当前字幕是否已加载
    toggleSubtitles,
    setSubtitles,
    clearSubtitles,   // ← 清除当前字幕，如未加载进来前，字幕是"暂无字幕"
    setAudioElement,
    play,
    pause,
    seekTo,
    setVolume
  } = usePlayerStore();




  //最后的[]表示当id发生变化时，执行fetchEpisodeDetail
  //当 id 更新（比如用户点了另一个节目）时，再运行一次。
  useEffect(() => {
    if (id) {
      fetchEpisodeDetail(id);
    }
  }, [id]);



 // 🔧 使用回调ref来确保获得audio元素
 {/*
  audioElement：参数名 HTMLAudioElement | null：联合类型，表示可能是音频元素或null
  audioRef.current：访问ref对象的current属性，获取<audio>的DOM元素对象，这是React ref的标准用法
  The HTMLAudioElement interface provides access to the properties of audio elements, as well as methods to manipulate them.
  
  HTML5 Audio API 的标准事件监听器：
  事件触发：当音频元素状态变化时，浏览器自动触发对应事件
  事件循环：事件进入JavaScript事件队列
  回调执行：注册的处理函数被调用
  状态同步：处理函数更新应用状态（这里是store）
  
  useCallback 是 React 的一个 Hook。它的主要作用是 记忆化（memoize）一个回调函数。
  记忆化: 意味着 useCallback 会记住（缓存）它接收到的第一个参数（一个函数），只有当它的依赖项数组（第二个参数）中的值发生变化时，才会重新创建这个函数。
  为什么需要 useCallback?: 在 React 函数组件中，每次组件重新渲染时，组件内部的函数都会被重新创建，useCallback 可以避免这些不必要的性能开销。
  React 的 ref 回调，会在 DOM 元素创建时自动被调用，DOM 元素刚创建，立即绑定事件监听器



  */}

  const setAudioRef = useCallback((audioElement: HTMLAudioElement | null) => {
    audioRef.current = audioElement;
    
    //立即设置到store，将DOM元素保存到全局状态中，供其他组件使用，store就是全局状态，在playerStore.ts中有设置setAudioElement方法
    if (audioElement && episode) {
      setAudioElement(audioElement);
      
      // 定义事件处理函数
      const handleTimeUpdate = () => {
        usePlayerStore.getState().setCurrentTime(audioElement.currentTime);
      };
      
      const handleDurationChange = () => {
        usePlayerStore.getState().setDuration(audioElement.duration);
      };

      const handleLoadedMetadata = () => {
        usePlayerStore.getState().setDuration(audioElement.duration);
      };

      const handleVolumeChange = () => {
        usePlayerStore.getState().setVolume(audioElement.volume);
      };
      
      const handlePlay = () => usePlayerStore.getState().setIsPlaying(true);
      const handlePause = () => usePlayerStore.getState().setIsPlaying(false);
      const handleEnded = () => usePlayerStore.getState().setIsPlaying(false);

      const handleError = (e: Event) => {
        const target = e.target as HTMLAudioElement;
        console.error('Audio error:', target.error);
      };

      // 添加事件监听器
      audioElement.addEventListener('timeupdate', handleTimeUpdate);
      audioElement.addEventListener('durationchange', handleDurationChange);
      audioElement.addEventListener('loadedmetadata', handleLoadedMetadata);
      audioElement.addEventListener('volumechange', handleVolumeChange);
      audioElement.addEventListener('play', handlePlay);
      audioElement.addEventListener('pause', handlePause);
      audioElement.addEventListener('ended', handleEnded);
      audioElement.addEventListener('error', handleError);
      
      // 清理函数
      return () => {
        audioElement.removeEventListener('timeupdate', handleTimeUpdate);
        audioElement.removeEventListener('durationchange', handleDurationChange);
        audioElement.removeEventListener('loadedmetadata', handleLoadedMetadata);
        audioElement.removeEventListener('volumechange', handleVolumeChange);
        audioElement.removeEventListener('play', handlePlay);
        audioElement.removeEventListener('pause', handlePause);
        audioElement.removeEventListener('ended', handleEnded);
        audioElement.removeEventListener('error', handleError);
      };
    }
  }, [episode, setAudioElement]);
  
  // 当 episode 变化时重新设置 audio，并清理旧的事件监听器
  useEffect(() => {
    if (audioRef.current && episode) {
      const cleanup = setAudioRef(audioRef.current);
      return () => {
        if (cleanup) cleanup();
      };
    }
  }, [episode, setAudioRef]);


  // 单独处理音量变化
  useEffect(() => {
    if (audioRef.current) {
      audioRef.current.volume = volume;
    }
  }, [volume]);

  // 获取剧集详情
  const fetchEpisodeDetail = async (episodeId: string) => {
    try {
      setLoading(true);
      setError(null);
      clearSubtitles();  // ← 添加这行：加载新剧集时先清空字幕
      
      const response = await listeningApi.getEpisodeDetail(episodeId);
      
      if (response.data.success) {
        const episodeData = response.data.data;
        if (episodeData) {
          setEpisode(episodeData);
          
          // 加载字幕文件
          if (episodeData.subtitleUrl) {
            try {
              const subtitleUrl = convertToProxyUrl(episodeData.subtitleUrl);
              const subtitleResponse = await fetch(subtitleUrl);
              
              if (subtitleResponse.ok) {
                const srtContent = await subtitleResponse.text();
                const parsedSubtitles = parseSRT(srtContent);
                setSubtitles(parsedSubtitles);
              }
            } catch (err) {
              console.warn('加载字幕失败:', err);
            }
          }
        }
      } else {
        setError(response.data.message || '获取音频详情失败');
      }
    } catch (err) {
      console.error('获取音频详情出错:', err);
      setError('网络错误，请稍后重试');
    } finally {
      setLoading(false);
    }
  };

  const handlePlayPause = () => {
    if (isPlaying) {
      pause();
    } else {
      play();
    }
  };



  // 直接调用 store 的 seekTo，等价于下面的函数
  const handleSeek = (value: number) => {
    seekTo(value);
  };
  /*const handleSeek = (value: number) => {
    if (audioRef.current) {
      audioRef.current.currentTime = value;
    }
  };*/


  const handleVolumeChange = (value: number) => {
    setVolume(value / 100);
  };

  const handleDownload = () => {
    if (episode?.audioUrl) {
      const downloadUrl = episode.audioUrl;
      //后端先定向到download接口，然后接口定向到aws下载接口
      window.open(downloadUrl, '_blank');
    }
  };




  //使用memoized URL避免重复转换，即保持当前的URL不变
  const audioSrc = useMemo(() => {
    if (!episode?.audioUrl) return '';
    return convertToProxyUrl(episode.audioUrl);
  }, [episode?.audioUrl]);

  const coverSrc = useMemo(() => {
    if (!episode?.coverImageUrl) return '/default-cover.jpg';
    return convertToProxyUrl(episode.coverImageUrl);
  }, [episode?.coverImageUrl]);



  if (loading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
        <p>正在加载音频...</p>
      </div>
    );
  }

  if (error) {
    return (
      <Alert
        message="加载失败"
        description={error}
        type="error"
        showIcon
        action={
          <Button onClick={() => navigate('/')}>
            返回首页
          </Button>
        }
      />
    );
  }

  if (!episode) {
    return (
      <Alert
        message="音频不存在"
        type="warning"
        showIcon
        action={
          <Button onClick={() => navigate('/')}>
            返回首页
          </Button>
        }
      />
    );
  }

  return (
    <div style={{ maxWidth: 800, margin: '0 auto' }}>
      <Button 
        icon={<ArrowLeftOutlined />} 
        onClick={() => navigate('/')}
        style={{ marginBottom: 16 }}
      >
        返回
      </Button>
      
      <Card>
        <Row gutter={[24, 24]}>
          <Col xs={24} md={8}>
            <img
              src={coverSrc}
              alt={episode.title}
              style={{ width: '100%', borderRadius: 8 }}
              onError={(e) => {
                const target = e.target as HTMLImageElement;
                target.src = '/default-cover.jpg';
              }}
            />
          </Col>
          
          <Col xs={24} md={16}>
            <Title level={3}>{episode.title}</Title>
            <Text type="secondary">{episode.description}</Text>
            
          {/*
            DOM 全称是 Document Object Model，翻译过来就是“文档对象模型”。
            浏览器在渲染网页的时候，会把 HTML 代码 转换成一个 树状结构，每个标签都会变成一个对象（DOM 节点）。
            这些对象就是 DOM 元素，它们可以在 JavaScript 里直接被操作。

            下面放的就是一个 <audio> 标签 👇
            ref 就是用来拿到的就是 对应的 <audio>的 DOM 元素对象
          */}
            <div style={{ marginTop: 24 }}>
              <audio 
                key={episode?.id}  // ← 使用 episode.id 作为 key
                ref={setAudioRef}  // ← 这里通过ref拿到的是<audio>的DOM元素对象的代表引用
                src={audioSrc}     // ← 这里是传入音频文件，即音频DOM元素对象
                preload="metadata"
                crossOrigin="anonymous"
              />
              
              {/* 播放控制 */}
              <div style={{ textAlign: 'center', marginBottom: 16 }}>
                <Button
                  type="primary"
                  size="large"
                  shape="circle"
                  icon={isPlaying ? <PauseCircleOutlined /> : <PlayCircleOutlined />}
                  onClick={handlePlayPause}
                  style={{ width: 64, height: 64, fontSize: 24 }}
                />
              </div>
              
              {/* 进度条 */}
              <div style={{ marginBottom: 16 }}>
                <Slider
                  value={currentTime}
                  max={duration}
                  onChange={handleSeek}// ← 拖动中只更新临时状态，不触渲染
                  tooltip={{ formatter: (value) => formatTime(value || 0) }}
                  disabled={!duration} // ← 修改：如果总时长为0，即没有加载进音频，禁用进度条
                />
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text type="secondary">{formatTime(currentTime)}</Text>
                  <Text type="secondary">{formatTime(duration)}</Text>
                </div>
              </div>
              
              {/* 音量和其他控制 */}
              <Row gutter={16} align="middle">
                <Col flex="auto">
                  <SoundOutlined />
                  <Slider
                    style={{ marginLeft: 8 }}
                    value={volume * 100}
                    onChange={handleVolumeChange}
                    tooltip={{ formatter: (value) => `${value}%` }}
                  />
                </Col>
                <Col>
                  <Tooltip title="字幕开关">
                    <Switch
                      checked={showSubtitles}
                      onChange={toggleSubtitles}
                      checkedChildren={<FontSizeOutlined />}
                      unCheckedChildren={<FontSizeOutlined />}
                    />
                  </Tooltip>
                </Col>
                <Col>
                  <Tooltip title="下载音频">
                    <Button
                      icon={<DownloadOutlined />}
                      onClick={handleDownload}
                    >
                      下载
                    </Button>
                  </Tooltip>
                </Col>
              </Row>
              
              {/* 字幕显示 */}
              {showSubtitles && (
                <div style={{ 
                  marginTop: 16, 
                  padding: 16, 
                  backgroundColor: '#f5f5f5', 
                  borderRadius: 8,
                  minHeight: 60,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center'
                }}>
                  <Text style={{ fontSize: 16, textAlign: 'center' }}>
                    {/* ← 修改这行：根据字幕加载状态显示不同内容 */}
                    {subtitlesLoaded 
                      ? (currentSubtitle || '') 
                      : '暂无字幕'}
                  </Text>
                </div>
              )}
            </div>
          </Col>
        </Row>
      </Card>
    </div>
  );
};

export default PlayerPage;