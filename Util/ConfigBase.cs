using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace RucheHome.Util
{
    /// <summary>
    /// ConfigKeeper{T} ジェネリッククラスで扱う設定の抽象基底クラス。
    /// </summary>
    [DataContract(Namespace = "")]
    public abstract class ConfigBase : INotifyPropertyChanged, IExtensibleDataObject
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        protected ConfigBase()
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
                if (propertyName != null && this.PropertyChanged != null)
                {
                    this.PropertyChanged(
                        this,
                        new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        #region INotifyPropertyChanged の実装

        /// <summary>
        /// プロパティ値の変更時に呼び出されるイベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IExtensibleDataObject の明示的実装

        ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }

        #endregion
    }
}
