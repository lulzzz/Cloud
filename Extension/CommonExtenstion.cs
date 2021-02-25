using System;
using System.Collections.Generic;
using System.Text;

namespace Cloud.AzureAD.Extension
{
    public static class EnumExtention
    {

        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var field = value.GetType().GetField(value.ToString());
            return Attribute.GetCustomAttribute(field, typeof(T)) as T;
        }
    }
}
