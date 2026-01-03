import React, { useMemo } from 'react';  // ← 添加 useMemo 导入
import { Card, Button, Tag, Tooltip } from 'antd';
import { PlayCircleOutlined, ClockCircleOutlined, SoundOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { usePlayerStore } from '@/store/playerStore';
import { formatTime } from '@/utils';
import type { Episode } from '@/types';

interface EpisodeCardProps {
  episode: Episode;
}

const EpisodeCard: React.FC<EpisodeCardProps> = ({ episode }) => {
  const navigate = useNavigate();
  const { setCurrentEpisode } = usePlayerStore();

  // ✅ 复制 PlayerPage 的 convertToProxyUrl 函数
  const convertToProxyUrl = (originalUrl?: string) => {
    if (!originalUrl) return '';
    
    console.log('🔧 [EpisodeCard] Converting URL:', originalUrl);
    
    // 如果已经是代理URL，直接返回
    if (originalUrl.startsWith('/api/file/')) {
      const result = originalUrl.replace('/download', '/content');
      console.log('🔧 Already proxy URL, converted to:', result);
      return result;
    }
    
    // 处理完整的后端URL：http(s)://localhost:7292/api/File/{id}/download
    if (originalUrl.includes('://localhost:7292/api/File/')) {
      const result = originalUrl
        .replace(/https?:\/\/localhost:7292\/api\/File\//i, '/api/file/')
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

  // ✅ 使用 useMemo 缓存转换后的 URL（完全复制 PlayerPage 的方式）
  const coverSrc = useMemo(() => {
    if (!episode?.coverImageUrl) return 'https://via.placeholder.com/300x200/e0e0e0/666666?text=暂无封面';
    return convertToProxyUrl(episode.coverImageUrl);
  }, [episode?.coverImageUrl]);

  const handlePlay = (e: React.MouseEvent) => {
    e.stopPropagation();
    setCurrentEpisode(episode);
    navigate(`/player/${episode.id}`);
  };

  const handleCardClick = () => {
    navigate(`/episode/${episode.id}`);
  };

  return (
    <Card
      className="episode-card"
      hoverable
      onClick={handleCardClick}
      cover={
        <div className="episode-cover">
          <img
            alt={episode.title}
            src={coverSrc}  /* ← 使用 useMemo 缓存的 URL */
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              // ✅ 防止无限循环
              if (!target.src.includes('placeholder.com')) {
                target.src = 'https://via.placeholder.com/300x200/e0e0e0/666666?text=暂无封面';
              }
            }}
          />
          <div className="play-overlay">
            <Button
              type="primary"
              shape="circle"
              size="large"
              icon={<PlayCircleOutlined />}
              onClick={handlePlay}
              className="play-button"
            />
          </div>
        </div>
      }
      actions={[
        <Tooltip title="播放时长">
          <span>
            <ClockCircleOutlined />
            {episode.duration ? formatTime(episode.duration) : '未知'}
          </span>
        </Tooltip>,
        <Tooltip title="立即播放">
          <Button
            type="text"
            icon={<SoundOutlined />}
            onClick={handlePlay}
          >
            播放
          </Button>
        </Tooltip>,
      ]}
    >
      <Card.Meta
        title={
          <div className="episode-title">
            <Tooltip title={episode.title}>
              <span>{episode.title}</span>
            </Tooltip>
          </div>
        }
        description={
          <div className="episode-description">
            <p>{episode.description || '暂无描述'}</p>
            <div className="episode-tags">
              {episode.duration && episode.duration > 600 && (
                <Tag color="orange">长音频</Tag>
              )}
            </div>
          </div>
        }
      />
    </Card>
  );
};

export default EpisodeCard;