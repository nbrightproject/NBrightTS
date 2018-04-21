using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace NBrightCorev2.common
{
    public class Security
    {

        public static string Decrypt(string strKey, string strData)
        {
            if (String.IsNullOrEmpty(strData))
            {
                return "";
            }
            string strValue = "";
            if (!String.IsNullOrEmpty(strKey))
            {
                //convert key to 16 characters for simplicity
                if (strKey.Length < 16)
                {
                    strKey = strKey + "XXXXXXXXXXXXXXXX".Substring(0, 16 - strKey.Length);
                }
                else
                {
                    strKey = strKey.Substring(0, 16);
                }

                //create encryption keys
                byte[] byteKey = Encoding.UTF8.GetBytes(strKey.Substring(0, 8));
                byte[] byteVector = Encoding.UTF8.GetBytes(strKey.Substring(strKey.Length - 8, 8));

                //convert data to byte array and Base64 decode
                var byteData = new byte[strData.Length];
                try
                {
                    byteData = Convert.FromBase64String(strData);
                }
                catch //invalid length
                {
                    strValue = strData;
                }
                if (String.IsNullOrEmpty(strValue))
                {
                    try
                    {
                        //decrypt
                        var objDes = new DESCryptoServiceProvider();
                        var objMemoryStream = new MemoryStream();
                        var objCryptoStream = new CryptoStream(objMemoryStream,
                            objDes.CreateDecryptor(byteKey, byteVector), CryptoStreamMode.Write);
                        objCryptoStream.Write(byteData, 0, byteData.Length);
                        objCryptoStream.FlushFinalBlock();

                        //convert to string
                        Encoding objEncoding = Encoding.UTF8;
                        strValue = objEncoding.GetString(objMemoryStream.ToArray());
                    }
                    catch //decryption error
                    {
                        strValue = "";
                    }
                }
            }
            else
            {
                strValue = strData;
            }
            return strValue;
        }

        public static string Encrypt(string strKey, string strData)
        {
            string strValue;
            if (!String.IsNullOrEmpty(strKey))
            {
                //convert key to 16 characters for simplicity
                if (strKey.Length < 16)
                {
                    strKey = strKey + "XXXXXXXXXXXXXXXX".Substring(0, 16 - strKey.Length);
                }
                else
                {
                    strKey = strKey.Substring(0, 16);
                }

                //create encryption keys
                byte[] byteKey = Encoding.UTF8.GetBytes(strKey.Substring(0, 8));
                byte[] byteVector = Encoding.UTF8.GetBytes(strKey.Substring(strKey.Length - 8, 8));

                //convert data to byte array
                byte[] byteData = Encoding.UTF8.GetBytes(strData);

                //encrypt 
                var objDes = new DESCryptoServiceProvider();
                var objMemoryStream = new MemoryStream();
                var objCryptoStream = new CryptoStream(objMemoryStream, objDes.CreateEncryptor(byteKey, byteVector),
                    CryptoStreamMode.Write);
                objCryptoStream.Write(byteData, 0, byteData.Length);
                objCryptoStream.FlushFinalBlock();

                //convert to string and Base64 encode
                strValue = Convert.ToBase64String(objMemoryStream.ToArray());
            }
            else
            {
                strValue = strData;
            }
            return strValue;
        }

        /// -----------------------------------------------------------------------------
        ///  <summary>
        ///  This function uses Regex search strings to remove HTML tags which are
        ///  targeted in Cross-site scripting (XSS) attacks.  This function will evolve
        ///  to provide more robust checking as additional holes are found.
        ///  </summary>
        ///  <param name="strInput">This is the string to be filtered</param>
        /// <param name="filterlinks">remove href elements</param>
        /// <returns>Filtered UserInput</returns>
        ///  <remarks>
        ///  This is a private function that is used internally by the FormatDisableScripting function
        ///  </remarks>
        ///  <history>
        ///      [cathal]        3/06/2007   Created
        ///  </history>
        /// -----------------------------------------------------------------------------
        private static string FilterStrings(string strInput, bool filterlinks)
        {
            //setup up list of search terms as items may be used twice
            var tempInput = strInput;
            var listStrings = new List<string>
            {
                "<script[^>]*>.*?</script[^><]*>",
                "<script",
                "<input[^>]*>.*?</input[^><]*>",
                "<object[^>]*>.*?</object[^><]*>",
                "<embed[^>]*>.*?</embed[^><]*>",
                "<applet[^>]*>.*?</applet[^><]*>",
                "<form[^>]*>.*?</form[^><]*>",
                "<option[^>]*>.*?</option[^><]*>",
                "<select[^>]*>.*?</select[^><]*>",
                "<iframe[^>]*>.*?</iframe[^><]*>",
                "<iframe.*?<",
                "<iframe.*?",
                "<ilayer[^>]*>.*?</ilayer[^><]*>",
                "<form[^>]*>",
                "</form[^><]*>",
                "onerror",
                "onmouseover",
                "javascript:",
                "vbscript:",
                "unescape",
                "alert[\\s(&nbsp;)]*\\([\\s(&nbsp;)]*'?[\\s(&nbsp;)]*[\"(&quot;)]?",
                @"eval*.\(",
                "onload"
            };

            if (filterlinks)
            {
                listStrings.Add("<a[^>]*>.*?</a[^><]*>");
                listStrings.Add("<a");
            }

            const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            const string replacement = " ";

            //check if text contains encoded angle brackets, if it does it we decode it to check the plain text
            if (tempInput.Contains("&gt;") && tempInput.Contains("&lt;"))
            {
                //text is encoded, so decode and try again
                tempInput = HttpUtility.HtmlDecode(tempInput);
                tempInput = listStrings.Aggregate(tempInput,
                    (current, s) => Regex.Replace(current, s, replacement, options));

                //Re-encode
                tempInput = HttpUtility.HtmlEncode(tempInput);
            }
            else
            {
                tempInput = listStrings.Aggregate(tempInput,
                    (current, s) => Regex.Replace(current, s, replacement, options));
            }
            return tempInput;
        }

        /// -----------------------------------------------------------------------------
        ///  <summary>
        ///  This function uses Regex search strings to remove HTML tags which are
        ///  targeted in Cross-site scripting (XSS) attacks.  This function will evolve
        ///  to provide more robust checking as additional holes are found.
        ///  </summary>
        ///  <param name="strInput">This is the string to be filtered</param>
        /// <param name="filterlinks">Remove href link elements</param>
        /// <returns>Filtered UserInput</returns>
        ///  <remarks>
        ///  This is a private function that is used internally by the InputFilter function
        ///  </remarks>
        /// -----------------------------------------------------------------------------
        public static string FormatDisableScripting(string strInput, bool filterlinks = true)
        {
            var tempInput = strInput;
            if (strInput == " " || String.IsNullOrEmpty(strInput))
            {
                return tempInput;
            }
            tempInput = FilterStrings(tempInput, filterlinks);
            return tempInput;
        }

    }
}
