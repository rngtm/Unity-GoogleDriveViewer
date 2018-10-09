using Google.Apis.Drive.v3;

namespace GoogleDriveViewer
{
    public static class DownloadSettings
    {
        /// <summary>
        /// ファイルダウンロード先のフォルダ
        /// </summary>
        public static string GetDownloadFolderPath()
        {
            return UnityEngine.Application.dataPath; // Assets直下
        }
    }
    
    internal static class Settings
    {
        /// <summary>
        /// credentials.jsonのパス (下記URLを参考に作成)
        /// https://developers.google.com/drive/api/v3/quickstart/dotnet
        /// </summary>
        public readonly static string CREDENTIAL_JSON_PATH = "Assets/credentials.json";

        /// <summary>
        /// アプリケーション名
        /// </summary>
        public readonly static string ApplicationName = "GoogleDriveAPIDemoApp";

        /// <summary>
        /// 認証情報の作成先のパス
        /// </summary>
        public readonly static string CREDENTIAL_PATH = ".credentials/drive-dotnet-Demo";

        /// <summary>
        /// GET時に取得するファイルの最大数
        /// </summary>
        public const int MaxFileCount = 100;

        public readonly static string[] Scopes = { DriveService.Scope.Drive };

    }
}
