﻿using System;

namespace XstarS.Diagnostics
{
    /// <summary>
    /// 为无参数的字符串表示对象 <see cref="IRepresenter{T}"/> 提供抽象基类。
    /// </summary>
    /// <typeparam name="T">要表示为字符串的对象的类型。</typeparam>
    [Serializable]
    public abstract class SimpleRepresenter<T> : Representer<T>
    {
        /// <summary>
        /// 初始化 <see cref="SimpleRepresenter{T}"/> 类的新实例。
        /// </summary>
        protected SimpleRepresenter() { }

        /// <summary>
        /// 确定指定的对象是否等于当前对象。
        /// </summary>
        /// <param name="obj">要与当前对象进行比较的对象。</param>
        /// <returns>如果指定的对象等于当前对象，
        /// 则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
        public sealed override bool Equals(object obj) =>
            this.GetType() == obj?.GetType();

        /// <summary>
        /// 获取当前对象的哈希代码。
        /// </summary>
        /// <returns>当前对象的哈希代码。</returns>
        public sealed override int GetHashCode() =>
            this.GetType().GetHashCode() * -1521134295 +
            typeof(SimpleRepresenter<T>).GetHashCode();
    }
}
