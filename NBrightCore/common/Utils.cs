using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using NBrightCore.providers;
using Image = System.Drawing.Image;


namespace NBrightCore.common
{

    public class UtilsEmail
    {
        bool _invalid = false;

        public bool IsValidEmail(string strIn)
        {
            _invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper);
            if (_invalid)
                return false;

            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strIn,
                   @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                   @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                   RegexOptions.IgnoreCase);
        }

        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            var idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                _invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
    }

    public class Utils
    {
        public static string UnCode(string codedval)
        {
            var strOut = "";
            var s = codedval.Split('.');
            foreach (var c in s)
            {
                if (c != "")
                {
                    strOut += (char)Convert.ToInt32(c);
                }                
            }
            return strOut;
        }

        public static void CreateFolder(string folderMapPath)
        {
            if (!Directory.Exists(folderMapPath))
            {
                Directory.CreateDirectory(folderMapPath);
            }
        }

        public static void DeleteFolder(string folderMapPath, bool recursive = false)
        {
            if (Directory.Exists(folderMapPath))
            {
                Directory.Delete(folderMapPath, recursive);
            }
        }

        public static string GetCurrentCulture()
        {
            return Thread.CurrentThread.CurrentCulture.ToString();
        }

        public static string GetCurrentCountryCode()
        {
            var cc = Thread.CurrentThread.CurrentCulture.Name;
            var c = cc.Split('-');
            var rtn = "";
            if (c.Length > 0) rtn = c[c.Length - 1];
            return rtn;
        }

        public static string RequestParam(HttpContext context, string paramName)
        {
            string result = null;

            if (context.Request.Form.Count != 0)
            {
                result = Convert.ToString(context.Request.Form[paramName]);
            }

            if (result == null)
            {
                if (context.Request.QueryString.Count != 0)
                {
                    result = Convert.ToString(context.Request.QueryString[paramName]);
                }
            }

            return (result == null) ? String.Empty : result.Trim();
        }

        public static string RequestQueryStringParam(HttpRequest Request, string paramName)
        {
            var result = String.Empty;

            if (Request.QueryString.Count != 0)
            {
                result = Convert.ToString(Request.QueryString[paramName]);
            }

            return (result == null) ? String.Empty : result.Trim();
        }

        public static string RequestQueryStringParam(HttpContext context, string paramName)
        {
            var result = String.Empty;

            if (context.Request.QueryString.Count != 0)
            {
                result = Convert.ToString(context.Request.QueryString[paramName]);
            }

            return (result == null) ? String.Empty : result.Trim();
        }


        public static void ForceDocDownload(string docFilePath, string fileName, HttpResponse response)
        {
            if (File.Exists(docFilePath) & !String.IsNullOrEmpty(fileName))
            {
                response.AppendHeader("content-disposition", "attachment; filename=" + fileName);
                response.ContentType = "application/octet-stream";
                response.WriteFile(docFilePath);

                response.Flush(); // Sends all currently buffered output to the client.
                response.SuppressContent = true;  // Gets or sets a value indicating whether to send HTTP content to the client.
                HttpContext.Current.ApplicationInstance.CompleteRequest(); // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event.
                //response.End();
            }

        }

        public static void ForceStringDownload(HttpResponse response, string fileName, string fileData)
        {
            response.AppendHeader("content-disposition", "attachment; filename=" + fileName);
            response.ContentType = "application/octet-stream";
            response.Write(fileData);
            response.End();
        }

        public static string FormatToSave(string inpData)
        {
            return FormatToSave(inpData, TypeCode.String,"");
        }

        public static string FormatToSave(string inpData, TypeCode dataTyp)
        {
            return FormatToSave(inpData, TypeCode.String,"");
        }

        public static string FormatToSave(string inpData, TypeCode dataTyp, string editlang)
        {
            if (string.IsNullOrWhiteSpace(editlang)) editlang = GetCurrentCulture();
            if (String.IsNullOrEmpty(inpData))
                return inpData;
            switch (dataTyp)
            {
                case TypeCode.Double:
                    //always save CultureInfo.InvariantCulture format to the XML
                    if (IsNumeric(inpData, editlang))
                    {
                        var cultureInfo = new CultureInfo(editlang, true);
                        var num = Convert.ToDouble(inpData, cultureInfo);
                        return num.ToString(CultureInfo.InvariantCulture);
                    }
                    if (IsNumeric(inpData)) // just check if we have a Invariant double
                    {
                        var num = Convert.ToDouble(inpData, CultureInfo.InvariantCulture);
                        return num.ToString(CultureInfo.InvariantCulture);
                    }
                    return "0";
                case TypeCode.DateTime:
                    if (IsDate(inpData, editlang))
                    {
                        var cultureInfo = new CultureInfo(editlang, true);
                        var dte = Convert.ToDateTime(inpData, cultureInfo);
                        return dte.ToString("s");
                    }
                    return "";
                default:
                    return Security.FormatDisableScripting(inpData);
            }
        }

        public static string FormatToDisplay(string inpData, TypeCode dataTyp, string formatCode = "")
        {
            return FormatToDisplay(inpData, GetCurrentCulture(), dataTyp, formatCode);
        }

        public static string FormatToDisplay(string inpData, string cultureCode, TypeCode dataTyp,
            string formatCode = "")
        {
            if (String.IsNullOrEmpty(inpData))
            {
                if (dataTyp == TypeCode.Double)
                {
                    return "0";
                }
                return inpData;
            }
            var outCulture = new CultureInfo(cultureCode, false);
            switch (dataTyp)
            {
                case TypeCode.Double:
                    if (IsNumeric(inpData))
                    {
                        return Double.Parse(inpData, CultureInfo.InvariantCulture).ToString(formatCode, outCulture);
                    }
                    return "0";
                case TypeCode.DateTime:
                    if (IsDate(inpData))
                    {
                        if (formatCode == "") formatCode = "d";
                        return DateTime.Parse(inpData).ToString(formatCode, outCulture);
                    }
                    return inpData;
                default:
                    return inpData;
            }
        }


        /// <summary>
        ///  IsEmail function checks for a valid email format         
        /// </summary>
        public static bool IsEmail(string emailaddress)
        {
            var e = new UtilsEmail();
            return e.IsValidEmail(emailaddress);
        }

        /// <summary>
        ///  IsNumeric function check if a given value is numeric, based on the culture code passed.  If no culture code is passed then a test on InvariantCulture is done.
        /// </summary>
        public static bool IsNumeric(object expression, string cultureCode = "")
        {
            if (expression == null) return false;

            double retNum;
            bool isNum = false;
            if (cultureCode != "")
            {
                var cultureInfo = new CultureInfo(cultureCode, true);
                isNum = Double.TryParse(Convert.ToString(expression), NumberStyles.Number, cultureInfo.NumberFormat,
                    out retNum);
            }
            else
            {
                isNum = Double.TryParse(Convert.ToString(expression), NumberStyles.Number, CultureInfo.InvariantCulture,
                    out retNum);
            }

            return isNum;
        }

        // IsDate culture Function
        public static bool IsDate(object expression, string cultureCode)
        {
            DateTime rtnD;
            return DateTime.TryParse(Convert.ToString(expression), CultureInfo.CreateSpecificCulture(cultureCode),
                DateTimeStyles.None, out rtnD);
        }

        public static bool IsDate(object expression)
        {
            return IsDate(expression, GetCurrentCulture());
        }

        public static void SaveFile(string fullFileName, string data)
        {
            var buffer = StrToByteArray(data);
            SaveFile(fullFileName, buffer);
        }

        public static void SaveFile(string fullFileName, byte[] buffer)
        {
            if (File.Exists(fullFileName))
            {
                File.SetAttributes(fullFileName, FileAttributes.Normal);
            }
            FileStream fs = null;
            try
            {
                fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
                fs.Write(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                // ignore, stop eror here, not important if locked.
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        public static string ReadFile(string filePath)
        {
            StreamReader reader = null;
            string fileContent;
            try
            {
                if (!File.Exists(filePath)) return "";
                reader = File.OpenText(filePath);
                fileContent = reader.ReadToEnd();
            }
            catch (Exception e)
            {
                // ignore, stop eror here, not important if locked.
                fileContent = "";
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return fileContent;
        }

        public static string FormatFolderPath(string folderPath)
        {
            if (String.IsNullOrEmpty(folderPath) || String.IsNullOrEmpty(folderPath.Trim()))
            {
                return "";
            }

            return folderPath.EndsWith("/") ? folderPath : folderPath + "/";
        }

        public static byte[] StrToByteArray(string str)
        {
            var encoding = new UTF8Encoding();
            return encoding.GetBytes(str);
        }

        /// <summary>
        /// Convert input stream to UTF8 string, can be used for text files.
        /// </summary>
        /// <param name="InpStream"></param>
        /// <returns></returns>
        public static string InputStreamToString(Stream InpStream)
        {
            // Create a Stream object.
            // Find number of bytes in stream.
            var strLen = Convert.ToInt32(InpStream.Length);
            // Create a byte array.
            var strArr = new byte[strLen];
            // Read stream into byte array.
            InpStream.Read(strArr, 0, strLen);
            // Convert byte array to a text string.
            var strmContents = Encoding.UTF8.GetString(strArr);
            return strmContents;
        }

        /// <summary>
        /// Convert input stream to base-64 string, can be used for image/binary files.
        /// </summary>
        /// <param name="InpStream"></param>
        /// <returns></returns>
        public static string Base64StreamToString(Stream InpStream)
        {
            // Create a Stream object.
            // Find number of bytes in stream.
            var strLen = Convert.ToInt32(InpStream.Length);
            // Create a byte array.
            var strArr = new byte[strLen];
            // Read stream into byte array.
            InpStream.Read(strArr, 0, strLen);
            var strmContents = Convert.ToBase64String(strArr);
            return strmContents;
        }

        public static MemoryStream Base64StringToStream(string inputStr)
        {
            var myByte = Convert.FromBase64String(inputStr);
            var theMemStream = new MemoryStream();
            theMemStream.Write(myByte, 0, myByte.Length);
            return theMemStream;
        }

        public static Image SaveImgBase64ToFile(string FileMapPath, string strBase64Img)
        {
            // Save the image to a file.
            var mem = Base64StringToStream(strBase64Img);
            Image pic = Image.FromStream(mem);
            var fPath = FileMapPath;
            if (fPath.ToLower().EndsWith(".gif"))
            {
                pic.Save(fPath, ImageFormat.Gif);
            }
            else if (fPath.ToLower().EndsWith(".jpg") | fPath.ToLower().EndsWith(".jpeg"))
            {
                pic.Save(fPath, ImageFormat.Jpeg);
            }
            else if (fPath.ToLower().EndsWith(".png"))
            {
                pic.Save(fPath, ImageFormat.Png);
            }
            else
            {
                pic.Save(fPath, ImageFormat.Jpeg);
            }
            return pic;
        }


        public static void SaveBase64ToFile(string FileMapPath, string strBase64)
        {
            // Save the image to a file.
            var mem = Base64StringToStream(strBase64);

            FileStream outStream = File.OpenWrite(FileMapPath);
            mem.WriteTo(outStream);
            outStream.Flush();
            outStream.Close();
        }



        public static string ReplaceFileExt(string fileName, string newExt)
        {
            var strOut = Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + newExt;
            return strOut;
        }

        public static string FormatAsMailTo(string email)
        {
            var functionReturnValue = "";

            if (!String.IsNullOrEmpty(email) && !String.IsNullOrEmpty(email.Trim(Convert.ToChar(" "))))
            {
                if (email.IndexOf(Convert.ToChar("@")) != -1)
                {
                    functionReturnValue = "<a href=\"mailto:" + email + "\">" + email + "</a>";
                }
                else
                {
                    functionReturnValue = email;
                }
            }

            return CloakText(functionReturnValue);

        }


        // obfuscate sensitive data to prevent collection by robots and spiders and crawlers
        public static string CloakText(string personalInfo)
        {
            return CloakText(personalInfo, true);
        }

        public static string CloakText(string personalInfo, bool addScriptTag)
        {
            if (personalInfo != null)
            {
                var sb = new StringBuilder();
                var chars = personalInfo.ToCharArray();
                foreach (char chr in chars)
                {
                    sb.Append(((int) chr).ToString(CultureInfo.InvariantCulture));
                    sb.Append(',');
                }
                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                if (addScriptTag)
                {
                    var sbScript = new StringBuilder();
                    sbScript.Append("<script type=\"text/javascript\">");
                    sbScript.Append("document.write(String.fromCharCode(" + sb + "))");
                    sbScript.Append("</script>");
                    return sbScript.ToString();
                }
                return String.Format("document.write(String.fromCharCode({0}))", sb);
            }
            return "";
        }

        public static void DeleteSysFile(string filePathName)
        {
            try
            {
                File.Delete(filePathName);
            }
            catch (Exception)
            {
                //ignore file could be locked.
                // should only be called if not important to remove file.                
            }
        }

        public static object GetCache(string strCacheKey)
        {
            return CmsProviderManager.Default.GetCache(strCacheKey);
        }

        public static void SetCache(string CacheKey, object objObject)
        {
            CmsProviderManager.Default.SetCache(CacheKey, objObject, DateTime.Now + new TimeSpan(1, 0, 0, 0));
        }

        public static void SetCache(string CacheKey, object objObject, DateTime AbsoluteExpiration)
        {
            CmsProviderManager.Default.SetCache(CacheKey, objObject, AbsoluteExpiration);
        }

        public static void RemoveCache(string strCacheKey)
        {
            CmsProviderManager.Default.RemoveCache(strCacheKey);
        }

        /// <summary>
        /// set cache and use a key list, so the cache objects can be cleared by RemoveCacheList()
        /// </summary>
        /// <param name="cacheKey">key for cache</param>
        /// <param name="objObject">object to be cached</param>
        /// <param name="cacheKeyList">optional keylist name</param>
        public static void SetCacheList(string cacheKey, object objObject,string cacheKeyList = "")
        {
            SetCache(cacheKey, objObject);
            if (cacheKeyList != "")
            {
                var lst = (List<string>)GetCache(cacheKeyList);
                if (lst == null)
                {
                    lst = new List<string>();
                }
                if (!lst.Contains(cacheKey))
                {
                    lst.Add(cacheKey);
                }
                SetCache(cacheKeyList,lst);                    
            }
        }
        /// <summary>
        /// Remove all cachekey from cache that exist in the cachelist (add by SetCacheList())
        /// </summary>
        /// <param name="cacheKeyList">cachelist name</param>
        public static void RemoveCacheList(string cacheKeyList)
        {
            var lst = (List<string>)GetCache(cacheKeyList);
            if (lst != null)
            {
                foreach (var ck in lst)
                {
                    RemoveCache(ck);
                }
                RemoveCache(cacheKeyList);
            }
        }


        /// <summary>
        /// Parses string input into array based on character
        /// </summary>
        /// <param name="templText">String to Parse</param>
        /// <param name="openchar">Open character</param>
        /// <param name="closechar">Close character</param>
        /// <returns></returns>
        public static string[] ParseTemplateText(string templText, String openchar = "[", String closechar = "]")
        {
            char[] paramAry = {Convert.ToChar(openchar), Convert.ToChar(closechar)};

            //use double sqr brqckets as escape char.
            var foundEscapeChar = false;
            if (templText.IndexOf(openchar + openchar, StringComparison.Ordinal) > 0 |
                templText.IndexOf(closechar + closechar, StringComparison.Ordinal) > 0)
            {
                templText = templText.Replace(openchar + openchar, "**SQROPEN**");
                templText = templText.Replace(closechar + closechar, "**SQRCLOSE**");
                foundEscapeChar = true;
            }

            var strOut = templText.Split(paramAry);

            if (foundEscapeChar)
            {
                for (var lp = 0; lp <= strOut.GetUpperBound(0); lp++)
                {
                    if (strOut[lp].Contains("**SQROPEN**"))
                    {
                        strOut[lp] = strOut[lp].Replace("**SQROPEN**", openchar);
                    }
                    if (strOut[lp].Contains("**SQRCLOSE**"))
                    {
                        strOut[lp] = strOut[lp].Replace("**SQRCLOSE**", closechar);
                    }
                }
            }

            return strOut;
        }

        public static string CleanInput(string strIn)
        {
            return CleanInput(strIn, "");
        }

        /// <summary>
        /// CleanInput strips out all nonalphanumeric characters except periods (.), at symbols (@), and hyphens (-), and returns the remaining string. However, you can modify the regular expression pattern so that it strips out any characters that should not be included in an input string.
        /// </summary>
        /// <param name="strIn">Dirty String</param>
        /// <param name="regexpr"></param>
        /// <returns>Clean String</returns>
        public static string CleanInput(string strIn, string regexpr = "")
        {
            if (regexpr == "") regexpr = @"[^\w\.@-]";
            // Replace invalid characters with empty strings. 
            return Regex.Replace(strIn, regexpr, "", RegexOptions.None);
        }

        /// <summary>
/// Produces optional, URL-friendly version of a title, "like-this-one". 
/// hand-tuned for speed, reflects performance refactoring contributed
/// by John Gietzen (user otac0n) 
/// </summary>
public static string UrlFriendly(string title)
{
    if (title == null) return "";

    const int maxlen = 255;
    int len = title.Length;
    bool prevdash = false;
    var sb = new StringBuilder(len);
    char c;

    for (int i = 0; i < len; i++)
    {
        c = title[i];
        if ((c >= 'a' && c <= 'z') || (c >= 'а' && c <= 'я') || (c >= '0' && c <= '9'))
        {
            sb.Append(c);
            prevdash = false;
        }
        else if ((c >= 0x4E00 && c <= 0x9FFF) ||
        (c >= 0x3400 && c <= 0x4DBF) ||
        (c >= 0x3400 && c <= 0x4DBF) ||
        (c >= 0x20000 && c <= 0x2CEAF) ||
        (c >= 0x2E80 && c <= 0x31EF) ||
        (c >= 0xF900 && c <= 0xFAFF) ||
        (c >= 0xFE30 && c <= 0xFE4F) ||
        (c >= 0xF2800 && c <= 0x2FA1F))
        {
            sb.Append(c);
            prevdash = false;
        }
        else if (c >= 'A' && c <= 'Z' || (c >= 'А' && c <= 'Я'))
        {
            // tricky way to convert to lowercase
            sb.Append((char)(c | 32));
            prevdash = false;
        }
        else if (c == ' ' || c == ',' || c == '.' || c == '/' || 
            c == '\\' || c == '-' || c == '_' || c == '=')
        {
            if (!prevdash && sb.Length > 0)
            {
                sb.Append('-');
                prevdash = true;
            }
        }
        else if ((int)c >= 128)
        {
            int prevlen = sb.Length;
            sb.Append(RemapInternationalCharToAscii(c));
            if (prevlen != sb.Length) prevdash = false;
        }
        if (i == maxlen) break;
    }

    if (prevdash)
        return sb.ToString().Substring(0, sb.Length - 1);
    else
        return sb.ToString();
}

public static string RemapInternationalCharToAscii(char c)
{
    string s = c.ToString().ToLowerInvariant();
    if ("àåáâäãåą".Contains(s))
    {
        return "a";
    }
    else if ("èéêëę".Contains(s))
    {
        return "e";
    }
    else if ("ìíîïı".Contains(s))
    {
        return "i";
    }
    else if ("òóôõöøőð".Contains(s))
    {
        return "o";
    }
    else if ("ùúûüŭů".Contains(s))
    {
        return "u";
    }
    else if ("çćčĉ".Contains(s))
    {
        return "c";
    }
    else if ("żźž".Contains(s))
    {
        return "z";
    }
    else if ("śşšŝ".Contains(s))
    {
        return "s";
    }
    else if ("ñń".Contains(s))
    {
        return "n";
    }
    else if ("ýÿ".Contains(s))
    {
        return "y";
    }
    else if ("ğĝ".Contains(s))
    {
        return "g";
    }
    else if (c == 'ř')
    {
        return "r";
    }
    else if (c == 'ł')
    {
        return "l";
    }
    else if (c == 'đ')
    {
        return "d";
    }
    else if (c == 'ß')
    {
        return "ss";
    }
    else if (c == 'Þ')
    {
        return "th";
    }
    else if (c == 'ĥ')
    {
        return "h";
    }
    else if (c == 'ĵ')
    {
        return "j";
    }
    else
    {
        return "";
    }
}



        /// <summary>
        /// Strip accents from string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>String without accents</returns>
        public static string StripAccents(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s.Normalize(NormalizationForm.FormKD))
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.EnclosingMark:
                        break;

                    default:
                        sb.Append(c);
                        break;
                }
            return sb.ToString();
        }

        /// <summary>
        /// Get Azure Authentication for Translator.
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="ClientSecret"></param>
        /// <returns></returns>
        public static AdmAccessToken GetAzureAccessToken(String ClientId, String ClientSecret)
        {
            var admAuth = new AdmAuthentication(ClientId, ClientSecret);
            var token = admAuth.GetAccessToken();
            return token;
        }

        /// <summary>
        /// Create dictionary of config setting from files
        /// </summary>
        /// <param name="DefaultConfigMapPath"></param>
        /// <param name="SecondaryConfigMapPath"></param>
        /// <param name="configNameCSV">CSV list of sectiuonnames to be returned, "" for all</param>
        /// <param name="AdvancedFlag">Flag to select advanced settings "1"=Advanved Only, "0"=Simple Only,""=All </param>
        /// <returns>Dictionary of all config settings</returns>
        public static Dictionary<string, NBrightSetting> ConfigBuildDictionary(String DefaultConfigMapPath,
            String SecondaryConfigMapPath, String configNameCSV = "", String AdvancedFlag = "")
        {
            var outDict = new Dictionary<string, NBrightSetting>();

            if (File.Exists(DefaultConfigMapPath))
            {

                var xmlConfigDoc = new XmlDocument();
                XmlNodeList xmlNodList = null;
                xmlConfigDoc.Load(DefaultConfigMapPath);

                if (configNameCSV == "")
                {
                    xmlNodList = xmlConfigDoc.SelectNodes("root/*");
                    if (xmlNodList != null)
                    {
                        foreach (XmlNode xNod in xmlNodList)
                        {
                            configNameCSV += xNod.Name + ",";
                        }
                        configNameCSV = configNameCSV.TrimEnd(',');
                    }
                }

                foreach (var configName in configNameCSV.Split(','))
                {
                    xmlNodList = xmlConfigDoc.SelectNodes("root/" + configName + "/*");
                    if (xmlNodList != null)
                    {
                        foreach (XmlNode xNod in xmlNodList)
                        {
                            if (xNod.Attributes != null && xNod.Attributes["value"] != null)
                            {
                                if ((AdvancedFlag == "") ||
                                    (AdvancedFlag == "0" &&
                                     (xNod.Attributes["advanced"] == null || xNod.Attributes["advanced"].Value == "0")) ||
                                    (AdvancedFlag == "1" && xNod.Attributes["advanced"] != null &&
                                     xNod.Attributes["advanced"].Value == "1"))
                                {
                                    var obj = new NBrightSetting();
                                    obj.Key = configName + "." + xNod.Name;
                                    obj.Name = xNod.Name;
                                    obj.Type = "";
                                    if (xNod.Attributes["type"] != null) obj.Type = xNod.Attributes["type"].Value;
                                    obj.Value = "";
                                    if (xNod.Attributes["value"] != null) obj.Value = xNod.Attributes["value"].Value;

                                    if (outDict.ContainsKey(configName + "." + xNod.Name))
                                    {
                                        outDict[configName + "." + xNod.Name] = obj;
                                    }
                                    else
                                    {
                                        outDict.Add(configName + "." + xNod.Name, obj);
                                    }
                                }
                            }
                        }
                    }

                }

                //overwrite with secondary file data
                if (File.Exists(SecondaryConfigMapPath))
                {
                    xmlConfigDoc = new XmlDocument();
                    xmlConfigDoc.Load(SecondaryConfigMapPath);
                    foreach (var configName in configNameCSV.Split(','))
                    {
                        xmlNodList = xmlConfigDoc.SelectNodes("root/" + configName + "/*");
                        if (xmlNodList != null)
                        {
                            foreach (XmlNode xNod in xmlNodList)
                            {
                                if (xNod.Attributes != null && xNod.Attributes["value"] != null)
                                {
                                    if ((AdvancedFlag == "") ||
                                        (AdvancedFlag == "0" &&
                                         (xNod.Attributes["advanced"] == null ||
                                          xNod.Attributes["advanced"].Value == "0")) ||
                                        (AdvancedFlag == "1" && xNod.Attributes["advanced"] != null &&
                                         xNod.Attributes["advanced"].Value == "1"))
                                    {
                                        var obj = new NBrightSetting();
                                        obj.Key = configName + "." + xNod.Name;
                                        obj.Name = xNod.Name;
                                        obj.Type = "";
                                        if (xNod.Attributes["type"] != null) obj.Type = xNod.Attributes["type"].Value;
                                        obj.Value = "";
                                        if (xNod.Attributes["value"] != null)
                                            obj.Value = xNod.Attributes["value"].Value;

                                        if (outDict.ContainsKey(configName + "." + xNod.Name))
                                        {
                                            outDict[configName + "." + xNod.Name] = obj;
                                        }
                                        else
                                        {
                                            outDict.Add(configName + "." + xNod.Name, obj);
                                        }
                                    }
                                }
                            }
                        }

                    }


                }
            }

            return outDict;

        }

        /// <summary>
        /// Create Dictionary of config sections  i.e. all "root/*" nodes
        /// </summary>
        /// <param name="DefaultConfigDictionary">Dictionary of all Config settings. (Created by ConfigBuildDictionary function)</param>
        /// <returns></returns>
        public static List<string> ConfigBuildSectionList(Dictionary<string, NBrightSetting> DefaultConfigDictionary)
        {
            var outL = new List<string>();

            foreach (var i in DefaultConfigDictionary)
            {
                var secName = i.Key.Split('.')[0];
                if (secName != null)
                {
                    if (!outL.Contains(secName)) outL.Add(secName);
                }
            }

            return outL;
        }


        /// <summary>
        /// Take the xml config file and convert it to a Template.
        /// </summary>
        /// <param name="settingDict">Dictionary of Settings</param>
        /// <param name="sectionName">Name of the config section to edit (e.g. "products")</param>
        /// <returns>String html nbright template for displaying settings options</returns>
        public static String ConfigConvertToTemplate(Dictionary<string, NBrightSetting> settingDict, String sectionName)
        {
            var strTempl = "";

            if (settingDict != null)
            {
                strTempl += "<table><th></th><th></th>";
                foreach (var i in settingDict)
                {
                    if (i.Key.ToLower().StartsWith(sectionName.ToLower()) | sectionName == "")
                    {
                        strTempl += "<tr><td>";
                        strTempl += i.Key + " : ";
                        strTempl += "</td><td>";
                        var obj = i.Value;
                        switch (obj.Type)
                        {
                            case "decimal":
                                strTempl += "[<tag id='config" + i.Key + "' type='textbox' text='" + obj.Value +
                                            "' width='100px' maxlength='50'  />]";
                                break;
                            case "int":
                                strTempl += "[<tag id='config" + i.Key + "' type='textbox' text='" + obj.Value +
                                            "' width='100px' maxlength='50'  />]";
                                break;
                            case "bool":
                                strTempl += "[<tag id='config" + i.Key + "' type='checkbox' checked='" + obj.Value +
                                            "' />]";
                                break;
                            default:
                                strTempl += "[<tag id='config" + i.Key + "' type='textbox' text='" + obj.Value +
                                            "' width='600px' maxlength='250'  />]";
                                break;
                        }
                        strTempl += "</td>";
                    }
                }
                strTempl += "</table>";
            }
            return strTempl;
        }

        /// <summary>
        /// Take a Repeater and convert it to XML config data.
        /// </summary>
        /// <param name="xmlConfig"></param>
        /// <returns></returns>
        public static XmlDocument ConfigConvertToXml()
        {
            var xmlDoc = new XmlDocument();

            return xmlDoc;
        }

        /// <summary>
        /// Replace settings tokens in a template. {Setting:*} and [Setting:*]
        /// </summary>
        /// <param name="strTemplate">Template txt to be searched</param>
        /// <param name="settingsDic">Dictionary of settings</param>
        /// <returns></returns>
        public static string ReplaceSettingTokens(string strTemplate, Dictionary<string, string> settingsDic)
        {
            const string tokenTag = "Settings:";
            if (strTemplate.Contains(tokenTag))
            {
                var aryTempl = ParseTemplateText(strTemplate);
                foreach (var s in aryTempl)
                {
                    if (s.StartsWith(tokenTag))
                    {
                        var xpath = s.Replace(tokenTag, "").Replace("]", "");
                        var xValue = "";
                        if (settingsDic.ContainsKey(xpath)) xValue = settingsDic[xpath];
                        strTemplate = strTemplate.Replace("{" + s + "}", xValue);
                            // deal with situation where a token is in the template as "[]" and a tag as "{}"
                        strTemplate = strTemplate.Replace(s, xValue);
                    }
                }

                //Search for {Settings:*}, this token may be used within a tag [] token
                aryTempl = ParseTemplateText(strTemplate, "{", "}");
                foreach (var s in aryTempl)
                {
                    if (s.StartsWith(tokenTag))
                    {
                        var xpath = s.Replace(tokenTag, "").Replace("}", "");
                        if (settingsDic.ContainsKey(xpath))
                            strTemplate = strTemplate.Replace("{" + s + "}", settingsDic[xpath]);
                    }
                }

            }
            return strTemplate;
        }


        /// <summary>
        /// Replaces any [Url:*] tokens in the template passed in.
        /// </summary>
        /// <param name="strTemplate"></param>
        /// <returns></returns>
        public static string ReplaceUrlTokens(string strTemplate)
        {
            const string tokenTag = "Url:";
            if (strTemplate.Contains(tokenTag))
            {
                var aryTempl = ParseTemplateText(strTemplate);
                foreach (var s in aryTempl)
                {
                    if (s.StartsWith(tokenTag))
                    {
                        var urlparam = s.Replace(tokenTag, "").Replace("]", "");
                        strTemplate = strTemplate.Replace("{" + s + "}", RequestParam(HttpContext.Current, urlparam));
                            // deal with situation where a token is in the template as "[]" and a tag as "{}"
                        strTemplate = strTemplate.Replace(s, RequestParam(HttpContext.Current, urlparam));
                    }
                }

                aryTempl = ParseTemplateText(strTemplate, "{", "}");
                foreach (var s in aryTempl)
                {
                    if (s.StartsWith(tokenTag))
                    {
                        var urlparam = s.Replace(tokenTag, "").Replace("}", "");
                        strTemplate = strTemplate.Replace("{" + s + "}", RequestParam(HttpContext.Current, urlparam));
                    }
                }

            }
            return strTemplate;
        }

        public static string GetUniqueKey(int maxSize = 8)
        {
            var a = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var chars = a.ToCharArray();
            var data = new byte[1];
            var crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            data = new byte[maxSize];
            crypto.GetNonZeroBytes(data);
            var result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b%(chars.Length - 1)]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Converts the provided app-relative path into an absolute Url containing the 
        /// full host name
        /// </summary>
        /// <param name="relativeUrl">App-Relative path</param>
        /// <returns>Provided relativeUrl parameter as fully qualified Url</returns>
        public static string ToAbsoluteUrl(string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl))
                return relativeUrl;

            if (HttpContext.Current == null)
                return relativeUrl;

            if (relativeUrl.StartsWith("/"))
                relativeUrl = relativeUrl.Insert(0, "~");
            if (!relativeUrl.StartsWith("~/"))
                relativeUrl = relativeUrl.Insert(0, "~/");

            var url = HttpContext.Current.Request.Url;
            var port = url.Port == 80 || (url.Scheme == "https" && url.Port == 443) ? "" : ":" + url.Port;

            return String.Format("{0}://{1}{2}{3}",
                url.Scheme, url.Host, port, VirtualPathUtility.ToAbsolute(relativeUrl));
        }

        public static string GetRelativeUrl(string fullUrl)
        {
            try
            {
                Uri uri = new Uri(fullUrl);//fullUrl is absoluteUrl   
                string relativeUrl = uri.AbsolutePath;//The Uri property AbsolutePath gives the relativeUrl   

                return relativeUrl;
            }
            catch (Exception ex)
            {
                return fullUrl;
                //throw ex;   
            }
        }

        public static string GetTranslatorHeaderValue(String clientId, String clientSecret)
        {
            String strTranslatorAccessURI = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
            String strRequestDetails = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1} &scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(clientSecret));

            AdmAccessToken token = HttpPost(strTranslatorAccessURI, strRequestDetails);

            return "Bearer " + token.access_token;
        }

		private static AdmAccessToken HttpPost(string DatamarketAccessUri, string requestDetails)
        {
			//Prepare OAuth request 
            WebRequest webRequest = WebRequest.Create(DatamarketAccessUri);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
			byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
            webRequest.ContentLength = bytes.Length;
			using (Stream outputStream = webRequest.GetRequestStream())
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
			using (WebResponse webResponse = webRequest.GetResponse())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
				//Get deserialized object from JSON stream
                AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
				return token;
            }
        }


        public static string GetTranslatedText(String headerValue, String txtToTranslate, string fromLang, string toLang)
        {

            if (fromLang != toLang)
            {
                string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" +
                     System.Web.HttpUtility.UrlEncode(txtToTranslate) + "&from=" + fromLang + "&to=" + toLang;
                System.Net.WebRequest translationWebRequest = System.Net.WebRequest.Create(uri);
                translationWebRequest.Headers.Add("Authorization", headerValue);

                System.Net.WebResponse response = null;
                response = translationWebRequest.GetResponse();
                System.IO.Stream stream = response.GetResponseStream();
                System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

                System.IO.StreamReader translatedStream = new System.IO.StreamReader(stream, encode);
                System.Xml.XmlDocument xTranslation = new System.Xml.XmlDocument();
                xTranslation.LoadXml(translatedStream.ReadToEnd());

                return xTranslation.InnerText;
            }
            return txtToTranslate;
        }

        public static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            int place = source.IndexOf(find, StringComparison.Ordinal);
            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        public static string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int place = source.LastIndexOf(find, StringComparison.Ordinal);
            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        public static string DecodeInternalField(string toDecode)
        {
            if (toDecode != null)
            {
                string decodedString = toDecode.Replace("_x", "%u").Replace("_", "");
                return HttpUtility.UrlDecode(decodedString);
            }
            else
            {
                return null;
            }
        }


    }
}
