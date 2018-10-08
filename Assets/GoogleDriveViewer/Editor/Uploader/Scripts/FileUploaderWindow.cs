using UnityEngine;
using UnityEditor;
using Path = System.IO.Path;
using Google.Apis.Drive.v3;
using System.Threading.Tasks;

namespace GoogleDriveViewer
{
    public class FileUploaderWindow : EditorWindow
    {
        readonly static string[] Scopes = { DriveService.Scope.Drive };

        readonly GUILayoutOption[] EmptyLayoutOption = new GUILayoutOption[0];
        readonly GUILayoutOption[] LabelLayoutOption = new GUILayoutOption[] { GUILayout.Width(110), };
        readonly GUILayoutOption[] SmallLabelLayoutOption = new GUILayoutOption[] { GUILayout.Width(80), };
        readonly GUILayoutOption[] OpenURLButtonLayoutOption = new GUILayoutOption[] { GUILayout.Width(90), };

        [SerializeField] private string m_UploadName;
        [SerializeField] private string m_FilePath;
        [SerializeField] private string m_FileId = "";
        [SerializeField] private string m_FileURL = "";
        private EMediaType m_MediaType = EMediaType.UNKNOWN;
        private bool m_IsUploading = false;

        [MenuItem(EditorSettings.MENU_TEXT_FILE_UPLOADER, false, EditorSettings.MENU_ORDER_FILE_UPLOADER)]
        static void Open()
        {
            var window = GetWindow<FileUploaderWindow>(EditorSettings.WINDOW_TITLE_FILE_UPLOADER);
            window.m_UploadName = "Test Upload PNG";
            window.m_FilePath = Application.dataPath
                + Path.DirectorySeparatorChar + "GoogleDriveViewer"
                + Path.DirectorySeparatorChar + "Sample"
                + Path.DirectorySeparatorChar + "sample.png"
                ;

            window.UpdateMediaType();
        }

        private void UpdateMediaType()
        {
            if (!string.IsNullOrEmpty(m_FilePath))
            {
                m_MediaType = MediaSettings.GetMediaFromPath(m_FilePath);
            }
            else
            {
                m_MediaType = EMediaType.UNKNOWN;
            }
        }

        private void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(m_IsUploading);

            DrawRegisterUploadName();
            DrawRegisterFilePath();
            DrawMediaType();

            DrawUploadButton();
            GUILayout.Space(16f);
            DrawUploadResponse();
            EditorGUI.EndDisabledGroup();
        }

        private void DrawRegisterFilePath()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File Path", LabelLayoutOption);
            EditorGUI.BeginChangeCheck();
            m_FilePath = EditorGUILayout.TextField(m_FilePath, EmptyLayoutOption);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateMediaType();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRegisterUploadName()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Upload Name", LabelLayoutOption);
            m_UploadName = EditorGUILayout.TextField(m_UploadName, EmptyLayoutOption);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMediaType()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File Type", LabelLayoutOption);
            EditorGUILayout.LabelField(m_MediaType.ToString());
            EditorGUILayout.EndHorizontal();
        }

        private void DrawUploadResponse()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, EmptyLayoutOption);
            {
                EditorGUILayout.LabelField("Upload Response");
                EditorGUI.indentLevel++;

                DrawFileID();
                GUILayout.Space(1f);
                DrawFileURLWithButton();

                EditorGUI.indentLevel--;
                GUILayout.Space(2f);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawFileURLWithButton()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File URL", SmallLabelLayoutOption);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(m_FileURL, EmptyLayoutOption);
            EditorGUI.EndDisabledGroup();

            DrawOpenURLButton();
            GUILayout.Space(2f);
            EditorGUILayout.EndHorizontal();
        }


        private void DrawFileID()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File ID", SmallLabelLayoutOption);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(m_FileId, EmptyLayoutOption);
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(2f);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Open GoogleDrive", EditorStyles.toolbarButton))
            {
                System.Diagnostics.Process.Start(@"https://drive.google.com/drive/my-drive");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawUploadButton()
        {
            EditorGUI.BeginDisabledGroup(m_MediaType == EMediaType.UNKNOWN);
            if (GUILayout.Button("アップロード", EmptyLayoutOption))
            {
                if (System.IO.File.Exists(m_FilePath))
                {
                    UploadFileAsync();
                }
                else
                {
                    throw new System.Exception(string.Format("File not found : {0}", m_FilePath));
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private async void UploadFileAsync()
        {
            m_IsUploading = true;
            m_FileId = "";
            m_FileURL = "";

            await Task.Run(() =>
            {
                m_FileId = DriveAPI.UploadFile(m_MediaType, m_UploadName, m_FilePath);
                m_FileURL = DriveAPI.GetFileURL(m_FileId);
            });
            Repaint();
            m_IsUploading = false;
        }

        private void DrawOpenURLButton()
        {
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_FileURL));
            if (GUILayout.Button("Open URL", OpenURLButtonLayoutOption))
            {
                System.Diagnostics.Process.Start(m_FileURL);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
