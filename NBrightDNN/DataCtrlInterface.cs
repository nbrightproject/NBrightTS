using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN.controls;
using RazorEngine.Text;

namespace NBrightDNN
{
    public abstract class NBrightDataCtrlInterface
    {
        public abstract List<NBrightInfo> GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string lang = "");
        public abstract int GetListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "");
        public abstract NBrightInfo Get(int itemId, string lang = "");
        public abstract NBrightInfo GetData(int itemId);
        public abstract int Update(NBrightInfo objInfo);
        public abstract void Delete(int itemId);
        public abstract void CleanData();
    }

    public abstract class DataCtrlInterface
    {
        public abstract List<NBrightInfo> GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string typeCodeLang = "", string lang = "");
        public abstract int GetListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string typeCodeLang = "", string lang = "");
        public abstract NBrightInfo Get(int itemId, string typeCodeLang = "", string lang = "");
        public abstract int Update(NBrightInfo objInfo);
        public abstract void Delete(int itemId);
        public abstract void CleanData();
    }

    public class NBrightInfo : ICloneable
    {
        public int ItemID { get; set; }
        public int PortalId { get; set; }
        public int ModuleId { get; set; }
        public string TypeCode { get; set; }
        public string GUIDKey { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string TextData { get; set; }
        public int XrefItemId { get; set; }
        public int ParentItemId { get; set; }
        public XmlDocument XMLDoc { get; set; }
        public string Lang { get; set; }
        public int UserId { get; set; }
        public int RowCount { get; set; }

        private string _xmlData;

        public NBrightInfo()
        {
        }

        /// <summary>
        /// Create new NBrightInfo class for generic XML data in NBright
        /// </summary>
        /// <param name="populate">Craete the basiuc XML strucutre</param>
        public NBrightInfo(Boolean populate)
        {
            if (populate)
            {
                XMLData = GenXmlFunctions.GetGenXml(new RepeaterItem(0, ListItemType.Item));
            }
        }

        public string XMLData
        {
            get { return _xmlData; }
            set
            {
                XMLDoc = null;
                _xmlData = value;
                try
                {
                    if (!String.IsNullOrEmpty(_xmlData))
                    {
                        XMLDoc = new XmlDocument();
                        XMLDoc.LoadXml(_xmlData);
                    }
                }
                catch (Exception)
                {
                    //trap erorr and don't report. (The XML might be invalid, but we don;t want to stop processing here.)
                    XMLDoc = null;
                }
            }
        }

        public string GetXmlNode(string xpath)
        {
            if (!string.IsNullOrEmpty(_xmlData) & XMLDoc != null)
            {
                try
                {
                    var selectSingleNode = XMLDoc.SelectSingleNode(xpath);
                    if (selectSingleNode != null) return selectSingleNode.InnerXml;
                }
                catch (Exception ex)
                {
                    return "XML READ ERROR";
                }
            }
            return "";
        }

        public void RemoveXmlNode(string xPath)
        {
            var xmlNod = XMLDoc.SelectSingleNode(xPath);
            if (xmlNod != null)
            {
                if (xmlNod.ParentNode != null) xmlNod.ParentNode.RemoveChild(xmlNod);
            }
            XMLData = XMLDoc.OuterXml;
        }

        /// <summary>
        /// Add single node to XML 
        /// </summary>
        /// <param name="nodeName">Node Name</param>
        /// <param name="nodeValue">Value of Node</param>
        /// <param name="xPathRootDestination">xpath of parent location to enter the node</param>
        public void AddSingleNode(string nodeName, string nodeValue, string xPathRootDestination)
        {
            var cdataStart = "<![CDATA[";
            var cdataEnd = "]]>";
            if (IsValidXmlString(nodeValue))
            {
                // don't need cdata
                cdataEnd = "";
                cdataStart = "";                
            }
            if (nodeValue.Contains(cdataEnd))
            {
                // if we already have a cdata in the node we can't wrap it into another and keep the XML strucutre.
                cdataEnd = "";
                cdataStart = "";
            }
            var strXml = "<root><" + nodeName + ">" + cdataStart + nodeValue + cdataEnd + "</" + nodeName + "></root>";
            try
            {
                AddXmlNode(strXml, "root/" + nodeName, xPathRootDestination);
            }
            catch (Exception)
            {
                // could have fail for bad chars, so try cdata. messy but we're trying to keep backward compatiblity. and the IsValidXmlString function returns true for char that won't add in as a node string.
                cdataStart = "<![CDATA[";
                cdataEnd = "]]>";
                try
                {
                    if (nodeValue.Contains(cdataEnd))
                    {
                        // if we already have a cdata in the node we can't wrap it into another and keep the XML strucutre.
                        cdataEnd = "";
                        cdataStart = "";
                    }
                    strXml = "<root><" + nodeName + ">" + cdataStart + nodeValue + cdataEnd + "</" + nodeName + "></root>";
                    AddXmlNode(strXml, "root/" + nodeName, xPathRootDestination);
                }
                catch (Exception)
                {
                    // log a message, but don't stop processing.  Should never add XML using this method, if we're going to use it.  
                    strXml = "<root><" + nodeName + ">ERROR - Unable to load node, possibly due to XML CDATA clash.</" + nodeName + "></root>";
                    AddXmlNode(strXml, "root/" + nodeName, xPathRootDestination);
                }
            }

        }

        public static bool IsValidXmlString(string text)
        {
            try
            {
                System.Xml.XmlConvert.VerifyXmlChars(text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Add a XML node to a parent destination 
        /// </summary>
        /// <param name="strXml">source XML</param>
        /// <param name="xPathSource">source xpath</param>
        /// <param name="xPathRootDestination">parent xpath in destination</param>
        public void AddXmlNode(string strXml, string xPathSource, string xPathRootDestination)
        {
            var xmlDocNew = new XmlDocument();
            xmlDocNew.LoadXml(strXml);

            var xmlTarget = XMLDoc.SelectSingleNode(xPathRootDestination);
            if (xmlTarget != null)
            {
                var xmlNod2 = xmlDocNew.SelectSingleNode(xPathSource);
                if (xmlNod2 != null)
                {
                    var newNod = XMLDoc.ImportNode(xmlNod2, true);
                    xmlTarget.AppendChild(newNod);
                    XMLData = XMLDoc.OuterXml;
                }
            }
        }

        /// <summary>
        /// Replace xml node in NBrightInfo structure
        /// </summary>
        /// <param name="strXml">New XML, must be in NBright Strucutre (genxml/...)</param>
        /// <param name="xPathSource">Source path of the xml, this is for the new node and the old existing node</param>
        /// <param name="xPathRootDestination">parent node to place the new node onto</param>
        /// <param name="addNode">add if the node doesn;t already exists.</param>
        public void ReplaceXmlNode(string strXml, string xPathSource, string xPathRootDestination, bool addNode = true)
        {
            var xmlDocNew = new XmlDocument();
            xmlDocNew.LoadXml(strXml);

            var xmlNod = XMLDoc.SelectSingleNode(xPathSource);
            if (xmlNod != null)
            {
                var xmlNod2 = xmlDocNew.SelectSingleNode(xPathSource);
                if (xmlNod2 != null)
                {
                    var newNod = XMLDoc.ImportNode(xmlNod2, true);
                    var selectSingleNode = XMLDoc.SelectSingleNode(xPathRootDestination);
                    if (selectSingleNode != null)
                    {
                        selectSingleNode.ReplaceChild(newNod, xmlNod);
                        XMLData = XMLDoc.OuterXml;
                    }
                }
            }
            else
            {
                if (addNode) AddXmlNode(strXml, xPathSource, xPathRootDestination);
            }
        }

        /// <summary>
        /// return int data type from XML
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public int GetXmlPropertyInt(string xpath)
        {
            if (!string.IsNullOrEmpty(XMLData))
            {
                try
                {
                    var x = GenXmlFunctions.GetGenXmlValueRawFormat(XMLData, xpath);
                    if (Utils.IsNumeric(x)) return Convert.ToInt32(x);
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// return double data type from XML
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public double GetXmlPropertyDouble(string xpath)
        {
            if (!string.IsNullOrEmpty(XMLData))
            {
                try
                {
                    var x = GenXmlFunctions.GetGenXmlValueRawFormat(XMLData, xpath);
                    if (Utils.IsNumeric(x))
                    {
                        return Convert.ToDouble(x, CultureInfo.GetCultureInfo("en-US"));
                        // double should always be saved as en-US                        
                    }
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// return Bool data type from XML
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public bool GetXmlPropertyBool(string xpath)
        {
            if (!string.IsNullOrEmpty(XMLData))
            {
                try
                {
                    var x = GenXmlFunctions.GetGenXmlValueRawFormat(XMLData, xpath);
                    // bool usually stored as "True" "False"
                    if (x.ToLower() == "true") return true;
                    // Test for 1 as true also.
                    if (Utils.IsNumeric(x))
                    {
                        if (Convert.ToInt32(x) > 0) return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        public string GetXmlProperty(string xpath)
        {
            if (!string.IsNullOrEmpty(XMLData))
            {
                try
                {
                    return GenXmlFunctions.GetGenXmlValue(XMLData, xpath);
                }
                catch (Exception ex)
                {
                    return "XML READ ERROR";
                }
            }
            return "";
        }

        /// <summary>
        /// get the data fromthe XML wothout reformatting for numbers or dates. (ISO YYYY-MM-DD for dates , en-US for numbers)
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public string GetXmlPropertyRaw(string xpath)
        {
            if (!string.IsNullOrEmpty(XMLData))
            {
                try
                {
                    return GenXmlFunctions.GetGenXmlValueRawFormat(XMLData, xpath);
                }
                catch (Exception ex)
                {
                    return "XML READ ERROR";
                }
            }
            return "";
        }


        public void AppendToXmlProperty(string xpath, string Value, System.TypeCode DataTyp = System.TypeCode.String,
            bool cdata = true)
        {
            if (!string.IsNullOrEmpty(XMLData))
            {
                var strData = GenXmlFunctions.GetGenXmlValue(XMLData, xpath) + Value;
                XMLData = GenXmlFunctions.SetGenXmlValue(XMLData, xpath, strData, cdata);
            }
        }

        public void SetXmlPropertyDouble(string xpath, Double value, int precision = 2)
        {
            SetXmlPropertyDouble(xpath, Math.Round(value, precision).ToString(""));
        }

        public void SetXmlPropertyDouble(string xpath, string value)
        {
            SetXmlProperty(xpath, value, System.TypeCode.Double, false);
        }

        public void SetXmlProperty(string xpath, string Value, System.TypeCode DataTyp = System.TypeCode.String, bool cdata = true, bool ignoresecurityfilter = false, bool filterlinks = false)
        {
            if (!string.IsNullOrEmpty(XMLData))
            {
                if (DataTyp == System.TypeCode.Double)
                {
                    // always save double in en-US format
                    if (Utils.IsNumeric(Value, Utils.GetCurrentCulture()))
                    {
                        var dbl = Convert.ToDouble(Value, CultureInfo.GetCultureInfo(Utils.GetCurrentCulture()));
                        Value = dbl.ToString(CultureInfo.GetCultureInfo("en-US"));
                    }
                }
                if (DataTyp == System.TypeCode.DateTime)
                {
                    if (Utils.IsDate(Value, Utils.GetCurrentCulture())) Value = Utils.FormatToSave(Value, System.TypeCode.DateTime);
                }
                XMLData = GenXmlFunctions.SetGenXmlValue(XMLData, xpath, Value, cdata,ignoresecurityfilter,filterlinks);
                
                // do the datatype after the node is created
                if (DataTyp == System.TypeCode.DateTime)
                    XMLData = GenXmlFunctions.SetGenXmlValue(XMLData, xpath + "/@datatype", "date", cdata);

                if (DataTyp == System.TypeCode.Double)
                    XMLData = GenXmlFunctions.SetGenXmlValue(XMLData, xpath + "/@datatype", "double", cdata);
            }
        }

        public string ToXmlItem(bool withTextData = true)
        {
            // don't use serlization, becuase depending what is in the TextData field could make it fail.
            var xmlOut = new StringBuilder("<item><itemid>" + ItemID.ToString("") + "</itemid><portalid>" + PortalId.ToString("") + "</portalid><moduleid>" + ModuleId.ToString("") + "</moduleid><xrefitemid>" + XrefItemId.ToString("") + "</xrefitemid><parentitemid>" + ParentItemId.ToString("") + "</parentitemid><typecode>" + TypeCode + "</typecode><guidkey>" + GUIDKey + "</guidkey><lang>" + Lang + "</lang><userid>" + UserId.ToString("") + "</userid>" + XMLData);
            if (withTextData)
            {
                xmlOut.Append("<data><![CDATA[" + TextData.Replace("<![CDATA[", "***CDATASTART***").Replace("]]>", "***CDATAEND***") + "]]></data>");
            }
            xmlOut.Append("</item>");

            return xmlOut.ToString();
        }

        public void FromXmlItem(string xmlItem)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlItem);

            //itemid
            var selectSingleNode = xmlDoc.SelectSingleNode("item/itemid");
            if (selectSingleNode != null) ItemID = Convert.ToInt32(selectSingleNode.InnerText);

            //portalid
            selectSingleNode = xmlDoc.SelectSingleNode("item/portalid");
            if (selectSingleNode != null) PortalId = Convert.ToInt32(selectSingleNode.InnerText);

            // moduleid
            selectSingleNode = xmlDoc.SelectSingleNode("item/moduleid");
            if (selectSingleNode != null) ModuleId = Convert.ToInt32(selectSingleNode.InnerText);

            //xrefitemid
            selectSingleNode = xmlDoc.SelectSingleNode("item/xrefitemid");
            if (selectSingleNode != null) XrefItemId = Convert.ToInt32(selectSingleNode.InnerText);

            //parentitemid
            selectSingleNode = xmlDoc.SelectSingleNode("item/parentitemid");
            if (selectSingleNode != null) ParentItemId = Convert.ToInt32(selectSingleNode.InnerText);

            //typecode
            selectSingleNode = xmlDoc.SelectSingleNode("item/typecode");
            if (selectSingleNode != null) TypeCode = selectSingleNode.InnerText;

            //guidkey
            selectSingleNode = xmlDoc.SelectSingleNode("item/guidkey");
            if (selectSingleNode != null) GUIDKey = selectSingleNode.InnerText;

            //XmlData
            selectSingleNode = xmlDoc.SelectSingleNode("item/genxml");
            if (selectSingleNode != null) XMLData = selectSingleNode.OuterXml;

            //TextData
            selectSingleNode = xmlDoc.SelectSingleNode("item/data");
            if (selectSingleNode != null)
                TextData = selectSingleNode.InnerText.Replace("***CDATASTART***", "<![CDATA[")
                    .Replace("***CDATAEND***", "]]>");

            //lang
            selectSingleNode = xmlDoc.SelectSingleNode("item/lang");
            if (selectSingleNode != null) Lang = selectSingleNode.InnerText;

            //userid
            selectSingleNode = xmlDoc.SelectSingleNode("item/userid");
            if ((selectSingleNode != null) && (Utils.IsNumeric(selectSingleNode.InnerText)))
                UserId = Convert.ToInt32(selectSingleNode.InnerText);

        }


        public Dictionary<String, String> ToDictionary(String xpathroot = "")
        {
            var rtnDictionary = new Dictionary<string, string>();
            if (XMLDoc != null)
            {

                rtnDictionary = AddToDictionary(rtnDictionary, xpathroot + "genxml/hidden/*");
                rtnDictionary = AddToDictionary(rtnDictionary, xpathroot + "genxml/textbox/*");
                rtnDictionary = AddToDictionary(rtnDictionary, xpathroot + "genxml/checkbox/*");
                rtnDictionary = AddToDictionary(rtnDictionary, xpathroot + "genxml/dropdownlist/*");
                rtnDictionary = AddToDictionary(rtnDictionary, xpathroot + "genxml/radiobuttonlist/*");
            }
            if (!rtnDictionary.ContainsKey("moduleid")) rtnDictionary.Add("moduleid",ModuleId.ToString(""));
            if (!rtnDictionary.ContainsKey("portalid")) rtnDictionary.Add("portalid", PortalId.ToString(""));
            if (!rtnDictionary.ContainsKey("itemid")) rtnDictionary.Add("itemid", ItemID.ToString(""));
            return rtnDictionary;
        }

        private Dictionary<string, string> AddToDictionary(Dictionary<string, string> inpDictionary, string xpath)
        {
            if (XMLDoc != null)
            {
                var nods = XMLDoc.SelectNodes(xpath);
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (inpDictionary.ContainsKey(nod.Name))
                        {
                            inpDictionary[nod.Name] = nod.InnerText; // overwrite same name node
                        }
                        else
                        {
                            inpDictionary.Add(nod.Name, nod.InnerText);
                        }
                        if (nod.Attributes != null && nod.Attributes["selectedtext"] != null)
                        {
                            var textname = nod.Name + "text";
                            if (inpDictionary.ContainsKey(textname))
                            {
                                inpDictionary[textname] = nod.Attributes["selectedtext"].Value;
                                    // overwrite same name node
                            }
                            else
                            {
                                inpDictionary.Add(textname, nod.Attributes["selectedtext"].Value);
                            }
                        }
                    }
                }
            }
            return inpDictionary;
        }

        public void UpdateAjax(String ajaxStrData)
        {
            UpdateAjax(ajaxStrData, "",false);
        }
        
        public void UpdateAjax(String ajaxStrData, String updateTypePrefix = "", bool ignoresecurityfilter = false, bool filterlinks = false)
        {
            ValidateXmlFormat(); // make sure we have correct structure so update works.
            var updateType = updateTypePrefix + "save";
            if (!String.IsNullOrEmpty(Lang)) updateType = updateTypePrefix + "lang";
            var ajaxInfo = new NBrightInfo();
            var xmlData = GenXmlFunctions.GetGenXmlByAjax(ajaxStrData, "", "genxml", ignoresecurityfilter, filterlinks);
            ajaxInfo.XMLData = xmlData;
            var nodList2 = ajaxInfo.XMLDoc.SelectNodes("genxml/*");
            if (nodList2 != null)
            {
                foreach (XmlNode nod1 in nodList2)
                {
                    var nodList = ajaxInfo.XMLDoc.SelectNodes("genxml/" + nod1.Name.ToLower() + "/*");
                    if (nodList != null)
                    {
                        foreach (XmlNode nod in nodList)
                        {
                            if (nod.Attributes != null && nod.Attributes["update"] != null)
                            {
                                if (nod1.Name.ToLower() == "checkboxlist")
                                {
                                    if (nod.Attributes["update"].InnerText.ToLower() == updateType)
                                    {
                                        RemoveXmlNode("genxml/checkboxlist/" + nod.Name.ToLower());
                                        AddXmlNode(nod.OuterXml, nod.Name.ToLower(), "genxml/checkboxlist");
                                    }
                                }
                                else
                                {
                                    if (nod.Attributes["update"].InnerText.ToLower() == updateType)
                                    {
                                        var xpath = "genxml/" + nod1.Name.ToLower() + "/" + nod.Name.ToLower();
                                        SetXmlProperty(xpath, nod.InnerText);
                                        if (nod.Attributes["datatype"] != null)
                                        {
                                            // just need to update the attr on the XML, the Formatting has been done by the GetGenXmlByAjax function.
                                            XMLData = GenXmlFunctions.SetGenXmlValue(XMLData, xpath + "/@datatype", nod.Attributes["datatype"].InnerText.ToLower());
                                        }
                                    }
                                }
                            }
                        }

                    }
                }

            }

        }


        #region "Xref"

        public void AddXref(string nodeName, string value)
        {
            //create node if not there.
            if (XMLDoc.SelectSingleNode("genxml/" + nodeName) == null)
                AddXmlNode("<genxml><" + nodeName + "></" + nodeName + "></genxml>", "genxml/" + nodeName, "genxml");
            //Add new xref node, if not there.
            if (XMLDoc.SelectSingleNode("genxml/" + nodeName + "/id[.='" + value + "']") == null)
                AddXmlNode("<genxml><" + nodeName + "><id>" + value + "</id></" + nodeName + "></genxml>",
                    "genxml/" + nodeName + "/id", "genxml/" + nodeName);
        }

        public void RemoveXref(string nodeName, string value)
        {
            //Removexref node, if there.
            if (XMLDoc.SelectSingleNode("genxml/" + nodeName + "/id[.='" + value + "']") != null)
                RemoveXmlNode("genxml/" + nodeName + "/id[.='" + value + "']");
        }


        public List<String> GetXrefList(string nodeName)
        {
            var strList = new List<String>();
            var nodList = XMLDoc.SelectNodes("genxml/" + nodeName + "/id");
            foreach (XmlNode nod in nodList)
            {
                strList.Add(nod.InnerText);
            }
            return strList;
        }

        public void ValidateXmlFormat()
        {
            if (XMLDoc == null) XMLData = GenXmlFunctions.GetGenXml(new RepeaterItem(0, ListItemType.Item)); // if we don;t have anything, create an empty default to stop errors.

            if (XMLDoc.SelectSingleNode("genxml/hidden") == null) SetXmlProperty("genxml/hidden", "");
            if (XMLDoc.SelectSingleNode("genxml/textbox") == null) SetXmlProperty("genxml/textbox", "");
            if (XMLDoc.SelectSingleNode("genxml/checkbox") == null) SetXmlProperty("genxml/checkbox", "");
            if (XMLDoc.SelectSingleNode("genxml/dropdownlist") == null) SetXmlProperty("genxml/dropdownlist", "");
            if (XMLDoc.SelectSingleNode("genxml/radiobuttonlist") == null) SetXmlProperty("genxml/radiobuttonlist", "");
        }

        #endregion

        public object Clone()
        {
            var obj = (NBrightInfo) this.MemberwiseClone();
            obj.XMLData = this.XMLData;
            return obj;
        }
    }


    public class NBrightRazor
    {
        public Dictionary<String,String> Settings { get; set; }
        public NameValueCollection UrlParams { get; set; }
        public List<object> List { get; set; }
        public int ModuleId { get; set; }
        public String ModuleRef { get; set; }
        public int ModuleIdDataSource { get; set; }

        public String FullTemplateName { get; set; }
        public String TemplateName { get; set; }
        public String ThemeFolder { get; set; }



        public NBrightRazor(List<object> list, Dictionary<String,String> settings, NameValueCollection urlParams)
        {
            Settings = settings;
            UrlParams = urlParams;
            List = list;

            ModuleRef = "";
            ModuleId = 0;
            ModuleIdDataSource = 0;

            if (settings.ContainsKey("modref")) ModuleRef = settings["modref"];
            if (settings.ContainsKey("moduleid") && Utils.IsNumeric(settings["moduleid"]))
            {
                ModuleId = Convert.ToInt32(settings["moduleid"]);
                ModuleIdDataSource = ModuleId;
            }
            if (settings.ContainsKey("moduleiddatasource") && !String.IsNullOrWhiteSpace(settings["moduleiddatasource"]))
            {
                ModuleIdDataSource = Convert.ToInt32(settings["moduleiddatasource"]);
            }

        }
        public NBrightRazor(List<object> list, Dictionary<String, String> settings)
        {
            Settings = settings;
            UrlParams = new NameValueCollection();
            List = list;
        }
        public NBrightRazor(List<object> list, NameValueCollection urlParams)
        {
            Settings = new Dictionary<string, string>();
            UrlParams = urlParams;
            List = list;
        }

        public String GetSetting(String key,String defaultValue = "")
        {
            if (Settings.ContainsKey(key)) return Settings[key];
            return defaultValue;
        }

        public Boolean GetSettingBool(String key, Boolean defaultValue = false)
        {
            try
            {
                if (Settings.ContainsKey(key))
                {
                    var x = Settings[key];
                    // bool usually stored as "True" "False"
                    if (x.ToLower() == "true") return true;
                    // Test for 1 as true also.
                    if (Utils.IsNumeric(x))
                    {
                        if (Convert.ToInt32(x) > 0) return true;
                    }
                    return false;
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                return defaultValue;
            }
        }


        public int GetSettingInt(String key, int defaultValue = -1)
        {
            if (Settings.ContainsKey(key))
            {
                var s = Settings[key];
                if (Utils.IsNumeric(s)) return Convert.ToInt32(s);
            }
            return defaultValue;
        }

        public IEncodedString GetSettingHtmlOf(String key, String defaultValue = "")
        {
            if (Settings.ContainsKey(key)) return new RawString(HttpUtility.HtmlDecode(Settings[key]));
            return new RawString(defaultValue);
        }

        public String GetUrlParam(String key, String defaultValue = "")
        {
            var result = defaultValue;
            if (UrlParams.Count != 0)
            {
                result = Convert.ToString(UrlParams[key]);
            }
            return (result == null) ? defaultValue : result.Trim(); 
        }

    }
}
