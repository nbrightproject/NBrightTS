using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using NBrightCore.common;

namespace NBrightCore.render
{
    public class GenXmlTemplate : ITemplate
    {

        protected string[] AryTempl;
        public List<string> MetaTags;
        protected string Rootname = "genxml";
        protected string DatabindColumn = "XMLData";
        protected Page CurrentPage;
        protected string EditCultureCode = "";
        protected Dictionary<string, string> Settings;
        private Dictionary<string, string> hiddenFields;
        private List<string> _ResourcePath;
        private ConcurrentStack<Boolean> visibleStatus;

        public String GetHiddenFieldValue(string Key)
        {
            if (hiddenFields.ContainsKey(Key.ToLower()))
            {
                return hiddenFields[Key.ToLower()];
            }
            return "";
        }

        public String SortItemId { get; set; }

        public void AddProvider()
        {
            
        }


        /// <summary>
        /// Add a relative folder for resx files to be included in templating localized data
        /// </summary>
        /// <param name="resxFolder"></param>
        public void AddResxFolder(String resxFolder)
        {
            _ResourcePath.Add(resxFolder);
        }

        public List<String> GetResxFolders()
        {
            return _ResourcePath;
        }

        public GenXmlTemplate(string templateText, Dictionary<string, string> settings, ConcurrentStack<Boolean> visibleStatus)
            : this(templateText, "genxml", "XMLData", "", settings, visibleStatus)
        {
        }

        public GenXmlTemplate(string templateText, Dictionary<string, string> settings)
            : this(templateText, "genxml", "XMLData", "", settings,null)
        {
        }

        public GenXmlTemplate(string templateText, string xmlRootName = "genxml", string dataBindXmlColumn = "XMLData", string cultureCode = "", Dictionary<string, string> settings = null, ConcurrentStack<Boolean> visibleStatusIn = null)
        {
            //set the rootname of the xml, this allows for compatiblity with legacy xml structure
            Rootname = xmlRootName;
            AryTempl = Utils.ParseTemplateText(templateText);
            DatabindColumn = dataBindXmlColumn;
            MetaTags = new List<string>();
            hiddenFields = new Dictionary<string, string>();
            EditCultureCode = cultureCode;
            Settings = settings;

            if (visibleStatusIn == null)
            {
                visibleStatus = new ConcurrentStack<Boolean>();
                visibleStatus.Push(true);
            }
            else
            {
                visibleStatus = visibleStatusIn;
            }

            // find any meta tags
            var xmlDoc = new XmlDocument();
            string ctrltype = "";
            _ResourcePath = new List<string>();
            foreach (var s in AryTempl)
            {
                var htmlDecode = System.Web.HttpUtility.HtmlDecode(s);
                    if (htmlDecode != null && htmlDecode.ToLower().StartsWith("<tag"))
                    {
                        var strXml = System.Web.HttpUtility.HtmlDecode(s);
                        strXml = "<root>" + strXml + "</root>";

                        try
                        {
                            xmlDoc.LoadXml(strXml);
                            var xmlNod = xmlDoc.SelectSingleNode("root/tag");
                            if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["type"] != null)))
                            {
                                ctrltype = xmlNod.Attributes["type"].InnerXml.ToLower();
                            }

                            if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["ctrltype"] != null)))
                            {
                                ctrltype = xmlNod.Attributes["ctrltype"].InnerXml.ToLower();
                            }

                            if (ctrltype == "meta" | ctrltype == "const" | ctrltype == "action")  //also add const to meta tag list
                            {
                                MetaTags.Add(s);
                                if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null)))
                                {
                                    // add these to hidden fields before the dtabind, so we can pick them up on poatback
                                    if (!hiddenFields.ContainsKey(xmlNod.Attributes["id"].Value.ToLower())) hiddenFields.Add(xmlNod.Attributes["id"].Value.ToLower(), xmlNod.Attributes["value"].Value);

                                    if (xmlNod.Attributes["id"].Value.ToLower().StartsWith("resourcepath"))
                                    {
                                        if (!_ResourcePath.Contains(xmlNod.Attributes["value"].Value)) _ResourcePath.Add(xmlNod.Attributes["value"].Value);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Might be an eror in the template, but we don;t want to find an error here, just skip it so we get a nasty erro messgae appearing later! 
                        }

                    }
            }

        }


        /// <summary>
        /// tag types: 
        /// <para>fileupload : file upload control</para>
        /// <para>linkbutton : link button to post back a response</para>
        /// <para>translatebutton : ?</para>
        /// <para>valueof : Display value of a field.</para>
        /// <para>breakof : Display value of a field and change newline to &gt;br/&lt;</para>
        /// <para>testof : Test for values and display text/html based on true or false.</para>
        /// <para>htmlof : Display encoded html, saved as html in the xml, mainly for the richtext editor.</para>
        /// <para>label : label control</para>
        /// <para>hidden : hidden field.</para>
        /// <para>const : hidden field, but without ability to change values on save.</para>
        /// <para>textbox : Textbox control.</para>
        /// <para>dropdownlist : dropdownlist control.</para>
        /// <para>checkboxlist : checkbox list control.</para>
        /// <para>checkbox : checkbox control.</para>
        /// <para>radiobuttonlist : radiobuttonlist control.</para>
        /// <para>rvalidator : field validator control.</para>
        /// <para>rfvalidator : required field validator.</para>
        /// <para>revalidator: RegExpr validator.</para>
        /// <para>cvalidator : custom validator.</para>
        /// <para>validationsummary : validation summary control.</para> 
        /// </summary>
        public void InstantiateIn(Control container)
        {

            if (CurrentPage != null)
            {
                container.Page = CurrentPage;                
            }

            var xmlDoc = new XmlDocument();
            string ctrltype = "";
            string tokennamespace = "";

            for (var lp = 0; lp <= AryTempl.GetUpperBound(0); lp++)
            {

                if ((AryTempl[lp] != null))
                {
                    var htmlDecode = System.Web.HttpUtility.HtmlDecode(AryTempl[lp]);
                    if (htmlDecode != null && htmlDecode.ToLower().StartsWith("<tag"))
                    {
                        var strXml = System.Web.HttpUtility.HtmlDecode(AryTempl[lp]);
                        strXml = strXml.Replace("&", "&amp;"); // escape to stop any error. can create a issue if we really need only (&)
                        strXml = "<root>" + strXml + "</root>";

                        xmlDoc.LoadXml(strXml);
                        var xmlNod = xmlDoc.SelectSingleNode("root/tag");
                        if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["type"] != null)))
                        {
                            ctrltype = xmlNod.Attributes["type"].InnerXml.ToLower();
                        }

                        if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["ctrltype"] != null)))
                        {
                            ctrltype = xmlNod.Attributes["ctrltype"].InnerXml.ToLower();
                        }

                        if (!string.IsNullOrEmpty(ctrltype))
                        {
							// get any Langauge Resource Data from CMS
							if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["resourcekey"] != null  || xmlNod.Attributes["resourcekeysave"] != null)))
							{
								xmlNod = GetCMSResourceData(xmlDoc);
							}

                            switch (ctrltype)
                            {
                                case "testof":
                                    CreateTestOf(container, xmlNod);
                                    break;
                                case "if":
                                    CreateTestOf(container, xmlNod);
                                    break;
                                case "endtestof":
                                    CreateEndTestOf(container, xmlNod);
                                    break;
                                case "endif":
                                    CreateEndTestOf(container, xmlNod);
                                    break;
                                case "fileupload":
                                    CreateFileUpload(container, xmlNod);
                                    break;
                                case "linkbutton":
                                    CreateLinkButton(container, xmlNod);
                                    break;
                                case "button":
                                    CreateButton(container, xmlNod);
                                    break;
                                case "translatebutton":
                                    CreateTransButton(container, xmlNod);
                                    break;
                                case "assignof":
                                    CreateAssignOf(container, xmlNod);
                                    break;
                                case "valueof":
                                    CreateValueOf(container, xmlNod);
                                    break;
                                case "breakof":
                                    CreateBreakOf(container, xmlNod);
                                    break;
                                case "listof":
                                    CreateListOf(container, xmlNod);
                                    break;
                                case "checkboxlistof":
                                    CreateCheckBoxListOf(container, xmlNod);
                                    break;
                                case "htmlof":
                                    CreateHtmlOf(container, xmlNod);
                                    break;
                                case "decodeof":
                                    CreateHtmlOf(container, xmlNod);
                                    break;
                                case "encodeof":
                                    CreateEncodeOf(container, xmlNod);
                                    break;
                                case "label":
                                    CreateLabel(container, xmlNod);
                                    break;
                                case "hidden":
                                    CreateHidden(container, xmlNod);
                                    if ((xmlNod != null) && (xmlNod.Attributes != null) && (xmlNod.Attributes["id"] != null) && (xmlNod.Attributes["value"] != null))
                                    {
                                            if (!hiddenFields.ContainsKey(xmlNod.Attributes["id"].Value.ToLower())) hiddenFields.Add(xmlNod.Attributes["id"].Value.ToLower(), xmlNod.Attributes["value"].Value);
                                    }
                                    break;
                                case "postback":
                                    CreatePostBack(container, xmlNod);
                                    break;
                                case "currentculture":
                                    CreateCurrentCulture(container, xmlNod);
                                    break;
                                case "const":
                                    CreateConst(container, xmlNod);
                                    if ((xmlNod != null) && (xmlNod.Attributes != null) && (xmlNod.Attributes["id"] != null) && (xmlNod.Attributes["value"] != null))
                                    {
                                        if (!hiddenFields.ContainsKey(xmlNod.Attributes["id"].Value.ToLower())) hiddenFields.Add(xmlNod.Attributes["id"].Value.ToLower(), xmlNod.Attributes["value"].Value);
                                    }
                                    break;
                                case "culturecode":
                                    CreateCultureCode(container, xmlNod);
                                    break;
                                case "textbox":
                                    CreateTextbox(container, xmlNod);
                                    break;
                                case "dropdownlist":
                                    CreateDropDownList(container, xmlNod);
                                    break;
                                case "checkboxlist":
                                    CreateCheckBoxList(container, xmlNod);
                                    break;
                                case "checkbox":
                                    CreateCheckBox(container, xmlNod);
                                    break;
                                case "radiobuttonlist":
                                    CreateRadioButtonList(container, xmlNod);
                                    break;
                                case "rvalidator":
                                    CreateRangeValidator(container, xmlNod);
                                    break;
                                case "rfvalidator":
                                    CreateRequiredFieldValidator(container, xmlNod);
                                    break;
                                case "revalidator":
                                    CreateRegExValidator(container, xmlNod);
                                    break;
                                case "cvalidator":
                                    CreateCustomValidator(container, xmlNod);
                                    break;
                                case "validationsummary":
                                    CreateValidationSummary(container, xmlNod);
                                    break;
                                case "meta":
                                    if ((xmlNod != null) && (xmlNod.Attributes != null) && (xmlNod.Attributes["id"] != null) && (xmlNod.Attributes["value"] != null))
                                    {
                                        if (!hiddenFields.ContainsKey(xmlNod.Attributes["id"].Value.ToLower())) hiddenFields.Add(xmlNod.Attributes["id"].Value.ToLower(), xmlNod.Attributes["value"].Value);
                                    }
                                    // meta tags are for passing data to the system only and should not be displayed. (e.g. orderby field)
                                    break;
                                case "action":
                                    if ((xmlNod != null) && (xmlNod.Attributes != null) && (xmlNod.Attributes["id"] != null) && (xmlNod.Attributes["value"] != null))
                                    {
                                        if (!hiddenFields.ContainsKey(xmlNod.Attributes["id"].Value.ToLower())) hiddenFields.Add(xmlNod.Attributes["id"].Value.ToLower(), xmlNod.Attributes["value"].Value);
                                    }
                                    // action tags are for passing data to the system only and should not be displayed.
                                    break;
                                case "itemindex":
                                    if (container.GetType() == typeof(RepeaterItem))
                                    {
                                        var c = (RepeaterItem)container;
                                        var litC = new Literal { Text = c.ItemIndex.ToString(CultureInfo.InvariantCulture) };
                                        container.Controls.Add(litC);                                        
                                    }
                                    break;
                                default:

                                    // use a token namespace if we specify a namepace tag (this is so we can use multiple provider extensions and have unique token names)
                                    if (ctrltype == "tokennamespace" && (xmlNod != null && xmlNod.Attributes != null && (xmlNod.Attributes["value"] != null))) tokennamespace = xmlNod.Attributes["value"].InnerText;
                                    if (!ctrltype.Contains(":") && tokennamespace.Trim() != "") ctrltype = tokennamespace.TrimEnd(':') + ":" + ctrltype;

                                    var providerCtrl = false;

                                    //check for any template providers.
                                    var providerList = providers.GenXProviderManager.ProviderList;
                                    if (providerList != null)
                                    {                                    
                                        foreach (var prov in providerList)
                                        {
                                            providerCtrl = prov.Value.CreateGenControl(ctrltype, container, xmlNod, Rootname, DatabindColumn, EditCultureCode, Settings, visibleStatus);
                                            if (providerCtrl)
                                            {
                                                break;
                                            }
                                        }   
                                    }


                                    if (providerCtrl == false && ctrltype != (tokennamespace.TrimEnd(':') + ":" + "tokennamespace")) //don;t display namespace tag
                                    {
                                        var lc = new Literal();
                                        xmlDoc.LoadXml(strXml);
                                        xmlNod = xmlDoc.SelectSingleNode("root/tag");
                                        if ((xmlNod != null) && (xmlNod.Attributes != null))
                                        {
                                            lc.Text = xmlNod.Attributes[0].InnerXml;
                                        }
                                        lc.DataBinding += GeneralDataBinding;
                                        container.Controls.Add(lc);
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        var lc = new Literal {Text = AryTempl[lp]};
                        lc.DataBinding += LiteralDataBinding;  // used to get if visible or not.
                        container.Controls.Add(lc);
                    }
                }
            }

        }


        #region "testing functions"

        private void CreateTestOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal { Text = xmlNod.OuterXml };
            lc.DataBinding += TestOfDataBinding;
            container.Controls.Add(lc);

            //we may have settings test in the provider, so create a dummay control, just to pass all the settings
            var providerList = providers.GenXProviderManager.ProviderList;
            if (providerList != null)
            {
                foreach (var prov in providerList)
                {
                    var providerCtrl = prov.Value.CreateGenControl("populatesettings", container, xmlNod, Rootname, DatabindColumn, EditCultureCode, Settings, visibleStatus);
                }
            }

        }

        private void TestOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var xmlDoc = new XmlDocument();
                string testValue = "";
                string display = "";
                string displayElse = "";
                string dataValue = "";
                var roleValid = true;
                var dotestprovider = true; // use a flag to check if we do have a dtavalue but it's empty

                xmlDoc.LoadXml("<root>" + lc.Text + "</root>");
                var xmlNod = xmlDoc.SelectSingleNode("root/tag");

                if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["settings"] != null)))
                {
                    dataValue = xmlNod.Attributes["settings"].InnerXml;
                    if (Settings != null && Settings.ContainsKey(dataValue)) dataValue = Settings[dataValue];
                    dotestprovider = false;
                }

                if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["testvalue"] != null)))
                {
                    testValue = xmlNod.Attributes["testvalue"].InnerXml;
                    if (testValue.ToLower() == "{username}") testValue = providers.CmsProviderManager.Default.GetCurrentUserName();
                    if (testValue.ToLower() == "{userid}") testValue = providers.CmsProviderManager.Default.GetCurrentUserId().ToString("");
                    if (Settings != null)
                    {
                        if (testValue.ToLower().StartsWith("settings:"))
                        {
                            var setkey = testValue.Replace("Settings:", "").Replace("settings:", "");
                            if (Settings.ContainsKey(setkey)) testValue = Settings[setkey];
                        }
                    }
                }

                if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["testinrole"] != null)))
                {
                    var testRole = xmlNod.Attributes["testinrole"].InnerXml;
                    //do test for user rolew
                    if (!providers.CmsProviderManager.Default.IsInRole(testRole))
                    {
                        roleValid = false;
                    }
                }

                if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["display"] != null)))
                {
                    display = xmlNod.Attributes["display"].InnerXml;
                }

                if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["displayelse"] != null)))
                {
                    displayElse = xmlNod.Attributes["displayelse"].InnerXml;
                }
                else
                {
                    if (display == "{ON}") displayElse = "{OFF}";
                    if (display == "{OFF}") displayElse = "{ON}";
                }

                if (container.DataItem != null && xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null)))
                {
                    var nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), xmlNod.Attributes["xpath"].InnerXml);
                    if (nod != null)
                    {
                        dataValue = nod.InnerText;
                        dotestprovider = false;
                    }
                }

                if (container.DataItem != null && xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null)))
                {
                    dataValue = Convert.ToString(DataBinder.Eval(container.DataItem, xmlNod.Attributes["databind"].InnerXml));
                    dotestprovider = false;
                }

                // special check to see if a sort item has been selected.
                if (testValue.ToLower() == "sortselected")
                {
                    if (Utils.IsNumeric(SortItemId))
                    {
                        dataValue = "sortselected";
                        dotestprovider = false;
                    }
                }

                //check for alternate row
                if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["alternate"] != null)))
                {
                    testValue = xmlNod.Attributes["alternate"].InnerText.ToLower();
                    dataValue = Convert.ToString(DataBinder.Eval(container.DataItem, "RowCount"));
                    dotestprovider = false;

                    if (Utils.IsNumeric(dataValue))
                    {
                        var rowcount = Convert.ToInt32(dataValue);
                        if (rowcount % 2 == 0)
                        {
                            dataValue = "true";
                        }
                        else
                        {
                            dataValue = "false";
                        }
                    }
                }

                // do test on static
                if (xmlNod.Attributes["staticvalue"] != null)
                {
                    dotestprovider = false;
                    dataValue = xmlNod.Attributes["staticvalue"].InnerText;
                }


                if (dotestprovider)
                {
                    //check for any providers.
                    var providerList = providers.GenXProviderManager.ProviderList;
                    if (providerList != null)
                    {
                        foreach (var prov in providerList)
                        {
                            var testData = prov.Value.TestOfDataBinding(sender, e);
                            if (testData != null && testData.DataValue != null)
                            {
                                dataValue = testData.DataValue;
                                if (testData.TestValue != null) testValue = testData.TestValue;
                                break;
                            }
                        }
                    }                    
                }


                // ---- DO TEST ---------------------------------------
                string output;
                if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["regex"] != null)))
                {
                    // do a regexpr pattern match test
                    var match = Regex.Match(dataValue, xmlNod.Attributes["regex"].InnerText, RegexOptions.IgnoreCase);
                    if (match.Success)
                        output = display;
                    else
                        output = displayElse;
                }
                else
                {
                    // check if we have a greater or lesser then test( < or > at front)
                    // This is also escaped by using >> or << at the front, so check for that first.
                    if (testValue.StartsWith("&lt;&lt;") || testValue.StartsWith("&gt;&gt;"))
                    {
                        // escape < and >
                        testValue = testValue.Replace("&lt;&lt;", "&lt;").Replace("&gt;&gt;", "&gt;");
                        if ((dataValue == testValue) & roleValid)
                            output = display;
                        else
                            output = displayElse;
                    }
                    else if (testValue.StartsWith("&lt;"))
                    {
                        // do greater ot lesser than (Assume numeric!!)
                        testValue = testValue.Replace("&lt;", "");
                        if (!Utils.IsNumeric(dataValue)) dataValue = "0";
                        if (!Utils.IsNumeric(testValue)) testValue = "0";
                        if ((Convert.ToDouble(dataValue) < Convert.ToDouble(testValue)) & roleValid)
                            output = display;
                        else
                            output = displayElse;
                    }
                    else if (testValue.StartsWith("&gt;"))
                    {
                        testValue = testValue.Replace("&gt;", "");
                        if (!Utils.IsNumeric(dataValue)) dataValue = "0";
                        if (!Utils.IsNumeric(testValue)) testValue = "0";
                        if ((Convert.ToDouble(dataValue) > Convert.ToDouble(testValue)) & roleValid)
                            output = display;
                        else
                            output = displayElse;
                    }
                    else
                    {
                        if ((dataValue == testValue) & roleValid)
                            output = display;
                        else
                            output = displayElse;
                    }
                }


                // If the Visible flag is OFF then keep it off, even if the child test is true
                // This allows nested tests to function correctly, by using the parent result.
                if (!visibleStatus.DefaultIfEmpty(true).First())
                {
                    if (output == "{ON}" | output == "{OFF}") visibleStatus.Push(false); // only add level on {} testof
                }
                else
                {
                    if (output == "{ON}") visibleStatus.Push(true);
                    if (output == "{OFF}") visibleStatus.Push(false);
                }

                if (visibleStatus.DefaultIfEmpty(true).First() && output != "{ON}") lc.Text = output;
                if (output == "{ON}" | output == "{OFF}") lc.Text = ""; // don;t display the test tag
                if (output == "{commentON}") lc.Text = "<!--";
                if (output == "{commentOFF}") lc.Text = "-->";
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        private void CreateEndTestOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal { Text = xmlNod.OuterXml };
            lc.DataBinding += EndTestOfDataBinding;
            container.Controls.Add(lc);
        }

        private void EndTestOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            bool item;
            if (visibleStatus.TryPop(out item))
            {
                // don't need to do anything, just removing the visible status due to endif
            }
            lc.Text = "";  //always display nothign for this, it just to stop testof state.
        }


        #endregion

        #region "create controls"


        private void CreateValueOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            
            if (xmlNod.Attributes != null && (xmlNod.Attributes["Text"] != null))
            {
                lc.Text = "resxdata:" + xmlNod.Attributes["Text"].InnerXml;
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].InnerXml;
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                lc.Text = "databind:" + xmlNod.Attributes["databind"].InnerXml;
            }

            // pass structured string to format data
            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null))
            {
                var strFormat = "";
                if ((xmlNod.Attributes["format"] != null))
                {
                    strFormat = xmlNod.Attributes["format"].InnerXml.Replace(":", "**COLON**");
                }
                if ((xmlNod.Attributes["culturecode"] != null))
                {
                    strFormat += "," + xmlNod.Attributes["culturecode"].InnerXml;
                }
                if (xmlNod.Attributes["datatype"].InnerText.ToLower() == "date")
                {
                    if (strFormat == "") strFormat = "d";
                    lc.Text = "date:" + strFormat + ":" + lc.Text;
                }
                if (xmlNod.Attributes["datatype"].InnerText.ToLower() == "double")
                {
                    lc.Text = "double:" + strFormat + ":" + lc.Text;
                }

            }

            lc.DataBinding += ValueOfDataBinding;
            container.Controls.Add(lc);
        }

        private void CreateAssignOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();

            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].InnerXml;
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                lc.Text = "databind:" + xmlNod.Attributes["databind"].InnerXml;
            }

            lc.DataBinding += AssignOfDataBinding;
            container.Controls.Add(lc);
        }

        private void CreateBreakOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();

            if (xmlNod.Attributes != null && (xmlNod.Attributes["Text"] != null))
            {
                lc.Text = "resxdata:" + xmlNod.Attributes["Text"].InnerXml;
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                lc.Text = "databind:" + xmlNod.Attributes["databind"].InnerXml;
            }

            lc.DataBinding += BreakOfDataBinding;
            container.Controls.Add(lc);
        }

        private void CreateListOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].InnerXml;
            }
            
            lc.DataBinding += ListOfDataBinding;
            container.Controls.Add(lc);
        }

        private void CreateCheckBoxListOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["displaytype"] != null))
            {
                lc.Text = lc.Text + ";" + xmlNod.Attributes["displaytype"].InnerXml;
            }

            lc.DataBinding += ChkBoxListOfDataBinding;
            container.Controls.Add(lc);
        }

        private void CreateHtmlOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["Text"] != null))
            {
                lc.Text = "resxdata:" + xmlNod.Attributes["Text"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                lc.Text = "databind:" + xmlNod.Attributes["databind"].InnerXml;
            }
            lc.DataBinding += HtmlOfDataBinding;
            container.Controls.Add(lc);
        }

        private void CreateEncodeOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                lc.Text = "databind:" + xmlNod.Attributes["databind"].InnerXml;
            }
            lc.DataBinding += EncodeOfDataBinding;
            container.Controls.Add(lc);
        }

        private void CreateRadioButtonList(Control container, XmlNode xmlNod)
        {
            var rbl = new RadioButtonList();
            rbl = (RadioButtonList) GenXmlFunctions.AssignByReflection(rbl, xmlNod);

            var dataTyp = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null))
            {
                dataTyp = xmlNod.Attributes["datatype"].InnerXml;
                rbl.Attributes.Add("datatype", dataTyp);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["searchindex"] != null))
            {
                rbl.Attributes.Add("searchindex", "1");
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["required"] != null))
            {
                rbl.Attributes.Add("required", xmlNod.Attributes["required"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                rbl.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["data"] != null || xmlNod.Attributes["datavalue"] != null))
            {
                var xmldata = "";
                if ((xmlNod.Attributes["data"] != null)) xmldata = HttpUtility.HtmlDecode(xmlNod.Attributes["data"].InnerXml);
                if (xmldata == "" && (xmlNod.Attributes["datavalue"] != null)) xmldata = xmlNod.Attributes["datavalue"].InnerText;

                string[] strListValue;
                if ((xmlNod.Attributes["datavalue"] != null))
                {
                    var xmldatavalue = HttpUtility.HtmlDecode(xmlNod.Attributes["datavalue"].InnerXml);
                    strListValue = xmldatavalue.Split(';');
                }
                else
                {
                    strListValue = xmldata.Split(';');
                }
                var strList = xmldata.Split(';');
                for (var lp = 0; lp <= strList.GetUpperBound(0); lp++)
                {
                    var li = new ListItem();
                    switch (dataTyp.ToLower())
                    {
                        case "double":
                            li.Text = strList[lp];
                            li.Value = strListValue[lp];
                            break;
                        default:
                            li.Text = strList[lp];
                            li.Value = strListValue[lp];
                            break;
                    }
                    if (xmlNod.Attributes != null && (xmlNod.Attributes["update"] != null))
                    {
                        li.Attributes.Add("update", xmlNod.Attributes["update"].InnerXml);
                    }
                    rbl.Items.Add(li);
                }

            }

            rbl.Visible = GetRoleVisible(xmlNod.OuterXml);
            rbl.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            rbl.DataBinding += RblDataBinding;
            container.Controls.Add(rbl);
        }

        private void CreateCheckBoxList(Control container, XmlNode xmlNod)
        {
            var chk = new CheckBoxList();
            chk = (CheckBoxList)GenXmlFunctions.AssignByReflection(chk, xmlNod);

            var dataTyp = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null))
            {
                dataTyp = xmlNod.Attributes["datatype"].InnerXml;
                chk.Attributes.Add("datatype", dataTyp);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["searchindex"] != null))
            {
                chk.Attributes.Add("searchindex", "1");
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["data"] != null))
            {
                var xmldata = HttpUtility.HtmlDecode(xmlNod.Attributes["data"].InnerXml);

                string[] strListValue;
                if (xmlNod.Attributes["datavalue"] != null)
                {
                    var xmldatavalue = HttpUtility.HtmlDecode(xmlNod.Attributes["datavalue"].InnerXml);
                    strListValue = xmldatavalue.Split(';');
                }
                else
                {
                    strListValue = xmldata.Split(';');
                }
                var strList = xmldata.Split(';');
                for (int lp = 0; lp <= strList.GetUpperBound(0); lp++)
                {
                    var li = new ListItem();
                    li.Attributes.Add("datavalue", strListValue[lp]);                        
                    switch (dataTyp.ToLower())
                    {
                        case "double":
                            li.Text = strList[lp];
                            li.Value = strListValue[lp];
                            break;
                        default:
                            li.Text = strList[lp];
                            li.Value = strListValue[lp];
                            break;
                    }
                    if (xmlNod.Attributes != null && (xmlNod.Attributes["update"] != null))
                    {
                        li.Attributes.Add("update", xmlNod.Attributes["update"].InnerXml);
                    }
                    if (li.Value != "") chk.Items.Add(li);
                }
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatabs"] != null))
            {
                var tList = providers.CmsProviderManager.Default.GetTabList(Utils.GetCurrentCulture());

                foreach (var tItem in tList)
                {
                    var li2 = new ListItem();
                    li2.Text = tItem.Value;
                    li2.Value = tItem.Key.ToString("");
                    if (xmlNod.Attributes != null && (xmlNod.Attributes["update"] != null))
                    {
                        li2.Attributes.Add("update", xmlNod.Attributes["update"].InnerXml);
                    }
                    if (li2.Value != "") chk.Items.Add(li2);
                }
            }

            chk.Visible = GetRoleVisible(xmlNod.OuterXml);
            chk.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            chk.DataBinding += ChkBDataBinding;
            if (chk.Items.Count > 0)
            { // only display if values exist.
                container.Controls.Add(chk);                
            }
        }

        private void CreateCheckBox(Control container, XmlNode xmlNod)
        {
            var chk = new CheckBox();
            chk = (CheckBox)GenXmlFunctions.AssignByReflection(chk, xmlNod);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                chk.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["update"] != null))
            {
                chk.InputAttributes.Add("update", xmlNod.Attributes["update"].InnerXml);
            }


            if (xmlNod.Attributes != null && (xmlNod.Attributes["searchindex"] != null))
            {
                chk.Attributes.Add("searchindex", "1");
            }

			if (xmlNod.Attributes != null && (xmlNod.Attributes["checked"] != null))
			{
				if (xmlNod.Attributes["checked"].Value == "1" | xmlNod.Attributes["checked"].Value == "True")
				{
					chk.Checked = true;
				}
			}


            chk.Visible = GetRoleVisible(xmlNod.OuterXml);
            chk.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            chk.DataBinding += ChkBoxDataBinding;
            container.Controls.Add(chk);
        }

        private void CreateDropDownList(Control container, XmlNode xmlNod)
        {
            var ddl = new DropDownList();
            ddl = (DropDownList)GenXmlFunctions.AssignByReflection(ddl, xmlNod);

            var dataTyp = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null))
            {
                dataTyp = xmlNod.Attributes["datatype"].InnerXml;
                ddl.Attributes.Add("datatype", dataTyp);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["searchindex"] != null))
            {
                ddl.Attributes.Add("searchindex", "1");
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                ddl.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["update"] != null))
            {
                ddl.Attributes.Add("update", xmlNod.Attributes["update"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatabs"] != null))
            {
                ddl.Attributes.Add("datatabs", xmlNod.Attributes["datatabs"].InnerXml);
                var tList = providers.CmsProviderManager.Default.GetTabList(Utils.GetCurrentCulture());

                if (xmlNod.Attributes["datatabs"].InnerXml.ToLower() == "blank")
                {
                    var li = new ListItem();
                    li.Text = "";
                    li.Value = "";
                    ddl.Items.Add(li);                    
                }

                foreach (var tItem in tList)
                {
                    var li = new ListItem();
                    li.Text = tItem.Value ;
                    li.Value = tItem.Key.ToString("");
                    ddl.Items.Add(li);
                }
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["dataculture"] != null))
            {
                ddl.Attributes.Add("dataculture", xmlNod.Attributes["dataculture"].InnerXml);

                if (xmlNod.Attributes["dataculturevalue"] != null)
                {
                    var li = new ListItem();
                    li.Text = xmlNod.Attributes["dataculture"].InnerXml;
                    li.Value = xmlNod.Attributes["dataculturevalue"].InnerXml;
                    ddl.Items.Add(li);
                }
                var cList = providers.CmsProviderManager.Default.GetCultureCodeList();

                foreach (var cItem in cList)
                {
                    var li = new ListItem();
                    li.Text = cItem;
                    li.Value = cItem;
                    ddl.Items.Add(li);
                }
            }


            if (xmlNod.Attributes != null && (xmlNod.Attributes["data"] != null))
            {
                string[] strListValue;
                if ((xmlNod.Attributes["datavalue"] != null))
                {
                    strListValue = HttpUtility.HtmlDecode(xmlNod.Attributes["datavalue"].InnerXml).Split(';');
                }
                else
                {
                    strListValue =  HttpUtility.HtmlDecode(xmlNod.Attributes["data"].InnerXml).Split(';');
                }
                var strList =  HttpUtility.HtmlDecode(xmlNod.Attributes["data"].InnerXml).Split(';');
                for (var lp = 0; lp <= strList.GetUpperBound(0); lp++)
                {
                    var li = new ListItem();
                    switch (dataTyp.ToLower())
                    {
                        case "double":
                            li.Text = Utils.FormatToDisplay(strList[lp],TypeCode.Double);
                            li.Value = Utils.FormatToDisplay(strListValue[lp], TypeCode.Double,"N");
                            break;
                        default:
                            li.Text = strList[lp];
                            li.Value = strListValue[lp];
                            break;
                    }
                    ddl.Items.Add(li);
                }
            }

            ddl.Visible = GetRoleVisible(xmlNod.OuterXml);
            ddl.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            ddl.DataBinding += DdListDataBinding;
            container.Controls.Add(ddl);
        }

        private static void CreateRangeValidator(Control container, XmlNode xmlNod)
        {
            var rfv = new RangeValidator {Text = "*"};
            rfv = (RangeValidator)GenXmlFunctions.AssignByReflection(rfv, xmlNod);

            container.Controls.Add(rfv);
        }

        private static void CreateRegExValidator(Control container, XmlNode xmlNod)
        {

            var rfv = new RegularExpressionValidator { Text = "*" };
            rfv = (RegularExpressionValidator)GenXmlFunctions.AssignByReflection(rfv, xmlNod);

            container.Controls.Add(rfv);
        }

        private static void CreateValidationSummary(Control container, XmlNode xmlNod)
        {
            var rfv = new ValidationSummary();
            rfv = (ValidationSummary)GenXmlFunctions.AssignByReflection(rfv, xmlNod);

            container.Controls.Add(rfv);
        }

        private static void CreateCustomValidator(Control container, XmlNode xmlNod)
        {
            var rfv = new CustomValidator { Text = "*" };
            rfv = (CustomValidator)GenXmlFunctions.AssignByReflection(rfv, xmlNod);

            container.Controls.Add(rfv);
        }

        private static void CreateRequiredFieldValidator(Control container, XmlNode xmlNod)
        {
            var rfv = new RequiredFieldValidator { Text = "*" };
            rfv = (RequiredFieldValidator)GenXmlFunctions.AssignByReflection(rfv, xmlNod);

            container.Controls.Add(rfv);
        }
        /// <summary title="Textbox Control">
        /// <para class="example">
        /// [<tag id="txtModuleKey" type="textbox" width="150" maxlength="50" />]
        /// [<tag id="txtSummary" type="textbox" height="100" width="500" maxlength="200" textmode="MultiLine"/>]
        /// </para>
        /// </summary>
        private TextBox GetCreateTextbox(XmlNode xmlNod)
        {
            var txt = new TextBox {Text = ""};

            txt = (TextBox)GenXmlFunctions.AssignByReflection(txt, xmlNod);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["text"] != null))
            {
                txt.Text = xmlNod.Attributes["text"].InnerXml;
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null))
            {
                txt.Attributes.Add("datatype", xmlNod.Attributes["datatype"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["searchindex"] != null))
            {
                txt.Attributes.Add("searchindex", "1");
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                txt.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["format"] != null))
            {
                txt.Attributes.Add("format", xmlNod.Attributes["format"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["required"] != null))
            {
                txt.Attributes.Add("required", xmlNod.Attributes["required"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["update"] != null))
            {
                txt.Attributes.Add("update", xmlNod.Attributes["update"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null) && xmlNod.Attributes["datatype"].InnerXml == "email")
            {
                txt.Attributes.Add("type", "email");
            }
            else if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null) && xmlNod.Attributes["datatype"].InnerXml == "url")
            {
                txt.Attributes.Add("type", "url");
            }
            else if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null) && xmlNod.Attributes["datatype"].InnerXml == "date")
            {
                txt.Attributes.Add("type", "date");
            }
            else
            {
                txt.Attributes.Add("type", "text");
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["placeholder"] != null)) txt.Attributes.Add("placeholder", xmlNod.Attributes["placeholder"].InnerXml);
            if (xmlNod.Attributes != null && (xmlNod.Attributes["disabled"] != null)) txt.Attributes.Add("disabled", xmlNod.Attributes["disabled"].InnerXml);
            if (xmlNod.Attributes != null && (xmlNod.Attributes["autocomplete"] != null)) txt.Attributes.Add("autocomplete", xmlNod.Attributes["autocomplete"].InnerXml);


            txt.Visible = GetRoleVisible(xmlNod.OuterXml);
            txt.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["resourcekeysave"] != null))
            {
                if (xmlNod.Attributes != null && (xmlNod.Attributes["lang"] != null)) txt.Attributes.Add("lang", xmlNod.Attributes["lang"].InnerXml); ;
                txt.Attributes.Add("resourcekeysave", xmlNod.Attributes["resourcekeysave"].InnerXml); 
            }
            else
                txt.DataBinding += TextDataBinding;

            return txt;
        }

        private void CreateTextbox(Control container, XmlNode xmlNod)
        {
            var txt = GetCreateTextbox(xmlNod);
            container.Controls.Add(txt);
        }

        private void CreateFileUpload(Control container, XmlNode xmlNod)
        {
            var txt = new TextBox();
            var fup = new FileUpload();
            var hid = new HtmlGenericControl();
            var hidInfo = new HtmlGenericControl();
            hid.Attributes["value"] = "";
            hidInfo.Attributes["value"] = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["cssclass"] != null))
            {
                fup.CssClass = xmlNod.Attributes["cssclass"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
            {
                fup.ID = xmlNod.Attributes["id"].InnerXml;
                hid.ID = "hid" + xmlNod.Attributes["id"].InnerXml;
                txt.ID = "txt" + xmlNod.Attributes["id"].InnerXml;
                hidInfo.ID = "hidInfo" + xmlNod.Attributes["id"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["tooltip"] != null))
            {
                fup.ToolTip = xmlNod.Attributes["tooltip"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["textwidth"] != null))
            {
                txt.Width = Convert.ToInt16(xmlNod.Attributes["textwidth"].InnerXml);
            }
            else
            {
                txt.Width = 150;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["maxlength"] != null))
            {
                txt.MaxLength = Convert.ToInt16(xmlNod.Attributes["maxlength"].InnerXml);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["width"] != null))
            {
                fup.Width = Convert.ToInt16(xmlNod.Attributes["width"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["deletefile"] != null))
            {
                fup.Attributes.Add("deletefile", xmlNod.Attributes["deletefile"].InnerText.ToLower());
            }

            txt.Visible = GetRoleVisible(xmlNod.OuterXml);
            txt.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            if (txt.Width == 0 )
            {
                txt.Visible = false;
            }

            fup.DataBinding += FileUploadBinding;  // we might want to hide it with the "testof token"

            fup.Visible = GetRoleVisible(xmlNod.OuterXml);
            fup.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            txt.DataBinding += TextDataBinding;
            container.Controls.Add(txt);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["separator"] != null) & fup.Visible)
            {
                var lc = new Literal {Text = xmlNod.Attributes["separator"].InnerXml};
                if (lc.Text == "br")
                {
                    lc.Text = "<br />";
                }
                container.Controls.Add(lc);
            }

            container.Controls.Add(fup);
            hid.DataBinding += HiddenDataBinding;
            container.Controls.Add(hid);
            hidInfo.DataBinding += HiddenDataBinding;
            container.Controls.Add(hidInfo);
        }

        private void FileUploadBinding(object sender, EventArgs e)
        {
            var fu = (FileUpload)sender;
            try
            {
                fu.Visible = visibleStatus.DefaultIfEmpty(true).First();
            }
            catch (Exception)
            {
                //do nothing
            }
        }


        private static HiddenField GetPostBackCtrl(XmlNode xmlNod)
        {
            var hid = new HiddenField();
            if (xmlNod.Attributes != null)
            {
                if ((xmlNod.Attributes["id"] != null))
                {
                    hid.ID = xmlNod.Attributes["id"].InnerXml.ToLower();
                }

                if ((xmlNod.Attributes["value"] != null))
                {
                    hid.Value = xmlNod.Attributes["value"].InnerXml;
                }
            }

            return hid;
        }


        private static HtmlGenericControl GetHiddenFieldCtrl(XmlNode xmlNod)
        {
            var hid = new HtmlGenericControl("input");
            hid.Attributes.Add("type", "hidden");
            if (xmlNod.Attributes != null)
            {
                var dataType = "";
                if ((xmlNod.Attributes["datatype"] != null))
                {
                    dataType = xmlNod.Attributes["datatype"].InnerXml;
                    hid.Attributes.Add("datatype", dataType);
                }

                if ((xmlNod.Attributes["id"] != null))
                {
                    hid.ID = xmlNod.Attributes["id"].InnerXml.ToLower();
                    // check for legacy datatype naming convension
                    if (xmlNod.Attributes["id"].InnerXml.ToLower().StartsWith("dbl"))
                    {
                        dataType = "double";
                    }
                    if (xmlNod.Attributes["id"].InnerXml.ToLower().StartsWith("dte"))
                    {
                        dataType = "date";
                    }
                    if (xmlNod.Attributes["id"].InnerXml.ToLower().StartsWith("date"))
                    {
                        dataType = "date";
                    }
                }

                if ((xmlNod.Attributes["const"] != null))
                {
                    dataType = xmlNod.Attributes["const"].InnerXml;
                    hid.Attributes.Add("const", dataType);
                }

                if ((xmlNod.Attributes["value"] != null))
                {
                    if (dataType.ToLower() == "double")
                    {
                        hid.Attributes.Add("value",xmlNod.Attributes["value"].InnerXml);
                    }
                    else if (dataType.ToLower() == "date")
                    {
                        hid.Attributes.Add("value",xmlNod.Attributes["value"].InnerXml);
                    }
                    else
                    {
                        hid.Attributes.Add("value",xmlNod.Attributes["value"].InnerXml);
                    }
                }
                if ((xmlNod.Attributes["class"] != null))
                {
                    hid.Attributes.Add("class", xmlNod.Attributes["class"].InnerXml);
                }
                if ((xmlNod.Attributes["cssclass"] != null))
                {//just cover the asp ccsclass 
                    hid.Attributes.Add("class", xmlNod.Attributes["cssclass"].InnerXml);
                }
                if ((xmlNod.Attributes["databind"] != null))
                {
                    hid.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
                }

                if (xmlNod.Attributes != null && (xmlNod.Attributes["update"] != null))
                {
                    hid.Attributes.Add("update", xmlNod.Attributes["update"].InnerXml);
                }

                if ((xmlNod.Attributes["xpath"] != null))
                {
                    hid.Attributes.Add("xpath", xmlNod.Attributes["xpath"].InnerXml);
                }
            }

            return hid;
        }

        private LinkButton GetLinkButtonCtrl(XmlNode xmlNod)
        {
            var cmd = new LinkButton();
            cmd = (LinkButton)GenXmlFunctions.AssignByReflection(cmd, xmlNod);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["src"] != null))
            {
                cmd.Text = "<img src=\"" + xmlNod.Attributes["src"].InnerXml + "\" border=\"0\" />" + cmd.Text;
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["confirm"] != null))
            {
                if (!string.IsNullOrEmpty(xmlNod.Attributes["confirm"].InnerXml))
                {
                    cmd.Attributes.Add("onClick", "javascript:return confirm('" + xmlNod.Attributes["confirm"].InnerXml + "');");
                }
            }

            cmd.Visible = GetRoleVisible(xmlNod.OuterXml);
            cmd.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            return cmd;
        }

        private void CreateLinkButton(Control container, XmlNode xmlNod)
        {
            var cmd = GetLinkButtonCtrl(xmlNod);

            cmd.DataBinding += LinkButtonDataBinding;
            container.Controls.Add(cmd);
        }

        private void CreateLabel(Control container, XmlNode xmlNod)
        {
            var hid = new Label {Text = ""};

            hid = (Label)GenXmlFunctions.AssignByReflection(hid, xmlNod);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                hid.Text = xmlNod.Attributes["xpath"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["ctrltype"] != null))
            {
                hid.Text = xmlNod.Attributes["ctrltype"].InnerXml;
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["length"] != null))
            {
                hid.Attributes.Add("length", xmlNod.Attributes["length"].InnerXml);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["substring"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["substring"].InnerXml))
                {
                    if (hid.Text.Length > Convert.ToInt32(xmlNod.Attributes["substring"].InnerXml))
                    {
                        hid.Text = hid.Text.Substring(0, Convert.ToInt32(xmlNod.Attributes["substring"].InnerXml));
                    }
                }
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["format"] != null))
            {
                hid.Attributes.Add("format", xmlNod.Attributes["format"].InnerXml);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
            {
                hid.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
            }

            hid.Visible = GetRoleVisible(xmlNod.OuterXml);
            hid.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            hid.DataBinding += LabelDataBinding;
            container.Controls.Add(hid);
        }

        /// <summary>
        /// Constant value for hidden field.
        /// <para>Special processing applys for a field with an id of "editlangauges".  This field will restructure the XMLData field in the DB to be a multiple langauge structure, use a CSV list of cultureCode specify specific cultures or use "*" to use all valid CMS lanaguges.</para>
        /// <para>A const with the id of "staticlangfields" will make the field the same across all langauges. Enter a CSV list of xpath values to specify the fields.</para>
        /// <para>[<tag id="Thumbsize" type="const" value="82,50" />]</para>
        /// <para>[<tag id="staticlangfields" type="const" value="/genxml/textbox/txtculturecode,/genxml/textbox/txtmobiurl" />]</para>
        /// <para>[<tag id="editlangauges" type="const" value="*" />]</para>
        /// </summary>
        private static void CreateConst(Control container, XmlNode xmlNod)
        {
            var hid = GetHiddenFieldCtrl(xmlNod);
            container.Controls.Add(hid);
        }

        private static void CreateCultureCode(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.Text = Utils.GetCurrentCulture();
            container.Controls.Add(l);
        }
        /// <summary>
        /// This control is a asp hiddenfield, this allows a postback value to be returned to the server.
        /// The normal "hidden" type control in NBrightCore is a HtmlGenericField, which does not allow for a postback, when data is set by JS or JQuery.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="xmlNod"></param>
        private void CreatePostBack(Control container, XmlNode xmlNod)
        {
            var hid = GetPostBackCtrl(xmlNod);
            if (xmlNod.Attributes != null && (xmlNod.Attributes["persistance"] != null))
            {
                // we normally would not want to bind postback field, but is we want to use the slected value again in JS on the return
                // for example to populate a ajax driven ddl, the we can persist the data back to the client.
                if (xmlNod.Attributes["persistance"].InnerText.ToLower() == "true") hid.DataBinding += PostBackDataBinding;
            }
            container.Controls.Add(hid);
        }

        /// <summary>
        /// create a hidden field that represents the current culture code, usualy for ajax calls whcih don't have langauge context.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="xmlNod"></param>
        private void CreateCurrentCulture(Control container, XmlNode xmlNod)
        {
            var hid = new HtmlGenericControl("input");
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
                hid.ID = xmlNod.Attributes["id"].InnerXml.ToLower();
            else
                hid.Attributes.Add("id", "lang");
            hid.Attributes.Add("type", "hidden");
            hid.Attributes.Add("value", Utils.GetCurrentCulture());
            container.Controls.Add(hid);
        }
        

        private void CreateHidden(Control container, XmlNode xmlNod)
        {
            var hid = GetHiddenFieldCtrl(xmlNod);
            hid.DataBinding += HiddenDataBinding;
            container.Controls.Add(hid);
        }

        private void CreateTransButton(Control container, XmlNode xmlNod)
        {
            var cmd = GetLinkButtonCtrl(xmlNod);
            cmd.DataBinding += LinkButtonDataBinding;
            container.Controls.Add(cmd);

            var hid = GetHiddenFieldCtrl(xmlNod);
            hid.DataBinding += HiddenDataBinding;
            container.Controls.Add(hid);
        }


        #endregion

        #region "databind controls"

        /// <summary>
        /// Hidden field tag
        /// <para>A hidden field with the id of "lang" will be used as the langauge being edited.</para>
        /// <para>[<tag id="" type="hidden" const="true|false" value="" databind="" datatype="double|date" format="" class="" />]</para>
        /// <para>id : Unique id require for the page.</para>
        /// <para>value : default value.  the value is used as a varible that is saved in the data, if const property is used then the value will always be used.</para>        
        /// <para>const: Optional "true|false", uses this as the canstant value for the value property, the DB field will not replace the value field.</para>
        /// <para>databind: Optional, specify the data column of the repeater control.  "const" property will be ignored if this property is set. </para>
        /// <para>datatype: Optional, datatype of the data "double|date" </para>
        /// <para>format: Optional, format code of datatype. default is used if not specified. </para>
        /// <para>class: Optional, class name so field can be used easily vis jQuery. </para>
        /// <para>[<tag id="ItemID" class="itemid" type="hidden" value="" />]</para>
        /// <para>[<tag id="Thumbsize" const="true" type="hidden" value="82,50" />]</para>
        /// <para>[<tag id="ImageResize" const="true" type="hidden" value="600" />]</para>
        /// <para>[<tag id="lang" type="hidden" value="" />]</para>
        /// </summary>
        private void HiddenDataBinding(object sender, EventArgs e)
        {
            var hid = (HtmlGenericControl)sender;
            var container = (IDataItemContainer)hid.NamingContainer;
            try
            {
                hid.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (hid.Attributes["databind"] != null)
                {
                    hid.Attributes["value"] = Convert.ToString(DataBinder.Eval(container.DataItem, hid.Attributes["databind"]));                    
                }
                else if (hid.Attributes["value"].ToLower().StartsWith("databind:"))
                {
                    // check for legacy databind method on value
                    hid.Attributes["value"] = Convert.ToString(DataBinder.Eval(container.DataItem, hid.Attributes["value"].ToLower().Replace("databind:", "")));
                }
                else if (hid.Attributes["xpath"] != null)
                {
                    hid.Attributes["value"] = GenXmlFunctions.GetGenXmlValue(Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn)), hid.Attributes["xpath"]);
                }
                else
                {
                    if (hid.Attributes["const"] == null | hid.Attributes["const"] == "false")
                    {
                        hid.Attributes["value"] = GenXmlFunctions.GetGenXmLnode(hid.ID, "hidden", Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn))).InnerText;
                    }
                }


                if (hid.Attributes["datatype"] != null)
                {
                    var strFormat = "";
                    if (hid.Attributes["datatype"] == "double")
                    {
                        if (Utils.IsNumeric(hid.Attributes["value"]))
                        {
                            strFormat = "N";
                            if (hid.Attributes["format"] != null)
                            {
                                strFormat = hid.Attributes["format"];
                            }
                            //hid.Attributes["value"] = Convert.ToDouble(hid.Attributes["value"]).ToString(strFormat);                            
                            hid.Attributes["value"] = Utils.FormatToDisplay(hid.Attributes["value"], Utils.GetCurrentCulture(), TypeCode.Double, strFormat);
                        }
                    }
                    else if (hid.Attributes["datatype"] == "date")
                    {
                        if (Utils.IsDate(hid.Attributes["value"]))
                        {
                            strFormat = "d";
                            if (hid.Attributes["format"] != null)
                            {
                                strFormat = hid.Attributes["format"];
                            }
                            hid.Attributes["value"] = Convert.ToDateTime(hid.Attributes["value"]).ToString(strFormat);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        /// <summary>
        /// Deal with postack data handling is persistance is set to true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PostBackDataBinding(object sender, EventArgs e)
        {
            var hid = (HiddenField)sender;
            var container = (IDataItemContainer)hid.NamingContainer;
            try
            {
                hid.Visible = visibleStatus.DefaultIfEmpty(true).First();
                hid.Value = GenXmlFunctions.GetGenXmLnode(hid.ID, "hidden", Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn))).InnerText;
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        /// <summary>
        /// Label control tag
        /// <para>[<tag type="label" xpath="" length="" databind="" datatype="" format="" />]</para>
        /// <para>xpath: Optional, specify the xpath in the genxml structure.  Must be specified if databind is not used.</para>
        /// <para>databind: Optional, specify the data column of the repeater control.  Must be specified if xpath is not used. </para>
        /// <para>length: Optional, Length of display field.</para>
        /// <para>datatype: Optional, datatype of the data "double|date" </para>
        /// <para>format: Optional, format code of datatype. default is used if not specified. </para>
        /// <para>Asp : Asp.net properties created by reflection. </para>
        /// <para>[<tag type="label" xpath="genxml/textbox/txtclientname" length="20" />]</para>
        /// </summary>
        private void LabelDataBinding(object sender, EventArgs e)
        {
            var hid = (Label)sender;
            var container = (IDataItemContainer)hid.NamingContainer;
            try
            {
                hid.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (container.DataItem != null && DataBinder.Eval(container.DataItem, DatabindColumn) != null)
                {
                    if ((hid.Attributes["databind"] != null))
                    {
                        hid.Text = Convert.ToString(DataBinder.Eval(container.DataItem, hid.Attributes["databind"]));
                    }
                    else
                    {
                        var nod = GenXmlFunctions.GetGenXmLnode(hid.ID, hid.Text, Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn)));
                        if ((nod != null))
                        {
                            hid.Text = nod.InnerText;
                        }
                        else
                        {
                            nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), hid.Text);
                            if (nod != null)
                            {
                                hid.Text = nod.InnerText;
                            }
                        }
                    }
                }

                if (hid.Attributes["datatype"] != null)
                {
                    var strFormat = "";
                    if (hid.Attributes["datatype"] == "double")
                    {
                        if (Utils.IsNumeric(hid.Attributes["value"]))
                        {
                            if (hid.Attributes["format"] != null)
                            {
                                strFormat = hid.Attributes["format"];
                            }
                            hid.Text = Utils.FormatToDisplay(hid.Text, Utils.GetCurrentCulture(), TypeCode.Double, strFormat);
                        }
                    }
                    else if (hid.Attributes["datatype"] == "date")
                    {
                        if (Utils.IsDate(hid.Attributes["value"]))
                        {
                            strFormat = "d";
                            if (hid.Attributes["format"] != null)
                            {
                                strFormat = hid.Attributes["format"];
                            }
                            hid.Text = Utils.FormatToDisplay(hid.Text, Utils.GetCurrentCulture(), TypeCode.DateTime, strFormat);
                        }
                    }
                }

                if ((hid.Attributes["length"] != null))
                {
                    hid.Text = hid.Text.Substring(0, Convert.ToInt32(hid.Attributes["length"]));
                }

            }
            catch (Exception)
            {
                //do nothing
            }
        }


        private void ValueOfDataBinding(object sender, EventArgs e)
        {
            // NOTE: Do not set Text = "", If we've assign a Text value in the template (or resourcekey) then use it as default. (unless Error)
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                // check if we have any formatting to do
                var strFormat = "";
                var strFormatType = "";
                var strFormatCultureCode = "";
                var xPath = lc.Text;
                if (lc.Text.ToLower().StartsWith("date:"))
                {
                    var strF = lc.Text.Split(':');
                    if (strF.Length == 3)
                    {
                        strFormat = strF[1].Replace("**COLON**", ":");
                        strFormatType = "date";
                        xPath = strF[2];
                    }
                }
                if (lc.Text.ToLower().StartsWith("double:"))
                {
                    var strF = lc.Text.Split(':');
                    if (strF.Length == 3)
                    {
                        var f = strF[1].Replace("**COLON**", ":").Split(',');
                        if (f.Length == 2) strFormatCultureCode = f[1];
                        strFormat = f[0];
                        strFormatType = "double";
                        xPath = strF[2];
                    }
                }

                if (lc.Text.ToLower().StartsWith("resxdata:"))
                {
                    //Text data passed as resx, so display it.
                    lc.Text = lc.Text.Replace("resxdata:", "");
                }
                else
                {
                    //Get Data or set to empty is no data exits
                    if (container.DataItem != null)
                    {
                        if (lc.Text.ToLower().StartsWith("databind:"))
                        {
                            lc.Text =
                                Convert.ToString(DataBinder.Eval(container.DataItem,
                                    lc.Text.ToLower().Replace("databind:", "")));
                        }
                        else
                        {
                            XmlNode nod =
                                GenXmlFunctions.GetGenXmLnode(
                                    DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), xPath);
                            if ((nod != null))
                            {
                                lc.Text = XmlConvert.DecodeName(nod.InnerText);
                            }
                            else
                            {
                                lc.Text = "";
                            }
                        }
                    }

                }


                //Do the formatting
                if (strFormatCultureCode == "") strFormatCultureCode = Utils.GetCurrentCulture();
                if (strFormatType == "date")
                {
                    lc.Text = Utils.FormatToDisplay(lc.Text, strFormatCultureCode, TypeCode.DateTime, strFormat);
                }
                if (strFormatType == "double")
                {
                    lc.Text = Utils.FormatToDisplay(lc.Text, strFormatCultureCode, TypeCode.Double, strFormat);
                }

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        private void AssignOfDataBinding(object sender, EventArgs e)
        {
            // Output data, but eacape "'" to "&apos;", this is so string values can be assign in javascript.
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                // check if we have any formatting to do
                var xPath = lc.Text;
                //Get Data or set to empty is no data exits
                if (container.DataItem != null)
                {
                    if (lc.Text.ToLower().StartsWith("databind:"))
                    {
                        lc.Text = Convert.ToString(DataBinder.Eval(container.DataItem, lc.Text.ToLower().Replace("databind:", "")));
                    }
                    else
                    {
                        XmlNode nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), xPath);
                        if ((nod != null))
                        {
                            lc.Text = XmlConvert.DecodeName(nod.InnerText);
                            if (lc.Text != null) lc.Text = lc.Text.Replace("'", "&apos;");
                        }
                        else
                        {
                            lc.Text = "";
                        }
                    }
                }
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        private void ChkBoxListOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;

            try
            {
                var s = lc.Text.Split(';');
                var xpath = s[0];
                var displaytype = "";
                if (s.Length > 1) displaytype = s[1]; 
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var xmlNod = GenXmlFunctions.GetGenXmLnode((string)DataBinder.Eval(container.DataItem, DatabindColumn), s[0]);
                lc.Text = "";
                var xmlNodeList = xmlNod.SelectNodes("./chk");
                if (xmlNodeList != null)
                {
                    if (displaytype == "htmllist") lc.Text += "<ul>";
                    foreach (XmlNode xmlNoda in xmlNodeList)
                    {
                        if (xmlNoda.Attributes != null && xmlNoda.Attributes["value"] != null)
                        {
                            if (displaytype == "")
                            {
                                if (xmlNoda.Attributes["value"].Value.ToLower() == "true")
                                {
                                    lc.Text += "[X] " + xmlNoda.InnerText + "<br/>";
                                }
                                else
                                {
                                    lc.Text += "[_] " + xmlNoda.InnerText + "<br/>";
                                }
                            }
                            else if (displaytype == "htmllist")
                            {
                                if (xmlNoda.Attributes["value"].Value.ToLower() == "true")
                                {
                                    lc.Text += "<li>" + xmlNoda.InnerText + "</li>";
                                }
                            }
                            else
                            {
                                if (xmlNoda.Attributes["value"].Value.ToLower() == "true")
                                {
                                    lc.Text += xmlNoda.InnerText + ", ";
                                }
                            }

                        }
                    }
                    if (displaytype == "htmllist") lc.Text += "</ul>";
                    // remove the html list if we don;t have anything selected
                    if (lc.Text == "<ul></ul>") lc.Text = "";
                }
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }


        private void BreakOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;

            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (lc.Text.ToLower().StartsWith("databind:"))
                {
                    lc.Text =Convert.ToString(DataBinder.Eval(container.DataItem, lc.Text.ToLower().Replace("databind:", "")));
                }
                else
                {
                    if (lc.Text.ToLower().StartsWith("resxdata:"))
                    {
                        //Text data passed as resx, so display it.
                        lc.Text = lc.Text.Replace("resxdata:", "");
                    }
                    else
                    {
                        var nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), lc.Text);
                        if ((nod != null))
                        {
                            lc.Text = nod.InnerText;
                        }
                        else
                        {
                            lc.Text = "";
                        }                        
                    }
                }
                lc.Text = System.Web.HttpUtility.HtmlEncode(lc.Text);
                lc.Text = lc.Text.Replace(Environment.NewLine, "<br/>");
                lc.Text = lc.Text.Replace("\t", "&nbsp;&nbsp;&nbsp;");
                lc.Text = lc.Text.Replace("'", "&apos;");

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        private void ListOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;

            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var strXml = DataBinder.Eval(container.DataItem, DatabindColumn).ToString();
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strXml);
                var xpath = lc.Text;
                lc.Text = "";
                var nodList = xmlDoc.SelectNodes(xpath);
                foreach (XmlNode nod in nodList)
                {
                    lc.Text += "<li>" + HttpUtility.HtmlEncode(nod.InnerText).Replace("'", "&apos;") + "</li>";                    
                }

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        private void HtmlOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (lc.Text.ToLower().StartsWith("resxdata:"))
                {
                    //Text data passed as resx, so display it.
                    lc.Text = System.Web.HttpUtility.HtmlDecode(lc.Text.Replace("resxdata:", ""));
                }
                else
                {

                    if (lc.Text.ToLower().StartsWith("databind:"))
                    {
                        lc.Text = System.Web.HttpUtility.HtmlDecode(Convert.ToString(DataBinder.Eval(container.DataItem, lc.Text.ToLower().Replace("databind:", ""))));
                    }
                    else
                    {
                        var nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), lc.Text);
                        if ((nod != null))
                        {
                            lc.Text = System.Web.HttpUtility.HtmlDecode(nod.InnerText);
                        }
                        else
                        {
                            lc.Text = "";
                        }
                    }
                }
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        private void EncodeOfDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (lc.Text.ToLower().StartsWith("databind:"))
                {
                    lc.Text = System.Web.HttpUtility.HtmlDecode(Convert.ToString(DataBinder.Eval(container.DataItem, lc.Text.ToLower().Replace("databind:", ""))));
                }
                else
                {
                    var nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), lc.Text);
                    if ((nod != null))
                    {
                        lc.Text = System.Web.HttpUtility.HtmlEncode(nod.InnerText);
                    }
                    else
                    {
                        lc.Text = "";
                    }
                }
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }


        private void LinkButtonDataBinding(object sender, EventArgs e)
        {
            var cmd = (LinkButton)sender;
            var container = (IDataItemContainer)cmd.NamingContainer;
            try
            {
                cmd.Visible = visibleStatus.DefaultIfEmpty(true).First();

                if (cmd.Text.ToLower().StartsWith("databind:"))
                {
                    // If using for repeated linkbutton, we can datbind the text (e.g. for paging) 
                    if ((DataBinder.Eval(container.DataItem, cmd.Text.Replace("databind:","")) != null))
                    {
                        //dataitem value matching commandarg name 
                        cmd.Text = Convert.ToString(DataBinder.Eval(container.DataItem, cmd.Text.Replace("databind:", "")));
                    }                    
                }

                if ((DataBinder.Eval(container.DataItem, cmd.CommandArgument) != null))
                {
                    //dataitem value matching commandarg name 
                    cmd.CommandArgument =Convert.ToString(DataBinder.Eval(container.DataItem, cmd.CommandArgument));
                }
                else
                {
                    //no value in dataitem matching commandarg name so search xml values
                    var nod = GenXmlFunctions.GetGenXmLnode(cmd.ID, cmd.Text,Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn)));
                    if ((nod != null))
                    {
                        cmd.CommandArgument = nod.InnerXml;
                    }
                    else
                    {
                        nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), cmd.Text);
                        if (nod != null)
                        {
                            cmd.CommandArgument = nod.InnerXml;
                        }
                        else
                        {
                            cmd.CommandArgument = "";
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }


        private void LiteralDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
        }

        private void GeneralDataBinding(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;

            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                lc.Text = Convert.ToString(DataBinder.Eval(container.DataItem, lc.Text));
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void RblDataBinding(object sender, EventArgs e)
        {

            var rbl = (RadioButtonList)sender;
            var container = (IDataItemContainer)rbl.NamingContainer;

            try
            {
                rbl.Visible = visibleStatus.DefaultIfEmpty(true).First();
                string strValue;
                if ((rbl.Attributes["databind"] != null))
                {
                    strValue = Convert.ToString(DataBinder.Eval(container.DataItem, rbl.Attributes["databind"]));
                }
                else
                {
                    strValue = GenXmlFunctions.GetGenXmLnode(rbl.ID, "radiobuttonlist",Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn))).InnerText;
                }
                if ((rbl.Items.FindByValue(strValue) != null))
                {
                    rbl.SelectedValue = strValue;
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void ChkBDataBinding(object sender, EventArgs e)
        {
            var chk = (CheckBoxList)sender;
            var container = (IDataItemContainer) chk.NamingContainer;

            try
            {
                chk.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var xmlNod = GenXmlFunctions.GetGenXmLnode(chk.ID, "checkboxlist", (string)DataBinder.Eval(container.DataItem, DatabindColumn));
                var xmlNodeList = xmlNod.SelectNodes("./chk");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode xmlNoda in xmlNodeList)
                    {
                        if (xmlNoda.Attributes != null)
                        {
                            if (xmlNoda.Attributes.GetNamedItem("data") != null)
                            {
                                var datavalue = xmlNoda.Attributes["data"].Value;
                                //use the data attribute if there
                                if ((chk.Items.FindByValue(datavalue) != null))
                                {
                                    chk.Items.FindByValue(datavalue).Selected =
                                        Convert.ToBoolean(xmlNoda.Attributes["value"].Value);
                                }
                            }
                            else
                            {
                                // use the text or value, if no data att exists (backward compatibility)
                                var findName = xmlNoda.Value;
                                if (string.IsNullOrEmpty(findName))
                                {
                                    findName = xmlNoda.InnerText;
                                    if ((chk.Items.FindByText(findName) != null && chk.Items.FindByText(findName).Value != null))
                                    {
                                        chk.Items.FindByText(findName).Selected =
                                            Convert.ToBoolean(xmlNoda.Attributes["value"].Value);
                                    }
                                }
                                else
                                {
                                    if ((chk.Items.FindByText(findName) != null && chk.Items.FindByValue(findName).Value != null))
                                    {
                                        chk.Items.FindByValue(findName).Selected =
                                            Convert.ToBoolean(xmlNoda.Attributes["value"].Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void ChkBoxDataBinding(object sender, EventArgs e)
        {
            var chk = (CheckBox)sender;
            var container = (IDataItemContainer)chk.NamingContainer;

            try
            {
                chk.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if ((chk.Attributes["databind"] != null))
                {
                    chk.Checked = Convert.ToBoolean(Convert.ToString(DataBinder.Eval(container.DataItem, chk.Attributes["databind"])));
                }
                else
                {
                    chk.Checked = Convert.ToBoolean(GenXmlFunctions.GetGenXmlValue(chk.ID, "checkbox",Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn))));
                }

            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void DdListDataBinding(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;

            try
            {
                ddl.Visible = visibleStatus.DefaultIfEmpty(true).First();
                string strValue;
                if ((ddl.Attributes["databind"] != null))
                {
                    strValue = Convert.ToString(Convert.ToString(DataBinder.Eval(container.DataItem, ddl.Attributes["databind"])));
                }
                else
                {
                    strValue = GenXmlFunctions.GetGenXmlValue(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn)));
                }

                if ((ddl.Items.FindByValue(strValue) != null))
                {
                        ddl.SelectedValue = strValue;                        
                }
                else
                {
                    var nod = GenXmlFunctions.GetGenXmLnode(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn)));
                    if (nod != null && (nod.Attributes != null) && (nod.Attributes["selectedtext"] != null))
                    {
                            strValue = XmlConvert.DecodeName(nod.Attributes["selectedtext"].Value);                            
                            if ((ddl.Items.FindByValue(strValue) != null))
                            {
                                ddl.SelectedValue = strValue;
                            }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void TextDataBinding(object sender, EventArgs e)
        {
            var txt = (TextBox)sender;
            var container = (IDataItemContainer)txt.NamingContainer;

            try
            {
                txt.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (txt.Width == 0) txt.Visible = false; // always hide if we have a width of zero.
                if ((txt.Attributes["databind"] != null))
                {
                    txt.Text = Convert.ToString(DataBinder.Eval(container.DataItem, txt.Attributes["databind"]));
                    if (txt.Text.Contains("**CDATASTART**"))
                    {
                        //convert back cdata marks converted so it saves OK into XML 
                        txt.Text = txt.Text.Replace("**CDATASTART**", "<![CDATA[");
                        txt.Text = txt.Text.Replace("**CDATAEND**", "]]>");
                    }
                }
                else
                {
                    var strData = GenXmlFunctions.GetGenXmlValue(txt.ID, "textbox", Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn)));
                    if (txt.Text == "")
                    {
                        txt.Text = strData;
                    }
                    else
                    {
                        if (strData != "") txt.Text = strData;
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        #endregion

        #region "button"

        private void CreateButton(Control container, XmlNode xmlNod)
        {
            var cmd = new Button();
            cmd = (Button)GenXmlFunctions.AssignByReflection(cmd, xmlNod);

            cmd.Visible = GetRoleVisible(xmlNod.OuterXml);
            cmd.Enabled = GetRoleEnabled(xmlNod.OuterXml);

            cmd.DataBinding += ButtonDataBinding;
            container.Controls.Add(cmd);
        }

        private void ButtonDataBinding(object sender, EventArgs e)
        {
            var cmd = (Button)sender;
            var container = (IDataItemContainer)cmd.NamingContainer;
            try
            {
                cmd.Visible = visibleStatus.DefaultIfEmpty(true).First();

                if (cmd.Text.ToLower().StartsWith("databind:"))
                {
                    // If using for repeated linkbutton, we can datbind the text (e.g. for paging) 
                    if ((DataBinder.Eval(container.DataItem, cmd.Text.Replace("databind:", "")) != null))
                    {
                        //dataitem value matching commandarg name 
                        cmd.Text = Convert.ToString(DataBinder.Eval(container.DataItem, cmd.Text.Replace("databind:", "")));
                    }
                }

                if ((DataBinder.Eval(container.DataItem, cmd.CommandArgument) != null))
                {
                    //dataitem value matching commandarg name 
                    cmd.CommandArgument = Convert.ToString(DataBinder.Eval(container.DataItem, cmd.CommandArgument));
                }
                else
                {
                    //no value in dataitem matching commandarg name so search xml values
                    var nod = GenXmlFunctions.GetGenXmLnode(cmd.ID, cmd.Text, Convert.ToString(DataBinder.Eval(container.DataItem, DatabindColumn)));
                    if ((nod != null))
                    {
                        cmd.CommandArgument = nod.InnerXml;
                    }
                    else
                    {
                        nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, DatabindColumn).ToString(), cmd.Text);
                        if (nod != null)
                        {
                            cmd.CommandArgument = nod.InnerXml;
                        }
                        else
                        {
                            cmd.CommandArgument = "";
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }



        #endregion

        #region "General Methods"


        private static bool GetRoleEnabled(string xmLproperties)
        {
            var genprop = GenXmlFunctions.GetGenControlPropety(xmLproperties, "editinrole");
            if (genprop == "" | genprop == null)
            {
                return true;
            }

            // Call CMS Security Interface 
            return providers.CmsProviderManager.Default.IsInRole(genprop);
        }

        private static bool GetRoleVisible(string xmLproperties)
        {
            var genprop = GenXmlFunctions.GetGenControlPropety(xmLproperties, "viewinrole");
            if (genprop == "" | genprop == null)
            {
                return true;
            }

            // Call CMS Security Interface 
            return providers.CmsProviderManager.Default.IsInRole(genprop);
            
        }

		private XmlNode GetCMSResourceData(XmlDocument xmlDoc)
		{
			var xmlNod = xmlDoc.SelectSingleNode("root/tag");
            if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["resourcekey"] != null || xmlNod.Attributes["resourcekeysave"] != null)))
			{
                if (_ResourcePath != null && _ResourcePath.Count > 0)
				{
                    foreach (var r in _ResourcePath)
                    {
                        //add resource attribuutes to tag xml node.
                        try
                        {
                            var resourcekey = "";
                            if (xmlNod.Attributes["resourcekey"] != null) resourcekey  = xmlNod.Attributes["resourcekey"].Value;
                            if (resourcekey == "" && xmlNod.Attributes["resourcekeysave"] != null) resourcekey = xmlNod.Attributes["resourcekeysave"].Value; // save key is for updating resx file
                            var lang = EditCultureCode;
                            if (xmlNod.Attributes["lang"] != null) lang = xmlNod.Attributes["lang"].Value;
                            var rList = providers.CmsProviderManager.Default.GetResourceData(r, resourcekey, lang);

                            foreach (var i in rList)
                            {
                                var aNod = xmlDoc.CreateAttribute(i.Key);
                                aNod.Value = i.Value;
                                xmlNod.Attributes.Append(aNod);
                            }                        
                        }
                        catch (Exception)
                        {
                            //ignore the theme/folder may have been removed.
                        }
                    }

                    var rNod = xmlNod.Attributes["resourcekey"];
                    xmlNod.Attributes.Remove(rNod);

				}
			}
			return xmlNod;
		}        

        #endregion

    }

}
