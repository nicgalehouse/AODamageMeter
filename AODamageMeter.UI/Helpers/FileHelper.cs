using System.IO;

namespace AODamageMeter.UI.Helpers
{
    public static class FileHelper
    {
        public static void CreateEmptyFile(string filePath)
            => File.Create(filePath).Dispose();
    }
}
