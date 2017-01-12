using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using NBrightCore.common;
using NBrightCore.images;
using NBrightCore.providers;

namespace NBrightCore.render
{

    //public delegate void UploadFileCompleted();

    public class GenXmlFunctions
    {

        //public static event UploadFileCompleted FileHasBeenUploaded;

        public static void ForceDocDownload(Repeater rpData, string fileUploadId, string folderMapPath,
                                            HttpResponse response)
        {
            var fup = (FileUpload) rpData.Items[0].FindControl(fileUploadId);
            if ((fup != null))
            {
                var g = GetHiddenField(rpData, "hid" + fup.ID);
                if (!String.IsNullOrEmpty(g))
                {
                    Utils.ForceDocDownload(folderMapPath.TrimEnd(Convert.ToChar(@"\")) + @"\" + g,
                                           GetField(rpData, "txt" + fup.ID), response);
                }
            }
        }

        public static void DeleteAllUploadedFiles(string itemXml, string folderMapPath,bool thumbsOnly = false)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(itemXml);

            //need to loop on the genxml/files/ctrl section to get the ctrl name 
            var nodList1 = xmlDoc.SelectNodes("genxml/files/ctrl");
            if (nodList1 != null)
            {
                foreach (XmlNode nod in nodList1)
                {
                    var fname = xmlDoc.SelectSingleNode("genxml/hidden/hid" + nod.InnerText + "[1]");
                    if (fname != null)
                    {
                        if (!thumbsOnly)
                        {
                            Utils.DeleteSysFile(folderMapPath.TrimEnd(Convert.ToChar(@"\")) + "\\" + fname.InnerXml);
                        }
                        var xmlNod2 = xmlDoc.SelectSingleNode("/genxml/hidden/thumbsize");
                        if ((xmlNod2 != null))
                        {
                            var tbSize = xmlNod2.InnerXml.Split(',');
                            foreach (var tb in tbSize)
                            {
                                Utils.DeleteSysFile(
                                    ImgUtils.GetThumbFilePathName(
                                        folderMapPath.TrimEnd(Convert.ToChar(@"\")) + "\\" + fname.InnerXml,
                                        ImgUtils.GetThumbWidth(tb), ImgUtils.GetThumbHeight(tb)));
                            }
                        }
                    }
                }
            }
        }


        public static string DeleteFileByCtrlName(string xmlData, string ctrlId, string folderMapPath)
        {
            if (!string.IsNullOrEmpty(xmlData))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlData);
                var xmlNods = xmlDoc.SelectNodes("genxml/files/*");
                if (xmlNods != null)
                {
                    foreach (XmlNode xmlNod in xmlNods)
                    {
                        var ctrlName = xmlNod.InnerText;
                        if (ctrlId.ToLower() == ctrlName.ToLower())
                        {
                            var thumbs = GetGenXmlValue(xmlData, "genxml/hidden/thumbsize");
                            var fName = GetGenXmlValue(xmlData, "genxml/hidden/hid" + ctrlName);
                            var lang = GetGenXmlValue(xmlData, "genxml/hidden/lang");
                            if (!String.IsNullOrEmpty(fName))
                            {
                                DeleteFile(fName, folderMapPath, thumbs);
                                if (!String.IsNullOrEmpty(lang))
                                {
                                    DeleteFile(lang + fName, folderMapPath, thumbs);
                                }
                                xmlData = SetGenXmlValue(xmlData, "genxml/hidden/hid" + ctrlName, "");
                                xmlData = SetGenXmlValue(xmlData, "genxml/hidden/hidinfo" + ctrlName, "");
                                xmlData = SetGenXmlValue(xmlData, "genxml/textbox/txt" + ctrlName, "");
                            }
                        }
                    }
                }
            }
            return xmlData;
        }

        public static string DeleteFile(string xmlData, string folderMapPath)
        {
            if (!string.IsNullOrEmpty(xmlData))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlData);
                var xmlNods = xmlDoc.SelectNodes("genxml/files/*");
                if (xmlNods != null)
                {
                    foreach (XmlNode xmlNod in xmlNods)
                    {
                        var ctrlName = xmlNod.InnerText;
                        var thumbs = GetGenXmlValue(xmlData, "genxml/hidden/thumbsize");
                        var fName = GetGenXmlValue(xmlData, "genxml/hidden/hid" + ctrlName);
                        var lang = GetGenXmlValue(xmlData, "genxml/hidden/lang");
                        if (!String.IsNullOrEmpty(fName))
                        {
                            DeleteFile(fName, folderMapPath, thumbs);
                            if (!String.IsNullOrEmpty(lang))
                            {
                                DeleteFile(lang + fName, folderMapPath, thumbs);
                            }
                            xmlData = SetGenXmlValue(xmlData, "genxml/hidden/hid" + ctrlName, "");
                            xmlData = SetGenXmlValue(xmlData, "genxml/hidden/hidinfo" + ctrlName, "");
                            xmlData = SetGenXmlValue(xmlData, "genxml/textbox/txt" + ctrlName, "");
                        }
                    }
                }
            }
            return xmlData;
        }


        public static void DeleteFile(string fileName, string folderMapPath, string thumbSize)
        {
            Utils.DeleteSysFile(folderMapPath.TrimEnd(Convert.ToChar(@"\")) + "\\" + fileName);
            if (thumbSize != "")
            {
                var tbSize = thumbSize.Split(',');
                foreach (var tb in tbSize)
                {
                    Utils.DeleteSysFile(ImgUtils.GetThumbFilePathName(folderMapPath.TrimEnd(Convert.ToChar(@"\")) + "\\" + fileName, ImgUtils.GetThumbWidth(tb), ImgUtils.GetThumbHeight(tb)));
                }
            }
        }


        public static void UploadImgFile(Repeater rpData, string fileUploadId, string folderMapPath)
        {
            UploadImgFile(rpData, fileUploadId, folderMapPath, false);
        }

        public static void UploadImgFile(Repeater rpData, string fileUploadId, string folderMapPath, bool addToCanvas)
        {
            if (!folderMapPath.EndsWith("\\"))
            {
                folderMapPath = folderMapPath + "\\";
            }

            UploadFile(rpData, fileUploadId, folderMapPath, true);
            var ctrl = ((HtmlGenericControl) rpData.Items[0].FindControl("hid" + fileUploadId));
            if (ctrl != null)
            {
                var fName = ctrl.Attributes["value"];

                if (addToCanvas)
                {
                    ImgUtils.AddToCanvas(folderMapPath + fName, folderMapPath + "canvas.jpg");
                }                
            }

        }

        public static void UploadFile(Repeater rpData, string fileUploadId, string folderMapPath)
        {
            UploadFile(rpData, fileUploadId, folderMapPath, false);
        }

        public static void UploadFile(Repeater rpData, string fileUploadId, string folderMapPath, bool imagesOnly)
        {
            UploadFile(rpData, fileUploadId, folderMapPath, "", false, imagesOnly);
        }

        public static void UploadFile(Repeater rpData, string fileUploadId, string folderMapPath, string lang, bool noResize, bool imagesOnly)
        {
            UploadFile(rpData.Items[0], fileUploadId, folderMapPath, "", false, imagesOnly);
        }

        public static void UploadFile(RepeaterItem rpItem, string fileUploadId, string folderMapPath, string lang = "", bool noResize = false , bool imagesOnly = false)
        {
            string strGuid = null;
            var blnDoUpload = true;

            var fup = (FileUpload)rpItem.FindControl(fileUploadId);
            if ((fup != null))
            {

                if (!String.IsNullOrEmpty(fup.FileName))
                {
                    if (imagesOnly)
                    {
                        if (!ImgUtils.IsImageFile(Path.GetExtension(fup.FileName)))
                        {
                            blnDoUpload = false;
                        }
                    }

                    if (blnDoUpload)
                    {
                        if (Directory.Exists(folderMapPath))
                        {
                            //check content length of upload file.
                            //--if zero then file may have already been uploaded.
                            //--so do not upload again, otherwise we'll have an invalid file on the system.

                            if ((fup.PostedFile.ContentLength > 0) & (fup.Enabled))
                            {
                                var g = GetHiddenField(rpItem, "hid" + fup.ID);
                                if (!String.IsNullOrEmpty(g))
                                {
                                    // only delete if language is not used or matching.
                                    if (g.StartsWith(lang + "_"))
                                    {
                                        //check if we've turned off deletefile
                                        if (fup.Attributes["deletefile"] == null || fup.Attributes["deletefile"].ToLower() == "true")
                                        {
                                            Utils.DeleteSysFile(folderMapPath.TrimEnd(Convert.ToChar("\\")) + "\\" + g);
                                            if (!String.IsNullOrEmpty(GetHiddenField(rpItem, "Thumbsize")))
                                            {
                                                var tbSize = GetHiddenField(rpItem, "Thumbsize").Split(',');
                                                foreach (var tb in tbSize)
                                                {
                                                    Utils.DeleteSysFile(
                                                        ImgUtils.GetThumbFilePathName(folderMapPath.TrimEnd(Convert.ToChar(@"\")) + "\\" + g,
                                                                                      ImgUtils.GetThumbWidth(tb), ImgUtils.GetThumbHeight(tb)));
                                                }

                                            }
                                        }
                                    }
                                }

                                //see if we have a friendly file flag
                                var strFriendlyName = GetHiddenField(rpItem, "FriendlyFileNames");
                                var strFileUploadName = Utils.CleanInput(fup.FileName);
                                var strhiddenFileName = GetHiddenField(rpItem, "HiddenFileNames");

                                if (String.IsNullOrEmpty(strFriendlyName))
                                {
                                    strGuid = Guid.NewGuid().ToString();
                                    var strExt = Path.GetExtension(strFileUploadName);
                                    if (String.IsNullOrEmpty(strExt)) strExt = "";
                                    if ((ImgUtils.IsImageFile(strExt) || (strExt == ".pdf") || (strExt == ".mp3")) && !strhiddenFileName.Contains(strExt))
                                    {
                                        // is image/pdf/mp3 so assume it's save to use the extension.
                                        strGuid += strExt;
                                    }
                                }
                                else
                                {
                                    var extension = Path.GetExtension(strFileUploadName);
                                    if (extension != null && extension.ToLower() != ".exe" && extension.ToLower() != ".js" && extension.ToLower() != ".aspx")
                                    {
                                        strGuid = Utils.StripAccents(strFileUploadName);
                                    }
                                }

                                //place language at front of filename. (Friendly names don;t support langauge)
                                if (lang != "" & !String.IsNullOrEmpty(strFriendlyName)) strGuid = lang + "_" + strGuid;

                                var newFileName = folderMapPath.TrimEnd(Convert.ToChar("\\")) + "\\" + strGuid;
                                fup.SaveAs(newFileName);

                                if (ImgUtils.IsImageFile(Path.GetExtension(strFileUploadName)) & !noResize)
                                {
                                    if (String.IsNullOrEmpty(strFriendlyName))
                                    {
                                        ResizeImage(rpItem, newFileName, fup.ID, folderMapPath, lang,"");
                                    }
                                    else
                                    {
                                        ResizeImage(rpItem, newFileName, fup.ID, folderMapPath, lang, Utils.StripAccents(strFileUploadName));
                                    }
                                }
                                else
                                {
                                    SetHiddenField(rpItem, "hidInfo" + fup.ID, ImgUtils.IsImageFile(Path.GetExtension(strFileUploadName)) ? "Img=True" : "Img=False");
                                    SetHiddenField(rpItem, "hid" + fup.ID, strGuid);
                                    SetHiddenField(rpItem, "hidExt" + fup.ID, Path.GetExtension(strFileUploadName));
                                }


                                // NOTE: This event creates a memory leak [TODO: find another way to do the resync of the filesystem]
                                //FileHasBeenUploaded(); // trigger event so file sync can be done if required.

                            }


                        }
                        else
                        {
                            SetField(rpItem, "txt" + fup.ID, "ERROR! No upload directory exists.");
                        }
                        // File uploaded already so disable, so it doesn;t load again.
                        fup.Enabled = false;
                    }
                }
            }
        }

        public static void ResizeImage(Repeater rpData, string originalFileName, string fupId, string folderMapPath)
        {
            ResizeImage(rpData, originalFileName, fupId, folderMapPath, "");
        }

        public static void ResizeImage(Repeater rpData, string originalFileName, string fupId, string folderMapPath, string lang)
        {
            ResizeImage(rpData, originalFileName, fupId, folderMapPath, lang, "");
        }

        public static void ResizeImage(Repeater rpData, string originalFileName, string fupId, string folderMapPath, string lang, string friendlyFileName)
        {
            ResizeImage(rpData.Items[0], originalFileName, fupId, folderMapPath, lang, friendlyFileName);
        }

        public static void ResizeImage(RepeaterItem rpItem, string originalFileName, string fupId, string folderMapPath, string lang, string friendlyFileName)
        {
            string strGUIDJPG;
            var newFileName = originalFileName;
            var fileext = Path.GetExtension(originalFileName);

            if (String.IsNullOrEmpty(friendlyFileName))
            {
                strGUIDJPG = lang + "_" + Guid.NewGuid() + fileext;
            }
            else
            {
                if (lang != "")
                {
                    strGUIDJPG = lang + "_" + friendlyFileName;
                }
                else
                {
                    //rename the friendlyname so we don;t get a lock on resize.
                    strGUIDJPG = friendlyFileName;
                    newFileName = Path.GetDirectoryName(newFileName) + "\\gen_" + Path.GetFileName(newFileName);
                    File.Move(originalFileName, newFileName);
                    originalFileName = newFileName; // rename orignal so we delete the correct img at the end.
                }
            }

            var newImageFileName = folderMapPath.TrimEnd(Convert.ToChar("\\")) + "\\" + strGUIDJPG;
            var imgSize = Utils.IsNumeric(GetHiddenField(rpItem, "ImageResize")) ? Convert.ToInt32(GetHiddenField(rpItem, "ImageResize")) : 640;
            var extension = Path.GetExtension(strGUIDJPG);
            if (extension != null && extension.ToLower() == ".png")
            {
                ImgUtils.ResizeImageToPng(newFileName, newImageFileName, imgSize);
            }
            else
            {
                ImgUtils.ResizeImageToJpg(newFileName, newImageFileName, imgSize);
            }
            SetHiddenField(rpItem, "hid" + fupId, strGUIDJPG);

            //get image orientation
            SetHiddenField(rpItem, "hidInfo" + fupId, "Img=True");

            //Make thumbnail is resize thumbnail hidden field is there.
            if (!String.IsNullOrEmpty(GetHiddenField(rpItem, "Thumbsize")))
            {
                ImgUtils.CreateThumbOnDisk(newImageFileName, GetHiddenField(rpItem, "Thumbsize"));
            }
            Utils.DeleteSysFile(originalFileName);
        }


        public static CheckBoxStats GetCheckBoxStats(Repeater rpData, string chkListId)
        {
            return GetCheckBoxStats(rpData, chkListId, "");
        }

        public static CheckBoxStats GetCheckBoxStats(Repeater rpData, string chkListId, string completedCheckBoxId)
        {
            var ary = chkListId.Split(',');
            return GetCheckBoxStats(rpData, ary, completedCheckBoxId);
        }

        public static CheckBoxStats GetCheckBoxStats(Repeater rpData, string[] chkListId)
        {
            return GetCheckBoxStats(rpData, chkListId, "");
        }

        public static CheckBoxStats GetCheckBoxStats(Repeater rpData, string[] chkListId, string completedCheckBoxId)
        {
            var cbs = new CheckBoxStats {CheckedCount = 0, Count = 0, PercentChecked = 0, UnCheckedCount = 0};

            Control ctrl;
            for (var lp = 0; lp <= chkListId.GetUpperBound(0); lp++)
            {
                ctrl = rpData.Items[0].FindControl(chkListId[lp]);
                if ((ctrl != null))
                {
// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
                    if (ctrl is CheckBox)
// ReSharper restore CanBeReplacedWithTryCastAndCheckForNull
                    {
                        cbs.Count += 1;
                        if (((CheckBox) ctrl).Checked)
                        {
                            cbs.CheckedCount += 1;
                        }
                        else
                        {
                            cbs.UnCheckedCount += 1;
                        }
                    }
// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
                    if (ctrl is CheckBoxList)
// ReSharper restore CanBeReplacedWithTryCastAndCheckForNull
                    {
                        var cbl = (CheckBoxList) ctrl;
                        for (var lp2 = 0; lp2 <= cbl.Items.Count - 1; lp2++)
                        {
                            cbs.Count += 1;
                            if (cbl.Items[lp2].Selected)
                            {
                                cbs.CheckedCount += 1;
                            }
                            else
                            {
                                cbs.UnCheckedCount += 1;
                            }
                        }
                    }
                }
            }

            if (cbs.Count > 0)
            {
                cbs.PercentChecked = Convert.ToInt32((100/cbs.Count)*cbs.CheckedCount);
            }

            //if the completedCheckBoxID is checked that make the group 100% completed
            ctrl = rpData.Items[0].FindControl(completedCheckBoxId);
            if ((ctrl != null))
            {
                if (((CheckBox) ctrl).Checked)
                {
                    cbs.PercentChecked = 100;
                }
            }

            return cbs;
        }


        public static string GetCheckBoxListValues(XmlDocument xmlDoc, string xpath)
        {
            var strRtn = "";
            if (xmlDoc != null)
            {
                var xmlNods = xmlDoc.SelectNodes(xpath);

                if (xmlNods != null)
                {
                    foreach (XmlNode nod in xmlNods)
                    {
                        if (nod.Attributes != null && nod.Attributes["value"] != null && nod.Attributes["value"].InnerText.ToLower() == "true")
                        {
                            strRtn += nod.InnerText + ",";
                        }
                    }
                }
            }
            return strRtn.TrimEnd(',');
        }

        public static void DisableTextBox(Repeater rpData, string fieldId)
        {
            DisableTextBox(rpData, fieldId, 0);
        }

        public static void DisableTextBox(Repeater rpData, string fieldId, int rowIndex)
        {
            DisableCtrl(rpData, fieldId, rowIndex);
        }

        public static void EnableTextBox(Repeater rpData, string fieldId)
        {
            EnableTextBox(rpData, fieldId, 0);
        }

        public static void EnableTextBox(Repeater rpData, string fieldId, int rowIndex)
        {
            EnableCtrl(rpData, fieldId, rowIndex);
        }

        public static void DisableCtrl(Repeater rpData, string fieldId)
        {
            DisableCtrl(rpData, fieldId, 0);
        }

        public static void DisableCtrl(Repeater rpData, string fieldId, int rowIndex)
        {
            var ctrl = rpData.Items[rowIndex].FindControl(fieldId);
            if (ctrl is TextBox)
            {
                ((TextBox) ctrl).Enabled = false;
            }
            else if (ctrl is DropDownList)
            {
                ((DropDownList) ctrl).Enabled = false;
            }
            else if (ctrl is RadioButtonList)
            {
                ((RadioButtonList) ctrl).Enabled = false;
            }
            else if (ctrl is CheckBox)
            {
                ((CheckBox) ctrl).Enabled = false;
            }
        }

        public static void EnableCtrl(Repeater rpData, string fieldId)
        {
            EnableCtrl(rpData, fieldId, 0);
        }

        public static void EnableCtrl(Repeater rpData, string fieldId, int rowIndex)
        {
            var ctrl = rpData.Items[rowIndex].FindControl(fieldId);

            if (ctrl is TextBox)
            {
                ((TextBox) ctrl).Enabled = true;
            }
            else if (ctrl is DropDownList)
            {
                ((DropDownList) ctrl).Enabled = true;
            }
            else if (ctrl is RadioButtonList)
            {
                ((RadioButtonList) ctrl).Enabled = true;
            }
            else if (ctrl is CheckBox)
            {
                ((CheckBox) ctrl).Enabled = true;
            }
        }

        public static void HideField(Repeater rpData, string fieldId, int rowIndex = 0)
        {
            var ctrl = rpData.Items[rowIndex].FindControl(fieldId);
            if ((ctrl != null))
            {
                ctrl.Visible = false;
            }
        }

        public static void ShowField(Repeater rpData, string fieldId, int rowIndex = 0)
        {
            var ctrl = rpData.Items[rowIndex].FindControl(fieldId);
            if ((ctrl != null))
            {
                ctrl.Visible = true;
            }
        }

        public static int GetFieldAsInteger(Repeater rpData, string fieldId, int rowIndex = 0)
        {
            var strTemp = GetField(rpData, fieldId, rowIndex);
            return Utils.IsNumeric(strTemp) ? Convert.ToInt32(strTemp) : 0;
        }

        public static int GetFieldAsInteger(RepeaterItem rpItem, string fieldId)
        {
            var strTemp = GetField(rpItem, fieldId);
            return Utils.IsNumeric(strTemp) ? Convert.ToInt32(strTemp) : 0;
        }

        public static double GetFieldAsDouble(Repeater rpData, string fieldId, int rowIndex = 0)
        {
            var strTemp = GetField(rpData, fieldId, rowIndex);
            return Utils.IsNumeric(strTemp,Utils.GetCurrentCulture()) ? Convert.ToDouble(strTemp) : 0;
        }

        public static double GetFieldAsDouble(RepeaterItem rpItem, string fieldId)
        {
            var strTemp = GetField(rpItem, fieldId);
            return Utils.IsNumeric(strTemp, Utils.GetCurrentCulture()) ? Convert.ToDouble(strTemp) : 0;
        }

        public static string GetField(Repeater rpData, string fieldId, int rowIndex = 0)
        {
            if (rowIndex < rpData.Items.Count)
            {
                return GetField(rpData.Items[rowIndex], fieldId);                
            }
            return "";
        }

        public static string GetField(RepeaterItem rpItem, string fieldId,bool rtnText = false)
        {
            var ctrl = rpItem.FindControl(fieldId);
            if (ctrl == null)
            {
                //search for text editor (For Legacy issue, unsure we need it now. Review Later)
                ctrl = rpItem.FindControl("gte" + fieldId);
            }
            if (ctrl is TextBox)
            {
                return ((TextBox) ctrl).Text;
            }
            if (ctrl is HtmlGenericControl)
            {
                return ((HtmlGenericControl)ctrl).Attributes["value"];
            }
            if (ctrl is Label)
            {
                return ((Label) ctrl).Text;
            }
            if (ctrl is DropDownList)
            {
                if (rtnText)
                {
                    return ((DropDownList)ctrl).SelectedItem.Text;    
                }
                return ((DropDownList) ctrl).SelectedValue;
            }
            if (ctrl is RadioButtonList)
            {
                if (rtnText)
                {
                    return ((RadioButtonList)ctrl).SelectedItem.Text;
                }
                return ((RadioButtonList)ctrl).SelectedValue;
            }
            if (ctrl is CheckBox)
            {
                return ((CheckBox) ctrl).Checked.ToString(CultureInfo.InvariantCulture);
            }
            if (ctrl is HiddenField)
            {
                return ((HiddenField)ctrl).Value;
            }

            //check for any template providers.
            var providerList = GenXProviderManager.ProviderList;
            if (providerList != null)
            {
                foreach (var prov in providerList)
                {
                    return prov.Value.GetField(ctrl);
                }
            }


            return "";
        }

        public static void SetField(Repeater rpData, string fieldId, string newValue, int rowIndex = 0)
        {
            SetField(rpData.Items[rowIndex],fieldId,newValue);
        }

        public static void SetField(RepeaterItem rpItem, string fieldId, string newValue)
        {
            var ctrl = rpItem.FindControl(fieldId);
            if (ctrl == null)
            {
                //search for text editor (For Legacy issue, unsure we need it now. Review Later)
                ctrl = rpItem.FindControl("gte" + fieldId);
            }

            if (ctrl is TextBox)
            {
                ((TextBox) ctrl).Text = newValue;
            }
            else if (ctrl is HtmlGenericControl)
            {
                ((HtmlGenericControl)ctrl).Attributes["value"] = newValue;
            }
            else if (ctrl is Label)
            {
                ((Label) ctrl).Text = newValue;
            }
            else if (ctrl is DropDownList)
            {
                if ((((DropDownList) ctrl).Items.FindByValue(newValue) != null))
                {
                    ((DropDownList) ctrl).SelectedValue = newValue;
                }
            }
            else if (ctrl is RadioButtonList)
            {
                if ((((RadioButtonList) ctrl).Items.FindByValue(newValue) != null))
                {
                    ((RadioButtonList) ctrl).SelectedValue = newValue;
                }
            }
            else if (ctrl is CheckBox)
            {
                ((CheckBox) ctrl).Checked = Convert.ToBoolean(newValue);
            }

            //check for any template providers.
            var providerList = GenXProviderManager.ProviderList;
            if (providerList != null)
            {
                foreach (var prov in providerList)
                {
                    prov.Value.SetField(ctrl, newValue);
                }
            }

        }

        public static string SetHiddenField(string dataXml, string FieldId, string newValue, bool cdata = true)
        {
            var xPath = "genxml/hidden/" + FieldId.ToLower();
            return SetGenXmlValue(dataXml, xPath, newValue, cdata);
        }

        public static void SetFieldItemList(Repeater rpData, string fieldId, ListItemCollection newValue, int rowIndex = 0)
        {
            if ((newValue != null))
            {
                var ctrl = rpData.Items[rowIndex].FindControl(fieldId);
                if (ctrl is DropDownList)
                {
                    foreach (ListItem li in newValue)
                    {
                        ((DropDownList) ctrl).Items.Add(li);
                    }
                }
                else if (ctrl is RadioButtonList)
                {
                    foreach (ListItem li in newValue)
                    {
                        ((RadioButtonList) ctrl).Items.Add(li);
                    }
                }
                else if (ctrl is CheckBoxList)
                {
                    foreach (ListItem li in newValue)
                    {
                        ((CheckBoxList)ctrl).Items.Add(li);
                    }
                }
            }
        }


        public static ListItemCollection GetFieldItemList(Repeater rpData, string fieldId, int rowIndex = 0)
        {
            var lic = new ListItemCollection();
            if (rpData.Items.Count > 0)
            {
                var ctrl = rpData.Items[rowIndex].FindControl(fieldId);
                if (ctrl is DropDownList)
                {
                    foreach (ListItem li in ((DropDownList) ctrl).Items)
                    {
                        lic.Add(li);
                    }
                }
                else if (ctrl is RadioButtonList)
                {
                    foreach (ListItem li in ((RadioButtonList) ctrl).Items)
                    {
                        lic.Add(li);
                    }
                }
                else if (ctrl is CheckBoxList)
                {
                    foreach (ListItem li in ((CheckBoxList) ctrl).Items)
                    {
                        lic.Add(li);
                    }
                }
            }
            return lic;
        }


        public static string GetHiddenField(Repeater rpData, string fieldId, int rowIndex = 0 )
        {
            if (rpData.Items.Count > 0 )
            {
                return GetHiddenField(rpData.Items[rowIndex], fieldId);                
            }
            return "";
        }

        public static string GetHiddenField(RepeaterItem rpItem, string fieldId)
        {
            var hid = (HtmlGenericControl)rpItem.FindControl(fieldId);
            if ((hid != null))
            {
                return hid.Attributes["value"];
            }
            return "";
        }

        public static string GetHiddenField(string dataXml, string FieldId)
        {
            var xPath = "genxml/hidden/" + FieldId.ToLower();
            return GetGenXmlValue(dataXml, xPath);
        }

        public static void SetHiddenField(Repeater rpData, string fieldId, string newValue, int rowIndex = 0)
        {
            if (rpData.Items.Count > 0 )
            {
                SetHiddenField(rpData.Items[rowIndex], fieldId.ToLower(), newValue);                
            }
        }

        public static void SetHiddenField(RepeaterItem rpItem, string fieldId, string newValue)
        {
            //assign hidden fields
            var hid = (HtmlGenericControl)rpItem.FindControl(fieldId);
            if ((hid != null))
            {
                if (hid.Attributes["datatype"] != null)
                {

                    if (hid.Attributes["datatype"].ToLower() == "double")
                    {
                        if (Utils.IsNumeric(newValue))
                        {
                            hid.Attributes["value"] = newValue;
                        }
                        else
                        {
                            hid.Attributes["value"] = "0";                            
                        }
                    }
                    else if (hid.Attributes["datatype"].ToLower() == "date")
                    {
                        if (Utils.IsDate(newValue, Utils.GetCurrentCulture()))
                        {
                            hid.Attributes["value"] = Convert.ToDateTime(newValue).ToString("s");
                        }
                        else
                        {
                            hid.Attributes["value"] = "";
                        }
                    }
                    else
                    {
                        hid.Attributes["value"] = newValue;
                    }
                }
                else
                {
                    hid.Attributes["value"] = newValue;
                }
            }
        }

        public static Repeater InitRepeater(object objInfo, string templateText)
        {
            return InitRepeater(objInfo, templateText, "en-GB");
        }

        public static Repeater InitRepeater(object objInfo, string templateText, string lang)
        {
            var dlGen = new Repeater {ItemTemplate = new GenXmlTemplate(templateText)};
            var l = new List<object> {objInfo};
            dlGen.DataSource = l;
            dlGen.DataBind();
            return dlGen;
        }


        public static string GetGenXml(Repeater rpGenXml)
        {
            return GetGenXml(rpGenXml, "", "");
        }

        public static string GetGenXml(Repeater rpGenXml, int rowIndex)
        {
            return GetGenXml(rpGenXml, "", "", rowIndex);
        }

        public static string GetGenXml(Repeater rpGenXml, int rowIndex, string xmlRootName)
        {
            return GetGenXml(rpGenXml, "", "", rowIndex, xmlRootName);
        }

        public static string GetGenXml(Repeater rpGenXml, string originalXml, string folderMapPath, int rowIndex = 0, string xmlRootName = "genxml")
        {
            //check row exists (0 based)
            if (rpGenXml.Items.Count <= rowIndex | rpGenXml.Items.Count == 0)
            {
                return "";
            }

            var rpItem = rpGenXml.Items[rowIndex];
            return GetGenXml(rpItem, originalXml, folderMapPath, xmlRootName);
        }

        public static string GetGenXml(RepeaterItem rpItem, string originalXml = "", string folderMapPath = "", string xmlRootName = "genxml")
        {
            var ddlCtrls = new List<Control>();
            var rblCtrls = new List<Control>();
            var chkCtrls = new List<Control>();
            var txtCtrls = new List<Control>();
            var hidCtrls = new List<Control>();
            var hidfieldCtrls = new List<Control>();
            var fupCtrls = new List<Control>();
            var chkboxCtrls = new List<Control>();
            var genCtrls = new List<Control>();
            var repeaterCtrls = new List<Control>();
            
            var lang = "";

            var providerList = GenXProviderManager.ProviderList;

            //build list of controls
            // if we have disabled and hidden the control then assume it does not want to be in the XML output.
            // (NOTE: Ideally this should be done with testing EnabledViewState which is avalable on base control type.
            //   turning off viewstate in control creation works, but turning viewstate on in databind does not work for a dropdownlist??...like to find out why, but not got the time!)
            foreach (Control ctrl in rpItem.Controls)
            {
                if (ctrl is Literal) continue;
                if (ctrl is DropDownList)
                {
                    var ctl = (DropDownList) ctrl;
                    if (ctl.Enabled & ctl.Visible) ddlCtrls.Add(ctrl);
                }
                else if (ctrl is CheckBoxList)
                {
                    var ctl = (CheckBoxList) ctrl;
                    if (ctl.Enabled & ctl.Visible) chkCtrls.Add(ctrl);
                }
                else if (ctrl is CheckBox)
                {
                    var ctl = (CheckBox) ctrl;
                    if (ctl.Enabled & ctl.Visible) chkboxCtrls.Add(ctrl);
                }
                else if (ctrl is TextBox)
                {
                    var ctl = (TextBox) ctrl;
                    // if we have a resourcekeysave attr don't save it into the data xml, this needs to update the resx file, using the GetGenXmlResx
                    bool isResourceKeySave = ctl.Attributes["resourcekeysave"] != null;
                    if (ctl.Enabled & ctl.Visible & !isResourceKeySave) txtCtrls.Add(ctrl);
                }
                else if (ctrl is RadioButtonList)
                {
                    var ctl = (RadioButtonList) ctrl;
                    if (ctl.Enabled & ctl.Visible) rblCtrls.Add(ctrl);
                }
                else if (ctrl is Repeater)
                {
                    var ctl = (Repeater)ctrl;
                    if (ctl.Visible) repeaterCtrls.Add(ctrl);
                }
                else if (ctrl is HtmlGenericControl)
                {
                    hidCtrls.Add(ctrl);
                    if (ctrl.ID.ToLower() == "lang")
                    {
                        // set lang , so file uploads are marked as multiple language (used to stop deletion)
                        var c = (HtmlGenericControl) ctrl;
                        lang = c.Attributes["value"];
                    }
                }
                else if (ctrl is HiddenField)
                {
                    hidfieldCtrls.Add(ctrl);
                }
                else if (ctrl is FileUpload)
                {
                    fupCtrls.Add(ctrl);
                }
                else
                {
                    // get any other controls to pass to provider.
                    genCtrls.Add(ctrl);
                }
            }

            //load original XML for update  
            var xmlDoc = new XmlDocument();
            if (!String.IsNullOrEmpty(originalXml))
            {
                xmlDoc.LoadXml(originalXml);
            }

            //Create XML
            var strXml = "";
            strXml += "<" + xmlRootName + ">";

            //Process embeded repeaters by recussion
            if (repeaterCtrls.Count > 0)
            {
                strXml += "<repeaters>";
                foreach (var rptctrl in repeaterCtrls)
                {
                    var rpCtrl = (Repeater)rptctrl;
                    strXml += "<repeater>";
                    foreach (RepeaterItem i in rpCtrl.Items)
                    {
                        strXml += GetGenXml(i);
                    }
                    strXml += "</repeater>";
                }
                strXml += "</repeaters>";                
            }


            //Upload any files that have been selected
            strXml += "<files>";
            foreach (FileUpload fupCtrl in fupCtrls)
            {
                if (!String.IsNullOrEmpty(folderMapPath))
                {
                    if (!String.IsNullOrEmpty(fupCtrl.FileName))
                    {
                        UploadFile(rpItem, fupCtrl.ID, folderMapPath,lang);
                    }
                }
                strXml += "<ctrl>";
                strXml += fupCtrl.ID.ToLower();
                strXml += "</ctrl>";
            }
            strXml += "</files>";

            strXml += "<hidden>";
            foreach (HtmlGenericControl hidCtrl in hidCtrls)
            {
                if (!String.IsNullOrEmpty(originalXml))
                {
                    if (hidCtrl.ID.ToLower().StartsWith("dbl"))
                    {
                        ReplaceXmlNode(xmlDoc, xmlRootName + "/hidden/" + hidCtrl.ID.ToLower(), Utils.FormatToSave(hidCtrl.Attributes["value"], TypeCode.Double));
                    }
                    else
                    {
                        ReplaceXmlNode(xmlDoc, xmlRootName + "/hidden/" + hidCtrl.ID.ToLower(), Utils.FormatToSave(hidCtrl.Attributes["value"]));
                    }
                }
                else
                {
                    if (hidCtrl.ID.ToLower().StartsWith("dbl"))
                    {
                        strXml += "<" + hidCtrl.ID.ToLower() + " datatype=\"double\"><![CDATA[";
                        strXml += Utils.FormatToSave(hidCtrl.Attributes["value"], TypeCode.Double);
                    }
                    else if (hidCtrl.ID.ToLower().StartsWith("dte"))
                    {
                        strXml += "<" + hidCtrl.ID.ToLower() + " datatype=\"date\"><![CDATA[";
                        strXml += Utils.FormatToSave(hidCtrl.Attributes["value"], TypeCode.DateTime);
                    }
                    else if (hidCtrl.ID.ToLower().StartsWith("html"))
                    {
                        strXml += "<" + hidCtrl.ID.ToLower() + " datatype=\"html\"><![CDATA[";
                        strXml += hidCtrl.Attributes["value"];
                    }
                    else
                    {
                        strXml += "<" + hidCtrl.ID.ToLower() + "><![CDATA[";
                        strXml += hidCtrl.Attributes["value"];
                    }
                    strXml += "]]></" + hidCtrl.ID.ToLower() + ">";
                }
            }
            foreach (HiddenField hidCtrl in hidfieldCtrls)  //deal with true hidden field for postback.
            {
                if (!String.IsNullOrEmpty(originalXml))
                {
                    if (hidCtrl.ID.ToLower().StartsWith("dbl"))
                    {
                        ReplaceXmlNode(xmlDoc, xmlRootName + "/hidden/" + hidCtrl.ID.ToLower(), Utils.FormatToSave(hidCtrl.Value, TypeCode.Double));
                    }
                    else
                    {
                        ReplaceXmlNode(xmlDoc, xmlRootName + "/hidden/" + hidCtrl.ID.ToLower(), Utils.FormatToSave(hidCtrl.Value));
                    }
                }
                else
                {
                    if (hidCtrl.ID.ToLower().StartsWith("dbl"))
                    {
                        strXml += "<" + hidCtrl.ID.ToLower() + " datatype=\"double\"><![CDATA[";
                        strXml += Utils.FormatToSave(hidCtrl.Value, TypeCode.Double);
                    }
                    else if (hidCtrl.ID.ToLower().StartsWith("dte"))
                    {
                        strXml += "<" + hidCtrl.ID.ToLower() + " datatype=\"date\"><![CDATA[";
                        strXml += Utils.FormatToSave(hidCtrl.Value, TypeCode.DateTime);
                    }
                    else if (hidCtrl.ID.ToLower().StartsWith("xml"))
                    {
                        strXml += "<" + hidCtrl.ID.ToLower() + " datatype=\"xml\"><![CDATA[";
                        strXml += EncodeCDataTag(hidCtrl.Value);
                    }
                    else
                    {
                        strXml += "<" + hidCtrl.ID.ToLower() + "><![CDATA[";
                        strXml += hidCtrl.Value;
                    }
                    strXml += "]]></" + hidCtrl.ID.ToLower() + ">";
                }
            }
            strXml += "</hidden>";

            strXml += "<textbox>";
            foreach (TextBox txtCtrl in txtCtrls)
            {
                if (txtCtrl.Text.Contains("<![CDATA["))
                {
                    //convert cdata marks so it saves OK into XML (will need converting back)
                    txtCtrl.Text = EncodeCDataTag(txtCtrl.Text);
                }

                var dataTyp = "";
                if ((txtCtrl.Attributes["datatype"] != null))
                {
                    dataTyp = txtCtrl.Attributes["datatype"];
                }


                if (!String.IsNullOrEmpty(originalXml))
                {
                    if (dataTyp.ToLower() == "double")
                    {
                        ReplaceXmlNode(xmlDoc, xmlRootName + "/textbox/" + txtCtrl.ID.ToLower(), Utils.FormatToSave(txtCtrl.Text, TypeCode.Double));
                    }
                    else if (dataTyp.ToLower() == "date")
                    {
                        ReplaceXmlNode(xmlDoc, xmlRootName + "/textbox/" + txtCtrl.ID.ToLower(), Utils.FormatToSave(txtCtrl.Text, TypeCode.DateTime));
                    }
                    else if (dataTyp.ToLower() == "email")
                    {
                        //create spamsafe version
                        ReplaceXmlNode(xmlDoc, xmlRootName + "/textbox/" + txtCtrl.ID.ToLower() + "_spamsafe", Utils.CloakText(txtCtrl.Text), true);
                        //create spamsafe mailto version
                        ReplaceXmlNode(xmlDoc, xmlRootName + "/textbox/" + txtCtrl.ID.ToLower() + "_mailto", Utils.CloakText(String.Format("<a href='mailto{1}{0}'>{0}</a>", txtCtrl.Text, ":")),true);
                        //create normal version
                        ReplaceXmlNode(xmlDoc, xmlRootName + "/textbox/" + txtCtrl.ID.ToLower(), Utils.FormatToSave(txtCtrl.Text));
                    }
                    else
                    {
                        ReplaceXmlNode(xmlDoc, xmlRootName + "/textbox/" + txtCtrl.ID.ToLower(), Utils.FormatToSave(txtCtrl.Text));
                    }
                }
                else
                {

                    if (dataTyp.ToLower() == "double")
                    {
                        strXml += "<" + txtCtrl.ID.ToLower() + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                        strXml += Utils.FormatToSave(txtCtrl.Text, TypeCode.Double);
                    }
                    else if (dataTyp.ToLower() == "date")
                    {
                        strXml += "<" + txtCtrl.ID.ToLower() + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                        strXml += Utils.FormatToSave(txtCtrl.Text, TypeCode.DateTime);
                    }
                    else if (dataTyp.ToLower() == "email")
                    {
                        //create spamsafe version
                        strXml += "<" + txtCtrl.ID.ToLower() + "_spamsafe" + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                        strXml += Utils.CloakText(txtCtrl.Text);
                        strXml += "]]></" + txtCtrl.ID.ToLower() + "_spamsafe" + ">";
                        //create spamsafe mailto version
                        strXml += "<" + txtCtrl.ID.ToLower() + "_mailto" + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                        strXml += Utils.CloakText(String.Format("<a href='mailto{1}{0}'>{0}</a>", txtCtrl.Text, ":"));
                        strXml += "]]></" + txtCtrl.ID.ToLower() + "_mailto" + ">";

                        //create normal version
                        strXml += "<" + txtCtrl.ID.ToLower() + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                        strXml += txtCtrl.Text;
                    }
                    else
                    {
                        strXml += "<" + txtCtrl.ID.ToLower() + "><![CDATA[";
                        strXml += txtCtrl.Text;
                    }

                    strXml += "]]></" + txtCtrl.ID.ToLower() + ">";
                }
            }


            // Do any provider base textbox. (e.g. dnndatecontrol)
            if (providerList != null)
            {
                foreach (var prov in providerList)
                {
                    strXml += prov.Value.GetGenXmlTextBox(genCtrls, xmlDoc, originalXml, folderMapPath, xmlRootName);
                }
            }

            strXml += "</textbox>";

            strXml += "<checkbox>";
            foreach (CheckBox chkboxCtrl in chkboxCtrls)
            {
                if (!String.IsNullOrEmpty(originalXml))
                {
                    ReplaceXmlNode(xmlDoc, xmlRootName + "/checkbox/" + chkboxCtrl.ID.ToLower(), chkboxCtrl.Checked.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    strXml += "<" + chkboxCtrl.ID.ToLower() + "><![CDATA[";
                    // hard code "True" and "False", this is to make sure we have a constant across languages for testof.
                    // I know this should be done by using CultureInfo, but I'm unable to test that on different language servers and this is sure to work!
                    if (chkboxCtrl.Checked)
                        strXml += "True";
                    else
                        strXml += "False";
                    strXml += "]]></" + chkboxCtrl.ID.ToLower() + ">";
                }
            }
            strXml += "</checkbox>";

            strXml += "<dropdownlist>";
            foreach (DropDownList ddlCtrl in ddlCtrls)
            {
                if (!String.IsNullOrEmpty(originalXml))
                {
                    ReplaceXmlNode(xmlDoc, xmlRootName + "/dropdownlist/" + ddlCtrl.ID.ToLower(), Utils.FormatToSave(ddlCtrl.SelectedValue));
                }
                else
                {
                    var dataTyp = "";
                    if ((ddlCtrl.Attributes["datatype"] != null))
                    {
                        dataTyp = ddlCtrl.Attributes["datatype"];
                    }

                    var selText = "";
                    if (ddlCtrl.SelectedItem != null)
                    {
                        selText = ddlCtrl.SelectedItem.Text;
                        selText = XmlConvert.EncodeName(selText);
                    }

                    if (dataTyp.ToLower() == "double")
                    {
                        strXml += "<" + ddlCtrl.ID.ToLower() + " datatype=\"" + dataTyp.ToLower() + "\" selectedtext=\"" + selText.Replace('"', ' ') + "\"><![CDATA[";
                        strXml += Utils.FormatToSave(ddlCtrl.SelectedValue, TypeCode.Double);
                    }
                    else if (dataTyp.ToLower() == "date")
                    {
                        strXml += "<" + ddlCtrl.ID.ToLower() + " datatype=\"" + dataTyp.ToLower() + "\" selectedtext=\"" + selText.Replace('"', ' ') + "\"><![CDATA[";
                        strXml += Utils.FormatToSave(ddlCtrl.SelectedValue, TypeCode.DateTime);
                    }
                    else
                    {
                        strXml += "<" + ddlCtrl.ID.ToLower() + " selectedtext=\"" + selText.Replace('"', ' ') + "\"><![CDATA[";
                        strXml += ddlCtrl.SelectedValue;
                    }
                    strXml += "]]></" + ddlCtrl.ID.ToLower() + ">";
                }
            }
            strXml += "</dropdownlist>";

            strXml += "<checkboxlist>";

            foreach (CheckBoxList chkCtrl in chkCtrls)
            {
                var dataTyp = "";
                if ((chkCtrl.Attributes["datatype"] != null))
                {
                    dataTyp = chkCtrl.Attributes["datatype"];
                }
                if (dataTyp.ToLower() == "double")
                {
                    strXml += "<" + chkCtrl.ID.ToLower() + " datatype=\"" + dataTyp.ToLower() + "\">";
                }
                else if (dataTyp.ToLower() == "date")
                {
                    strXml += "<" + chkCtrl.ID.ToLower() + " datatype=\"" + dataTyp.ToLower() + "\">";
                }
                else
                {
                    strXml += "<" + chkCtrl.ID.ToLower() + ">";
                }
                foreach (ListItem lItem in chkCtrl.Items)
                {
                    // hard code "True" and "False", this is to make sure we have a constant across languages for testof.
                    // I know this should be done by using CultureInfo, but I'm unable to test that on different language servers and this is sure to work!
                    var strVal = "False";
                    if (lItem.Selected) strVal = "True";

                    if (!String.IsNullOrEmpty(originalXml))
                    {
                        ReplaceXmLatt(xmlDoc, xmlRootName + "/checkboxlist/" + chkCtrl.ID.ToLower() + "/chk[.='" + Utils.FormatToSave(lItem.Text) + "']", strVal);
                    }
                    else
                    {
                        strXml += "<chk value=\"" + strVal + "\" data=\"" + lItem.Value + "\" >";
                        if (dataTyp.ToLower() == "double")
                        {
                            strXml += "<![CDATA[" + Utils.FormatToSave(lItem.Text, TypeCode.Double) + "]]>";
                        }
                        else if (dataTyp.ToLower() == "date")
                        {
                            strXml += "<![CDATA[" + Utils.FormatToSave(lItem.Text, TypeCode.DateTime) + "]]>";
                        }
                        else
                        {
                            strXml += "<![CDATA[" + lItem.Text + "]]>";
                        }
                        strXml += "</chk>";
                    }
                }
                strXml += "</" + chkCtrl.ID.ToLower() + ">";
            }
            strXml += "</checkboxlist>";

            strXml += "<radiobuttonlist>";
            foreach (RadioButtonList rblCtrl in rblCtrls)
            {
                if (!String.IsNullOrEmpty(originalXml))
                {
                    ReplaceXmlNode(xmlDoc, xmlRootName + "/radiobuttonlist/" + rblCtrl.ID.ToLower(), Utils.FormatToSave(rblCtrl.SelectedValue));
                }
                else
                {
                    var dataTyp = "";
                    if ((rblCtrl.Attributes["datatype"] != null))
                    {
                        dataTyp = rblCtrl.Attributes["datatype"];
                    }

                    var selText = "";
                    if (rblCtrl.SelectedItem != null)
                    {
                        selText = rblCtrl.SelectedItem.Text;
                        selText = XmlConvert.EncodeName(selText);                        
                    }

                    if (dataTyp.ToLower() == "double")
                    {
                        strXml += "<" + rblCtrl.ID.ToLower() + " datatype=\"" + dataTyp.ToLower() + "\" selectedtext=\"" + selText.Replace('"', ' ') + "\"><![CDATA[";
                        strXml += Utils.FormatToSave(rblCtrl.SelectedValue, TypeCode.Double);
                    }
                    else if (dataTyp.ToLower() == "date")
                    {
                        strXml += "<" + rblCtrl.ID.ToLower() + " datatype=\"" + dataTyp.ToLower() + "\" selectedtext=\"" + selText.Replace('"', ' ') + "\"><![CDATA[";
                        strXml += Utils.FormatToSave(rblCtrl.SelectedValue, TypeCode.DateTime);
                    }
                    else
                    {
                        strXml += "<" + rblCtrl.ID.ToLower() + " selectedtext=\"" + selText.Replace('"', ' ') + "\"><![CDATA[";
                        strXml += rblCtrl.SelectedValue;
                    }
                    strXml += "]]></" + rblCtrl.ID.ToLower() + ">";
                }
            }
            strXml += "</radiobuttonlist>";



            //check for any template providers.
            if (providerList != null)
            {
                foreach (var prov in providerList)
                {
                    strXml += prov.Value.GetGenXml(genCtrls, xmlDoc, originalXml, folderMapPath, xmlRootName);
                }
            }


            strXml += "</" + xmlRootName + ">";

            if (!String.IsNullOrEmpty(originalXml))
            {
                strXml = xmlDoc.OuterXml;
            }

            return strXml;

        }


        public static Dictionary<String, String> GetGenXmlResx(Repeater rpGenXml, int rowIndex = 0)
        {
            //check row exists (0 based)
            if (rpGenXml.Items.Count <= rowIndex | rpGenXml.Items.Count == 0) return new Dictionary<String, String>();

            var rpItem = rpGenXml.Items[rowIndex];
            return GetGenXmlResx(rpItem);
        }

        public static Dictionary<String, String> GetGenXmlResx(RepeaterItem rpItem)
        {
            var rtnDic = new Dictionary<String, String>();

            var txtCtrls = new List<Control>();

            //build list of controls
            foreach (Control ctrl in rpItem.Controls)
            {
                if (ctrl is TextBox)
                {
                    var ctl = (TextBox)ctrl;
                    // if we have a resourcekeysave attr return the contrbol data
                    bool isResourceKeySave = ctl.Attributes["resourcekeysave"] != null;
                    if (ctl.Enabled & ctl.Visible & isResourceKeySave) rtnDic.Add(ctl.Attributes["resourcekeysave"], ctl.Text);
                }
            }

            return rtnDic;

        }

        public static String EncodeCDataTag(String xmlData)
        {
            return xmlData.Replace("<![CDATA[", "**CDATASTART**").Replace("]]>", "**CDATAEND**");
        }

        public static String DecodeCDataTag(String xmlData)
        {
            return xmlData.Replace("**CDATASTART**","<![CDATA[").Replace("**CDATAEND**","]]>");
        }

        /// <summary>
        /// Convert ajax xml passed form client into DB XML strucutre.
        /// </summary>
        /// <param name="xmlAjaxData"></param>
        /// <param name="originalXml"></param>
        /// <param name="xmlRootName"></param>
        /// <param name="ignoresecurityfilter"></param>
        /// <param name="filterlinks"></param>
        /// <returns></returns>
        public static string GetGenXmlByAjax(string xmlAjaxData, string originalXml, string xmlRootName = "genxml",bool ignoresecurityfilter = false, bool filterlinks = false)
        {

            //load original XML for update  
            var xmlDoc1 = new XmlDocument();
            if (!String.IsNullOrEmpty(xmlAjaxData))
            {
                xmlDoc1.LoadXml(xmlAjaxData);

                //load original XML for update  
                var xmlDoc = new XmlDocument();
                if (!String.IsNullOrEmpty(originalXml))
                {
                    xmlDoc.LoadXml(originalXml);
                }

                //Create XML
                var strXml = "";
                strXml += "<" + xmlRootName + ">";

                strXml += "<hidden>";

                var xmlNodeList = xmlDoc1.SelectNodes("root/f[@t='hid']");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode nod in xmlNodeList)
                    {
                        var ajaxId = "";
                        var updateStatus = "";
                        if (nod.Attributes != null)
                        {
                            ajaxId = GetAjaxShortId(nod.Attributes["id"].InnerText).ToLower();
                            if (nod.Attributes["upd"] != null) updateStatus = " update=\"" + nod.Attributes["upd"].InnerText + "\" ";
                        }

                        if (!String.IsNullOrEmpty(originalXml))
                        {
                            ReplaceXmlNode(xmlDoc, xmlRootName + "/hidden/" + ajaxId, Utils.FormatToSave(nod.InnerText));
                        }
                        else
                        {
                            if (ajaxId.StartsWith("dbl"))
                            {
                                strXml += "<" + ajaxId + updateStatus + " datatype=\"double\"><![CDATA[";
                                strXml += Utils.FormatToSave(nod.InnerText, TypeCode.Double);
                            }
                            else if (ajaxId.StartsWith("dte"))
                            {
                                strXml += "<" + ajaxId + updateStatus + " datatype=\"date\"><![CDATA[";
                                strXml += Utils.FormatToSave(nod.InnerText, TypeCode.DateTime);
                            }
                            else if (ajaxId.StartsWith("html"))
                            {
                                strXml += "<" + ajaxId + updateStatus + " datatype=\"html\"><![CDATA[";
                                if (ignoresecurityfilter)
                                {
                                    strXml += nod.InnerText;
                                }
                                else
                                {
                                    strXml += Security.FormatDisableScripting(nod.InnerText, filterlinks);
                                }
                            }
                            else
                            {
                                strXml += "<" + ajaxId + updateStatus + "><![CDATA[";
                                if (ignoresecurityfilter)
                                {
                                    strXml += nod.InnerText;
                                }
                                else
                                {
                                    strXml += Security.FormatDisableScripting(nod.InnerText,filterlinks);
                                }
                            }
                            strXml += "]]></" + ajaxId + ">";
                        }
                    }
                }
                strXml += "</hidden>";

                strXml += "<textbox>";

                xmlNodeList = xmlDoc1.SelectNodes("root/f[@t='txt']|root/f[@t='undefined']");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode nod in xmlNodeList)
                    {
                        var ajaxId = "";
                        var updateStatus = "";
                        if (nod.Attributes != null)
                        {
                            ajaxId = GetAjaxShortId(nod.Attributes["id"].InnerText).ToLower();
                            if (nod.Attributes["upd"] != null) updateStatus = " update=\"" + nod.Attributes["upd"].InnerText + "\"  ";
                        }

                        if (!ajaxId.StartsWith("ddl"))
                        {

                            if (!String.IsNullOrEmpty(originalXml))
                            {
                                ReplaceXmlNode(xmlDoc, xmlRootName + "/textbox/" + ajaxId, Utils.FormatToSave(nod.InnerText));
                            }
                            else
                            {
                                var dataTyp = "";
                                if (nod.Attributes != null && (nod.Attributes["dt"] != null))
                                {
                                    dataTyp = nod.Attributes["dt"].InnerText;
                                }
                                if (dataTyp.ToLower() == "double")
                                {
                                    strXml += "<" + ajaxId + updateStatus + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                                    strXml += Utils.FormatToSave(nod.InnerText, TypeCode.Double);
                                }
                                else if (dataTyp.ToLower() == "date")
                                {
                                    strXml += "<" + ajaxId + updateStatus + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                                    strXml += Utils.FormatToSave(nod.InnerText, TypeCode.DateTime);
                                }
                                else if (dataTyp.ToLower() == "email")
                                {
                                    //create spamsafe version
                                    strXml += "<" + ajaxId + "_spamsafe" + updateStatus + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                                    strXml += Utils.CloakText(nod.InnerText);
                                    strXml += "]]></" + ajaxId + "_spamsafe" + ">";
                                    //create spamsafe mailto version
                                    strXml += "<" + ajaxId + "_mailto" + updateStatus + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                                    strXml += Utils.CloakText(String.Format("<a href='mailto{1}{0}>{0}'></a>", nod.InnerText, ":"));
                                    strXml += "]]></" + ajaxId + "_mailto" + ">";
                                    //create normal version
                                    strXml += "<" + ajaxId + updateStatus + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                                    strXml += Security.FormatDisableScripting(nod.InnerText);
                                }
                                else
                                {
                                    if (dataTyp.ToLower() != "")
                                    {
                                        strXml += "<" + ajaxId + updateStatus + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                                    }
                                    else
                                    {
                                        strXml += "<" + ajaxId + updateStatus + "><![CDATA[";
                                    }

                                    if (ignoresecurityfilter)
                                    {
                                        strXml += nod.InnerText;
                                    }
                                    else
                                    {
                                        strXml += Security.FormatDisableScripting(nod.InnerText, filterlinks);
                                    }

                                }
                                strXml += "]]></" + ajaxId + ">";
                            }
                        }
                    }
                }

                strXml += "</textbox>";

                strXml += "<checkbox>";
                xmlNodeList = xmlDoc1.SelectNodes("root/f[@t='cb']");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode nod in xmlNodeList)
                    {
                        if (nod.Attributes != null)
                        {
                            if (!IsAjaxGroup(nod.Attributes["id"].InnerText))
                            {
                                var strValue = "False";
                                if (nod.InnerText == "checked" || nod.InnerText.ToLower() == "true")
                                {
                                    strValue = "True";
                                }

                                var ajaxId = "";
                                var updateStatus = "";
                                ajaxId = GetAjaxShortId(nod.Attributes["id"].InnerText).ToLower();
                                if (nod.Attributes["upd"] != null) updateStatus = " update=\"" + nod.Attributes["upd"].InnerText + "\"  ";

                                if (!String.IsNullOrEmpty(originalXml))
                                {
                                    ReplaceXmlNode(xmlDoc, xmlRootName + "/checkbox/" + ajaxId, strValue);
                                }
                                else
                                {
                                    strXml += "<" + ajaxId + updateStatus + ">";
                                    strXml += strValue;
                                    strXml += "</" + ajaxId + ">";
                                }
                            }
                        }
                    }
                }
                strXml += "</checkbox>";

                strXml += "<dropdownlist>";
                xmlNodeList = xmlDoc1.SelectNodes("root/f[@t='dd']");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode nod in xmlNodeList)
                    {
                        if (nod.Attributes != null)
                        {
                            var ajaxId = "";
                            var updateStatus = "";
                            if (nod.Attributes != null)
                            {
                                ajaxId = GetAjaxShortId(nod.Attributes["id"].InnerText).ToLower();
                                if (nod.Attributes["upd"] != null) updateStatus = " update=\"" + nod.Attributes["upd"].InnerText + "\"  ";
                            }

                            var dataValue = "";
                            if (nod.Attributes != null && (nod.Attributes["val"] != null)) dataValue = nod.Attributes["val"].InnerText;
                            var dataTyp = "";
                            if (nod.Attributes != null && (nod.Attributes["dt"] != null)) dataTyp = nod.Attributes["dt"].InnerText;

                            var selText = nod.InnerText;

                            if (!String.IsNullOrEmpty(originalXml))
                            {
                                ReplaceXmlNode(xmlDoc, xmlRootName + "/dropdownlist/" + ajaxId, Utils.FormatToSave(dataValue));
                            }
                            else
                            {
                                if (dataTyp.ToLower() == "double")
                                {
                                    strXml += "<" + ajaxId + updateStatus + " datatype=\"" + dataTyp.ToLower() + "\" selectedtext=\"" + selText.Replace('"',' ') + "\"><![CDATA[";
                                    strXml += Utils.FormatToSave(dataValue, TypeCode.Double);
                                }
                                else if (dataTyp.ToLower() == "date")
                                {
                                    strXml += "<" + ajaxId + updateStatus + " datatype=\"" + dataTyp.ToLower() + "\" selectedtext=\"" + selText.Replace('"', ' ') + "\"><![CDATA[";
                                    strXml += Utils.FormatToSave(dataValue, TypeCode.DateTime);
                                }
                                else
                                {
                                    strXml += "<" + ajaxId + updateStatus + " selectedtext=\"" + selText.Replace('"', ' ') + "\"><![CDATA[";
                                    strXml += dataValue;
                                }
                                strXml += "]]></" + ajaxId + ">";
                            }
                        }
                    }
                }
                strXml += "</dropdownlist>";

                strXml += "<checkboxlist>";

                var updateStatus2 = new Dictionary<string, string>();
                // build list of checkboxlists
                var l = new List<string>();
                xmlNodeList = xmlDoc1.SelectNodes("root/f[@t='cbl']");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode nod in xmlNodeList)
                    {
                        if (nod.Attributes != null)
                        {
                            var ajId = GetAjaxShortId(nod.Attributes["id"].InnerText);
                            // we have no upd attr on the ajax xml so we need to assume that cbl always get updated via ajax as save.
                            if (!updateStatus2.ContainsKey(ajId)) updateStatus2.Add(ajId, " update=\"save\"  ");

                            var xmlNodeList2 = xmlDoc1.SelectNodes("root/f[@t='cbl']");
                            if (xmlNodeList2 != null)
                            {
                                foreach (XmlNode nod2 in xmlNodeList2)
                                {
                                    if (nod2.Attributes != null)
                                    {
                                        if (!l.Contains(ajId))
                                        {
                                            l.Add(ajId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (string s in l)
                {
                    var updattr = "";
                    if (updateStatus2.ContainsKey(s)) updattr = updateStatus2[s]; 

                    strXml += "<" + s.ToLower() + updattr + ">";

                    xmlNodeList = xmlDoc1.SelectNodes("root/f[contains(@id,'" + s + "')]");
                    if (xmlNodeList != null)
                    {
                        foreach (XmlNode nod in xmlNodeList)
                        {
                            if (nod.Attributes != null)
                            {
                                var ajaxId = "";
                                if (nod.Attributes != null)
                                {
                                    ajaxId = GetAjaxShortId(nod.Attributes["id"].InnerText).ToLower();
                                }
                                var dataTyp = "";
                                if (nod.Attributes != null && (nod.Attributes["dt"] != null))
                                {
                                    dataTyp = nod.Attributes["dt"].InnerText;
                                }

                                var dataValue = "";
                                if (nod.Attributes != null && (nod.Attributes["val"] != null))
                                {
                                    dataValue = nod.Attributes["val"].InnerText;
                                }

                                var dataText = "";
                                if (nod.Attributes != null && (nod.Attributes["for"] != null))
                                {
                                    dataText = nod.Attributes["for"].InnerText;
                                }

                                var strValue = "False";
                                if (nod.InnerText == "checked" || nod.InnerText.ToLower() == "true")
                                {
                                    strValue = "True";
                                }

                                if (!String.IsNullOrEmpty(originalXml))
                                {
                                    ReplaceXmLatt(xmlDoc, xmlRootName + "/checkboxlist/" + ajaxId + "/chk[.='" + Utils.FormatToSave(dataValue) + "']", strValue);
                                }
                                else
                                {
                                    if (dataValue != strValue)
                                    {
                                        strXml += "<chk value=\"" + strValue + "\" data=\"" + dataValue + "\" >";
                                    }
                                    else
                                    {
                                        strXml += "<chk value=\"" + strValue + "\">";
                                    }
                                    if (dataTyp.ToLower() == "double")
                                    {
                                        strXml += "<![CDATA[" + Utils.FormatToSave(dataText, TypeCode.Double) + "]]>";
                                    }
                                    else if (dataTyp.ToLower() == "date")
                                    {
                                        strXml += "<![CDATA[" + Utils.FormatToSave(dataText, TypeCode.DateTime) + "]]>";
                                    }
                                    else
                                    {
                                        strXml += "<![CDATA[" + dataText + "]]>";
                                    }
                                    strXml += "</chk>";
                                }
                            }
                        }
                    }
                    strXml += "</" + s.ToLower() + ">";
                }
                strXml += "</checkboxlist>";

                strXml += "<radiobuttonlist>";

                xmlNodeList = xmlDoc1.SelectNodes("root/f[@t='rb']");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode nod in xmlNodeList)
                    {
                        if (nod.Attributes != null)
                        {
                            var ajaxId = GetAjaxShortId(nod.Attributes["id"].InnerText).ToLower();
                            var updateStatus = "";
                            if (nod.Attributes["upd"] != null) updateStatus = " update=\"" + nod.Attributes["upd"].InnerText + "\"  ";

                            if (nod.InnerText.ToLower() == "true")
                            {

                                var dataValue = "";
                                if (nod.Attributes != null && (nod.Attributes["val"] != null))
                                {
                                    dataValue = nod.Attributes["val"].InnerText;
                                }

                                if (!String.IsNullOrEmpty(originalXml))
                                {
                                    ReplaceXmlNode(xmlDoc, xmlRootName + "/radiobuttonlist/" + ajaxId, Utils.FormatToSave(dataValue));
                                }
                                else
                                {
                                    var dataTyp = "";
                                    if (nod.Attributes != null && (nod.Attributes["dt"] != null))
                                    {
                                        dataTyp = nod.Attributes["dt"].InnerText;
                                    }

                                    if (dataTyp.ToLower() == "double")
                                    {
                                        strXml += "<" + ajaxId + updateStatus + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                                        strXml += Utils.FormatToSave(dataValue, TypeCode.Double);
                                    }
                                    else if (dataTyp.ToLower() == "date")
                                    {
                                        strXml += "<" + ajaxId + updateStatus + " datatype=\"" + dataTyp.ToLower() + "\"><![CDATA[";
                                        strXml += Utils.FormatToSave(dataValue, TypeCode.DateTime);
                                    }
                                    else
                                    {
                                        strXml += "<" + ajaxId + updateStatus + "><![CDATA[";
                                        strXml += dataValue;
                                    }
                                    strXml += "]]></" + ajaxId + ">";
                                }
                            }
                        }
                    }
                }
                strXml += "</radiobuttonlist>";



                strXml += "</" + xmlRootName + ">";

                if (!String.IsNullOrEmpty(originalXml))
                {
                    strXml = xmlDoc.OuterXml;
                }

                return strXml;
            }
            return "";
        }

        public static bool IsAjaxGroup(string fullId)
        {
            var s = fullId.Split('_');
            if (Utils.IsNumeric(s[s.GetUpperBound(0)]))
            {
                return true;
            }
            return false;
        }

        public static string GetAjaxShortId(string fullId)
        {
            var s = fullId.Split('_');
            if (Utils.IsNumeric(s[s.GetUpperBound(0)]))
            {
                if (s.GetLength(0) > 1)
                {
                    return s[(s.GetUpperBound(0)) - 1];                    
                }
                return "";
            }
            return s[s.GetUpperBound(0)];
        }

        public static string GetGenSearchFieldText(Repeater rpData, int rowIndex = 0)
	{

        var ddlCtrls = new List<Control>();
		var rblCtrls = new List<Control>();
		var chkCtrls = new List<Control>();
		var txtCtrls = new List<Control>();
		var chkboxCtrls = new List<Control>();

		//check row exists (0 based)
		if (rpData.Items.Count <= rowIndex) {
			return "";
		}

		//only do if entry already created

		if (rpData.Items.Count >= 1) {
			var dlItem = rpData.Items[rowIndex];

			//build list of controls
			foreach (Control ctrl in dlItem.Controls) {
				if (ctrl is DropDownList) {
					ddlCtrls.Add(ctrl);
				}
				if (ctrl is CheckBoxList) {
					chkCtrls.Add(ctrl);
				}
				if (ctrl is CheckBox) {
					chkboxCtrls.Add(ctrl);
				}
				if (ctrl is TextBox) {
						txtCtrls.Add(ctrl);
				}
				if (ctrl is RadioButtonList) {
					rblCtrls.Add(ctrl);
				}
			}

			//Create XML
			string rtnSearchText = "";

			foreach (TextBox  txtCtrl in txtCtrls) {
				if ((txtCtrl.Attributes["searchindex"] != null)) {
					rtnSearchText += txtCtrl.Text + " ";
				}
			}

			foreach (CheckBox chkboxCtrl in chkboxCtrls) {
				if ((chkboxCtrl.Attributes["searchindex"] != null)) {
					if (chkboxCtrl.Checked) {
						rtnSearchText += chkboxCtrl.Text + " ";
					}
				}
			}

			foreach (DropDownList ddlCtrl in ddlCtrls) {
				if ((ddlCtrl.Attributes["searchindex"] != null)) {
					rtnSearchText += ddlCtrl.SelectedValue + " ";
				}
			}

			foreach (CheckBoxList chkCtrl in chkCtrls) {
				if ((chkCtrl.Attributes["searchindex"] != null)) {
					foreach (ListItem lItem in chkCtrl.Items) {
						if (lItem.Selected) {
							rtnSearchText += lItem.Text + " ";
						}
					}
				}
			}

            foreach (RadioButtonList rblCtrl in rblCtrls)
            {
				if ((rblCtrl.Attributes["searchindex"] != null)) {
					rtnSearchText += rblCtrl.SelectedValue + " ";
				}
			}

			return rtnSearchText;
		}
            return "";
	}


        public static void ReplaceXmLatt(XmlDocument xmlDoc, string xPath, string newValue)
        {
            var nod = xmlDoc.SelectSingleNode(xPath);
            if ((nod != null))
            {
                if (nod.Attributes != null)
                {
                    nod.Attributes["value"].InnerText = newValue;
                }
            }
            else
            {
                var xpatharray = xPath.Split('@');
                if (xpatharray.Count() == 2)
                {
                    var attrName = xpatharray[1];
                    var oAtt = xmlDoc.CreateAttribute(attrName);
                    oAtt.Value = newValue;

                    nod = xmlDoc.SelectSingleNode(xpatharray[0].TrimEnd('/'));
                    if (nod != null) nod.Attributes.Append(oAtt);                        
                }
            }
        }


        public static void ReplaceXmlNode(XmlDocument xmlDoc, string xPath, string newValue)
        {
            ReplaceXmlNode(xmlDoc, xPath, newValue, true);
        }

        public static void ReplaceXmlNode(XmlDocument xmlDoc, string xPath, string newValue, bool cdata)
        {
            var nod = xmlDoc.SelectSingleNode(xPath);
            if ((nod != null))
            {
                if (newValue == "") cdata = false; //stops invalid "<" char error
                if (cdata)
                {
                    nod.InnerXml = "<![CDATA[" + newValue + "]]>";
                }
                else
                {
                    nod.InnerXml = newValue;
                }
            }
            else
            {
                string[] partsOfXPath = xPath.Trim('/').Split('/');
 
                //Create a new node.
                var elem = xmlDoc.CreateElement(partsOfXPath[partsOfXPath.Length -1]);
                if (cdata)
                {
                    elem.InnerXml = "<![CDATA[" + newValue + "]]>";
                }
                else
                {
                    elem.InnerXml = newValue;
                }
                
                //Add the node to the document.
                var selectSingleNode = xmlDoc.SelectSingleNode(Utils.ReplaceLastOccurrence(xPath, partsOfXPath[partsOfXPath.Length - 1], "").TrimEnd('/'));
                if (selectSingleNode != null)                    
                {
                    selectSingleNode.AppendChild(elem);
                }                        
            }
        }


        public static void MergeXmlNodeText(XmlDocument xmlDoc, XmlDocument xmlSourceDoc, string xPathStr)
        {
            MergeXmlNodeText(xmlDoc, xmlSourceDoc, xPathStr, "");
        }

        public static void MergeXmlNodeText(XmlDocument xmlDoc, XmlDocument xmlSourceDoc, string xPathStr, string xPathSource)
        {
            if (String.IsNullOrEmpty(xPathSource))
                xPathSource = xPathStr;
            if ((xmlSourceDoc.SelectSingleNode(xPathSource) != null))
            {
                var selectSingleNode = xmlSourceDoc.SelectSingleNode(xPathSource);
                if (selectSingleNode != null)
                {
                    if ((xmlDoc.SelectSingleNode(xPathStr) != null))
                    {
                        ReplaceXmlNode(xmlDoc, xPathStr, selectSingleNode.InnerText);
                    }
                }
            }
        }

        public static string SetGenXmlValue(string dataXml, string xpath, string Value, bool cdata = true, bool ignoresecurityfilter = false, bool filterlinks = false)
        {
            if (ignoresecurityfilter)
            {
                // clear cross scripting if not html field.
                Value = Security.FormatDisableScripting(Value,filterlinks);
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(dataXml);
            if (xpath.Contains("@"))
                ReplaceXmLatt(xmlDoc, xpath, Value);
            else
                ReplaceXmlNode(xmlDoc, xpath, Value,cdata);
            return xmlDoc.OuterXml;
        }

        public static string GetGenXmlValue(string ctrlId, string ctrlType, string dataXml)
        {
            var rtnValue = GetGenXmlValue(dataXml, "genxml/" + ctrlType + "/" + ctrlId.ToLower());
            if (rtnValue == "")
            {
                rtnValue = GetGenXmlValueFromLang(ctrlId, ctrlType, dataXml);
            }
            return rtnValue;
        }

        public static string GetGenXmlValueFromLang(string ctrlId, string ctrlType, string dataXml)
        {
            return GetGenXmlValue(dataXml, "genxml/lang/genxml/" + ctrlType + "/" + ctrlId.ToLower());
        }


        public static string GetGenXmlValue(string ctrlId, string ctrlType, string dataXml, string xmlRootName)
        {
            return GetGenXmlValue(dataXml, xmlRootName + "/" + ctrlType + "/" + ctrlId.ToLower());
        }

        public static string GetGenXmlValue(string dataXml, string xPath)
        {
            var xmlNod = GetGenXmLnode(dataXml, xPath);
            if (xmlNod == null)
            {
                return "";
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null))
            {
                switch (xmlNod.Attributes["datatype"].InnerText.ToLower())
                {
                    case "double":
                        return Utils.FormatToDisplay(xmlNod.InnerText,Utils.GetCurrentCulture(),TypeCode.Double,"N");
                    case "date":
                        return Utils.FormatToDisplay(xmlNod.InnerText, Utils.GetCurrentCulture(), TypeCode.DateTime,"d");
                    case "html":
                        return xmlNod.InnerXml;
                    default:
                        var strOut = xmlNod.InnerText;
                        if (strOut.Contains("<![CDATA["))
                        {
                            //convert back cdata marks con verted so it saves OK into XML 
                            strOut = strOut.Replace("**CDATASTART**","<![CDATA[");
                            strOut = strOut.Replace("**CDATAEND**","]]>");
                        }
                        return strOut;
                }
            }
            return xmlNod.InnerText;
        }

        /// <summary>
        /// get the data fromthe XML wothout reformatting for numbers or dates.
        /// </summary>
        /// <param name="dataXml"></param>
        /// <param name="xPath"></param>
        /// <returns></returns>
        public static string GetGenXmlValueRawFormat(string dataXml, string xPath)
        {
            var xmlNod = GetGenXmLnode(dataXml, xPath);
            if (xmlNod == null)
            {
                return "";
            }
            return xmlNod.InnerText;
        }

        public static XmlNode GetGenXmLnode(string ctrlId, string ctrlType, string dataXml)
        {
            return GetGenXmLnode(ctrlId, ctrlType, dataXml, "genxml");
        }

        public static XmlNode GetGenXmLnode(string ctrlId, string ctrlType, string dataXml, string xmlRootName)
        {
            if (ctrlId == null | ctrlType == null | dataXml == null)
            {
                return null;
            }
            return GetGenXmlNodeData(dataXml, xmlRootName + "/" + ctrlType + "/" + ctrlId.ToLower(), xmlRootName);
        }

        public static XmlNode GetGenXmLnode(string dataXml, string xPath)
        {
            return GetGenXmlNodeData(dataXml, xPath);
        }

        private static XmlNode GetGenXmlNodeData(string dataXml, string xPath, string xmlRootName = "genxml")
        {
            try
            {
                if (String.IsNullOrEmpty(dataXml)) return null;
                if (String.IsNullOrEmpty(xPath)) return null;

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(dataXml);
                var xmlNod = xmlDoc.SelectSingleNode(xPath);
                // check we don;t have a language node
                if (xmlNod == null)
                {
                    xmlNod = xmlDoc.SelectSingleNode(xmlRootName + "/lang/" + xPath);
                }
                return xmlNod;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static object AssignByReflection(object obj, string tagXmlString)
        {
            return AssignByReflection(obj, GetTokenPropXmLnode(tagXmlString));
        }

        public static XmlNode GetTokenPropXmLnode(string tagXmlString)
        {
            var xmlDoc = new XmlDocument();
            var strXml = HttpUtility.HtmlDecode(tagXmlString);
            strXml = "<root>" + strXml + "</root>";

            xmlDoc.LoadXml(strXml);
            var xmlNod = xmlDoc.SelectSingleNode("root/prop");
            return xmlNod;
        }

        public static object AssignByReflection(object obj, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null)
            {
                foreach (XmlAttribute xmlAtt in xmlNod.Attributes)
                {
                    obj = AssignByReflection(obj, xmlAtt.Name, xmlAtt.InnerText);
                }                
            }
            return obj;
        }

        public static object AssignByReflection(object obj, string propName, string propValue)
        {
            try
            {
                var typ = obj.GetType();
                var prop = typ.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    switch (prop.PropertyType.Name)
                    {
                        case "Int32":
                        case "Integer":
                            prop.SetValue(obj, Convert.ToInt32(propValue), null);
                            break;
                        case "Int16":
                            prop.SetValue(obj, Convert.ToInt16(propValue), null);
                            break;
                        case "Int64":
                            prop.SetValue(obj, Convert.ToInt64(propValue), null);
                            break;
                        case "Boolean":
                            prop.SetValue(obj, Convert.ToBoolean(propValue), null);
                            break;
                        case "String":
                            prop.SetValue(obj, propValue, null);
                            break;
                        case "Decimal":
                            prop.SetValue(obj, Convert.ToDecimal(propValue), null);
                            break;
                        case "Unit":
                            prop.SetValue(obj, Unit.Parse(propValue), null);
                            break;
                        default:
                            if (Enum.IsDefined(prop.PropertyType, propValue))
                            {
                                prop.SetValue(obj, Enum.Parse(prop.PropertyType, propValue, true), null);
                            }
                            else
                            {
                                if (Utils.IsNumeric(propValue))
                                {
                                    prop.SetValue(obj, Convert.ToInt32(propValue), null);
                                }
                                else
                                {
                                    prop.SetValue(obj, propValue, null);
                                }
                            }
                            break;
                    }
                }
            }
// ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
            {
                //don't do anything, failure expected in some cases
            }
            return obj;
        }

        public static string GetCreateViewSql(string viewName, string templateText, string tableName, string selectClause = "", string dbColumns = "", string xmlDataColumnName = "XMLData", string xmlRootName = "genxml")
        {
            var rtnSql = "CREATE VIEW " + viewName + " AS SELECT " + dbColumns;
            var xmlDoc = new XmlDocument();

            var aryTempl = ParseTemplateText(templateText);

            for (var lp = 0; lp <= aryTempl.GetUpperBound(0); lp++)
            {
                var htmlDecode = HttpUtility.HtmlDecode(aryTempl[lp]);
                if (htmlDecode != null && htmlDecode.ToLower().StartsWith("<tag"))
                {
                    var strXml = HttpUtility.HtmlDecode(aryTempl[lp]);
                    strXml = "<root>" + strXml + "</root>";
                    xmlDoc.LoadXml(strXml);
                    var xmlNod = xmlDoc.SelectSingleNode("root/tag");

                    if (xmlNod != null)
                    {
                        if (xmlNod.Attributes != null && xmlNod.Attributes["type"] != null)
                        {
                            rtnSql += GetXmlSqlColumn(xmlNod, xmlDataColumnName, xmlRootName);
                        }
                    }
                }
            }
            rtnSql = rtnSql.TrimEnd(' ');
            rtnSql = rtnSql.TrimEnd(',');
            rtnSql += " FROM " + tableName;
            if (!String.IsNullOrEmpty(selectClause))
            {
                rtnSql += " WHERE " + selectClause;
            }
            return rtnSql;
        }

        private static string GetXmlSqlColumn(XmlNode xmlNod, string xmlDataColumnName = "XMLData", string xmlRootName = "genxml")
	{
		var rtnSql = "";
		if (xmlNod.Attributes != null && xmlNod.Attributes["id"] != null) {
			var strId = xmlNod.Attributes["id"].InnerXml;
			string strDataType;
			string strColumnName;

			if ((xmlNod.Attributes["sqldatatype"] != null)) {
				strDataType = xmlNod.Attributes["sqldatatype"].InnerXml;
				if (strDataType.ToLower() == "none") {
					return "";
				}
			} else {
				if ((xmlNod.Attributes["datatype"] != null)) {
					switch (xmlNod.Attributes["datatype"].InnerXml.ToLower()) {
						case "date":
							strDataType = "datetime";
							break;
						default:
							strDataType = "nvarchar(256)";
							break;
					}
				} else {
					strDataType = "nvarchar(256)";
				}
			}

			if ((xmlNod.Attributes["sqlcolumnname"] != null)) {
				strColumnName = xmlNod.Attributes["sqlcolumnname"].InnerXml;
			} else {
				strColumnName = strId;
			}

			switch (xmlNod.Attributes["type"].InnerXml.ToLower()) {
				case "fileupload":
					rtnSql += "[" + xmlDataColumnName + "].value('(" + xmlRootName + "/textbox/txt" + strId.ToLower() + ")[1]', '" + strDataType + "') AS [txt" + strColumnName + "], ";
					rtnSql += "[" + xmlDataColumnName + "].value('(" + xmlRootName + "/hidden/hid" + strId.ToLower() + ")[1]', '" + strDataType + "') AS [hid" + strColumnName + "], ";
					break;
				case "hidden":
					rtnSql += "[" + xmlDataColumnName + "].value('(" + xmlRootName + "/hidden/" + strId.ToLower() + ")[1]', '" + strDataType + "') AS [" + strColumnName + "], ";
					break;
				case "const":
					rtnSql += "[" + xmlDataColumnName + "].value('(" + xmlRootName + "/hidden/" + strId.ToLower() + ")[1]', '" + strDataType + "') AS [" + strColumnName + "], ";
					break;
				case "textbox":
					rtnSql += "[" + xmlDataColumnName + "].value('(" + xmlRootName + "/textbox/" + strId.ToLower() + ")[1]', '" + strDataType + "') AS [" + strColumnName + "], ";
					break;
				case "dateeditcontrol":
					rtnSql += "[" + xmlDataColumnName + "].value('(" + xmlRootName + "/textbox/" + strId.ToLower() + ")[1]', '" + strDataType + "') AS [" + strColumnName + "], ";
					break;
				case "dropdownlist":
					rtnSql += "[" + xmlDataColumnName + "].value('(" + xmlRootName + "/dropdownlist/" + strId.ToLower() + ")[1]', '" + strDataType + "') AS [" + strColumnName + "], ";
					break;
				case "checkboxlist":
					var xmlNodLIst = xmlNod.SelectNodes(xmlRootName + "/checkboxlist/" + strId);
                    if (xmlNodLIst != null)
                    {
                        var lp = 1;
                        foreach (XmlNode xmlNod2 in xmlNodLIst)
                        {
                            rtnSql += "[" + xmlDataColumnName + "].value('(" + xmlRootName + "/checkboxlist/" + strId.ToLower() + "/" +
                                      xmlNod2.Name + "/@value)[" + lp + "]', 'bit') AS [" + strColumnName + lp.ToString(CultureInfo.InvariantCulture) +
                                      xmlNod2.InnerXml + "], ";
                            lp = lp + 1;
                        }
                    }
			        break;
				case "checkbox":
					rtnSql += "[" + xmlDataColumnName + "].value('(" + xmlRootName + "/checkbox/" + strId.ToLower() + ")[1]', 'bit') AS [" + strColumnName + "], ";
					break;
				case "radiobuttonlist":
					rtnSql += "[" + xmlDataColumnName + "].value('(" + xmlRootName + "/radiobuttonlist/" + strId.ToLower() + ")[1]', '" + strDataType + "') AS [" + strColumnName + "], ";
					break;
				case "dnntexteditor":
					rtnSql += "[" + xmlDataColumnName + "].value('(" + xmlRootName + "/edt/" + strId.ToLower() + ")[1]', '" + strDataType + "') AS [" + strColumnName + "], ";
					break;
			}

		}

		return rtnSql;
	}

        public static object PopulateGenObject(Repeater rpData, object obj, int rowIndex = 0)
	{

        var ddlCtrls = new List<Control>();
		var rblCtrls = new List<Control>();
		var txtCtrls = new List<Control>();
		var chkboxCtrls = new List<Control>();
		var genCtrls = new List<Control>();

		//check row exists (0 based)
		if (rpData.Items.Count <= rowIndex) {
			return obj;
		}

		//only do if entry already created

		if (rpData.Items.Count >= 1) {

		    var dlItem = rpData.Items[rowIndex];
			//build list of controls
			foreach (Control ctrl in dlItem.Controls) {
				//hidden fields do not have an attribute to do databind
				if (ctrl is DropDownList) {
					ddlCtrls.Add(ctrl);
				}
				else if (ctrl is CheckBox) {
					chkboxCtrls.Add(ctrl);
				}
                else if (ctrl is TextBox)
                {
						txtCtrls.Add(ctrl);
				}
                else if (ctrl is RadioButtonList)
                {
					rblCtrls.Add(ctrl);
				}
                else 
                {
                    // get any other controls to pass to provider.
					genCtrls.Add(ctrl);
				}
			}

			foreach (TextBox txtCtrl in txtCtrls) {
				if ((txtCtrl.Attributes["databind"] != null)) {
					obj = AssignByReflection(obj, txtCtrl.Attributes["databind"], txtCtrl.Text);
				}
			}

			foreach (CheckBox chkboxCtrl in chkboxCtrls) {
				if ((chkboxCtrl.Attributes["databind"] != null)) {
					obj = AssignByReflection(obj, chkboxCtrl.Attributes["databind"], chkboxCtrl.Checked.ToString(CultureInfo.InvariantCulture));
				}
			}

			foreach (DropDownList ddlCtrl in ddlCtrls) {
				if ((ddlCtrl.Attributes["databind"] != null)) {
					obj = AssignByReflection(obj, ddlCtrl.Attributes["databind"], ddlCtrl.SelectedValue);
				}
			}

			foreach (RadioButtonList rblCtrl in rblCtrls) {
				if ((rblCtrl.Attributes["databind"] != null)) {
					obj = AssignByReflection(obj, rblCtrl.Attributes["databind"], rblCtrl.SelectedValue);
				}
			}

            //check for any template providers.
            var providerList = GenXProviderManager.ProviderList;
            if (providerList != null)
            {
                foreach (var prov in providerList)
                {
                    obj = prov.Value.PopulateGenObject(genCtrls, obj);
                }
            }

		}

		return obj;

	}

        public static string[] ParseTemplateText(string templText)
        {
            char[] paramAry = {Convert.ToChar("["),Convert.ToChar("]")};

            var strOut = templText.Split(paramAry);

            return strOut;
        }

        /// <summary>
        /// Render reporter with templates already assigned.
        /// </summary>
        /// <param name="rpData">Repeater to be rendered</param>
        /// <param name="objInfo">Data object, if null then it is assumed the repeater already have the data assigned.</param>
        /// <returns></returns>
        public static string RenderRepeater(Repeater rpData, object objInfo = null)
        {
            if (objInfo != null)
            {
                var arylist = new ArrayList();
                arylist.Add(objInfo);
                rpData.DataSource = arylist;                
            }

            rpData.DataBind();

            //Get the rendered HTML
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            var htmlTw = new HtmlTextWriter(sw);
            rpData.RenderControl(htmlTw);

            return sb.ToString();
        }

        public static string RenderRepeater(IList objList, GenXmlTemplate genXmlTemplate)
        {
            var dlGen = new Repeater { ItemTemplate = genXmlTemplate};

            dlGen.DataSource = objList;
            dlGen.DataBind();

            //Get the rendered HTML
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            var htmlTw = new HtmlTextWriter(sw);
            dlGen.RenderControl(htmlTw);

            return sb.ToString();
        }


        public static string RenderRepeater(IList objList, string templateText, string xmlRootName = "", string dataBindXmlColumn = "XMLData", string cultureCode = "", Dictionary<string, string> settings = null, ConcurrentStack<Boolean> visibleStatus = null)
        {
            var dlGen = new Repeater { ItemTemplate = new GenXmlTemplate(templateText, xmlRootName, dataBindXmlColumn, cultureCode, settings, visibleStatus) };

            dlGen.DataSource = objList;
            dlGen.DataBind();

            //Get the rendered HTML
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            var htmlTw = new HtmlTextWriter(sw);
            dlGen.RenderControl(htmlTw);

            return sb.ToString();
        }

        public static string RenderRepeater(object objInfo, string templateText, string xmlRootName = "", string dataBindXmlColumn = "XMLData", string cultureCode = "", Dictionary<string, string> settings = null, ConcurrentStack<Boolean> visibleStatus = null)
        {
            var arylist = new ArrayList();
            var dlGen = new Repeater { ItemTemplate = new GenXmlTemplate(templateText, xmlRootName, dataBindXmlColumn, cultureCode, settings, visibleStatus) };

            arylist.Add(objInfo);

            dlGen.DataSource = arylist;
            dlGen.DataBind();

            //Get the rendered HTML
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            var htmlTw = new HtmlTextWriter(sw);
            dlGen.RenderControl(htmlTw);

            return sb.ToString();
        }


        public static string GetGenControlPropety(string xmLproperties, string propertyname)
        {
            var xmlDoc = new XmlDocument();

            xmlDoc.LoadXml("<root>" + xmLproperties + "</root>");
            var xmlNod2 = xmlDoc.SelectSingleNode("root/tag");

            if (xmlNod2 != null && xmlNod2.Attributes != null)
            {
                if ((xmlNod2.Attributes[propertyname] != null))
                {
                    return xmlNod2.Attributes[propertyname].InnerText;
                }
            }
            return "";
        }

        public static string GetSqlSearchFilters(Repeater rp1)
        {
            return GetSqlSearchFilters(rp1, null);
        }

        public static string GetSqlSearchFilters(Repeater rp1, HttpContext context)
        {
            var strOut = "";
            var objTempl = (GenXmlTemplate)rp1.ItemTemplate;
            if (objTempl != null)
            {
                strOut = GetSqlSearchFilters(rp1,objTempl,context);
            }
            return strOut;
        }
        public static string GetSqlSearchFilters(Repeater rp1, string TemplateText, HttpContext context)
        {
            var objTempl = new GenXmlTemplate(TemplateText);
            {
                return GetSqlSearchFilters(rp1, objTempl, context);
            }
        }

        public static string GetSqlSearchFilters(Repeater rp1, GenXmlTemplate objTempl, HttpContext context)
        {
                var strOut = "";
                var processFilterSql = "";
                // add meta tags to list (meta tag are now the default, this breaks compatablity with hidden fields.)
                if (objTempl != null)
                {
                    foreach (var mt in objTempl.MetaTags)
                    {
                        var orderId = GenXmlFunctions.GetGenXmlValue(mt, "tag/@id");
                        if (orderId.ToLower().StartsWith("filter"))
                        {
                            var strAttr = GenXmlFunctions.GetGenXmlValue(mt, "tag/@value").Split(';');
                            if (strAttr.Count() == 5)
                            {
                                var strAttrOr1 = strAttr[0].Split(Convert.ToChar("|"));
                                var strAttrOr2 = strAttr[1].Split(Convert.ToChar("|"));
                                var strAttrOr3 = strAttr[2].Split(Convert.ToChar("|"));

                                var dataField = strAttr[3];
                                var testType = strAttr[4];

                                var strOut2 = " and (";

                                // Loop over strings
                                for (int i = 0; i < strAttrOr1.Length; i++)
                                {
                                    var searchText = "";
                                    if (strAttrOr3[i].ToLower().StartsWith("param:"))
                                    {
                                        // if search as param in
                                        var param = strAttrOr3[i].ToLower().Replace("param:", "");
                                        if (context != null && Utils.RequestParam(context, param) != "")
                                        {
                                            searchText = Utils.RequestParam(context, param);
                                        }
                                    }
                                    else
                                    {
                                        if (rp1.Items.Count >= 1)  // Can only get postback vars after page load has processed. (use param: to pass back search paramfor xsl which is initialized in page init)
                                            searchText = GetField(rp1, strAttrOr3[i].ToLower());
                                        //[TODO: would be nice to use the postback context to get the field value, but need to work out how to get the form key correct.]
                                        //if (context.Request.Form[strAttrOr3[i].ToLower()] != null)
                                        //    searchText = context.Request.Form[strAttrOr3[i].ToLower()];
                                    }

                                    if (searchText != "")
                                    {
                                        if (i > 0)
                                        {
                                            if (rp1.Items.Count >= 1)  // Can only get postback vars after page load has processed. (use param: to pass back search paramfor xsl which is initialized in page init)
                                            {
                                                if (GetField(rp1, strAttrOr3[i - 1].ToLower()) != "")
                                                {
                                                    strOut2 += " or ";
                                                }                                                
                                            }
                                        }
                                        if (testType == "=")
                                        {
                                            strOut2 += GetSqlFilterText(strAttrOr1[i], strAttrOr2[i], searchText, dataField);
                                        }
                                        else
                                        {
                                            strOut2 += GetSqlFilterLikeText(strAttrOr1[i], strAttrOr2[i], searchText, dataField);
                                        }

                                    }
                                }

                                strOut2 += ")";

                                // Only add entry is not blank
                                if (strOut2 != " and ()")
                                {
                                    strOut += strOut2;
                                }
                            }
                        }

                        // set flag to process filtersql data if there.
                        if (orderId.ToLower() == "sqlfilter") processFilterSql = GenXmlFunctions.GetGenXmlValue(mt, "tag/@value");

                    }

                    // see if we've specific some SQL to filter the data.
                    if (processFilterSql != "")
                    {
                        if (processFilterSql.TrimStart(' ').ToLower().StartsWith("and"))
                        {
                            strOut += " " + System.Web.HttpUtility.HtmlDecode(processFilterSql);
                        }
                        else
                        {
                            strOut += " and " + System.Web.HttpUtility.HtmlDecode(processFilterSql);
                        }
                    }
                }

                //remove possible SQL injection commands
                strOut = StripSqlCommands(strOut);

                return strOut;
        }

        public static string GetSqlOrderBy(Repeater rp1)
        {
            var orderValue = "";

            //only do if entry already created
            if (rp1.Items.Count >= 1)
            {
                // dynamic orderby must use a dropdown list to select order, so search for it.
                var sortId = GenXmlFunctions.GetField(rp1, "ddlOrderBy");
                if (sortId == "")
                {
                    sortId = "0";
                }

                orderValue = GenXmlFunctions.GetHiddenField(rp1, "orderby" + sortId);

            }

            if (orderValue == "")
            {
                // static orderby can be passed as a template meta tag, so search for orderby tag.
                var objTempl = (GenXmlTemplate) rp1.ItemTemplate;
                if (objTempl != null)
                {
                    foreach (var mt in objTempl.MetaTags)
                    {
                        var orderId = GenXmlFunctions.GetGenXmlValue(mt, "tag/@id");
                        if (orderId.ToLower().StartsWith("orderby"))
                        {
                            orderValue = GenXmlFunctions.GetGenXmlValue(mt, "tag/@value");
                            break; // only want first value
                        }
                    }
                }
            }
            if (orderValue == "{bycategoryproduct}") return orderValue; // special custom sort on each category
            return GetSqlOrderByWithValue(orderValue);
        }

        public static string GetSqlOrderBy(string TemplateText, String orderByIndexValue = "")
        {
            // static orderby can be passed as a template meta tag, so search for orderby tag.
            var objTempl = new GenXmlTemplate(TemplateText);
            return GetSqlOrderBy(objTempl, orderByIndexValue);
        }

        public static string GetSqlOrderBy(GenXmlTemplate Template, String orderByIndexValue = "")
        {
            var orderValue = "";
            // static orderby can be passed as a template meta tag, so search for orderby tag.
            foreach (var mt in Template.MetaTags)
            {
                var orderId = GenXmlFunctions.GetGenXmlValue(mt, "tag/@id");
                if (orderId.ToLower().StartsWith("orderby" + orderByIndexValue))
                {
                    orderValue = GenXmlFunctions.GetGenXmlValue(mt, "tag/@value");
                    return GetSqlOrderByWithValue(orderValue);
                }
            }
            return "";
        }

        private static string GetSqlOrderByWithValue(string orderValue)
        {
            var hidCtrls = new List<Control>();
            var strOut = " Order by ";

            if (orderValue != "")
            {
                var strAttr = orderValue.Split(Convert.ToChar(";"));
                if (strAttr.Count() == 4)
                {
                    var xpath = strAttr[0].Split(',');
                    var sqlType = strAttr[1].Split(',');
                    var dataField = strAttr[2].Split(',');
                    var orderSeq = strAttr[3].Split(',');

                    for (int i = 0; i < xpath.Length; i++)
                    {
                        var orgSqlType = "";
                        if (sqlType[i].StartsWith("int") | sqlType[i].StartsWith("bigint") | sqlType[i].StartsWith("smallint") | sqlType[i].StartsWith("tinyint") | sqlType[i].StartsWith("decimal"))
                        {
                            //OMG - we've got a numeric...fine, but what if the data is not in the correct format, we need to convert the XML as nvarchar and then wrap the dam thing in a isnumeric test!!!
                            // NOTE: Isnumeric return ture for chars "$,.+-" (which is not perhaps a totally bad thing for us!)
                            orgSqlType = sqlType[i].Replace('.', ','); // use the . for the decimal(10,2) formatting in template, so we don;t clash on the Split. decimal(10.2) ---> decimal(10,2)
                            sqlType[i] = "nvarchar(50)";
                        }
                        var strThis = "";
                        strThis += "[" + dataField[i] + "].value('(" + xpath[i];
                        strThis += ")[1]', '" + sqlType[i];
                        strThis += "') ";

                        if (orgSqlType != "")
                        {
                            //OK damit..we've got a numeric, lets write some ugly SQL!
                            strThis = " convert(" + orgSqlType + ",(select case when isnumeric(isnull(" + strThis + ",'0')) = 1 then isnull(" + strThis + ",'0') else '0' end))";
                        }

                        strOut += strThis + orderSeq[i];

                        if (i < (xpath.Length - 1))
                        {
                            strOut += ", ";
                        }
                    }
                }
                else
                {
                    strOut += strAttr[0];
                }

                //remove possible SQL injection commands
                strOut = StripSqlCommands(strOut);

                return strOut;
            }
            return "";
        }

        public static string GetSqlFilterRange(string xpath, string sqlType, string searchFrom, string searchTo, string dataField = "XMLData")
        {
            var strOut = "";

            //remove SQL injection
            searchFrom = searchFrom.Replace("\'", "''");
            searchTo = searchTo.Replace("\'", "''");

            if (xpath == "")
            {
                strOut += "(" + dataField + " ";
                if (sqlType == "datetime")
                    strOut += " >= convert(datetime,'" + searchFrom + "') ";
                else if (sqlType.StartsWith("decimal"))
                    strOut += " >= " + searchFrom + " ";
                else
                    strOut += " >= '" + searchFrom + "' ";

                strOut += " and ";
                strOut += "" + dataField + " ";
                if (sqlType == "datetime")
                    strOut += " <= convert(datetime,'" + searchTo + "') ";
                else if (sqlType.StartsWith("decimal"))
                    strOut += " <= " + searchTo + " ";
                else
                    strOut += " <= '" + searchTo + "' ";
                strOut += ")";
            }
            else
            {

                if (sqlType == "datetime")
                {
                    strOut += "( ([" + dataField + "].value('(" + xpath;
                    strOut += ")[1]', '" + sqlType;
                    strOut += "') >= convert(datetime,'" + searchFrom + "') ";
                }
                else if (sqlType.StartsWith("decimal"))
                {
                    strOut += "( (CAST(CASE WHEN ISNUMERIC ([" + dataField + "].value('(" + xpath + ")[1]', 'nvarchar(max)')) = 0 THEN 0 ELSE ";
                    strOut += "[" + dataField + "].value('(" + xpath + ")[1]', 'decimal') END AS DECIMAL) >= " + searchFrom;
                }
                else
                {
                    strOut += "( ([" + dataField + "].value('(" + xpath;
                    strOut += ")[1]', '" + sqlType;
                    strOut += "') >= '" + searchFrom + Convert.ToChar("'");
                }
                strOut += ") and ";
                if (sqlType == "datetime")
                {
                    strOut += "([" + dataField + "].value('(" + xpath;
                    strOut += ")[1]', '" + sqlType;
                    strOut += "') <= convert(datetime,'" + searchTo + "') ";
                }
                else if (sqlType == "decimal")
                {
                    strOut += " (CAST(CASE WHEN ISNUMERIC ([" + dataField + "].value('(" + xpath + ")[1]', 'nvarchar(max)')) = 0 THEN 0 ELSE ";
                    strOut += "[" + dataField + "].value('(" + xpath + ")[1]', 'decimal') END AS DECIMAL) <= " + searchTo;
                }
                else
                {
                    strOut += "([" + dataField + "].value('(" + xpath;
                    strOut += ")[1]', '" + sqlType;
                    strOut += "') <= '" + searchTo + Convert.ToChar("'");
                }
                strOut += ") )";
            }
            //remove possible SQL injection commands
            strOut = StripSqlCommands(strOut);

            return strOut;
        }

        public static string GetSqlFilterText(string xpath, string sqlType, string searchText, string dataField = "XMLData", string testoperator = "=")
        {
            var strOut = "";

            //remove SQL injection
            searchText = searchText.Replace("\'", "''");

            //loop on each word in search criteria
            var words = searchText.Replace("  ", " ").Split(' ');
            var lp = 1;
            foreach (var word in words)
            {
                if (lp >= 2) strOut += " and ";
                strOut += "(";
                if (xpath == "")
                {
                    strOut += " " + dataField + " " + testoperator + " '" + searchText + "' ";
                }
                else
                {
                    strOut += "([" + dataField + "].value('(" + xpath;
                    strOut += ")[1]', 'nvarchar(max)";
                    strOut += "') " + testoperator + " '" + searchText + Convert.ToChar("'") + " collate SQL_Latin1_General_CP1_CI_AI ";
                    strOut += ") ";
                }
                strOut += ") ";
                lp = lp + 1;
            }

            //remove possible SQL injection commands
            strOut = StripSqlCommands(strOut);

            return strOut;
        }

        public static string GetSqlFilterLikeText(string xpath, string sqlType, string searchText, string dataField = "XMLData")
        {
            var strOut = "";

            //remove SQL injection
            searchText = searchText.Replace("\'", "''");

            //loop on each word in search criteria
            var words = searchText.Replace("  ", " ").Split(' ');
            var lp = 1;
            foreach (var word in words)
            {
                if (lp >= 2) strOut += " and ";
                strOut += "(";
                if (xpath == "")
                {
                    strOut += " " + dataField + " Like '%" + word + "%' ";
                }
                else
                {
                    strOut += "([" + dataField + "].value('(" + xpath;
                    strOut += ")[1]', 'nvarchar(max)";
                    strOut += "') LIKE '%" + word + "%' collate SQL_Latin1_General_CP1_CI_AI ";
                    strOut += ")";
                }
                strOut += ") ";
                lp = lp + 1;
            }

            //remove possible SQL injection commands
            strOut = StripSqlCommands(strOut);

            return strOut;
        }

        public static string StripSqlCommands(String SqlCmd)
        {
            // Strip command from string to make SQL Injection Safe.
            SqlCmd = " " + SqlCmd.ToLower();
            SqlCmd = SqlCmd.Replace(" dbcc "," ");
            SqlCmd = SqlCmd.Replace(" update ", " ");
            SqlCmd = SqlCmd.Replace(" delete ", " ");
            SqlCmd = SqlCmd.Replace(" insert ", " ");
            SqlCmd = SqlCmd.Replace(" drop ", " ");
            SqlCmd = SqlCmd.Replace(" create ", " ");
            SqlCmd = SqlCmd.Replace(" alter ", " ");
            SqlCmd = SqlCmd.Replace(" backup ", " ");
            SqlCmd = SqlCmd.Replace(" restore ", " ");
            SqlCmd = SqlCmd.Replace(" open ", " ");
            SqlCmd = SqlCmd.Replace(" close ", " ");
            SqlCmd = SqlCmd.Replace(" kill ", " ");
            SqlCmd = SqlCmd.Replace(" set ", " ");
            SqlCmd = SqlCmd.Replace(" add ", " ");
            SqlCmd = SqlCmd.Replace(" truncate ", " ");
            SqlCmd = SqlCmd.Replace(" disable ", " ");
            SqlCmd = SqlCmd.Replace(" commit ", " ");
            SqlCmd = SqlCmd.Replace(" begin ", " ");
            SqlCmd = SqlCmd.Replace(" enable ", " ");
            SqlCmd = SqlCmd.Replace(" disable ", " ");
            SqlCmd = SqlCmd.Replace(" use ", " ");

            return SqlCmd;

        }

        public static string SerializeToString(object obj)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());

            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);

                return writer.ToString();
            }
        }

        public static object DeserializeFromXml(string Xml, System.Type ObjType)
        {

            XmlSerializer ser;
            ser = new XmlSerializer(ObjType);
            StringReader stringReader;
            stringReader = new StringReader(Xml);
            XmlTextReader xmlReader;
            xmlReader = new XmlTextReader(stringReader);
            object obj;
            obj = ser.Deserialize(xmlReader);
            xmlReader.Close();
            stringReader.Close();
            return obj;

        }


        public static string ConvertTemplateToDisplayOnly(string inTemplate)
        {
            var outTemplate = "";
            var aryTempl = ParseTemplateText(inTemplate);

            for (var lp = 0; lp <= aryTempl.GetUpperBound(0); lp++)
            {

                if ((aryTempl[lp] != null))
                {
                    var htmlDecode = System.Web.HttpUtility.HtmlDecode(aryTempl[lp]);
                    if (htmlDecode != null && htmlDecode.ToLower().StartsWith("<tag"))
                    {

                        var strXml = System.Web.HttpUtility.HtmlDecode(aryTempl[lp]);
                        strXml = "<root>" + strXml + "</root>";
                        var ctrlid = "";
                        var ctrltype = "";
                        var ctrlxpath = "";

                        var xmlDoc = new XmlDocument();
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

                        if (xmlNod != null && (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null)))
                        {
                            ctrlid = xmlNod.Attributes["id"].InnerXml.ToLower();
                        }


                        if (!string.IsNullOrEmpty(ctrltype))
                        {
                            switch (ctrltype)
                            {
                                case "fileupload":
                                    ctrlxpath = "genxml/textbox/txt" + ctrlid;
                                    outTemplate += String.Format("[<tag type=\"valueof\" xpath=\"{0}\" />]", ctrlxpath);
                                    break;
                                case "label":
                                    ctrlxpath = "genxml/textbox/" + ctrlid;
                                    outTemplate += String.Format("[<tag type=\"valueof\" xpath=\"{0}\" />]", ctrlxpath);
                                    break;
                                case "textbox":
                                    ctrlxpath = "genxml/textbox/" + ctrlid;
                                    outTemplate += String.Format("[<tag type=\"valueof\" xpath=\"{0}\" />]", ctrlxpath);
                                    break;
                                case "dropdownlist":
                                    ctrlxpath = "genxml/dropdownlist/" + ctrlid + "/@selectedtext";
                                    outTemplate += String.Format("[<tag type=\"valueof\" xpath=\"{0}\" />]", ctrlxpath);
                                    break;
                                case "checkboxlist":
                                    ctrlxpath = "genxml/checkboxlist/" + ctrlid;
                                    outTemplate += String.Format("[<tag type=\"checkboxlistof\" xpath=\"{0}\" />]", ctrlxpath);
                                    break;
                                case "checkbox":
                                    ctrlxpath = "genxml/checkbox/" + ctrlid;
                                    outTemplate += String.Format("[<tag type=\"valueof\" xpath=\"{0}\" />]", ctrlxpath);
                                    break;
                                case "radiobuttonlist":
                                    ctrlxpath = "genxml/radiobuttonlist/" + ctrlid;
                                    outTemplate += String.Format("[<tag type=\"valueof\" xpath=\"{0}\" />]", ctrlxpath);
                                    break;
                                case "valueof":
                                    outTemplate += "[" + aryTempl[lp] + "]";
                                    break;
                                case "breakof":
                                    outTemplate += "[" + aryTempl[lp] + "]";
                                    break;
                                case "checkboxlistof":
                                    outTemplate += "[" + aryTempl[lp] + "]";
                                    break;
                                case "testof":
                                    outTemplate += "[" + aryTempl[lp] + "]";
                                    break;
                                case "htmlof":
                                    outTemplate += "[" + aryTempl[lp] + "]";
                                    break;
                            }
                        }
                        else
                        {
                            outTemplate += "[" + aryTempl[lp] + "]";
                        }
                    }
                    else
                    {
                        if ((lp % 2) == 0)
                        {
                            outTemplate += aryTempl[lp];
                        }
                        else
                        {
                            outTemplate += "[" + aryTempl[lp] + "]";                            
                        }
                    }
                }
            }

            return outTemplate;
        }


    }

}
