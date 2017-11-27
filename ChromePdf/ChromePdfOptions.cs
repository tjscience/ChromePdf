using CefSharp;
using CommandLine;

namespace ChromePdf
{
    public class ChromePdfOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input URI (required).")]
        public string InputUri { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file path (required).")]
        public string Output { get; set; }

        [Option('l', "landscape", DefaultValue = false, HelpText = "Specifies that the PDF should be rendered in landscape orientation.")]
        public bool IsLandscape { get; set; }

        [Option('b', "enable-backgrounds", DefaultValue = false, HelpText = "Specifies that background graphics should be rendered in the PDF.")]
        public bool EnableBackgrounds { get; set; }

        [Option('n', "timeout", DefaultValue = 0, HelpText = "Specifies a timeout (milliseconds) in which to kill the process if it has not exited successfully (0 = no timeout).")]
        public long Timeout { get; set; }

        [Option('d', "delay", DefaultValue = 0, HelpText = "Specifies a delay (milliseconds) in which to wait for the URI to load before processing (0 = no delay; This is independent on the time it takes the page to load in the browser.).")]
        public int Delay { get; set; }

        [Option('m', "margin-type", DefaultValue = CefPdfPrintMarginType.Default, HelpText = "Specifies the margin for the PDF (available values: Custom, Default, Minimum, None).")]
        public CefPdfPrintMarginType MarginType { get; set; }

        [Option('h', "enable-header-footer", DefaultValue = false, HelpText = "Specifies that the header/footer should be rendered in the PDF.")]
        public bool EnableHeaderFooter { get; set; }

        [Option('t', "title", HelpText = "A title to be rendered in the PDF header (only used if enable-header-footer present).")]
        public string Title { get; set; }

        [Option('f', "footer-text", HelpText = "Text to be rendered in the PDF footer (only used if enable-header-footer present).")]
        public string Footer { get; set; }

        [Option("margin-bottom", DefaultValue = 0, HelpText = "Bottom margin (only used if margin-type is set to custom).")]
        public double MarginBottom { get; set; }

        [Option("margin-left", DefaultValue = 0, HelpText = "Left margin (only used if margin-type is set to custom).")]
        public double MarginLeft { get; set; }

        [Option("margin-right", DefaultValue = 0, HelpText = "Right margin (only used if margin-type is set to custom).")]
        public double MarginRight { get; set; }

        [Option("margin-top", DefaultValue = 0, HelpText = "Top margin (only used if margin-type is set to custom).")]
        public double MarginTop { get; set; }

        [Option("page-height", DefaultValue = 0, HelpText = "Page height in microns (if 0 paper size is set to A4).")]
        public int PageHeight { get; set; }

        [Option("page-width", DefaultValue = 0, HelpText = "Page width in microns (if 0 paper size is set to A4).")]
        public int PageWidth { get; set; }

        public ChromePdfOptions(string[] args)
        {
            var isValid = Parser.Default.ParseArgumentsStrict(args, this);
        }
    }
}
