using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public abstract class BindableConfigBase : BindableBase, IExtensibleDataObject
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        protected BindableConfigBase() : base()
        {
        }

        /// <summary>
        /// INotifyPropertyChanged インタフェース実装型プロパティ値を設定し、
        /// PropertyChanged イベントの伝搬設定を行い、変更をイベント通知する。
        /// </summary>
        /// <typeparam name="T">
        /// プロパティ値の型。 INotifyPropertyChanged インタフェースを実装していること。
        /// </typeparam>
        /// <param name="field">設定先フィールド。</param>
        /// <param name="value">設定値。</param>
        /// <param name="propertyName">
        /// プロパティ名。 CallerMemberNameAttribute により自動設定される。
        /// </param>
        protected void SetPropertyWithPropertyChangedChain<T>(
            ref T field,
            T value,
            [CallerMemberName] string propertyName = null)
            where T : INotifyPropertyChanged
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                this.RemovePropertyChangedChain(ref field, propertyName);
                field = value;
                this.AddPropertyChangedChain(ref field, propertyName);

                this.RaisePropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// INotifyCollectionChanged インタフェース実装型プロパティ値を設定し、
        /// PropertyChanged イベントの伝搬設定を行い、変更をイベント通知する。
        /// </summary>
        /// <typeparam name="T">
        /// プロパティ値の型。 INotifyCollectionChanged インタフェースを実装していること。
        /// </typeparam>
        /// <param name="field">設定先フィールド。</param>
        /// <param name="value">設定値。</param>
        /// <param name="propertyName">
        /// プロパティ名。 CallerMemberNameAttribute により自動設定される。
        /// </param>
        protected void SetPropertyWithCollectionChangedChain<T>(
            ref T field,
            T value,
            [CallerMemberName] string propertyName = null)
            where T : INotifyCollectionChanged
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                this.RemoveCollectionChangedChain(ref field, propertyName);
                field = value;
                this.AddCollectionChangedChain(ref field, propertyName);

                this.RaisePropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// BindableCollection{TItem} 派生型プロパティ値を設定し、
        /// PropertyChanged イベントの伝搬設定を行い、変更をイベント通知する。
        /// </summary>
        /// <typeparam name="TItem">プロパティ値の要素型。明示する必要がある。</typeparam>
        /// <typeparam name="T">
        /// プロパティ値の型。 BindableCollection{TItem} 派生型であること。
        /// </typeparam>
        /// <param name="field">設定先フィールド。</param>
        /// <param name="value">設定値。</param>
        /// <param name="propertyName">
        /// プロパティ名。 CallerMemberNameAttribute により自動設定される。
        /// </param>
        protected void SetBindableCollectionPropertyWithEventChain<TItem, T>(
            ref T field,
            T value,
            [CallerMemberName] string propertyName = null)
            where T : BindableCollection<TItem>
            where TItem : INotifyPropertyChanged
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                if (field != null)
                {
                    field.ItemPropertyChanged -=
                        this.GetPropertyChangedChainDelegate(propertyName);
                    this.RemoveCollectionChangedChain(ref field, propertyName);
                }

                field = value;

                if (field != null)
                {
                    this.AddCollectionChangedChain(ref field, propertyName);
                    field.ItemPropertyChanged +=
                        this.GetPropertyChangedChainDelegate(propertyName);
                }

                this.RaisePropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// INotifyPropertyChanged インタフェース実装型プロパティの変更通知を
        /// このオブジェクトの PropertyChanged イベントに伝搬させるように設定する。
        /// </summary>
        /// <typeparam name="T">
        /// プロパティ値の型。 INotifyPropertyChanged インタフェースを実装していること。
        /// </typeparam>
        /// <param name="field">設定先フィールド。</param>
        /// <param name="propertyName">
        /// プロパティ名。 CallerMemberNameAttribute により自動設定される。
        /// </param>
        protected void AddPropertyChangedChain<T>(
            ref T field,
            [CallerMemberName] string propertyName = null)
            where T : INotifyPropertyChanged
        {
            if (field != null)
            {
                field.PropertyChanged +=
                    this.GetPropertyChangedChainDelegate(propertyName);
            }
        }

        /// <summary>
        /// INotifyPropertyChanged インタフェース実装型プロパティの変更通知を
        /// このオブジェクトの PropertyChanged イベントに伝搬させる設定を解除する。
        /// </summary>
        /// <typeparam name="T">
        /// プロパティ値の型。 INotifyPropertyChanged インタフェースを実装していること。
        /// </typeparam>
        /// <param name="field">設定解除先フィールド。</param>
        /// <param name="propertyName">
        /// プロパティ名。 CallerMemberNameAttribute により自動設定される。
        /// </param>
        protected void RemovePropertyChangedChain<T>(
            ref T field,
            [CallerMemberName] string propertyName = null)
            where T : INotifyPropertyChanged
        {
            if (field != null)
            {
                field.PropertyChanged -=
                    this.GetPropertyChangedChainDelegate(propertyName);
            }
        }

        /// <summary>
        /// INotifyCollectionChanged インタフェース実装型プロパティのコレクション変更通知を
        /// このオブジェクトの PropertyChanged イベントに伝搬させるように設定する。
        /// </summary>
        /// <typeparam name="T">
        /// プロパティ値の型。 INotifyCollectionChanged インタフェースを実装していること。
        /// </typeparam>
        /// <param name="field">設定先フィールド。</param>
        /// <param name="propertyName">
        /// プロパティ名。 CallerMemberNameAttribute により自動設定される。
        /// </param>
        protected void AddCollectionChangedChain<T>(
            ref T field,
            [CallerMemberName] string propertyName = null)
            where T : INotifyCollectionChanged
        {
            if (field != null)
            {
                field.CollectionChanged +=
                    this.GetCollectionChangedChainDelegate(propertyName);
            }
        }

        /// <summary>
        /// INotifyCollectionChanged インタフェース実装型プロパティのコレクション変更通知を
        /// このオブジェクトの PropertyChanged イベントに伝搬させる設定を解除する。
        /// </summary>
        /// <typeparam name="T">
        /// プロパティ値の型。 INotifyCollectionChanged インタフェースを実装していること。
        /// </typeparam>
        /// <param name="field">設定解除先フィールド。</param>
        /// <param name="propertyName">
        /// プロパティ名。 CallerMemberNameAttribute により自動設定される。
        /// </param>
        protected void RemoveCollectionChangedChain<T>(
            ref T field,
            [CallerMemberName] string propertyName = null)
            where T : INotifyCollectionChanged
        {
            if (field != null)
            {
                field.CollectionChanged -=
                    this.GetCollectionChangedChainDelegate(propertyName);
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

        /// <summary>
        /// PropertyChanged イベント伝搬用デリゲートディクショナリ。
        /// </summary>
        private Dictionary<string, PropertyChangedEventHandler>
        PropertyChangedChainDelegates =
            new Dictionary<string, PropertyChangedEventHandler>();

        /// <summary>
        /// PropertyChanged イベント伝搬用デリゲートを取得する。
        /// </summary>
        /// <param name="propertyName">プロパティ名。</param>
        private PropertyChangedEventHandler GetPropertyChangedChainDelegate(
            string propertyName)
        {
            PropertyChangedEventHandler result;
            if (!PropertyChangedChainDelegates.TryGetValue(propertyName, out result))
            {
                result = (s, e) => this.RaisePropertyChanged(propertyName);
                PropertyChangedChainDelegates.Add(propertyName, result);
            }

            return result;
        }

        /// <summary>
        /// CollectionChanged イベント伝搬用デリゲートディクショナリ。
        /// </summary>
        private Dictionary<string, NotifyCollectionChangedEventHandler>
        CollectionChangedChainDelegates =
            new Dictionary<string, NotifyCollectionChangedEventHandler>();

        /// <summary>
        /// CollectionChanged イベント伝搬用デリゲートを取得する。
        /// </summary>
        /// <param name="propertyName">プロパティ名。</param>
        private NotifyCollectionChangedEventHandler GetCollectionChangedChainDelegate(
            string propertyName)
        {
            NotifyCollectionChangedEventHandler result;
            if (!CollectionChangedChainDelegates.TryGetValue(propertyName, out result))
            {
                result = (s, e) => this.RaisePropertyChanged(propertyName);
                CollectionChangedChainDelegates.Add(propertyName, result);
            }

            return result;
        }

        #region IExtensibleDataObject の明示的実装

        ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }

        #endregion
    }
}
