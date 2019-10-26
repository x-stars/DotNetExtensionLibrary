﻿using System;
using System.Collections.Generic;
using System.Reflection;
using XstarS.Collections.Specialized;

namespace XstarS
{
    /// <summary>
    /// 提供获取指定对象基于值的哈希代码的方法。
    /// </summary>
    [Serializable]
    internal sealed class ValueHashableObject
    {
        /// <summary>
        /// 要获取基于值的哈希代码的对象。
        /// </summary>
        public readonly object Value;

        /// <summary>
        /// 当前对象的基于值的哈希代码。
        /// </summary>
        [NonSerialized]
        private int HashCode;

        /// <summary>
        /// 已经计算过哈希代码的对象。
        /// </summary>
        [NonSerialized]
        private HashSet<object> Computed;

        /// <summary>
        /// 使用要获取基于值的哈希代码的对象初始化 <see cref="ValueHashableObject"/> 类的新实例。
        /// </summary>
        /// <param name="value">要获取基于值的哈希代码的对象。</param>
        public ValueHashableObject(object value)
        {
            this.Value = value;
        }

        /// <summary>
        /// 获取当前实例包含的对象基于值的哈希代码。
        /// 将递归计算至对象的字段（数组的元素）为 .NET 基元类型 (<see cref="Type.IsPrimitive"/>)、
        /// 字符串 <see cref="string"/> 或指针类型 (<see cref="Type.IsPointer"/>)。
        /// </summary>
        /// <returns><see cref="ValueHashableObject.Value"/> 基于值的哈希代码。</returns>
        public int GetValueHashCode()
        {
            this.HashCode = 0;
            this.Computed = new HashSet<object>(
                ReferenceEqualityComparer<object>.Default);

            this.AppendValueHashCode(this.Value);
            int hashCode = this.HashCode;

            this.Computed = null;
            this.HashCode = 0;
            return hashCode;
        }

        /// <summary>
        /// 将指定哈希代码附加到当前哈希代码中。
        /// </summary>
        /// <param name="hashCode">要附加到当前哈希代码的哈希代码。</param>
        private void AppendHashCode(int hashCode)
        {
            this.HashCode = this.HashCode * -1521134295 + hashCode;
        }

        /// <summary>
        /// 将指定对象基于值的哈希代码附加到当前的哈希代码中。
        /// </summary>
        /// <param name="value">要将其基于值的哈希代码追加到当前哈希代码的对象。</param>
        private void AppendValueHashCode(object value)
        {
            // 空引用。
            if (value is null) { this.AppendEmptyValueHashCode(); return; }
            // 已计算过的对象。
            if (!this.Computed.Add(value)) { this.AppendEmptyValueHashCode(); return; }

            // 根据类型附加哈希代码。
            var type = value.GetType();
            if (type.IsPrimitive) { this.AppendPrimitiveValueHashCode(value); }
            else if (type == typeof(string)) { this.AppendStringValueHashCode((string)value); }
            else if (type == typeof(Pointer)) { this.AppendPointerValueHashCode((Pointer)value); }
            else if (type.IsArray) { this.AppendArrayValueHashCode((Array)value); }
            else { this.AppendObjectValueHashCode(value); }
        }

        /// <summary>
        /// 将一个空哈希代码附加到当前的哈希代码中。
        /// </summary>
        private void AppendEmptyValueHashCode()
        {
            this.AppendHashCode(0);
        }

        /// <summary>
        /// 将指定基元类型对象 (<see cref="Type.IsPrimitive"/>) 基于值的哈希代码附加到当前的哈希代码中。
        /// </summary>
        /// <param name="value">要将其基于值的哈希代码追加到当前哈希代码的基元类型对象。</param>
        private void AppendPrimitiveValueHashCode(object value)
        {
            this.AppendHashCode(value.GetHashCode());
        }

        /// <summary>
        /// 将指定字符串 <see cref="string"/> 基于值的哈希代码附加到当前的哈希代码中。
        /// </summary>
        /// <param name="value">要将其基于值的哈希代码追加到当前哈希代码的字符串。</param>
        private void AppendStringValueHashCode(string value)
        {
            this.AppendHashCode(value.GetHashCode());
        }

        /// <summary>
        /// 将指定指针包装 <see cref="Pointer"/> 基于值的哈希代码附加到当前的哈希代码中。
        /// </summary>
        /// <param name="value">要将其基于值的哈希代码追加到当前哈希代码的指针包装。</param>
        private unsafe void AppendPointerValueHashCode(Pointer value)
        {
            this.AppendHashCode(((IntPtr)Pointer.Unbox(value)).GetHashCode());
        }

        /// <summary>
        /// 将指定数组 <see cref="Array"/> 基于所有元素的值的哈希代码附加到当前的哈希代码中。
        /// 将递归计算至元素为 .NET 基元类型、字符串或指针类型。
        /// </summary>
        /// <param name="value">要将其基于值的哈希代码追加到当前哈希代码的数组。</param>
        private void AppendArrayValueHashCode(Array value)
        {
            this.AppendEmptyValueHashCode();

            var typeArray = value.GetType();
            // 指针数组，反射调用无法访问的 Get 方法。
            if (typeArray.GetElementType().IsPointer)
            {
                var methodGet = typeArray.GetMethod("Get");
                for (int i = 0; i < value.Length; i++)
                {
                    this.AppendValueHashCode(
                        methodGet.Invoke(value, Array.ConvertAll(
                            value.OffsetToIndices(i), index => (object)index)));
                }
            }
            // 一般数组。
            else
            {
                bool isMultiDim = value.Rank > 1;
                for (int i = 0; i < value.Length; i++)
                {
                    this.AppendValueHashCode(isMultiDim ?
                        value.GetValue(value.OffsetToIndices(i)) : value.GetValue(i));
                }
            }
        }

        /// <summary>
        /// 将指定对象基于所有字段的值的哈希代码附加到当前的哈希代码中。
        /// 将递归计算至字段为 .NET 基元类型、字符串或指针类型。
        /// </summary>
        /// <param name="value">要将其基于值的哈希代码追加到当前哈希代码的对象。</param>
        private void AppendObjectValueHashCode(object value)
        {
            this.AppendEmptyValueHashCode();

            // 循环获取基类。
            for (var type = value.GetType(); !(type is null); type = type.BaseType)
            {
                // 获取每个实例字段。
                var fields = type.GetFields(BindingFlags.DeclaredOnly |
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                // 依次递归获取每个字段的哈希代码。
                foreach (var field in fields)
                {
                    this.AppendValueHashCode(field.GetValue(value));
                }
            }
        }
    }
}
