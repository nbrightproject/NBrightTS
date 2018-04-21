using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using NBrightCorev2.common;

namespace NBrightCorev2.TemplateEngine
{
    public class TemplateController
    {

        public string TemplateMapPath { get; private set; }
        public string ThemeFolder { get; private set; }
        public string HomeMapPath { get; private set; }

        private bool _isTemplateFound = false;
        public string TemplateConfigMapPath { get; private set; }

        // Constructor
        public TemplateController(string homeMapPath, string themeFolder = "NBrightTemplates")
        {
            HomeMapPath = homeMapPath.TrimEnd('\\');
            ThemeFolder = themeFolder.TrimEnd('\\');
            TemplateMapPath = string.Format("{0}\\{1}\\", homeMapPath.TrimEnd('\\'), themeFolder.TrimEnd('\\'));
            TemplateConfigMapPath = string.Format("{0}\\{1}\\", homeMapPath.TrimEnd('\\'), "Config");
        }

        #region methods


        private void SetupThemeFolders(string themefolder)
		{
			var folderPath = string.Format("{0}\\{1}\\", TemplateMapPath.TrimEnd('\\'), themefolder);
			if (!Directory.Exists(folderPath))
			{
				try
				{
					Directory.CreateDirectory(folderPath);
				}
				catch (Exception)
				{
					// we might get an error if it's an invlid path,  created by export import DNN portals onto different system folder
					// we can ignore these, becuase when editing a module template the setting should re-align when updated.
				}
			}
		}


    	#endregion

        #region Get

        public List<string> GetTemplateLangauges(string templatename)
        {
            var langList = new List<string>();
            if (File.Exists(TemplateMapPath + "Default\\" + templatename))
            {
                langList.Add("Default");
            }
            var dirs = Directory.GetDirectories(TemplateMapPath, "*-*");
            foreach (var dir in dirs)
            {
                if (File.Exists(dir + "\\" + templatename))
                {
                    langList.Add(dir.Replace(TemplateMapPath, ""));                    
                }
            }
            return langList;
        }

        public List<string> GetLangaugeFolders()
        {
            var langList = new List<string> {"Default"};
            var dirs = Directory.GetDirectories(TemplateMapPath, "*-*");
            foreach (var dir in dirs)
            {
                langList.Add(dir.Replace(TemplateMapPath,""));
            }
            return langList;
        }

        /// <summary>
        /// gets a full set of templates for a specific langauge, default templates are also returned if no language specific template exists.
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetTemplateFileList(string lang = "Default")
        {
            var templateList = new Dictionary<string, string>();

            //Get all the Default templates
            var d  = GetSpecificTemplateFileList();
            foreach (var f in d)
            {
                templateList.Add(f.Key,f.Value);
            }

            //Get all the langauge Specific templates
            if ((lang != "Default") && (lang != ""))
            {
                d = GetSpecificTemplateFileList(lang);
                foreach (var f in d)
                {
                    if (templateList.ContainsKey(f.Key))
                    {
                        templateList.Remove(f.Key);
                        templateList.Add(f.Key, f.Value);
                    }
                    else
                    {
                        templateList.Add(f.Key, f.Value);
                    }
                }
            }
            return templateList;
        }

        /// <summary>
        /// get list of langauge Specific files.  this function only returns the template files matching the lang passed in the lang param
        /// </summary>
        /// <param name="lang">Culture Code.</param>
        /// <returns></returns>
        public Dictionary<string, string> GetSpecificTemplateFileList(string lang = "Default")
        {
            var templateList = new Dictionary<string, string>();

            //Get all the langauge Specific templates
                var folderPath = string.Format("{0}\\{1}\\", TemplateMapPath.TrimEnd('\\'), lang);
                if (Directory.Exists(folderPath))
                {
                    var langfiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                    foreach (string file in langfiles)
                    {
                        if (Path.GetFileName(file) != null)
                        {
                            var fkey = Path.GetFileName(file);
                            if (fkey != null)
                            {
                                if (templateList.ContainsKey(fkey))
                                {
                                    templateList.Remove(fkey);
                                    templateList.Add(fkey, file);
                                }
                                else
                                {
                                    templateList.Add(fkey, file);
                                }
                            }
                        }
                    }
                }
            return templateList;
        }

        /// <summary>
        /// Get all templates for a list of culture codes
        /// </summary>
        /// <param name="CultureCodeList">Generic list of culture codes</param>
        /// <returns></returns>
        public Dictionary<string, TemplateInfo> GetAllTemplates(List<string> CultureCodeList)
        {
             var templateList = new Dictionary<string, TemplateInfo>();

            foreach (var culturecode in  CultureCodeList)
            {
                var tList2 = GetAllTemplates(culturecode,true);
                foreach (var t in tList2)
                {
                    templateList.Add(t.Key + t.Value.Lang, t.Value);
                }
            }

            return templateList;
        }


        /// <summary>
        /// Get all templates for a culturecode
        /// </summary>
        /// <param name="lang">Culture Code</param>
        /// <param name="LangaugeSpecific">Only return specific langauge templates</param>
        /// <returns></returns>
        public Dictionary<string, TemplateInfo> GetAllTemplates(string lang, bool LangaugeSpecific = false)
        {
            var templateList = new Dictionary<string, TemplateInfo>();

            var l = new Dictionary<string, string>();
            if (LangaugeSpecific)
            {
                l = GetSpecificTemplateFileList(lang);
            }
            else
            {
                l = GetTemplateFileList(lang);                
            }

            foreach (var kvp in l)
            {
                var objTi = new TemplateInfo();
                var objT = new Template(kvp.Value);

                objTi.FullFolderPath = kvp.Value;
                objTi.ThemeFolderPath = string.Format("{0}\\{1}", ThemeFolder, kvp.Value.Replace(TemplateMapPath, ""));
                objTi.ThemeFolderPath = objTi.ThemeFolderPath.Replace("\\" + kvp.Key, "");
                objTi.Lang = lang;
                objTi.Template = objT;

                templateList.Add(kvp.Key, objTi);
            }

            return templateList;
        }

        public string GetTemplateData(string templatename, string lang, bool replaceTemplateTokens = true)
        {
            var objT = GetTemplate(templatename, lang);
            var templateData = objT.TemplateData;
            _isTemplateFound = objT.IsTemplateFound;
            if (replaceTemplateTokens)
            {
                templateData = ReplaceTemplateTokens(templateData, lang);                
            }
            return templateData;
        }

        public Template GetTemplate(string templatename)
        {
            var templatepath = string.Format("{0}\\{1}\\{2}", TemplateMapPath.TrimEnd('\\'), "Default", templatename);
            var objT = new Template(templatepath);
            return objT;
        }

        public Template GetTemplate(string templatename, string lang)
        {
            return GetTemplate(templatename,lang,"Default");
        }

        /// <summary>
        /// Get template from language, System or Portal level 
        /// </summary>
        /// <param name="templatename">Template Name</param>
        /// <param name="lang">language</param>
        /// <param name="themesubfolder">the subfolder of the theme, usually "Default","css","resx","js","img".  But can be anything. </param>
        /// <returns></returns>
        public Template GetTemplate(string templatename, string lang, string themesubfolder)
        {
            var templatepath = string.Format("{0}\\{1}\\{2}", TemplateMapPath.TrimEnd('\\'), lang, templatename);
            if (templatename.StartsWith("/") | templatename.StartsWith("{"))
            {
                templatepath = HttpContext.Current.Server.MapPath(templatename);
                templatepath = templatepath.Replace("{LANG}", lang);
            }
            var objT = new Template(templatepath);
            if (objT.Exists() == false)
            {
                templatepath = string.Format("{0}\\{1}\\{2}", TemplateMapPath.TrimEnd('\\'), themesubfolder, templatename);
                if (templatename.StartsWith("/") | templatename.StartsWith("{"))
                {
                    templatepath = HttpContext.Current.Server.MapPath(templatename);
                    templatepath = templatepath.Replace("{LANG}", themesubfolder);
                }
                objT = new Template(templatepath);
            }
            if (objT.Exists() == false)
            {
                templatepath = string.Format("{0}\\{1}\\{2}", TemplateConfigMapPath.TrimEnd('\\'), themesubfolder, templatename);
                if (templatename.StartsWith("/") | templatename.StartsWith("{"))
                {
                    templatepath = HttpContext.Current.Server.MapPath(templatename);
                    templatepath = templatepath.Replace("{LANG}", themesubfolder);
                }
                objT = new Template(templatepath);
            }
            return objT;
        }

        #endregion

        #region Save

        public void SaveTemplate(string templatename, string lang, string templatedata)
        {
            var langpath = string.Format("{0}\\{1}\\", TemplateMapPath.TrimEnd('\\'), lang);
            if (!Directory.Exists(langpath))
            {
                Directory.CreateDirectory(langpath);
            }
            var templatepath = string.Format("{0}\\{1}\\{2}", TemplateMapPath.TrimEnd('\\'), lang, templatename);
            var objT = new Template(templatepath);
            objT.Save(templatedata);
        }

        public void SaveTemplate(string templatename, string templatedata)
        {
            SaveTemplate(templatename, "Default", templatedata);
        }


        #endregion

        #region Delete

        public void DeleteTheme()
        {
            if (Directory.Exists(TemplateMapPath))
            {
                Directory.Delete(TemplateMapPath, true);
            }
        }

        public void DeleteLangaugeFolder(string lang)
        {
            var dirpath = string.Format("{0}\\{1}", TemplateMapPath, lang);
            if (Directory.Exists(dirpath)) 
            {
                Directory.Delete(dirpath, true);
            }
        }

        public void DeleteTemplate(string templatename, string lang)
        {
            var templatepath = string.Format("{0}\\{1}\\{2}", TemplateMapPath, lang, templatename);
            var objT = new Template(templatepath);
            objT.Delete();
        }

        public void DeleteTemplate(string templatename)
        {
            DeleteTemplate(templatename,"Default");
        }

        #endregion

        #region Export

        public string ExportThemeXml()
        {
            var langlist = GetLangaugeFolders();
            var outXml = "<theme>";
            outXml += "<name>";
            outXml += ThemeFolder;
            outXml += "</name>";
            outXml += "<lang>";

            foreach (var lg in langlist)
            {
                outXml += ExportXml(lg);
            }

            outXml += "</lang>";
            outXml += "</theme>";

            return outXml;
        }


        public string ExportXml(string lang)
        {
            var templatelist = GetAllTemplates(lang);
            var outXml = "<root>";
            outXml += "<lang>";
            outXml += lang;
            outXml += "</lang>";
            outXml += "<templates>";

            foreach (var kvp in templatelist)
            {
                outXml += GetTemplateInfoXml(kvp.Value);
            }

            outXml += "</templates>";
            outXml += "</root>";

            return outXml;
        }

        public string GetTemplateInfoXml(TemplateInfo templateinfo)
        {
            var outXml = "";

            outXml += "<template>";
            outXml += "<name>";
            outXml += templateinfo.Template.TemplateName;
            outXml += "</name>";
            outXml += "<themefolderpath>";
            outXml += templateinfo.ThemeFolderPath;
            outXml += "</themefolderpath>";
            outXml += "<data><![CDATA[";
            outXml += templateinfo.Template.TemplateData;
            outXml += "]]></data>";
            outXml += "</template>";

            return outXml;
        }

        #endregion

        #region Import


        public void ImportThemeXml(string xmldata, bool overwrite)
        {
            var xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xmldata);

                var xmlNodList = xmlDoc.SelectNodes("theme/lang/*");

                if (xmlNodList != null)
                {
                    foreach (XmlNode xmlNod in xmlNodList)
                    {
                        ImportXml(xmlNod.OuterXml,overwrite);
                    }
                }
        }

        public void ImportXml(string xmldata, bool overwrite)
        {
            var xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xmldata);

            var xmllang = xmlDoc.SelectSingleNode("root/lang");
            if (xmllang != null)
            {

                var xmlNodList = xmlDoc.SelectNodes("root/templates/*");

                if (xmlNodList != null)
                {

                    SetupThemeFolders("Default");

                    foreach (XmlNode xmlNod in xmlNodList)
                    {
                        var xmltemplatedata = xmlNod.SelectSingleNode("./data");
                        var xmlthemefolderpath = xmlNod.SelectSingleNode("./themefolderpath");
                        var xmlname = xmlNod.SelectSingleNode("./name");
                        if (xmlname != null && xmltemplatedata != null && xmlthemefolderpath != null)
                        {
                            SetupThemeFolders(xmlthemefolderpath.InnerText);
                            var template = new Template(string.Format("{0}\\{1}", HomeMapPath, xmlthemefolderpath.InnerText) , xmlname.InnerText);
                            if (overwrite)
                            {
                                template.Save(xmltemplatedata.InnerText);
                            }
                            else
                            {
                                if (template.Exists() == false )
                                {
                                    template.Save(xmltemplatedata.InnerText);                                    
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region "General Methods"


        public string ReplaceTemplateTokens(string templText, string lang, int recursiveCount = 0)
        {
            var strOut = templText;
            var blnTemplateFound = false;
            var aryT = Utils.ParseTemplateText(templText);
            if ((aryT.Length > 0) && (recursiveCount < 5))
            {
                for (var lp = 0; lp <= aryT.GetUpperBound(0); lp++)
                {
                    if ((aryT[lp] != null))
                    {
                        var htmlDecode = System.Web.HttpUtility.HtmlDecode(aryT[lp]);
                        if (htmlDecode != null && htmlDecode.ToLower().StartsWith("template:"))
                        {
                            var newTemplateText = "";
                            var tRoles = "";
                            var strSplit = aryT[lp].Split(':');
                            var tName = strSplit[1];
                            if (strSplit.Length == 3)
                            {
                                tRoles = strSplit[2];
                            }

                            if ((tRoles == "") | providers.CmsProviderManager.Default.IsInRole(tRoles))
                            {
                                newTemplateText = GetTemplateData(tName,lang);
                                //newTemplateText = System.Web.HttpUtility.HtmlDecode(newTemplateText);
                                if (_isTemplateFound)
                                {
                                    strOut = strOut.Replace("[" + aryT[lp] + "]", newTemplateText);
                                    blnTemplateFound = true;
                                }
                            }
                        }
                    }
                }
            }
            if (blnTemplateFound)
            {
                strOut = ReplaceTemplateTokens(strOut, lang, recursiveCount + 1);
            }
            return strOut;
        }

        public string ReplaceResourceString(string templText)
        {
            var sValues = new Dictionary<String, String>();

            var aryT = Utils.ParseTemplateText(templText);
            if ((aryT.Length > 0))
            {
                for (var lp = 0; lp <= aryT.GetUpperBound(0); lp++)
                {
                    if ((aryT[lp] != null))
                    {
                        var htmlDecode = System.Web.HttpUtility.HtmlDecode(aryT[lp]);
                        if (htmlDecode != null && htmlDecode.ToLower().StartsWith("string:"))
                        {
                            var strSplit = aryT[lp].Split(':');
                            var tName = strSplit[1];
                            if (strSplit.Length == 3)
                            {
								if (!sValues.ContainsKey(strSplit[1]))
								{
									sValues.Add(strSplit[1], strSplit[2]);
								}
								templText = templText.Replace("[" + aryT[lp] + "]", "");
							}
                        }
                    }
                }

                if (sValues.Count > 0)
                {
                    foreach (var kvp in sValues)
                    {
                        templText = templText.Replace("{" + kvp.Key + "}", kvp.Value);
                    }
                }
            }

            return templText;
        }


        #endregion

    }
}
