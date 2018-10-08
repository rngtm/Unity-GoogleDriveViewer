using UnityEngine;
using UnityEditor;

namespace GoogleDriveViewer
{
    public class GoogleDriveViewerWindow : EditorWindow
    {
        FileTreeView m_TreeView;
        Vector2 m_TableScroll = new Vector2(0f, 0f);

        [MenuItem(EditorSettings.MENU_TEXT_FILE_VIEWER, false, EditorSettings.MENU_ORDER_FILE_VIEWER)]
        static void Open()
        {
            GetWindow<GoogleDriveViewerWindow>(EditorSettings.WINDOW_TITLE_FILE_VIEWER);
        }

        private void OnGUI()
        {
            if (m_TreeView == null)
            {
                m_TreeView = new FileTreeView();
            }

            EditorGUI.BeginDisabledGroup(m_TreeView.IsGettingFile | m_TreeView.IsDeletingFile | m_TreeView.IsDownloadingFile);
            DrawHeader();
            CustomUI.RenderTable(m_TreeView, ref m_TableScroll);
            EditorGUI.EndDisabledGroup();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("GET Files", EditorStyles.toolbarButton))
            {
                m_TreeView.ReloadFilesAsync();
            }
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("GoogleDrive", EditorStyles.toolbarButton))
            {
                System.Diagnostics.Process.Start(@"https://drive.google.com/drive/");
            }

            EditorGUILayout.EndHorizontal();
        }

    }
}