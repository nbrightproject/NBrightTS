using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace NBrightCore.common
{

    public class XmlUtils
    {
        public static string ToXML(object obj)
        {

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var writerSettings = new XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = true;
            var stringWriter = new StringWriter();
            using (var xmlWriter = XmlWriter.Create(stringWriter, writerSettings))
            {
                var x = new XmlSerializer(obj.GetType(), "");
                x.Serialize(xmlWriter, obj,ns);
            }
            return stringWriter.ToString();

        }
    }
}
