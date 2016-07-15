using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RucheHome.Util
{
    /// <summary>
    /// プロパティ変更通知をサポートするクラスの抽象基底クラス。
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        protected BindableBase()
        {
        }

        /// <summary>
        /// プロパティ値を設定し、変更をイベント通知する。
        /// </summary>
        /// <typeparam name="T">プロパティ値の型。</typeparam>
        /// <param name="field">設定先フィールド。</param>
        /// <param name="value">設定値。</param>
        /// <param name="propertyName">
        /// プロパティ名。 CallerMemberNameAttribute により自動設定される。
        /// </param>
        protected void SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                this.RaisePropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// PropertyChanged イベントを発生させる。
        /// </summary>
        /// <param name="propertyName">
        /// プロパティ名。指定しなければ CallerMemberNameAttribute により自動設定される。
        /// </param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region INotifyPropertyChanged の実装

        /// <summary>
        /// プロパティ値の変更時に呼び出されるイベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
