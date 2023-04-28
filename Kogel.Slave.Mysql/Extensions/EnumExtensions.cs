using System;
using System.ComponentModel;
using System.Reflection;

namespace Kogel.Slave.Mysql.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return value.ToString();
        }

        public static T GetEnumValueFromDescription<T>(string description) where T : Enum
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new ArgumentException($"{type.Name} is not an enum type.");
            }

            foreach (T enumValue in Enum.GetValues(type))
            {
                MemberInfo memberInfo = type.GetMember(enumValue.ToString())[0];
                DescriptionAttribute descriptionAttribute = (DescriptionAttribute)memberInfo.GetCustomAttribute(typeof(DescriptionAttribute), false);

                if (descriptionAttribute != null && descriptionAttribute.Description == description)
                {
                    return enumValue;
                }
            }

            throw new ArgumentException($"No enum value with description '{description}' found in {type.Name}.");
        }
    }
}
