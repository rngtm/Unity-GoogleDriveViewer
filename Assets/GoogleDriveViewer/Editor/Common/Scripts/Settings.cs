using Google.Apis.Drive.v3;

namespace GoogleDriveViewer
{
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

        public readonly static string[] Scopes = { DriveService.Scope.Drive };
    }
}
