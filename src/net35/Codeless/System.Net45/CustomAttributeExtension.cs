using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Reflection {
  public static class CustomAttributeExtension {
    public static T GetCustomAttribute<T>(this MemberInfo memberInfo, bool inherit) where T : Attribute {
      return memberInfo.GetCustomAttributes<T>(inherit).FirstOrDefault();
    }

    public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo memberInfo, bool inherit) where T : Attribute {
      return memberInfo.GetCustomAttributes(typeof(T), inherit).OfType<T>();
    }

    public static T GetCustomAttribute<T>(this Assembly assembly) where T : Attribute {
      return assembly.GetCustomAttributes<T>().FirstOrDefault();
    }

    public static IEnumerable<T> GetCustomAttributes<T>(this Assembly assembly) where T : Attribute {
      return assembly.GetCustomAttributes(typeof(T), false).OfType<T>();
    }
  }
}
