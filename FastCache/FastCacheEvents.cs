using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Web;

namespace FastCache
{
    public class FastCacheEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PublishingStrategy.Published += PublishingStrategy_Published;
        }

        private void PublishingStrategy_Published(
            IPublishingStrategy sender,
            PublishEventArgs<IContent> e
            )
        {
            // specifically delete all the cache files for the published content
            var hashes = e
                .PublishedEntities
                .Select(c =>
                   FastCacheCore.GetMd5Hash(
                       UmbracoContext.Current.UrlProvider.GetUrl(c.Id)
                       ));

            foreach (var hash in hashes)
            {
                var file = IOHelper.MapPath($"~/FastCache/{hash}.html");

                // clear it out of the app cache
                UmbracoContext
                    .Current
                    .HttpContext
                    .Cache
                    .Remove(hash);

                System.IO.File.Delete(file);

                LogHelper.Info<string>($"Deleted Umbraco cache file {file}");
            }
        }
    }
}