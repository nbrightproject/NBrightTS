using System;
using System.Web;
using System.Xml;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Linq;

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

            var paramCmd = Utils.RequestQueryStringParam(context, "cmd");
            var lang = Utils.RequestQueryStringParam(context, "lang");
            var language = Utils.RequestQueryStringParam(context, "language");

        if (lang == "") lang = language;
        _lang = lang;

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(_lang);

            #endregion

            #region "Do processing of command"

            strOutXml = "<root>** No Action **</root>";
            switch (paramCmd)
            {
                case "test":
                    strOutXml = "<root>" + UserController.Instance.GetCurrentUserInfo().Username + "</root>";
                    break;
            }


            var jsonText = XmlToJson(strOutXml);

        #endregion

            #region "return results"

            //send back xml as plain text
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            context.Response.Write(jsonText);
            context.Response.End();

            #endregion

        }


        private string XmlToJson(string xmlString)
        {
            return new JavaScriptSerializer().Serialize(GetXmlValues(XElement.Parse(xmlString)));
        }

        private Dictionary<string, object> GetXmlValues(XElement xml)
        {
            var attr = xml.Attributes().ToDictionary(d => d.Name.LocalName, d => (object)d.Value);
            if (xml.HasElements) attr.Add("_value", xml.Elements().Select(e => GetXmlValues(e)));
            else if (!xml.IsEmpty) attr.Add("_value", xml.Value);

            return new Dictionary<string, object> { { xml.Name.LocalName, attr } };
        }

    }
}