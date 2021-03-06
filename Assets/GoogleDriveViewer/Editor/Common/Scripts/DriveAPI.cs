﻿using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using File = Google.Apis.Drive.v3.Data.File;
using Debug = UnityEngine.Debug;
using Google.Apis.Download;
using System.Threading.Tasks;
using System.Linq;

namespace GoogleDriveViewer
{
    public static class DriveAPI
    {
        public static string GetFileURL(string fileId)
        {
            return string.Format("https://drive.google.com/open?id={0}", fileId);
        }

        /// <summary>
        /// ファイルを削除します
        /// </summary>
        public static string DeleteFile(string fileId)
        {
            FilesResource.DeleteRequest request = OpenDrive().Files.Delete(fileId);
            return request.Execute();
        }

        /// <summary>
        /// ファイルをダウンロードします
        /// </summary>
        public static async Task DownloadFileAsync(string fileId, string savePath)
        {
            var service = OpenDrive();
            var getRequest = service.Files.Get(fileId);
            using (var fs = new System.IO.FileStream(savePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                await getRequest.DownloadAsync(fs);
            }
        }

        /// <summary>
        /// ドキュメントをダウンロードします
        /// </summary>
        public static async Task DownloadDocumentAsync(string fileId, string savePath, string mimeType)
        {
            var service = OpenDrive();
            var request = service.Files.Export(fileId, mimeType);

            using (var fs = new System.IO.FileStream(savePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                await request.DownloadAsync(fs);
            }
        }

        /// <summary>
        /// ファイルをアップロードします
        /// </summary>
        public static string UploadFile(EMediaType type, string uploadName, string filePath)
        {
            DriveService drive = OpenDrive();

            /////////////////////////////// UPLOAD FILE /////////////////////////////////////
            File body = new File();
            body.Name = uploadName;
            body.Description = "test upload";
            body.MimeType = MediaSettings.GetLocalMimeFromMedia(type);
            body.Parents = new List<string>
            {
            };

            string fileId;
            UploadFile(drive, type, body, filePath, out fileId);

            return fileId;
        }

        private static DriveService OpenDrive()
        {
            UserCredential credential;

            using (var stream =
                new FileStream(Settings.CREDENTIAL_JSON_PATH, FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, Settings.CREDENTIAL_PATH);

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Settings.Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                UnityEngine.Debug.Log("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Settings.ApplicationName,
            });
            return service;
        }

        private static void UploadFile(DriveService service, EMediaType type, File body, string filePath, out string fileId)
        {
            // File's content.
            byte[] byteArray = System.IO.File.ReadAllBytes(filePath);
            MemoryStream stream = new MemoryStream(byteArray);
            try
            {
                var request = service.Files.Create(body, stream, MediaSettings.GetLocalMimeFromMedia(type)); // https://developers.google.com/drive/api/v3/mime-types
                request.Upload();

                File file = request.ResponseBody;
                Debug.Log(file.Id);

                fileId = file.Id;
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred: " + e.Message);
                fileId = "";
            }
        }

        /// <summary>
        /// 指定したフォルダの中にあるファイル一覧を取得します
        /// </summary>
        public static IList<File> GetFiles()
        {
            DriveService drive = OpenDrive();

            // Define parameters of request.
            FilesResource.ListRequest listRequest = drive.Files.List();
            listRequest.PageSize = Settings.MaxFileCount; // 取得するファイルの最大数の指定

            IList<File> files = listRequest.Execute().Files;
            return files;
        }

        /// <summary>
        /// 指定したフォルダの中にあるファイル一覧を取得します
        /// </summary>
        public static IList<File> GetFilesInFolder(string folderName) 
        {
            var service = OpenDrive();

            var listRequest = service.Files.List();
            listRequest.PageSize = 1;

            // 取得するフォルダの条件をクエリ構文で指定
            listRequest.Q = $"(name = '{folderName}') and (mimeType = 'application/vnd.google-apps.folder') and (trashed = false)";
            listRequest.Fields = "nextPageToken, files(id)";

            // フォルダの取得
            var folders = listRequest.Execute().Files;
            if (folders == null || folders.Count == 0)
            {
                throw new System.Exception($"Folder not found: {folderName}");
            }

            // フォルダの下に含まれるファイルを取得
            var folderId = folders.First().Id;
            listRequest.Q = $"('{folderId}' in parents) and (mimeType != 'application/vnd.google-apps.folder') and (trashed = false)";
            listRequest.PageSize = Settings.MaxFileCount; // 取得するファイルの最大数の指定
            listRequest.Fields = "nextPageToken, files(id, name, mimeType)";

            // ファイル一覧の取得
            return listRequest.Execute().Files;
        }
    }
}
