using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Umbraco.Web;
using System.Web.Caching;

/*
 * Written by @KevinGiszewski 
 */

namespace FastCache
{
    public class CacheModule : IHttpModule
    {
        private const string MimeType = "text/html";
        private readonly bool _enabled = Configuration.FastCacheEnabled;
        private HttpApplication _app;
        private string _cachedUrl = null;
        private bool _containsExcludedPath;
        private string _hashedPath = null;
        private string _path = null;
        private string _pathQuery = null;
        private bool _hasExtension;
        private StreamWatcher _watcher;

        public void Init(
            HttpApplication app )
        {
            if( !_enabled )
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
            EventArgs e )
        {
            SetParams();

            if( !IsCachable() )
                return;
            
            var pageId = UmbracoContext.Current.PageId;

            if( pageId == null )
                return;
            
            var html = _watcher
                .ToString()
                .Trim();
        
            var filePath = _app.Server.MapPath(_cachedUrl);

            Directory.CreateDirectory( 
                Path.GetDirectoryName( filePath ) 
                );
            
            File.WriteAllText( 
                filePath, 
                html 
                );
            
            _app.Application.Set(_hashedPath, html);
        }

        private static bool HasExcludedPath(
            string path )
        {
            return FastCacheCore
                    .ExcludedPaths
                    .Any( excluded => path
                        .ToLower()
                        .StartsWith( excluded.ToLower() ) 
                        );
        }

        private bool IsCachable()
        {
            return (
                !_containsExcludedPath 
                && !_pathQuery.Contains( "404" ) 
                &&  _app.Request.HttpMethod == "GET" 
                && _app.Response.ContentType == MimeType 
                && _app.Response.StatusCode == 200 
                && !_hasExtension
                );
        }

        private void SetParams()
        {
            _path = _app.Request.Url.AbsolutePath;

            _pathQuery = _app.Request.Url.PathAndQuery;

            _containsExcludedPath = HasExcludedPath( _path );

            _hashedPath = FastCacheCore.GetMd5Hash( _pathQuery );

            _hasExtension = _path.Contains( '.' );
            
            _cachedUrl = $"~/{Configuration.FastCacheDirectory}/{_hashedPath}.html";
        }

        private void Start(
            object sender,
            EventArgs e )
        {
            SetParams();

            if( !IsCachable() )
                return;

            var appCache = (string) _app.Application.Get(_hashedPath );

            // in preference, use in-memory app cache as file system access is slow
            // on azure
            if ( appCache != null )
            {
                _app.Response.Write(appCache);
                _app.Response.Headers.Add("cache-mode", "app-cache");
                _app.Response.ContentType = "text/html";
            }

            else if ( File.Exists( _app.Server.MapPath( _cachedUrl ) ) )
            {
                _app.Application.Set(
                    _hashedPath, 
                    File.ReadAllText(_app.Server.MapPath(_cachedUrl)) 
                    );

                _app.Server.Transfer( _cachedUrl );
                _app.Response.Headers.Add("cache-mode", "file-cache");

                // force the content type to be text/html in case there
                // isn't a static mime mapping in the web.config file
                _app.Response.ContentType = "text/html";
            }

            else
            {
                _watcher = new StreamWatcher( _app.Response.Filter );
                _app.Response.Filter = _watcher;
            }
        }
    }
}
