using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Kogel.Subscribe.Mssql
{
    /// <summary>
    /// 
    /// </summary>
    public class SubscribeProgram
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
            {
                assemblies.AddRange(assemblyList);
            }
            var subscribeTypeInfo = typeof(Subscribe<>).GetTypeInfo();
            foreach (var assembly in assemblies)
            {
                foreach (var classImpl in assembly.GetTypes())
                {
                    //判断是否继承过CcSubscribe
                    if (IsAssignableToGenericType(classImpl.GetTypeInfo(), subscribeTypeInfo))
                    {
                        //只要继承过都需要启动
                        var impl = Activator.CreateInstance(classImpl) as ISubscribe<object>;
                        if (impl != null)
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

        ~SubscribeProgram()
        {
            _subscribes?.ForEach(x => x?.Dispose());
        }
    }
}
