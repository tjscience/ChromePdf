using System;
using System.Net;

namespace ChromePdf
{
    public static class WebHelper
    {
        public static string DownloadString(string url)
        {
            // Accept all security protocols
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                
            using (var wc = new System.Net.WebClient())
                return wc.DownloadString(url);
        }
    }
}
