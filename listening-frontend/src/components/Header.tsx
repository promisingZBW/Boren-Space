// 导入 React 库
import React, { useState } from 'react';
// 从 Antd UI 库导入组件
import { Layout, Menu, Button, Avatar, Dropdown, Modal, Form, Input, message } from 'antd';
// 导入图标
import { UserOutlined, LogoutOutlined, HomeOutlined, SettingOutlined, RocketOutlined } from '@ant-design/icons';
// 导入路由
import { useNavigate, useLocation } from 'react-router-dom';
// 导入工具函数
import { storage } from '@/utils';
// 导入 API
import { authApi } from '@/api';

import type { MenuProps } from 'antd';

const { Header: AntHeader } = Layout;

const Header: React.FC = () => {
  const navigate = useNavigate();  // 页面跳转功能
  const location = useLocation();  // 获取当前路径
  // useState 是 React 的一个 Hook，用于在函数组件中添加 状态（state）。它返回一个数组，第一个值是当前的状态值（这里是 user），第二个值是更新状态的函数（这里是 setUser）
  const [user, setUser] = useState(storage.get('user'));  // 获取用户信息
  
  // 登录弹窗状态
  const [loginModalVisible, setLoginModalVisible] = useState(false);
  const [loginLoading, setLoginLoading] = useState(false);
  const [loginForm] = Form.useForm();

  const menuItems = [
    {
      // key: '/': 这表明点击该菜单项时，应用将导航到根路径，也就是主页
      key: '/',
      icon: <HomeOutlined />,
      label: 'Home Page',
    },
  ];

  // 登录处理
  const handleLogin = async (values: any) => {
    try {
      setLoginLoading(true);
      const response = await authApi.login({
        userNameOrEmail: values.username,
        password: values.password,
      });

      /*
      REST API 是一种流行的、灵活且高效的 Web 服务架构风格
      通常其返回的格式如下：
      {
        "success": true,
        "message": "操作成功",
        "data": {
          "token": "abc123",
          "user": {
            "id": 1,
            "name": "Alice"
          }
        }
      }
      */ 
      if (response.data.success && response.data.data) {
        const { token, user } = response.data.data;
        
        // 🔍 调试：打印用户信息
        console.log('✅ 登录成功，用户信息:', user);
        console.log('✅ Token:', token);
        console.log('✅ 用户角色:', user.roles);
        
        // 保存到本地存储
        storage.set('token', token);
        storage.set('user', user);
        setUser(user);
        
        message.success('登录成功！');
        setLoginModalVisible(false);
        //loginForm 是一个表单控制器，resetFields() 是它的一个方法，用来重置表单的所有输入框，
        // 如用户登录成功后用户名输入框 → 变回空白 ""
        loginForm.resetFields();
      } else {
        message.error(response.data.message || '登录失败');
      }
    } catch (error: any) {
      message.error(error.response?.data?.message || '登录失败，请检查用户名和密码');
    } finally {
      setLoginLoading(false);
    }
  };

  // 退出登录
  const handleLogout = () => {
    storage.remove('token');
    storage.remove('user');
    setUser(null);
    message.success('已退出登录');
  };

  // 打开管理后台
  const handleOpenAdmin = () => {
    navigate('/admin');
  };

  // 用户下拉菜单
  const userMenuItems: MenuProps['items'] = [
    {
      key: 'admin',
      icon: <SettingOutlined />,
      label: '管理后台',
      onClick: handleOpenAdmin,
    },
    {
      type: 'divider',
    },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: '退出登录',
      onClick: handleLogout,
    },
  ];

  //({ key }) 是解构赋值的写法。这表示函数接收一个对象，并直接提取这个对象中的 key 属性。
  // 例如，如果调用这个函数时传入一个对象 { key: '/' }，那么 key 会被赋值为 '/'
  const handleMenuClick = ({ key }: { key: string }) => {
    navigate(key);
  };


  //逻辑区域（return 之前）
  //return 里面只能写 UI 结构，就像画图纸一样
  //return 之前 = 餐厅后厨（准备食材、处理逻辑）
  //return 里面 = 餐厅前台（展示给客户看的菜品）
  return (
    <>
      <AntHeader className="header">
        <div className="header-content">
          <div className="logo">
            <RocketOutlined className="logo-icon" />
            <span className="logo-text">Boren's personal space</span>
          </div>

          <Menu
          theme="dark"              // 深色主题
          mode="horizontal"         // 水平排列
          selectedKeys={[location.pathname]}  // 根据当前路径高亮对应菜单项
          items={menuItems}         // 菜单项数据
          onClick={handleMenuClick} // 点击菜单项的处理函数
          className="nav-menu"
        />

          <div className="user-section">
            {user ? (
              // 已登录：显示用户头像和下拉菜单
              <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
                <Button type="text" className="user-button">
                  <Avatar size="small" icon={<UserOutlined />} />
                  <span className="username">{user.userName}</span>
                </Button>
              </Dropdown>
            ) : (
              // 未登录：显示登录按钮
              <Button type="primary" onClick={() => setLoginModalVisible(true)}>
                管理员登录
              </Button>
            )}
          </div>
        </div>
      </AntHeader>

      {/* 登录弹窗 */}
      <Modal
        title="管理员登录"
        open={loginModalVisible}              // 控制弹窗显示/隐藏
        onCancel={() => setLoginModalVisible(false)}  // 关闭弹窗
        footer={null}                          // 不使用默认底部按钮
        width={400}
      >
        <Form
          form={loginForm}              // 表单实例
          layout="vertical"             // 垂直布局（标签在输入框上方）
          onFinish={handleLogin}        // 表单提交处理函数
          autoComplete="off"            // 禁用浏览器自动完成
        >
          {/* 用户名输入框 */}
          <Form.Item
            label="用户名"
            name="username"
            rules={[{ required: true, message: '请输入用户名' }]}
          >
            <Input placeholder="admin" />
          </Form.Item>

          {/* 密码输入框 */}
          <Form.Item
            label="密码"
            name="password"
            rules={[{ required: true, message: '请输入密码' }]}
          >
            <Input.Password placeholder="请输入密码" />
          </Form.Item>

          {/* 提交按钮 */}
          <Form.Item>
            <Button 
              type="primary" 
              htmlType="submit"       // HTML 表单提交类型
              loading={loginLoading}  // 加载状态显示
              block                   // 按钮占满整行
            >
              登录
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
};

export default Header;