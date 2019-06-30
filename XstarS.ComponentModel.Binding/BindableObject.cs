﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XstarS.ComponentModel
{
    /// <summary>
    /// 为数据绑定类型提供抽象基类实现，包含属性发生更改时通知客户端的方法。
    /// </summary>
    [Serializable]
    public abstract class BindableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// 初始化 <see cref="BindableObject"/> 类的新实例。
        /// </summary>
        protected BindableObject() { }

        /// <summary>
        /// 属性改变事件，在属性值更改时发生。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性改变事件。
        /// </summary>
        /// <param name="propertyName">已更改属性的名称，可由编译器自动获取。</param>
        protected virtual void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(
                this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 更改属性的值，并通知客户端属性发生更改。
        /// </summary>
        /// <remarks>
        /// 应在属性的 <see langword="set"/> 处调用此方法，
        /// 在更改属性值的同时触发 <see cref="INotifyPropertyChanged.PropertyChanged"/> 事件。
        /// </remarks>
        /// <typeparam name="T">属性的类型。</typeparam>
        /// <param name="item">属性对应的字段。</param>
        /// <param name="value">属性的新值，一般为 <see langword="value"/> 关键字。</param>
        /// <param name="propertyName">属性的名称，可由编译器自动获取。</param>
        protected void SetProperty<T>(ref T item, T value,
            [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(item, value))
            {
                item = value;
                this.OnPropertyChanged(propertyName);
            }
        }
    }
}
