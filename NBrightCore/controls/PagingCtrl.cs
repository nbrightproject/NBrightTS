
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using NBrightCorev2.render;

namespace NBrightCorev2.controls 
{
    public class PagingCtrl:  WebControl
    {

        #region "setup"


        protected Repeater RpData;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public string CssPagingDiv { get; set; }
        public string CssPositionDiv { get; set; }
        public string CssSelectedPage { get; set; }
        public string CssNormalPage { get; set; }
        public string CssFirstPage { get; set; }
        public string CssLastPage { get; set; }
        public string CssPrevPage { get; set; }
        public string CssNextPage { get; set; }
        public string CssPrevSection { get; set; }
        public string CssNextSection { get; set; }
        public string TextFirst { get; set; }
        public string TextLast { get; set; }
        public string TextPrev { get; set; }
        public string TextNext { get; set; }
        public string TextPrevSection { get; set; }
        public string TextNextSection { get; set; }

        private String _headerTemplate = "";
        private String _bodyTemplate = "";
        private String _footerTemplate = "";

        /// <summary>
        /// If set to true the new list format will be used, otherwise the legacy span system will be use. 
        /// </summary>
        public Boolean UseListDisplay { get; set; }


        //
        /// <summary>
        /// Use a html href link link for the paging buttons, this is so SEO robots can follow them easily (Only needed for Front Office Display)
        /// </summary>
        public bool UseHrefLink
        {
            set
            {
                if (value)
                {
                    //SEO page link
                    var modparam = "";
                    if (ModuleId != "") modparam = "&pagemid=" + ModuleId;
                    RpData.ItemTemplate = new GenXmlTemplate("[<tag type='valueof' databind='PreText' />]<a href=\"?page=[<tag type='valueof' databind='PageNumber' />]" + modparam + "\">[<tag type='valueof' databind='Text' />]</a>[<tag type='valueof' databind='PostText' />]");
                }
            }
        }
        /// <summary>
        /// Allows client application to specify a friendly url tmeplate for the paging.
        /// This template token will need to be created in the consumer application token system "e.g. GenXmlTemplateExt.cs class in the case of NBrightStore"
        /// </summary>
        public String HrefLinkTemplate
        {
            set
            {
                RpData.ItemTemplate = new GenXmlTemplate(value);
            }
        }

        /// <summary>
        /// Use to add a moduleid onto the pg param, so multiple modules can use paging on 1 page.
        /// </summary>
        public String ModuleId { get; set; }

        public PagingCtrl()
        {
            UseHrefLink = false;
            UseListDisplay = false;
            ModuleId = "";
            CurrentPage = 1;
            PageSize = 10;
            TotalRecords = 0;
            CssPagingDiv = "NBrightPagingDiv";
            CssPositionDiv = "NBrightPositionPgDiv";
            CssSelectedPage = "NBrightSelectPg";
            CssNormalPage = "NBrightNormalPg";
            CssFirstPage = "NBrightFirstPg";
            CssLastPage = "NBrightLastPg";
            CssPrevPage = "NBrightPrevPg";
            CssNextPage = "NBrightNextPg";
            CssPrevSection = "NBrightPrevSection";
            CssNextSection = "NBrightNextSection";            
            TextFirst = "<<";
            TextLast = ">>";
            TextPrev = "<";
            TextNext = ">";
            TextNext = ">";
            TextPrevSection = "...";
            TextNextSection = "...";

            // set the for default <span> diplay
            _headerTemplate = "<div class='" + CssPagingDiv + "'>";
            _bodyTemplate = "[<tag type='valueof' databind='PreText' />][<tag type='if' databind='Text' testvalue='' display='{OFF}' />][<tag id='cmdPg' type='linkbutton' Text='databind:Text' commandname='Page' commandargument='PageNumber' />][<tag type='endif' />][<tag type='valueof' databind='PostText' />]";
            _footerTemplate = "</div>";
        }

        #endregion

        #region "events"


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            RpData = new Repeater();
            RpData.ItemCommand += new RepeaterCommandEventHandler(ClientItemCommand);
            RpData.ItemTemplate = new GenXmlTemplate(_bodyTemplate);

            this.Controls.AddAt(0, new LiteralControl(_footerTemplate));
            this.Controls.AddAt(0, RpData);
            this.Controls.AddAt(0, new LiteralControl(_headerTemplate));
        }

        public event RepeaterCommandEventHandler PageChanged;

        protected void ClientItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();

            switch (e.CommandName.ToLower())
            {
                case "page":
                    if (PageChanged != null)
                    {
                        PageChanged(this, e);                        
                    }
                    break;
            }

        }

        #endregion


        #region "methods"

        public void BindPageLinks()
        {

            var lastPage = Convert.ToInt32(TotalRecords / PageSize);
            if (TotalRecords != (lastPage * PageSize)) lastPage = lastPage + 1;
            if (lastPage == 1) return;            //if only one page, don;t process

            var pageL = new List<NBrightPaging>();
            
            if (CurrentPage <= 0) CurrentPage = 1;
            NBrightPaging p;
            const int pageLinksPerPage = 10;
            var rangebase = Convert.ToInt32((CurrentPage - 1)/pageLinksPerPage);
            var lowNum = (rangebase * pageLinksPerPage) + 1;
            var highNum = lowNum + (pageLinksPerPage -1);
            if (highNum > Convert.ToInt32(lastPage)) highNum = Convert.ToInt32(lastPage);
            if (lowNum < 1) lowNum = 1;

            var listtype = "span";
            if (UseListDisplay) listtype = "li";


                #region "header"

            if (UseListDisplay)
            {
                p = new NBrightPaging { PageNumber = "", PreText = "<ul>", Text = "", PostText = "" };
                pageL.Add(p);                
            }

                if ((lowNum != 1) && (CurrentPage > 1) && (TextFirst != ""))
                {
                    p = new NBrightPaging { PageNumber = "1", PreText = "<" + listtype + " class='" + CssFirstPage + "'>", Text = TextFirst , PostText = "</" + listtype + ">" };
                    pageL.Add(p);
                }

                if ((CurrentPage > 1) && (TextPrev != ""))
                {
                    p = new NBrightPaging { PageNumber = Convert.ToString(CurrentPage - 1), PreText = "<" + listtype + " class='" + CssPrevPage + "'>", Text = TextPrev, PostText = "</" + listtype + ">" };
                    pageL.Add(p);
                }

                if ((lowNum > 1) && (TextPrevSection != ""))
                {
                    p = new NBrightPaging { PageNumber = Convert.ToString(lowNum - 1), PreText = "<" + listtype + " class='" + CssPrevSection + "'>", Text = TextPrevSection, PostText = "</" + listtype + ">" };
                    pageL.Add(p);
                }

                #endregion

                #region "body"
                for (int i = lowNum; i <= highNum; i++)
                {

                    if (i == CurrentPage)
                    {
                        p = new NBrightPaging { PageNumber = Convert.ToString(i), PreText = "<" + listtype + " class='" + CssSelectedPage + "'>" , Text = Convert.ToString(i), PostText = "</" + listtype + ">" };
                    }
                    else
                    {
                        p = new NBrightPaging { PageNumber = Convert.ToString(i), PreText = "<" + listtype + " class='" + CssNormalPage + "'>" , Text =  Convert.ToString(i) , PostText = "</" + listtype + ">" };
                    }
                    pageL.Add(p);

                }

                #endregion

                #region "footer"
                if ((lastPage > highNum) && (TextNextSection != ""))
                {
                    p = new NBrightPaging { PageNumber = Convert.ToString(highNum + 1), PreText = "<" + listtype + " class='" + CssNextSection + "'>", Text = TextNextSection, PostText = "</" + listtype + ">" };
                    pageL.Add(p);
                }


                if ((lastPage > CurrentPage) && (TextNext != ""))
                {
                    p = new NBrightPaging { PageNumber = Convert.ToString(CurrentPage + 1), PreText = "<" + listtype + " class='" + CssNextPage + "'>", Text = TextNext, PostText = "</" + listtype + ">" };
                    pageL.Add(p);
                }

                if ((lastPage != highNum) && (lastPage > CurrentPage) && (TextLast != ""))
                {
                    p = new NBrightPaging { PageNumber = Convert.ToString(lastPage), PreText = "<" + listtype + " class='" + CssLastPage + "'>", Text = TextLast, PostText = "</" + listtype + ">" };
                    pageL.Add(p);
                }

                if (UseListDisplay)
                {
                    p = new NBrightPaging { PageNumber = "", PreText = "", Text = "", PostText = "</ul>" };
                    pageL.Add(p);
                }

                #endregion

            RpData.DataSource = pageL;
            RpData.DataBind();


        }

        public String RenderPager(int recordCount, int pageSize,int pageNumber)
        {
            UseListDisplay = true;
            TotalRecords = recordCount;
            PageSize = pageSize;
            CurrentPage = pageNumber;
    
            //redefine template for ajax processing
            _bodyTemplate = "[<tag type='valueof' databind='PreText' />]<a class='cmdPg' pagenumber='[<tag type='valueof' databind='PageNUmber' />]'>[<tag type='valueof' databind='Text' />]</a>[<tag type='valueof' databind='PostText' />]";

            var e = new EventArgs();
            OnInit(e);

            BindPageLinks();

            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            var htmlTw = new HtmlTextWriter(sw);
            RenderControl(htmlTw);

            return sb.ToString();

        }

        #endregion


        #region "data classes"

        private class NBrightPaging
        {
            public string PageNumber { get; set; }
            public string PreText { get; set; }
            public string Text { get; set; }
            public string PostText { get; set; }

        }

        #endregion

    }
}
