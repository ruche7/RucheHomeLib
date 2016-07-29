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
        /// <param name="oldValues">
        /// 置換元文字列列挙。 null や空文字列を含んでいてはならない。
        /// </param>
        /// <param name="newValues">
        /// 置換先文字列列挙。 null を含んでいてはならない。
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
            // 置換処理用アイテムリスト作成
            // 引数の正当性チェックも行われる
            var items = MakeReplaceItems(self, oldValues, newValues);
            if (items.Count <= 0)
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

                // 置換処理用アイテムリスト更新
                UpdateReplaceItems(items, self, selfPos);
            }
            while (items.Count > 0);

            // 末尾までの文字列を追加
            if (selfPos < self.Length)
            {
                dest.Append(self.Substring(selfPos));
            }

            return dest.ToString();
        }

        /// <summary>
        /// 置換処理用アイテムクラス。
        /// </summary>
        private class ReplaceItem : IComparable<ReplaceItem>
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
        /// 置換処理用アイテムリストを作成する。
        /// </summary>
        /// <param name="self">置換対象文字列。</param>
        /// <param name="oldValues">
        /// 置換元文字列列挙。 null や空文字列を含んでいてはならない。
        /// </param>
        /// <param name="newValues">
        /// 置換先文字列列挙。 null を含んでいてはならない。
        /// </param>
        /// <returns>置換処理用アイテムリスト。</returns>
        private static List<ReplaceItem> MakeReplaceItems(
            string self,
            IEnumerable<string> oldValues,
            IEnumerable<string> newValues)
        {
            // null チェック
            ValidateArgumentNull(self, nameof(self));
            ValidateArgumentNull(oldValues, nameof(oldValues));
            ValidateArgumentNull(newValues, nameof(newValues));

            var newVals = newValues.ToArray();
            if (newVals.Length <= 0 && oldValues.Any())
            {
                throw new ArgumentException(
                    @"置換先文字列列挙の要素数が 0 です。",
                    nameof(newValues));
            }
            if (newVals.Contains(null))
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

                            var searchResult = self.IndexOf(v);
                            return
                                (searchResult < 0) ?
                                    null :
                                    new ReplaceItem(
                                        i,
                                        v,
                                        newVals[Math.Min(i, newVals.Length - 1)],
                                        searchResult);
                        })
                    .Where(item => item != null)
                    .ToList();
        }

        /// <summary>
        /// 置換処理用アイテムリストを更新する。
        /// </summary>
        /// <param name="items">置換処理用アイテムリスト。</param>
        /// <param name="self">置換対象文字列。</param>
        /// <param name="searchResultMin">置換元文字列の検索開始位置。</param>
        private static void UpdateReplaceItems(
            List<ReplaceItem> items,
            string self,
            int searchResultMin)
        {
            // 検索結果値更新
            foreach (var item in items)
            {
                if (item.SearchResult < searchResultMin)
                {
                    item.SearchResult = self.IndexOf(item.OldValue, searchResultMin);
                }
            }

            // 文字列が見つからない要素をリストから削除
            items.RemoveAll(item => item.SearchResult < 0);
        }
    }
}
