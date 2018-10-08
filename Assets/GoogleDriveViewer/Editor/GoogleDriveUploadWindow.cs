using UnityEngine;
using UnityEditor;
using Path = System.IO.Path;
using Google.Apis.Drive.v3;

namespace GoogleDriveViewer
{
    public class GoogleDriveUploader : EditorWindow
    {
        readonly static string[] Scopes = { DriveService.Scope.Drive };

        readonly GUILayoutOption[] EmptyLayoutOption = new GUILayoutOption[0];
        readonly GUILayoutOption[] LabelLayoutOption = new GUILayoutOption[] { GUILayout.Width(110), };
        readonly GUILayoutOption[] SmallLabelLayoutOption = new GUILayoutOption[] { GUILayout.Width(80), };
        readonly GUILayoutOption[] OpenURLButtonLayoutOption = new GUILayoutOption[] { GUILayout.Width(90), };

        [SerializeField] private string m_UploadName;
        [SerializeField] private string m_FilePath;
        [SerializeField] private EMediaType m_MediaType;
        [SerializeField] private string m_FileId = "";
        [SerializeField] private string m_FileURL = "";

        [MenuItem("GoogleDrive/File Uploader")]
        static void Open()
        {
            var window = GetWindow<GoogleDriveUploader>("GoogleDriveViewer");
            window.m_UploadName = "Test Upload PNG";
            window.m_MediaType = EMediaType.PNG;
            window.m_FilePath = Application.dataPath
                + Path.DirectorySeparatorChar + "GoogleDriveViewer"
                + Path.DirectorySeparatorChar + "Sample"
                + Path.DirectorySeparatorChar + "sample.png"
                ;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Upload Name", LabelLayoutOption);
            m_UploadName = EditorGUILayout.TextField(m_UploadName, EmptyLayoutOption);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File Path", LabelLayoutOption);
            m_FilePath = EditorGUILayout.TextField(m_FilePath, EmptyLayoutOption);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File Type", LabelLayoutOption);
            m_MediaType = (EMediaType)EditorGUILayout.EnumPopup(m_MediaType, EmptyLayoutOption);
            EditorGUILayout.EndHorizontal();

            DrawUploadButton();

            GUILayout.Space(16f);

            DrawUploadResponse();
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
            if (GUILayout.Button("アップロード", EmptyLayoutOption))
            {
                if (System.IO.File.Exists(m_FilePath))
                {
                    m_FileId = DriveAPI.UploadFile(m_MediaType, m_UploadName, m_FilePath);
                    m_FileURL = DriveAPI.GetFileURL(m_FileId);
                }
                else
                {
                    throw new System.Exception(string.Format("File not found : {0}", m_FilePath));
                }
            }
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
