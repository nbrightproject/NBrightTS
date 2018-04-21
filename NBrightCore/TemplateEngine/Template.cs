using System;
using System.IO;

namespace NBrightCorev2.TemplateEngine
{

    public class Template
    {
        public string FolderPath { get; set; }
        public string TemplateName { get; set; }
        public string TemplateData { get; private set; }
        public bool IsTemplateFound { get; private set; }

        private readonly string _templatePath;
       
        // Constructor
        public Template(string templatepath)
        {
            TemplateData = "";
            IsTemplateFound = false;
            _templatePath = templatepath;
            if (!String.IsNullOrEmpty(templatepath))
            {
                char[] charsToTrim = {'\\', '.', ' '};
                TemplateName = Path.GetFileName(templatepath.TrimEnd(charsToTrim).TrimStart(charsToTrim));
                if (TemplateName != null)
                {
                    FolderPath = templatepath.TrimEnd(charsToTrim).TrimStart(charsToTrim).Replace(TemplateName, "");
                }
                Load();
            }
        }

        public Template(string folderpath, string templatename)
        {
            IsTemplateFound = false;
            char[] charsToTrim = { '\\', '.', ' ' };
            FolderPath = folderpath.TrimEnd(charsToTrim).TrimStart(charsToTrim);
            TemplateName = templatename.TrimEnd(charsToTrim).TrimStart(charsToTrim);
            if ((FolderPath != "" && TemplateName != ""))
            {
                    _templatePath = FolderPath + "\\" + TemplateName;
            }
            else
            {
                _templatePath = "";
            }
            TemplateData = "NO TEMPLATE DATA";
            Load();

        }

        public string Load()
        {

            if (_templatePath != "")
            {
                try
                {
                    TemplateData = "";

                    if (File.Exists(_templatePath))
                    {
                        string inputLine;
                        var inputStream = new FileStream(_templatePath, FileMode.Open, FileAccess.Read);
                        var streamReader = new StreamReader(inputStream);

                        while ((inputLine = streamReader.ReadLine()) != null)
                        {
                            TemplateData += inputLine + Environment.NewLine;
                        }
                        streamReader.Close();
                        inputStream.Close();

                        if (TemplateData.Contains("**CDATASTART**"))
                        {
                            //convert back cdata marks converted so it saves OK into XML 
                            TemplateData = TemplateData.Replace("**CDATASTART**", "<![CDATA[");
                            TemplateData = TemplateData.Replace("**CDATAEND**", "]]>");
                        }
                        IsTemplateFound = true;
                    }
                    else
                    {
                        TemplateData = "";
                    }
                }
                catch (Exception)
                {
                    TemplateData = string.Format("ERROR ON TEMPLATE READ ({0})", TemplateName);
                }
            
            }

            return TemplateData;
        }

        public void Save(string templatedata)
        {
            if (_templatePath != "")
            {
                Delete(); // Delete the existing file.

                TemplateData = templatedata;

                var outputStream = new FileStream(_templatePath, FileMode.OpenOrCreate, FileAccess.Write);
                var streamWriter = new StreamWriter(outputStream);

                if (TemplateData.Contains("**CDATASTART**"))
                {
                    //convert back cdata marks converted so it saves OK into XML 
                    TemplateData = TemplateData.Replace("**CDATASTART**", "<![CDATA[");
                    TemplateData = TemplateData.Replace("**CDATAEND**", "]]>");
                }

                streamWriter.Write(TemplateData);

                streamWriter.Close();
                outputStream.Close();
            }
        }

        public void Delete()
        {
            if (_templatePath != "")
            {
                if (File.Exists(_templatePath))
                {
                    File.Delete(_templatePath);
                }
            }
        }

        public Boolean Exists()
        {
            if (_templatePath == "")
            {
                return false;
            }
            if (File.Exists(_templatePath))
            {
                return true;
            }
            return false;
        }

    }

    public class TemplateMetaData
    {
        public string FolderPath { get; set; }
        public string TemplateName { get; set; }
        public string Lang { get; set; }
        public string ThemeFolderPath { get; set; }
        public string FullFolderPath { get; set; }
        public string Roles { get; set; }
    }

}
