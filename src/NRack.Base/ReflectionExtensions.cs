using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace System.Reflection
{
    public static class ReflectionExtensions
    {
#if DOTNETCORE
        public static Attribute[] GetCustomAttributes(this Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }
#endif

        public static Type GetBaseType(this Type type)
        {
#if DOTNETCORE
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }
    }

}