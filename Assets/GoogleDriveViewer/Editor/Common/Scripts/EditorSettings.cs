using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveViewer
{
    public static class EditorSettings
    {
        public const string MENU_TEXT_FILE_VIEWER = "GoogleDrive/File Viewer/Open";
        public const string MENU_TEXT_ASSET_UPLOADER = "GoogleDrive/File Uploader/Open(Asset)";
        public const string MENU_TEXT_FILE_UPLOADER = "GoogleDrive/File Uploader/Open(Path)";

        public const string WINDOW_TITLE_FILE_VIEWER = "GoogleDrive Viewer";
        public const string WINDOW_TITLE_ASSET_UPLOADER = "GoogleDrive Uploader";
        public const string WINDOW_TITLE_FILE_UPLOADER = "GoogleDrive Uploader";


        public const int MENU_ORDER_FILE_VIEWER = 1;
        public const int MENU_ORDER_ASSET_UPLOADER = 2;
        public const int MENU_ORDER_FILE_UPLOADER = 3;
    }
}
