using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using umbraco.BusinessLogic;

using System.Text;
using System.Security.Cryptography;


namespace FastCache
{
    public class FastCacheCore
    {

        public static string CacheDirectory{
            get{
                string cacheDirectory = System.Web.Configuration.WebConfigurationManager.AppSettings["fastCache:directory"];
                
                if (cacheDirectory == null)
                {
                    cacheDirectory = "FastCache";
                }

                return cacheDirectory;
            }
        }

        public static string[] ExcludedDocTypes
        {
            get
            {
                return System.Web.Configuration.WebConfigurationManager.AppSettings["fastCache:excludedDocTypes"].Split(',');
            }
        }

        public static List<string> ExcludedExtensions
        {
            get
            {
                List<string> list = new List<string>() {"html", "axd"};

                string[] config=System.Web.Configuration.WebConfigurationManager.AppSettings["fastCache:excludedExtension"].Split(',');
                foreach (string ext in config)
                {
                    if (ext != "")
                    {
                        list.Add(ext);
                    }
                }

                return list;
            }
        }

        public static bool Enabled
        {
            get{
                return Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["fastCache:enabled"]);
            }
        }

        public static bool Debug
        {
            get
            {
                return Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["fastCache:debug"]);
            }
        }

        public static string[] ExcludedPaths
        {
            get
            {
                return System.Web.Configuration.WebConfigurationManager.AppSettings["fastCache:excludedPaths"].Split(',');
            }
        }

        public static void ClearCache()
        {
            string[] files = GetCacheFiles();

            foreach (string filePath in files)
            {
                File.Delete(filePath);
            }

            Log.Add(LogTypes.Custom, 0, "Fast Cache Cleared");
        }

        public static string[] GetCacheFiles()
        {
            return Directory.GetFiles(HttpContext.Current.Server.MapPath("~/" + CacheDirectory));
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }
    }
}