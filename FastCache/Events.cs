using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.NodeFactory;
using umbraco.businesslogic;

namespace FastCache
{
    public class FastCacheEvents : ApplicationStartupHandler
    {
        public FastCacheEvents()
        {
            Document.AfterPublish += new Document.PublishEventHandler(AfterPublish_Handler);
        }

        static void AfterPublish_Handler(Document sender, umbraco.cms.businesslogic.PublishEventArgs e)
        {
            FastCacheCore.ClearCache();
        }
    }
}