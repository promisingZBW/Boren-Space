using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Zbw.Commons
{
    public static class ReflectionHelper
    {
        /// <summary>
        /// 获取所有引用的程序集
        /// 主要是为了后续启用MediatR时，能够自动扫描所有的handlers，而不是一个一个的单独注册
        /// </summary>
        public static Assembly[] GetAllReferencedAssemblies()
        {
            var assemblies = new List<Assembly>();

            // 添加当前应用程序域中已加载的程序集
            assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());

            // 尝试从依赖上下文获取更多程序集
            var deps = DependencyContext.Default;
            if (deps != null)
            {
                foreach (var lib in deps.RuntimeLibraries)
                {
                    try
                    {
                        // 只加载我们自己的程序集（以避免加载过多系统程序集）
                        if (lib.Name.StartsWith("Zbw.") ||
                            lib.Name.StartsWith("Listening.") ||
                            lib.Name.StartsWith("FileService.") ||
                            lib.Name.StartsWith("IdentityService.") ||
                            lib.Name.StartsWith("MediaEncoder.") ||
                            lib.Name.StartsWith("SearchService.") ||
                            lib.Name == "CommonInitializer")
                        {
                            var assembly = Assembly.Load(lib.Name);
                            if (!assemblies.Contains(assembly))
                            {
                                assemblies.Add(assembly);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // 忽略加载失败的程序集
                    }
                }
            }

            return assemblies.ToArray();
        }

        /// <summary>
        /// 从程序集中获取所有指定类型的实现类
        /// </summary>
        public static IEnumerable<Type> GetImplementations<T>(Assembly[] assemblies)
        {
            var targetType = typeof(T);
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (targetType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                    {
                        yield return type;
                    }
                }
            }
        }

        /// <summary>
        /// 获取所有继承自指定类型的具体类
        /// </summary>
        public static IEnumerable<Type> GetAllConcreteTypes<T>(Assembly[] assemblies) where T : class
        {
            var baseType = typeof(T);
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type))
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}