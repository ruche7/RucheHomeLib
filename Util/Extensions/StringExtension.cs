using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RucheHome.Util.Extensions
{
    /// <summary>
    /// string 型に対する拡張メソッドを提供する静的クラス。
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 文字列列挙による文字列の置換処理を行う。
        /// </summary>
        /// <param name="self">置換対象文字列。</param>
        /// <param name="oldValues">置換元文字列列挙。</param>
        /// <param name="newValues">
        /// 置換先文字列列挙。要素数 1 以上でなければならない。
        /// </param>
        /// <returns>置換された文字列。</returns>
        /// <remarks>
        /// <para>引数 oldValues と newValues の各要素がそれぞれ対応する。</para>
        /// <para>
        /// newValues の要素数が oldValues の要素数より少ない場合、
        /// 超過分の置換先文字列には newValues の末尾要素が利用される。
        /// </para>
        /// <para>
        /// newValues の要素数が oldValues の要素数より多い場合、超過分は無視される。
        /// </para>
        /// </remarks>
        public static string Replace(
            this string self,
            IEnumerable<string> oldValues,
            IEnumerable<string> newValues)
        {
            // 置換処理用アイテム列挙作成
            // 引数の正当性チェックも行われる
            var items = MakeReplaceItems(self, oldValues, newValues);
            if (!items.Any())
            {
                return self;
            }

            var dest = new StringBuilder();
            int selfPos = 0;

            do
            {
                // 最も優先度の高いアイテムを取得
                var item = items.Min();

                // 対象アイテムまでの文字列と対象アイテムの置換先文字列を追加
                dest.Append(self.Substring(selfPos, item.SearchResult - selfPos));
                dest.Append(item.NewValue);

                // 文字列検索基準位置を更新
                selfPos = item.SearchResult + item.OldValue.Length;
                if (selfPos >= self.Length)
                {
                    break;
                }

                // 置換処理用アイテム列挙更新
                items = UpdateReplaceItems(items, self, selfPos);
            }
            while (items.Any());

            // 末尾までの文字列を追加
            if (selfPos < self.Length)
            {
                dest.Append(self.Substring(selfPos));
            }

            return dest.ToString();
        }

        /// <summary>
        /// 置換処理用アイテム構造体。
        /// </summary>
        private struct ReplaceItem : IComparable<ReplaceItem>
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            /// <param name="itemIndex">アイテムの優先度を表すインデックス値。</param>
            /// <param name="oldValue">置換元文字列。</param>
            /// <param name="newValue">置換先文字列。</param>
            /// <param name="searchResult">検索結果保存値。</param>
            public ReplaceItem(
                int itemIndex,
                string oldValue,
                string newValue,
                int searchResult = -1)
            {
                this.ItemIndex = itemIndex;
                this.OldValue = oldValue;
                this.NewValue = newValue;
                this.SearchResult = searchResult;
            }

            /// <summary>
            /// アイテムの優先度を表すインデックス値を取得する。
            /// </summary>
            public int ItemIndex { get; }

            /// <summary>
            /// 置換元文字列を取得する。
            /// </summary>
            public string OldValue { get; }

            /// <summary>
            /// 置換先文字列を取得する。
            /// </summary>
            public string NewValue { get; }

            /// <summary>
            /// 検索結果保存値を取得または設定する。
            /// </summary>
            public int SearchResult { get; set; }

            /// <summary>
            /// 優先度の比較処理を行う。
            /// </summary>
            /// <param name="other">比較対象。</param>
            /// <returns>比較対象との優先順位を表す数値。</returns>
            /// <remarks>
            /// SearchResult の値が異なる場合はその値が小さいほど優先する。
            /// そうではなく OldValue.Length の値が異なる場合はその値が大きいほど優先する。
            /// そうでもなければ ItemIndex の値が小さいほど優先する。
            /// 上記すべての値が等しければ優先順位は等価と判断する。
            /// </remarks>
            public int CompareTo(ReplaceItem other)
            {
                if (this.SearchResult != other.SearchResult)
                {
                    return (this.SearchResult - other.SearchResult);
                }
                if (this.OldValue.Length != other.OldValue.Length)
                {
                    return (other.OldValue.Length - this.OldValue.Length);
                }
                return (this.ItemIndex - other.ItemIndex);
            }
        }

        /// <summary>
        /// 引数値の null 検証を行い、
        /// null であれば ArgumentNullException 例外を発生させます。
        /// </summary>
        /// <param name="arg">引数値。</param>
        /// <param name="argName">引数名。 null ならば利用されない。</param>
        private static void ValidateArgumentNull(object arg, string argName)
        {
            if (arg == null)
            {
                throw
                    (argName == null) ?
                        new ArgumentNullException() : new ArgumentNullException(argName);
            }
        }

        /// <summary>
        /// 置換処理用アイテム列挙を作成する。
        /// </summary>
        /// <param name="self">置換対象文字列。</param>
        /// <param name="oldValues">置換元文字列列挙。</param>
        /// <param name="newValues">
        /// 置換先文字列列挙。要素数 1 以上でなければならない。
        /// </param>
        /// <returns>置換処理用アイテム列挙。</returns>
        private static IEnumerable<ReplaceItem> MakeReplaceItems(
            string self,
            IEnumerable<string> oldValues,
            IEnumerable<string> newValues)
        {
            // null チェック
            ValidateArgumentNull(self, nameof(self));
            ValidateArgumentNull(oldValues, nameof(oldValues));
            ValidateArgumentNull(newValues, nameof(newValues));

            var newVals = newValues.ToArray();
            if (newVals.Length <= 0)
            {
                throw new ArgumentException(
                    @"置換先文字列列挙の要素数が 0 です。",
                    nameof(newValues));
            }
            if (newVals.Any(v => v == null))
            {
                throw new ArgumentException(
                    @"置換先文字列列挙内に null が含まれています。",
                    nameof(newValues));
            }

            return
                oldValues
                    .Select(
                        (v, i) =>
                        {
                            if (v == null)
                            {
                                throw new ArgumentException(
                                    @"置換元文字列列挙内に null が含まれています。",
                                    nameof(oldValues));
                            }
                            if (v == "")
                            {
                                throw new ArgumentException(
                                    @"置換元文字列列挙内に空文字列が含まれています。",
                                    nameof(oldValues));
                            }

                            var newVal = newVals[Math.Min(i, newVals.Length - 1)];
                            return new ReplaceItem(i, v, newVal, self.IndexOf(v));
                        })
                    .Where(item => item.SearchResult >= 0);
        }

        /// <summary>
        /// 置換処理用アイテム列挙を更新する。
        /// </summary>
        /// <param name="items">置換処理用アイテム列挙。</param>
        /// <param name="self">置換対象文字列。</param>
        /// <param name="searchResultMin">置換元文字列の検索開始位置。</param>
        /// <returns>更新された置換処理用アイテム列挙。</returns>
        private static IEnumerable<ReplaceItem> UpdateReplaceItems(
            IEnumerable<ReplaceItem> items,
            string self,
            int searchResultMin)
        {
            return
                items
                    .Select(
                        item =>
                        {
                            if (item.SearchResult < searchResultMin)
                            {
                                item.SearchResult =
                                    self.IndexOf(item.OldValue, searchResultMin);
                            }
                            return item;
                        })
                    .Where(item => item.SearchResult >= 0);
        }
    }
}
