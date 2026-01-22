using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Infrastructure;
using IdentityService.Domain;
using Microsoft.EntityFrameworkCore;

// 启用传统时间戳行为（PostgreSQL 兼容）
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("╔═══════════════════════════════════════════════╗");
Console.WriteLine("║     ELWeb 管理员初始化工具 v1.0              ║");
Console.WriteLine("║     Admin Initialization Tool                 ║");
Console.WriteLine("╚═══════════════════════════════════════════════╝");
Console.WriteLine();

try
{
    // 1. 获取数据库连接字符串
    Console.WriteLine("【步骤 1/5】输入数据库连接字符串");
    Console.WriteLine("────────────────────────────────────────────────");
    Console.WriteLine("示例（本地开发）：");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine("Host=localhost;Port=5432;Database=elwebdb_dev;Username=postgres;Password=dev_password_123");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine("示例（AWS RDS）：");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine("Host=your-db.xxxxx.ap-southeast-2.rds.amazonaws.com;Port=5432;Database=elwebdb_prod;Username=postgres;Password=YourPassword");
    Console.ResetColor();
    Console.WriteLine();
    Console.Write("请输入连接字符串: ");
    var connStr = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(connStr))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n❌ 错误：连接字符串不能为空！");
        Console.ResetColor();
        return;
    }

    // 2. 连接数据库
    Console.WriteLine("\n【步骤 2/5】连接数据库");
    Console.WriteLine("────────────────────────────────────────────────");
    Console.Write("正在连接...");

    var options = new DbContextOptionsBuilder<IdentityDbContext>()
        .UseNpgsql(connStr)
        .Options;

    using var dbContext = new IdentityDbContext(options);

    // 测试连接
    if (!await dbContext.Database.CanConnectAsync())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n❌ 错误：无法连接到数据库！");
        Console.WriteLine("请检查：");
        Console.WriteLine("  1. 连接字符串是否正确");
        Console.WriteLine("  2. 数据库服务是否运行");
        Console.WriteLine("  3. 网络是否可达（如果是云数据库）");
        Console.WriteLine("  4. 防火墙/安全组是否允许连接");
        Console.ResetColor();
        return;
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(" ✓ 连接成功！");
    Console.ResetColor();

    // 3. 检查数据库状态
    Console.WriteLine("\n【步骤 3/5】检查数据库状态");
    Console.WriteLine("────────────────────────────────────────────────");

    // 检查表是否存在
    var tablesExist = true;
    try
    {
        await dbContext.Users.AnyAsync();
    }
    catch
    {
        tablesExist = false;
    }

    if (!tablesExist)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠ 警告：数据库表尚未创建！");
        Console.WriteLine("请先运行 EF Core Migrations：");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("  dotnet ef database update --project IdentityService.Infrastructure --startup-project IdentityService.WebAPI --context IdentityDbContext");
        Console.ResetColor();
        return;
    }

    // 检查是否已有管理员
    var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var existingAdmins = await dbContext.UserRoles
        .Where(ur => ur.RoleId == adminRoleId)
        .Include(ur => ur.User)
        .Where(ur => !ur.User.IsDeleted)
        .Select(ur => ur.User)
        .ToListAsync();

    if (existingAdmins.Any())
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠ 警告：数据库中已存在 {existingAdmins.Count} 个管理员账户：");
        foreach (var admin in existingAdmins)
        {
            Console.WriteLine($"  • {admin.UserName} ({admin.Email}) - ID: {admin.Id}");
        }
        Console.ResetColor();
        Console.WriteLine();
        Console.Write("是否继续添加新管理员？(y/n): ");
        var confirm = Console.ReadLine()?.Trim().ToLower();
        if (confirm != "y" && confirm != "yes")
        {
            Console.WriteLine("\n操作已取消。");
            return;
        }
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ 数据库中暂无管理员账户");
        Console.ResetColor();
    }

    // 4. 收集管理员信息
    Console.WriteLine("\n【步骤 4/5】输入管理员信息");
    Console.WriteLine("────────────────────────────────────────────────");

    // 用户名
    string? userName = null;
    while (string.IsNullOrWhiteSpace(userName))
    {
        Console.Write("用户名 (必填): ");
        userName = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(userName))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ✗ 用户名不能为空！");
            Console.ResetColor();
            continue;
        }

        // 检查用户名是否已存在
        if (await dbContext.Users.AnyAsync(u => u.UserName == userName && !u.IsDeleted))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✗ 用户名 '{userName}' 已存在！");
            Console.ResetColor();
            userName = null;
        }
    }

    // 邮箱
    string? email = null;
    while (string.IsNullOrWhiteSpace(email))
    {
        Console.Write("邮箱 (必填): ");
        email = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ✗ 邮箱不能为空！");
            Console.ResetColor();
            continue;
        }

        // 简单的邮箱格式验证
        if (!email.Contains("@") || !email.Contains("."))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ✗ 邮箱格式不正确！");
            Console.ResetColor();
            email = null;
            continue;
        }

        // 检查邮箱是否已存在
        if (await dbContext.Users.AnyAsync(u => u.Email == email && !u.IsDeleted))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✗ 邮箱 '{email}' 已存在！");
            Console.ResetColor();
            email = null;
        }
    }

    // 密码
    string? password = null;
    while (string.IsNullOrWhiteSpace(password))
    {
        Console.Write("密码 (必填，输入时不显示): ");
        password = ReadPassword();

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ✗ 密码长度至少 6 位！");
            Console.ResetColor();
            password = null;
            continue;
        }

        Console.Write("确认密码: ");
        var confirmPassword = ReadPassword();

        if (password != confirmPassword)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ✗ 两次密码输入不一致！");
            Console.ResetColor();
            password = null;
        }
    }

    // 手机号（可选）
    Console.Write("手机号 (可选，直接回车跳过): ");
    var phoneNumber = Console.ReadLine()?.Trim();

    // 5. 创建管理员
    Console.WriteLine("\n【步骤 5/5】创建管理员账户");
    Console.WriteLine("────────────────────────────────────────────────");
    Console.Write("正在创建...");

    // 使用 PasswordService 哈希密码
    var passwordService = new PasswordService();
    var passwordHash = passwordService.HashPassword(password);

    // 创建用户
    var user = new User(userName, email, passwordHash);

    if (!string.IsNullOrWhiteSpace(phoneNumber))
    {
        user.UpdateProfile(phoneNumber);
    }

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync();

    // 分配管理员角色
    var userRole = new UserRole(user.Id, adminRoleId);

    dbContext.UserRoles.Add(userRole);
    await dbContext.SaveChangesAsync();

    // 6. 成功提示
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(" ✓ 创建成功！");
    Console.ResetColor();

    Console.WriteLine();
    Console.WriteLine("╔═══════════════════════════════════════════════╗");
    Console.WriteLine("║            ✓ 管理员创建成功！                 ║");
    Console.WriteLine("╚═══════════════════════════════════════════════╝");
    Console.WriteLine();
    Console.WriteLine("账户信息：");
    Console.WriteLine($"  用户名: {userName}");
    Console.WriteLine($"  邮箱:   {email}");
    Console.WriteLine($"  用户ID: {user.Id}");
    if (!string.IsNullOrWhiteSpace(phoneNumber))
    {
        Console.WriteLine($"  手机号: {phoneNumber}");
    }
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("⚠ 请妥善保管登录信息！");
    Console.ResetColor();
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n❌ 发生错误：");
    Console.WriteLine($"类型: {ex.GetType().Name}");
    Console.WriteLine($"消息: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"内部错误: {ex.InnerException.Message}");
    }
    Console.ResetColor();

    if (System.Diagnostics.Debugger.IsAttached)
    {
        Console.WriteLine("\n完整堆栈跟踪：");
        Console.WriteLine(ex.StackTrace);
    }
}

Console.WriteLine("\n按任意键退出...");
Console.ReadKey();

// 辅助方法：安全读取密码（不显示字符）
static string ReadPassword()
{
    var password = "";
    ConsoleKeyInfo key;

    do
    {
        key = Console.ReadKey(true);

        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
        {
            password += key.KeyChar;
            Console.Write("*");
        }
        else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password = password[0..^1];
            Console.Write("\b \b");
        }
    } while (key.Key != ConsoleKey.Enter);

    Console.WriteLine();
    return password;
}
