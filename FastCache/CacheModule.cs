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
        private string _cachedUrl = string.Empty;
        private bool _containsExcludedPath;
        private string _extension = string.Empty;
        private string _hashedPath = string.Empty;
        private string _path = string.Empty;
        private string _pathQuery = string.Empty;
        private bool _hasExtension;

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

            var filter = (StreamWatcher) _app.Context.Items[Configuration.WatcherCode];

            if( filter == null )
                return;

            var pageId = UmbracoContext.Current.PageId;

            if( pageId == null )
                return;
            
            //setup for a new cached file
            var responseText = filter.ToString()
                .Trim();

            //write the new cache file
            var file = new FileInfo( _app.Server.MapPath( _cachedUrl ) );
            file.Directory.Create();

            File.WriteAllText( _app.Server.MapPath( _cachedUrl ), responseText );
        }

        private static bool HasExcludedPath(
            string path )
        {
            return FastCacheCore.ExcludedPaths.Any( excluded => path.ToLower()
                .StartsWith( excluded.ToLower() ) );
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

            if( _hasExtension )
            {
                _extension = Path
                    .GetExtension( _path )
                    .Substring( 1 );
            }

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
                var watcher = new StreamWatcher( _app.Response.Filter );

                _app.Context.Items[Configuration.WatcherCode] = watcher;
                _app.Response.Filter = watcher;
            }
        }
    }
}
