using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Molinos.Orquest.Dominio.Helpers
{
    public static class ExtensionesEnum
    {
        public static string DisplayEnum<TEnum>(this TEnum enumValue) where TEnum : struct
        {
            //You can't use a type constraints on the special class Enum. So I use this workaround
            if (!typeof (TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum must be of type System.Enum");
            }

            Type type = typeof(TEnum);
            MemberInfo[] memberInfo = type.GetMember(enumValue.ToString());
            if (memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                if (attrs.Length > 0)
                {
                    return ((DisplayAttribute) attrs[0]).GetName();
                }
            }
            return enumValue.ToString();
        }
    }
}
