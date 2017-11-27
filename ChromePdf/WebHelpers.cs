namespace ChromePdf
{
    public static class WebHelpers
    {
        public static string DownloadString(string url)
        {
            using (var wc = new System.Net.WebClient())
                return wc.DownloadString(url);
        }
    }
}
