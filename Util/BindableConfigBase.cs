using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace RucheHome.Util
{
    /// <summary>
    /// ConfigKeeper{T} ジェネリッククラスで扱われる、
    /// プロパティ変更通知付き設定の抽象基底クラス。
    /// </summary>
    [DataContract(Namespace = "")]
    public abstract class BindableConfigBase
        :
        INotifyPropertyChanged,
        IExtensibleDataObject
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        protected BindableConfigBase()
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

        /// <summary>
        /// DataMemberAttribute 属性の付与されたプロパティおよびフィールドを、
        /// 既定のコンストラクタを呼び出した直後の値で上書きする。
        /// </summary>
        /// <remarks>
        /// 既定のコンストラクタが存在しない場合は例外が送出される。
        /// </remarks>
        protected void ResetDataMembers()
        {
            var type = this.GetType();
            var src = Activator.CreateInstance(type, true);

            // プロパティ上書き
            var propInfos =
                type
                    .GetProperties(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic)
                    .Where(
                        i =>
                            i.IsDefined(typeof(DataMemberAttribute)) &&
                            i.CanRead &&
                            i.CanWrite);
            foreach (var pi in propInfos)
            {
                pi.SetValue(this, pi.GetValue(src));
            }

            // フィールド上書き
            var fieldInfos =
                type
                    .GetFields(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic)
                    .Where(i => i.IsDefined(typeof(DataMemberAttribute)));
            foreach (var fi in fieldInfos)
            {
                fi.SetValue(this, fi.GetValue(src));
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
