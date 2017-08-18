using System;
using System.ComponentModel;

namespace MPMFEVRP.Models
{
    public static class EnumExtensions
    {
        // Extension to enum type for the descriptions.
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
            if (attributes.Length != 0)
                return (T)attributes[0];
            return null;
        }

        // This method creates a specific call to the above method, requesting the
        // Description MetaData attribute.
        public static string GetDescription(this Enum value)
        {
            var attribute = value.GetAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }

    }
}
