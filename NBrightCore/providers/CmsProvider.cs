using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Web.UI.WebControls;
using System.Xml;

namespace NBrightCore.providers
{
    public abstract class CmsProvider : ProviderBase
    {

        public abstract int GetCurrentUserId();

        public abstract string GetCurrentUserName();

        public abstract bool IsInRole(string testRole);

        public abstract string HomeMapPath();

        public abstract void SetCache(string cacheKey, object objObject, DateTime absoluteExpiration);

        public abstract object GetCache(string cacheKey);

        public abstract void RemoveCache(string cacheKey);

        public abstract Dictionary<int, string> GetTabList(string cultureCode);

        public abstract List<string> GetCultureCodeList();

		// This method is designed to return a list of resource keys and values that can be used for localization.
		public abstract Dictionary<String, String> GetResourceData(String resourcePath, String resourceKey, String lang = "");

    }
}
