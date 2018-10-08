using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public bool IsGettingFile { get; private set; }
        public bool IsDeletingFile { get; private set; }
        public bool IsDownloadingFile { get; private set; }

        public FileTreeView() // constructer
            : this(new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[]
            {
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"),
                },
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Id"),
                },
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("MimeType"),
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

        public async void ReloadFilesAsync()
        {
            IsGettingFile = true;

            await Task.Run(() =>
            {
                var files = DriveAPI.GetFiles();
                if (files != null && files.Count > 0)
                {
                    RegisterFiles(files);
                }
            });

            IsGettingFile = false;
            SetSelection(new int[0]); // cancel selection

            Repaint();
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

        private void RegisterFiles(IList<File> files)
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

            EditorApplication.delayCall += () => // main thread
            {
                Reload();
            };
        }

        public void ClearTreeItems()
        {
            var root = new TreeViewItem { depth = -1 };
            var children = new List<TreeViewItem>();

            CurrentBindingItems = children;
            root.children = CurrentBindingItems as List<TreeViewItem>;

            EditorApplication.delayCall += () => // main thread
            {
                Reload();
            };
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

            int id = 0;
            foreach (var child in CurrentBindingItems)
            {
                child.id = id;
                id++;
            }

            BuildRows(rootItem);
        }

        protected override void ContextClickedItem(int id)
        {
            base.ContextClickedItem(id);

            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("ファイルを開く"), false, () =>
            {
                var item = (FileTreeViewItem)GetRows()[id];
                var openURL = DriveAPI.GetFileURL(item.FileId);
                System.Diagnostics.Process.Start(openURL);
            });
            menu.AddItem(new GUIContent("ファイルをダウンロード"), false, () =>
            {
                DownloadFileAsync(id);
            });
            menu.AddSeparator("");

            menu.AddItem(new GUIContent("コピー/ID"), false, () =>
            {
                var item = (FileTreeViewItem)GetRows()[id];
                GUIUtility.systemCopyBuffer = item.FileId;
            });
            menu.AddItem(new GUIContent("コピー/MimeType"), false, () =>
            {
                var item = (FileTreeViewItem)GetRows()[id];
                GUIUtility.systemCopyBuffer = item.MimeType;
            });
            menu.AddSeparator("");

            menu.AddItem(new GUIContent("ファイルを削除"), false, () =>
            {
                var ids = GetSelection();
                bool ok = EditorUtility.DisplayDialog(
                    "ファイルの削除",
                    GetDeleteDialogMessage(ids),
                    "削除する", "やめる"
                    );
                if (!ok) { return; }

                DeleteAsync(ids);
            });

            menu.ShowAsContext();
        }

        private async void DownloadFileAsync(int id)
        {
            var item = (FileTreeViewItem)GetRows()[id];
            var mediaType = MediaSettings.GetMediaFromRemoteMime(item.MimeType);
            var fileExt = MediaSettings.GetExtensionFromMedia(mediaType);
            var fileName = item.FileName + fileExt;
            var savePath = System.IO.Path.Combine(DownloadSettings.GetDownloadFolderPath(), fileName);

            IsDownloadingFile = true;
            Debug.Log("Download start: " + fileName);
            await DriveAPI.DownloadFileAsync(item.FileId, savePath);
            Debug.Log("Download end");
            IsDownloadingFile = false;

            Debug.LogFormat("File saved to: {0}", savePath);
            AssetDatabase.Refresh();
            System.Diagnostics.Process.Start(DownloadSettings.GetDownloadFolderPath()); // ダウンロード先を開く
        }

        private async void DeleteAsync(IList<int> ids)
        {
            IsDeletingFile = true;
            await Task.Run(() =>
            {
                TryDeleteFiles(ids); // 複数削除
            });
            IsDeletingFile = false;
        }

        private void TryDeleteFiles(IList<int> ids)
        {
            foreach (var id in ids)
            {
                var item = (FileTreeViewItem)GetRows()[id];
                DriveAPI.DeleteFile(item.FileId);
                Debug.LogFormat("Delete: {0} ({1})", item.FileName, item.FileId);
            }

            EditorApplication.delayCall += () =>
            {
                ReloadFilesAsync();
            };
        }

        private string GetDeleteDialogMessage(IList<int> ids)
        {
            int counter = 0;
            var message = new StringBuilder();
            message.AppendFormat("{0}個のファイルを削除しますか?\n", ids.Count);
            const int ShowFileCount = 3;
            foreach (var id in ids)
            {
                if (counter >= ShowFileCount)
                {
                    message.AppendFormat("他{0}個のファイル\n", ids.Count - ShowFileCount);
                    break;
                }

                var item = (FileTreeViewItem)GetRows()[id];
                message.AppendFormat("・{0}\n", item.FileName, item.FileId);
                counter++;
            }
            return message.ToString();
        }
    }
}
