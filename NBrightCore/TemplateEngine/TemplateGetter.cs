using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using NBrightCore.common;

namespace NBrightCore.TemplateEngine
{
    public class TemplateGetter
    {

        private TemplateController TemplCtrl1;
        private TemplateController TemplCtrl2;
        private TemplateController TemplCtrl1b;
        private TemplateController TemplCtrl2b;
        private TemplateController TemplCtrl3;
        private TemplateController TemplCtrl4;

        /// <summary>
        /// Initialize the template getter 
        /// </summary>
        /// <param name="primaryMapPath">folder to look for a themes (On a multiple portal system this will usually be the portal root)</param>
        /// <param name="secondaryMapPath">fallback folder to look for a themes if not found in primary (Usually the module admin folder, where default installed templates are saved)</param>
        /// <param name="defaultThemeFolder">Default theme folder name to look for</param>
        /// <param name="themeFolder">custom theme folder name to look for, if no template is found here the system theme, then the default theme will be searched.</param>
        /// <param name="systemThemeFolder">system theme folder, will search themefolder, systemThemefolder and then defaultThemeFolder</param>
        public TemplateGetter(string primaryMapPath, string secondaryMapPath, string defaultThemeFolder = "NBrightTemplates", string themeFolder = "",string systemThemeFolder = "")
        {
            if (themeFolder != "")
            {
                TemplCtrl1 = new TemplateController(primaryMapPath, themeFolder);
                TemplCtrl2 = new TemplateController(secondaryMapPath, themeFolder);                
            }
            if (systemThemeFolder != "" && systemThemeFolder != themeFolder)
            {
                TemplCtrl1b = new TemplateController(primaryMapPath, systemThemeFolder);
                TemplCtrl2b = new TemplateController(secondaryMapPath, systemThemeFolder);                
            }
            TemplCtrl3 = new TemplateController(primaryMapPath, defaultThemeFolder);
            TemplCtrl4 = new TemplateController(secondaryMapPath, defaultThemeFolder);
        }


        /// <summary>
        /// Get template from the filesytem, search primary mappath (both themes), if not found search socendary mappath (both themes)
        /// </summary>
        /// <param name="templatename">template file anme</param>
        /// <param name="lang">langauge to get</param>
        /// <param name="replaceTemplateTokens">replace the [Template:*] tokens</param>
        /// <param name="replaceStringTokens">replace the [String:*] tokens</param>
        /// <param name="portalLevel">if false the system level template will be returned, even if a portal level template exists</param>
        /// <param name="settings">If passed a replacement of settings tokens is done directly after the template is loaded</param>
        /// <returns></returns>
        public string GetTemplateData(string templatename, string lang, bool replaceTemplateTokens, bool replaceStringTokens,bool portalLevel, Dictionary<String,String> settings)
        {
            var templateData = "";
            var objT = new Template("");
            if (TemplCtrl1 != null)
            {
                // search custom themefolders
                objT = TemplCtrl1.GetTemplate(templatename, lang);
                templateData = objT.TemplateData;
                if (!objT.IsTemplateFound || !portalLevel)
                {
                    objT = TemplCtrl2.GetTemplate(templatename, lang);
                    templateData = objT.TemplateData;
                }                
            }
            if (!objT.IsTemplateFound && TemplCtrl1b != null)
            {
                // search custom systemThemefolders
                objT = TemplCtrl1b.GetTemplate(templatename, lang);
                templateData = objT.TemplateData;
                if (!objT.IsTemplateFound || !portalLevel)
                {
                    objT = TemplCtrl2b.GetTemplate(templatename, lang);
                    templateData = objT.TemplateData;
                }
            }
            if (!objT.IsTemplateFound)
            {
                // search default themefolders
                objT = TemplCtrl3.GetTemplate(templatename, lang);
                templateData = objT.TemplateData;
                if (!objT.IsTemplateFound || !portalLevel)
                {
                    objT = TemplCtrl4.GetTemplate(templatename, lang);
                    templateData = objT.TemplateData;
                }                                
            }

            // we need to replace settings tokens before the templates, so templates can load from a setting token folder.
            if (settings != null) templateData = Utils.ReplaceSettingTokens(templateData, settings);

            if (replaceTemplateTokens) templateData = ReplaceTemplateTokens(templateData, lang);

            if (settings != null) templateData = Utils.ReplaceSettingTokens(templateData, settings); // replace all settings tokens in injected templates.

            if (replaceStringTokens) templateData = ReplaceResourceString(templateData);

            return templateData;
        }

        public string GetTemplateData(string templatename, string lang, bool replaceTemplateTokens = true, bool replaceStringTokens = true, bool portalLevel = true)
        {
            return GetTemplateData(templatename,lang, replaceTemplateTokens,replaceStringTokens,portalLevel,null);
        }

        public string ReplaceTemplateTokens(string templText, string lang, int recursiveCount = 0)
        {
            var strOut = templText;
            if (TemplCtrl1 != null)
            {
                strOut = TemplCtrl1.ReplaceTemplateTokens(strOut, lang, recursiveCount);
                strOut = TemplCtrl2.ReplaceTemplateTokens(strOut, lang, recursiveCount);
            }
            if (TemplCtrl1b != null)
            {
                strOut = TemplCtrl1b.ReplaceTemplateTokens(strOut, lang, recursiveCount);
                strOut = TemplCtrl2b.ReplaceTemplateTokens(strOut, lang, recursiveCount);
            }
            strOut = TemplCtrl3.ReplaceTemplateTokens(strOut, lang, recursiveCount);
            strOut = TemplCtrl4.ReplaceTemplateTokens(strOut, lang, recursiveCount);
            return strOut;
        }

        public string ReplaceResourceString(string templText)
        {
            var strOut = templText;
            if (TemplCtrl1 != null)
            {
                strOut = TemplCtrl1.ReplaceResourceString(strOut);
            }
            if (TemplCtrl1b != null)
            {
                strOut = TemplCtrl1b.ReplaceResourceString(strOut);
            }
            strOut = TemplCtrl3.ReplaceResourceString(strOut);
            return strOut;

        }

        public void SaveTemplate(string templatename, string lang, string templatedata, Boolean portallevel = true)
        {
            // save the template on secondary folder (usually portal in multiportal system) 
            // NOTE: Can't save back to the systemthemefolder.
            if (TemplCtrl1 != null)
            {
                if (portallevel)  // normally only save templates at portal level (So they override default template at module level, but don't overwrite them)
                    TemplCtrl1.SaveTemplate(templatename, lang, templatedata); // save in   
                else
                    TemplCtrl2.SaveTemplate(templatename, lang, templatedata); // save in custom theme (Module Level)
            }
            else
            {
                if (portallevel)  // normally only save templates at portal level
                    TemplCtrl3.SaveTemplate(templatename, lang, templatedata); // save in default theme
                else
                    TemplCtrl4.SaveTemplate(templatename, lang, templatedata); // save in default theme (Module Level)
            }
        }

        public void SaveTemplate(string templatename, string templatedata, Boolean portallevel = true)
        {
            SaveTemplate(templatename, "Default", templatedata, portallevel);
        }

        public void RemovePortalLevelTemplate(string templatename,String lang = "Default")
        {
            // NOTE: Can;t alter systemThemefolder
            if (TemplCtrl1 != null) TemplCtrl1.DeleteTemplate(templatename, lang);
            if (TemplCtrl3 != null) TemplCtrl3.DeleteTemplate(templatename, lang);
        }

    }
}
