using Android.Webkit;

using System;

namespace JSPad
{
    class EditorWebViewClient : WebViewClient
    {
        public Action PageLoaded { get; set; }
        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            PageLoaded?.Invoke();
        }

        public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
        {
            view.LoadUrl(request.Url.ToString());
            return false;
        }
    }
}