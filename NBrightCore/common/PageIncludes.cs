using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace NBrightCorev2.common
{
    public class PageIncludes
    {

        public static void IncludeScripts(int portalId, string modulePath, Page page, string jsIncludeList, string cssIncludeList)
        {

            //custom js includes
            var jsList = jsIncludeList.Split(',');
            foreach (var j in jsList)
            {
                if (!string.IsNullOrEmpty(j))
                {
                    if (j.StartsWith(@"/"))
                    {
                        RegisterJs("GenCSS" + j, j, modulePath, page);
                    }
                    else
                    {
                        char[] charsToTrim = { '/', '\\', '.', ' ' };
                        RegisterJs("GenCSS" + j, j, modulePath.TrimEnd(charsToTrim), page);
                    }

                }
            }

            //custom css
            var cssList = cssIncludeList.Split(',');
            foreach (var c in cssList)
            {
                if (!string.IsNullOrEmpty(c))
                {
                    if (c.StartsWith(@"/"))
                    {
                        IncludeCssFile(page, "GenCSS" + c, c);
                    }
                    else
                    {
                        char[] charsToTrim = {'/', '\\', '.', ' '};
                        IncludeCssFile(page, "GenCSS" + c, modulePath.TrimEnd(charsToTrim) + @"/css/" + c);
                    }
                }
            }

        }

        public static void RegisterJs(string regName, string jsFileName, string modulePath, Page page)
        {
            if (!string.IsNullOrEmpty(jsFileName))
            {
                if (!page.ClientScript.IsClientScriptIncludeRegistered(regName))
                {
                    if (jsFileName.StartsWith(@"/"))
                    {
                        IncludeJsFile(page,regName,jsFileName);
                    }
                    else
                    {
                        char[] charsToTrim = {'/', '\\', '.', ' '};
                        IncludeJsFile(page, regName, modulePath.TrimEnd(charsToTrim) + @"/js/" + jsFileName);
                    }
                }
            }
        }

        public static void LoadJQueryCode(string jCodeKey, string jCode, Page page)
        {
            if (!string.IsNullOrEmpty(jCode))
            {
                if (!jCode.ToLower().StartsWith("<script"))
                {
                    jCode = System.Web.HttpUtility.HtmlDecode(jCode);
                    jCode = "<script language=\"javascript\" type=\"text/javascript\">" + jCode + "</script>";
                }

                page.ClientScript.RegisterStartupScript(page.GetType(), jCodeKey, jCode);
            }
        }

        public static void IncludeJsFile(Page page, string id, string href)
        {
            if (!string.IsNullOrEmpty(href))
            {
                if (!page.ClientScript.IsClientScriptIncludeRegistered(id))
                {
                    page.ClientScript.RegisterClientScriptInclude(id, href);
                }
            }
        }


        public static void IncludeCssFile(Page page, string id, string href)
        {
            if (!string.IsNullOrEmpty(href))
            {
                string strId = id.Replace(@"/", "");
                var cssLink = (HtmlLink) page.Header.FindControl(strId);
                if (cssLink == null)
                {
                    cssLink = new HtmlLink {ID = strId};
                    cssLink.Attributes.Add("rel", "stylesheet");
                    cssLink.Attributes.Add("type", "text/css");
                    cssLink.Href = href;
                    page.Header.Controls.Add(cssLink);
                }
            }
        }


        public static void IncludeCanonicalLink(Page page, string href)
        {
            if (!string.IsNullOrEmpty(href))
            {
                var cLink = new HtmlLink();
                cLink.Attributes.Add("rel", "canonical");
                cLink.Href = href;
                page.Header.Controls.Add(cLink);
            }
        }

        public static void IncludeTextInHeader(Page page, string TextToInclude)
        {
            if (TextToInclude != "")
                page.Header.Controls.Add(new LiteralControl(TextToInclude));
        }
    }
}
