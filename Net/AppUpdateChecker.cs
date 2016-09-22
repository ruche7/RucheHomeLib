using System;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RucheHome.Util;

namespace RucheHome.Net
{
    /// <summary>
    /// アプリ更新情報チェッククラス。
    /// </summary>
    public class AppUpdateChecker : BindableBase
    {
        /// <summary>
        /// 現在実行中のプロセスを用いるコンストラクタ。
        /// </summary>
        /// <remarks>
        /// 実行中のプロセスに AssemblyProductAttribute 属性と
        /// AssemblyVersionAttribute 属性が定義されている必要がある。
        /// </remarks>
        public AppUpdateChecker() : this(Assembly.GetEntryAssembly())
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="app">
        /// 対象アセンブリ。
        /// AssemblyProductAttribute 属性と
        /// AssemblyVersionAttribute 属性が定義されている必要がある。
        /// </param>
        public AppUpdateChecker(Assembly app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var productAttr = app.GetCustomAttribute<AssemblyProductAttribute>();
            if (productAttr == null)
            {
                throw new ArgumentException(
                    nameof(AssemblyProductAttribute) + @" is not defined.",
                    nameof(app));
            }
            if (string.IsNullOrWhiteSpace(productAttr.Product))
            {
                throw new ArgumentException(
                    nameof(AssemblyProductAttribute) + @" is blank.",
                    nameof(app));
            }

            var version = app.GetName()?.Version;
            if (version == null)
            {
                throw new ArgumentException(
                    @"The version of application is not defined.",
                    nameof(app));
            }

            this.Product = productAttr.Product;
            this.CurrentVersion = version;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="product">プロダクト名。</param>
        /// <param name="currentVersion">現行バージョン。</param>
        public AppUpdateChecker(string product, Version currentVersion)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            if (string.IsNullOrWhiteSpace(product))
            {
                throw new ArgumentException(@"`product` is blank.", nameof(product));
            }
            if (currentVersion == null)
            {
                throw new ArgumentNullException(nameof(currentVersion));
            }

            this.Product = product;
            this.CurrentVersion = currentVersion;
        }

        /// <summary>
        /// プロダクト名を取得する。
        /// </summary>
        public string Product { get; }

        /// <summary>
        /// 現行バージョンを取得する。
        /// </summary>
        public Version CurrentVersion { get; }

        /// <summary>
        /// 更新チェック処理中であるか否かを取得する。
        /// </summary>
        public bool IsBusy
        {
            get { return this.busy; }
            private set { this.SetProperty(ref this.busy, value); }
        }
        private bool busy = false;

        /// <summary>
        /// 新しいバージョンのアプリが存在するか否かを取得する。
        /// </summary>
        /// <remarks>
        /// 未チェック状態では false を返す。
        /// </remarks>
        public bool CanUpdate
        {
            get { return this.canUpdate; }
            private set { this.SetProperty(ref this.canUpdate, value); }
        }
        private bool canUpdate = false;

        /// <summary>
        /// 最新バージョンを取得する。
        /// </summary>
        /// <remarks>
        /// 未チェック状態では null を返す。
        /// </remarks>
        public Version NewestVersion
        {
            get { return this.newestVersion; }
            private set { this.SetProperty(ref this.newestVersion, value); }
        }
        private Version newestVersion = null;

        /// <summary>
        /// ダウンロードページURIを取得する。
        /// </summary>
        /// <remarks>
        /// 未チェック状態では null を返す。
        /// </remarks>
        public Uri PageUri
        {
            get { return this.pageUri; }
            private set { this.SetProperty(ref this.pageUri, value); }
        }
        private Uri pageUri = null;

        /// <summary>
        /// 更新チェック処理を行う。
        /// </summary>
        /// <returns>成功したならば true 。そうでなければ false 。</returns>
        public async Task<bool> Run()
        {
            if (this.IsBusy || Interlocked.Exchange(ref this.runLock, 1) != 0)
            {
                return false;
            }

            Version version = null;
            Uri pageUri = null;

            try
            {
                this.IsBusy = true;

                // アプリ更新情報JSONファイルをダウンロード
                var json =
                    await this.Client.DownloadDataTaskAsync(
                        JsonBaseUri + this.Product + @".json");
                if (json == null || json.Length <= 0)
                {
                    return false;
                }

                // JSONを読み取る
                XElement elem = null;
                using (
                    var reader =
                        JsonReaderWriterFactory.CreateJsonReader(
                            json,
                            XmlDictionaryReaderQuotas.Max))
                {
                    elem = await Task.Run(() => XElement.Load(reader));
                }

                var productElem = elem.Element(@"product");
                var versionElem = elem.Element(@"version");
                var pageUriElem = elem.Element(@"page_uri");

                if (
                    productElem == null ||
                    versionElem == null ||
                    pageUriElem == null ||
                    (string)productElem != this.Product)
                {
                    return false;
                }

                version = new Version((string)versionElem);
                pageUri = new Uri((string)pageUriElem);

                this.NewestVersion = version;
                this.PageUri = pageUri;
                this.CanUpdate = (this.NewestVersion > this.CurrentVersion);
            }
            catch
            {
                return false;
            }
            finally
            {
                Interlocked.Exchange(ref this.runLock, 0);
                this.IsBusy = false;
            }

            return true;
        }
        private int runLock = 0;

        /// <summary>
        /// アプリ更新情報JSONファイルのベースURI。
        /// </summary>
        private static readonly string JsonBaseUri =
            @"https://ruche-home.net/files/versions/";

        /// <summary>
        /// Webクライアントを取得する。
        /// </summary>
        private WebClient Client { get; } =
            new WebClient
            {
                // キャッシュしない
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)
            };
    }
}
