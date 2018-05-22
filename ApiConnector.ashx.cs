using System;
using System.Web;
using System.Xml;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Linq;
using DotNetNuke.Common;

namespace NBrightDNN
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class ApiConnector : IHttpHandler
    {
        private String _lang = "";

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            #region "Initialize"

            var strOutXml = "";
            var strJson = "";

            var paramCmd = Utils.RequestQueryStringParam(context, "cmd");
            var lang = Utils.RequestQueryStringParam(context, "lang");
            var language = Utils.RequestQueryStringParam(context, "language");

        if (lang == "") lang = language;
        _lang = lang;

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(_lang);

            #endregion

            #region "Do processing of command"

            switch (paramCmd)
            {
                case "test":
                    strOutXml = "<root>" + UserController.Instance.GetCurrentUserInfo().Username + "</root>";
                    break;
                case "dnnpages":
                    strOutXml = "<pages>";
                    var tList = DnnUtils.GetTreeTabListOnTabId();
                    foreach (var tItem in tList)
                    {
                        var tabid = tItem.Key;
                        if (Utils.IsNumeric(tabid))
                        {
                            var taburl = Globals.NavigateURL(Convert.ToInt32(tabid));
                            strOutXml += " <page url='" + taburl + "'>" + tItem.Value + "</page>";
                        }
                    }
                    strOutXml += "</pages>";

                    break;
            }

            #endregion

            #region "return results"

            //send back xml as plain text
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            context.Response.Write(strOutXml);
            context.Response.End();

            #endregion

        }


    }
}