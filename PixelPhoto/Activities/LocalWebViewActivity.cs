using System;
using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Webkit;
using Android.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using PixelPhoto.Activities.Base;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Utils;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class LocalWebViewActivity : BaseActivity
    {
        #region Variables Basic

        private static SwipeRefreshLayout SwipeRefreshLayout;
        private static WebView HybridView;
        private string Url = "", TypeUrl = "";
        private AdView MAdView;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);  
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.LocalWebViewLayout);

                var data = Intent?.GetStringExtra("URL") ?? "Data not available";
                if (data != "Data not available" && !string.IsNullOrEmpty(data))
                {
                    Url = data;
                }
                var type = Intent?.GetStringExtra("Type") ?? "Data not available";
                if (type != "Data not available" && !string.IsNullOrEmpty(type))
                {
                    TypeUrl = type;
                }

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetWebView();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);

                MAdView?.Resume();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);

                MAdView?.Pause();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        protected override void OnDestroy()
        {
            try
            {
                MAdView?.Destroy();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                HybridView = FindViewById<WebView>(Resource.Id.LocalWebView);
              
                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, null);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = TypeUrl;
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                  
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetWebView()
        {
            try
            {
                //Set WebView and Load url to be rendered on WebView
                if (!Methods.CheckConnectivity())
                {
                    SwipeRefreshLayout.Refreshing = false;

                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
                else
                {
                    //Set WebView
                    HybridView.SetWebViewClient(new MyWebViewClient(this));

                    //Load url to be rendered on WebView
                    HybridView.LoadUrl(Url);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Event Refresh Data Page
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                HybridView.Reload();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private class MyWebViewClient : WebViewClient, IValueCallback
        {
            private readonly Activity MActivity;
            public MyWebViewClient(Activity mActivity)
            {
                MActivity = mActivity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                view.LoadUrl(request.Url.ToString());
                return true;
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                try
                {
                    base.OnPageStarted(view, url, favicon);
                    SwipeRefreshLayout.Refreshing = true;
                    SwipeRefreshLayout.Enabled = true;
                     
                    view.Settings.JavaScriptEnabled = true;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnPageFinished(WebView view, string url)
            {
                try
                {
                    base.OnPageFinished(view, url);
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;
                     
                    const string js = "javascript:" +
                                      "$('.header-container').hide();" +
                                      "$('.footer-wrapper').hide();" +
                                      "$('.content-container').css('margin-top', '0');" +
                                      "$('.wo_about_wrapper_parent').css('top', '0');";

                    if (Build.VERSION.SdkInt >= (BuildVersionCodes)19)
                    {
                        view.EvaluateJavascript(js, this);
                    }
                    else
                    {
                        view.LoadUrl(js);
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
            {
                try
                {
                    base.OnReceivedError(view, request, error);
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;
                    const string js = "javascript:" +
                                      "$('.header-container').hide();" +
                                      "$('.footer-wrapper').hide();" +
                                      "$('.content-container').css('margin-top', '0');" +
                                      "$('.wo_about_wrapper_parent').css('top', '0');";

                    if (Build.VERSION.SdkInt >= (BuildVersionCodes)19)
                    {
                        view.EvaluateJavascript(js, this);
                    }
                    else
                    {
                        view.LoadUrl(js);
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnReceiveValue(Object value)
            {
                try
                {

                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }
    }
} 