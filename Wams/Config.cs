using System.IO;
using System.Reflection;

namespace Wams
{
    public class Config
    {
        public const string MEDIA_ACCOUNT_NAME = "YourMediaServiceAccountName";
        public const string MEDIA_ACCESS_KEY = "YourMediaServiceAccessKey";
        public const string STORAGE_ACCOUNT_NAME = "YourStorageAccountName";
        public const string STORAGE_ACCESS_KEY = "YourStorageAccessKey";

        public static string VideoFile
        {
            get
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return string.Format(@"{0}\Wonders of Nature.mp4", path);
            }
        }
    }
}