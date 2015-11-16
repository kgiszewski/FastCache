using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Umbraco.Core;
using Umbraco.Web;

/*
 * Written by @KevinGiszewski 
 */

namespace FastCache
{
    public class CacheModule : IHttpModule
    {
        private const string MimeType = "text/html";
        private readonly bool _enabled = Configuration.FastCacheEnabled;
        private readonly MD5 _md5 = MD5.Create();
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
            
            //setup for a new cached file
            var responseText = _watcher
                .ToString()
                .Trim();

            //write the new cache file
            var file = new FileInfo( _app.Server.MapPath( _cachedUrl ) );

            if(!file.Directory.Exists)
                file.Directory.Create();

            File.WriteAllText( _app.Server.MapPath( _cachedUrl ), responseText );
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

            _hashedPath = FastCacheCore.GetMd5Hash( _md5, _pathQuery );

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

            if( File.Exists( _app.Server.MapPath( _cachedUrl ) ) )
            {
                _app.Server.Transfer( _cachedUrl );
            }

            else
            {
                _watcher = new StreamWatcher( _app.Response.Filter );
                _app.Response.Filter = _watcher;
            }
        }
    }
}
