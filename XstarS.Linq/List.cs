﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace XstarS.Linq
{
    /// <summary>
    /// 提供 <see cref="IList{T}"/> 的扩展方法的静态类。
    /// </summary>
    public static class List
    {
        /// <summary>
        /// 将指定集合的元素添加到 <see cref="IList{T}"/> 的末尾。
        /// </summary>
        /// <typeparam name="T"><see cref="IList{T}"/> 中元素的类型。</typeparam>
        /// <param name="source"><see cref="IList{T}"/> 实例，扩展方法源。</param>
        /// <param name="collection">
        /// 应将其元素添加到 <see cref="IList{T}"/> 的末尾的集合。
        /// 集合自身不能为 <see langword="null"/>，但它可以包含为 <see langword="null"/> 的元素。
        /// </param>
        /// <exception cref="ArgumentException"><paramref name="collection"/> 为 <see langword="null"/>。</exception>
        public static void AddRange<T>(this IList<T> source, IEnumerable<T> collection)
        {
            foreach (var item in collection) { source.Add(item); }
        }

        /// <summary>
        /// 将指定集合的元素插入到 <see cref="IList{T}"/> 的的指定索引处。
        /// </summary>
        /// <typeparam name="T"><see cref="IList{T}"/> 中元素的类型。</typeparam>
        /// <param name="source"><see cref="IList{T}"/> 实例，扩展方法源。</param>
        /// <param name="index">应在此处插入新元素的从零开始的索引。</param>
        /// <param name="collection">
        /// 应将其元素添加到 <see cref="IList{T}"/> 的末尾的集合。
        /// 集合自身不能为 <see langword="null"/>，但它可以包含为 <see langword="null"/> 的元素。
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/> 小于 0，
        /// 或 <paramref name="index"/> 大于 <see cref="ICollection{T}.Count"/>。
        /// </exception>
        public static void InsertRange<T>(this IList<T> source, int index, IEnumerable<T> collection)
        {
            int i = index;
            foreach (var item in collection)
            {
                source.Insert(i, item);
                i++;
            }
        }

        /// <summary>
        /// 从 <see cref="IList{T}"/> 中移除一定范围的元素。
        /// </summary>
        /// <typeparam name="T"><see cref="IList{T}"/> 中元素的类型。</typeparam>
        /// <param name="source"><see cref="IList{T}"/> 实例，扩展方法源。</param>
        /// <param name="index">要移除的元素范围的从零开始的起始索引。</param>
        /// <param name="count">要移除的元素数。</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> 小于 0。或 <paramref name="count"/> 小于 0。
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> 和 <paramref name="count"/>
        /// 不表示 <see cref="IList{T}"/> 中元素的有效范围。
        /// </exception>
        public static void RemoveRange<T>(this IList<T> source, int index, int count)
        {
            for (int i = 0; i < count; i++) { source.RemoveAt(index); }
        }
    }
}
