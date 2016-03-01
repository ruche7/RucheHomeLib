using System;
using System.Diagnostics;
using System.Threading;

namespace RucheHome.Util
{
    /// <summary>
    /// スレッドID付きのデバッグ出力を行う静的クラス。
    /// </summary>
    public static class ThreadDebug
    {
        /// <summary>
        /// メッセージと改行をデバッグ出力する。
        /// </summary>
        /// <param name="message">メッセージ。</param>
        [Conditional("DEBUG")]
        public static void WriteLine(string message)
        {
            var tid = @"[TID:" + Thread.CurrentThread.ManagedThreadId + @"] ";
            Debug.WriteLine(tid + message);
        }

        /// <summary>
        /// 値と改行をデバッグ出力する。
        /// </summary>
        /// <param name="value">値。</param>
        [Conditional("DEBUG")]
        public static void WriteLine(object value)
        {
            WriteLine(value?.ToString() ?? @"(null)");
        }

        /// <summary>
        /// フォーマット文字列と改行をデバッグ出力する。
        /// </summary>
        /// <param name="format">フォーマット。</param>
        /// <param name="args">パラメータ配列。</param>
        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }
    }
}
