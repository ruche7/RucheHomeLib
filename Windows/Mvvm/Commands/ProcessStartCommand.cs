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
        /// コマンドを実行可能であるか否かを取得する。
        /// </summary>
        /// <param name="parameter">コマンドパラメータ。</param>
        /// <returns>常に true 。</returns>
        public bool CanExecute(object parameter) => true;

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

            try
            {
                Process.Start(cmd);
            }
            catch { }
        }

        #endregion

        #region ICommand の明示的実装

        event EventHandler ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }

        #endregion
    }
}
