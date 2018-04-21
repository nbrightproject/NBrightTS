using System.IO;
using System.Web.UI;
using System.Xml;
using DotNetNuke.Services.Localization;
using NBrightCorev2.render;


namespace NBrightDNNv2.render
{
    [ToolboxData("<{0}:GenLabelControl runat=server></{0}:GenLabelControl>")]
    public sealed class GenLabelControl : UserControl
    {

        public GenLabelControl(XmlNode xmlNod, string ctrlPath = "~/controls/LabelControl.ascx")
        {
            var oControl = (DotNetNuke.UI.UserControls.LabelControl)LoadControl(ctrlPath);
            oControl = (DotNetNuke.UI.UserControls.LabelControl)GenXmlFunctions.AssignByReflection(oControl, xmlNod);
            Controls.Add(oControl);
        }

    }
}
