using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Umbraco.Web;
using System.Web.Caching;

/*
 * Written by @KevinGiszewski, @ecsplendid
 */

namespace FastCache
{
    public class CacheModule : IHttpModule
    {
        private const string MimeType = "text/html";
        private HttpApplication _app;
        private string _cachedUrl = null;
        private bool _containsExcludedPath;
        private string _hashedPath = null;
        private string _path = null;
        private string _pathQuery = null;
        private bool _hasExtension;
        private StreamWatcher _watcher;
        private static readonly object Lock = new object();

        public void Init(
            HttpApplication app)
        {
            if (!Configuration.FastCacheEnabled)
                return;

            _app = app;
            app.ResolveRequestCache += Start;
            app.PreSendRequestContent += Finish;
        }

        public void Dispose()
        {
        }

        private void Finish(
            object sender,
            EventArgs e)
        {
            // the watcher gets created when we need to record the html to the file system
            // because its not in the app cache or it's not already there (being in the app cache
            // implies it is there). The idea is to avoid all file system access most of the time
            if (_watcher == null)
                return;

            SetParams();

            if (!IsCachable())
                return;

            var pageId = UmbracoContext
                .Current
                .PageId;

            if (pageId == null)
                return;

            var filePath = _app
                .Server
                .MapPath(_cachedUrl);

            Directory.CreateDirectory(
                Path.GetDirectoryName(filePath)
                );

            var html = _watcher
                .ToString()
                .Trim();

            // we don't want to be writing while its being read from another thread
            lock(Lock)
            {
                // put html in file cache (so it can survive an app reload)
                File.WriteAllText(
                    filePath,
                    html
                    );

                // put html in app cache for fast access inside this app cycle
                MutateAppCacheWithHtml(html);
            }
        }

        private void MutateAppCacheWithHtml(
            string html )
        {
            _app
                .Context
                .Cache
                .Insert( _hashedPath, html );
        }

        private static bool HasExcludedPath(
            string path)
        {
            return FastCacheCore
                    .ExcludedPaths
                    .Any(excluded => path
                       .ToLower()
                       .StartsWith(excluded.ToLower())
                        );
        }

        private bool IsCachable()
        {
            return (
                !_containsExcludedPath
                && !_pathQuery.Contains("404")
                && _app.Request.HttpMethod == "GET"
                && _app.Response.ContentType == MimeType
                && _app.Response.StatusCode == 200
                && !_hasExtension
                );
        }

        private void SetParams()
        {
            _path = _app.Request.Url.AbsolutePath;

            _pathQuery = _app.Request.Url.AbsolutePath;

            if( !_pathQuery.EndsWith( "/" ) )
                _pathQuery = $"{_pathQuery}/";

            _containsExcludedPath = HasExcludedPath(_path);
            _hashedPath = FastCacheCore.GetMd5Hash(_pathQuery);
            _hasExtension = _path.Contains('.');
            _cachedUrl = $"~/{Configuration.FastCacheDirectory}/{_hashedPath}.html";
        }

        private void Start(
            object sender,
            EventArgs e)
        {
            SetParams();

            if (!IsCachable())
                return;

            _app.Response.Headers.Add("cache-file", _cachedUrl);

            var appCache = (string)_app.Application.Get(_hashedPath);

            if( _app.Request.RawUrl.Contains( "no-cache" ) )
            {
                _app.Response.Headers.Add("cache-mode", "no-cache");

                // clear the app cache
                _app.Context.Cache.Remove( _hashedPath );
                
                // delete the cache file
                File.Delete(_app.Server.MapPath(_cachedUrl));

                SetWatcher();
                return;
            }

            // in preference, use in-memory app cache as file system access is slow
            // on azure (I assume, because the file system is not local to the machine)
            if ( !string.IsNullOrWhiteSpace(appCache) )
            {
                _app.Response.Headers.Add("cache-mode", "app-cache");
                _app.Response.ContentType = "text/html";
                _app.Response.Write(appCache);

                // don't do anything else!
                _app.Response.End();

                return;
            }
            
            if (File.Exists(_app.Server.MapPath(_cachedUrl)))
            {
                string html = null;

                lock(Lock)
                {
                    html = File.ReadAllText(_app.Server.MapPath(_cachedUrl));

                    MutateAppCacheWithHtml(html);
                }
                
                // force the content type to be text/html in case there
                // isn't a static mime mapping in the web.config file
                _app.Response.ContentType = "text/html";
                _app.Response.Headers.Add("cache-mode", "file-cache");
                _app.Response.Write(html);

                // don't do anything else!
                _app.Response.End();

                return;
            }
            
            // only do this if we want to record the page i.e. 
            // if there is no file or app cache, or if specifically asked to do so
            SetWatcher();
        }

        private void SetWatcher()
        {
            _watcher = new StreamWatcher( _app.Response.Filter );
            _app.Response.Filter = _watcher;
        }
    }
}
