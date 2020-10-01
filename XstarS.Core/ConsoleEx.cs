﻿using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using mstring = System.Text.StringBuilder;

namespace XstarS
{
    /// <summary>
    /// 提供控制台应用程序的输入流、输出流和错误流的扩展方法。
    /// </summary>
    public static class ConsoleEx
    {
        /// <summary>
        /// 提供类似于 <see cref="int.Parse(string)"/> 的字符串解析方法的委托。
        /// </summary>
        /// <typeparam name="T">要解析为的数值类型。</typeparam>
        private static class ParseMethod<T>
        {
            /// <summary>
            /// 表示 <typeparamref name="T"/> 类型的字符串解析方法的委托。
            /// </summary>
            internal static readonly Converter<string, T> Delegate = ParseMethod<T>.CreateDelegate();

            /// <summary>
            /// 创建 <typeparamref name="T"/> 类型的字符串解析方法的委托。
            /// </summary>
            /// <returns><typeparamref name="T"/> 类型的字符串解析方法的委托。</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage(
                "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
            private static Converter<string, T> CreateDelegate()
            {
                try
                {
                    var method = typeof(T).GetMethod(nameof(int.Parse), new[] { typeof(string) });
                    return (!(method is null) && method.IsStatic && (method.ReturnType == typeof(T))) ?
                        (Converter<string, T>)method.CreateDelegate(typeof(Converter<string, T>)) : null;
                }
                catch (Exception) { return null; }
            }
        }

        /// <summary>
        /// 表示读取方法的同步锁对象。
        /// </summary>
        private static readonly object ReadSyncRoot = new object();

        /// <summary>
        /// 表示写入方法的同步锁对象。
        /// </summary>
        private static readonly object WriteSyncRoot = new object();

        /// <summary>
        /// 表示所有空白字符的集合。
        /// </summary>
        private static readonly char[] WhiteSpaces = Enumerable.Range(
            char.MinValue, char.MaxValue).Select(Convert.ToChar).Where(char.IsWhiteSpace).ToArray();

        /// <summary>
        /// 将当前字符串表示形式转换为其等效的数值形式。
        /// </summary>
        /// <typeparam name="T">数值形式的类型，
        /// 应为枚举类型或包含类似与 <see cref="int.Parse(string)"/> 的方法。</typeparam>
        /// <param name="text">包含要转换数值的字符串。</param>
        /// <returns>与 <paramref name="text"/> 等效的数值形式。</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="text"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="text"/> 不表示有效的值。</exception>
        /// <exception cref="FormatException"><paramref name="text"/> 的格式不正确。</exception>
        /// <exception cref="InvalidCastException">指定的从字符串的转换无效。</exception>
        /// <exception cref="OverflowException">
        /// <paramref name="text"/> 表示的值超出了 <typeparamref name="T"/> 能表示的范围。</exception>
        public static T ParseAs<T>(this string text)
        {
            return !(ParseMethod<T>.Delegate is null) ? ParseMethod<T>.Delegate.Invoke(text) :
                typeof(T).IsEnum ? (T)Enum.Parse(typeof(T), text) : throw new InvalidCastException();
        }

        /// <summary>
        /// 从标准输入流读取下一个字符串值。
        /// </summary>
        /// <returns>输入流中的下一个字符串值；
        /// 如果当前没有更多的可用字符串值，则为 <see langword="null"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 下一个字符串值的字符数大于 <see cref="int.MaxValue"/>。</exception>
        /// <exception cref="OutOfMemoryException">
        /// 没有足够的内存来为下一个字符串值分配缓冲区。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static string ReadToken()
        {
            lock (ConsoleEx.ReadSyncRoot)
            {
                var iChar = -1;
                while ((iChar = Console.Read()) != -1)
                {
                    if (!char.IsWhiteSpace((char)iChar)) { break; }
                }
                var token = new mstring();
                token.Append((char)iChar);
                while ((iChar = Console.Read()) != -1)
                {
                    if (char.IsWhiteSpace((char)iChar)) { break; }
                    token.Append((char)iChar);
                }
                return token.ToString();
            }
        }

        /// <summary>
        /// 从标准输入流读取下一个字符串值，并将其转换为指定的数值形式。
        /// </summary>
        /// <typeparam name="T">数值形式的类型。</typeparam>
        /// <returns>输入流中的下一个字符串值的数值形式。</returns>
        /// <exception cref="ArgumentNullException">当前没有更多的可用字符串值。</exception>
        /// <exception cref="ArgumentException">读取到的字符串不表示有效的值。</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 下一个字符串值的字符数大于 <see cref="int.MaxValue"/>。</exception>
        /// <exception cref="OutOfMemoryException">
        /// 没有足够的内存来为下一个字符串值分配缓冲区。</exception>
        /// <exception cref="FormatException">读取到的字符串的格式不正确。</exception>
        /// <exception cref="InvalidCastException">指定的从字符串的转换无效。</exception>
        /// <exception cref="OverflowException">
        /// 读取到的字符串表示的值超出了 <typeparamref name="T"/> 能表示的范围。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static T ReadTokenAs<T>() => ConsoleEx.ParseAs<T>(ConsoleEx.ReadToken());

        /// <summary>
        /// 从标准输入流读取下一行的所有字符串值。
        /// </summary>
        /// <returns>输入流中的下一行包含的所有字符串值。</returns>
        /// <exception cref="ArgumentNullException">当前没有更多的可用行。</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 下一行中的字符的字符数大于 <see cref="int.MaxValue"/>。</exception>
        /// <exception cref="OutOfMemoryException">
        /// 没有足够的内存来为下一行的字符串分配缓冲区。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static string[] ReadLineTokens() =>
            Console.ReadLine().Split(ConsoleEx.WhiteSpaces, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// 从标准输入流读取下一行字符，并将其包含的所有字符串值转换为指定的数值形式。
        /// </summary>
        /// <typeparam name="T">数值形式的类型。</typeparam>
        /// <returns>输入流中的下一行包含的所有字符串值的数值形式。</returns>
        /// <exception cref="ArgumentNullException">当前没有更多的可用行。</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 下一行中的字符的字符数大于 <see cref="int.MaxValue"/>。</exception>
        /// <exception cref="ArgumentException">读取到的字符串不表示有效的值。</exception>
        /// <exception cref="OutOfMemoryException">
        /// 没有足够的内存来为下一行的字符串分配缓冲区。</exception>
        /// <exception cref="FormatException">读取到的字符串的格式不正确。</exception>
        /// <exception cref="InvalidCastException">指定的从字符串的转换无效。</exception>
        /// <exception cref="OverflowException">
        /// 读取到的字符串表示的值超出了 <typeparamref name="T"/> 能表示的范围。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static T[] ReadLineTokensAs<T>() =>
            Array.ConvertAll(ConsoleEx.ReadLineTokens(), ConsoleEx.ParseAs<T>);

        /// <summary>
        /// 从标准输入流读取到末尾的所有字符。
        /// </summary>
        /// <returns>输入流到末尾的所有字符。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 到末尾的字符的字符数大于 <see cref="int.MaxValue"/>。</exception>
        /// <exception cref="OutOfMemoryException">没有足够的内存来为到末尾的字符串分配缓冲区。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ReadToEnd() => Console.In.ReadToEnd();

        /// <summary>
        /// 从标准输入流读取到末尾的所有字符串值。
        /// </summary>
        /// <returns>输入流到末尾的所有字符串值。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 到末尾的字符的字符数大于 <see cref="int.MaxValue"/>。</exception>
        /// <exception cref="OutOfMemoryException">
        /// 没有足够的内存来为到末尾的字符串分配缓冲区。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static string[] ReadTokensToEnd() =>
            ConsoleEx.ReadToEnd().Split(ConsoleEx.WhiteSpaces, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// 从标准输入流读取到末尾的所有字符，并将其包含的所有字符串值转换为指定的数值形式。
        /// </summary>
        /// <typeparam name="T">数值形式的类型。</typeparam>
        /// <returns>输入流读取到末尾的所有字符串值的数值形式。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 下一行中的字符的字符数大于 <see cref="int.MaxValue"/>。</exception>
        /// <exception cref="ArgumentException">读取到的字符串不表示有效的值。</exception>
        /// <exception cref="OutOfMemoryException">
        /// 没有足够的内存来为下一行的字符串分配缓冲区。</exception>
        /// <exception cref="FormatException">读取到的字符串的格式不正确。</exception>
        /// <exception cref="InvalidCastException">指定的从字符串的转换无效。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static T[] ReadTokensToEndAs<T>() =>
            Array.ConvertAll(ConsoleEx.ReadTokensToEnd(), ConsoleEx.ParseAs<T>);

        /// <summary>
        /// 将指定的字符串值以指定的前景色和背景色写入标准输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <param name="background">要使用的控制台背景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteInColor(string value, ConsoleColor foreground, ConsoleColor background)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                Console.Write(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定的字符串值以指定的前景色写入标准输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteInColor(string value, ConsoleColor foreground)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ResetColor();
                Console.ForegroundColor = foreground;
                Console.Write(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定对象的文本表示形式以指定的前景色和背景色写入标准输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <param name="background">要使用的控制台背景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteInColor(object value, ConsoleColor foreground, ConsoleColor background)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                Console.Write(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定对象的文本表示形式以指定的前景色写入标准输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteInColor(object value, ConsoleColor foreground)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ResetColor();
                Console.ForegroundColor = foreground;
                Console.Write(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定的字符串值（后跟当前行终止符）以指定的前景色和背景色写入标准输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <param name="background">要使用的控制台背景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteLineInColor(string value, ConsoleColor foreground, ConsoleColor background)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                Console.WriteLine(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定的字符串值（后跟当前行终止符）以指定的前景色写入标准输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteLineInColor(string value, ConsoleColor foreground)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ResetColor();
                Console.ForegroundColor = foreground;
                Console.WriteLine(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定对象的文本表示形式（后跟当前行终止符）以指定的前景色和背景色写入标准输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <param name="background">要使用的控制台背景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteLineInColor(object value, ConsoleColor foreground, ConsoleColor background)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                Console.WriteLine(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定对象的文本表示形式（后跟当前行终止符）以指定的前景色写入标准输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteLineInColor(object value, ConsoleColor foreground)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ResetColor();
                Console.ForegroundColor = foreground;
                Console.WriteLine(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定的字符串值写入标准错误输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteError(string value) => Console.Error.Write(value);

        /// <summary>
        /// 将指定对象的文本表示形式写入标准错误输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteError(object value) => Console.Error.Write(value);

        /// <summary>
        /// 将指定的 Unicode 字符数组写入标准错误输出流。
        /// </summary>
        /// <param name="buffer">Unicode 字符数组。</param>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteError(char[] buffer) => Console.Error.Write(buffer);

        /// <summary>
        /// 将指定的 Unicode 字符子数组写入标准错误输出流。
        /// </summary>
        /// <param name="buffer">Unicode 字符的数组。</param>
        /// <param name="index"><paramref name="buffer"/> 中的起始位置。</param>
        /// <param name="count">要写入的字符数。</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> 或 <paramref name="count"/> 小于零。</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> 加 <paramref name="count"/>
        /// 指定不在 <paramref name="buffer"/> 内的位置。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteError(char[] buffer, int index, int count) =>
            Console.Error.Write(buffer, index, count);

        /// <summary>
        /// 使用指定的格式信息，将指定对象的文本表示形式写入标准错误输出流。
        /// </summary>
        /// <param name="format">复合格式字符串。</param>
        /// <param name="arg">要使用 <paramref name="format"/> 写入的对象的数组。</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="format"/> 或 <paramref name="arg"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="FormatException"><paramref name="format"/> 中的格式规范无效。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteError(string format, params object[] arg) =>
            Console.Error.Write(format, arg);

        /// <summary>
        /// 将当前行终止符写入标准错误输出流。
        /// </summary>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteErrorLine() => Console.Error.WriteLine();

        /// <summary>
        /// 将指定的字符串值（后跟当前行终止符）写入标准错误输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteErrorLine(string value) => Console.Error.WriteLine(value);

        /// <summary>
        /// 将指定对象的文本表示形式（后跟当前行终止符）写入标准错误输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteErrorLine(object value) => Console.Error.WriteLine(value);

        /// <summary>
        /// 将指定的 Unicode 字符数组（后跟当前行终止符）写入标准错误输出流。
        /// </summary>
        /// <param name="buffer">Unicode 字符数组。</param>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteErrorLine(char[] buffer) => Console.Error.WriteLine(buffer);

        /// <summary>
        /// 将指定的 Unicode 字符子数组（后跟当前行终止符）写入标准错误输出流。
        /// </summary>
        /// <param name="buffer">Unicode 字符的数组。</param>
        /// <param name="index"><paramref name="buffer"/> 中的起始位置。</param>
        /// <param name="count">要写入的字符数。</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> 或 <paramref name="count"/> 小于零。</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> 加 <paramref name="count"/>
        /// 指定不在 <paramref name="buffer"/> 内的位置。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteErrorLine(char[] buffer, int index, int count) =>
            Console.Error.WriteLine(buffer, index, count);

        /// <summary>
        /// 使用指定的格式信息，将指定对象的文本表示形式（后跟当前行终止符）写入标准错误输出流。
        /// </summary>
        /// <param name="format">复合格式字符串。</param>
        /// <param name="arg">要使用 <paramref name="format"/> 写入的对象的数组。</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="format"/> 或 <paramref name="arg"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="FormatException"><paramref name="format"/> 中的格式规范无效。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteErrorLine(string format, params object[] arg) =>
            Console.Error.WriteLine(format, arg);

        /// <summary>
        /// 将指定的字符串值以指定的前景色和背景色写入标准错误输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <param name="background">要使用的控制台背景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteErrorInColor(string value, ConsoleColor foreground, ConsoleColor background)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                ConsoleEx.WriteError(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定的字符串值以指定的前景色写入标准错误输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteErrorInColor(string value, ConsoleColor foreground)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ResetColor();
                Console.ForegroundColor = foreground;
                ConsoleEx.WriteError(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定对象的文本表示形式以指定的前景色和背景色写入标准错误输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <param name="background">要使用的控制台背景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteErrorInColor(object value, ConsoleColor foreground, ConsoleColor background)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                ConsoleEx.WriteError(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定对象的文本表示形式以指定的前景色写入标准错误输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteErrorInColor(object value, ConsoleColor foreground)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ResetColor();
                Console.ForegroundColor = foreground;
                ConsoleEx.WriteError(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定的字符串值（后跟当前行终止符）以指定的前景色和背景色写入标准错误输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <param name="background">要使用的控制台背景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteErrorLineInColor(string value, ConsoleColor foreground, ConsoleColor background)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                ConsoleEx.WriteErrorLine(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定的字符串值（后跟当前行终止符）以指定的前景色写入标准错误输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteErrorLineInColor(string value, ConsoleColor foreground)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ResetColor();
                Console.ForegroundColor = foreground;
                ConsoleEx.WriteErrorLine(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定对象的文本表示形式（后跟当前行终止符）以指定的前景色和背景色写入标准错误输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <param name="background">要使用的控制台背景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteErrorLineInColor(object value, ConsoleColor foreground, ConsoleColor background)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                ConsoleEx.WriteErrorLine(value);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 将指定对象的文本表示形式（后跟当前行终止符）以指定的前景色写入标准错误输出流。
        /// </summary>
        /// <param name="value">要写入的值。</param>
        /// <param name="foreground">要使用的控制台前景色。</param>
        /// <exception cref="ArgumentException">
        /// 指定的颜色不是 <see cref="ConsoleColor"/> 的有效成员。</exception>
        /// <exception cref="SecurityException">用户没有设置控制台颜色的权限。</exception>
        /// <exception cref="IOException">出现 I/O 错误。</exception>
        public static void WriteErrorLineInColor(object value, ConsoleColor foreground)
        {
            lock (ConsoleEx.WriteSyncRoot)
            {
                Console.ResetColor();
                Console.ForegroundColor = foreground;
                ConsoleEx.WriteErrorLine(value);
                Console.ResetColor();
            }
        }
    }
}