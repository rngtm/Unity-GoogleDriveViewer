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
                case 1:
                    orderedEnumerable = ascending ? items.OrderBy(item => item.FileName) : items.OrderByDescending(item => item.FileName);
                    break;
                case 2:
                    orderedEnumerable = ascending ? items.OrderBy(item => item.FileId) : items.OrderByDescending(item => item.FileId);
                    break;
                case 3:
                    orderedEnumerable = ascending ? items.OrderBy(item => item.MimeType) : items.OrderByDescending(item => item.MimeType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }

            CurrentBindingItems = rootItem.children = orderedEnumerable.Cast<TreeViewItem>().ToList();
            BuildRows(rootItem);
        }
    }
}
