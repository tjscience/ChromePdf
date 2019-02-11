using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromePdf
{
    public class RenderProcessMessageHandler : IRenderProcessMessageHandler
    {
        public void OnContextReleased(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {

        }

        public void OnFocusedNodeChanged(IWebBrowser browserControl, IBrowser browser, IFrame frame, IDomNode node)
        {

        }

        public void OnUncaughtException(IWebBrowser browserControl, IBrowser browser, IFrame frame, JavascriptException exception)
        {

        }

        // Wait for the underlying JavaScript Context to be created. This is only called for the main frame.
        // If the page has no JavaScript, no context will be created.
        void IRenderProcessMessageHandler.OnContextCreated(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
            const string script = @"
window.onerror = function (msg) {
    console.log('chrome-pdf-error->' + msg);
};

window.chromePdf = { 
    convert: function () { 
        setTimeout(function () {
            console.log('chrome-pdf->convert'); 
        }, 0);
    }
};";

            frame.ExecuteJavaScriptAsync(script);
        }
    }
}
