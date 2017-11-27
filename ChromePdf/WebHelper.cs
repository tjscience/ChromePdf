using HtmlAgilityPack;
using System;
using System.Net;

namespace ChromePdf
{
    public static class WebHelper
    {
        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.59 Safari/537.36";

        public static string DownloadString(string url)
        {
            var web = new HtmlWeb();
            web.UserAgent = WebHelper.UserAgent;
            var document = web.Load(url);
            document.OptionFixNestedTags = true;
            document.OptionCheckSyntax = true;
            document.OptionWriteEmptyNodes = true;
            document.OptionOutputAsXml = true;
            return document.DocumentNode.OuterHtml;

        }
    }
}
