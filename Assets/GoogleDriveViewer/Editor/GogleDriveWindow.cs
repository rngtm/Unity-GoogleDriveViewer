using UnityEngine;
using UnityEditor;

namespace GoogleDriveViewer
{
    public class GoogleDriveWindow : EditorWindow
    {
        FileTreeView m_TreeView;
        Vector2 m_TableScroll = new Vector2(0f, 0f);

        [MenuItem("GoogleDrive/GoogleDrive Viewer")]
        static void Open()
        {
            GetWindow<GoogleDriveWindow>("GoogleDriveViewer");
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
                var files = DriveAPI.GetFiles();

                if (files != null && files.Count > 0)
                {
                    m_TreeView.RegisterFiles(files);
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

    }
}