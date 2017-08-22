using System.Collections.Generic;
using System.IO;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace FastCache
{
    public class FastCacheCore
    {
        public static string[] ExcludedPaths =>
            Configuration.ExcludePaths.Split(',');

        public static void ClearCache()
        {
            var files = GetCacheFiles();

            foreach (var filePath in files)
            {
                File.Delete(filePath);
            }

            LogHelper.Info<string>("Fast Cache Cleared");
        }

        public static IEnumerable<string> GetCacheFiles()
        {
            return Directory.GetFiles(
                IOHelper.MapPath( $"~/{Configuration.FastCacheDirectory}" )
                );
        }
    }
}