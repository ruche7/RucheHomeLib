﻿using System;
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
        /// <param name="subDirectoryName">
        /// 設定保存先サブディレクトリ名。基準ディレクトリからの相対パス。
        /// 全体で設定を共有するならば空文字列または null 。
        /// </param>
        /// <param name="baseDirectoryPath">
        /// 基準ディレクトリパス。既定値を用いるならば空文字列または null 。
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

            this.BaseDirectoryPath =
                !string.IsNullOrEmpty(baseDirectoryPath) ?
                    baseDirectoryPath :
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        @"ruche-home");

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

            return true;
        }

        /// <summary>
        /// 設定を書き出す。
        /// </summary>
        /// <returns>成功したならば true 。失敗したならば false 。</returns>
        public bool Save()
        {
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

            return true;
        }
    }
}