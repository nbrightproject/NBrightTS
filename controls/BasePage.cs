using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Common;
using NBrightCore.common;
using NBrightCore.render;

namespace NBrightDNN.controls
{
    /// <summary>
    /// This class is kept for backward compatiblity with the NBrightEspace and NBrightGen projects.
    /// THIS CLASS SHOULD NOT BE USED FOR NEW PROJECT.  It's trying to do too much to be used across multiple projects,
    /// better practise would be to create a local basepage class in the your project and not use this. 
    /// </summary>
    [System.Obsolete("Kept for backward compatiblity and will be removed in future.")]
    public class BasePage : DotNetNuke.Entities.Modules.PortalModuleBase
    {
        public DataCtrlInterface ObjCtrl { get; set; }

        protected const string EncrytionKey = "";

        protected NBrightCore.controls.PagingCtrl CtrlPaging;
        protected Repeater CtrlSearch;
        protected Repeater CtrlList;
		protected LiteralControl CtrlListMsg;
        private bool _activatePaging = false;

        protected UserDataInfo UInfo;

        protected NBrightCore.TemplateEngine.TemplateGetter TemplCtrl;

        private string _entityTypeCode;

        public string EntityTypeCode
        {
            get { return _entityTypeCode; }
            set
            {
                _entityTypeCode = value;
                if (String.IsNullOrEmpty(CtrlTypeCode))
                {
                    CtrlTypeCode = _entityTypeCode;
                }
            }
        }

        public string PrimaryTemplateMapPath { get; set; }
        public string SecondaryTemplateMapPath { get; set; }
        public string TemplateThemeFolder { get; set; }
        public string EntityLangauge { get; set; }
        public string CtrlTypeCode { get; set; }
        public string EntityTypeCodeLang { get; set; }
        public string ItemId { get; set; }
        public string ItemIdLang { get; set; }
        public String ControlAdminPath { get; set; }
		public String ControlAdminIncludePath { get; set; }
		public List<NBrightInfo> OverRideInfoList { get; set; }
        public String OverRideWebserviceUrl { get; set; }  //used to pass webserice to parent, so we use the webservice on a OnLoad event.
        public Boolean DisablePaging { get; set; } // disable the paging control
        public Boolean DisableUserInfo { get; set; } // disable Saving of UserInfoData to DB
        
		//// Debug code for cache improvement timing: REMOVE FOR BUILD
		//public String NBrightLogTrace = "";
		//public long NBrightLogStartTick;
		//public long NBrightLogEndTick;
		//public long NBrightLogElapsedTick;
		//// Debug code for cache improvement timing: REMOVE FOR BUILD

        public bool FileHasBeenUploaded = false;

        #region "Page Events"

        protected override void OnInit(EventArgs e)
        {
			//// Debug code for cache improvement timing: REMOVE FOR BUILD
			//NBrightLogTrace = NBrightCore.common.Utils.ReadFile(PortalSettings.HomeDirectoryMapPath + "\\NBrightLogTrace.txt");
			//NBrightLogStartTick = DateTime.Now.Ticks;
			//// Debug code for cache improvement timing: REMOVE FOR BUILD

            base.OnInit(e);

            
            // Attach events
            //NOTE: this event has been removed to stop memory leak (hold on memory) DO NOT PUT IT BACK!!!!
            //GenXmlFunctions.FileHasBeenUploaded += new UploadFileCompleted(OnFileUploaded);

            OverRideInfoList = null;

            if (String.IsNullOrEmpty(ControlAdminPath)) ControlAdminPath = ControlPath;
                
            UInfo = new UserDataInfo(PortalId, ModuleId, ObjCtrl,CtrlTypeCode);

            EntityLangauge = Utils.RequestQueryStringParam(Context, "lang");
            if (EntityLangauge.Length != 5) EntityLangauge = Utils.GetCurrentCulture();
            //make sure we have a valid culture code in upper and lower case. (url re-writers can make all url lowercase) (none is the default editing langauge for templates/admin content)
            if (EntityLangauge != "none") EntityLangauge = EntityLangauge.Substring(0, 2).ToLower() + "-" + EntityLangauge.Substring(3, 2).ToUpper();

            //get the ItemId
            ItemId = Utils.RequestQueryStringParam(Context, "itemid");

            //get the langauge ItemId
            ItemIdLang = Utils.RequestQueryStringParam(Context, "itemidlang");
            if (ItemIdLang == "" && ItemId != "")
            {
                ItemIdLang = "0";
                var obj = GetDataLang();
                if (obj != null) ItemIdLang = obj.ItemID.ToString();
            }

            CtrlSearch = new Repeater();
            this.Controls.Add(CtrlSearch);

			CtrlListMsg = new LiteralControl();
			this.Controls.Add(CtrlListMsg);
			CtrlListMsg.Visible = false;

            CtrlList = new Repeater();
            this.Controls.Add(CtrlList);

            if (!DisablePaging)
            {
                CtrlPaging = new NBrightCore.controls.PagingCtrl();
                this.Controls.Add(CtrlPaging);

                CtrlList.ItemCommand += new RepeaterCommandEventHandler(CtrlListItemCommand);
                CtrlSearch.ItemCommand += new RepeaterCommandEventHandler(CtrlSearchItemCommand);
                CtrlPaging.PageChanged += new RepeaterCommandEventHandler(PagingClick);                
            }


            if (String.IsNullOrEmpty(PrimaryTemplateMapPath)) PrimaryTemplateMapPath = PortalSettings.HomeDirectoryMapPath;
            if (String.IsNullOrEmpty(SecondaryTemplateMapPath)) SecondaryTemplateMapPath = MapPath(ControlAdminPath);
            if (String.IsNullOrEmpty(TemplateThemeFolder)) TemplateThemeFolder = ""; // we need a valid value, even if empty
            TemplCtrl = new NBrightCore.TemplateEngine.TemplateGetter(PrimaryTemplateMapPath, SecondaryTemplateMapPath, "NBrightTemplates", TemplateThemeFolder);
        }

        protected override void OnLoad(System.EventArgs e)
        {

            //Get UserDataInfo
            if (Page.IsPostBack)
            {
                // on postback update userdatainfo with form input data.
                UpdateUserData();
            }
            else
            {
                PopulateSearchHeader(CtrlTypeCode); // Update the search header.
            }

			//// Debug code for cache improvement timing: REMOVE FOR BUILD
			//NBrightLogEndTick = DateTime.Now.Ticks;
			//NBrightLogElapsedTick = NBrightLogEndTick - NBrightLogStartTick;
			//NBrightLogTrace += NBrightLogElapsedTick.ToString() + " - Total Ticks " + base.ModuleId.ToString("") + "\r\n"; ;
			//NBrightCore.common.Utils.SaveFile(PortalSettings.HomeDirectoryMapPath + "\\NBrightLogTrace.txt", NBrightLogTrace);
			//// Debug code for cache improvement timing: REMOVE FOR BUILD

            base.OnLoad(e);

        }


        #endregion

        #region "Get data Methods"

        /* *********************  Object Gets ********************** */

        /// <summary>
        /// Gets singe record of Data including language data merged onto the XML. 
        /// </summary>
        /// <param name = "lang">Tells function which language data should be included.  If empty no langauge data is included in the return data.</param>
        /// <param name = "seluserId">Allows selection of data for a specific user</param>
        public NBrightInfo GetData(string lang = "", string seluserId = "")
        {
            if (Utils.IsNumeric(ItemId))
                return GetData(Convert.ToInt32(ItemId), EntityTypeCodeLang, lang, seluserId);
            return null;
        }

        /// <summary>
        /// Gets singe record of Data including language data merged onto the XML. 
        /// </summary>
        /// <param name = "itemId">Record DB ItemId</param>
        /// <param name = "entityTypeCodeLang">EntityTypeCodeLang, if empty no language data is returned.</param>
        /// <param name = "lang">Tells function which language data should be included.  If empty no langauge data is included in the return data.</param>
        /// <param name = "seluserId">Allows selection of data for a specific user</param>
        public NBrightInfo GetData(int itemId, string entityTypeCodeLang = "", string lang = "", string seluserId = "")
        {
            if (seluserId != "")
            {
                //[TODO: I'm not sure this seciton of code for users data is valid anymore, what if we have data without a langauge for a user? 25/07/2013 ]
                var strFilter = " and userid = '" + seluserId + "' ";
                var l = GetList(PortalId, ModuleId, EntityTypeCode, strFilter, "", 1, 0, 0, 0, entityTypeCodeLang, lang);
                return l.Count >= 1 ? l[0] : null;
            }

            var obj =  (NBrightInfo)((DataCtrlInterface)ObjCtrl).Get(itemId, entityTypeCodeLang, lang);
            if (obj== null)
            {
                //there is no langauge record, but there may be a non-langauge record, so get that.
                obj = (NBrightInfo)((DataCtrlInterface)ObjCtrl).Get(itemId);
            }
            return obj;
        }

        /// <summary>
        /// Gets singe record of language Data. (EntityLangauge).
        /// </summary>
        /// <param name="seluserId">select by userid.  (not to be confused with a language param)</param>
        /// <returns></returns>
        public NBrightInfo GetDataLang(string seluserId = "")
        {
            if (Utils.IsNumeric(ItemId))
                return GetDataLang(Convert.ToInt32(ItemId), EntityLangauge, seluserId);
            return null;
        }

        /// <summary>
        /// Gets singe record of language Data. 
        /// </summary>
        /// <param name="parentItemId">Parent itemId</param>
        /// <param name="lang">Entity langauge to select</param>
        /// <param name="seluserId">select by userid</param>
        /// <returns></returns>
        public NBrightInfo GetDataLang(int parentItemId, string lang = "", string seluserId = "")
        {
            if (lang == "") lang = EntityLangauge;
            var strFilter = " and NB1.parentitemid = '" + parentItemId + "' and ISNULL(NB2.[Lang],ISNULL(NB1.[Lang],'''')) =  '" + lang + "' ";
            if (seluserId != "")
            {
                strFilter += " and userid = '" + seluserId + "' ";
            }
            var l = GetList(PortalId, ModuleId, EntityTypeCodeLang, strFilter, "", 0);

            // START: FIX DATA ISSUES
            // In some cases we have a mismatch between the itemid of the record and the itemid in the XML data
            // I'm not sure how this happens (maybe import/export), but here we just make sure it's OK.
            NBrightInfo rtnObj = null;
            if (l.Count >= 1)
            {
                rtnObj = l[0];                
                var i = rtnObj.GetXmlProperty("genxml/hidden/itemid");
                if (i != "" && i != rtnObj.ItemID.ToString("")) // record might not have a hidden itemid field.
                {
                    rtnObj.SetXmlProperty("genxml/hidden/itemid", rtnObj.ItemID.ToString(""));
                    ObjCtrl.Update(rtnObj); // fix record.
                }
            }
            // I think!! because of the above issue we might have multiple lang record, remove the invalid ones.
            if (l.Count >= 2)
            {
                for (int i = 1; i < l.Count; i++)
                {
                    NBrightInfo obj = l[i];
                    ObjCtrl.Delete(obj.ItemID);
                }
            }
            // END: FIX.

            return rtnObj;
        }


        /* *********************  list Gets ********************** */

        /// <summary>
        /// Gets a list of Data records, using the meta data in the repeater to specify the filter and order.
        /// </summary>
        /// <param name="rp1"></param>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="entityTypeCode"></param>
        /// <param name="returnLimit"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <param name="entityTypeCodeLang">If empty no langauge data is returned</param>
        /// <param name = "lang">Tells function which language data should be included.  If empty no langauge data is included in the return data.</param>
        /// <returns></returns>
       public List<NBrightInfo> GetList(Repeater rp1, int portalId, int moduleId, string entityTypeCode, int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string entityTypeCodeLang = "", string lang = "")
        {
            var sqlSearchFilter = GenXmlFunctions.GetSqlSearchFilters(rp1);
            var sqlOrderBy = GenXmlFunctions.GetSqlOrderBy(rp1);
            //Default orderby if not set
            if (String.IsNullOrEmpty(sqlOrderBy)) sqlOrderBy = " Order by ModifiedDate DESC ";

            return ((DataCtrlInterface)ObjCtrl).GetList(portalId, moduleId, entityTypeCode, sqlSearchFilter, sqlOrderBy, returnLimit, pageNumber, pageSize, recordCount, entityTypeCodeLang, lang);
        }

        /// <summary>
        /// Gets a list of Data records
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="entityTypeCode"></param>
        /// <param name="sqlSearchFilter"></param>
        /// <param name="sqlOrderBy"></param>
        /// <param name="returnLimit"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <param name="entityTypeCodeLang">If empty no langauge data is returned</param>
        /// <param name = "lang">Tells function which language data should be included.  If empty no langauge data is included in the return data.</param>
        /// <returns></returns>
        public List<NBrightInfo> GetList(int portalId, int moduleId, string entityTypeCode, string sqlSearchFilter = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string entityTypeCodeLang = "", string lang = "")
        {
            return ((DataCtrlInterface)ObjCtrl).GetList(portalId, moduleId, entityTypeCode, sqlSearchFilter, sqlOrderBy, returnLimit, pageNumber, pageSize, recordCount, entityTypeCodeLang, lang);
        }

        #endregion

        #region "Delete Methods"

        /// <summary>
        /// Delete all records linked to the typecode for a module.
        /// </summary>
        /// <param name="entityTypeCode"></param>
        /// <param name="uploadFolder"></param>
		public void DeleteAllEntityData(string entityTypeCode, string uploadFolder)
		{
			DeleteAllEntityData(base.PortalId, base.ModuleId, entityTypeCode, uploadFolder);
		}

        public void DeleteAllEntityData(int portalId, int moduleId, string entityTypeCode, string uploadFolder)
        {
            var l = GetList(portalId, moduleId, entityTypeCode);
            foreach (var obj in l)
            {
                // NOTE: If we select a empty type code, the return list will include all "-1" moduleid records, so test we only delet the correct module data.
                if (obj.ModuleId == moduleId) DeleteData(obj.ItemID, uploadFolder);
            }
        }

        public void DeleteData(Repeater rp1, string uploadFolder = "")
        {
            var objInfo = new NBrightInfo();
            var itemId = GenXmlFunctions.GetHiddenField(rp1, "ItemID");

            if (itemId == "")
            { // No valid itemid on XML, take from current id.  
                itemId = ItemId;
            }

            if (Utils.IsNumeric(itemId))
            {
                DeleteData(Convert.ToInt32(itemId), uploadFolder);
            }
        }

        public void DeleteData(int itemID, string uploadFolder,string folderMapPath = "")
        {
            var objInfo = ((DataCtrlInterface)ObjCtrl).Get(itemID);
            if (objInfo != null)
            {

                // delete any child records linked to parent.
                var strFilter = " and NB1.parentitemid = '" + itemID.ToString("") + "' ";
                var l = GetList(objInfo.PortalId,-1,"",strFilter);
                foreach (var o in l)
                {
                    DeleteData(o.ItemID,uploadFolder);
                }

                // delete any xref records linked to parent.
                strFilter = " and NB1.XrefItemId = '" + itemID.ToString("") + "' ";
                l = GetList(objInfo.PortalId, -1, "", strFilter);
                foreach (var o in l)
                {
                    DeleteData(o.ItemID, uploadFolder);
                }

                DeleteLinkedFiles(itemID, uploadFolder, folderMapPath);
                ((DataCtrlInterface)ObjCtrl).Delete(itemID);

            }

        }

        public void DeleteLinkedFiles(int itemId, string uploadFolder, string folderMapPath = "")
        {
            var obj = ObjCtrl.Get(itemId);
            if ((uploadFolder != "" | folderMapPath !="") & obj != null)
            {
                var fldr = PortalSettings.HomeDirectoryMapPath + uploadFolder;
                if (folderMapPath != "") fldr = folderMapPath;
                obj.XMLData = GenXmlFunctions.DeleteFile(obj.XMLData, fldr);
                ObjCtrl.Update(obj);
            }
        }

        #endregion

        #region "update methods"

        private object UpdateDetailData(Repeater rp1, string typeCode, string uploadFolderMapPath, string itemId = "0", string selUserId = "", string GUIDKey = "")
        {
            if (!Utils.IsNumeric(selUserId))
            {
                selUserId = UserId.ToString("");
            }

            var objInfo = new NBrightInfo();

            if (Utils.IsNumeric(GenXmlFunctions.GetHiddenField(rp1, "ItemID")))
            {
                itemId = GenXmlFunctions.GetHiddenField(rp1, "ItemID");
            }

            if (Utils.IsNumeric(itemId))
            {
                // read any existing data or create new.
                objInfo = ObjCtrl.Get(Convert.ToInt32(itemId));
                if (objInfo == null)
                {
                    objInfo = new NBrightInfo();
                    // populate data
                    objInfo.PortalId = PortalId;
                    objInfo.ModuleId = ModuleId;
                    objInfo.ItemID = Convert.ToInt32(itemId);
                    objInfo.TypeCode = typeCode;
                    objInfo.UserId = Convert.ToInt32(selUserId);
                    objInfo.GUIDKey = GUIDKey;
                }

                // populate changed data
                GenXmlFunctions.SetHiddenField(rp1, "dteModifiedDate", Convert.ToString(DateTime.Now));
                objInfo.ModifiedDate = DateTime.Now;

                objInfo.UserId = Convert.ToInt32(selUserId);
                
                //rebuild xml
                objInfo.XMLData = GenXmlFunctions.GetGenXml(rp1, "", uploadFolderMapPath);

                //update GUIDKey 
                if (GUIDKey != "")
                {
                    objInfo.GUIDKey = GUIDKey;
                }

                objInfo.ItemID = ((DataCtrlInterface)ObjCtrl).Update(objInfo);

            }
            return objInfo;
        }

        public object UpdateDetailNoValidate(Repeater rp1, string typeCode, string uploadFolderMapPath, string itemId = "0", string selUserId = "", string GUIDKey = "")
        {
            var objInfo = (NBrightInfo)UpdateDetailData(rp1, typeCode, uploadFolderMapPath, itemId, selUserId,GUIDKey);
            if (Convert.ToString(objInfo.ItemID) != ItemId)
            {
                ItemId = Convert.ToString(objInfo.ItemID); // make sure base class has correct ID                    
                GenXmlFunctions.SetHiddenField(rp1, "itemid", ItemId);
                //rebuild xml
                objInfo.XMLData = GenXmlFunctions.GetGenXml(rp1);
                objInfo.ItemID = ((DataCtrlInterface)ObjCtrl).Update(objInfo);
            }
            return objInfo;
        }

        public object UpdateDetailLangNoValidate(Repeater rp1, string typeCode, string uploadFolderMapPath, string itemIdLang = "0", string selUserId = "",string GUIDKey = "")
        {
            var objInfo = (NBrightInfo)UpdateDetailData(rp1, typeCode, uploadFolderMapPath, itemIdLang, selUserId, GUIDKey);
			if ((Convert.ToString(objInfo.ItemID) != ItemIdLang) | (String.IsNullOrEmpty(objInfo.Lang))) //update lang field if enpty.
            {
                ItemIdLang = Convert.ToString(objInfo.ItemID); // make sure base class has correct ID                    
                GenXmlFunctions.SetHiddenField(rp1, "itemid", ItemIdLang);
                GenXmlFunctions.SetHiddenField(rp1, "lang", EntityLangauge);
                //rebuild xml
                objInfo.XMLData = GenXmlFunctions.GetGenXml(rp1);
                objInfo.ParentItemId = Convert.ToInt32(ItemId);
            	objInfo.Lang = EntityLangauge;
                objInfo.ItemID = ((DataCtrlInterface)ObjCtrl).Update(objInfo);
            }
            return objInfo;
        }

        public int UpdateData(NBrightInfo objInfo)
        {
            return ((DataCtrlInterface)ObjCtrl).Update(objInfo);
        }

        public int AddBlankEntity(string DatabaseTypeCode)
        {
            var objInfo = new NBrightInfo { ItemID = -1, PortalId = PortalId, ModuleId = ModuleId, TypeCode = DatabaseTypeCode, ModifiedDate = DateTime.Now };
            return ((DataCtrlInterface)ObjCtrl).Update(objInfo);
        }

        public void CopyDataLang(string fromCultureCode, string toCultureCode, string uploadFolder = "")
        {
            if ((fromCultureCode != "" & toCultureCode != "") & (fromCultureCode != toCultureCode) && Utils.IsNumeric(ItemId))
            {
                var newItemId = -1;
                var objToLang = GetDataLang(Convert.ToInt32(ItemId),toCultureCode);
                if (objToLang != null)
                {
                    newItemId = objToLang.ItemID;
                }

                var objFromLang = GetDataLang(Convert.ToInt32(ItemId), fromCultureCode);
                if (objFromLang != null)
                {
                    objFromLang.ItemID = newItemId;
                    objFromLang.SetXmlProperty("genxml/hidden/lang", toCultureCode);
                    objFromLang.SetXmlProperty("genxml/hidden/itemid", objFromLang.ItemID.ToString(""));
                	objFromLang.Lang = toCultureCode;
					if (newItemId >= 0)
                    {
                        DeleteData(newItemId, uploadFolder);
                    }
                    else
                    {
                        newItemId = UpdateData(objFromLang);
                        objFromLang.SetXmlProperty("genxml/hidden/itemid", newItemId.ToString(""));
                        objFromLang.ItemID = newItemId;
                    }
                    UpdateData(objFromLang);
                }
            }
        }

        #endregion

        #region "userData search methods"

        public void SetSearchUserDataInfoVar(Repeater rpSearch)
        {
            var strFilters = GenXmlFunctions.GetSqlSearchFilters(rpSearch);
            var strOrderBy = GenXmlFunctions.GetSqlOrderBy(rpSearch);

            if (GenXmlFunctions.GetHiddenField(CtrlSearch, "lang") != "")
            {
                strFilters += " and ISNULL(NB2.[Lang],ISNULL(NB1.[Lang],'''')) = '" + Utils.GetCurrentCulture() + "' or ISNULL(Lang,'') = '') ";
            }

            UInfo.SearchFilters = strFilters;
            UInfo.SearchOrderby = strOrderBy;
            UInfo.SearchReturnLimit = GenXmlFunctions.GetHiddenField(CtrlSearch, "searchreturnlimit");
            if (UInfo.SearchPageNumber == "") UInfo.SearchPageNumber = "1";
            UInfo.SearchPageSize = GenXmlFunctions.GetHiddenField(CtrlSearch, "searchpagesize");
            if (UInfo.SearchPageSize == "") UInfo.SearchPageSize = GenXmlFunctions.GetHiddenField(CtrlSearch, "pagesize");
            var strSearchModuleId = GenXmlFunctions.GetHiddenField(CtrlSearch, "searchmoduleid");
            if (Utils.IsNumeric(strSearchModuleId)) UInfo.SearchModuleId = Convert.ToInt32(strSearchModuleId);
            UInfo.SearchClearAfter = "0";

            UpdateUserData();

        }

        public void UpdateUserData()
        {
            if (UserInfo.UserID >= 0 && !DisableUserInfo) // only save this to DB if it's a registered user.  We don;t want robots creating records in DB.
            {
                UInfo.UserId = UserInfo.UserID;
                UInfo.TabId = TabId;
                UInfo.SkinSrc = Globals.QueryStringEncode(Utils.RequestQueryStringParam(Context, "SkinSrc"));
                UInfo.EntityTypeCode = EntityTypeCode;
                UInfo.CtrlTypeCode = CtrlTypeCode;

                // set these returns independantly, to allow return to previous pages & ajax called over paging. 
                //UInfo.RtnSelUrl = EditUrl("itemid", ItemId, CtrlTypeCode);
                //UInfo.RtnUrl = EditUrl("itemid", ItemId, CtrlTypeCode);
                //UInfo.FromItemId = ItemId;

                if (CtrlSearch.Visible)
                {
                    UInfo.SearchGenXml = GenXmlFunctions.GetGenXml(CtrlSearch);
                }
                UInfo.Save();
            }
        }


        public void PopulateSearchHeader(string typeCode)
        {
            if (CtrlSearch != null && CtrlSearch.Visible & !Page.IsPostBack)
            {
                var obj = new NBrightInfo();

                obj.ItemID = -1;
                obj.GUIDKey = "";
                obj.ModifiedDate = DateTime.Now;
                obj.TypeCode = typeCode;
                obj.XMLData = UInfo.SearchGenXml;   
                var l = new List<NBrightInfo> { obj };
                CtrlSearch.DataSource = l;
                CtrlSearch.DataBind();
            }
        }


        #endregion

        #region "Display Methods"

        public void DoList(Repeater rp1, int portalId, int moduleId, string typeCode, int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string typeCodeLang = "", string lang = "")
        {
            rp1.DataSource = GetList(rp1, portalId, moduleId, typeCode, returnLimit, pageNumber, pageSize, recordCount, typeCodeLang, lang);
            rp1.DataBind();
        }

        public void DoDetailLang(Repeater rp1, bool forceDisplay = true)
        {
            NBrightInfo obj = null;
            obj = GetDataLang();
            if (obj == null && forceDisplay)
            {
                obj = new NBrightInfo {ModuleId = ModuleId, PortalId = PortalId, XMLData = "<genxml></genxml>"};
            }
            if (obj != null)
            {
                ItemIdLang = obj.ItemID.ToString(); //assign ItemIdLang. Is done on init, but making sure here.
                var l = new List<object> {obj};
                rp1.DataSource = l;
                rp1.DataBind();
            }
        }


        public void DoDetail(Repeater rp1, NBrightInfo obj)
        {
            var l = new List<object> {obj};
            rp1.DataSource = l;
            rp1.DataBind();
        }


        public void DoDetail(Repeater rp1,bool forceDisplay = true)
        {
            NBrightInfo obj = null;
            if (Utils.IsNumeric(ItemId) && ItemId != "0") obj = GetData(Convert.ToInt32(ItemId), EntityTypeCodeLang, EntityLangauge);            
            if (obj == null && forceDisplay) obj = new NBrightInfo { ModuleId = ModuleId, PortalId = PortalId, XMLData = "<genxml></genxml>" };
            if (obj != null)
            {
                var cahceKey = "NBrightRepeater_" + ItemId + "*" + EntityTypeCodeLang + "*" + EntityLangauge;
                var l = new List<object> { obj };
                rp1.DataSource = l;
                rp1.DataBind();
            }
        }

        public List<NBrightInfo> GetListByUserDataInfoVar(string typeCode,string webserviceurl = "")
        {
            try
            {
                var weblist = new List<NBrightInfo>();
                var recordCount = 0;
                
                // in some ascx we want to run a wesvice on the OnLoad event, the base.OnLoad doesn;t allow us to pass the webservice url, so we can use this to override the normal function and force a webservice to be used.
                if (!string.IsNullOrEmpty(OverRideWebserviceUrl)) webserviceurl = OverRideWebserviceUrl;

                if (EntityTypeCode == "" && !string.IsNullOrEmpty(webserviceurl))
                {
                    // No EntityType, therefore data must be selected from WebService.
                    var l = new List<NBrightInfo>();
                    var xmlDoc = new XmlDocument();
                    
                    // pass the userdatainfo into the header request (saves using or creating a post field or adding to url)
                    var objInfo = ObjCtrl.Get(UInfo.ItemId);
                    var userdatainfo = "";
                    if (objInfo != null)
                    {
                        if (objInfo.TypeCode == "USERDATAINFO")
                        {
                            //userdatainfo = DotNetNuke.Common.Globals.HTTPPOSTEncode(objInfo.XMLData);
                            userdatainfo = objInfo.XMLData;
                        }
                    }

                    string strResp = DnnUtils.GetDataResponseAsString(webserviceurl, "userdatainfo", userdatainfo);
                    try
                    {
                        xmlDoc.LoadXml(strResp);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    var rc = xmlDoc.SelectSingleNode("root/recordcount");
                    if (rc != null && Utils.IsNumeric(rc.InnerText))
                    {
                        recordCount = Convert.ToInt32(rc.InnerText);
                    }

                    var xmlNodeList = xmlDoc.SelectNodes("root/item");
                    if (xmlNodeList != null)
                    {
                        foreach (XmlNode xmlNod in xmlNodeList)
                        {
                            var obj = new NBrightInfo();
                            obj.FromXmlItem(xmlNod.OuterXml);
                            l.Add(obj);
                        }
                    }
                    weblist = l;
					if (recordCount == 0) recordCount = weblist.Count;                    	
                }
                else
                {
                    if (OverRideInfoList != null)
                    {
                        recordCount = OverRideInfoList.Count;
                    }
                    else
                    {
                        recordCount = ObjCtrl.GetListCount(UInfo.SearchPortalId, UInfo.SearchModuleId, EntityTypeCode, UInfo.SearchFilters, EntityTypeCodeLang, EntityLangauge);
                    }
                }


                if (!Utils.IsNumeric(UInfo.SearchPageNumber)) UInfo.SearchPageNumber = "1";
                UInfo.SearchPageSize = GenXmlFunctions.GetHiddenField(CtrlSearch, "searchpagesize");
                if (UInfo.SearchPageSize == "") UInfo.SearchPageSize = GenXmlFunctions.GetHiddenField(CtrlSearch, "pagesize");
                UInfo.SearchReturnLimit = GenXmlFunctions.GetHiddenField(CtrlSearch, "searchreturnlimit");
                if (!Utils.IsNumeric(UInfo.SearchPageSize)) UInfo.SearchPageSize = "25";
                if (!Utils.IsNumeric(UInfo.SearchReturnLimit)) UInfo.SearchReturnLimit = "0";

                if (_activatePaging)
                {
                    CtrlPaging.PageSize = Convert.ToInt32(UInfo.SearchPageSize);
                    CtrlPaging.TotalRecords = recordCount;
                    CtrlPaging.CurrentPage = Convert.ToInt32(UInfo.SearchPageNumber);
                    CtrlPaging.BindPageLinks();
                }
                else
                {
                    CtrlPaging.Visible = false;
                }

                if (UInfo.SearchClearAfter == "1")
                {
                    UInfo.ClearSearchData();
                }

                if (OverRideInfoList != null)
                {
                    //overiding list passed from control, so use linq to do the paging, select  
                    var records = (from o in OverRideInfoList select o);

                    var pgNo = Convert.ToInt32(UInfo.SearchPageNumber);
                    var pgRec = Convert.ToInt32(UInfo.SearchPageSize);

                    var rtnRecords = records.Skip((pgNo - 1) * pgRec).Take(pgRec).ToList();

                    return rtnRecords;
                }

				if (EntityTypeCode == "" & webserviceurl != "")
				{
					// use website (Might be empty).
					return weblist;
				}
				else
				{
                    var l = GetList(UInfo.SearchPortalId, UInfo.SearchModuleId, EntityTypeCode, UInfo.SearchFilters, UInfo.SearchOrderby, Convert.ToInt32(UInfo.SearchReturnLimit), Convert.ToInt32(UInfo.SearchPageNumber), Convert.ToInt32(UInfo.SearchPageSize), recordCount, EntityTypeCodeLang, EntityLangauge);
				    return l;
				}
            }
            catch (Exception)
            {
                //clear data incase error in userdata
                UInfo.ClearSearchData();
                throw;
            }
        }

        #endregion

        #region "methods"

        public void OnInitActivateList(string listheaderTemplate, string listbodyTemplate, string listfooterTemplate, string searchTemplate = "", bool withPaging = true)
        {

            if (searchTemplate != "")
            {
                searchTemplate = ReplaceBasicTokens(searchTemplate);
                CtrlSearch.ItemTemplate = new GenXmlTemplate(searchTemplate);
            }
            else
            {
                CtrlSearch.Visible = false;
            }


            //set default filter
            if (UInfo.SearchClearAfter == "")
            {
                UInfo.SearchFilters = "";
            }

            if (UInfo.SearchOrderby == "")

            {
                if (searchTemplate != "")
                {
                    UInfo.SearchOrderby = GenXmlFunctions.GetSqlOrderBy(searchTemplate);
                }
                if (UInfo.SearchOrderby == "")
                {
                    UInfo.SearchOrderby = GenXmlFunctions.GetSqlOrderBy(listheaderTemplate);
                }
            }

            if (!Utils.IsNumeric(UInfo.SearchPageSize)) UInfo.SearchPageSize = "25";
            if (!Utils.IsNumeric(UInfo.SearchReturnLimit)) UInfo.SearchReturnLimit = "0";
            if (!Utils.IsNumeric(UInfo.SearchPageNumber)) UInfo.SearchPageNumber = "1";

            _activatePaging = withPaging;
            CtrlList.HeaderTemplate = new GenXmlTemplate(listheaderTemplate);
            var templ = new GenXmlTemplate(listbodyTemplate);
            templ.SortItemId = UInfo.SortItemId;
            CtrlList.ItemTemplate = templ;
            CtrlList.FooterTemplate = new GenXmlTemplate(listfooterTemplate);

        }

        public void OnInitActivateList(bool withSearch = true, bool withPaging = true, string templatePath = "")
        {
            var strListSearch = "";
            var strListHeader = "";
            var strListBody = "";
            var strListFooter = "";

            if (withSearch)
            {
                strListSearch = TemplCtrl.GetTemplateData(CtrlTypeCode + "_Search.html", Utils.GetCurrentCulture());
            }

            _activatePaging = withPaging;
			strListHeader = TemplCtrl.GetTemplateData(CtrlTypeCode + "_ListH.html", Utils.GetCurrentCulture());
			strListBody = TemplCtrl.GetTemplateData(CtrlTypeCode + "_List.html", Utils.GetCurrentCulture());
			strListFooter = TemplCtrl.GetTemplateData(CtrlTypeCode + "_ListF.html", Utils.GetCurrentCulture());

            OnInitActivateList(strListHeader, strListBody, strListFooter, strListSearch, withPaging);

        }

        public string ReplaceBasicTokens(string templateText,NBrightInfo nbSettings = null)
        {
            var strOut = templateText;
            strOut = strOut.Replace("[TokenSearch:searchDate1]", UInfo.SearchDate1);
            strOut = strOut.Replace("[TokenSearch:searchDate2]", UInfo.SearchDate2);
            strOut = strOut.Replace("[TokenSearch:searchExtra1]", UInfo.SearchExtra1);
            strOut = strOut.Replace("[TokenSearch:searchExtra2]", UInfo.SearchExtra2);
            strOut = strOut.Replace("[Token:langauge]", Utils.GetCurrentCulture());

            if (nbSettings != null)
            {
                strOut = strOut.Replace("[Token:modulekey]", nbSettings.GUIDKey);                
            }

            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(UInfo))
            {
                var value = prop.GetValue(UInfo);
                if (value == null) value = ""; 
                strOut = strOut.Replace("[UInfo:" + prop.Name + "]", value.ToString());
            }

            return strOut;
        }

        public void BindListData(string webServiceUrl = "")
        {

            EventBeforeBindListData(CtrlList, webServiceUrl);

            var l = GetListByUserDataInfoVar(CtrlTypeCode, webServiceUrl);

			if (l == null || l.Count == 0)
			{
				CtrlList.Visible = false;
				CtrlListMsg.Text = DotNetNuke.Services.Localization.Localization.GetString("noresult", base.LocalResourceFile);
				CtrlListMsg.Visible = true;
			}
			else
			{
				CtrlList.Visible = true;
				CtrlList.DataSource = l;
				CtrlList.DataBind();
			}

            EventAfterBindListData(CtrlList,webServiceUrl);
        }

        public void BindData(Repeater rpData,string webServiceUrl = "")
        {
            EventBeforeBindData(rpData, webServiceUrl);

            var l = GetListByUserDataInfoVar(CtrlTypeCode, webServiceUrl);

            rpData.DataSource = l;
            rpData.DataBind();

            EventAfterBindData(rpData, webServiceUrl);
        }

        #endregion


        #region "events"

        protected virtual void PagingClick(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            if (Utils.IsNumeric(cArg))
            {
                UInfo.SearchPageNumber = cArg;
                UInfo.Save();
            }
            EventBeforePageChange(source, e);
        }

        public virtual void EventBeforeBindData(Repeater rpCtrlList, string webServiceUrl = "")
        {

        }

        public virtual void EventAfterBindData(Repeater rpCtrlList, string webServiceUrl = "")
        {

        }


        public virtual void EventBeforeBindListData(Repeater rpData, string webServiceUrl = "")
        {

        }

        public virtual void EventAfterBindListData(Repeater rpData, string webServiceUrl = "")
        {

        }


        public virtual void EventBeforePageChange(object source, RepeaterCommandEventArgs e)
        {

        }

        protected virtual void CtrlListItemCommand(object source, RepeaterCommandEventArgs e)
        {

        }

        public virtual void EventListItemCommand(object source, RepeaterCommandEventArgs e)
        {

        }

        protected virtual void CtrlSearchItemCommand(object source, RepeaterCommandEventArgs e)
        {

        }

        public virtual void EventSearchItemCommand(object source, RepeaterCommandEventArgs e)
        {

        }


        //NOTE: This event cause memory leak
        //public void OnFileUploaded()
        //{
        //    FileHasBeenUploaded = true;
        //}

        #endregion

    }
}
