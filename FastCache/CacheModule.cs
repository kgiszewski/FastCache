using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Security.Cryptography;

using Umbraco.Core.Services;

/*
 * Written by @KevinGiszewski 
 * 
 * 
 */

namespace FastCache
{
    public class CacheModule:IHttpModule
    {
        private HttpApplication app;
        private MD5 md5=MD5.Create();

        private static string mimeType = "text/html";

        private bool enabled = FastCacheCore.Enabled;
        private bool debug = FastCacheCore.Debug;

        private string cachedUrl = "";

        private string path = "";

        private string pathQuery = "";
        private string hashedPath = "";

        private string extension = "";

        private bool containsExcludedPath = false;

        public void Init(System.Web.HttpApplication _app)
        {
            if (enabled)
            {
                app = _app;
                _app.ResolveRequestCache += new EventHandler(Start);
                _app.PreSendRequestContent += new EventHandler(Finish);
            }
        }

        public void Dispose()
        {

        }

        private void Start(Object sender, EventArgs e)
        {
            SetParams();

            if (IsCachable())
            {
                if (debug) app.Response.Write("Looking for=>" + cachedUrl + "<br/>");
                if (debug) app.Response.Write("Mime/Type=>" + app.Response.ContentType + "<br/>");
                //if (debug) app.Response.Write("Extension=>" + extension + "<br/>");
                //app.Response.End();

                
                if (File.Exists(app.Server.MapPath(cachedUrl)))
                {
                    if (debug) app.Response.Write("Found cache=>" + hashedPath + " - " + pathQuery + " - " + app.Request.Url.Query + " - " + cachedUrl + " - "+ app.Response.StatusCode + "<br/>");
                    //app.Context.RewritePath(cachedUrl,false);
                    app.Server.Transfer(cachedUrl);
                    
                }
                else
                {
                    StreamWatcher watcher = new StreamWatcher(app.Response.Filter);
                    app.Context.Items["StaticCacheModule_watcher"] = watcher;
                    app.Response.Filter = watcher;  
                }
            }
        }

        private void Finish(object sender, EventArgs e)
        {
            SetParams();
                
            if (IsCachable())
            {
               StreamWatcher filter = app.Context.Items["StaticCacheModule_watcher"] as StreamWatcher; 

                if (filter != null)
                {
                    //test to see if the doctype is allowed to be cached

                    int? pageId = Umbraco.Web.UmbracoContext.Current.PageId;
                    if (pageId != null)
                    {
                        Umbraco.Core.Models.IContent currentPage= Umbraco.Core.ApplicationContext.Current.Services.ContentService.GetById((int)pageId);

                        //test the doctype
                        if (!FastCacheCore.ExcludedDocTypes.Contains(currentPage.ContentType.Alias))
                        {
                            //setup for a new cached file
                            string responseText = filter.ToString().Trim();

                            if (debug) app.Response.Write("Cache Size: " + responseText.Length + "<br/>");

                            //write the new cache file
                            FileInfo file = new System.IO.FileInfo(app.Server.MapPath(cachedUrl));
                            file.Directory.Create();

                            File.WriteAllText(app.Server.MapPath(cachedUrl), responseText);
                            if (debug) app.Response.Write("Creating Cache file: " + cachedUrl + "<br/>");
                        }
                    }
                }
            }            
        }

        private bool HasExcludedPath(string path){

            foreach(string excluded in FastCacheCore.ExcludedPaths){
                //if (debug) app.Response.Write("Checking path: " + path + " against "+ excluded +"<br/>");
                                
                if(path.ToLower().StartsWith(excluded.ToLower())){
                    //if (debug) app.Response.Write("rejected path=> " + path+ "<br/>");
                    return true;
                }
            }
            return false;
        }

        private void SetParams()
        {
            path = app.Request.Url.AbsolutePath;
            pathQuery = app.Request.Url.PathAndQuery;
            
            containsExcludedPath = HasExcludedPath(path);
            hashedPath = FastCacheCore.GetMd5Hash(md5, pathQuery);

            if (path.Contains('.'))
            {
                try
                {
                    extension = Path.GetExtension(path).Substring(1);
                }
                catch { }
            }

            cachedUrl = "~/" + FastCacheCore.CacheDirectory + "/" + hashedPath + ".html";
        }

        private bool IsCachable()
        {
            return (
                !containsExcludedPath &&
                !pathQuery.Contains("404") &&
                app.Request.HttpMethod == "GET" &&
                app.Response.ContentType == mimeType &&
                app.Response.StatusCode == 200 &&
                !FastCacheCore.ExcludedExtensions.Contains(extension)
            );
        }
    }
}