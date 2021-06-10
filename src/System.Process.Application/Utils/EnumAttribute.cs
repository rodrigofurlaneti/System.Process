using System.Process.Domain.Enums;
using System;
using System.Reflection;

namespace System.Process.Application.Commands.Utils
{
    public static class EnumAttribute
    {
        public static string GetElementInfo(Enum value)
        {
            string output = null;
            Type type = value.GetType();
            FieldInfo fi = type.GetField(value.ToString());
            ElementInfoAttribute[] attrs = fi.GetCustomAttributes(typeof(ElementInfoAttribute), false) as ElementInfoAttribute[];
            if (attrs.Length > 0)
            {
                output = attrs[0].GetValue;
            }
            return output;
        }

        public static string GetStatusProperty(Enum value)
        {
            string output = null;
            Type type = value.GetType();
            FieldInfo fi = type.GetField(value.ToString());
            StatusPropertyAttribute[] attrs = fi.GetCustomAttributes(typeof(StatusPropertyAttribute), false) as StatusPropertyAttribute[];
            if (attrs.Length > 0)
            {
                output = attrs[0].GetValue;
            }
            return output;
        }
    }
}
