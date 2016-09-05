using System;
using System.Diagnostics;
using System.Windows.Input;

namespace RucheHome.Windows.Mvvm.Commands
{
    /// <summary>
    /// コマンドパラメータを Process.Start メソッドに渡すコマンドを定義するクラス。
    /// </summary>
    public class ProcessStartCommand : ICommand
    {
        /// <summary>
        /// インスタンス。 XAML から x:Static で参照する。
        /// </summary>
        public static readonly ProcessStartCommand Instance =
            new ProcessStartCommand();

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ProcessStartCommand()
        {
        }

        #region ICommand の実装

        /// <summary>
        /// コマンドの実行可能状態が変化した時に呼び出されるイベント。
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// コマンドを実行可能であるか否かを取得する。
        /// </summary>
        /// <param name="parameter">コマンドパラメータ。</param>
        /// <returns>実行可能ならば true 。そうでなければ false 。</returns>
        public bool CanExecute(object parameter) =>
            !string.IsNullOrWhiteSpace(parameter as string);

        /// <summary>
        /// コマンド処理を行う。
        /// </summary>
        /// <param name="parameter">コマンドパラメータ。</param>
        public void Execute(object parameter)
        {
            var cmd = parameter as string;
            if (string.IsNullOrWhiteSpace(cmd))
            {
                return;
            }

            Process.Start(cmd);
        }

        #endregion
    }
}
