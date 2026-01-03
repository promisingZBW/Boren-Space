// listening-frontend/src/App.tsx

import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Layout, Modal, Button } from 'antd';
import { Header, Footer } from '@/components';
import HomePage from './pages/HomePage';
import PlayerPage from './pages/PlayerPage';
import AdminPage from './pages/AdminPage';  // 新增

const { Content } = Layout;

const App: React.FC = () => {
  // 控制欢迎弹窗的显示状态
  const [welcomeModalVisible, setWelcomeModalVisible] = useState(false);

  // 组件加载时显示欢迎弹窗
  useEffect(() => {
    setWelcomeModalVisible(true);
  }, []);

  // 关闭欢迎弹窗
  const handleCloseWelcome = () => {
    setWelcomeModalVisible(false);
  };

  return (
    <Router>
      <Layout className="app-layout">
        <Header />
        <Content className="main-content">
          <div className="content-container">
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/player/:id?" element={<PlayerPage />} />
              <Route path="/admin" element={<AdminPage />} />  {/* 新增 */}
            </Routes>
          </div>
        </Content>
        <Footer />
      </Layout>

      {/* 欢迎弹窗 */}
      <Modal
        title={null}
        open={welcomeModalVisible}
        onCancel={handleCloseWelcome}
        footer={null}
        centered
        width={650}
        styles={{
          body: { padding: '40px 50px' }
        }}
      >
        <div style={{ textAlign: 'center' }}>
          {/* 标题 */}
          <h2 style={{ 
            fontSize: '28px', 
            fontWeight: 'bold',
            marginBottom: '20px', 
            color: '#1890ff',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: '10px'
          }}>
            <span style={{ fontSize: '32px' }}>🚀</span>
            Welcome to Boren's Personal Space!
          </h2>

          {/* 介绍文字 */}
          <p style={{ 
            fontSize: '16px', 
            lineHeight: '1.8',
            color: '#595959',
            marginBottom: '30px',
            textAlign: 'left'
          }}>
            Here, I'll keep updating the audio versions and related scripts of the videos I've posted on Bilibili and YouTube. 
            I'll also keep rolling out new content and features down the line—feel free to follow along!
          </p>

          {/* 社交媒体链接 */}
          <div style={{ 
            marginBottom: '35px',
            padding: '20px',
            background: '#f5f5f5',
            borderRadius: '8px'
          }}>
            <div style={{ marginBottom: '12px' }}>
              <span style={{ fontSize: '16px', color: '#262626', fontWeight: '500' }}>📺 Bilibili: </span>
              <a 
                href="https://space.bilibili.com/34917959" 
                target="_blank" 
                rel="noopener noreferrer"
                style={{ fontSize: '15px', color: '#1890ff' }}
              >
                https://space.bilibili.com/34917959
              </a>
            </div>
            <div>
              <span style={{ fontSize: '16px', color: '#262626', fontWeight: '500' }}>🎬 YouTube: </span>
              <a 
                href="https://www.youtube.com/@promisingBoren" 
                target="_blank" 
                rel="noopener noreferrer"
                style={{ fontSize: '15px', color: '#1890ff' }}
              >
                https://www.youtube.com/@promisingBoren
              </a>
            </div>
          </div>

          {/* Get it 按钮 */}
          <Button 
            type="primary" 
            size="large" 
            onClick={handleCloseWelcome}
            style={{ 
              minWidth: '140px',
              height: '44px',
              fontSize: '16px',
              fontWeight: '500'
            }}
          >
            Get it
          </Button>
        </div>
      </Modal>
    </Router>
  );
};

export default App;