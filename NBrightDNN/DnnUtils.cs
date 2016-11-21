using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.FileSystem;
using ICSharpCode.SharpZipLib.Zip;
using NBrightCore.common;
using FileInfo = System.IO.FileInfo;

namespace NBrightDNN
{
    public class DnnUtils
    {

        public static void Zip(string zipFileMapPath, List<String> fileMapPathList )
        {
            // Zip up the files - From SharpZipLib Demo Code
            using (var s = new ZipOutputStream(File.Create(zipFileMapPath)))
            {
                s.SetLevel(9); // 0-9, 9 being the highest compression

                byte[] buffer = new byte[4096];

                foreach (string file in fileMapPathList)
                {

                    ZipEntry entry = new
                    ZipEntry(Path.GetFileName(file));

                    entry.DateTime = DateTime.Now;
                    s.PutNextEntry(entry);

                    using (FileStream fs = File.OpenRead(file))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0,
                            buffer.Length);

                            s.Write(buffer, 0, sourceBytes);

                        } while (sourceBytes > 0);
                    }
                }
                s.Finish();
                s.Close();
            }
 

        }

        public static void ZipFolder(string folderName, String zipFileMapPath)
        {
            var zipStream = new ZipOutputStream(File.Create(zipFileMapPath));
            try
            {
                int folderOffset = folderName.Length + (folderName.EndsWith("\\") ? 0 : 1);
                zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
                CompressFolder(folderName, zipStream, folderOffset);
                zipStream.Close();
            }
            catch (Exception ex)
            {
                zipStream.Close();
                throw ex;
            }
        }

        private static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {

            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {

                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    ZipUtilCopy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }

        /// <summary>
        /// Copy the contents of one <see cref="Stream"/> to another.  Taken from StreamUtils on SharpZipLib as stripped
        /// </summary>
        /// <param name="source">The stream to source data from.</param>
        /// <param name="destination">The stream to write data to.</param>
        /// <param name="buffer">The buffer to use during copying.</param>
        private static void ZipUtilCopy(Stream source, Stream destination, byte[] buffer)
        {

            // Ensure a reasonable size of buffer is used without being prohibitive.
            if (buffer.Length < 128)
            {
                throw new ArgumentException("Buffer is too small", "buffer");
            }

            bool copying = true;

            while (copying)
            {
                int bytesRead = source.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    destination.Write(buffer, 0, bytesRead);
                }
                else
                {
                    destination.Flush();
                    copying = false;
                }
            }
        }


        public static void UnZip(string zipFileMapPath, string outputFolder)
        {
            var zipStream = new FileStream(zipFileMapPath, FileMode.Open, FileAccess.Read);
            var zStream = new ZipInputStream(zipStream);
            UnzipResources(zStream, outputFolder);
            zipStream.Close();
            zStream.Close();
        }

        public static void UnzipResources(ZipInputStream zipStream, string destPath)
        {
            try
            {
                ZipEntry objZipEntry;
                string LocalFileName;
                string RelativeDir;
                string FileNamePath;
                objZipEntry = zipStream.GetNextEntry();
                while (objZipEntry != null)
                {
                    LocalFileName = objZipEntry.Name;
                    RelativeDir = Path.GetDirectoryName(objZipEntry.Name);
                    if ((RelativeDir != string.Empty) && (!Directory.Exists(Path.Combine(destPath, RelativeDir))))
                    {
                        Directory.CreateDirectory(Path.Combine(destPath, RelativeDir));
                    }
                    if ((!objZipEntry.IsDirectory) && (!String.IsNullOrEmpty(LocalFileName)))
                    {
                        FileNamePath = Path.Combine(destPath, LocalFileName).Replace("/", "\\");
                        try
                        {
                            if (File.Exists(FileNamePath))
                            {
                                File.SetAttributes(FileNamePath, FileAttributes.Normal);
                                File.Delete(FileNamePath);
                            }
                            FileStream objFileStream = null;
                            try
                            {
                                objFileStream = File.Create(FileNamePath);
                                int intSize = 2048;
                                var arrData = new byte[2048];
                                intSize = zipStream.Read(arrData, 0, arrData.Length);
                                while (intSize > 0)
                                {
                                    objFileStream.Write(arrData, 0, intSize);
                                    intSize = zipStream.Read(arrData, 0, arrData.Length);
                                }
                            }
                            finally
                            {
                                if (objFileStream != null)
                                {
                                    objFileStream.Close();
                                    objFileStream.Dispose();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                         //   DnnLog.Error(ex);
                        }
                    }
                    objZipEntry = zipStream.GetNextEntry();
                }
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Close();
                    zipStream.Dispose();
                }
            }
        }



        public static List<System.IO.FileInfo> GetFiles(string FolderMapPath)
        {
            DirectoryInfo di = new DirectoryInfo(FolderMapPath);
            List<System.IO.FileInfo> files = new List<System.IO.FileInfo>();
            if (di.Exists)
            {
                foreach (System.IO.FileInfo file in di.GetFiles())
                {
                    files.Add(file);
                }                
            }
            return files;
        }

        public static Dictionary<String,String> GetCountryCodeList(int portalId = -1)
        {
            var rtnDic = new Dictionary<String, String>();
            if (portalId == -1 && PortalSettings.Current != null) portalId = PortalSettings.Current.PortalId;
            if (portalId != -1)
            {
                var objLCtrl = new ListController();
                var l = objLCtrl.GetListEntryInfoItems("Country").ToList();
                foreach (var i in l)
                {
                    rtnDic.Add(i.Value,i.Text);
                }
            }
            return rtnDic;
        }

        public static List<string> GetCultureCodeList(int portalId = -1)
        {
			var rtnList = new List<string>();
            if (portalId == -1 && PortalSettings.Current != null) portalId = PortalSettings.Current.PortalId;
            if (portalId != -1)
			{
				var enabledLanguages = LocaleController.Instance.GetLocales(portalId);
				foreach (KeyValuePair<string, Locale> kvp in enabledLanguages)
				{
					rtnList.Add(kvp.Value.Code);
				}				
			}
            return rtnList;            
        }

        public static string GetCurrentValidCultureCode(List<string> validCultureCodes = null)
        {
            var validCurrentCulture = Utils.GetCurrentCulture();

            if (validCultureCodes != null)
            {
                if (validCultureCodes.Count > 0 && !validCultureCodes.Contains(validCurrentCulture))
                {
                    //Cannot find the current culture so return the first in the valid list
                    return validCultureCodes[0];
                }
            }

            return validCurrentCulture;
        }

        public static string GetDataResponseAsString(string dataurl,string headerFieldId = "",string headerFieldData = "")
        {
            string strOut = "";
            if (!string.IsNullOrEmpty(dataurl))
            {
                try
                {

                    // solution for exception
                    // The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel.
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    System.Net.HttpWebRequest req = DotNetNuke.Common.Globals.GetExternalRequest(dataurl);
                    if (headerFieldId!= "")
                    {
                        //allow a get request to pass data via the header.
                        //  This is limited to 32K by default in IIS, but can be limited to less. (So use with care!)
                        req.Headers.Add(headerFieldId,headerFieldData);
                    }
                    System.Net.WebResponse resp = req.GetResponse();
                    var s = resp.GetResponseStream();
                    if (s != null)
                    {
                        var reader = new StreamReader(s);
                        strOut = reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    strOut = "ERROR - dataurl=" + dataurl + "  ex=" + ex.ToString();
                }
            }
            else
            {
                strOut = "ERROR - No Data Url given";
            }
            return strOut;
        }


        public static void PurgeDataBaseInfo(int portalId, int moduleId, DataCtrlInterface objCtrl, string entityTypeCode, int purgeDays = -7)
        {
            var l = objCtrl.GetList(portalId, moduleId, entityTypeCode);
            foreach (NBrightInfo obj in l)
            {
                if (obj.ModifiedDate < (DateTime.Now.AddDays(purgeDays)))
                {
                    objCtrl.Delete(obj.ItemID);                    
                }
            }

        }

        public static DotNetNuke.Entities.Tabs.TabCollection GetPortalTabs(int portalId)
        {
            var portalTabs = (DotNetNuke.Entities.Tabs.TabCollection)NBrightCore.common.Utils.GetCache("NBright_portalTabs" + portalId.ToString(""));
            if (portalTabs == null)
            {
                var objTabCtrl = new DotNetNuke.Entities.Tabs.TabController();
                portalTabs = objTabCtrl.GetTabsByPortal(portalId);
                NBrightCore.common.Utils.SetCache("NBright_portalTabs" + portalId.ToString(""), portalTabs);
            }
            return portalTabs;
        }

        public static DotNetNuke.Entities.Users.UserInfo GetValidUser(int PortalId, string username, string password)
        {
            var userLoginStatus = new DotNetNuke.Security.Membership.UserLoginStatus();
            return DotNetNuke.Entities.Users.UserController.ValidateUser(PortalId, username, password, "", "", "", ref userLoginStatus);
        }

        public static bool IsValidUser(int PortalId, string username, string password)
        {
            var u = GetValidUser(PortalId,username,password); 
            if (u != null)
            {
                return true;
            }
            return false;
        }

        public static string GetLocalizedString(string Key, string resourceFileRoot,string lang)
        {
            return Localization.GetString(Key, resourceFileRoot, lang);
        }

        public static int GetPortalByModuleID(int moduleId)
        {
            var objMCtrl = new DotNetNuke.Entities.Modules.ModuleController();
            var objMInfo = objMCtrl.GetModule(moduleId);
            if (objMInfo == null) return -1;
            return objMInfo.PortalID;
        }

        /// <summary>
        /// GET Portals 
        /// </summary>
        /// <returns></returns>
        public static List<PortalInfo> GetAllPortals()
        {
            var pList = new List<PortalInfo>();
            var objPC = new DotNetNuke.Entities.Portals.PortalController();

            var list = objPC.GetPortals();

            if (list == null || list.Count == 0)
            {
                //Problem with DNN6 GetPortals when ran from scheduler.
                PortalInfo objPInfo;
                var flagdeleted = 0; 

                for (var lp = 0; lp <= 500; lp++)
                {
                    objPInfo = objPC.GetPortal(lp);
                    if ((objPInfo != null))
                    {
                        pList.Add(objPInfo);
                    }
                    else
                    {
                        // some portals may be deleted, skip 3 to see if we've got to the end of the list.
                        // VERY weak!!! shame!! but issue with a DNN6 version only.
                        if (flagdeleted == 3) break;
                        flagdeleted += 1;
                    }
                }
            }
            else
            {
                foreach (PortalInfo p in list)
                {
                    pList.Add(p);
                }
            }


            return pList;
        }

		public static string GetModuleVersion(int moduleId)
		{
			var strVersion = "";
			var objMCtrl = new DotNetNuke.Entities.Modules.ModuleController();
			var objMInfo = objMCtrl.GetModule(moduleId);
			if (objMInfo != null)
			{
				strVersion = objMInfo.DesktopModule.Version;
			}
			return strVersion;
		}

        public static ModuleInfo GetModuleinfo(int moduleId)
        {
            var objMCtrl = new DotNetNuke.Entities.Modules.ModuleController();
            var objMInfo = objMCtrl.GetModule(moduleId);
            return objMInfo;
        }

        public static void CreateFolder(string fullfolderPath)
        {
            // This function is to get around medium trust not allowing createfolder in .Net 2.0. 
            // DNN seems to have some work around (Not followed up on what exactly, probably security allowed in shared hosting environments for DNN except???).
            // But this leads me to have this rather nasty call to DNN FolderManager.
            // Prefered method is to us the Utils.CreateFolder function in NBrightCore.

            var blnCreated = false;
            //try normal test (doesn;t work on medium trust, but stops us forcing a "AddFolder" and suppressing the error.)
            try
            {
                blnCreated = System.IO.Directory.Exists(fullfolderPath);
            }
            catch (Exception ex)
            {
                blnCreated = false;
            }

            if (!blnCreated)
            {
                try
                {
                    var f = FolderManager.Instance.AddFolder(FolderMappingController.Instance.GetFolderMapping(8), fullfolderPath);
                }
                catch (Exception ex)
                {
                    // Suppress error, becuase the folder may already exist!..NASTY!!..try and find a better way to deal with folders out of portal range!!
                }
            }
        }

        public static void CreatePortalFolder(DotNetNuke.Entities.Portals.PortalSettings PortalSettings, string FolderName)
        {
            bool blnCreated = false;

            //try normal test (doesn;t work on medium trust, but avoids waiting for GetFolder.)
            try
            {
                blnCreated = System.IO.Directory.Exists(PortalSettings.HomeDirectoryMapPath + FolderName);
            }
            catch (Exception ex)
            {
                blnCreated = false;
            }

            if (!blnCreated)
            {
                FolderManager.Instance.Synchronize(PortalSettings.PortalId, PortalSettings.HomeDirectory, true,true);
                var folderInfo =  FolderManager.Instance.GetFolder(PortalSettings.PortalId, FolderName);
                if (folderInfo == null & !string.IsNullOrEmpty(FolderName))
                {
                    //add folder and permissions
                    try
                    {
                        FolderManager.Instance.AddFolder(PortalSettings.PortalId, FolderName);
                    }
                    catch (Exception ex)
                    {
                    }
                    folderInfo = FolderManager.Instance.GetFolder(PortalSettings.PortalId, FolderName);
                    if ((folderInfo != null))
                    {
                        int folderid = folderInfo.FolderID;
                        DotNetNuke.Security.Permissions.PermissionController objPermissionController = new DotNetNuke.Security.Permissions.PermissionController();
                        var arr = objPermissionController.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "");
                        foreach (DotNetNuke.Security.Permissions.PermissionInfo objpermission in arr)
                        {
                            if (objpermission.PermissionKey == "WRITE")
                            {
                                // add READ permissions to the All Users Role
                                FolderManager.Instance.SetFolderPermission(folderInfo, objpermission.PermissionID, int.Parse(DotNetNuke.Common.Globals.glbRoleAllUsers));
                            }
                        }
                    }
                }
            }
        }

        public static DotNetNuke.Entities.Portals.PortalSettings GetCurrentPortalSettings()
        {
            return (DotNetNuke.Entities.Portals.PortalSettings) System.Web.HttpContext.Current.Items["PortalSettings"];
        }

        public static Dictionary<int, string> GetTreeTabList()
        {
            var tabList = DotNetNuke.Entities.Tabs.TabController.GetTabsBySortOrder(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, Utils.GetCurrentCulture(), true);
            var rtnList = new Dictionary<int, string>();
            return GetTreeTabList(rtnList, tabList, 0, 0);
        }

        private static Dictionary<int, string> GetTreeTabList(Dictionary<int, string> rtnList, List<TabInfo> tabList, int level, int parentid, string prefix = "")
        {

            if (level > 20) // stop infinate loop
            {
                return rtnList;
            }
            if (parentid > 0) prefix += "..";
            foreach (TabInfo tInfo in tabList)
            {
                var parenttestid = tInfo.ParentId;
                if (parenttestid < 0) parenttestid = 0;
                if (parentid == parenttestid)
                {
                    if (!tInfo.IsDeleted && tInfo.TabPermissions.Count > 2)
                    {
                        rtnList.Add(tInfo.TabID, prefix + "" + tInfo.TabName);
                        GetTreeTabList(rtnList, tabList, level + 1, tInfo.TabID, prefix);
                    }
                }
            }

            return rtnList;
        }


        public static Dictionary<Guid, string> GetTreeTabListOnUniqueId()
        {
            var tabList = DotNetNuke.Entities.Tabs.TabController.GetTabsBySortOrder(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, Utils.GetCurrentCulture(), true);
            var rtnList = new Dictionary<Guid, string>();
            return GetTreeTabListOnUniqueId(rtnList, tabList, 0, 0);
        }

        private static Dictionary<Guid, string> GetTreeTabListOnUniqueId(Dictionary<Guid, string> rtnList, List<TabInfo> tabList, int level, int parentid, string prefix = "")
        {

            if (level > 20) // stop infinate loop
            {
                return rtnList;
            }
            if (parentid > 0) prefix += "..";
            foreach (TabInfo tInfo in tabList)
            {
                var parenttestid = tInfo.ParentId;
                if (parenttestid < 0) parenttestid = 0;
                if (parentid == parenttestid)
                {
                    if (!tInfo.IsDeleted && tInfo.TabPermissions.Count > 2)
                    {
                        rtnList.Add(tInfo.UniqueId, prefix + "" + tInfo.TabName);
                        GetTreeTabListOnUniqueId(rtnList, tabList, level + 1, tInfo.TabID, prefix);
                    }
                }
            }

            return rtnList;
        }

        public static Dictionary<int, string> GetTreeTabListOnTabId()
        {
            var tabList = DotNetNuke.Entities.Tabs.TabController.GetTabsBySortOrder(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, Utils.GetCurrentCulture(), true);
            var rtnList = new Dictionary<int, string>();
            return GetTreeTabListOnTabId(rtnList, tabList, 0, 0);
        }

        private static Dictionary<int, string> GetTreeTabListOnTabId(Dictionary<int, string> rtnList, List<TabInfo> tabList, int level, int parentid, string prefix = "")
        {

            if (level > 20) // stop infinate loop
            {
                return rtnList;
            }
            if (parentid > 0) prefix += "..";
            foreach (TabInfo tInfo in tabList)
            {
                var parenttestid = tInfo.ParentId;
                if (parenttestid < 0) parenttestid = 0;
                if (parentid == parenttestid)
                {
                    if (!tInfo.IsDeleted && tInfo.TabPermissions.Count > 2)
                    {
                        rtnList.Add(tInfo.TabID, prefix + "" + tInfo.TabName);
                        GetTreeTabListOnTabId(rtnList, tabList, level + 1, tInfo.TabID, prefix);
                    }
                }
            }

            return rtnList;
        }


        public static String GetTreeViewTabJSData(String selectTabIdCVS = "")
        {
            var tabList = DotNetNuke.Entities.Tabs.TabController.GetTabsBySortOrder(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, Utils.GetCurrentCulture(), true);
            var rtnDataString = "";
            var selecttabidlist = selectTabIdCVS.Split(',');
            rtnDataString = GetTreeViewTabJSData(rtnDataString, tabList, 0, 0, selecttabidlist);
            rtnDataString = rtnDataString.Replace(", children: []", "");
            rtnDataString = "var treeData = [" + rtnDataString + "];";
            return rtnDataString;
        }

        private static String GetTreeViewTabJSData(String rtnDataString, List<TabInfo> tabList, int level, int parentid, String[] selecttabidlist)
        {

            if (level > 20) // stop infinate loop
            {
                return rtnDataString;
            }
            foreach (TabInfo tInfo in tabList)
            {
                var parenttestid = tInfo.ParentId;
                if (parenttestid < 0) parenttestid = 0;
                if (parentid == parenttestid)
                {
                    if (!tInfo.IsDeleted && tInfo.TabPermissions.Count > 2)
                    {
                        var selectedvalue = "false";
                        if (selecttabidlist.Contains(tInfo.TabID.ToString(""))) selectedvalue = "true";
                        rtnDataString += "{title: '" + tInfo.TabName + "', key:'" + tInfo.TabID + "', selected: " + selectedvalue + ", children: [";
                        rtnDataString = GetTreeViewTabJSData(rtnDataString, tabList, level + 1, tInfo.TabID, selecttabidlist);
                        rtnDataString += "]},";
                    }
                }
            }
            rtnDataString = rtnDataString.TrimEnd(',');
            return rtnDataString;
        }


        public static Dictionary<string, string> GetUserProfileProperties(UserInfo userInfo)
        {
            var prop = new Dictionary<string, string>();
            foreach (DotNetNuke.Entities.Profile.ProfilePropertyDefinition p in userInfo.Profile.ProfileProperties)
            {
                prop.Add(p.PropertyName, p.PropertyValue);
            }
            return prop;
        }

        public static Dictionary<string, string> GetUserProfileProperties(String userId)
        {
            if (!Utils.IsNumeric(userId)) return null;
            var userInfo = UserController.GetUserById(PortalSettings.Current.PortalId,Convert.ToInt32(userId));
            return GetUserProfileProperties(userInfo);
        }

        public static void SetUserProfileProperties(UserInfo userInfo,Dictionary<string, string> properties)
        {
            foreach (var p in properties)
            {
                userInfo.Profile.SetProfileProperty(p.Key,p.Value);
                UserController.UpdateUser(PortalSettings.Current.PortalId, userInfo);
            }
        }
        public static void SetUserProfileProperties(String userId,Dictionary<string, string> properties)
        {
            if (Utils.IsNumeric(userId))
            {
                var userInfo = UserController.GetUserById(PortalSettings.Current.PortalId, Convert.ToInt32(userId));
                SetUserProfileProperties(userInfo, properties);                
            }
        }

        public static PortalSettings GetPortalSettings(int portalId)
        {
            var controller = new PortalController();
            var portal = controller.GetPortal(portalId);
            return new PortalSettings(portal);
        }

        public static String GetResourceString(String resourcePath, String resourceKey,String resourceExt = "Text", String lang = "")
        {
            var resDic = GetResourceData(resourcePath, resourceKey, lang);
            if (resDic != null && resDic.ContainsKey(resourceExt))
            {
                return resDic[resourceExt];
            }
            return "";
        }

        public static Dictionary<String, String> GetResourceData(String resourcePath, String resourceKey, String lang = "")
        {
            if (lang == "") lang = DnnUtils.GetCurrentValidCultureCode();
            var ckey = resourcePath + resourceKey + lang;
            var obj = Utils.GetCache(ckey);
            if (obj != null) return (Dictionary<String, String>)obj;

            var rtnList = new Dictionary<String, String>();
            var s = resourceKey.Split('.');
            if (s.Length == 2 && resourcePath != "")
            {
                var fName = s[0];
                var rKey = s[1];
                var relativefilename = resourcePath.TrimEnd('/') + "/" + fName + ".ascx.resx";
                var fullFileName = System.Web.Hosting.HostingEnvironment.MapPath(relativefilename);
                if (!String.IsNullOrEmpty(fullFileName) && System.IO.File.Exists(fullFileName))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(fullFileName);
                    var xmlNodList = xmlDoc.SelectNodes("root/data[starts-with(./@name,'" + rKey + ".')]");
                    if (xmlNodList != null)
                    {
                        foreach (XmlNode nod in xmlNodList)
                        {
                            if (nod.Attributes != null)
                            {
                                var n = nod.Attributes["name"].Value;
                                if (lang == "") lang = Utils.GetCurrentCulture();
                                var rtnValue = Localization.GetString(n, relativefilename, PortalSettings.Current, lang, true);
                                if (!rtnList.ContainsKey(n.Replace(rKey + ".", "")))
                                {
                                    rtnList.Add(n.Replace(rKey + ".", ""), rtnValue);
                                }
                            }
                        }
                    }
                }

                Utils.SetCache(ckey, rtnList, DateTime.Now.AddMinutes(20));
            }
            return rtnList;
        }

        public static void ClearPortalCache(int portalId)
        {
            DataCache.ClearPortalCache(portalId, true);
        }


        #region "encryption"

        public static String Encrypt(String value, String passkey = "")
        {
            var objSec = new PortalSecurity();
            if (value == null) return "";
            if (passkey == "")
            {
                var ps = GetCurrentPortalSettings();
                passkey = ps.GUID.ToString();
            }
            return objSec.Encrypt(passkey, value);
        }

        public static String Decrypt(String value, String passkey = "")
        {
            var objSec = new PortalSecurity();
            if (value == null) return "";
            if (passkey == "")
            {
                var ps = GetCurrentPortalSettings();
                passkey = ps.GUID.ToString();
            }
            return objSec.Decrypt(passkey, value);
        }

        #endregion

    }
}
