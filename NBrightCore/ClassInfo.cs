using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Runtime.Serialization.Json;


namespace NBrightCore
{
    public class CheckBoxStats
    {
        #region "Constructors"

        #endregion

        #region "Public Properties"

        public int Count { get; set; }

        public int CheckedCount { get; set; }

        public int UnCheckedCount { get; set; }

        public int PercentChecked { get; set; }

        #endregion
    }


    public class NBrightTextBox
    {
        private string _BackColor = "";
        private string _BorderColor = "";
        private BorderStyle _BorderStyle = BorderStyle.NotSet;
        private string _BorderWidth = "";
        private bool _CausesValidation = true;
        private string _CssClass = "";
        private bool _Enabled = true;
        private string _ForeColor = "";
        private string _Height = "";
        private string _ID = "";
        private string _Text = "";
        private TextBoxMode _TextMode = TextBoxMode.SingleLine;
        private string _ToolTip = "";

        private bool _Visible = true;
        private string _Width = "";
        private bool _Wrap = true;

        #region "Constructors"

        #endregion

        #region "Public Properties"

        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        public bool CausesValidation
        {
            get { return _CausesValidation; }
            set { _CausesValidation = value; }
        }

        public int Columns { get; set; }

        public int MaxLength { get; set; }

        public TextBoxMode TextMode
        {
            get { return _TextMode; }
            set { _TextMode = value; }
        }

        public int Rows { get; set; }

        public bool Wrap
        {
            get { return _Wrap; }
            set { _Wrap = value; }
        }

        public string BackColor
        {
            get { return _BackColor; }
            set { _BackColor = value; }
        }

        public string BorderWidth
        {
            get { return _BorderWidth; }
            set { _BorderWidth = value; }
        }

        public string BorderColor
        {
            get { return _BorderColor; }
            set { _BorderColor = value; }
        }

        public BorderStyle BorderStyle
        {
            get { return _BorderStyle; }
            set { _BorderStyle = value; }
        }

        public string CssClass
        {
            get { return _CssClass; }
            set { _CssClass = value; }
        }

        public bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; }
        }

        public string ForeColor
        {
            get { return _ForeColor; }
            set { _ForeColor = value; }
        }

        public string Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        public int TabIndex { get; set; }

        public string ToolTip
        {
            get { return _ToolTip; }
            set { _ToolTip = value; }
        }

        public string Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        public bool Visible
        {
            get { return _Visible; }
            set { _Visible = value; }
        }

        #endregion
    }


    public class NBrightDropDownList
    {
        private string _BackColor = "";
        private string _BorderColor = "";
        private BorderStyle _BorderStyle = BorderStyle.NotSet;
        private string _BorderWidth = "";
        private bool _CausesValidation = true;
        private string _CssClass = "";
        private string _Data = "";

        private string _DataValue = "";
        private bool _Enabled = true;
        private string _ForeColor = "";
        private string _Height = "";
        private string _ID = "";
        private int _SelectedIndex = -1;
        private string _SelectedValue = "";
        private string _Text = "";
        private string _ToolTip = "";
        private bool _Visible = true;
        private string _Width = "";

        #region "Constructors"

        #endregion

        #region "Public Properties"

        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public string BorderColor
        {
            get { return _BorderColor; }
            set { _BorderColor = value; }
        }

        public BorderStyle BorderStyle
        {
            get { return _BorderStyle; }
            set { _BorderStyle = value; }
        }

        public string BorderWidth
        {
            get { return _BorderWidth; }
            set { _BorderWidth = value; }
        }

        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set { _SelectedIndex = value; }
        }

        public bool CausesValidation
        {
            get { return _CausesValidation; }
            set { _CausesValidation = value; }
        }

        public string SelectedValue
        {
            get { return _SelectedValue; }
            set { _SelectedValue = value; }
        }

        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        public string BackColor
        {
            get { return _BackColor; }
            set { _BackColor = value; }
        }

        public string CssClass
        {
            get { return _CssClass; }
            set { _CssClass = value; }
        }

        public bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; }
        }

        public string ForeColor
        {
            get { return _ForeColor; }
            set { _ForeColor = value; }
        }

        public string Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        public int TabIndex { get; set; }

        public string ToolTip
        {
            get { return _ToolTip; }
            set { _ToolTip = value; }
        }

        public string Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        public bool Visible
        {
            get { return _Visible; }
            set { _Visible = value; }
        }

        public string data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        public string datavalue
        {
            get { return _DataValue; }
            set { _DataValue = value; }
        }

        #endregion
    }


    public class NBrightCheckBox
    {
        private string _BackColor = "";
        private string _BorderColor = "";
        private BorderStyle _BorderStyle = BorderStyle.NotSet;
        private string _BorderWidth = "";
        private bool _CausesValidation = true;
        private string _CssClass = "";
        private bool _Enabled = true;
        private string _ForeColor = "";
        private string _Height = "";
        private string _ID = "";
        private string _Text = "";
        private TextAlign _TextAlign = TextAlign.Left;
        private string _ToolTip = "";

        private bool _Visible = true;
        private string _Width = "";

        #region "Constructors"

        #endregion

        #region "Public Properties"

        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public bool CausesValidation
        {
            get { return _CausesValidation; }
            set { _CausesValidation = value; }
        }

        public bool Checked { get; set; }

        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        public TextAlign TextAlign
        {
            get { return _TextAlign; }
            set { _TextAlign = value; }
        }

        public string BackColor
        {
            get { return _BackColor; }
            set { _BackColor = value; }
        }

        public string BorderColor
        {
            get { return _BorderColor; }
            set { _BorderColor = value; }
        }

        public string BorderWidth
        {
            get { return _BorderWidth; }
            set { _BorderWidth = value; }
        }

        public BorderStyle BorderStyle
        {
            get { return _BorderStyle; }
            set { _BorderStyle = value; }
        }

        public string CssClass
        {
            get { return _CssClass; }
            set { _CssClass = value; }
        }

        public bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; }
        }

        public string ForeColor
        {
            get { return _ForeColor; }
            set { _ForeColor = value; }
        }

        public string Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        public int TabIndex { get; set; }

        public string ToolTip
        {
            get { return _ToolTip; }
            set { _ToolTip = value; }
        }

        public string Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        public bool Visible
        {
            get { return _Visible; }
            set { _Visible = value; }
        }

        #endregion
    }


    public class NBrightRadioButtonList
    {
        private string _BackColor = "";
        private string _BorderColor = "";
        private BorderStyle _BorderStyle = BorderStyle.NotSet;
        private string _BorderWidth = "";
        private bool _CausesValidation = true;
        private int _CellPadding = -1;
        private int _CellSpacing = -1;
        private string _CssClass = "";
        private string _Data = "";

        private string _DataValue = "";
        private bool _Enabled = true;
        private string _ForeColor = "";
        private string _Height = "";
        private string _ID = "";
        private RepeatDirection _RepeatDirection = RepeatDirection.Vertical;
        private RepeatLayout _RepeatLayout = RepeatLayout.Table;
        private int _SelectedIndex = -1;
        private string _SelectedValue = "";
        private string _Text = "";
        private TextAlign _TextAlign = TextAlign.Left;
        private string _ToolTip = "";
        private bool _Visible = true;
        private string _Width = "";

        #region "Constructors"

        #endregion

        #region "Public Properties"

        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public int CellPadding
        {
            get { return _CellPadding; }
            set { _CellPadding = value; }
        }

        public int CellSpacing
        {
            get { return _CellSpacing; }
            set { _CellSpacing = value; }
        }

        public int RepeatColumns { get; set; }

        public RepeatDirection RepeatDirection
        {
            get { return _RepeatDirection; }
            set { _RepeatDirection = value; }
        }

        public RepeatLayout RepeatLayout
        {
            get { return _RepeatLayout; }
            set { _RepeatLayout = value; }
        }

        public TextAlign TextAlign
        {
            get { return _TextAlign; }
            set { _TextAlign = value; }
        }

        public bool CausesValidation
        {
            get { return _CausesValidation; }
            set { _CausesValidation = value; }
        }

        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set { _SelectedIndex = value; }
        }

        public string SelectedValue
        {
            get { return _SelectedValue; }
            set { _SelectedValue = value; }
        }

        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        public string BackColor
        {
            get { return _BackColor; }
            set { _BackColor = value; }
        }

        public string BorderColor
        {
            get { return _BorderColor; }
            set { _BorderColor = value; }
        }

        public string BorderWidth
        {
            get { return _BorderWidth; }
            set { _BorderWidth = value; }
        }

        public BorderStyle BorderStyle
        {
            get { return _BorderStyle; }
            set { _BorderStyle = value; }
        }

        public string CssClass
        {
            get { return _CssClass; }
            set { _CssClass = value; }
        }

        public bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; }
        }

        public string ForeColor
        {
            get { return _ForeColor; }
            set { _ForeColor = value; }
        }

        public string Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        public int TabIndex { get; set; }

        public string ToolTip
        {
            get { return _ToolTip; }
            set { _ToolTip = value; }
        }

        public string Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        public bool Visible
        {
            get { return _Visible; }
            set { _Visible = value; }
        }

        public string data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        public string datavalue
        {
            get { return _DataValue; }
            set { _DataValue = value; }
        }

        #endregion
    }


    public class NBrightDateEditControl
    {
        private string _BackColor = "";
        private string _BorderColor = "";
        private BorderStyle _BorderStyle = BorderStyle.NotSet;
        private string _BorderWidth = "";
        private bool _CausesValidation = true;
        private string _CssClass = "";
        private bool _Enabled = true;
        private string _ForeColor = "";
        private string _Height = "";
        private string _ID = "";
        private string _Text = "";
        private TextBoxMode _TextMode = TextBoxMode.SingleLine;
        private string _ToolTip = "";

        private bool _Visible = true;
        private string _Width = "";
        private bool _Wrap = true;

        #region "Constructors"

        #endregion

        #region "Public Properties"

        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        public bool CausesValidation
        {
            get { return _CausesValidation; }
            set { _CausesValidation = value; }
        }

        public int Columns { get; set; }

        public int MaxLength { get; set; }

        public TextBoxMode TextMode
        {
            get { return _TextMode; }
            set { _TextMode = value; }
        }

        public int Rows { get; set; }

        public bool Wrap
        {
            get { return _Wrap; }
            set { _Wrap = value; }
        }

        public string BackColor
        {
            get { return _BackColor; }
            set { _BackColor = value; }
        }

        public string BorderWidth
        {
            get { return _BorderWidth; }
            set { _BorderWidth = value; }
        }

        public string BorderColor
        {
            get { return _BorderColor; }
            set { _BorderColor = value; }
        }

        public BorderStyle BorderStyle
        {
            get { return _BorderStyle; }
            set { _BorderStyle = value; }
        }

        public string CssClass
        {
            get { return _CssClass; }
            set { _CssClass = value; }
        }

        public bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; }
        }

        public string ForeColor
        {
            get { return _ForeColor; }
            set { _ForeColor = value; }
        }

        public string Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        public int TabIndex { get; set; }

        public string ToolTip
        {
            get { return _ToolTip; }
            set { _ToolTip = value; }
        }

        public string Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        public bool Visible
        {
            get { return _Visible; }
            set { _Visible = value; }
        }

        #endregion
    }

        public class AdmAccessToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }
    }
    
    public class AdmAuthentication
    {
        public static readonly string DatamarketAccessUri = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
        private string clientId;
        private string cientSecret;
        private string request;

        public AdmAuthentication(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.cientSecret = clientSecret;
            //If clientid or client secret has special characters, encode before sending request
            this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(clientSecret));
        }

        public AdmAccessToken GetAccessToken()
        {
            return HttpPost(DatamarketAccessUri, this.request);
        }

        private AdmAccessToken HttpPost(string DatamarketAccessUri, string requestDetails)
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
    }

    /// <summary>
    /// Class to store NBright Settings data
    /// </summary>
    public class NBrightSetting
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }



}