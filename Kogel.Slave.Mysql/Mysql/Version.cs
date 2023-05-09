using System.ComponentModel;

namespace Kogel.Slave.Mysql
{
    public enum Version
    {
        [Description("5.0+")]
        FivePlus = 5,

        [Description("8.0+")]
        EightPlus = 8
    }
}
