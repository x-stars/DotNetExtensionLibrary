﻿using System;
using System.Diagnostics;

namespace XstarS.Diagnostics
{
    /// <summary>
    /// 提供用于程序诊断和测试的帮助方法。
    /// </summary>
    public static class DiagnosticsHelper
    {
        /// <summary>
        /// 返回指定过程的执行指定次数的总时间。
        /// </summary>
        /// <param name="process">一个过程的 <see cref="Action"/> 委托。</param>
        /// <param name="repeat">过程重复执行的次数。</param>
        /// <returns><paramref name="process"/> 执行 <paramref name="repeat"/> 次的总时间。</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="process"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="repeat"/> 小于 1。</exception>
        public static TimeSpan ExecutingTime(Action process, int repeat = 1)
        {
            if (process is null)
            {
                throw new ArgumentNullException(nameof(process));
            }
            if (repeat < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(repeat));
            }

            var sw = new Stopwatch();
            for (int count = 0; count < repeat; count++)
            {
                sw.Start();
                process();
                sw.Stop();
            }
            return sw.Elapsed;
        }

        /// <summary>
        /// 返回指定过程的执行指定次数的总时间。
        /// </summary>
        /// <typeparam name="T">过程的返回值的类型。</typeparam>
        /// <param name="process">一个过程的 <see cref="Func{TResult}"/> 委托。</param>
        /// <param name="repeat">过程重复执行的次数。</param>
        /// <returns><paramref name="process"/> 执行 <paramref name="repeat"/> 次的总时间。</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="process"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="repeat"/> 小于 1。</exception>
        public static TimeSpan ExecutingTime<T>(Func<T> process, int repeat = 1)
        {
            if (process is null)
            {
                throw new ArgumentNullException(nameof(process));
            }
            if (repeat < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(repeat));
            }

            var sw = new Stopwatch();
            for (int count = 0; count < repeat; count++)
            {
                sw.Start();
                process();
                sw.Stop();
            }
            return sw.Elapsed;
        }

        /// <summary>
        /// 将当前对象表示为字符串。
        /// </summary>
        /// <param name="value">要表示为字符串的对象。</param>
        /// <returns>表示 <paramref name="value"/> 的字符串。</returns>
        public static string RepresentToString<T>(this T value) =>
            ObjectRepresenter.Represent(value);
    }
}
