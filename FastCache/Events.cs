using System.Linq;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Web;

namespace FastCache
{
    public class FastCacheEvents : ApplicationEventHandler
    {
        public FastCacheEvents()
        {
            PublishingStrategy.Published += PublishingStrategy_Published;
        }

        /// <summary>
        /// trs note -- these don't seem to get called; only the constructor does
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        public void OnApplicationInitialized(
            UmbracoApplicationBase umbracoApplication,
            ApplicationContext applicationContext)
        {

        }

        private void PublishingStrategy_Published(
            IPublishingStrategy sender,
            PublishEventArgs<IContent> e
            )
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);

            // specifically delete all the cache files for the published content
            var hashes = e
                .PublishedEntities
                .Select(c =>
                   FastCacheCore.GetMd5Hash(
                       umbracoHelper.NiceUrl(c.Id)
                       ));
            
            foreach (var hash in hashes)
            {
                var file = UmbracoContext
                                .Current
                                .HttpContext
                                .Server
                                .MapPath($"~/FastCache/{hash}.html");

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

        public void OnApplicationStarting(
            UmbracoApplicationBase umbracoApplication,
            ApplicationContext applicationContext)
        {

        }

        public void OnApplicationStarted(
            UmbracoApplicationBase umbracoApplication,
            ApplicationContext applicationContext)
        {

        }
    }
}