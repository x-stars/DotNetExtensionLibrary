﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace XstarS.Collections.Specialized
{
    /// <summary>
    /// 用于比较对象引用是否相等的比较器。
    /// </summary>
    /// <typeparam name="T">要比较的对象的类型。</typeparam>
    [Serializable]
    public sealed class ReferenceEqualityComparer<T> : EqualityComparer<T>
    {
        /// <summary>
        /// 初始化 <see cref="ReferenceEqualityComparer{T}"/> 类的新实例。
        /// </summary>
        public ReferenceEqualityComparer() { }

        /// <summary>
        /// 获取默认的 <see cref="ReferenceEqualityComparer{T}"/> 实例。
        /// </summary>
        public static new ReferenceEqualityComparer<T> Default { get; } = new ReferenceEqualityComparer<T>();

        /// <summary>
        /// 确定两对象的引用是否相等。
        /// </summary>
        /// <param name="x">要比较的第一个对象。</param>
        /// <param name="y">要比较的第二个对象。</param>
        /// <returns>若 <paramref name="x"/> 与 <paramref name="y"/> 的引用相等，
        /// 则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public override bool Equals(T x, T y) => object.ReferenceEquals(x, y);

        /// <summary>
        /// 获取指定对象基于引用的哈希代码。
        /// </summary>
        /// <param name="obj">要获取哈希代码的对象。</param>
        /// <returns><paramref name="obj"/> 基于引用的哈希代码。</returns>
        public override int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
