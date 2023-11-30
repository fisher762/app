using Android.Content;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Ext.Ima;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using Java.Lang;
using System;
using System.Linq;
using Com.Google.Ads.Interactivemedia.V3.Api;
using Com.Google.Android.Exoplayer2.Drm;
using Com.Google.Android.Exoplayer2.Source.Ads;
using Com.Google.Android.Exoplayer2.Source.Dash;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Source.Smoothstreaming;
using PixelPhoto.Activities.Posts.page;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.MediaPlayers;
using Exception = System.Exception;
using Uri = Android.Net.Uri;

namespace PixelPhoto.Helpers.Controller
{
    public class VideoController : Java.Lang.Object, View.IOnClickListener
    {
        #region Variables Basic

        private View ActivityContext { get; set; }
        private string ActivityName { get; set; }

        public SimpleExoPlayer Player { get; private set; }

        private ImaAdsLoader ImaAdsLoader;
        private PlayerEvents PlayerListener;
        private PlayerView FullscreenPlayerView;

        private PlayerView SimpleExoPlayerView;
        private FrameLayout MainVideoFrameLayout, MFullScreenButton;
        private PlayerControlView ControlView;
        private ImageView FullScreenIcon; 
        private static IMediaSource VideoSource; 

        private readonly int ResumeWindow = 0;
        private readonly long ResumePosition = 0;
        private string VideoUrL;

        #endregion
         
        public VideoController(View activity, string activityName)
        {
            try
            {  
                ActivityName = activityName;
                ActivityContext = activity;
                 
                Initialize();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Initialize()
        {
            try
            {
                PlayerListener = new PlayerEvents(ActivityContext, ControlView);

                if (ActivityName != "FullScreen")
                {
                    SimpleExoPlayerView = ActivityContext.FindViewById<PlayerView>(Resource.Id.player_view);
                    SimpleExoPlayerView.SetControllerVisibilityListener(PlayerListener);
                    SimpleExoPlayerView.RequestFocus();

                    //Player initialize
                    ControlView = SimpleExoPlayerView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);
                    PlayerListener = new PlayerEvents(ActivityContext, ControlView);

                    FullScreenIcon = ControlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                    MFullScreenButton = ControlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);

                    MainVideoFrameLayout = ActivityContext.FindViewById<FrameLayout>(Resource.Id.root);
                    MainVideoFrameLayout.SetOnClickListener(this);

                    if (!MFullScreenButton.HasOnClickListeners)
                        MFullScreenButton.SetOnClickListener(this);
                }
                else
                {
                    FullscreenPlayerView = ActivityContext.FindViewById<PlayerView>(Resource.Id.player_view2);
                    ControlView = FullscreenPlayerView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);
                    PlayerListener = new PlayerEvents(ActivityContext, ControlView);

                    FullScreenIcon = ControlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                    MFullScreenButton = ControlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);

                    if (!MFullScreenButton.HasOnClickListeners)
                        MFullScreenButton.SetOnClickListener(this);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void PlayVideo(string videoUrL)
        {
            try
            {
                if (!string.IsNullOrEmpty(videoUrL))
                {
                    VideoUrL = videoUrL;

                    ReleaseVideo();

                    FullScreenIcon.SetImageDrawable(ActivityContext.Context.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_expand));

                    var url = Uri.Parse(videoUrL);

                    var trackSelectionFactory = new AdaptiveTrackSelection.Factory();
                    var trackSelector = new DefaultTrackSelector(DefaultTrackSelector.Parameters.GetDefaults(ActivityContext.Context), trackSelectionFactory);
                    trackSelector.SetParameters(new DefaultTrackSelector.ParametersBuilder(ActivityContext.Context));

                    Player = new SimpleExoPlayer.Builder(ActivityContext.Context).Build();


                    // Produces DataSource instances through which media data is loaded.
                    var defaultSource = GetMediaSourceFromUrl(url, url?.Path?.Split('.').Last(), "normal");

                    VideoSource = null!;

                    if (SimpleExoPlayerView == null)
                        Initialize();

                    //Set Interactive Media Ads 
                    if (PlayerSettings.ShowInteractiveMediaAds)
                        VideoSource = CreateMediaSourceWithAds(defaultSource, PlayerSettings.ImAdsUri);
                     
                    //Set Cache Media Load
                    if (PlayerSettings.EnableOfflineMode)
                    {
                        VideoSource = VideoSource == null ? CreateCacheMediaSource(defaultSource, url) : CreateCacheMediaSource(VideoSource, url);
                        if (VideoSource != null)
                        {
                            SimpleExoPlayerView.Player = Player;
                            Player.Prepare(VideoSource);
                            //Player.AddListener(PlayerListener);
                            Player.PlayWhenReady = true;

                            var haveResumePosition = ResumeWindow != C.IndexUnset;
                            if (haveResumePosition)
                                Player.SeekTo(ResumeWindow, ResumePosition);

                            return;
                        }
                    }


                    if (VideoSource == null && !string.IsNullOrEmpty(url?.Path))
                    {
                        VideoSource = GetMediaSourceFromUrl(url, url.Path?.Split('.').Last(), "normal");

                        SimpleExoPlayerView.Player = Player;
                        Player.Prepare(VideoSource);
                        //Player.AddListener(PlayerListener);
                        Player.PlayWhenReady = true;

                        var haveResumePosition = ResumeWindow != C.IndexUnset;
                        if (haveResumePosition)
                            Player.SeekTo(ResumeWindow, ResumePosition);
                    }
                    else
                    {
                        SimpleExoPlayerView.Player = Player;
                        Player.Prepare(VideoSource);
                        //Player.AddListener(PlayerListener);
                        Player.PlayWhenReady = true;

                        var haveResumePosition = ResumeWindow != C.IndexUnset;
                        if (haveResumePosition)
                            Player.SeekTo(ResumeWindow, ResumePosition);
                    } 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ReleaseVideo()
        {
            try
            {
                if (Player != null)
                {
                    SetStopVideo();

                    Player?.Release();
                    Player = null!;

                    //GC Collector
                    GC.Collect();
                }

                SimpleExoPlayerView.Player = null!;
                ReleaseAdsLoader();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetStopVideo()
        {
            try
            {
                if (SimpleExoPlayerView.Player != null)
                {
                    if (SimpleExoPlayerView.Player.PlaybackState == IPlayer.StateReady)
                    {
                        SimpleExoPlayerView.Player.PlayWhenReady = false;
                    }

                    //GC Collector
                    GC.Collect();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Video player
        private IMediaSource CreateCacheMediaSource(IMediaSource videoSource, Uri videoUrL)
        {
            try
            {
                if (PlayerSettings.EnableOfflineMode)
                {
                    videoSource = GetMediaSourceFromUrl(videoUrL, videoUrL?.Path?.Split('.').Last(), "normal");
                    return videoSource;
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        private IMediaSource CreateMediaSourceWithAds(IMediaSource videoSource, Uri imAdsUri)
        {
            try
            {
                // Player = ExoPlayerFactory.NewSimpleInstance(ActivityContext);
                SimpleExoPlayerView.Player = Player;

                if (ImaAdsLoader == null)
                {
                    Player ??= new SimpleExoPlayer.Builder(ActivityContext.Context).Build();
                    SimpleExoPlayerView.Player = Player;

                    if (ImaAdsLoader == null)
                    {
                        var imaSdkSettings = ImaSdkFactory.Instance.CreateImaSdkSettings();
                        imaSdkSettings.AutoPlayAdBreaks = true;
                        imaSdkSettings.DebugMode = true;

                        ImaAdsLoader = new ImaAdsLoader.Builder(ActivityContext.Context)
                            .SetImaSdkSettings(imaSdkSettings)
                            .SetMediaLoadTimeoutMs(30 * 1000)
                            .SetVastLoadTimeoutMs(30 * 1000)
                            .BuildForAdTag(imAdsUri); // here is url for vast xml file

                        IMediaSourceFactory adMediaSourceFactory = new MediaSourceFactoryAnonymousInnerClass(this);

                        IMediaSource mediaSourceWithAds = new AdsMediaSource(videoSource, adMediaSourceFactory, ImaAdsLoader, SimpleExoPlayerView);
                        return mediaSourceWithAds;
                    }
                }
            }
            catch (ClassNotFoundException e)
            {
                Console.WriteLine(e.Message);
                // IMA extension not loaded.
                return null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
            return null!;
        }

        private class MediaSourceFactoryAnonymousInnerClass : Java.Lang.Object, IMediaSourceFactory
        {
            private readonly VideoController outerInstance;
            private IDrmSessionManager drmSessionManager = null;

            public MediaSourceFactoryAnonymousInnerClass(VideoController outerInstance)
            {
                this.outerInstance = outerInstance;
                drmSessionManager = IDrmSessionManager.DummyDrmSessionManager;
            }

            public IMediaSource CreateMediaSource(Uri uri)
            {
                return outerInstance.GetMediaSourceFromUrl(uri, uri?.Path?.Split('.').Last(), "ads");
            }

            public int[] GetSupportedTypes()
            {
                return new[] { C.TypeDash, C.TypeSs, C.TypeHls, C.TypeOther };
            }

            public IMediaSourceFactory SetDrmSessionManager(IDrmSessionManager drmSessionManager)
            {
                this.drmSessionManager = drmSessionManager ?? IDrmSessionManager.DummyDrmSessionManager;
                return this;
            }
        }

        private IMediaSource GetMediaSourceFromUrl(Uri uri, string extension, string tag)
        {
            try
            {
                var BandwidthMeter = DefaultBandwidthMeter.GetSingletonInstance(ActivityContext.Context);
                //DefaultDataSourceFactory dataSourceFactory = new DefaultDataSourceFactory(ActivityContext, Util.GetUserAgent(ActivityContext.Context, AppSettings.ApplicationName), mBandwidthMeter);
                var buildHttpDataSourceFactory = new DefaultDataSourceFactory(ActivityContext.Context, BandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(ActivityContext.Context, AppSettings.ApplicationName)));
                var buildHttpDataSourceFactoryNull = new DefaultDataSourceFactory(ActivityContext.Context, BandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(ActivityContext.Context, AppSettings.ApplicationName)));
                var type = Util.InferContentType(uri, extension);
                var src = type switch
                {
                    C.TypeSs => new SsMediaSource.Factory(new DefaultSsChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).SetDrmSessionManager(IDrmSessionManager.DummyDrmSessionManager).CreateMediaSource(uri),
                    C.TypeDash => new DashMediaSource.Factory(new DefaultDashChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).SetDrmSessionManager(IDrmSessionManager.DummyDrmSessionManager).CreateMediaSource(uri),
                    C.TypeHls => new HlsMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).SetDrmSessionManager(IDrmSessionManager.DummyDrmSessionManager).CreateMediaSource(uri),
                    C.TypeOther => new ProgressiveMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri),
                    _ => new ProgressiveMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri)
                };
                return src;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }


        private void ReleaseAdsLoader()
        {
            try
            {
                if (ImaAdsLoader != null)
                {
                    ImaAdsLoader.Release();
                    ImaAdsLoader = null!;
                    SimpleExoPlayerView.OverlayFrameLayout.RemoveAllViews();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void RestartPlayAfterShrinkScreen()
        {
            try
            {
                SimpleExoPlayerView.Player = null!;
                SimpleExoPlayerView.Player = Player;
                SimpleExoPlayerView.Player.PlayWhenReady = true;
                FullScreenIcon.SetImageDrawable(ActivityContext.Context.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_expand));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void PlayFullScreen()
        {
            try
            {
                if (FullscreenPlayerView != null)
                {
                    Player?.AddListener(PlayerListener);
                    FullscreenPlayerView.Player = Player;
                    if (FullscreenPlayerView.Player != null) FullscreenPlayerView.Player.PlayWhenReady = true;
                    FullScreenIcon.SetImageDrawable(ActivityContext.Context.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_skrink));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion Video player

        public void OnClick(View v)
        {
            try
            {
                if (v.Id == FullScreenIcon.Id || v.Id == MFullScreenButton.Id)
                {
                    InitFullscreenDialog();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Event

        //Full Screen
        private void InitFullscreenDialog()
        {
            try
            {
                Player.PlayWhenReady = false;

                var intent = new Intent(ActivityContext.Context, typeof(VideoFullScreenActivity));
                intent.PutExtra("videoUrl", VideoUrL);
                //  intent.PutExtra("videoDuration", videoPlayer.Duration.ToString());
                ActivityContext.Context.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion Event

    }
}