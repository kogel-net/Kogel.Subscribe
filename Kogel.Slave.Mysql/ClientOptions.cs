using System;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Slave.Mysql
{
    public class ClientOptions
    {
        /// <summary>
        /// 连接HOST
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 如果为空时，自动设置一个可用的id
        /// </summary>
        public int? ServerId { get; set; }
    }
}
