using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Codeless {
  /// <summary>
  /// Provides methods to simplify reflection operations.
  /// </summary>
  public static class ReflectionHelper {
    internal const BindingFlags ALL = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// Gets the types defined in the specified assembly.
    /// Types that cannot be loaded are excluded in the result.
    /// </summary>
    /// <param name="assembly">An assembly object.</param>
    /// <returns>An array of loadable types defined in this assembly.</returns>
    [DebuggerStepThrough]
    public static Type[] GetLoadedTypes(this Assembly assembly) {
      CommonHelper.ConfirmNotNull(assembly, "assembly");
      try {
        return assembly.GetTypes();
      } catch (ReflectionTypeLoadException ex) {
        return ex.Types.Where(v => v != null).ToArray();
      }
    }

    /// <summary>
    /// Gets a base definition of an overriden property. It is analogous to <see cref="MethodInfo.GetBaseDefinition"/>.
    /// </summary>
    /// <param name="property">An object representing a property.</param>
    /// <returns>Base definition of the property; or the supplied property if there is no base definition.</returns>
    public static PropertyInfo GetBaseDefinition(this PropertyInfo property) {
      CommonHelper.ConfirmNotNull(property, "property");
      Type[] indexTypes = property.GetIndexParameterTypes();
      MethodInfo accessor = property.GetAccessors(true)[0];
      MethodInfo baseAccessor = accessor.GetBaseDefinition();
      if (baseAccessor != accessor) {
        return baseAccessor.DeclaringType.GetProperty(property.Name, ALL, null, property.PropertyType, indexTypes, null);
      }
      return property;
    }

    /// <summary>
    /// Gets the types of the parameters of the specified method or constructor.
    /// </summary>
    /// <param name="method">An object representing a method or constructor.</param>
    /// <returns>A collection containing the types of each parameter.</returns>
    [DebuggerStepThrough]
    public static Type[] GetParameterTypes(this MethodBase method) {
      CommonHelper.ConfirmNotNull(method, "method");
      ParameterInfo[] parameters = method.GetParameters();
      Type[] types = new Type[parameters.Length];
      for (int i = parameters.Length - 1; i >= 0; i--) {
        types[i] = parameters[i].ParameterType;
      }
      return types;
    }

    /// <summary>
    /// Gets the types of the index parameters of the specified indexer.
    /// </summary>
    /// <param name="property">An object representing an indexer.</param>
    /// <returns>A collection containing the types of each parameter.</returns>
    [DebuggerStepThrough]
    public static Type[] GetIndexParameterTypes(this PropertyInfo property) {
      CommonHelper.ConfirmNotNull(property, "property");
      ParameterInfo[] parameters = property.GetIndexParameters();
      Type[] types = new Type[parameters.Length];
      for (int i = parameters.Length - 1; i >= 0; i--) {
        types[i] = parameters[i].ParameterType;
      }
      return types;
    }

    /// <summary>
    /// Creates an instance of the specified type <typeparamref name="T"/> using the default parameterless constructor.
    /// Any exception thrown from the constructor is rethrown directly instead of <see cref="TargetInvocationException"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to create.</typeparam>
    /// <returns>A reference to the newly created object.</returns>
    [DebuggerStepThrough]
    public static T CreateInstance<T>() {
      try {
        return Activator.CreateInstance<T>();
      } catch (TargetInvocationException ex) {
        throw ex.InnerException.Rethrow();
      }
    }

    /// <summary>
    /// Creates an instance of the specified type <typeparamref name="T"/> using the default parameterless constructor.
    /// Any exception thrown from the constructor is rethrown directly instead of <see cref="TargetInvocationException"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to create.</typeparam>
    /// <param name="parameters">A list of parameters to be passed to the constructor.</param>
    /// <returns>A reference to the newly created object.</returns>
    [DebuggerStepThrough]
    public static T CreateInstance<T>(params object[] parameters) {
      return (T)CreateInstance(typeof(T), parameters);
    }

    /// <summary>
    /// Creates an instance of the specified type using the inferred constructor from the specified parameters.
    /// If the specified class is a generic class definition, the generic type arguments are also inferred from the specified parameters.
    /// Any exception thrown from the constructor is rethrown directly instead of <see cref="TargetInvocationException"/>.
    /// </summary>
    /// <param name="type">The type of object to create.</param>
    /// <param name="parameters">A list of parameters to be passed to the constructor.</param>
    /// <returns>A reference to the newly created object.</returns>
    public static object CreateInstance(this Type type, params object[] parameters) {
      CommonHelper.ConfirmNotNull(type, "type");
      try {
        ConstructorInfo ctor;
        type.InferGenericParameters(parameters, out ctor);
        return ctor.Invoke(parameters);
      } catch (TargetInvocationException ex) {
        throw ex.InnerException.Rethrow();
      }
    }

    /// <summary>
    /// Gets the value of a field supported by a given object.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="member">An object representing the data field of the value to get.</param>
    /// <param name="target">The object whose field value will be returned.</param>
    /// <returns>An object containing the value of the field reflected by this instance.</returns>
    [DebuggerStepThrough]
    public static T GetValue<T>(this FieldInfo member, object target) {
      CommonHelper.ConfirmNotNull(member, "member");
      try {
        return (T)member.GetValue(target);
      } catch (TargetInvocationException ex) {
        throw ex.InnerException.Rethrow();
      }
    }

    /// <summary>
    /// Gets the value of a property supported by a given object.
    /// If the specified property is declared on a generic class definition, the generic type arguments of the declaring type is inferred from the specified parameters.
    /// Any exception thrown from the property accessor method is rethrown directly instead of <see cref="TargetInvocationException"/>.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="member">An object representing the data property of the value to get.</param>
    /// <param name="target">The object whose property value will be returned.</param>
    /// <param name="parameters">A list of parameters supplied to the data property if it accepts index parameters.</param>
    /// <returns>An object containing the value of the property reflected by this instance.</returns>
    [DebuggerStepThrough]
    public static T GetValue<T>(this PropertyInfo member, object target, params object[] parameters) {
      CommonHelper.ConfirmNotNull(member, "member");
      try {
        return (T)member.InferGenericParameters(target, parameters, typeof(T)).GetValue(target, parameters);
      } catch (TargetInvocationException ex) {
        throw ex.InnerException.Rethrow();
      }
    }

    /// <summary>
    /// Sets the value of a field supported by a given object.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="member">An object representing the data field of the value to set.</param>
    /// <param name="target">The object whose field value will be set.</param>
    /// <param name="value">An object containing the value.</param>
    [DebuggerStepThrough]
    public static void SetValue<T>(this FieldInfo member, object target, T value) {
      CommonHelper.ConfirmNotNull(member, "member");
      try {
        member.SetValue(target, value);
      } catch (TargetInvocationException ex) {
        throw ex.InnerException.Rethrow();
      }
    }

    /// <summary>
    /// Sets the value of a property supported by a given object.
    /// If the specified property is declared on a generic class definition, the generic type arguments of the declaring type is inferred from the specified parameters.
    /// Any exception thrown from the property accessor method is rethrown directly instead of <see cref="TargetInvocationException"/>.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="member">An object representing the data property of the value to set.</param>
    /// <param name="target">The object whose property value will be returned.</param>
    /// <param name="value">An object containing the value.</param>
    /// <param name="parameters">A list of parameters supplied to the data property if it accepts index parameters.</param>
    [DebuggerStepThrough]
    public static void SetValue<T>(this PropertyInfo member, object target, T value, params object[] parameters) {
      CommonHelper.ConfirmNotNull(member, "member");
      try {
        member.InferGenericParameters(target, parameters, typeof(T)).SetValue(target, value, parameters);
      } catch (TargetInvocationException ex) {
        throw ex.InnerException.Rethrow();
      }
    }

    /// <summary>
    /// Invokes the specified method or constructor, using the specified parameters.
    /// If the specified method is declared on a generic class definition, or is a generic method definition, 
    /// the generic type arguments of the declaring type is inferred from the specified parameters.
    /// Any exception thrown from the method is rethrown directly instead of <see cref="TargetInvocationException"/>.
    /// </summary>
    /// <typeparam name="T">The return type of the specified method.</typeparam>
    /// <param name="member">An object representing the method or constructor to invoke.</param>
    /// <param name="target">The object whose method will be invoked.</param>
    /// <param name="parameters">A list of parameters supplied to the method or constructor.</param>
    /// <returns>The value returned by the method invocation.</returns>
    [DebuggerStepThrough]
    public static T Invoke<T>(this MethodBase member, object target, params object[] parameters) {
      CommonHelper.ConfirmNotNull(member, "member");
      try {
        return (T)member.InferGenericParameters(target, parameters, typeof(T)).Invoke(target, parameters);
      } catch (TargetInvocationException ex) {
        throw ex.InnerException.Rethrow();
      }
    }

    /// <summary>
    /// Raises the specified event, using the specified parameters.
    /// Any exception thrown from the method is rethrown directly instead of <see cref="TargetInvocationException"/>.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="name"></param>
    /// <param name="parameters"></param>
    public static void RaiseEvent(object target, string name, params object[] parameters) {
      CommonHelper.ConfirmNotNull(target, "target");
      CommonHelper.ConfirmNotNull(name, "name");
      EventInfo evt = target.GetType().GetEvent(name, ALL);
      if (evt == null) {
        throw new MissingMemberException(target.GetType().FullName, name);
      }
      MulticastDelegate multicast = evt.DeclaringType.GetField(name, ALL).GetValue<MulticastDelegate>(target);
      if (multicast != null) {
        foreach (Delegate handler in multicast.GetInvocationList()) {
          handler.Method.Invoke<object>(handler.Target, parameters);
        }
      }
    }

    /// <summary>
    /// Invokes the specified method, using the specified parameters.
    /// Any exception thrown from the method is rethrown directly instead of <see cref="TargetInvocationException"/>.
    /// </summary>
    /// <param name="target">An object representing the method to invoke.</param>
    /// <param name="name">The object whose method will be invoked.</param>
    /// <param name="parameters">A list of parameters supplied to the method.</param>
    /// <returns>The value returned by the method invocation.</returns>
    [DebuggerStepThrough]
    public static object InvokeMethod(object target, string name, params object[] parameters) {
      return InvokeMethod<object>(target, name, parameters);
    }

    /// <summary>
    /// Invokes the specified method, using the specified parameters.
    /// Any exception thrown from the method is rethrown directly instead of <see cref="TargetInvocationException"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target">An object representing the method to invoke.</param>
    /// <param name="name">The object whose method will be invoked.</param>
    /// <param name="parameters">A list of parameters supplied to the method.</param>
    /// <returns>The value returned by the method invocation.</returns>
    public static T InvokeMethod<T>(object target, string name, params object[] parameters) {
      CommonHelper.ConfirmNotNull(target, "target");
      CommonHelper.ConfirmNotNull(name, "name");
      foreach (MethodInfo method in target.GetType().GetMethods(ALL)) {
        Type[] arr1, arr2, arr3;
        if (name == method.Name && IsSignatureMatched(method, target, parameters, typeof(T), out arr1, out arr2, out arr3)) {
          if (method.ContainsGenericParameters) {
            return method.MakeGenericMethod(arr2).Invoke<T>(target, parameters);
          }
          return method.Invoke<T>(target, parameters);
        }
      }
      throw new MissingMethodException(target.GetType().FullName, name);
    }

    private static Type InferGenericParameters(this Type type, object[] parameters, out ConstructorInfo inferredCtor) {
      foreach (ConstructorInfo ctor in type.GetConstructors(ALL)) {
        Type[] arr1, arr2, arr3;
        if (IsSignatureMatched(ctor, null, parameters, null, out arr1, out arr2, out arr3)) {
          if (type.ContainsGenericParameters) {
            Type inferredType = type.GetGenericTypeDefinition().MakeGenericType(arr1);
            inferredCtor = inferredType.GetConstructor(ALL, null, arr3, null);
            return inferredType;
          }
          inferredCtor = ctor;
          return type;
        }
      }
      throw new InvalidOperationException("Unable to infer usage from supplied arguments.");
    }

    private static PropertyInfo InferGenericParameters(this PropertyInfo member, object target, object[] parameters, Type returnType) {
      if (member.DeclaringType.IsGenericTypeDefinition) {
        MethodBase inferredMethod = member.GetAccessors(true)[0].InferGenericParameters(target, parameters, returnType);
        return inferredMethod.DeclaringType.GetProperty(member.Name, inferredMethod.GetParameterTypes());
      }
      return member;
    }

    private static MethodBase InferGenericParameters(this MethodBase member, object target, object[] parameters, Type returnType) {
      if (member.ContainsGenericParameters) {
        Type[] arr1, arr2, arr3;
        if (IsSignatureMatched(member, target, parameters, returnType, out arr1, out arr2, out arr3)) {
          return MakeGenericMethod(member, arr1, arr2, arr3);
        }
        throw new InvalidOperationException("Unable to infer usage from supplied arguments.");
      }
      return member;
    }

    internal static MethodBase MakeGenericMethod(MethodBase member, Type[] typeGenericArguments, Type[] methodGenericArguments, Type[] parameterTypes) {
      if (member.DeclaringType.ContainsGenericParameters) {
        Type inferredType = member.DeclaringType.MakeGenericType(typeGenericArguments);
        if (member.MemberType == MemberTypes.Constructor) {
          member = inferredType.GetConstructor(ALL, null, parameterTypes, null);
        } else {
          member = inferredType.GetMethod(member.Name, ALL, null, parameterTypes, null);
        }
      }
      if (member.ContainsGenericParameters) {
        member = ((MethodInfo)member).MakeGenericMethod(methodGenericArguments);
      }
      return member;
    }

    private static bool IsSignatureMatched(MethodBase method, object target, object[] parameters, Type returnType, out Type[] typeGenericArguments, out Type[] methodGenericArguments, out Type[] parameterTypes) {
      if (method.GetParameters().Length != parameters.Length) {
        typeGenericArguments = methodGenericArguments = parameterTypes = null;
        return false;
      }
      Type targetType = null;
      parameterTypes = new Type[parameters.Length];
      if (target != null) {
        targetType = target.GetType();
      }
      for (int i = 0; i < parameters.Length; i++) {
        parameterTypes[i] = parameters[i] != null ? parameters[i].GetType() : null;
      }
      return IsSignatureMatched(method, targetType, parameterTypes, returnType, out typeGenericArguments, out methodGenericArguments, out parameterTypes);
    }

    internal static bool IsSignatureMatched(MethodBase method, Type targetType, Type[] parameterTypes, Type returnType, out Type[] typeGenericArguments, out Type[] methodGenericArguments, out Type[] actualParameterTypes) {
      CommonHelper.ConfirmNotNull(method, "method");
      CommonHelper.ConfirmNotNull(parameterTypes, "parameterTypes");
      bool hasTypeArguments = method.ContainsGenericParameters;
      Type dummy;

      actualParameterTypes = method.GetParameterTypes();

      // trivial case where parameter count does not match
      if (actualParameterTypes.Length != parameterTypes.Length) {
        typeGenericArguments = methodGenericArguments = actualParameterTypes = null;
        return false;
      }

      Hashtable hashtable;
      if (hasTypeArguments) {
        typeGenericArguments = method.DeclaringType.GetGenericArguments();
        methodGenericArguments = method.MemberType == MemberTypes.Constructor ? Type.EmptyTypes : method.GetGenericArguments();

        // create a dictionary to map generic parameters and generic arguments
        hashtable = new Hashtable(typeGenericArguments.Length + methodGenericArguments.Length);
        for (int i = 0; i < typeGenericArguments.Length; i++) {
          hashtable[typeGenericArguments[i]] = null;
        }
        for (int i = 0; i < methodGenericArguments.Length; i++) {
          hashtable[methodGenericArguments[i]] = null;
        }
      } else {
        typeGenericArguments = methodGenericArguments = Type.EmptyTypes;
        hashtable = null;
      }

      if (method.MemberType == MemberTypes.Method) {
        if (targetType != null && !IsTypeMatched(hashtable, methodGenericArguments, ((MethodInfo)method).DeclaringType, targetType, false, out dummy)) {
          goto NotMatch;
        }
        if (returnType != null && returnType != typeof(object) && !IsTypeMatched(hashtable, methodGenericArguments, ((MethodInfo)method).ReturnType, returnType, true, out dummy)) {
          goto NotMatch;
        }
      }
      for (int i = 0; i < actualParameterTypes.Length; i++) {
        if (parameterTypes[i] == null ? actualParameterTypes[i].IsValueType : !IsTypeMatched(hashtable, methodGenericArguments, actualParameterTypes[i], parameterTypes[i], false, out actualParameterTypes[i])) {
          goto NotMatch;
        }
      }

      if (hasTypeArguments) {
        // map inferred generic arguments to out arrays
        for (int i = 0; i < typeGenericArguments.Length; i++) {
          typeGenericArguments[i] = (Type)hashtable[typeGenericArguments[i]];
        }
        for (int i = 0; i < methodGenericArguments.Length; i++) {
          methodGenericArguments[i] = (Type)hashtable[methodGenericArguments[i]];
        }
      }
      return true;

      NotMatch:
      typeGenericArguments = methodGenericArguments = actualParameterTypes = null;
      return false;
    }

    private static bool IsTypeMatched(Hashtable hashtable, Type[] methodGenericArguments, Type declaredType, Type actualType, bool contravariance, out Type actualParameterType) {
      if (declaredType.ContainsGenericParameters) {
        return MapGenericArguments(hashtable, methodGenericArguments, declaredType, actualType, out actualParameterType);
      }
      actualParameterType = declaredType;
      return contravariance ? declaredType.IsOf(actualType) : actualType.IsOf(declaredType);
    }

    private static bool MapGenericArguments(Hashtable hashtable, Type[] methodGenericArguments, Type declaredType, Type actualType, out Type actualParameterType) {
      // case where declared type is a generic parameter e.g. T
      if (declaredType.IsGenericParameter) {
        // here we infer generic parameters of both method and its declaring type
        // then return the parameter type as if the generic type is constructed with generic arguments
        actualParameterType = (Type)hashtable[declaredType];

        // if this generic parameter is not inferred before
        // we assume that this actual type in the same position infers this generic parameter
        if (actualParameterType == null) {
          hashtable[declaredType] = actualType;

          // the method signature will still contains this T if the declared type T is a method generic parameter
          // otherwise the method signature will contains the inferred type <actualType>
          // after generic type is constructed with generic arguments
          actualParameterType = Array.IndexOf(methodGenericArguments, declaredType) >= 0 ? declaredType : actualType;
          return true;
        }

        // if this generic parameter has been inferred before
        // and the actual type is compatible to the inferred type
        if (actualType.IsOf(actualParameterType)) {
          actualParameterType = Array.IndexOf(methodGenericArguments, declaredType) >= 0 ? declaredType : actualType;
          return true;
        }
        return false;
      }

      // case where declared type contains generic arguments e.g. ClassOrInterface<T>
      if (declaredType.ContainsGenericParameters) {
        Type[] srcTypeArguments = declaredType.GetGenericArguments();
        Type[] dstTypeArguments;

        // if the actual type is compatible to the declared type e.g. SubClassOrInterface : ClassOrInterface<T>
        // recursively map all of the generic arguments
        // and construct a new type from <declaredType> that will eventually appears on the method signature
        if (actualType.IsOf(declaredType.GetGenericTypeDefinition(), out dstTypeArguments)) {
          for (int i = srcTypeArguments.Length - 1; i >= 0; i--) {
            MapGenericArguments(hashtable, methodGenericArguments, srcTypeArguments[i], dstTypeArguments[i], out dstTypeArguments[i]);
          }
          actualParameterType = declaredType.GetGenericTypeDefinition().MakeGenericType(dstTypeArguments);
          return true;
        }
        actualParameterType = null;
        return false;
      }

      // simple case there is no generic parameters to infer
      if (actualType.IsOf(declaredType)) {
        actualParameterType = declaredType;
        return true;
      }
      actualParameterType = null;
      return false;
    }
  }
}
