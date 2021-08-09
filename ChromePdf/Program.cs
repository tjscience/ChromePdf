using CefSharp;
using CefSharp.OffScreen;
using Ghostscript.NET.Rasterizer;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ChromePdf
{
    public class Program
    {
        static ChromiumWebBrowser browser;
        static Task waitTask;
        static CancellationTokenSource tokenSource;
        static CancellationToken token;
        static string html;
        static ChromePdfOptions options;
        static bool manualConvert = false;
        static bool orientationInScript = false;
        static bool isLandscape = false;
        static Stopwatch watch;
        static string domain;
        static Uri inputUri;

        static void Main(string[] args)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            options = new ChromePdfOptions(args);

            inputUri = new Uri(options.InputUri);

            CefSettings settings = new CefSettings
            {
                CachePath = string.Empty
            };

            settings.SetOffScreenRenderingBestPerformanceArgs();
            settings.CefCommandLineArgs.Add("disable-application-cache", "1");
            settings.CefCommandLineArgs.Add("disable-session-storage", "1");
            settings.LogSeverity = (LogSeverity)Enum.Parse(typeof(LogSeverity), ChromePdf.Properties.Settings.Default.LogSeverity, true);

            Cef.Initialize(settings);
            Cef.EnableHighDPISupport();
            Cef.GetGlobalCookieManager().DeleteCookies();

            watch = new Stopwatch();

            if (inputUri.IsFile)
            {
                domain = "http://localhost:9222";
                // This is an html file so we need to get the html and manually load it into the browser.
                html = File.ReadAllText(inputUri.LocalPath);
            }
            else
            {
                domain = inputUri.AbsoluteUri;
                html = WebHelper.DownloadString(inputUri.OriginalString);
            }

            if (html.IndexOf("chromePdf.convert()") > -1)
                manualConvert = true;

            if (html.IndexOf("chromePdf.setOrientation('landscape')", StringComparison.OrdinalIgnoreCase) > -1)
            {
                orientationInScript = true;
                isLandscape = true;
            }
            else if (html.IndexOf("chromePdf.setOrientation('portrait')", StringComparison.OrdinalIgnoreCase) > -1)
            {
                orientationInScript = true;
                isLandscape = false;
            }

            browser = new ChromiumWebBrowser();
            browser.RenderProcessMessageHandler = new RenderProcessMessageHandler();
            browser.BrowserInitialized += Browser_BrowserInitialized;
            browser.ConsoleMessage += Browser_ConsoleMessage;
            browser.FrameLoadEnd += Browser_FrameLoadEnd;

            waitTask = Task.Run(() =>
            {
                for (int i = 0; i <= int.MaxValue; i++)
                {
                    var sw = new SpinWait();

                    for (int j = 0; j <= 100; j++)
                        sw.SpinOnce();

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    else if (watch.IsRunning && watch.ElapsedMilliseconds >= options.Timeout)
                    {
                        tokenSource.Cancel();
                    }
                }
            }, token);

            try
            {
                waitTask.Wait();
            }
            catch { }

            browser.Dispose();

            Cef.Shutdown();

            Console.WriteLine("chromepdf finished.");

#if DEBUG
            Console.ReadKey();
#endif
        }

        private static void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            if (e.Message.StartsWith("chrome-pdf-error->"))
            {
                // Cancels process on any javascript error...
                //tokenSource.Cancel();
            }
            
            if (manualConvert && e.Message == "chrome-pdf->convert")
                PrintPdf();
        }

        private static void Browser_BrowserInitialized(object sender, EventArgs e)
        {
            if (inputUri.IsFile)
                browser.LoadHtml(html, domain);
            else
                browser.Load(inputUri.OriginalString);

            if (options.Timeout > 0)
                watch.Start();
        }

        private static void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (!manualConvert)
                PrintPdf();
        }

        private static void PrintPdf()
        {
            Task.Run(async () =>
            {
                var settings = new PdfPrintSettings
                {
                    BackgroundsEnabled = options.EnableBackgrounds,
                    Landscape = orientationInScript ? isLandscape : options.IsLandscape,
                    MarginType = options.MarginType,
                    HeaderFooterEnabled = options.EnableHeaderFooter,
                    HeaderFooterTitle = options.Title,
                    HeaderFooterUrl = options.Footer,
                    MarginBottom = options.MarginBottom,
                    MarginLeft = options.MarginLeft,
                    MarginRight = options.MarginRight,
                    MarginTop = options.MarginTop,
                    PageHeight = options.PageHeight,
                    PageWidth = options.PageWidth,

                };

                Thread.Sleep(options.Delay);

                await browser.PrintToPdfAsync(options.Output, settings);

                // Compress
                //byte[] inputFile = File.ReadAllBytes(options.Output);

                //Load a existing PDF document
                /*using (PdfLoadedDocument ldoc = new PdfLoadedDocument(inputFile))
                {

                    //Create a new PDF compression options
                    PdfCompressionOptions compressOptions = new PdfCompressionOptions();
                    compressOptions.CompressImages = true;
                    compressOptions.ImageQuality = 50;
                    compressOptions.OptimizeFont = true;
                    compressOptions.OptimizePageContents = true;
                    compressOptions.RemoveMetadata = true;
                    ldoc.CompressionOptions = compressOptions;
                    ldoc.FileStructure.IncrementalUpdate = false;

                    if (options.RemoveBlankPages)
                    {
                        int i = 0;

                        while (i < ldoc.Pages.Count)
                        {
                            //Extract image from the loaded page 
                            Image[] img = ldoc.Pages[i].ExtractImages();

                            //Extract text from the existing page
                            string text = ldoc.Pages[i].ExtractText();

                            //If extracted images and string are empty, then remove the blank page 
                            if (img.Length == 0 && string.IsNullOrEmpty(text))
                                ldoc.Pages.RemoveAt(i);

                            i += 1;
                        }
                    }

                    ldoc.Save(options.Output);
                }*/

                /*if (options.RemoveBlankPages)
                {
                    if (!Ghostscript.NET.GhostscriptVersionInfo.IsGhostscriptInstalled)
                    {
                        Console.WriteLine("To use --remove-blank-pages option, Ghostscript must be installed on the system!");
                    }
                    else
                    {
                        // remove any blank pages
                        try
                        {
                            List<int> pagesToRemove = new List<int>();

                            using (var rasterizer = new GhostscriptRasterizer())
                            {
                                rasterizer.Open(options.Output);

                                for (var pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
                                {
                                    var img = rasterizer.GetPage(96, 96, pageNumber);
                                    var bitmap = (System.Drawing.Bitmap)img;

                                    var isBlank = bitmap.IsBlank();

                                    if (isBlank)
                                    {
                                        pagesToRemove.Add(pageNumber - 1);
                                    }

                                }
                            }

                            // delete the blank pages
                            using (PdfSharp.Pdf.PdfDocument pdf = PdfReader.Open(options.Output, PdfDocumentOpenMode.Modify))
                            {
                                pagesToRemove.Reverse();

                                foreach (var page in pagesToRemove)
                                {
                                    pdf.Pages.RemoveAt(page);
                                }

                                pdf.Save(options.Output);
                            }

                            // Compress
                            byte[] inputFile = File.ReadAllBytes(options.Output);

                            //Load a existing PDF document
                            using (PdfLoadedDocument ldoc = new PdfLoadedDocument(inputFile))
                            {

                                //Create a new PDF compression options
                                PdfCompressionOptions compressOptions = new PdfCompressionOptions();
                                compressOptions.CompressImages = true;
                                compressOptions.ImageQuality = 50;
                                compressOptions.OptimizeFont = true;
                                compressOptions.OptimizePageContents = true;
                                compressOptions.RemoveMetadata = true;
                                ldoc.CompressionOptions = compressOptions;
                                ldoc.FileStructure.IncrementalUpdate = false;

                                ldoc.Save(options.Output);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }*/

                if (options.Png)
                {
                    // generate png
                    var pngFile = Path.Combine(Path.GetDirectoryName(options.Output), Path.GetFileNameWithoutExtension(options.Output) + ".png");
                    browser.ScreenshotOrNull()?.Save(pngFile, System.Drawing.Imaging.ImageFormat.Png);
                }

                tokenSource.Cancel();
            });
        }
    }
}
