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
        private ApplicationContext _applicationContext;
        private UmbracoApplicationBase _umbracoApplication;

        public void OnApplicationInitialized(
            UmbracoApplicationBase umbracoApplication,
            ApplicationContext applicationContext )
        {
          
        }

        private void PublishingStrategy_Published(
            IPublishingStrategy sender, 
            PublishEventArgs<IContent> e
            )
        {
            var umbracoHelper = new UmbracoHelper( UmbracoContext.Current );

            // specifically delete all the cache files for the published content
            var files = e
                .PublishedEntities
                .Select( c =>
                    FastCacheCore.GetMd5Hash(
                        umbracoHelper.NiceUrl( c.Id )
                        ) )
                .Select(f => _umbracoApplication.Server.MapPath($"/FastCache/{f}"));
            
            foreach( var file in files )
            {
                System.IO.File.Delete(file);

                LogHelper.Info<string>($"Deleted Umbraco cache file {file}");
            }
        }

        public void OnApplicationStarting(
            UmbracoApplicationBase umbracoApplication,
            ApplicationContext applicationContext )
        {
           
        }

        public void OnApplicationStarted(
            UmbracoApplicationBase umbracoApplication,
            ApplicationContext applicationContext )
        {
            _umbracoApplication = umbracoApplication;
            _applicationContext = applicationContext;

            PublishingStrategy.Published += PublishingStrategy_Published;
        }
    }
}