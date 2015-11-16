using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using umbraco.BusinessLogic;
using System.Text;
using System.Security.Cryptography;
using System.Web.Configuration;
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
                HttpContext.Current.Server.MapPath( $"~/{Configuration.FastCacheDirectory}" )
                );
        }

        public static string GetMd5Hash(string input)
        {
            var md5Hash = MD5.Create();

            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            var sb = new StringBuilder();

            foreach( var t in data )
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}