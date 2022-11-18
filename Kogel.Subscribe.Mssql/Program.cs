using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Kogel.Subscribe.Mssql
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        private static List<ISubscribe<object>> _subscribes;

        /// <summary>
        /// 运行所有订阅监听
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
                    if (classImpl.GetTypeInfo().IsAssignableToGenericType(subscribeTypeInfo))
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

        ~Program()
        {
            _subscribes?.ForEach(x => x?.Dispose());
        }
    }
}
