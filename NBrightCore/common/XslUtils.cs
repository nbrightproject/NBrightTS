using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace NBrightCorev2.common
{
    public class XslUtils
    {

        #region "XSL translation"

        public static string XslTrans(string xmlData, string xslFilePath)
        {

            try
            {
                var xmlDoc = new XmlDocument();

                xmlDoc.LoadXml(xmlData);

                var xslt = new System.Xml.Xsl.XslCompiledTransform();

                xslt.Load(xslFilePath);

                var myWriter = new System.IO.StringWriter();
                xslt.Transform(xmlDoc, null, myWriter);

                return myWriter.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static string XslTransInMemory(string xmlData, string xslData)
        {
            return XslTransInMemory(xmlData,xslData,null);
        }

        public static string XslTransInMemory(string xmlData, string xslData, XsltArgumentList argslist)
        {
            try
            {

                var xmlDoc = new XmlDocument();

                xmlDoc.LoadXml(xmlData);

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(xslData);
                var xslStream = new System.IO.MemoryStream(bytes) {Position = 0};

                XmlReader xslStylesheet = default(System.Xml.XmlReader);
                xslStylesheet = new System.Xml.XmlTextReader(xslStream);

                var xslt = new System.Xml.Xsl.XslCompiledTransform();

                var settings = new System.Xml.Xsl.XsltSettings {EnableDocumentFunction = true};

                xslt.Load(xslStylesheet, settings, null);

                var myWriter = new System.IO.StringWriter();
                xslt.Transform(xmlDoc, argslist, myWriter);

                return myWriter.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString() + " .............. " + xslData;
            }
        }

        #endregion


    }

}
