using System;
using System.Linq;
using System.Reflection;
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

        #region IExtensibleDataObject の明示的実装

        ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }

        #endregion
    }
}
