using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;

namespace RucheHome.Util
{
    /// <summary>
    /// 設定の読み書きを行うクラス。
    /// </summary>
    /// <typeparam name="T">設定値の型。</typeparam>
    public class ConfigKeeper<T>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="subDirectoryName">
        /// 設定保存先サブディレクトリ名。基準ディレクトリからの相対パス。
        /// 全体で設定を共有するならば空文字列または null 。
        /// </param>
        /// <param name="baseDirectoryPath">
        /// 基準ディレクトリパス。
        /// 相対パスを指定するとローカルアプリケーションフォルダを基準位置とする。
        /// 実行中プロセスの AssemblyCompanyAttribute 属性を用いるならば null 。
        /// </param>
        /// <param name="serializer">
        /// シリアライザ。既定のシリアライザを用いるならば null 。
        /// </param>
        public ConfigKeeper(
            string subDirectoryName = null,
            string baseDirectoryPath = null,
            XmlObjectSerializer serializer = null)
        {
            this.Value = default(T);

            this.BaseDirectoryPath = MakeBaseDirectoryPath(baseDirectoryPath);

            var fileName = typeof(T).FullName + @".config";
            var filePath =
                string.IsNullOrEmpty(subDirectoryName) ?
                    fileName : Path.Combine(subDirectoryName, fileName);
            this.FilePath = Path.Combine(this.BaseDirectoryPath, filePath);

            this.Serializer = serializer ?? (new DataContractJsonSerializer(typeof(T)));
        }

        /// <summary>
        /// 設定値を取得または設定する。
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 設定ファイルパスを取得する。
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// シリアライザを取得する。
        /// </summary>
        public XmlObjectSerializer Serializer { get; }

        /// <summary>
        /// ベースディレクトリパスを取得する。
        /// </summary>
        public string BaseDirectoryPath { get; }

        /// <summary>
        /// 設定を読み取る。
        /// </summary>
        /// <returns>成功したならば true 。失敗したならば false 。</returns>
        public bool Load()
        {
            // ファイルがなければ読み取れない
            if (!File.Exists(this.FilePath))
            {
                return false;
            }

            if (Interlocked.Exchange(ref this.ioLock, 1) != 0)
            {
                return false;
            }

            try
            {
                // 読み取り
                using (var stream = File.OpenRead(this.FilePath))
                {
                    var value = this.Serializer.ReadObject(stream);
                    if (!(value is T))
                    {
                        return false;
                    }
                    this.Value = (T)value;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                Interlocked.Exchange(ref this.ioLock, 0);
            }

            return true;
        }

        /// <summary>
        /// 設定を書き出す。
        /// </summary>
        /// <returns>成功したならば true 。失敗したならば false 。</returns>
        public bool Save()
        {
            if (Interlocked.Exchange(ref this.ioLock, 1) != 0)
            {
                return false;
            }

            try
            {
                // 親ディレクトリ作成
                var dirPath = Path.GetDirectoryName(Path.GetFullPath(this.FilePath));
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                // 書き出し
                using (var stream = File.Create(this.FilePath))
                {
                    this.Serializer.WriteObject(stream, this.Value);
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                Interlocked.Exchange(ref this.ioLock, 0);
            }

            return true;
        }

        /// <summary>
        /// I/O処理排他ロック用。
        /// </summary>
        private int ioLock = 0;

        /// <summary>
        /// コンストラクタ引数値を基に基準ディレクトリパスを作成する。
        /// </summary>
        /// <param name="baseDirectoryPath">コンストラクタ引数値。</param>
        /// <returns>基準ディレクトリパス。</returns>
        private static string MakeBaseDirectoryPath(string baseDirectoryPath)
        {
            var baseDir = baseDirectoryPath;
            bool useCompany = (baseDirectoryPath == null);

            // null ならば実行中プロセスの会社名を用いる
            if (useCompany)
            {
                baseDir =
                    Assembly
                        .GetEntryAssembly()?
                        .GetCustomAttribute<AssemblyCompanyAttribute>()?
                        .Company;
                if (baseDir == null)
                {
                    throw new InvalidOperationException(
                        nameof(AssemblyCompanyAttribute) + @" is not defined.");
                }
                else if (string.IsNullOrWhiteSpace(baseDir))
                {
                    throw new InvalidOperationException(
                        nameof(AssemblyCompanyAttribute) + @" is blank.");
                }
            }
            else if (string.IsNullOrWhiteSpace(baseDir))
            {
                throw new ArgumentException(
                    $@"`{nameof(baseDirectoryPath)}` is blank.",
                    nameof(baseDirectoryPath));
            }

            // 会社名or相対パスならばローカルアプリケーションフォルダを基準位置とする
            if (useCompany || !Path.IsPathRooted(baseDir))
            {
                baseDir =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        baseDir);
            }

            return baseDir;
        }
    }
}
