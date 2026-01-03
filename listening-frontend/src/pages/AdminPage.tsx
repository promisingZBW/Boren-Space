import React, { useState, useEffect } from 'react';
import { 
  Button, Table, Modal, Form, Input, Upload, message, 
  Popconfirm, Space, Progress, Card 
} from 'antd';
import { 
  UploadOutlined, DeleteOutlined, InboxOutlined, 
  PlusOutlined, ReloadOutlined 
} from '@ant-design/icons';
import { adminApi } from '@/api';
import { useNavigate } from 'react-router-dom';
import { storage } from '@/utils';
import type { Episode } from '@/types';


const AdminPage: React.FC = () => {
  const navigate = useNavigate();
  
  // 🔍 调试：先检查 localStorage 原始数据
  const rawUserData = localStorage.getItem('user');
  console.log('🔍 [AdminPage] localStorage 原始数据:', rawUserData);
  
  const user = storage.get('user');
  
  // 🔍 调试：打印用户信息
  console.log('🔍 [AdminPage] 解析后的用户对象:', user);
  console.log('🔍 [AdminPage] 用户对象的所有键:', user ? Object.keys(user) : 'null');
  console.log('🔍 [AdminPage] user.roles (小写):', user?.roles);
  console.log('🔍 [AdminPage] user.Roles (大写):', (user as any)?.Roles);
  console.log('🔍 [AdminPage] 是否包含Admin角色:', user?.roles?.includes('Admin'));
  
  // 检查是否是管理员
  useEffect(() => {
    if (!user || !user.roles?.includes('Admin')) {
      const debugInfo = {
        hasUser: !!user,
        roles: user?.roles,
        allKeys: user ? Object.keys(user) : [],
        hasAdminRole: user?.roles?.includes('Admin'),
        rawData: localStorage.getItem('user')
      };
      console.error('❌ 权限检查失败:', debugInfo);
      
      // 🔍 临时添加 alert 来暂停执行，让你能看到控制台
      alert('权限检查失败，请查看控制台（F12）的详细信息！\n\n' + JSON.stringify(debugInfo, null, 2));
      
      message.error('无权限访问');
      navigate('/');
    } else {
      console.log('✅ 权限检查通过，用户有Admin权限');
    }
  }, [user, navigate]);

  // Line 26 - Use proper type
  const [episodes, setEpisodes] = useState<Episode[]>([]);
  const [loading, setLoading] = useState(false);
  const [uploadModalVisible, setUploadModalVisible] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [uploadForm] = Form.useForm();

  // 加载音频列表
  const fetchEpisodes = async () => {
    try {
      setLoading(true);
      const response = await adminApi.getEpisodes();
      if (response.data.success) {
        setEpisodes(response.data.data || []);
      }
    } catch (error) {
      message.error('获取列表失败');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEpisodes();
  }, []);

  // 上传音频
  const handleUpload = async (values: any) => {
    try {
      setUploading(true);
      setUploadProgress(0);

      const formData = new FormData();
      formData.append('Title', values.title);
      formData.append('Description', values.description || '');
      
      if (values.audioFile?.[0]) {
        formData.append('AudioFile', values.audioFile[0].originFileObj);
      }
      
      if (values.subtitleFile?.[0]) {
        formData.append('SubtitleFile', values.subtitleFile[0].originFileObj);
      }
      
      if (values.coverImage?.[0]) {
        formData.append('CoverImage', values.coverImage[0].originFileObj);
      }

      const response = await adminApi.uploadEpisode(formData, setUploadProgress);

      if (response.data.success) {
        message.success('上传成功！');
        setUploadModalVisible(false);
        uploadForm.resetFields();
        setUploadProgress(0);
        fetchEpisodes();
      } else {
        message.error(response.data.message || '上传失败');
      }
    } catch (error: any) {
      message.error(error.response?.data?.message || '上传失败');
    } finally {
      setUploading(false);
    }
  };

  // 删除音频
  const handleDelete = async (id: string) => {
    try {
      const response = await adminApi.deleteEpisode(id);
      if (response.data.success) {
        message.success('删除成功');
        fetchEpisodes();
      }
    } catch (error) {
      message.error('删除失败');
    }
  };

  // 表格列定义
  const columns = [
    {
      title: '标题',
      dataIndex: 'title',
      ellipsis: true,
    },
    {
      title: '描述',
      dataIndex: 'description',
      ellipsis: true,
      width: 200,
    },
    {
      title: '时长',
      dataIndex: 'duration',
      width: 100,
      render: (duration: number) => {
        const mins = Math.floor(duration / 60);
        const secs = duration % 60;
        return `${mins}:${secs.toString().padStart(2, '0')}`;
      },
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      width: 180,
      render: (date: string) => new Date(date).toLocaleString('zh-CN'),
    },
    {
      title: '操作',
      key: 'action',
      width: 150,
      fixed: 'right' as const,
      render: (_: any, record: any) => (
        <Space>
          <Button 
            type="link" 
            onClick={() => navigate(`/player/${record.id}`)}
          >
            预览
          </Button>
          <Popconfirm
            title="确定删除这个音频吗？"
            description="删除后无法恢复"
            onConfirm={() => handleDelete(record.id)}
            okText="确定"
            cancelText="取消"
            okButtonProps={{ danger: true }}
          >
            <Button type="link" danger icon={<DeleteOutlined />}>
              删除
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: '24px', maxWidth: 1400, margin: '0 auto' }}>
      <Card>
        <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ margin: 0 }}>音频管理</h2>
          <Space>
            <Button 
              icon={<ReloadOutlined />} 
              onClick={fetchEpisodes}
              loading={loading}
            >
              刷新
            </Button>
            <Button 
              type="primary" 
              icon={<PlusOutlined />}
              onClick={() => setUploadModalVisible(true)}
            >
              上传新音频
            </Button>
          </Space>
        </div>

        <Table
          columns={columns}
          dataSource={episodes}
          loading={loading}
          rowKey="id"
          pagination={{ 
            pageSize: 10,
            showTotal: (total) => `共 ${total} 条`,
          }}
          scroll={{ x: 1000 }}
        />
      </Card>

      {/* 上传弹窗 */}
      <Modal
        title="上传新音频"
        open={uploadModalVisible}
        onCancel={() => {
          if (!uploading) {
            setUploadModalVisible(false);
            uploadForm.resetFields();
            setUploadProgress(0);
          }
        }}
        footer={null}
        width={600}
        maskClosable={!uploading}
      >
        <Form
          form={uploadForm}
          layout="vertical"
          onFinish={handleUpload}
        >
          <Form.Item
            name="title"
            label="标题"
            rules={[{ required: true, message: '请输入标题' }]}
          >
            <Input placeholder="例如：英语日常对话 - 第一课" maxLength={200} />
          </Form.Item>

          <Form.Item
            name="description"
            label="描述"
          >
            <Input.TextArea 
              rows={3} 
              placeholder="简单描述这个音频的内容..."
              maxLength={500}
            />
          </Form.Item>

          <Form.Item
            name="audioFile"
            label="音频文件（必填）"
            rules={[{ required: true, message: '请上传音频文件' }]}
            valuePropName="fileList"
            getValueFromEvent={(e) => e?.fileList}
          >
            <Upload.Dragger
              maxCount={1}
              accept=".mp3,.wav,.m4a,.aac"
              beforeUpload={() => false}
            >
              <p className="ant-upload-drag-icon">
                <InboxOutlined />
              </p>
              <p className="ant-upload-text">点击或拖拽音频文件到此处</p>
              <p className="ant-upload-hint">支持 MP3, WAV, M4A, AAC 格式</p>
            </Upload.Dragger>
          </Form.Item>

          <Form.Item
            name="subtitleFile"
            label="字幕文件（可选）"
            valuePropName="fileList"
            getValueFromEvent={(e) => e?.fileList}
          >
            <Upload
              maxCount={1}
              accept=".srt,.vtt"
              beforeUpload={() => false}
            >
              <Button icon={<UploadOutlined />}>选择字幕文件 (SRT/VTT)</Button>
            </Upload>
          </Form.Item>

          <Form.Item
            name="coverImage"
            label="封面图片（可选）"
            valuePropName="fileList"
            getValueFromEvent={(e) => e?.fileList}
          >
            <Upload
              maxCount={1}
              accept=".jpg,.jpeg,.png,.webp"
              listType="picture-card"
              beforeUpload={() => false}
            >
              <div>
                <PlusOutlined />
                <div style={{ marginTop: 8 }}>上传封面</div>
              </div>
            </Upload>
          </Form.Item>

          {uploading && (
            <Form.Item>
              <Progress 
                percent={uploadProgress} 
                status={uploadProgress === 100 ? 'success' : 'active'}
              />
            </Form.Item>
          )}

          <Form.Item>
            <Space style={{ width: '100%', justifyContent: 'flex-end' }}>
              <Button onClick={() => setUploadModalVisible(false)} disabled={uploading}>
                取消
              </Button>
              <Button 
                type="primary" 
                htmlType="submit" 
                loading={uploading}
              >
                {uploading ? `上传中... ${uploadProgress}%` : '开始上传'}
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default AdminPage;