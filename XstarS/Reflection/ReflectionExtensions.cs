﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace XstarS.Reflection
{
    /// <summary>
    /// 提供反射相关的扩展方法。
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// 确定当前 <see cref="MemberInfo"/> 是否是运算符方法。
        /// </summary>
        /// <param name="method">要确定是否为运算符的 <see cref="MemberInfo"/> 对象。</param>
        /// <returns>若 <paramref name="method"/> 是运算符方法，
        /// 则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="method"/> 为 <see langword="null"/>。</exception>
        public static bool IsOperator(this MethodInfo method)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return method.IsStatic && method.IsSpecialName && method.Name.StartsWith("op_");
        }

        /// <summary>
        /// 确定当前 <see cref="MemberInfo"/> 是否是强制转换运算符方法。
        /// </summary>
        /// <param name="method">要确定是否为运算符的 <see cref="MemberInfo"/> 对象。</param>
        /// <returns>若 <paramref name="method"/> 是强制转换运算符方法，
        /// 则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="method"/> 为 <see langword="null"/>。</exception>
        public static bool IsConversionOperator(MethodInfo method)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return method.IsOperator() &&
                (method.Name == "op_Implicit" || method.Name == "op_Explicit");
        }

        /// <summary>
        /// 获取当前 <see cref="Type"/> 的默认值，即通过 <see langword="default"/> 表达式获得的值。
        /// </summary>
        /// <param name="type">要获取默认值的 <see cref="Type"/> 对象。</param>
        /// <returns><paramref name="type"/> 类型的默认值。</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type"/> 为 <see langword="null"/>。</exception>
        public static object GetDefaultValue(this Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// 检索当前 <see cref="Type"/> 可以访问的所有事件的集合。
        /// </summary>
        /// <param name="type">要检索事件的 <see cref="Type"/> 对象。</param>
        /// <returns><paramref name="type"/> 可以访问的所有事件的集合。</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type"/> 为 <see langword="null"/>。</exception>
        public static IEnumerable<EventInfo> GetAccessibleEvents(this Type type) =>
            type.GetAccessibleMembers(RuntimeReflectionExtensions.GetRuntimeEvents);

        /// <summary>
        /// 检索当前 <see cref="Type"/> 可以访问的所有字段的集合。
        /// </summary>
        /// <param name="type">要检索字段的 <see cref="Type"/> 对象。</param>
        /// <returns><paramref name="type"/> 可以访问的所有字段的集合。</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type"/> 为 <see langword="null"/>。</exception>
        public static IEnumerable<FieldInfo> GetAccessibleFields(this Type type) =>
            type.GetAccessibleMembers(RuntimeReflectionExtensions.GetRuntimeFields);

        /// <summary>
        /// 检索当前 <see cref="Type"/> 可以访问的所有方法的集合。
        /// </summary>
        /// <param name="type">要检索方法的 <see cref="Type"/> 对象。</param>
        /// <returns><paramref name="type"/> 可以访问的所有方法的集合。</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type"/> 为 <see langword="null"/>。</exception>
        public static IEnumerable<MethodInfo> GetAccessibleMethods(this Type type) =>
            type.GetAccessibleMembers(RuntimeReflectionExtensions.GetRuntimeMethods);

        /// <summary>
        /// 检索当前 <see cref="Type"/> 可以访问的所有属性的集合。
        /// </summary>
        /// <param name="type">要检索属性的 <see cref="Type"/> 对象。</param>
        /// <returns><paramref name="type"/> 可以访问的所有属性的集合。</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type"/> 为 <see langword="null"/>。</exception>
        public static IEnumerable<PropertyInfo> GetAccessibleProperties(this Type type) =>
            type.GetAccessibleMembers(RuntimeReflectionExtensions.GetRuntimeProperties);

        /// <summary>
        /// 检索当前 <see cref="Type"/> 可以访问的所有指定类型的成员的集合。
        /// </summary>
        /// <typeparam name="TMemberInfo">要检索的成员的类型。</typeparam>
        /// <param name="type">要检索成员的 <see cref="Type"/> 对象。</param>
        /// <param name="memberFinder">检索指定类型的成员的 <see cref="Func{T, TResult}"/> 委托。</param>
        /// <returns><paramref name="type"/> 可以访问的所有指定类型的成员的集合。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/>
        /// 或 <paramref name="memberFinder"/> 为 <see langword="null"/>。</exception>
        private static IEnumerable<TMemberInfo> GetAccessibleMembers<TMemberInfo>(
            this Type type, Func<Type, IEnumerable<TMemberInfo>> memberFinder)
            where TMemberInfo : MemberInfo
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (memberFinder is null)
            {
                throw new ArgumentNullException(nameof(memberFinder));
            }

            var result = new List<TMemberInfo>(memberFinder(type));
            if (type.IsInterface)
            {
                foreach (var iType in type.GetInterfaces())
                {
                    if (iType.IsVisible)
                    {
                        result.AddRange(memberFinder(iType));
                    }
                }
                result.AddRange(memberFinder(typeof(object)));
            }
            return result.ToArray();
        }
    }
}
