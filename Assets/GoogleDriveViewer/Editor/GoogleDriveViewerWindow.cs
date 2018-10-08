using UnityEngine;
using UnityEditor;

namespace GoogleDriveViewer
{
    public class GoogleDriveViewerWindow : EditorWindow
    {
        FileTreeView m_TreeView;
        Vector2 m_TableScroll = new Vector2(0f, 0f);

        [MenuItem("GoogleDrive/File Viewer", false, 2)]
        static void Open()
        {
            GetWindow<GoogleDriveViewerWindow>("GoogleDriveViewer");
        }

        private void OnGUI()
        {
            if (m_TreeView == null)
            {
                m_TreeView = new FileTreeView();
            }

            DrawHeader();
            CustomUI.RenderTable(m_TreeView, ref m_TableScroll);
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("GET Files", EditorStyles.toolbarButton))
            {
                m_TreeView.ReloadFiles();
            }
            GUILayout.FlexibleSpace();
            //if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
            //{
            //    m_TreeView.ClearTreeItems();
            //}

            if (GUILayout.Button("GoogleDrive", EditorStyles.toolbarButton))
            {
                System.Diagnostics.Process.Start(@"https://drive.google.com/drive/");
            }

            EditorGUILayout.EndHorizontal();
        }

    }
}