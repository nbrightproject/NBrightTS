using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Xml;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Localization;
using NBrightCore.common;
using NBrightCore.providers;

namespace NBrightDNN
{
    public class DnnInterface : CmsProvider
    {

        public override int GetCurrentUserId()
        {
            var objUser = UserController.GetCurrentUserInfo();
            return objUser.UserID;
        }

        public override String GetCurrentUserName()
        {
            var objUser = UserController.GetCurrentUserInfo();
            if (objUser.Username == null) return "";
            return objUser.Username;
        }

        public override bool IsInRole(string testRole)
        {
            var objUser = UserController.GetCurrentUserInfo();
            return objUser.IsInRole(testRole);
        }

        public override string HomeMapPath()
        {
            return DotNetNuke.Entities.Portals.PortalSettings.Current.HomeDirectoryMapPath;
        }

        public override void SetCache(string cacheKey, object objObject, DateTime absoluteExpiration)
        {
            DataCache.SetCache(cacheKey, objObject, absoluteExpiration);
        }

        public override object GetCache(string cacheKey)
        {
            return DataCache.GetCache(cacheKey);
        }

        public override void RemoveCache(string cacheKey)
        {
            DataCache.RemoveCache(cacheKey);
        }

        public override Dictionary<int, string> GetTabList(string cultureCode)
        {
            return DnnUtils.GetTreeTabList();
        }

        public override List<string> GetCultureCodeList()
        {
            return DnnUtils.GetCultureCodeList();
        }

		public override Dictionary<String, String> GetResourceData(String resourcePath, String resourceKey,String lang = "")
		{
		    return DnnUtils.GetResourceData(resourcePath, resourceKey, lang);
		}





    }
}
