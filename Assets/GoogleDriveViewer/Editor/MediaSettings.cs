using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoogleDriveViewer
{
    public enum EMediaType
    {
        PNG,
        EXCEL,
        MP4,
    }

    internal static class MediaSettings
    {
        static Dictionary<EMediaType, string> MediaToContentType = new Dictionary<EMediaType, string>
        {
            { EMediaType.PNG  , "image/png"   },
            { EMediaType.MP4  , "video/mp4"   },
            { EMediaType.EXCEL, "vnd.ms-excel"},
        };

        // https://developers.google.com/drive/api/v3/mime-types
        static Dictionary<EMediaType, string> MediaToMIME = new Dictionary<EMediaType, string>
        {
            { EMediaType.PNG  , "application/vnd.google-apps.photo" },
            { EMediaType.MP4  , "application/vnd.google-apps.video" },
            { EMediaType.EXCEL, "application/vnd.google-apps.spreadsheet" }
        };


        public static string GetContentType(EMediaType type)
        {
            string result = "";
            switch (type)
            {
                case EMediaType.PNG:
                    result = "image/png";
                    break;
                case EMediaType.MP4:
                    result = "video/mp4";
                    break;
                case EMediaType.EXCEL:
                    result = "application/vnd.ms-excel";
                    break;
            }

            return result;

            //if (MediaToContentType.ContainsKey(type))
            //{
            //    return MediaToContentType[type];
            //}
            //else
            //{
            //    throw new System.NotSupportedException();
            //}
        }

        public static string GetMimeType(EMediaType type)
        {
            if (MediaToMIME.ContainsKey(type))
            {
                return MediaToMIME[type];
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }
    }
}