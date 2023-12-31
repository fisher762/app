﻿using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Gms.Ads;
using Android.Gms.Ads.AppOpen;
using Android.Gms.Ads.DoubleClick;
using Android.Gms.Ads.Formats;
using Android.Gms.Ads.Initialization;
using Android.Gms.Ads.Rewarded;
using Android.Gms.Ads.RewardedInterstitial;
using Android.Util;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace PixelPhoto.Helpers.Ads
{
    public static class AdsGoogle
    {
        private static int CountInterstitial;
        private static int CountRewarded;
        private static int CountAppOpen;
        private static int CountRewardedInterstitial;

        #region Interstitial

        private class AdMobInterstitial : AdListener
        {
            private InterstitialAd Ad;

            public void ShowAd(Context context)
            {
                try
                {
                    Ad = new InterstitialAd(context) { AdUnitId = AppSettings.AdInterstitialKey };
                    Ad.AdListener = this;
                    Ad.AdListener.OnAdLoaded();

                    var requestBuilder = new AdRequest.Builder();
#pragma warning disable 618
                    requestBuilder.AddTestDevice(UserDetails.AndroidId);
#pragma warning restore 618
                    Ad.LoadAd(requestBuilder.Build());
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnAdLoaded()
            {
                try
                {
                    base.OnAdLoaded();

                    if (Ad != null && Ad.IsLoaded)
                        Ad?.Show();
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }

        public static void Ad_Interstitial(Activity context)
        {
            try
            { 
                if (AppSettings.ShowAdMobInterstitial)
                {
                    if (CountInterstitial == AppSettings.ShowAdMobInterstitialCount)
                    {
                        CountInterstitial = 0;
                        var ads = new AdMobInterstitial();
                        ads.ShowAd(context);
                    }
                    else
                    {
                        Ad_AppOpenManager(context);
                    }

                    CountInterstitial++;
                }
                else
                {
                    Ad_AppOpenManager(context);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Native

        private class AdMobNative : Object, UnifiedNativeAd.IOnUnifiedNativeAdLoadedListener
        {
            private TemplateView Template;
            private Activity Context;
            public void ShowAd(Activity context, TemplateView template = null)
            {
                try
                {
                    Context = context;

                    Template = template ?? Context.FindViewById<TemplateView>(Resource.Id.my_template);
                    if (Template != null)
                    {
                        Template.Visibility = ViewStates.Gone;

                        if (AppSettings.ShowAdMobNative)
                        {
                            var builder = new AdLoader.Builder(Context, AppSettings.AdAdMobNativeKey);
                            builder.ForUnifiedNativeAd(this);
                            var videoOptions = new VideoOptions.Builder()
                                .SetStartMuted(true)
                                .Build();
                            var adOptions = new NativeAdOptions.Builder()
                                .SetVideoOptions(videoOptions)
                                .Build();

                            builder.WithNativeAdOptions(adOptions);

                            var adLoader = builder.WithAdListener(new AdListener()).Build();
                            adLoader.LoadAd(new AdRequest.Builder().Build());
                        }
                        else
                        {
                            Template.Visibility = ViewStates.Gone;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnUnifiedNativeAdLoaded(UnifiedNativeAd ad)
            {
                try
                {
                    var styles = new NativeTemplateStyle.Builder().Build();

                    if (Template.GetTemplateTypeName() == TemplateView.NativeContentAd)
                    {
                        Template.NativeContentAdView(ad);
                    }
                    else
                    {
                        Template.SetStyles(styles);
                        Template.SetNativeAd(ad);
                    }

                    Template.Visibility = ViewStates.Visible;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public static void Ad_AdMobNative(Activity context, TemplateView template = null)
        {
            try
            {
                if (AppSettings.ShowAdMobNative)
                {
                    var ads = new AdMobNative();
                    ads.ShowAd(context, template);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Rewarded

        public class AdMobRewardedVideo : RewardedAdLoadCallback
        {
            private RewardedAd Rad;
            private Activity Context;
            public void ShowAd(Activity context)
            {
                try
                {
                    Context = context;

                    // Use an activity context to get the rewarded video instance. 
                    Rad = new RewardedAd(context, AppSettings.AdRewardVideoKey);

                    var adRequest = new AdRequest.Builder().Build();
                    Rad.LoadAd(adRequest, this);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnRewardedAdLoaded()
            {
                try
                {
                    base.OnRewardedAdLoaded();

                    if (Rad != null && Rad.IsLoaded)
                        Rad.Show(Context, new MyRewardedAdCallback(Rad));
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnRewardedAdFailedToLoad(LoadAdError p0)
            {
                try
                {
                    base.OnRewardedAdFailedToLoad(p0);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private class MyRewardedAdCallback : RewardedAdCallback
            {
                private RewardedAd Rad;
                public MyRewardedAdCallback(RewardedAd rad)
                {
                    Rad = rad;
                }

                public override void OnRewardedAdOpened()
                {
                    try
                    {
                        base.OnRewardedAdOpened();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }

                public override void OnRewardedAdClosed()
                {
                    try
                    {
                        base.OnRewardedAdClosed();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }

                public override void OnRewardedAdFailedToShow(AdError p0)
                {
                    try
                    {
                        base.OnRewardedAdFailedToShow(p0);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }

                public override void OnUserEarnedReward(IRewardItem p0)
                {
                    try
                    {
                        //finish time ad    
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }
            }
        }

        public static void Ad_RewardedVideo(Activity context)
        {
            try
            { 
                if (AppSettings.ShowAdMobRewardVideo)
                {
                    if (CountRewarded == AppSettings.ShowAdMobRewardedVideoCount)
                    {
                        CountRewarded = 0;
                        var ads = new AdMobRewardedVideo();
                        ads.ShowAd(context);
                    }
                    else
                    {
                        Ad_RewardedInterstitial(context);
                    }

                    CountRewarded++;
                }
                else
                {
                    Ad_RewardedInterstitial(context);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Banner

        public static void InitAdView(AdView mAdView, RecyclerView mRecycler)
        {
            try
            {
                if (mAdView == null)
                    return;

                if (AppSettings.ShowAdMobBanner)
                {
                    mAdView.Visibility = ViewStates.Visible;
                    var adRequest = new AdRequest.Builder();
#pragma warning disable 618
                    adRequest.AddTestDevice(UserDetails.AndroidId);
#pragma warning restore 618
                    mAdView.LoadAd(adRequest.Build());
                    mAdView.AdListener = new MyAdListener(mAdView, mRecycler);
                }
                else
                {
                    mAdView.Pause();
                    mAdView.Visibility = ViewStates.Gone;
                    if (mRecycler != null) Methods.SetMargin(mRecycler, 0, 0, 0, 0);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyAdListener : AdListener
        {
            private readonly AdView MAdView;
            private readonly RecyclerView MRecycler;
            public MyAdListener(AdView mAdView, RecyclerView mRecycler)
            {
                MAdView = mAdView;
                MRecycler = mRecycler;
            }

            public override void OnAdFailedToLoad(LoadAdError p0)
            {
                try
                {
                    MAdView.Visibility = ViewStates.Gone;
                    if (MRecycler != null) Methods.SetMargin(MRecycler, 0, 0, 0, 0);
                    base.OnAdFailedToLoad(p0);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }


            public override void OnAdLoaded()
            {
                try
                {
                    MAdView.Visibility = ViewStates.Visible;

                    var r = Application.Context.Resources;
                    var px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, MAdView.AdSize.Height, r.DisplayMetrics);
                    if (MRecycler != null) Methods.SetMargin(MRecycler, 0, 0, 0, px);

                    base.OnAdLoaded();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        #endregion

        #region Publisher

        public static void InitPublisherAdView(PublisherAdView mAdView)
        {
            try
            {
                if (mAdView == null)
                    return;

                if (AppSettings.ShowAdMobBanner)
                {
                    mAdView.Visibility = ViewStates.Visible;
                    var adRequest = new PublisherAdRequest.Builder();
#pragma warning disable 618
                    adRequest.AddTestDevice(UserDetails.AndroidId);
#pragma warning restore 618
                    mAdView.AdListener = new MyPublisherAdViewListener(mAdView);
                    mAdView.LoadAd(adRequest.Build());
                }
                else
                {
                    mAdView.Pause();
                    mAdView.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyPublisherAdViewListener : AdListener
        {
            private readonly PublisherAdView MAdView;
            public MyPublisherAdViewListener(PublisherAdView mAdView)
            {
                MAdView = mAdView;
            }

            public override void OnAdFailedToLoad(LoadAdError p0)
            {
                try
                {
                    MAdView.Visibility = ViewStates.Gone;
                    base.OnAdFailedToLoad(p0);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnAdLoaded()
            {
                try
                {
                    MAdView.Visibility = ViewStates.Visible;
                    base.OnAdLoaded();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        #endregion

        #region AppOpen

        public static void Ad_AppOpenManager(Activity context)
        {
            try
            {
                switch (AppSettings.ShowAdMobAppOpen)
                {
                    case true:
                        {
                            if (CountAppOpen == AppSettings.ShowAdMobAppOpenCount)
                            {
                                CountAppOpen = 0;

                                var appOpenManager = new AppOpenManager(context);
                                appOpenManager.ShowAdIfAvailable();
                            }
                            else
                            {
                                AdsFacebook.InitInterstitial(context);
                            }

                            CountAppOpen++;
                            break;
                        }
                    default:
                        AdsFacebook.InitInterstitial(context);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class AppOpenManager : AppOpenAd.AppOpenAdLoadCallback
        {
            private static readonly long AdExpiryDuration = 3600000 * 4;
            private readonly Activity MostCurrentActivity;
            private static AppOpenAd Ad;
            private static bool IsShowingAd;
            private static long LastAdFetchTime;

            public AppOpenManager(Activity context)
            {
                try
                {
                    MostCurrentActivity = context;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public bool IsAdExpired()
            {
                try
                {
                    return Methods.Time.CurrentTimeMillis() - LastAdFetchTime > AdExpiryDuration;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return false;
                }
            }

            public bool IsAdAvailable()
            {
                try
                {
                    return Ad != null && !IsAdExpired();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return false;
                }
            }

            public void FetchAd()
            {
                try
                {
                    if (IsAdAvailable())
                    {
                        return;
                    }

                    var request = new AdRequest.Builder().Build();
                    AppOpenAd.Load(MostCurrentActivity, AppSettings.AdAdMobAppOpenKey, request, AppOpenAd.AppOpenAdOrientationPortrait, this);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void ShowAdIfAvailable(FullScreenContentCallback listener)
            {
                try
                {
                    switch (IsShowingAd)
                    {
                        case true:
                            //Can't show the ad: Already showing the ad
                            return;
                    }

                    if (!IsAdAvailable())
                    {
                        //Can't show the ad: Ad not available
                        FetchAd();
                        return;
                    }

                    Ad.Show(MostCurrentActivity, new MyFullScreenContentCallback(this, listener));
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void ShowAdIfAvailable()
            {
                try
                {
                    ShowAdIfAvailable(new FullScreenContentCallback());
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnAppOpenAdLoaded(AppOpenAd ad)
            {
                try
                {
                    base.OnAppOpenAdLoaded(ad);

                    LastAdFetchTime = Methods.Time.CurrentTimeMillis();
                    Ad = ad;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnAppOpenAdFailedToLoad(LoadAdError error)
            {
                try
                {
                    base.OnAppOpenAdFailedToLoad(error);
                    Console.WriteLine("Failed to load an ad: " + error.Message);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private class MyFullScreenContentCallback : FullScreenContentCallback
            {
                private readonly FullScreenContentCallback Listener;
                private readonly AppOpenManager AppOpenAdManager;
                public MyFullScreenContentCallback(AppOpenManager appOpenManager, FullScreenContentCallback listener)
                {
                    try
                    {
                        Listener = listener;
                        AppOpenAdManager = appOpenManager;
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }

                public override void OnAdFailedToShowFullScreenContent(AdError p0)
                {
                    try
                    {
                        base.OnAdFailedToShowFullScreenContent(p0);
                        Listener?.OnAdFailedToShowFullScreenContent(p0);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }

                public override void OnAdShowedFullScreenContent()
                {
                    try
                    {
                        base.OnAdShowedFullScreenContent();
                        Listener?.OnAdShowedFullScreenContent();
                        IsShowingAd = true;
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }

                public override void OnAdDismissedFullScreenContent()
                {
                    try
                    {
                        base.OnAdDismissedFullScreenContent();
                        Listener?.OnAdDismissedFullScreenContent();
                        IsShowingAd = false;
                        Ad = null;
                        AppOpenAdManager.FetchAd();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }
            }
        }

        #endregion

        #region RewardedInterstitial

        public class AdMobRewardedInterstitial : RewardedInterstitialAdLoadCallback
        {
            private Activity Context;
            private RewardedInterstitialAd Rad;
            public void ShowAd(Activity context)
            {
                try
                {
                    Context = context;

                    var adRequest = new AdRequest.Builder().Build();

                    // Use an activity context to get the rewarded video instance. 
                    RewardedInterstitialAd.Load(context, AppSettings.AdRewardedInterstitialKey, adRequest, this);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnRewardedInterstitialAdFailedToLoad(LoadAdError p0)
            {
                try
                {
                    base.OnRewardedInterstitialAdFailedToLoad(p0);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnRewardedInterstitialAdLoaded(RewardedInterstitialAd ad)
            {
                try
                {
                    base.OnRewardedInterstitialAdLoaded(ad);
                    Rad = ad;
                    Rad?.Show(Context, new MyUserEarnedRewardListener(Rad));

                    //Rad.SetFullScreenContentCallback(new MyFullScreenContentCallback(Rad));
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private class MyUserEarnedRewardListener : Object, IOnUserEarnedRewardListener
            {
                private RewardedInterstitialAd Rad;
                public MyUserEarnedRewardListener(RewardedInterstitialAd rad)
                {
                    Rad = rad;
                }

                public void OnUserEarnedReward(IRewardItem p0)
                {

                }
            }

            private class MyFullScreenContentCallback : FullScreenContentCallback
            {
                private RewardedInterstitialAd Rad;
                public MyFullScreenContentCallback(RewardedInterstitialAd rad)
                {
                    Rad = rad;
                }

                public override void OnAdFailedToShowFullScreenContent(AdError p0)
                {
                    try
                    {
                        base.OnAdFailedToShowFullScreenContent(p0);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }

                public override void OnAdShowedFullScreenContent()
                {
                    try
                    {
                        base.OnAdShowedFullScreenContent();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }

                public override void OnAdDismissedFullScreenContent()
                {
                    try
                    {
                        base.OnAdDismissedFullScreenContent();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }
            }
        }

        public static AdMobRewardedInterstitial Ad_RewardedInterstitial(Activity context)
        {
            try
            {
                switch (AppSettings.ShowAdMobRewardedInterstitial)
                {
                    case true when CountRewardedInterstitial == AppSettings.ShowAdMobRewardedInterstitialCount:
                        {
                            CountRewardedInterstitial = 0;
                            var ads = new AdMobRewardedInterstitial();
                            ads.ShowAd(context);
                            return ads;
                        }
                    case true:
                        AdsFacebook.InitRewardVideo(context);
                        break;
                    default:
                        AdsFacebook.InitRewardVideo(context);
                        break;
                }

                return null!;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        #endregion

        public static class InitializeAdsGoogle
        {
            public static void Initialize(Context context)
            {
                try
                {
                    if (AppSettings.ShowAdMobBanner || AppSettings.ShowAdMobInterstitial || AppSettings.ShowAdMobRewardVideo || AppSettings.ShowAdMobNative || AppSettings.ShowAdMobAppOpen || AppSettings.ShowAdMobRewardedInterstitial)
                        MobileAds.Initialize(context, new MyInitializationCompleteListener());
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private class MyInitializationCompleteListener : Object, IOnInitializationCompleteListener
            {
                public void OnInitializationComplete(IInitializationStatus p0)
                {

                }
            }
        }
    }
}