# ELWeb 管理员初始化工具

## 📋 功能说明

这是一个独立的命令行工具，用于在数据库中创建管理员账户。适用于：

- ✅ 本地开发环境初始化
- ✅ 生产环境首次部署
- ✅ 创建额外的管理员账户

## 🚀 使用方法

### 本地开发环境

```powershell
# 1. 确保 PostgreSQL 正在运行
docker-compose up -d

# 2. 确保数据库已迁移
cd ../IdentityService.WebAPI
dotnet ef database update --project ../IdentityService.Infrastructure --context IdentityDbContext
cd ../InitAdminTool

# 3. 运行工具
dotnet run
```

### 输入信息示例

```
【步骤 1/5】输入数据库连接字符串
请输入连接字符串: Host=localhost;Port=5432;Database=elwebdb_dev;Username=postgres;Password=dev_password_123

【步骤 4/5】输入管理员信息
用户名 (必填): admin
邮箱 (必填): admin@elweb.com
密码 (必填，输入时不显示): ********
确认密码: ********
手机号 (可选，直接回车跳过): 
```

## 🌐 生产环境部署

### 1. 发布工具

```powershell
# 编译发布版本
dotnet publish -c Release -o ./publish

# 打包
Compress-Archive -Path ./publish/* -DestinationPath InitAdminTool.zip
```

### 2. 上传到服务器

```bash
# 使用 SCP 上传
scp -i "your-key.pem" InitAdminTool.zip ec2-user@your-ec2-ip:/home/ec2-user/
```

### 3. 在服务器上运行

```bash
# SSH 连接
ssh -i "your-key.pem" ec2-user@your-ec2-ip

# 解压
unzip InitAdminTool.zip -d InitAdminTool

# 运行
cd InitAdminTool
dotnet InitAdminTool.dll

# 输入 AWS RDS 连接字符串
# Host=your-rds.xxxxx.ap-southeast-2.rds.amazonaws.com;Port=5432;Database=elwebdb_prod;Username=postgres;Password=YourPassword

# 使用完删除（可选）
cd ..
rm -rf InitAdminTool*
```

## 🔐 AWS RDS 连接字符串

### 格式

```
Host={RDS终端节点};Port=5432;Database={数据库名};Username={主用户名};Password={密码};SSL Mode=Require
```

### 获取 RDS 终端节点

1. 登录 AWS 控制台
2. 进入 RDS → 数据库
3. 点击您的数据库实例
4. 复制 "终端节点" (Endpoint)

### 实际例子

```
Host=elweb-db.c9akjgxyz123.ap-southeast-2.rds.amazonaws.com;Port=5432;Database=elwebdb_prod;Username=postgres;Password=MySecurePass123!;SSL Mode=Require
```

## ⚠️ 注意事项

1. **首次使用前**：确保数据库已创建并运行了 EF Core Migrations
2. **密码安全**：输入时不显示，最少 6 位
3. **重复创建**：如果数据库已有管理员，工具会提示是否继续
4. **安全建议**：
   - 使用强密码
   - 首次登录后立即修改密码
   - 生产环境使用后立即删除工具

## 🐛 常见问题

### 无法连接到数据库

**原因**：
- 连接字符串错误
- 数据库服务未运行
- 防火墙/安全组阻止连接

**解决**：
```bash
# 测试 PostgreSQL 连接
psql -h localhost -p 5432 -U postgres -d elwebdb_dev

# 检查 Docker 容器
docker ps | grep postgres

# AWS RDS 检查安全组
# 确保入站规则允许端口 5432
```

### 表不存在

**原因**：未运行 EF Core Migrations

**解决**：
```bash
cd ../IdentityService.WebAPI
dotnet ef database update --project ../IdentityService.Infrastructure --context IdentityDbContext
```

### 用户名或邮箱已存在

**原因**：该账户已经创建

**解决**：
- 使用不同的用户名/邮箱
- 或者检查现有管理员账户

## 📞 技术支持

如有问题，请检查：
1. PostgreSQL 日志
2. 工具的错误提示
3. 数据库连接字符串格式

## 🔗 相关文档

- [PostgreSQL 连接字符串文档](https://www.npgsql.org/doc/connection-string-parameters.html)
- [AWS RDS PostgreSQL 文档](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/CHAP_PostgreSQL.html)
