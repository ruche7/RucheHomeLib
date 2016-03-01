using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RucheHome.Util
{
    /// <summary>
    /// 時刻、スレッドID、呼び出し元情報付きのトレース出力を行う静的クラス。
    /// </summary>
    public static class ThreadTrace
    {
        /// <summary>
        /// 時刻、スレッドID、呼び出し元情報を付けてテキストと改行をトレース出力する。
        /// </summary>
        /// <param name="text">テキスト。不要ならば null 。</param>
        /// <param name="member">
        /// 呼び出し元メンバ名。 CallerMemberNameAttribute により自動設定される。
        /// </param>
        /// <param name="file">
        /// 呼び出し元ファイル名。 CallerFilePathAttribute により自動設定される。
        /// </param>
        /// <param name="line">
        /// 呼び出し元行番号。 CallerLineNumberAttribute により自動設定される。
        /// </param>
        [Conditional("TRACE")]
        public static void WriteLine(
            string text = null,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Trace.WriteLine(ThreadDebug.MakeMessage(text, member, file, line));
        }

        /// <summary>
        /// 時刻、スレッドID、呼び出し元情報を付けて値と改行をトレース出力する。
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
        [Conditional("TRACE")]
        public static void WriteLine(
            object value,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            WriteLine(value?.ToString(), member, file, line);
        }
    }
}
