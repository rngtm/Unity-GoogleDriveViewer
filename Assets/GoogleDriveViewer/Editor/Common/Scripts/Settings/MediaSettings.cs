using System.Collections.Generic;
using System.Linq;

namespace GoogleDriveViewer
{
    public enum EMediaType
    {
        UNKNOWN,
        PNG,
        JPG,
        MP4,
        MP3,
        ZIP,
        EXCEL,
    }

    internal class MediaBinding
    {
        public EMediaType MediaType { get; private set; }
        public string LocalMimeType { get; private set; }
        public string RemoteMimeType { get; private set; }
        public string FileExtension { get; private set; }

        public MediaBinding(EMediaType mediaType, string localMimeType, string remoteMimeType, string ext)
        {
            MediaType = mediaType;
            LocalMimeType = localMimeType;
            RemoteMimeType = remoteMimeType;
            FileExtension = ext;
        }
    }

    public static class MediaSettings
    {
        static readonly MediaBinding[] MediaBindings = new MediaBinding[]
        {
            new MediaBinding(EMediaType.PNG  , "image/png", "image/png", ".png"  ),
            new MediaBinding(EMediaType.JPG  , "image/jpeg", "image/jpeg", ".jpg"  ),
            new MediaBinding(EMediaType.MP4  , "video/mp4", "video/mp4", ".mp4"  ),
            new MediaBinding(EMediaType.MP3  , "audio/mp3", "audio/mp3", ".mp3"  ),
            new MediaBinding(EMediaType.ZIP  , "application/zip", "application/zip", ".zip"  ),
            new MediaBinding(EMediaType.EXCEL, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                "application/vnd.google-apps.spreadsheet", ".xlsx" ),
        };
        
        static Dictionary<EMediaType, string> LocalMediaToMime
            = MediaBindings.ToDictionary(item => item.MediaType, item => item.LocalMimeType);
        static Dictionary<string, EMediaType> LocalMimeToMedia
            = MediaBindings.ToDictionary(item => item.LocalMimeType, item => item.MediaType);
        static Dictionary<EMediaType, string> RemoteMediaToMime
            = MediaBindings.ToDictionary(item => item.MediaType, item => item.RemoteMimeType);
        static Dictionary<string, EMediaType> RemoteMimeToMedia
            = MediaBindings.ToDictionary(item => item.RemoteMimeType, item => item.MediaType);
        static Dictionary<EMediaType, string> MediaToExt
            = MediaBindings.ToDictionary(item => item.MediaType, item => item.FileExtension);
        static Dictionary<string, EMediaType> ExtToMedia
            = MediaBindings.ToDictionary(item => item.FileExtension, item => item.MediaType);

        public static string GetExtensionFromMedia(EMediaType type)
        {
            if (MediaToExt.ContainsKey(type))
            {
                return MediaToExt[type];
            }
            else
            {
                throw new System.NotSupportedException(
                    string.Format("unknown EMediaType : {0}", type));
            }
        }

        public static EMediaType GetMediaFromPath(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath);
            if (!ExtToMedia.ContainsKey(ext))
            {
                return EMediaType.UNKNOWN;
            }
            return ExtToMedia[ext];
        }

        /// local mime (for upload)
        public static string GetLocalMimeFromMedia(EMediaType type)
        {
            if (LocalMediaToMime.ContainsKey(type))
            {
                return LocalMediaToMime[type];
            }
            else
            {
                throw new System.NotSupportedException(
                    string.Format("unknown EMediaType : {0}", type));
            }
        }

        /// local mime (for upload)
        public static EMediaType GetMediaFromLocalMime(string mimeType)
        {
            if (LocalMimeToMedia.ContainsKey(mimeType))
            {
                return LocalMimeToMedia[mimeType];
            }
            else
            {
                throw new System.NotSupportedException(
                    string.Format("unknown MimeType : {0}", mimeType));
            }
        }

        /// remote mime (for download)
        public static string GetRemoteMimeFromMedia(EMediaType type)
        {
            if (RemoteMediaToMime.ContainsKey(type))
            {
                return RemoteMediaToMime[type];
            }
            else
            {
                throw new System.NotSupportedException(
                    string.Format("unknown EMediaType : {0}", type));
            }
        }

        /// remote mime (for download)
        public static EMediaType GetMediaFromRemoteMime(string mimeType)
        {
            if (RemoteMimeToMedia.ContainsKey(mimeType))
            {
                return RemoteMimeToMedia[mimeType];
            }
            else
            {
                throw new System.NotSupportedException(
                    string.Format("unknown MimeType : {0}", mimeType));
            }
        }
    }
}