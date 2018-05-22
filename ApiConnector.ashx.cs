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
                    strOutXml = "<pages><page url='/test/page1'>TEST1</page><page url='/test/page2'>TEST2</page><page url='/test/page3'>TEST3</page></pages>";
                    //strOutXml = "[{\"FriendID\":1,\"FriendMobile\":\"999999786\",\"FriendName\":\"Shree Sai\",\"FriendPlace\":\"Shirdi\"}]";
                    //strJson = "[[29,\"mike\"],[5,\"dummy\"]]";
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