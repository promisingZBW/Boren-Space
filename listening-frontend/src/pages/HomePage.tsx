import React, { useState, useEffect } from 'react';
import { Row, Col, Typography, Spin, Alert } from 'antd';
import { EpisodeCard } from '@/components';
import { listeningApi } from '@/api';
import type { Episode } from '@/types';

const { Title } = Typography;

const HomePage: React.FC = () => {
  const [episodes, setEpisodes] = useState<Episode[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchEpisodes();
  }, []);

  const fetchEpisodes = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await listeningApi.getEpisodes();
      
      if (response.data.success) {
        setEpisodes(response.data.data || []);
      } else {
        setError(response.data.message || '获取音频列表失败');
      }
    } catch (err) {
      console.error('获取音频列表出错:', err);
      setError('网络错误，请稍后重试');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <Title level={2} style={{ marginBottom: 24 }}>
       Boren's Podcast
      </Title>
      
      {loading ? (
        <div className="loading-container">
          <Spin size="large" />
          <p>正在加载音频列表...</p>
        </div>
      ) : error ? (
        <Alert
          message="加载失败"
          description={error}
          type="error"
          showIcon
          style={{ margin: '20px 0' }}
        />
      ) : episodes.length === 0 ? (
        <div className="empty-container">
          <p>暂无音频内容</p>
          <p style={{ color: '#8c8c8c', marginTop: 8 }}>
            请先在管理后台上传音频文件
          </p>
        </div>
      ) : (
        <Row gutter={[24, 24]} className="episodes-grid">
          {/* 按上传时间降序排序，新的在前 */}
          {[...episodes]
            .sort((a, b) => new Date(b.createTime).getTime() - new Date(a.createTime).getTime())
            .map((episode) => (
              <Col key={episode.id} xs={24} sm={12} md={8} lg={8} xl={8}>
                <EpisodeCard episode={episode} />
              </Col>
            ))}
        </Row>
      )}
    </div>
  );
};

export default HomePage;