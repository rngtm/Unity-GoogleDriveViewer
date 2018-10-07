using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using File = Google.Apis.Drive.v3.Data.File;
namespace GoogleDriveViewer
{
    internal static class DriveAPI
    {
        /// <summary>
        /// credentials.jsonのパス (下記URLを参考に作成)
        /// https://developers.google.com/drive/api/v3/quickstart/dotnet
        /// </summary>
        public readonly static string CREDENTIAL_JSON_PATH = "Assets/credentials.json";

        /// <summary>
        /// アプリケーション名
        /// </summary>
        static string ApplicationName = "GoogleDriveAPIDemoApp";

        /// <summary>
        /// 認証情報の作成先のパス
        /// </summary>
        public readonly static string CREDENTIAL_PATH = ".credentials/drive-dotnet-Demo";

        static string[] Scopes = { DriveService.Scope.Drive };

        private static DriveService OpenDrive()
        {
            UserCredential credential;

            using (var stream =
                new FileStream(CREDENTIAL_JSON_PATH, FileMode.Open, FileAccess.Read))
            {
                //string credPath = System.Environment.GetFolderPath(
                //    System.Environment.SpecialFolder.Personal);

                string credPath = Path.Combine
                    (System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                     ".credentials/drive.googleapis.com-dotnet-quickstart.json");

                credPath = Path.Combine(credPath, CREDENTIAL_PATH);

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                UnityEngine.Debug.Log("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            return service;
        }

        public static IList<File> GetFiles()
        {
            DriveService drive = OpenDrive();
            // Define parameters of request.
            FilesResource.ListRequest listRequest = drive.Files.List();
            listRequest.PageSize = 100;

            IList<File> files = listRequest.Execute().Files;
            return files;
        }
    }
}
