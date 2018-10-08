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
using Debug = UnityEngine.Debug;
using Google.Apis.Download;

namespace GoogleDriveViewer
{
    public class DriveAPI
    {

        public static string GetFileURL(string fileId)
        {
            return string.Format("https://drive.google.com/open?id={0}", fileId);
        }

        public static string UploadFile(EMediaType type, string uploadName, string filePath)
        {
            DriveService drive = OpenDrive();

            /////////////////////////////// UPLOAD FILE /////////////////////////////////////
            File body = new File();
            body.Name = uploadName;
            body.Description = "test upload";
            body.MimeType = MediaSettings.GetContentType(type);
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
                Console.WriteLine("Credential file saved to: " + credPath);
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
                var request = service.Files.Create(body, stream, MediaSettings.GetMimeType(type)); // https://developers.google.com/drive/api/v3/mime-types
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
