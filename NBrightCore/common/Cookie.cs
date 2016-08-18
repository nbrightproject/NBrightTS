using System;
using System.Globalization;
using System.Web;

namespace NBrightCore.common
{
    public class Cookie
    {

        public static void RemoveCookie(int portalId, string cookieName)
        {
            var fullCookieName = string.Format("NBright_{0}_Portal{1}", cookieName, portalId.ToString(CultureInfo.InvariantCulture));
            if (HttpContext.Current != null)
            {
                var foundCookie = HttpContext.Current.Request.Cookies[fullCookieName];
                if ((foundCookie != null))
                {
                    foundCookie.Expires = DateTime.Now.AddYears(-30);
                    HttpContext.Current.Response.Cookies.Add(foundCookie);
                }
            }
        }

        //public static HttpCookie GetCookie(int portalId, string cookieName)
        //{
        //    return GetCookie(portalId, cookieName, "");
        //}

        //public static HttpCookie GetCookie(int portalId, string cookieName, string encryptkey)
        //{
        //    var fullCookieName = string.Format("NBright_{0}_Portal{1}", cookieName, portalId.ToString(CultureInfo.InvariantCulture));
        //    var foundCookie = HttpContext.Current.Request.Cookies[fullCookieName] ?? new HttpCookie(fullCookieName);
        //    return foundCookie;
        //}

        #region get



        public static string GetCookieValue(int portalId, string cookieName, string valueId)
        {
            return GetCookieValue(portalId,cookieName, valueId, "");
        }

        public static string GetCookieValue(int portalId, string cookieName, string valueId, string encryptkey)
        {
            var fullCookieName = string.Format("NBright_{0}_Portal{1}", cookieName, portalId.ToString(CultureInfo.InvariantCulture));
            HttpCookie foundCookie = HttpContext.Current.Request.Cookies[fullCookieName];
            return GetCookieValue(foundCookie, valueId, encryptkey).Replace("*SC*", ";").Replace("*AMP*", "&");
        }

        #endregion

        #region set

        public static void SetCookieValue(int portalId, string cookieName, string valueId, string value)
        {
            SetCookieValue(portalId, cookieName, valueId, value, 30, "");
        }

        public static void SetCookieValue(int portalId, string cookieName, string valueId, string value, string encryptkey)
        {
            SetCookieValue(portalId, cookieName, valueId, value, 30, encryptkey);
        }

        public static void SetCookieValue(int portalId, string cookieName, string valueId, string value, int expireDays)
        {
            SetCookieValue(portalId, cookieName, valueId, value, expireDays, "");
        }

        public static void SetCookieValue(int portalId, string cookieName, string valueId, string value, int expireDays, string encryptkey)
        {
            var fullCookieName = string.Format("NBright_{0}_Portal{1}", cookieName, portalId.ToString(CultureInfo.InvariantCulture));
            var foundCookie = HttpContext.Current.Request.Cookies[fullCookieName] ?? new HttpCookie(fullCookieName);

            if (value != null)
            {
                // replace special chars, terminates the cookie string
                value = value.Replace(";", "*SC*");
                value = value.Replace("&", "*AMP*");

                if (encryptkey != "")
                {
                    valueId = Security.Encrypt(encryptkey, valueId);
                    //trim any "=" off so the key works
                    valueId = valueId.TrimEnd('=');
                    value = Security.Encrypt(encryptkey, value);
                }
                foundCookie[valueId] = value;                
            }
            else
            {
                foundCookie[valueId] = "";                   
            }

            SetCookie(foundCookie, expireDays);
        }

        #endregion

        #region private methods

        private static void SetCookie(HttpCookie objCookie, int expireDays)
        {
            var amtDay = new TimeSpan(expireDays, 0, 0, 0);
            objCookie.Expires = DateTime.Now + amtDay;
            HttpContext.Current.Response.Cookies.Add(objCookie);
        }

        private static string GetCookieValue(HttpCookie objCookie, string valueId, string encryptkey)
        {
            if (objCookie != null)
            {
                if (encryptkey != "")
                {
                    valueId = Security.Encrypt(encryptkey, valueId);
                    //trim any "=" off so the key works
                    valueId = valueId.TrimEnd('=');
                    return Security.Decrypt(encryptkey,objCookie[valueId]) ?? "";
                }
                return objCookie[valueId] ?? "";
            }
            return "";
        }
        #endregion

    }
}
