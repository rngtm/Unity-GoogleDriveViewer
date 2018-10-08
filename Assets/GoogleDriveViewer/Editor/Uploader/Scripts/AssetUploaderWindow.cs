using UnityEngine;
using UnityEditor;
using Google.Apis.Drive.v3;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace GoogleDriveViewer
{
    public class AssetUploaderWindow : EditorWindow
    {
        readonly static string[] Scopes = { DriveService.Scope.Drive };

        readonly GUILayoutOption[] EmptyLayoutOption = new GUILayoutOption[0];
        readonly GUILayoutOption[] LabelLayoutOption = new GUILayoutOption[] { GUILayout.Width(110), };
        readonly GUILayoutOption[] SmallLabelLayoutOption = new GUILayoutOption[] { GUILayout.Width(80), };
        readonly GUILayoutOption[] OpenURLButtonLayoutOption = new GUILayoutOption[] { GUILayout.Width(90), };

        [SerializeField] private string m_UploadName;
        [SerializeField] private Object m_UploadAsset;
        [SerializeField] private string m_FileId = "";
        [SerializeField] private string m_FileURL = "";
        private EMediaType m_MediaType = EMediaType.UNKNOWN;
        private bool m_IsUploading = false;

        [MenuItem(EditorSettings.MENU_TEXT_ASSET_UPLOADER, false, EditorSettings.MENU_ORDER_ASSET_UPLOADER)]
        static void Open()
        {
            var window = GetWindow<AssetUploaderWindow>(EditorSettings.WINDOW_TITLE_ASSET_UPLOADER);

            window.m_UploadName = "Test Upload Asset";
            var defaultAssetPath = "Assets"
                + Path.DirectorySeparatorChar + "GoogleDriveViewer"
                + Path.DirectorySeparatorChar + "Sample"
                + Path.DirectorySeparatorChar + "sample.png"
                ;
            window.m_UploadAsset = AssetDatabase.LoadAssetAtPath(defaultAssetPath, typeof(Object));
            window.UpdateMediaType();
        }

        private void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(m_IsUploading);

            DrawRegisterNameField();
            DrawRegisterAssetField();
            DrawMediaType();

            DrawUploadButton();
            GUILayout.Space(16f);
            DrawUploadResponse();
            EditorGUI.EndDisabledGroup();
        }

        private void OnFocus()
        {
            UpdateMediaType();
        }

        private void DrawMediaType()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File Type", LabelLayoutOption);
            EditorGUILayout.LabelField(m_MediaType.ToString());
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRegisterNameField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Upload Name", LabelLayoutOption);
            m_UploadName = EditorGUILayout.TextField(m_UploadName, EmptyLayoutOption);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRegisterAssetField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Upload Asset", LabelLayoutOption);
            EditorGUI.BeginChangeCheck();
            m_UploadAsset = EditorGUILayout.ObjectField(m_UploadAsset, typeof(Object), false);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateMediaType();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void UpdateMediaType()
        {
            if (m_UploadAsset != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(m_UploadAsset);
                m_MediaType = MediaSettings.GetMediaType(assetPath);
            }
            else
            {
                m_MediaType = EMediaType.UNKNOWN;
            }
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
                UploadFileAsync();
            }
            EditorGUI.EndDisabledGroup();
        }

        private async void UploadFileAsync()
        {
            m_IsUploading = true;
            m_FileId = "";
            m_FileURL = "";

            var filePath = AssetDatabase.GetAssetPath(m_UploadAsset);
            await Task.Run(() =>
            {
                var mediaType = MediaSettings.GetMediaType(filePath);
                m_FileId = DriveAPI.UploadFile(mediaType, m_UploadName, filePath);
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
