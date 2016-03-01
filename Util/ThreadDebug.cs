using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RucheHome.Util
{
    /// <summary>
    /// スレッドID、呼び出し元情報付きのデバッグ出力を行う静的クラス。
    /// </summary>
    public static class ThreadDebug
    {
        /// <summary>
        /// スレッドID、呼び出し元情報を付けてメッセージと改行をデバッグ出力する。
        /// </summary>
        /// <param name="message">メッセージ。</param>
        /// <param name="member">
        /// 呼び出し元メンバ名。 CallerMemberNameAttribute により自動設定される。
        /// </param>
        /// <param name="file">
        /// 呼び出し元ファイル名。 CallerFilePathAttribute により自動設定される。
        /// </param>
        /// <param name="line">
        /// 呼び出し元行番号。 CallerLineNumberAttribute により自動設定される。
        /// </param>
        [Conditional("DEBUG")]
        public static void WriteLine(
            string message = "",
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            var tid = @"[TID:" + Thread.CurrentThread.ManagedThreadId + @"]";
            var caller = @"[" + member + @"@" + file + @":" + line + @"]";

            var msg = message ?? "";
            if (msg != "")
            {
                msg = " " + msg + " ";
            }

            Debug.WriteLine(tid + msg + caller);
        }

        /// <summary>
        /// スレッドID、呼び出し元情報を付けて値と改行をデバッグ出力する。
        /// </summary>
        /// <param name="value">値。</param>
        /// <param name="member">
        /// 呼び出し元メンバ名。 CallerMemberNameAttribute により自動設定される。
        /// </param>
        /// <param name="file">
        /// 呼び出し元ファイル名。 CallerFilePathAttribute により自動設定される。
        /// </param>
        /// <param name="line">
        /// 呼び出し元行番号。 CallerLineNumberAttribute により自動設定される。
        /// </param>
        [Conditional("DEBUG")]
        public static void WriteLine(
            object value,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            WriteLine(value?.ToString() ?? @"(null)", member, file, line);
        }
    }
}
