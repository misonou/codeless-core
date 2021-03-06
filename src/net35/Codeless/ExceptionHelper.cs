﻿using System;
using System.Diagnostics;
using System.Reflection;

namespace Codeless {
  /// <summary>
  /// Provides extension methods to <see cref="Exception"/> objects.
  /// </summary>
  public static class ExceptionHelper {
    /// <summary>
    /// Rethrows an exception while maintaining the original stack trace.
    /// The method rethrows the exception on its own and does not actually return. 
    /// The returned value is to allow writing <code>throw ex.Rethrow()</code> to maintain certain compile-time checking.
    /// </summary>
    /// <param name="ex">Exception to be rethrown.</param>
    /// <returns>Supplied exception object.</returns>
    [DebuggerStepThrough]
    public static Exception Rethrow(this Exception ex) {
      typeof(Exception).GetMethod("PrepForRemoting", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ex, new object[0]);
      throw ex;
    }
  }
}
