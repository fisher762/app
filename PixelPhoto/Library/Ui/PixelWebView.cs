using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Webkit;
using PixelPhoto.Helpers.Utils;

namespace PixelPhoto.Library.Ui
{
    public class PixelWebView : WebView
    {
        protected PixelWebView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public PixelWebView(Context context) : base(context)
        {
            Init();
        }

        public PixelWebView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public PixelWebView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init();
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public PixelWebView(Context context, IAttributeSet attrs, int defStyleAttr, bool privateBrowsing) : base(context, attrs, defStyleAttr, privateBrowsing)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            Init();
        }

        public PixelWebView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init();
        }

        private void Init()
        {
            try
            {
                //SetWebChromeClient(new WebChromeClient());
                //SetInitialScale(1);
                //HorizontalScrollBarEnabled = false;
                //VerticalScrollBarEnabled = false;

                //Settings.JavaScriptEnabled = true;
                //Settings.AllowFileAccess = true;
                //Settings.SetPluginState(WebSettings.PluginState.On);
                //Settings.SetPluginState(WebSettings.PluginState.OnDemand);
                //Settings.LoadWithOverviewMode = true;
                //Settings.UseWideViewPort = true;
                //Settings.DomStorageEnabled = true;
                //Settings.LoadsImagesAutomatically = true;
                //Settings.JavaScriptCanOpenWindowsAutomatically = true;
                //Settings.MediaPlaybackRequiresUserGesture = false;
                //Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
                //ScrollBarStyle = ScrollbarStyles.InsideOverlay;

                Settings.LoadsImagesAutomatically = true;
                Settings.JavaScriptEnabled = true;
                Settings.JavaScriptCanOpenWindowsAutomatically = true;
                Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
                Settings.DomStorageEnabled = true;
                Settings.AllowFileAccess = true;
                Settings.DefaultTextEncodingName = "utf-8";

                Settings.UseWideViewPort = true;
                Settings.LoadWithOverviewMode = true;

                Settings.SetSupportZoom(false);
                Settings.BuiltInZoomControls = false;
                Settings.DisplayZoomControls = false;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}