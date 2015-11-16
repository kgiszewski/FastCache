using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;


namespace FastCache
{
    public partial class FastCache : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            numPages.InnerHtml = FastCacheCore.GetCacheFiles().Count().ToString();

            if( !Page.IsPostBack )
                return;

            FastCacheCore.ClearCache();
            numPages.InnerHtml = "0";
        }
    }
}