using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using File = Google.Apis.Drive.v3.Data.File;

namespace GoogleDriveViewer
{
    internal class FileTreeViewItem : TreeViewItem
    {
        public string FileName = "-";
        public string FileId = "-";
        public string MimeType = "-";
    }

    internal class FileTreeView : TreeView
    {
        static readonly Vector2 ButtonSize = new Vector2(48f, 16f);
        static readonly float ButtonSpaceX = 3f;
        static readonly float ButtonPositionY = 2f;
        static readonly int RowHeight = 20;
        static readonly string SortedColumnIndexStateKey = "AssetStoreImporterTreeView_sortedColumnIndex";
        static readonly int DefaultSortedColumnIndex = 1;
        public IReadOnlyList<TreeViewItem> CurrentBindingItems;
        private Action<int> m_DeleteFileAction;

        public FileTreeView() // constructer
            : this(new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[]
            {
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"),
                },
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Id"),
                },
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("MimeType"),
                    width = 40f,
                },
            })))
        {
        }

        public FileTreeView(TreeViewState state, MultiColumnHeader header) // constructer
            : base(state, header)
        {
            rowHeight = RowHeight;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            header.sortingChanged += Header_sortingChanged;

            header.ResizeToFit();
            Reload();

            header.sortedColumnIndex = SessionState.GetInt(SortedColumnIndexStateKey, DefaultSortedColumnIndex);
        }

        public void ReloadFiles()
        {
            var files = DriveAPI.GetFiles();
            if (files != null && files.Count > 0)
            {
                RegisterFiles(files);
            }
        }

        protected override void RowGUI(RowGUIArgs args) // draw gui
        {
            var item = args.item as FileTreeViewItem;

            for (var visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
            {
                var rect = args.GetCellRect(visibleColumnIndex);
                var columnIndex = args.GetColumn(visibleColumnIndex);
                var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                labelStyle.alignment = TextAnchor.MiddleLeft;

                switch (columnIndex)
                {
                    case 0:
                        EditorGUI.LabelField(rect, item.FileName, labelStyle);
                        break;
                    case 1:
                        EditorGUI.LabelField(rect, item.FileId, labelStyle);
                        break;
                    case 2:
                        EditorGUI.LabelField(rect, item.MimeType, labelStyle);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
                }
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { depth = -1 };
            if (CurrentBindingItems == null || CurrentBindingItems.Count == 0)
            {
                var children = new List<TreeViewItem>();
                CurrentBindingItems = children;
            }

            root.children = CurrentBindingItems as List<TreeViewItem>;
            return root;
        }


        public void RegisterFiles(IList<File> files)
        {
            var root = new TreeViewItem { depth = -1 };
            var children = new List<TreeViewItem>();
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                children.Add(new FileTreeViewItem
                {
                    id = i,
                    FileName = file.Name,
                    FileId = file.Id,
                    MimeType = file.MimeType,
                });
            }

            CurrentBindingItems = children;
            root.children = CurrentBindingItems as List<TreeViewItem>;
            Reload();
        }

        public void ClearTreeItems()
        {
            var root = new TreeViewItem { depth = -1 };
            var children = new List<TreeViewItem>();

            CurrentBindingItems = children;
            root.children = CurrentBindingItems as List<TreeViewItem>;
            Reload();
        }


        private void Header_sortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SessionState.SetInt(SortedColumnIndexStateKey, multiColumnHeader.sortedColumnIndex);
            var index = multiColumnHeader.sortedColumnIndex;
            var ascending = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);
            var items = rootItem.children.Cast<FileTreeViewItem>();

            // sorting
            IOrderedEnumerable<FileTreeViewItem> orderedEnumerable;
            switch (index)
            {
                case 0:
                    orderedEnumerable = ascending ? items.OrderBy(item => item.FileName) : items.OrderByDescending(item => item.FileName);
                    break;
                case 1:
                    orderedEnumerable = ascending ? items.OrderBy(item => item.FileId) : items.OrderByDescending(item => item.FileId);
                    break;
                case 2:
                    orderedEnumerable = ascending ? items.OrderBy(item => item.MimeType) : items.OrderByDescending(item => item.MimeType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }

            CurrentBindingItems = rootItem.children = orderedEnumerable.Cast<TreeViewItem>().ToList();
            BuildRows(rootItem);
        }

        public void RegisterDeleteFileAction(Action<int> action)
        {
            m_DeleteFileAction = action;
        }

        protected override void ContextClickedItem(int id)
        {
            base.ContextClickedItem(id);
            var selection = GetSelection();
            if (selection.Count == 0) { return; }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("ファイルを削除"), false, () =>
            {
                TryDeleteFile(id);

                //if (selection.Count == 1)
                //{
                //    TryDeleteFile(id);
                //}
                //else
                //{
                //    TryDeleteFiles(selection); // 複数削除
                //}
            });
            menu.ShowAsContext();
        }

        private void TryDeleteFile(int id)
        {
            var item = (FileTreeViewItem)GetRows()[id];
            bool ok = EditorUtility.DisplayDialog(
                "ファイルの削除",
                string.Format("ファイル \'{0}\'を削除しますか? \n(ファイルID :{1})", item.FileName, item.FileId),
                "削除する", "やめる"
                );
            if (!ok) { return; }

            DriveAPI.DeleteFile(item.FileId);
            Debug.LogFormat("Delete : {0}", item.FileId);

            ClearTreeItems();
            EditorApplication.delayCall += () =>
            {
                ReloadFiles();
            };
        }

        private void TryDeleteFiles(IList<int> ids)
        {
            bool ok = EditorUtility.DisplayDialog(
                "ファイルの削除",
                string.Format("{0}個のファイルを削除しますか? ", ids.Count),
                "削除する", "やめる"
                );
            if (!ok) { return; }

            foreach (var id in ids)
            {
                var item = (FileTreeViewItem)GetRows()[id];
                DriveAPI.DeleteFile(item.FileId);
                Debug.LogFormat("Delete: {0} ({1})", item.FileName, item.FileId);
            }

            ClearTreeItems();
            EditorApplication.delayCall += () =>
            {
                ReloadFiles();
            };
        }
    }
}
