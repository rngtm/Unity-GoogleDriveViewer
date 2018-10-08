using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoogleDriveViewer
{
    public enum EMediaType
    {
        UNKNOWN = -1,
        PNG = 0,
        EXCEL,
        MP4,
    }

    public static class MediaSettings
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

        static Dictionary<string, EMediaType> ExtensionToMedia = new Dictionary<string, EMediaType>
        {
            { ".png", EMediaType.PNG },
            { ".mp4", EMediaType.MP4 },
            { ".xlsx", EMediaType.EXCEL },
        };

        public static EMediaType GetMediaType(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath);
            if (!ExtensionToMedia.ContainsKey(ext))
            {
                return EMediaType.UNKNOWN;
            }
            return ExtensionToMedia[ext];
        }

        public static string GetContentType(EMediaType type)
        {
            if (MediaToContentType.ContainsKey(type))
            {
                return MediaToContentType[type];
            }
            else
            {
                throw new System.NotSupportedException(
                    string.Format("unknown type : {0}", type));
            }
        }

        public static string GetMimeType(EMediaType type)
        {
            if (MediaToMIME.ContainsKey(type))
            {
                return MediaToMIME[type];
            }
            else
            {
                throw new System.NotSupportedException(
                    string.Format("unknown type : {0}", type));
            }
        }
    }
}