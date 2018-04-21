using System;
using System.Reflection;
using System.Web.UI;
using System.Xml;
using NBrightCorev2.render;

namespace NBrightDNNv2.render
{
    [ToolboxData("<{0}:GenFileUpload runat=server></{0}:GenTextEditor>")]
    public sealed class GenFileUpload : UserControl
    {

        public string Text
        {
            get { return GetProperty("Text"); }
            set { SetProperty("Text", value); }
        }

        public GenFileUpload(XmlNode xmlNod, string ctrlPath = "~/controls/filepickeruploader.ascx")
        {
            var oControl = LoadControl(ctrlPath);
            oControl = (Control) GenXmlFunctions.AssignByReflection(oControl, xmlNod);
            Controls.Add(oControl);
        }

        public string GetProperty(string propertyName)
        {

            var typ = Controls[0].GetType();
            var prop = typ.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
            {
                return Convert.ToString(prop.GetValue(Controls[0], null));
            }
            return "";
        }

        public void SetProperty(string propertyName ,object value)
        {

            var typ = Controls[0].GetType();
            var prop = typ.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
            {
                prop.SetValue(Controls[0], value, null);
            }
        }

    }
}
