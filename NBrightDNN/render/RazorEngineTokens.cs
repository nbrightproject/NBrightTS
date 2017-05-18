using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using NBrightCore.common;
using NBrightCore.providers;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace NBrightDNN.render
{
    public class RazorEngineTokens<T> : TemplateBase<T>
    {
        public Dictionary<String,List<String>> Metadata;

        public RazorEngineTokens()
        {
            Metadata = new Dictionary<String, List<String>>();
        }

        #region "token to add meta data for tokens"

        public IEncodedString AddMetaData(String metaType, String metaValue)
        {
            var l = new List<String>();
            if (Metadata.ContainsKey(metaType)) l = Metadata[metaType];                
            l.Add(metaValue);

            if (Metadata.ContainsKey(metaType))
                Metadata[metaType] = l;
            else
                Metadata.Add(metaType,l);

            return new RawString(""); //return nothing
        }

        public IEncodedString AddMetaData(String metaKey, String metaValue, String templateFullName)
        {
            // if we have a templatename add to preprocess cache meta data.
            return AddPreProcessMetaData(metaKey, metaValue, templateFullName,"");
        }

        /// <summary>
        /// This method add the meta data to a specific cache list, so the we can use that data in the module code, before the razor template is rendered.
        /// This allows use to use the metadata token to add data selection information, like search filters and sort before we get the data from the DB.
        /// </summary>
        /// <param name="metaKey"></param>
        /// <param name="metaValue"></param>
        /// <param name="templateFullName">This is the cache key that is used and MUST be {theme}.{templatename}.{templateExtension}  e.g. Classic.list.cshtml</param>
        /// <param name="moduleId">moduleid to identify individual modules (required for filters)</param>
        /// <returns></returns>
        public IEncodedString AddPreProcessMetaData(String metaKey, String metaValue,String templateFullName,String moduleId)
        {
            var cachedlist = (Dictionary<String, String>)Utils.GetCache("preprocessmetadata" + templateFullName + moduleId);
            if (cachedlist == null)
            {
                cachedlist = new Dictionary<String, String>();
            }

            if (cachedlist.ContainsKey(metaKey))
                cachedlist[metaKey] = metaValue;
            else
                cachedlist.Add(metaKey, metaValue);

            Utils.SetCache("preprocessmetadata" + templateFullName + moduleId, cachedlist);

            // add to internal metadata, so we can use it in the razor template if needed.
            return AddMetaData(metaKey, metaValue);
        }

        #endregion

        #region "general html control tokens"

        public IEncodedString HiddenField(NBrightInfo info, String xpath, String attributes = "", String defaultValue = "")
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();
            if (defaultValue.StartsWith("ResourceKey:")) defaultValue = ResourceKey(defaultValue.Replace("ResourceKey:", "")).ToString();

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            var value = info.GetXmlProperty(xpath);
            if (value == "") value = defaultValue;
            var strOut = "<input value='" + value + "' id='" + id + "' " + attributes + " " + upd + " type='hidden' />";

            return new RawString(strOut);
        }

        public IEncodedString TextBox(NBrightInfo info, String xpath, String attributes = "", String defaultValue = "")
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();
            if (defaultValue.StartsWith("ResourceKey:")) defaultValue = ResourceKey(defaultValue.Replace("ResourceKey:", "")).ToString();

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            var value = info.GetXmlProperty(xpath);
            if (value == "") value = defaultValue;

            var typeattr = "type='text'";
            if (attributes.ToLower().Contains(" type=")) typeattr = "";

            var strOut = "<input value='" + value.Replace("'", "&#39;") + "' id='" + id + "' " + attributes + " " + upd + " " + typeattr + " />";

            return new RawString(strOut);
        }

        public IEncodedString NBrightTextBox(NBrightInfo info, String xpath, String attributes = "", String defaultValue = "")
        {
            attributes = attributes + " class='form-control' ";
            var strOut = TextBox(info, xpath, attributes, defaultValue).ToString();
            if (xpath.Contains("genxml/lang/"))
            {
                strOut = "<div class='input-group'>" + strOut + "<span class='input-group-addon'><img src='/Images/Flags/" + info.Lang + ".gif' width='24px'/></span></div>";
            }
            return new RawString(strOut);
        }

        public IEncodedString NBrightDate(NBrightInfo info, String xpath, String attributes = "", String defaultValue = "",String extraDatePickerOptions = "")
        {
            attributes = attributes + " class='form-control' datatype='date' ";
            var strOut = TextBox(info, xpath, attributes, defaultValue).ToString();
            if (xpath.Contains("genxml/lang/"))
            {
                strOut = "<div class='input-group'><div class='input-group-addon'><span class='glyphicon glyphicon-th'></span></div>" + strOut + "<span class='input-group-addon'><img src='/Images/Flags/" + info.Lang + ".gif' width='24px'/></span></div>";
            }
            else
            {
                strOut = "<div class='input-group'><div class='input-group-addon'><span class='glyphicon glyphicon-th'></span></div>" + strOut + "</div>";
            }
            var id = getIdFromXpath(xpath);
            var currentculture = Utils.GetCurrentCulture();
            var datelang = "en";
            var localcodelist = "en-GB,fr-CH,it-CH,nl-BE,pt-BR,zh-CN,zh-TW";
            if (localcodelist.Contains(currentculture))
            {
                datelang = currentculture;
            }
            else
            {
                datelang = currentculture.Substring(0, 2).ToLower();
            }
            if (!extraDatePickerOptions.Contains("autoclose: true,")) extraDatePickerOptions = extraDatePickerOptions.Trim(',') + ",autoclose: true";
            if (!extraDatePickerOptions.StartsWith(",")) extraDatePickerOptions = "," + extraDatePickerOptions;
            strOut += "<script>$('#" + id + "').datepicker({language: '" + datelang + "'"+ extraDatePickerOptions + "});</script>";
            return new RawString(strOut);
        }

        public IEncodedString TextArea(NBrightInfo info, String xpath, String attributes = "", String defaultValue = "")
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();
            if (defaultValue.StartsWith("ResourceKey:")) defaultValue = ResourceKey(defaultValue.Replace("ResourceKey:", "")).ToString();

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            var value = info.GetXmlProperty(xpath);
            if (value == "") value = defaultValue;
            var strOut = "<textarea id='" + id + "' " + attributes + " " + upd + " type='text'>" + value + "</textarea>";

            return new RawString(strOut);
        }

        public IEncodedString NBrightTextArea(NBrightInfo info, String xpath, String attributes = "", String defaultValue = "")
        {
            attributes = attributes + " class='form-control' ";
            var strOut = TextArea(info, xpath, attributes, defaultValue).ToString();
            if (xpath.Contains("genxml/lang/"))
            {
                strOut = "<div class='input-group'>" + strOut + "<span class='input-group-addon'><img src='/Images/Flags/" + info.Lang + ".gif' width='24px'/></span></div>";
            }
            return new RawString(strOut);
        }

        /// <summary>
        /// Display richText CKEditor for eding
        /// NOTE: Data is sent back tothe server via a temp field.  This is populated by injected JS script by this token.
        /// Validate triggers are: #savedata, .selecteditlanguage, .flieclick, .imageclick, .saverichtextdata
        /// If the template savebutton or save event does NOT match any of these trrigegrs, use ".saverichtextdata" by adding it as a class onto the save button.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="xpath"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        [Obsolete("RichTextBox is deprecated (can cause race condition), please use CkEditor instead.")]
        public IEncodedString RichTextBox(NBrightInfo info, String xpath, String attributes = "")
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            var strOut = " <textarea id='" + id + "' datatype='html' type='text' name='editor" + id + "' " + attributes + " " + upd + " >" + info.GetXmlProperty(xpath) + "</textarea>";
            strOut += "<script> var editorvar" + id + " = '';  $(document).ready(function () { editorvar" + id + " = CKEDITOR.replace('editor" + id + "', { customConfig: '/DesktopModules/NBright/NBrightData/ckeditor/nbrightconfig.js' } ); $('#savedata').click(function () { var value = editorvar" + id + ".getData(); $('#" + id + "').val(value);}); $('.selecteditlanguage').click(function () { var value = editorvar" + id + ".getData(); $('#" + id + "').val(value);});  $('.fileclick').click(function () { var value = editorvar" + id + ".getData(); $('#" + id + "').val(value);});  $('.imageclick').click(function () { var value = editorvar" + id + ".getData(); $('#" + id + "').val(value);});   });</script>";
            return new RawString(strOut);
        }

        /// <summary>
        /// Display richText CKEditor for eding
        /// NOTE: Data is sent back tothe server via a temp field.  This is populated by change event on the CKEDITOR.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="xpath"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public IEncodedString CkEditor(NBrightInfo info, String xpath, String attributes = "")
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            var strOut = " <textarea id='" + id + "' datatype='html' type='text' name='editor" + id + "' " + attributes + " " + upd + " >" + info.GetXmlProperty(xpath) + "</textarea>";
            strOut += "<script> var editorvar" + id + " = '';  $(document).ready(function () { editorvar" + id + " = CKEDITOR.replace('editor" + id + "', { customConfig: '/DesktopModules/NBright/NBrightData/ckeditor/nbrightconfig.js' } ); editorvar" + id + ".on('change', function (event) { var value = editorvar" + id + ".getData(); $('#" + id + "').val(value); }); });</script>";
            return new RawString(strOut);
        }
        public IEncodedString CkEditorFull(NBrightInfo info, String xpath, String attributes = "")
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            var strOut = " <textarea id='" + id + "' datatype='html' type='text' name='editor" + id + "' " + attributes + " " + upd + " >" + info.GetXmlProperty(xpath) + "</textarea>";
            strOut += "<script> var editorvar" + id + " = '';  $(document).ready(function () { editorvar" + id + " = CKEDITOR.replace('editor" + id + "', { customConfig: '/DesktopModules/NBright/NBrightData/ckeditor/none.js' } ); editorvar" + id + ".on('change', function (event) { var value = editorvar" + id + ".getData(); $('#" + id + "').val(value); }); });</script>";
            return new RawString(strOut);
        }

        public IEncodedString CheckBox(NBrightInfo info, String xpath,String text, String attributes = "", Boolean defaultValue = false)
        {
            if (text.StartsWith("ResourceKey:")) text = ResourceKey(text.Replace("ResourceKey:", "")).ToString();
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            var strOut = "    <input id='" + id + "' type='checkbox' " + getChecked(info, xpath, defaultValue) + " " + attributes + " " + upd + " /><label>" + text + "</label>";
            return new RawString(strOut);
        }

        public IEncodedString CheckBoxList(NBrightInfo info, String xpath, String datavalue, String datatext, String attributes = "", Boolean defaultValue = false)
        {
            if (datavalue.StartsWith("ResourceKey:")) datavalue = ResourceKey(datavalue.Replace("ResourceKey:", "")).ToString();
            if (datatext.StartsWith("ResourceKey:")) datatext = ResourceKey(datatext.Replace("ResourceKey:", "")).ToString();
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var strOut = "";

            var datat = datatext.Split(',');
            if (datavalue == "")
            {
                var lp = 1;
                foreach (var v in datat)
                {
                    datavalue += lp.ToString() + ",";
                    lp += 1;
                }
                datavalue = datavalue.TrimEnd(',');
            }
                var datav = datavalue.Split(',');
            if (datav.Count() == datat.Count())
            {
                var upd = getUpdateAttr(xpath, attributes);
                var id = getIdFromXpath(xpath);
                strOut = "<div id='" + id + "' " + upd + " " + attributes + ">";
                var c = 0;
                foreach (var v in datav)
                {
                    strOut += "    <input id='" + id + "_" + c.ToString("") + "' name='" + id + "$" + c.ToString("") + "' type='checkbox' value='" + v + "' " + getChecked(info, xpath + "/chk[@data='" + v + "']/@value", defaultValue) + " /><label for='" + id + "_" + c.ToString("") + "'>" + datat[c] + "</label>";
                    c += 1;
                }
                strOut += "</div>";
            }
            return new RawString(strOut);
        }

        public IEncodedString RadioButtonList(NBrightInfo info, String xpath, String datavalue, String datatext, String attributes = "", String defaultValue = "",String labelattributes = "")
        {
            if (datavalue.StartsWith("ResourceKey:")) datavalue = ResourceKey(datavalue.Replace("ResourceKey:", "")).ToString();
            if (datatext.StartsWith("ResourceKey:")) datatext = ResourceKey(datatext.Replace("ResourceKey:", "")).ToString();
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();
            if (defaultValue.StartsWith("ResourceKey:")) defaultValue = ResourceKey(defaultValue.Replace("ResourceKey:", "")).ToString();

            var strOut = "";
            var datav = datavalue.Split(',');
            var datat = datatext.Split(',');
            if (datav.Count() == datat.Count())
            {
                var upd = getUpdateAttr(xpath, attributes);
                var id = getIdFromXpath(xpath); 
                strOut = "<div " + attributes + ">";
                var c = 0;
                var s = "";
                var value = info.GetXmlProperty(xpath);
                if (value == "") value = defaultValue; 
                foreach (var v in datav)
                {
                    if (value == v)
                        s = "checked";
                    else
                        s = "";
                    strOut += "    <label " + labelattributes + "><input id='" + id + "_" + c.ToString("") + "' " + upd + " name='" + id + "radio' type='radio' value='" + v + "'  " + s + "/>" + datat[c] + "</label>";
                    c += 1;
                }
                strOut += "</div>";
            }
            return new RawString(strOut);
        }

        public IEncodedString DropDownList(NBrightInfo info, String xpath, String datavalue, String datatext, String attributes = "", String defaultValue = "")
        {
            if (datavalue.StartsWith("ResourceKey:")) datavalue = ResourceKey(datavalue.Replace("ResourceKey:", "")).ToString();
            if (datatext.StartsWith("ResourceKey:")) datatext = ResourceKey(datatext.Replace("ResourceKey:", "")).ToString();
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();
            if (defaultValue.StartsWith("ResourceKey:")) defaultValue = ResourceKey(defaultValue.Replace("ResourceKey:", "")).ToString();

            var strOut = "";
            var datav = datavalue.Split(',');
            var datat = datatext.Split(',');
            if (datav.Count() == datat.Count())
            {
                var upd = getUpdateAttr(xpath,attributes);
                var id = getIdFromXpath(xpath);
                strOut = "<select id='" + id + "' " + upd + " " + attributes + ">";
                var c = 0;
                var s = "";
                var value = info.GetXmlProperty(xpath);
                if (value == "") value = defaultValue;
                foreach (var v in datav)
                {
                    if (value == v)
                        s = "selected";
                    else
                        s = "";

                    strOut += "    <option value='" + v + "' " + s + ">" + datat[c] + "</option>";
                    c += 1;
                }
                strOut += "</select>";
            }
            return new RawString(strOut);
        }

        #endregion

        #region DNN specific tokens

        public IEncodedString TabSelectList(NBrightInfo info, String xpath, String attributes = "",Boolean allowEmpty = true)
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var tList = DnnUtils.GetTreeTabListOnUniqueId();
            var strOut = "";

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            strOut = "<select id='" + id + "' " + upd + " guidkey='tab' " + attributes + ">";
            var c = 0;
            var s = "";
            if (allowEmpty) strOut += "    <option value=''></option>";                
            foreach (var tItem in tList)
            {
                if (info.GetXmlProperty(xpath) == tItem.Key.ToString())
                    s = "selected";
                else
                    s = "";
                strOut += "    <option value='" + tItem.Key.ToString() + "' " + s + ">" + tItem.Value + "</option>";
            }
            strOut += "</select>";

            return new RawString(strOut);
        }

        /// <summary>
        /// Dropdown list of tabs, using TabId as return value
        /// </summary>
        /// <param name="info"></param>
        /// <param name="xpath"></param>
        /// <param name="attributes"></param>
        /// <param name="allowEmpty"></param>
        /// <returns></returns>
        public IEncodedString TabSelectListOnTabId(NBrightInfo info, String xpath, String attributes = "", Boolean allowEmpty = true)
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var tList = DnnUtils.GetTreeTabListOnTabId();
            var strOut = "";

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            strOut = "<select id='" + id + "' " + upd + " guidkey='tab' " + attributes + ">";
            var c = 0;
            var s = "";
            if (allowEmpty) strOut += "    <option value=''></option>";
            foreach (var tItem in tList)
            {
                if (info.GetXmlProperty(xpath) == tItem.Key.ToString())
                    s = "selected";
                else
                    s = "";
                strOut += "    <option value='" + tItem.Key.ToString() + "' " + s + ">" + tItem.Value + "</option>";
            }
            strOut += "</select>";

            return new RawString(strOut);
        }

        public IEncodedString TreeViewTabsFancyTree()
        {

            var strOut = DnnUtils.GetTreeViewTabJSData();

            return new RawString(strOut);
        }


        public IEncodedString DnnLabel(String id, String resourceFileKey, String lang = "")
        {
            var strOut = new StringBuilder("<div class='dnnLabel'>");
            strOut.Append("<label><span>" + ResourceKey(resourceFileKey, lang) + "</span> </label>");
            var msg = ResourceKey(resourceFileKey, lang, "Help").ToString();
            if (msg == "") msg = ResourceKey(resourceFileKey, lang, "HelpText").ToString();
            if (msg != "")
            {
                strOut.Append("<a id='" + id + "_cmdHelp' tabindex='-1' class='dnnFormHelp' href='javascript:void();'></a>");
                strOut.Append("<div id='" + id + "_pnlHelp' class='dnnTooltip'><div class='dnnFormHelpContent dnnClear'>");
                strOut.Append("<span id='" + id + "_lblHelp' class='dnnHelpText'>" + msg + "</span>");
                strOut.Append("<a href='#' class='pinHelp'></a>");
                strOut.Append("</div></div>");                
            }
            strOut.Append("</div>");

            return new RawString(strOut.ToString());
        }

        public IEncodedString GetTabUrlByGuid(String tabguid)
        {
            var strOut = "";

            var t = (from kvp in TabController.GetTabsBySortOrder(PortalSettings.Current.PortalId) where kvp.UniqueId.ToString() == tabguid select kvp.TabID);
            if (t.Any())
            {
                var tabid = t.First();
                strOut = Globals.NavigateURL(tabid);
            }

            return new RawString(strOut);
        }

        public IEncodedString GetTabUrlByGuid(NBrightInfo info, String xpath)
        {
            var strOut = "";

            var t = (from kvp in TabController.GetTabsBySortOrder(PortalSettings.Current.PortalId) where kvp.UniqueId.ToString() == info.GetXmlProperty(xpath) select kvp.TabID);
            if (t.Any())
            {
                var tabid = t.First();
                strOut = Globals.NavigateURL(tabid);
            }

            return new RawString(strOut);
        }

        public IEncodedString ResourceKey(String resourceFileKey, String lang = "",String resourceExtension = "Text")
        {
            return new RawString(ResourceKeyString(resourceFileKey, lang , resourceExtension));
        }

        public IEncodedString ResourceKeyJS(String resourceFileKey, String lang = "", String resourceExtension = "Text")
        {
            var strOut = ResourceKeyString(resourceFileKey, lang, resourceExtension);
            strOut = strOut.Replace("'", "\\'");
            return new RawString(strOut);
        }

        private string ResourceKeyString(String resourceFileKey, String lang = "", String resourceExtension = "Text")
        {
            if (lang == "") lang = Utils.GetCurrentCulture();
            var strOut = "";
            if (Metadata.ContainsKey("resourcepath"))
            {
                var l = Metadata["resourcepath"];
                foreach (var r in l)
                {
                    strOut = DnnUtils.GetResourceString(r, resourceFileKey, resourceExtension, lang);
                    if (strOut != "") break;
                }
            }
            return strOut;
        }

        #endregion

        #region "extra tokens"

        public IEncodedString EditCultureSelect(String cssclass, String cssclassli, Boolean addDefault = false)
        {
            var enabledlanguages = LocaleController.Instance.GetLocales(PortalSettings.Current.PortalId);
            var strOut = new StringBuilder("<ul class='" + cssclass + "'>");

            if (addDefault)
            {
                strOut.Append("<li>");
                strOut.Append("<a href='javascript:void(0)' lang='' class='selecteditlanguage " + cssclassli + "'><img src='/Images/Flags/None.gif' alt='default' /></a>");
                strOut.Append("</li>");
            }

            foreach (var l in enabledlanguages)
            {
                strOut.Append("<li>");
                strOut.Append("<a href='javascript:void(0)' lang='" + l.Value.Code + "' class='selecteditlanguage " + cssclassli + "'><img src='/Images/Flags/" + l.Value.Code + ".gif' alt='" + l.Value.NativeName + "' /></a>");
                strOut.Append("</li>");
            }
            strOut.Append("</ul>");
            return new RawString(strOut.ToString());
        }


        public IEncodedString WebsiteUrl(String parameters = "")
        {
            var strOut = "";
            var ps = DnnUtils.GetCurrentPortalSettings();
            var strAry = ps.DefaultPortalAlias.Split('/');
            if (strAry.Any())
                strOut = strAry[0]; // Only display base domain, without lanaguge
            else
                strOut = ps.DefaultPortalAlias;
            if (strOut == "")
            {
                strAry = ps.PortalAlias.HTTPAlias.Split('/');
                if (strAry.Any())
                    strOut = strAry[0]; // Only display base domain, without lanaguge
                else
                    strOut = ps.DefaultPortalAlias;
            }
            if (parameters != "") strOut += "?" + parameters;
            return new RawString(strOut);
        }

        public IEncodedString EmailOf(NBrightInfo info, String xpath)
        {
            var strOut = info.GetXmlProperty(xpath);
            strOut = Utils.FormatAsMailTo(strOut);
            return new RawString(strOut);
        }

        public IEncodedString HtmlOf(NBrightInfo info, String xpath)
        {
            var strOut = info.GetXmlProperty(xpath);
            strOut = System.Web.HttpUtility.HtmlDecode(strOut);
            return new RawString(strOut);
        }

        public IEncodedString HtmlOf(String htmlString)
        {
            return new RawString(System.Web.HttpUtility.HtmlDecode(htmlString));
        }

        public IEncodedString BreakOf(NBrightInfo info, String xpath)
        {
            var strOut = info.GetXmlProperty(xpath);
            return BreakOf(strOut);
        }

        public IEncodedString BreakOf(IEncodedString strIn)
        {
            return BreakOf(strIn.ToString());
        }

        public IEncodedString BreakOf(String strIn)
        {
            var strOut = System.Web.HttpUtility.HtmlEncode(strIn);
            strOut = strOut.Replace(Environment.NewLine, "<br/>");
            strOut = strOut.Replace("\t", "&nbsp;&nbsp;&nbsp;");
            strOut = strOut.Replace("'", "&apos;");
            return new RawString(strOut);
        }

        public IEncodedString HeadingOf(String text, String headerstyle)
        {
            var headingstylestart = "<" + headerstyle + ">";
            var headingstyleend = "</" + headerstyle + ">";
            var strOut = headingstylestart + text + headingstyleend;
            return new RawString(strOut);
        }

        public IEncodedString CheckBoxListOf(NBrightInfo info, String xpath, String datavalue, String datatext, String attributes = "")
        {
            if (datavalue.StartsWith("ResourceKey:")) datavalue = ResourceKey(datavalue.Replace("ResourceKey:", "")).ToString();
            if (datatext.StartsWith("ResourceKey:")) datatext = ResourceKey(datatext.Replace("ResourceKey:", "")).ToString();
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var strOut = "";
            var datav = datavalue.Split(',');
            var datat = datatext.Split(',');
            if (datav.Count() == datat.Count())
            {
                strOut = "<ul " + attributes + ">";
                var c = 0;
                foreach (var v in datav)
                {
                    if (info.GetXmlProperty(xpath + "/chk[@data='" + v + "']/@value") == "True") strOut += "    <li>" + datat[c] + "</li>";
                    c += 1;
                }
                strOut += "</ul>";
            }
            return new RawString(strOut.ToString());
        }

        public IEncodedString FileSelectList(string selectedfilename, String relitiveRootFolder, String attributes = "", Boolean allowEmpty = true)
        {
            var nbi = new NBrightInfo(true);
            nbi.SetXmlProperty("genxml/selectedfilename", selectedfilename);
            return FileSelectList(nbi, "genxml/selectedfilename", relitiveRootFolder, attributes, allowEmpty);
        }

        public IEncodedString FileSelectList(NBrightInfo info, String xpath, String relitiveRootFolder, String attributes = "", Boolean allowEmpty = true)
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var mappathRootFolder = System.Web.Hosting.HostingEnvironment.MapPath(relitiveRootFolder);
            var dirlist = System.IO.Directory.GetFiles(mappathRootFolder);
            var tList = new List<String>();
            foreach (var d in dirlist)
            {
                var dr = new System.IO.FileInfo(d);
                tList.Add(dr.Name);
            }
            var strOut = "";

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            strOut = "<select id='" + id + "' " + upd + " " + attributes + ">";
            var c = 0;
            var s = "";
            if (allowEmpty) strOut += "    <option value=''></option>";
            foreach (var tItem in tList)
            {
                if (info.GetXmlProperty(xpath) == tItem)
                    s = "selected";
                else
                    s = "";
                strOut += "    <option value='" + tItem + "' " + s + ">" + tItem + "</option>";
            }
            strOut += "</select>";

            return new RawString(strOut);
        }


        public IEncodedString FolderSelectList(NBrightInfo info, String xpath, String relitiveRootFolder, String attributes = "", Boolean allowEmpty = true)
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var mappathRootFolder = System.Web.Hosting.HostingEnvironment.MapPath(relitiveRootFolder);
            var dirlist = System.IO.Directory.GetDirectories(mappathRootFolder);
            var tList = new List<String>();
            foreach (var d in dirlist)
            {
                var dr = new System.IO.DirectoryInfo(d);
                tList.Add(dr.Name);
            }
            var strOut = "";

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            strOut = "<select id='" + id + "' " + upd + " " + attributes + ">";
            var c = 0;
            var s = "";
            if (allowEmpty) strOut += "    <option value=''></option>";
            foreach (var tItem in tList)
            {
                if (info.GetXmlProperty(xpath) == tItem)
                    s = "selected";
                else
                    s = "";
                strOut += "    <option value='" + tItem + "' " + s + ">" + tItem + "</option>";
            }
            strOut += "</select>";

            return new RawString(strOut);
        }


        public IEncodedString RenderTemplate(String templateRelPath, NBrightRazor model)
        {
            var TemplateData = "";
            var strOut = "";
            var templatePath = HttpContext.Current.Server.MapPath(templateRelPath);
            if (File.Exists(templatePath))
            {
                string inputLine;
                var inputStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
                var streamReader = new StreamReader(inputStream);

                while ((inputLine = streamReader.ReadLine()) != null)
                {
                    TemplateData += inputLine + Environment.NewLine;
                }
                streamReader.Close();
                inputStream.Close();

                strOut = RazorUtils.RazorRender(model, TemplateData, templatePath, false);

            }

            return new RawString(strOut);
        }

        #endregion


        #region functions


        public String getIdFromXpath(String xpath)
        {
            if (xpath == "") return "";

            // if we only have 1 "genxml" in the xpath, the id is the last element in the array.
            if (xpath.LastIndexOf("genxml") == 0) return xpath.Split('/').Last();

            // if we have multiple "genxml", we need to build an id based on the xpath structure
            var rtnid = xpath.Replace("genxml/textbox/","");
            rtnid = rtnid.Replace("genxml/dropdownlist/", "");
            rtnid = rtnid.Replace("genxml/hidden/", "");
            rtnid = rtnid.Replace("genxml/checkbox/", "");
            rtnid = rtnid.Replace("genxml/checkboxlist/", "");
            rtnid = rtnid.Replace("genxml/radiobuttonlist/", "");
            rtnid = rtnid.Replace("genxml/", "");
            rtnid = rtnid.Replace("/", "_");
            return rtnid;
        }

        public String getUpdateAttr(String xpath,String attributes)
        {
            if (xpath == "") return "";
            var upd = "update='save'";
            if (xpath.StartsWith("genxml/lang/")) upd = "update='lang'";
            if (attributes.Contains("update=")) upd = "";
            return upd;
        }

        public String getChecked(NBrightInfo info, String xpath, Boolean defaultValue)
        {
            if (info.GetXmlProperty(xpath) == "True") return "checked='True'";
            if (info.GetXmlProperty(xpath) == "")
            {
                if (defaultValue) return "checked='True'";                
            }            
            return "";
        }

        #endregion


    }


}
