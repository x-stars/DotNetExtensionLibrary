﻿using System;

namespace XstarS
{
    /// <summary>
    /// 提供类型无关的通用扩展方法。
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// 创建当前对象的浅表副本。
        /// </summary>
        /// <param name="source">一个 <see cref="object"/> 类型的对象。</param>
        /// <returns><paramref name="source"/> 的浅表副本。</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> 为 <see langword="null"/>。</exception>
        public static object ShallowClone(this object source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new CloneableObject(source).ShallowClone();
        }

        /// <summary>
        /// 创建当前对象的深度副本。
        /// </summary>
        /// <param name="source">一个 <see cref="object"/> 类型的对象。</param>
        /// <returns><paramref name="source"/> 的深度副本。</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> 为 <see langword="null"/>。</exception>
        public static object DeepClone(this object source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new CloneableObject(source).DeepClone();
        }

        /// <summary>
        /// 确定当前对象与指定对象的所有字段的值（对数组则是所有元素的值）是否相等。
        /// 将递归比较至字段（元素）为 .NET 基元类型（<see cref="Type.IsPrimitive"/>）或指针类型。
        /// </summary>
        /// <remarks>基于反射调用，可能存在性能问题。</remarks>
        /// <param name="source">一个 <see cref="object"/> 类型的对象。</param>
        /// <param name="other">要与当前对象进行比较的对象。</param>
        /// <returns>若 <paramref name="source"/> 与 <paramref name="other"/> 的所有实例字段（元素）的值都相等，
        /// 则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool ValueEquals(this object source, object other)
        {
            return new ValueEquatablePair(source, other).ValueEquals;
        }
    }
}
