using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Kogel.Subscribe.Mssql
{
    /// <summary>
    /// 应用程序入口
    /// </summary>
    public class ApplicationProgram
    {
        /// <summary>
        /// 
        /// </summary>
        private static List<ISubscribe<object>> _subscribes;

        /// <summary>
        /// 运行所有订阅监听
        /// 使用CDC需要开启Agent，/opt/mssql/bin/mssql-conf set sqlagent.enabled true
        /// </summary>
        /// <param name="assemblyList"></param>
        public static void Run(List<Assembly> assemblyList = null)
        {
            if (_subscribes != null)
                _subscribes = new List<ISubscribe<object>>();
            List<Assembly> assemblies = new List<Assembly>();
            //获取调用者程序集信息
            StackTrace trace = new StackTrace();
            var currentAssembly = trace.GetFrame(1)?.GetMethod()?.DeclaringType?.Assembly;
            assemblies.Add(currentAssembly);
            if (assemblyList != null)
                assemblies.AddRange(assemblyList);
            var subscribeTypeInfo = typeof(Subscribe<>).GetTypeInfo();
            foreach (var assembly in assemblies)
            {
                foreach (var classImpl in assembly.GetTypes())
                {
                    //判断是否继承过Subscribe
                    if (!classImpl.Attributes.HasFlag(TypeAttributes.Abstract) && IsAssignableToGenericType(classImpl.GetTypeInfo(), subscribeTypeInfo))
                    {
                        //只要继承过都需要启动
                        if (Activator.CreateInstance(classImpl) is ISubscribe<object> impl)
                        {
                            _subscribes.Add(impl);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 比对验证开放的泛型
        /// </summary>
        /// <param name="givenType"></param>
        /// <param name="genericType"></param>
        /// <returns></returns>
        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();
            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }
            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;
            Type baseType = givenType.BaseType;
            if (baseType == null) return false;
            return IsAssignableToGenericType(baseType, genericType);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Close()
        {
            _subscribes?.ForEach(x => x?.Dispose());
        }

        ~ApplicationProgram() => Close();
    }
}
