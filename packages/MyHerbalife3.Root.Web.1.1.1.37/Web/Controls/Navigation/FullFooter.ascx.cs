using HL.Common.Configuration;
using MyHerbalife3.Core.ExperienceProvider.Interfaces;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Web.Controls.Navigation
{
    public partial class FullFooter : System.Web.UI.UserControl
    {
        protected readonly IGlobalContext _GlobalContext = (HttpContext.Current.ApplicationInstance as IGlobalContext);        
        public IExperience currentExperience
        {
            get { return _GlobalContext.CurrentExperience; }
        }

        public bool IsMicroServiceEnabled
        {
            get { return Settings.GetRequiredAppSetting("MyHLNavigationEnabled", false); }
        }
    }
}