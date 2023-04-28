using System.ComponentModel;

namespace Kogel.Slave.Mysql
{
    public enum Version
    {
        [Description("5.0+")]
        FivePlus,

        [Description("8.0+")]
        EightPlus
    }
}
