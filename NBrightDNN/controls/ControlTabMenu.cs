using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using NBrightCorev2.common;

namespace NBrightDNNv2.controls
{
    public class ControlTabMenu : UserControlBase
    {

        #region "control events"

        public bool DebugMode { get; set;}
        public String ControlAdminPath { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            if (ControlAdminPath == "") ControlAdminPath = ((PortalModuleBase)this.Parent).ControlPath;

            base.OnLoad(e);
            if (Page.IsPostBack == false)
            {
               PageLoad();
            }
        }

        #endregion

        #region  "methods" 

        private void PageLoad()
        {

            var xmlDoc = new System.Xml.XmlDocument();
            var strXslPath = Server.MapPath(ControlAdminPath) + "menu\\ControlTabMenu.xsl";

            try
            {
                xmlDoc.Load(strXslPath);

                if (DebugMode)
                {
                    var xmlDocData = new System.Xml.XmlDocument();
                    xmlDocData.LoadXml(GetActionXml().Trim());
                    xmlDocData.Save(Server.MapPath(ControlAdminPath) + "menu\\menu.xml");                    
                }

                this.Controls.Add(new LiteralControl(XslUtils.XslTransInMemory(GetActionXml().Trim(), xmlDoc.OuterXml)));

            }
            catch (Exception)
            {
                this.Controls.Add(new LiteralControl("CANNOT READ XSL : " + strXslPath));
            }

        }

        private string GetActionXml()
        {
            var p = (BaseAdmin)this.Parent;

            var strXML = "<root>";

            strXML += "<current>";
            strXML += "<controlkeyurl>";
            strXML += Utils.RequestParam(Context, "ctl");            
            strXML += "</controlkeyurl>";
            strXML += "<controlkey>";
            strXML += p.ModuleConfiguration.ModuleControl.ControlKey;
            strXML += "</controlkey>";
            strXML += "<controltitle>";
            strXML += p.ModuleConfiguration.ModuleControl.ControlTitle;
            strXML += "</controltitle>";
            strXML += "<controlsrc>";
            strXML += p.ModuleConfiguration.ModuleControl.ControlSrc;
            strXML += "</controlsrc>";
            strXML += "<lang>";
            strXML += Utils.GetCurrentCulture();
            strXML += "</lang>";
            strXML += "</current>";


            foreach (ModuleControlInfo c in p.ModuleConfiguration.ModuleDefinition.ModuleControls.Values)
            {
                if ((c.ControlType != SecurityAccessLevel.Admin) | (DnnUtils.IsInRole("Administrators")))
                {

                    var o = new ControlLinkInfo();
                    var strTitle = Localization.GetString(c.ControlKey + ".Text", ((PortalModuleBase)this.Parent).ControlPath.TrimEnd('/') + "/App_LocalResources/menu.ascx.resx");
                    if (String.IsNullOrEmpty(strTitle))
                    {
                        strTitle = Localization.GetString(c.ControlKey + ".Text", ControlAdminPath.TrimEnd('/') + "/App_LocalResources/menu.ascx.resx");
                    }
                    if (String.IsNullOrEmpty(strTitle))
                    {
                        strTitle = c.ControlTitle;                        
                    }
                    o.ControlTitle = strTitle;
                    o.ControlKey = c.ControlKey;
                    var strText = Localization.GetString(c.ControlKey + ".Help", ((PortalModuleBase)this.Parent).ControlPath.TrimEnd('/') + "/App_LocalResources/menu.ascx.resx");
                    if (String.IsNullOrEmpty(strText))
                    {
                        strText = Localization.GetString(c.ControlKey + ".Help", ControlAdminPath.TrimEnd('/') + "/App_LocalResources/menu.ascx.resx");
                    }
                    o.Text = strText;
                    o.ViewOrder = c.ViewOrder;
                    o.ControlType = c.ControlType;
                    o.IconFile = p.ControlPath + "menu/img/" + c.ControlKey + ".png";
                    o.KeyID = c.KeyID;
                    o.ModuleControlID = c.ModuleControlID;
                    o.ModuleDefID = c.ModuleDefID;
                    o.NavigateUrl = p.EditUrl(c.ControlKey);
                    o.TabLevel = "";
                    o.ControlSrc = c.ControlSrc;
                    strXML += DotNetNuke.Common.Utilities.XmlUtils.Serialize(o);
                }
            }

            //Add EXIT button
            var oe = new ControlLinkInfo();
            oe.ControlTitle = "Exit";
            oe.ControlKey = "";
            oe.Text = "";
            oe.ViewOrder = 99;
            oe.IconFile = p.ControlPath + "menu/img/exit.png"; ;
            oe.KeyID = -1;
            oe.ModuleControlID = -1;
            oe.ModuleDefID = -1;
            oe.NavigateUrl = DotNetNuke.Common.Globals.NavigateURL(p.TabId);
            oe.TabLevel = "";
            oe.ControlSrc = "";
            strXML += DotNetNuke.Common.Utilities.XmlUtils.Serialize(oe);


            strXML += "</root>";
            return strXML;
        }

        #endregion

        #region "data class"

        public class ControlLinkInfo
        {
            public string ControlTitle { get; set; }
            public string ControlKey { get; set; }
            public string Text { get; set; }
            public int ViewOrder { get; set; }
            public DotNetNuke.Security.SecurityAccessLevel  ControlType { get; set; }
            public string IconFile { get; set; }
            public int KeyID { get; set; }
            public int ModuleControlID { get; set; }
            public int ModuleDefID { get; set; }            
            public string NavigateUrl { get; set; }
            public string TabLevel { get; set; }
            public string ControlSrc { get; set; }            
        }

        #endregion


    }
}
