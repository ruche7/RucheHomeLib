using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

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
        /// <param name="directoryName">
        /// 設定保存先ディレクトリ名。基準位置からの相対パス。
        /// 全体で設定を共有するならば空文字列または null 。
        /// </param>
        public ConfigKeeper(string directoryName)
        {
            this.Value = default(T);

            var fileName = typeof(T).FullName + @".config";
            this.RelativeFilePath =
                string.IsNullOrEmpty(directoryName) ?
                    fileName :
                    Path.Combine(directoryName, fileName);
        }

        /// <summary>
        /// 設定値を取得または設定する。
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 設定ファイルパスを取得する。
        /// </summary>
        public string FilePath =>
            Path.Combine(this.BaseDirectoryPath, this.RelativeFilePath);

        /// <summary>
        /// 設定を読み取る。
        /// </summary>
        /// <returns>成功したならば true 。失敗したならば false 。</returns>
        public bool Load()
        {
            var filePath = this.FilePath;

            // ファイルがなければ読み取れない
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                // 読み取り
                using (var stream = File.OpenRead(filePath))
                {
                    var serializer = this.MakeSerializer();
                    var value = serializer.ReadObject(stream);
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

            return true;
        }

        /// <summary>
        /// 設定を書き出す。
        /// </summary>
        /// <returns>成功したならば true 。失敗したならば false 。</returns>
        public bool Save()
        {
            var filePath = this.FilePath;

            try
            {
                // 親ディレクトリ作成
                var dirPath = Path.GetDirectoryName(Path.GetFullPath(filePath));
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                // 書き出し
                using (var stream = File.Create(filePath))
                {
                    var serializer = this.MakeSerializer();
                    serializer.WriteObject(stream, this.Value);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ベースディレクトリパスを取得する。
        /// </summary>
        protected virtual string BaseDirectoryPath { get; } =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                @"ruche-home");

        /// <summary>
        /// シリアライザを生成する。
        /// </summary>
        /// <returns>シリアライザ。</returns>
        protected XmlObjectSerializer MakeSerializer()
        {
            return new DataContractJsonSerializer(typeof(T));
        }

        /// <summary>
        /// ベースディレクトリからの相対ファイルパスを取得または設定する。
        /// </summary>
        private string RelativeFilePath { get; }
    }
}
