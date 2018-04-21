using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.UI.WebControls;

namespace NBrightDNNv2.render
{
    public class GenDateControl : DateEditControl 
    {
        public TextBox DateField { get; private set; }
        public HyperLink LinkCalendar{ get; set; }

        public GenDateControl()
        {
            DateField = new TextBox();
            LinkCalendar = new HyperLink();
        }

        public string Text
        {
            get { return DateField.Text; }
            set { DateField.Text = value; }
        }
      
        protected override void CreateChildControls()
        {
            DateField.ControlStyle.CopyFrom(ControlStyle);
            DateField.ID = ID + "date";
            Controls.Add(DateField);

            Controls.Add(new LiteralControl("&nbsp;"));

            LinkCalendar.CssClass = "CommandButton";
            LinkCalendar.Text = "<img src=\"" + Globals.ApplicationPath + "/images/calendar.png\" border=\"0\" />";
            LinkCalendar.NavigateUrl = DotNetNuke.Common.Utilities.Calendar.InvokePopupCal(DateField);
            Controls.Add(LinkCalendar);
        }

    }
}
