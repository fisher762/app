using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AndroidX.RecyclerView.Widget;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Database;
using Com.Google.Android.Exoplayer2.Drm;
using Com.Google.Android.Exoplayer2.Offline;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Source.Dash;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Source.Smoothstreaming;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.RestCalls;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream.Cache;
using Java.Util;
using PixelPhoto.Activities.Posts.Adapters;
using PixelPhoto.Activities.Posts.page;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using Console = System.Console;
using LayoutDirection = Android.Views.LayoutDirection;
using Uri = Android.Net.Uri;
using Object = Java.Lang.Object;
using Exception = System.Exception;
using Thread = System.Threading.Thread;

namespace PixelPhoto.Activities.Posts.Extras
{
    public class PRecyclerView : RecyclerView 
    {
        private static PRecyclerView Instance;

        private enum VolumeState
        {
            On,
            Off
        }

        private FrameLayout MediaContainerLayout;
        private ImageView Thumbnail, PlayControl;
        private PlayerView VideoSurfaceView;
        private SimpleExoPlayer VideoPlayer;
        private Uri VideoUrl;
        private View ViewHolderParent;
        private int VideoSurfaceDefaultHeight;
        private int ScreenDefaultHeight;
        private Context MainContext;
        private bool IsVideoViewAdded;
        private int PlayPosition = -1;

        private RecyclerScrollListener MainScrollEvent;
        public NewsFeedAdapter NativeFeedAdapter;

        //private PopupBubble PopupBubbleView;
       
        private VolumeState VolumeStateProvider;
        private ExoPlayerRecyclerEvent PlayerListener;

        private CacheDataSourceFactory CacheDataSourceFactory;
        private static SimpleCache Cache;

        private static DefaultBandwidthMeter BandwidthMeter;
        //private ExctractorMediaListener EventLogger;
        private DefaultDataSourceFactory DefaultDataSourceFac;
        private static NativeFeedType LastAdsType = NativeFeedType.AdMob1;

        protected PRecyclerView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        public PRecyclerView(Context context) : base(context)
        {
            Init(context);
        }

        public PRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public PRecyclerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Init(context);
        }

        private void Init(Context context)
        {
            try
            {
                MainContext = context;

                Instance = this;

                if (AppSettings.FlowDirectionRightToLeft)
                    LayoutDirection = LayoutDirection.Rtl;

                HasFixedSize = true;
                SetItemViewCacheSize(50);

                ClearAnimation();
                var f = GetItemAnimator();
                ((SimpleItemAnimator)f).SupportsChangeAnimations = false;
                f.ChangeDuration = 0;
                 
                var point = Methods.App.OverrideGetSize(MainContext);
                if (point != null)
                {
                    VideoSurfaceDefaultHeight = point.X;
                    ScreenDefaultHeight = point.Y;
                }

                VideoSurfaceView = new PlayerView(MainContext)
                {
                    ResizeMode = AspectRatioFrameLayout.ResizeModeFixedWidth
                };

                //===================== Exo Player ========================
                SetPlayer();
                //=============================================

                MainScrollEvent = new RecyclerScrollListener(this);
                AddOnScrollListener(MainScrollEvent);
                AddOnChildAttachStateChangeListener(new ChildAttachStateChangeListener(this));
                MainScrollEvent.LoadMoreEvent += MainScrollEvent_LoadMoreEvent;
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

      
         
        public static PRecyclerView GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        private bool RunApi;
        public void SetXAdapter(NewsFeedAdapter adapter , bool runApi)
        {
            RunApi = runApi;
            NativeFeedAdapter = adapter;
        }
        //wael
        //public void SetXPopupBubble(PopupBubble popupBubble)
        //{
        //    PopupBubbleView = popupBubble;
        //    PopupBubbleView.SetRecyclerView(this);
        //    PopupBubbleView.SetPopupBubbleListener(new PopupBubbleClickEvent(this));
        //}

        public async Task FetchNewsFeedApiPosts(string typeRun, string offset = "0")
        {
            if (!RunApi)
                return;

            var countList = NativeFeedAdapter.PostList.Count; 
            var (respondCode, respondString) = await RequestsAsync.Post.FetchHomePosts(offset);

            if (respondCode != 200 || !(respondString is FetchListPostsObject postsObject))
            {
                Methods.DisplayReportResult(HomeActivity.GetInstance(), respondString);
            }
            else
            {
                var respondList = postsObject.Data.Count;
                if (respondList > 0)
                {
                    postsObject.Data = AppTools.FilterPost(postsObject.Data);

                    foreach (var item in from item in postsObject.Data let check = NativeFeedAdapter.PostList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                    { 
                        var checkType = NativeFeedAdapter.PostList.LastOrDefault();

                        if (checkType?.Type == "video" || checkType?.Type == "Video")
                            CacheVideosFiles(Uri.Parse(item.MediaSet[0].File));

                        if (NativeFeedAdapter.PostList.Count > 0 && NativeFeedAdapter.PostList.Count % AppSettings.ShowAdMobNativeCount == 0 && AppSettings.ShowAdMobNativePost)
                        {
                            if (checkType?.Type == "AdMob1" || checkType?.Type == "AdMob2")
                                return;
                             
                            if (LastAdsType == NativeFeedType.AdMob2)
                            {
                                LastAdsType = NativeFeedType.AdMob1;
                                NativeFeedAdapter.PostList.Add(new PostsObject { Type = "AdMob1" });
                            }
                            else
                            {
                                LastAdsType = NativeFeedType.AdMob2;
                                NativeFeedAdapter.PostList.Add(new PostsObject { Type = "AdMob2" });
                            }
                        }
                        
                        if (NativeFeedAdapter.PostList.Count > 0 && NativeFeedAdapter.PostList.Count % AppSettings.ShowFbNativeAdsCount == 0 && AppSettings.ShowFbNativeAds)
                        {
                            if (checkType?.Type != "FbNativeAds")
                            {
                                NativeFeedAdapter.PostList.Add(new PostsObject { Type = "FbNativeAds" });
                            }
                        }
                         
                        var check = NativeFeedAdapter.PostList.FirstOrDefault(a => a.Type == "Funding");
                        if (check == null && AppSettings.ShowFunding && NativeFeedAdapter.PostList.Count > 0 && NativeFeedAdapter.PostList.Count % AppSettings.ShowFundingCount == 0 && ListUtils.FundingList.Count > 0)
                        {
                            NativeFeedAdapter.PostList.Add(new PostsObject { Type = "Funding" });
                        }

                        if (item.Boosted == 1)
                        {
                            NativeFeedAdapter.PostList.Insert(0, item);
                        }
                        else
                        {
                            if (typeRun == "Add")
                                NativeFeedAdapter.PostList.Add(item);
                            else
                                NativeFeedAdapter.PostList.Insert(0, item);
                        }  
                    }

                    if (countList > 0)
                    {
                        NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.PostList.Count - countList);
                    }
                    else
                    {
                        NativeFeedAdapter.NotifyDataSetChanged();
                    }
                }
                else
                {
                    if (NativeFeedAdapter.PostList.Count > 10 && !CanScrollVertically(1))
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short)?.Show();
                }

                MainScrollEvent.IsLoading = false;

                var newsFeedFragment = HomeActivity.GetInstance()?.NewsFeedFragment;
                if (newsFeedFragment != null)
                    HomeActivity.GetInstance()?.RunOnUiThread(newsFeedFragment.ShowEmptyPage);
            }
        }
         
        //play the video in the row
        private void PlayVideo(bool isEndOfList)
        {
            try
            {
                int targetPosition;
                if (!isEndOfList)
                {
                    var startPosition = ((LinearLayoutManager)Objects.RequireNonNull(GetLayoutManager())).FindFirstVisibleItemPosition();
                    var endPosition = ((LinearLayoutManager)GetLayoutManager()).FindLastVisibleItemPosition();
                    // if there is more than 2 list-items on the screen, set the difference to be 1
                    if (endPosition - startPosition > 1)
                    {
                        endPosition = startPosition + 1;
                    }

                    // something is wrong. return.
                    if (startPosition < 0 || endPosition < 0)
                    {
                        return;
                    }

                    // if there is more than 1 list-item on the screen
                    if (startPosition != endPosition)
                    {
                        var startPositionVideoHeight = GetVisibleVideoSurfaceHeight(startPosition);
                        var endPositionVideoHeight = GetVisibleVideoSurfaceHeight(endPosition);
                        targetPosition = startPositionVideoHeight > endPositionVideoHeight ? startPosition : endPosition;
                    }
                    else
                    {
                        targetPosition = startPosition;
                    }
                }
                else
                {
                    targetPosition = NativeFeedAdapter.PostList.Count - 1;
                }

                // video is already playing so return
                if (targetPosition == PlayPosition)
                    return;

                // set the position of the list-item that is to be played
                PlayPosition = targetPosition;
                if (VideoSurfaceView == null)
                    return;

                var currentPosition = targetPosition;
                 
                var typePost = NativeFeedAdapter.GetItem(currentPosition);
                if (typePost == null)
                    return;

                var postFeedType = NewsFeedAdapter.GetPostType(typePost);
                if (postFeedType != NativeFeedType.Video)
                    return;

                var item = NativeFeedAdapter.PostList.FirstOrDefault(a => a.Type.ToLower() == "video" && a.PostId == typePost.PostId);
                if (item != null)
                {
                    var indexPost = NativeFeedAdapter.PostList.IndexOf(item);

                    var child = ((LinearLayoutManager)GetLayoutManager()).FindViewByPosition(indexPost);
                    var holder = (Holders.VideoAdapterViewHolder)child?.Tag;
                    if (holder == null)
                        return;

                    // remove any old surface views from previously playing videos
                    VideoSurfaceView.Visibility = ViewStates.Invisible;
                    RemoveVideoView(VideoSurfaceView);

                    if (VideoPlayer == null)
                        SetPlayer();

                    MediaContainerLayout = holder.MediaContainerLayout;
                    Thumbnail = holder.VideoImage;
                    ViewHolderParent = holder.ItemView;
                    PlayControl = holder.PlayControl;

                    if (!IsVideoViewAdded)
                        AddVideoView();

                    VideoSurfaceView.Player = VideoPlayer;

                    var controlView = VideoSurfaceView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);

                    VideoSurfaceView.SetMinimumHeight(holder.VideoImage.Height);
                    controlView.SetMinimumHeight(holder.VideoImage.Height);

                    VideoUrl = Uri.Parse(item.MediaSet[0].File);

                    holder.VideoUrl = VideoUrl.ToString();

                    //if (item.Blur != "0")
                    //    return;

                    HomeActivity.GetInstance()?.SetOnWakeLock();

                    //>> Old Code 
                    //===================== Exo Player ======================== 
                    PlayerListener = new ExoPlayerRecyclerEvent(controlView, this, holder);
                    PlayerListener.MFullScreenButton.SetOnClickListener(new NewClicker(PlayerListener.MFullScreenButton, holder.VideoUrl, this));

                    var dataSpec = new DataSpec(VideoUrl); //0, 1000 * 1024, null

                    if (Cache == null)
                        CacheVideosFiles(VideoUrl);

                    CacheDataSourceFactory ??= new CacheDataSourceFactory(Cache, DefaultDataSourceFac);

                    CacheUtil.GetCached(dataSpec, Cache, new MyCacheKeyFactory());
                    var videoSource = GetMediaSourceFromUrl(VideoUrl, VideoUrl?.Path?.Split('.').Last(), "normal");

                    VideoPlayer.Prepare(videoSource);
                    VideoPlayer.PlayWhenReady = true;
                    //VideoPlayer.AddListener(PlayerListener);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void PlayVideo(bool isEndOfList, Holders.VideoAdapterViewHolder holder, PostsObject item)
        {
            try
            {
                Console.WriteLine(isEndOfList);
                if (VideoPlayer == null)
                    SetPlayer();

                if (VideoSurfaceView == null)
                    return;

                VideoSurfaceView.Visibility = ViewStates.Invisible;
                RemoveVideoView(VideoSurfaceView);

                MediaContainerLayout = holder.MediaContainerLayout;
                Thumbnail = holder.VideoImage;
                ViewHolderParent = holder.ItemView;
                PlayControl = holder.PlayControl;

                if (!IsVideoViewAdded)
                    AddVideoView();

                VideoSurfaceView.Player = VideoPlayer;

                var controlView = VideoSurfaceView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);
                VideoUrl = Uri.Parse(item.MediaSet[0].File);

                holder.VideoUrl = VideoUrl.ToString();
                 
                HomeActivity.GetInstance()?.SetOnWakeLock();

                //>> Old Code 
                //===================== Exo Player ======================== 
                PlayerListener = new ExoPlayerRecyclerEvent(controlView, this, holder);
                PlayerListener.MFullScreenButton.SetOnClickListener(new NewClicker(PlayerListener.MFullScreenButton, holder.VideoUrl, this));
                 
                var dataSpec = new DataSpec(VideoUrl); //0, 1000 * 1024, null

                if (Cache == null)
                    CacheVideosFiles(VideoUrl);

                CacheDataSourceFactory ??= new CacheDataSourceFactory(Cache, DefaultDataSourceFac);

                CacheUtil.GetCached(dataSpec, Cache,  new MyCacheKeyFactory());
                var videoSource = GetMediaSourceFromUrl(VideoUrl, VideoUrl?.Path?.Split('.').Last(), "normal");

                VideoPlayer.Prepare(videoSource);
                VideoPlayer.PlayWhenReady = true;

                //VideoPlayer.AddListener(null);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private class NewClicker : Object, IOnClickListener
        {
            private string VideoUrl { get; set; }
            private FrameLayout FullScreenButton { get; set; }
            private PRecyclerView WRecyclerViewController { get; set; }
            public NewClicker(FrameLayout fullScreenButton, string videoUrl, PRecyclerView wRecyclerViewController)
            {
                WRecyclerViewController = wRecyclerViewController;
                FullScreenButton = fullScreenButton;
                VideoUrl = videoUrl;
            }
            public void OnClick(View v)
            {
                if (v.Id == FullScreenButton.Id)
                {
                    try
                    { 
                       WRecyclerViewController.VideoPlayer.PlayWhenReady = false;

                        var intent = new Intent(WRecyclerViewController.MainContext, typeof(VideoFullScreenActivity));
                        intent.PutExtra("videoUrl", VideoUrl);
                        //  intent.PutExtra("videoDuration", videoPlayer.Duration.ToString());
                        WRecyclerViewController.MainContext.StartActivity(intent);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }
            } 
        }
         
        public void PlayFullScreen()
        {
            try
            {
                if (FullScreenVideoActivity.FullscreenPlayerView != null)
                {
                    FullScreenVideoActivity.FullscreenPlayerView.Player = VideoPlayer;

                    if (FullScreenVideoActivity.FullscreenPlayerView.Player != null) 
                        FullScreenVideoActivity.FullscreenPlayerView.Player.PlayWhenReady = true;

                    FullScreenVideoActivity.MFullScreenIcon.SetImageDrawable(MainContext.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_skrink));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MainScrollEvent_LoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                MainScrollEvent.IsLoading = true;
                 
                if (NativeFeedAdapter.PostList.Count <= 5)
                    return;
                  
                var item = NativeFeedAdapter.PostList.LastOrDefault();
                if (item != null)
                {
                    var lastItem = NativeFeedAdapter.PostList.IndexOf(item);

                    item = NativeFeedAdapter.PostList[lastItem];

                    if (item.Type == "AdMob1" ||item.Type == "AdMob2" || item.Type == "FbNativeAds" || item.Type == "Funding")
                    {
                        item = NativeFeedAdapter.PostList.LastOrDefault(a => a.Type != "AdMob1" &&a.Type != "AdMob2" && a.Type != "FbNativeAds" && a.Type != "Funding");
                    }
                }

                var offset = "0";

                if (item != null)
                    offset = item.PostId.ToString();

                if (!Methods.CheckConnectivity())
                    Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => FetchNewsFeedApiPosts("Add",offset)});
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public class RecyclerScrollListener : OnScrollListener
        {
            public delegate void LoadMoreEventHandler(object sender, EventArgs e);

            public event LoadMoreEventHandler LoadMoreEvent;

            public bool IsLoading { get; set; }
            public bool IsScrolling { get; set; }

            private PreCachingLayoutManager LayoutManager { get; set; }
            private readonly PRecyclerView XRecyclerView;

            public RecyclerScrollListener(PRecyclerView recyclerView)
            {
                XRecyclerView = recyclerView;
                //LayoutManager = recyclerView.PreCachingLayoutManager;
                IsLoading = false;
            }

            public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
            {
                try
                {
                    base.OnScrollStateChanged(recyclerView, newState);

                    switch (newState)
                    {
                        case (int)Android.Widget.ScrollState.TouchScroll:
                        //if (Glide.With(XRecyclerView.Context).IsPaused)
                        //    Glide.With(XRecyclerView.Context).ResumeRequests();
                        case (int)Android.Widget.ScrollState.Fling:
                            IsScrolling = true;
                            //Glide.With(XRecyclerView.Context).PauseRequests();
                            break;
                        case (int)Android.Widget.ScrollState.Idle:
                            {
                                // There's a special case when the end of the list has been reached.
                                // Need to handle that with this bit of logic
                                if (AppSettings.AutoPlayVideo)
                                {
                                    XRecyclerView.PlayVideo(!recyclerView.CanScrollVertically(1));
                                }

                                //if (Glide.With(XRecyclerView.Context).IsPaused)
                                //    Glide.With(XRecyclerView.Context).ResumeRequests();
                                break;
                            }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                try
                {
                    base.OnScrolled(recyclerView, dx, dy);

                    LayoutManager ??= (PreCachingLayoutManager)recyclerView.GetLayoutManager();

                    var visibleItemCount = recyclerView.ChildCount;
                    var totalItemCount = recyclerView.GetAdapter().ItemCount;

                    var pastItems = LayoutManager.FindFirstVisibleItemPosition();

                    var counter = visibleItemCount + pastItems + 10;
                    if (counter < totalItemCount)
                        return;

                    if (IsLoading)
                        return;

                    Console.WriteLine("WOLog" + counter + "WOLog totalItemCount=" + totalItemCount);
                    LoadMoreEvent?.Invoke(this, null);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        #region Listeners

        private class ChildAttachStateChangeListener : Object, IOnChildAttachStateChangeListener
        {
            private readonly PRecyclerView XRecyclerView;

            public ChildAttachStateChangeListener(PRecyclerView recyclerView)
            {
                XRecyclerView = recyclerView;
            }

            public void OnChildViewAttachedToWindow(View view)
            {
                try
                {
                    if (XRecyclerView.ViewHolderParent != null && XRecyclerView.ViewHolderParent.Equals(view))
                    {
                        if (!XRecyclerView.IsVideoViewAdded)
                            return;

                        XRecyclerView.RemoveVideoView(XRecyclerView.VideoSurfaceView);
                        XRecyclerView.PlayPosition = -1;
                        XRecyclerView.VideoSurfaceView.Visibility = ViewStates.Invisible;

                        XRecyclerView.ReleasePlayer();

                        if (XRecyclerView.Thumbnail != null)
                        {
                            XRecyclerView.Thumbnail.Visibility = ViewStates.Visible;
                            if (!string.IsNullOrEmpty(XRecyclerView.VideoUrl.Path))
                                XRecyclerView.NativeFeedAdapter.FullGlideRequestBuilder.Load(XRecyclerView.VideoUrl).Into(XRecyclerView.Thumbnail);
                        }

                        if (XRecyclerView.PlayControl != null) XRecyclerView.PlayControl.Visibility = ViewStates.Visible;

                        var mainHolder = XRecyclerView.GetChildViewHolder(view);
                        if (!(mainHolder is Holders.VideoAdapterViewHolder holder))
                            return;

                        holder.VideoImage.Visibility = ViewStates.Visible;
                        holder.PlayControl.Visibility = ViewStates.Visible;
                        holder.VideoProgressBar.Visibility = ViewStates.Gone;
                    } 
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnChildViewDetachedFromWindow(View view)
            {
                try
                {
                    if (XRecyclerView.ViewHolderParent != null && XRecyclerView.ViewHolderParent.Equals(view))
                    {
                        if (!XRecyclerView.IsVideoViewAdded)
                            return;

                        XRecyclerView.RemoveVideoView(XRecyclerView.VideoSurfaceView);
                        XRecyclerView.PlayPosition = -1;
                        XRecyclerView.VideoSurfaceView.Visibility = ViewStates.Invisible;

                        XRecyclerView.ReleasePlayer();

                        if (XRecyclerView.Thumbnail != null)
                        {
                            XRecyclerView.Thumbnail.Visibility = ViewStates.Visible;
                            if (!string.IsNullOrEmpty(XRecyclerView.VideoUrl.Path))
                                XRecyclerView.NativeFeedAdapter.FullGlideRequestBuilder.Load(XRecyclerView.VideoUrl).Into(XRecyclerView.Thumbnail);
                        }

                        if (XRecyclerView.PlayControl != null) XRecyclerView.PlayControl.Visibility = ViewStates.Visible;

                        var mainHolder = XRecyclerView.GetChildViewHolder(view);
                        if (!(mainHolder is Holders.VideoAdapterViewHolder holder))
                            return;

                        holder.VideoImage.Visibility = ViewStates.Visible;
                        holder.PlayControl.Visibility = ViewStates.Visible;
                        holder.VideoProgressBar.Visibility = ViewStates.Gone;
                    } 
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        //public TrackGroupArray lastSeenTrackGroupArray;
        private class ExoPlayerRecyclerEvent : Object, IPlayerEventListener
        {
            private readonly ProgressBar LoadingProgressBar;
            private readonly ImageButton VideoPlayButton, VideoResumeButton;
            private readonly ImageView VolumeControl;
            public readonly FrameLayout MFullScreenButton;
            private readonly PRecyclerView XRecyclerView;
            public ExoPlayerRecyclerEvent(PlayerControlView controlView, PRecyclerView recyclerView, Holders.VideoAdapterViewHolder holder)
            {
                try
                {
                    XRecyclerView = recyclerView;
                    if (controlView == null)
                        return;

                    var mFullScreenIcon = controlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                    MFullScreenButton = controlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);

                    VideoPlayButton = controlView.FindViewById<ImageButton>(Resource.Id.exo_play);
                    VideoResumeButton = controlView.FindViewById<ImageButton>(Resource.Id.exo_pause);
                    VolumeControl = controlView.FindViewById<ImageView>(Resource.Id.exo_volume_icon);

                    if (!AppSettings.ShowFullScreenVideoPost)
                    {
                        mFullScreenIcon.Visibility = ViewStates.Gone;
                        MFullScreenButton.Visibility = ViewStates.Gone;
                    }

                    if (holder != null) LoadingProgressBar = holder.VideoProgressBar;

                    SetVolumeControl(XRecyclerView.VolumeStateProvider == VolumeState.On ? VolumeState.On : VolumeState.Off);

                    if (!VolumeControl.HasOnClickListeners)
                    {
                        VolumeControl.Click += (sender, args) =>
                        {
                            ToggleVolume();
                        };
                    }

                    //if (!MFullScreenButton.HasOnClickListeners)
                    //{
                    //    MFullScreenButton.Click += (sender, args) =>
                    //    {
                    //        new PostClickListener((Activity)recyclerView.Context, recyclerView.NativeFeedAdapter.NativePostType).InitFullscreenDialog(Uri.Parse(holder?.VideoUrl), null);
                    //    };
                    //}
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private void ToggleVolume()
            {
                try
                {
                    if (XRecyclerView.VideoPlayer == null)
                        return;

                    switch (XRecyclerView.VolumeStateProvider)
                    {
                        case VolumeState.Off:
                            SetVolumeControl(VolumeState.On);
                            break;
                        case VolumeState.On:
                            SetVolumeControl(VolumeState.Off);
                            break;
                        default:
                            SetVolumeControl(VolumeState.Off);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private void SetVolumeControl(VolumeState state)
            {
                try
                {
                    XRecyclerView.VolumeStateProvider = state;
                    switch (state)
                    {
                        case VolumeState.Off:
                            XRecyclerView.VolumeStateProvider = VolumeState.Off;
                            XRecyclerView.VideoPlayer.Volume = 0f;
                            AnimateVolumeControl();
                            break;
                        case VolumeState.On:
                            XRecyclerView.VolumeStateProvider = VolumeState.On;
                            XRecyclerView.VideoPlayer.Volume = 1f;
                            AnimateVolumeControl();
                            break;
                        default:
                            XRecyclerView.VideoPlayer.Volume = 1f;
                            AnimateVolumeControl();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private void AnimateVolumeControl()
            {
                try
                {
                    if (VolumeControl == null)
                        return;

                    VolumeControl.BringToFront();
                    switch (XRecyclerView.VolumeStateProvider)
                    {
                        case VolumeState.Off:
                            VolumeControl.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);

                            break;
                        case VolumeState.On:
                            VolumeControl.SetImageResource(Resource.Drawable.ic_volume_up_grey_24dp);
                            break;
                        default:
                            VolumeControl.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);
                            break;
                    }
                    //VolumeControl.Animate().Cancel();

                    //VolumeControl.Alpha = (1f);

                    //VolumeControl.Animate().Alpha(0f).SetDuration(600).SetStartDelay(1000);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            //public void OnLoadingChanged(bool isLoading)
            //{
            //    Console.WriteLine("Loading changed");
            //}

            //public void OnPlaybackParametersChanged(PlaybackParameters p0)
            //{
            //    //this._mediaPlayer.PlaybackParameters = p0;
            //}

            //public void OnPlayerError(ExoPlaybackException ex)
            //{
            //    //this.OnMediaFileFailed(new MediaFileFailedEventArgs(ex, this.CurrentFile));
            //}
             
            public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
            {
                try
                {
                    //if (VideoResumeButton == null || VideoPlayButton == null || LoadingProgressBar == null)
                    //    return;

                    if (playbackState == IPlayer.StateEnded)
                    {
                        if (playWhenReady == false)
                            VideoResumeButton.Visibility = ViewStates.Visible;
                        else
                        {
                            VideoResumeButton.Visibility = ViewStates.Gone;
                            VideoPlayButton.Visibility = ViewStates.Visible;
                        }

                        LoadingProgressBar.Visibility = ViewStates.Invisible;
                        XRecyclerView.VideoPlayer.SeekTo(0);

                        HomeActivity.GetInstance()?.SetOffWakeLock();
                    }
                    else if (playbackState == IPlayer.StateReady)
                    {
                        if (playWhenReady == false)
                        {
                            VideoResumeButton.Visibility = ViewStates.Gone;
                            VideoPlayButton.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            VideoResumeButton.Visibility = ViewStates.Visible;
                        }

                        LoadingProgressBar.Visibility = ViewStates.Invisible;

                        if (!XRecyclerView.IsVideoViewAdded)
                            XRecyclerView.AddVideoView();

                        HomeActivity.GetInstance()?.SetOnWakeLock();
                    }
                    else if (playbackState == IPlayer.StateBuffering)
                    {
                        VideoPlayButton.Visibility = ViewStates.Invisible;
                        LoadingProgressBar.Visibility = ViewStates.Visible;
                        VideoResumeButton.Visibility = ViewStates.Invisible;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            //public void OnPositionDiscontinuity(int p0)
            //{
            //}

            //public void OnRepeatModeChanged(int p0)
            //{
            //}

            //public void OnSeekProcessed()
            //{
            //}

            //public void OnShuffleModeEnabledChanged(bool p0)
            //{
            //}
              
            //public void OnTracksChanged(TrackGroupArray trackGroups, TrackSelectionArray trackSelections)
            //{
                 
            //}
        }

        #endregion

        #region VideoObject player

        private void SetPlayer()
        {
            try
            {
                BandwidthMeter = DefaultBandwidthMeter.GetSingletonInstance(MainContext);

                var trackSelector = new DefaultTrackSelector(MainContext);
                trackSelector.SetParameters(new DefaultTrackSelector.ParametersBuilder(MainContext));

                VideoPlayer = new SimpleExoPlayer.Builder(MainContext).SetTrackSelector(trackSelector).Build();

                DefaultDataSourceFac = new DefaultDataSourceFactory(MainContext, Util.GetUserAgent(MainContext, AppSettings.ApplicationName), BandwidthMeter);
                VideoSurfaceView.UseController = true;
                VideoSurfaceView.Player = VideoPlayer;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private IMediaSource GetMediaSourceFromUrl(Uri uri, string extension, string tag)
        {
            try
            {
                BandwidthMeter = DefaultBandwidthMeter.GetSingletonInstance(MainContext);
                //DefaultDataSourceFactory dataSourceFactory = new DefaultDataSourceFactory(MainContext, Util.GetUserAgent(MainContext, AppSettings.ApplicationName), mBandwidthMeter);
                var buildHttpDataSourceFactory = new DefaultDataSourceFactory(MainContext, BandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(MainContext, AppSettings.ApplicationName)));
                var buildHttpDataSourceFactoryNull = new DefaultDataSourceFactory(MainContext, BandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(MainContext, AppSettings.ApplicationName)));
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

        public void CacheVideosFiles(Uri videoUrl)
        {
            try
            { 
                Cache ??= new SimpleCache(MainContext.CacheDir, new NoOpCacheEvictor(), new ExoDatabaseProvider(MainContext));

                CacheDataSourceFactory ??= new CacheDataSourceFactory(Cache, DefaultDataSourceFac);

                //var dataSpec = new DataSpec(videoUrl, 0, 3000 * 1024, null); //0, 1000 * 1024, null
                //CacheUtil.GetCached(dataSpec, Cache, new MyCacheKeyFactory());
                //string userAgent = Util.GetUserAgent(MainContext, AppSettings.ApplicationName);
                //var bandwidthMeter = new DefaultBandwidthMeter.Builder(MainContext).Build();
                //var defaultDataSourceFactory = new DefaultDataSourceFactory(MainContext, bandwidthMeter, new DefaultHttpDataSourceFactory(userAgent, bandwidthMeter));

                new Thread(() =>
                {
                    try
                    { 
                        //DownloaderConstructorHelper constructorHelper = new DownloaderConstructorHelper(Cache, defaultDataSourceFactory);
                        //IDownloaderFactory factory = new DefaultDownloaderFactory(constructorHelper);

                        var type = Util.InferContentType(videoUrl, VideoUrl?.Path?.Split('.').Last());
                        var typeVideo = type switch
                        {
                            C.TypeSs => DownloadRequest.TypeSs,
                            C.TypeDash => DownloadRequest.TypeDash,
                            C.TypeHls => DownloadRequest.TypeHls,
                            C.TypeOther => DownloadRequest.TypeProgressive,
                            _ => DownloadRequest.TypeProgressive
                        };
                        
                        var downloadManager = new DownloadManager(MainContext , new DefaultDatabaseProvider(new ExoDatabaseProvider(MainContext)), Cache , CacheDataSourceFactory);
                        downloadManager.AddDownload(new DownloadRequest("id", typeVideo, videoUrl, /* streamKeys= */new List<StreamKey>(), null, /* data= */ null));
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }).Start(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private class MyCacheKeyFactory : Java.Lang.Object, ICacheKeyFactory
        {
            public string Key { set; get; }

            public string BuildCacheKey(DataSpec dataSpec)
            {
                Key = dataSpec.Key;
                return dataSpec.Key;
            }
        }
         
        private int GetVisibleVideoSurfaceHeight(int playPosition)
        {
            try
            {
                var at = playPosition - ((LinearLayoutManager)Objects.RequireNonNull(GetLayoutManager())).FindFirstVisibleItemPosition();

                var child = GetChildAt(at);
                if (child == null)
                {
                    return 0;
                }
                var location = new int[2];
                child.GetLocationInWindow(location);
                if (location[1] < 0)
                {
                    return location[1] + VideoSurfaceDefaultHeight;
                }
                else
                {
                    return ScreenDefaultHeight - location[1];
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        private void AddVideoView()
        {
            try
            {
                //var d = MediaContainerLayout.FindViewById<PlayerView>(VideoSurfaceView.Id);
                //if (d == null)
                //{
                MediaContainerLayout.AddView(VideoSurfaceView);
                IsVideoViewAdded = true;
                VideoSurfaceView.RequestFocus();
                VideoSurfaceView.Visibility = ViewStates.Visible;

                //}

                Thumbnail.Visibility = ViewStates.Gone;
                PlayControl.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RemoveVideoView(PlayerView videoView)
        {
            try
            {
                var parent = (ViewGroup)videoView.Parent;
                if (parent == null)
                    return;

                var index = parent.IndexOfChild(videoView);
                if (index < 0)
                    return;

                parent.RemoveViewAt(index);
                IsVideoViewAdded = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StopVideo()
        {
            try
            {
                if (VideoSurfaceView.Player == null) return;
                if (VideoSurfaceView.Player.PlaybackState == IPlayer.StateReady)
                {
                    VideoSurfaceView.Player.PlayWhenReady = false;
                    //VideoSurfaceView.Player.Stop();
                    HomeActivity.GetInstance()?.SetOffWakeLock();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ReleasePlayer()
        {
            try
            {
                StopVideo();

                VideoPlayer?.Release();
                VideoPlayer = null!;

                ViewHolderParent = null!;

                //GC Collect
                GC.Collect();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}