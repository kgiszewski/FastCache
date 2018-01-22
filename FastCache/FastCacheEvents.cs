using System.IO;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace FastCache
{
    public class FastCacheEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Published += ContentService_Published;
        }

        private void ContentService_Published(IPublishingStrategy sender, PublishEventArgs<Umbraco.Core.Models.IContent> e)
        {
            // specifically delete all the cache files for the published content
            foreach (var hash in e
            .PublishedEntities
            .Select( pe => UmbracoContext.Current.UrlProvider.GetUrl(pe.Id) )
            // if the URL is empty or a empty-anchor (hash), then skip
            .Where( url => string.IsNullOrWhiteSpace(url) || url.Equals("#") )
            // get a hash of the url and cached file path
            .Select( s => url.ToMd5() )
            )
            {
                var file = IOHelper.MapPath($"~/FastCache/{hash}.html");

                // clear it out of the app cache
                UmbracoContext
                    .Current
                    .HttpContext
                    .Cache
                    .Remove(hash);

                File.Delete(file);

                LogHelper.Info<string>($"Deleted Umbraco cache file {file}");
            }
        }
    }
}