import React from 'react';
import { Layout } from 'antd';
import { GithubOutlined, HeartFilled } from '@ant-design/icons';

const { Footer: AntFooter } = Layout;

const Footer: React.FC = () => {
  return (
    <AntFooter className="footer">
      <div className="footer-content">
        <div className="footer-left">
          <span>©  Boren's personal space. All rights reserved.</span>
        </div>
        
        <div className="footer-center">
          <span>
            Made with <HeartFilled className="heart-icon" /> by Boren Zhou
          </span>
        </div>
        
        {/*<a> 标签: 用于创建超链接*/}
        <div className="footer-right">
          <a 
            href="https://github.com/promisingZBW" 
            target="_blank" 
            rel="noopener noreferrer"
            className="footer-link"
          >
            <GithubOutlined /> GitHub
          </a>
        </div>
      </div>
    </AntFooter>
  );
};

export default Footer;
