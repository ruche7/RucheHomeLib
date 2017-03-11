using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace RucheHome.Windows.Mvvm.Commands
{
    /// <summary>
    /// バインディングしたコマンドを実行するだけのコマンド。
    /// </summary>
    [ContentProperty(nameof(Command))]
    public class BindingCommand : DependencyObject, ICommand
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public BindingCommand()
        {
        }

        /// <summary>
        /// Command 依存関係プロパティ。
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(BindingCommand),
                new PropertyMetadata(
                    null,
                    (sender, e) =>
                        (sender as BindingCommand)?.OnCommandChanged(
                            e.OldValue as ICommand,
                            e.NewValue as ICommand)));

        /// <summary>
        /// 処理対象のコマンドを取得または設定する。
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Command プロパティ変更時の処理を行う。
        /// </summary>
        /// <param name="oldValue">変更前の値。</param>
        /// <param name="newValue">変更後の値。</param>
        private void OnCommandChanged(ICommand oldValue, ICommand newValue)
        {
            if (oldValue != null)
            {
                oldValue.CanExecuteChanged -= this.OnCanExecuteChanged;
            }
            if (newValue != null)
            {
                newValue.CanExecuteChanged += this.OnCanExecuteChanged;
            }

            // コマンドが変わったので発生させておく
            this.OnCanExecuteChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Command プロパティの CanExecuteChanged イベント発生時に呼び出される。
        /// </summary>
        /// <param name="sender">呼び出し元。</param>
        /// <param name="e">イベント引数。</param>
        private void OnCanExecuteChanged(object sender, EventArgs e) =>
            this.CanExecuteChanged?.Invoke(this, e);

        #region ICommand の実装

        /// <summary>
        /// CanExecute メソッドの戻り値が変化した時に発生するイベント。
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// コマンドが実行可能であるか否かを取得する。
        /// </summary>
        /// <param name="parameter">コマンドパラメータ。</param>
        /// <returns>実行可能ならば true 。そうでなければ false 。</returns>
        public bool CanExecute(object parameter) =>
            (this.Command?.CanExecute(parameter) == true);

        /// <summary>
        /// コマンドを実行する。
        /// </summary>
        /// <param name="parameter">コマンドパラメータ。</param>
        public void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
            {
                this.Command?.Execute(parameter);
            }
        }

        #endregion
    }
}
