using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DotNetNuke.Common.Utilities;
using NBrightCore.common;
using NBrightCore.render;

namespace NBrightDNN
{
    public class UserDataInfo
    {
        private NBrightInfo _obj;
        private DataCtrlInterface _objCtrl;

        public string UserDataKey { set; get; }
        public int PortalId { set; get; }
        public int ModuleId { set; get; }

        public UserDataInfo()
        {
            //default constructor for serialization
        }

        public UserDataInfo(int portalId, int moduleId, DataCtrlInterface objCtrl, string ctrlTypeCode)
        {
            _objCtrl = objCtrl;
            PortalId = portalId;
            ModuleId = moduleId;
            SearchPortalId = portalId;
			SearchModuleId = 0; // don't auto link search moduledid to module. (This is done in the "EventBeforeBindListData" event)
            CtrlTypeCode = ctrlTypeCode;

            UserDataKey = Cookie.GetCookieValue(portalId, "UserDataInfo", "UserDataKey", moduleId.ToString(""));
            if (UserDataKey != "")
            {
                var strFilter = " and guidkey = '" + UserDataKey + "' ";
                var l = objCtrl.GetList(portalId, moduleId, "USERDATAINFO", strFilter, "", 1);
                if (l.Count >= 1)
                {
                    _obj = l[0];
                }
                if (_obj == null)
                {
                    CreateNewUserDataInfoRecord();
                }
            }
            else
            {
                CreateNewUserDataInfoRecord();
            }

            if (_obj == null) return;

            ItemId = _obj.ItemID;
            GUIDKey = _obj.GUIDKey;
            var s = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/current/tabid");
            if (Utils.IsNumeric(s)) TabId = Convert.ToInt32(s);
            else TabId = -1;

            SkinSrc = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/current/skinsrc");
            EntityTypeCode = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/current/entitytypecode");
            EntityTypeCodeLang = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/current/entitytypecodelang");
            RtnSelUrl = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/current/rtnselurl");
            RtnUrl = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/current/rtnurl");
            FromItemId = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/current/fromitemid");
            SelItemId = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/current/selitemid");
            SelType = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/current/seltype");
            SortItemId = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/current/sortitemid");

            if (CtrlTypeCode != null)
            {
                SearchClearAfter = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/" + CtrlTypeCode.ToLower() + "/searchclearafter");
                SearchExtra1 = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/" + CtrlTypeCode.ToLower() + "/searchextra1");
                SearchExtra2 = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/" + CtrlTypeCode.ToLower() + "/searchextra2");
                SearchFilters = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/" + CtrlTypeCode.ToLower() + "/searchfilters");
                SearchOrderby = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/" + CtrlTypeCode.ToLower() + "/searchorderby");
                SearchPageNumber = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/" + CtrlTypeCode.ToLower() + "/searchpagenumber");
                SearchReturnLimit = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/" + CtrlTypeCode.ToLower() + "/searchreturnlimit");
                SearchDate1 = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/" + CtrlTypeCode.ToLower() + "/searchsearchdate1");
                SearchDate2 = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/" + CtrlTypeCode.ToLower() + "/searchsearchdate2");

                var strSearchPortalId = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/" + CtrlTypeCode.ToLower() + "/searchportalid");
                if (Utils.IsNumeric(strSearchPortalId) && (Convert.ToInt32(strSearchPortalId) > 0))
                {
                    SearchPortalId = Convert.ToInt32(strSearchPortalId);
                }
                var strSearchModuleId = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/" + CtrlTypeCode.ToLower() + "/searchmoduleid");
                if (Utils.IsNumeric(strSearchModuleId) && (Convert.ToInt32(strSearchModuleId) > 0 | Convert.ToInt32(strSearchModuleId) == -1))
                {
                    SearchModuleId = Convert.ToInt32(strSearchModuleId);
                }

                var xmlNod = GenXmlFunctions.GetGenXmLnode(_obj.XMLData, "root/" + CtrlTypeCode.ToLower() + "/searchgenxml");
                if (xmlNod != null)
                {
                    SearchGenXml = xmlNod.InnerXml;
                }
                else
                {
                    SearchGenXml = "";
                }
            }

            var d = new XmlDocument();
            s = GenXmlFunctions.GetGenXmlValue(_obj.XMLData, "root/extraxml");
            if (s != "") d.LoadXml(s);
            ExtraXml = d;
        }

        #region "properties"

        public string SearchGenXml { set; get; }
        public string SearchFilters { set; get; }
        public string SearchOrderby { set; get; }
        public string SearchReturnLimit { set; get; }
        public string SearchPageNumber { set; get; }
        public string SearchPageSize { set; get; }
        public string SearchClearAfter { set; get; }
        public string SearchExtra2 { set; get; }
        public string SearchExtra1 { set; get; }
        public string SearchDate2 { set; get; }
        public string SearchDate1 { set; get; }
        public int SearchPortalId { set; get; }
        public int SearchModuleId { set; get; }
        public int UserId { set; get; }
        public int TabId { set; get; }
        public string SkinSrc { set; get; }
        public string EntityTypeCode { set; get; }
        public string EntityTypeCodeLang { set; get; }
        public string CtrlTypeCode { set; get; }
        public string RtnSelUrl { set; get; }
        public string RtnUrl { set; get; }
        public string FromItemId { set; get; }
        public string SelItemId { set; get; }
        public string SelType { set; get; }
        public int ItemId { set; get; }
        public string GUIDKey { set; get; }
        public string SortItemId { set; get; }

        public XmlDocument ExtraXml { set; get; }

        #endregion

        #region "Public methods"

        public void Save()
        {
            if (UserId >= 0) // only save this to DB if it's a valid user.  We don;t want robots creating records in DB.
            {
                if (!String.IsNullOrEmpty(CtrlTypeCode))
                {

                    var strXml = "<root>";

                    strXml += "<current>";
                    strXml += "<userid>" + UserId.ToString("") + "</userid>";
                    strXml += "<tabid>" + TabId.ToString("") + "</tabid>";
                    strXml += "<moduleid>" + ModuleId.ToString("") + "</moduleid>";
                    strXml += "<skinsrc><![CDATA[" + SkinSrc + "]]></skinsrc>";
                    strXml += "<entitytypecode>" + EntityTypeCode + "</entitytypecode>";
                    strXml += "<entitytypecodelang>" + EntityTypeCodeLang + "</entitytypecodelang>";
                    strXml += "<ctrltypecode>" + CtrlTypeCode + "</ctrltypecode>";
                    strXml += "<rtnselurl><![CDATA[" + RtnSelUrl + "]]></rtnselurl>";
                    strXml += "<rtnurl><![CDATA[" + RtnUrl + "]]></rtnurl>";
                    strXml += "<fromitemid>" + FromItemId + "</fromitemid>";
                    strXml += "<selitemid>" + SelItemId + "</selitemid>";
                    strXml += "<seltype>" + SelType + "</seltype>";
                    strXml += "<sortitemid>" + SortItemId + "</sortitemid>";
                    strXml += "</current>";

                    strXml += "<" + CtrlTypeCode.ToLower() + ">";

                    strXml += "<searchclearafter>" + SearchClearAfter + "</searchclearafter>";
                    strXml += "<searchextra1>" + SearchExtra1 + "</searchextra1>";
                    strXml += "<searchextra2>" + SearchExtra2 + "</searchextra2>";
                    strXml += "<searchfilters><![CDATA[" + SearchFilters + "]]></searchfilters>";
                    strXml += "<searchgenxml>" + SearchGenXml + "</searchgenxml>";
                    strXml += "<searchorderby><![CDATA[" + SearchOrderby + "]]></searchorderby>";
                    strXml += "<searchpagenumber>" + SearchPageNumber + "</searchpagenumber>";
                    strXml += "<searchpagesize>" + SearchPageSize + "</searchpagesize>";
                    strXml += "<searchreturnlimit>" + SearchReturnLimit + "</searchreturnlimit>";
                    strXml += "<searchsearchdate1>" + SearchDate1 + "</searchsearchdate1>";
                    strXml += "<searchsearchdate2>" + SearchDate2 + "</searchsearchdate2>";
                    strXml += "<searchportalid>" + SearchPortalId + "</searchportalid>";
                    strXml += "<searchmoduleid>" + SearchModuleId + "</searchmoduleid>";

                    strXml += "</" + CtrlTypeCode.ToLower() + ">";

                    strXml += "<extraxml>";
                    if (ExtraXml != null)
                    {
                        strXml += ExtraXml.OuterXml;
                    }
                    strXml += "</extraxml>";

                    strXml += "</root>";

                    if (String.IsNullOrEmpty(_obj.XMLData))
                    {
                        _obj.XMLData = strXml;
                    }
                    else
                    {
                        // merge current
                        _obj.ReplaceXmlNode(strXml, "root/current", "root");

                        // merge search data
                        _obj.ReplaceXmlNode(strXml, "root/" + CtrlTypeCode.ToLower(), "root");

                        // merge extra xml data
                        _obj.ReplaceXmlNode(strXml, "root/extraxml", "root");
                    }

                    // create new userdatakey if needed.
                    if (String.IsNullOrEmpty(_obj.GUIDKey))
                    {
                        UserDataKey = Guid.NewGuid().ToString("");
                        _obj.GUIDKey = UserDataKey;
                        _obj.ItemID = -1;
                        // Cookie does not exists, so create.
                        Cookie.SetCookieValue(PortalId, "UserDataInfo", "UserDataKey", UserDataKey,
                                              ModuleId.ToString(""));
                    }

                    _obj.ItemID = _objCtrl.Update(_obj);
                    ItemId = _obj.ItemID;
                }
            }
        }

        public void ClearSearchData()
        {
            SearchClearAfter = "";
            SearchExtra1 = "";
            SearchExtra2 = "";
            SearchFilters = "";
            SearchGenXml = "";
            SearchOrderby = "";
            SearchPageNumber = "";
            SearchPageSize = "25";
            SearchReturnLimit = "";
            SearchDate1 = "";
            SearchDate2 = "";
            SearchPortalId = -1;
            SearchModuleId = 0;
            SortItemId = "";
            Save();
        }

        public void PurgeRecords(int purgeDays = -7)
        {
            //Remove unrequired USERDATAINFO
            NBrightDNN.DnnUtils.PurgeDataBaseInfo(-1, -1, _objCtrl, "USERDATAINFO", purgeDays);
        }

        #endregion

        #region "private methods"

        private void CreateNewUserDataInfoRecord()
        {
            _obj = new NBrightInfo();
            _obj.ItemID = -1;
            _obj.PortalId = PortalId;
            _obj.ModuleId = ModuleId;
            _obj.GUIDKey = UserDataKey;
            _obj.TypeCode = "USERDATAINFO";
            // Do NOT update here, it creates blank records with no data.
            //  On devices/PC that don;t save cookies this creates multiple blank records.
            //_obj.ItemID = _objCtrl.Update(_obj);
        }

        #endregion

    }

}
