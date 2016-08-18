using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace NBrightCore.providers
{
    public abstract class GenXProvider : ProviderBase
    {

        public abstract bool CreateGenControl(string ctrltype, Control container, XmlNode xmlNod, string rootname = "genxml", string databindColum = "XMLData", string cultureCode = "", Dictionary<string, string> settings = null, ConcurrentStack<Boolean> visibleStatusIn = null);

        public abstract string GetField(Control ctrl);

        public abstract void SetField(Control ctrl, string newValue);

        public abstract string GetGenXml(List<Control> genCtrls, XmlDocument xmlDoc, string originalXml, string folderMapPath, string xmlRootName = "genxml");

        public abstract string GetGenXmlTextBox(List<Control> genCtrls, XmlDocument xmlDoc, string originalXml, string folderMapPath, string xmlRootName = "genxml");

        public abstract object PopulateGenObject(List<Control> genCtrls, object obj);

        /// <summary>
        /// Allow the provider to do specific testing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns>TestOfData</returns>
        public abstract TestOfData TestOfDataBinding(object sender, EventArgs e);

        /// <summary>
        /// Allow a proivider to accept and process command buttons
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public abstract void CtrlItemCommand(object source, RepeaterCommandEventArgs e);

    }

    /// <summary>
    /// Class to hold testof values
    /// </summary>
    public class TestOfData
    {
        public String DataValue { get; set; }
        /// <summary>
        /// Test value of token. (on return from provider an NULL value will be ignored and the token testvalue used)
        /// </summary>
        public String TestValue { get; set; }
    }

}
