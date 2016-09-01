using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web;

namespace Codeless {
  [DebuggerStepThrough]
  internal static class CommonHelper {
    public static T ConfirmNotNull<T>(T value, string argumentName) {
      if (Object.ReferenceEquals(value, null)) {
        throw new ArgumentNullException(argumentName);
      }
      return value;
    }
    
    public static T AccessNotNull<T>(T value, string argumentName) {
      if (Object.ReferenceEquals(value, null)) {
        throw new MemberAccessException(argumentName);
      }
      return value;
    }
    
    public static T TryCastOrDefault<T>(object value) where T : class {
      return value as T;
    }
    
    public static bool IsNullOrWhiteSpace(string value) {
      return String.IsNullOrEmpty(value) || value.Trim().Length == 0;
    }

    [DebuggerStepThrough]
    public static T HttpContextSingleton<T>() where T : new() {
      HttpContext context = HttpContext.Current;
      if (context != null) {
        return context.Items.EnsureKeyValue(typeof(T).GUID, ReflectionHelper.CreateInstance<T>);
      }
      return default(T);
    }

    [DebuggerStepThrough]
    public static T HttpContextSingleton<T>(Func<T> valueFactory) {
      CommonHelper.ConfirmNotNull(valueFactory, "valueFactory");
      HttpContext context = HttpContext.Current;
      if (context != null) {
        return context.Items.EnsureKeyValue(typeof(T).GUID, valueFactory);
      }
      return default(T);
    }

    [DebuggerStepThrough]
    public static bool HasFlag(this Enum value, Enum flag) {
      ulong keysVal = Convert.ToUInt64(value);
      ulong flagVal = Convert.ToUInt64(flag);
      return (keysVal & flagVal) == flagVal;
    }

    public static void CopyTo(this Stream source, Stream output) {
      CommonHelper.ConfirmNotNull(output, "output");
      byte[] buffer = new byte[16 * 1024];
      int bytesRead;
      while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0) {
        output.Write(buffer, 0, bytesRead);
      }
    }
  }
}
