using CefSharp;
using CefSharp.OffScreen;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;

namespace ChromePdf
{
    class Program
    {
        static ChromiumWebBrowser browser;
        static Task waitTask;
        static CancellationTokenSource tokenSource;
        static CancellationToken token;
        static string html;
        static ChromePdfOptions options;
        static bool manualConvert = false;
        static Stopwatch watch;
        static string domain;
        static Uri inputUri;

        static void Main(string[] args)
        {
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            options = new ChromePdfOptions(args);

            inputUri = new Uri(options.InputUri);

            CefSettings settings = new CefSettings
            {
                CachePath = string.Empty
            };

            settings.SetOffScreenRenderingBestPerformanceArgs();

            Cef.Initialize(settings);

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
                        Console.WriteLine("Process timed-out.");
                        tokenSource.Cancel();
                    }
                }
            }, token);

            try
            {
                waitTask.Wait();
            }
            catch { }

            Cef.Shutdown();
        }

        private static void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            if (e.Message.StartsWith("chrome-pdf-error->"))
            {
                Console.WriteLine(e.Message);
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
                    Landscape = options.IsLandscape,
                    MarginType = options.MarginType,
                    HeaderFooterEnabled = options.EnableHeaderFooter,
                    HeaderFooterTitle = options.Title,
                    HeaderFooterUrl = options.Footer,
                    MarginBottom = options.MarginBottom,
                    MarginLeft = options.MarginLeft,
                    MarginRight = options.MarginRight,
                    MarginTop = options.MarginTop,
                    PageHeight = options.PageHeight,
                    PageWidth = options.PageWidth
                };

                Thread.Sleep(options.Delay);

                await browser.PrintToPdfAsync(options.Output, settings);
                tokenSource.Cancel();
            });
        }
    }
}
