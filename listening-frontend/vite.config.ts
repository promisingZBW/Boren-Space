import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// 前端代码proxy（代理）的配置，把前端代码的请求代理到后端服务上
// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  // 加载环境变量
  const env = loadEnv(mode, process.cwd(), '')

  return {
    plugins: [react()],
    resolve: {
      alias: {//alias是别名，@是别名，src是源码目录
        '@': path.resolve(__dirname, './src'),
      },
    },
    server: {
      port: 3000,// 开发服务器端口
      // 只在开发环境启用代理
      proxy: mode === 'development' ? {//proxy是代理
        // 具体的路径规则要放在通用规则之前
        '/api/listening': {
          target: env.VITE_API_LISTENING || 'http://localhost:7191', // Listening.Main.WebAPI
          changeOrigin: true,//changeOrigin表示是否改变源地址，这里把源地址的localhost:3000改为http://localhost:7191，
          //前面浏览器的地址是http://localhost:3000/api/listening，但是这个地址是错的，要用代理转化为后面访问的http://localhost:7191/api/Listening
          //所以是true，表示改变源地址
          secure: false,
          //简单理解完整含义：匹配以 "/api/listening" 开头的字符串，并将其替换为 "/api/Listening"
          rewrite: (path) => path.replace(/^\/api\/listening/, '/api/Listening')
        },
        '/api/auth': {
          target: env.VITE_API_IDENTITY || 'http://localhost:7116',  // IdentityService.WebAPI 端口
          changeOrigin: true,
          secure: false,
        },
        '/api/file': {
          target: env.VITE_API_FILE || 'http://localhost:7292', // FileService
          changeOrigin: true,
          secure: false,
          rewrite: (path) => {
            // 后端 C# ASP.NET Core 控制器命名为 FileController，路由为 /api/File，而前端统一小写路径，需要访问/api/file
            // 处理 swagger 路径
            if (path.includes('/swagger/')) {
              return path.replace(/^\/api\/file/, '');
            }
            // 处理普通 API 路径
            return path.replace(/^\/api\/file/, '/api/File');
          }
        },
        // 管理员 API
        '/api/episodes': {
          target: env.VITE_API_ADMIN || 'http://localhost:7109',  // Listening.Admin.WebAPI 端口
          changeOrigin: true,
          secure: false,
        },
        // 通用的 /api 规则放在最后
        '/api': {
          target: env.VITE_API_IDENTITY || 'http://localhost:7116', // IdentityService
          changeOrigin: true,
          secure: false,
        }
      } : undefined
    }
  }
})
