using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Com.Airbnb.Lottie;
using Java.Lang;
using Me.Relex.CircleIndicatorLib;
using Newtonsoft.Json;
using PixelPhoto.Activities.Comment.Adapters;
using PixelPhoto.Activities.Posts.Adapters;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.Library.Anjo.SuperTextLibrary;
using PixelPhoto.Library.Ui;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.Posts.page
{
    public class GlobalPostViewerFragment : Fragment,MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView ImageDisplay;
        private ImageView UserAvatar, LikeIcon, CommentIcon, Favicon, ShareIcon, MoreIcon;
        private TextView Fullname, LikeCount, TimeTextView, ViewCommentsButton;
        public TextView CommentCount;
        public SuperTextView Description;
        private RecyclerView CommentRecyclerView; 
        private HomeActivity MainContext;
        private StReadMoreOption ReadMoreOption;
        public CommentsAdapter CommentsAdapter;
        private string Type, Json, PostId;
        private SocialIoClickListeners ClickListeners;
        private LinearLayout StubLoader;
        private LottieAnimationView LikeAnimationView, FavAnimationView, TapLikeAnimation;
        private VideoController VideoActionsController;
        private PostsObject DataObject;
        private NativeFeedType PostType;
        private CommentObject CommentObject;
        private View SubPostView;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            MainContext = (HomeActivity)Activity ?? HomeActivity.GetInstance();
            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.MultiPostView, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);
                 
                Type = Arguments.GetString("type") ?? "";
                Json = Arguments.GetString("object") ?? "";
                PostId = Arguments.GetString("PostId") ?? "";

                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);
                StubLayoutLoader();
                AddOrRemoveEvent(view ,true);
                ReadPassedData(Type);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);

            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public override void OnDestroy()
        {
            try
            {
                switch (PostType)
                {
                    case NativeFeedType.Video:
                        VideoActionsController?.ReleaseVideo();
                        break;
                }

                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnStop()
        {
            try
            {
                switch (PostType)
                {
                    case NativeFeedType.Video:
                    {
                        if (VideoActionsController?.Player != null)
                            VideoActionsController.Player.PlayWhenReady = false;
                        break;
                    }
                }

                base.OnStop();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Menu
         
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    MainContext.FragmentNavigatorBack();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                Fullname = view.FindViewById<TextView>(Resource.Id.username);
                UserAvatar = view.FindViewById<ImageView>(Resource.Id.userAvatar);
                MoreIcon = view.FindViewById<ImageView>(Resource.Id.moreicon);
                LikeIcon = view.FindViewById<ImageView>(Resource.Id.Like);
                CommentIcon = view.FindViewById<ImageView>(Resource.Id.Comment);
                Favicon = view.FindViewById<ImageView>(Resource.Id.fav);
                ShareIcon = view.FindViewById<ImageView>(Resource.Id.share);

                //Anim
                LikeAnimationView = view.FindViewById<LottieAnimationView>(Resource.Id.animation_view_of_like);
                FavAnimationView = view.FindViewById<LottieAnimationView>(Resource.Id.animation_view_of_fav);
                LikeAnimationView.SetAnimation("LikeHeart.json");
                FavAnimationView.SetAnimation("FavAnim.json");

                TapLikeAnimation = view.FindViewById<LottieAnimationView>(Resource.Id.animation_like);
                TapLikeAnimation.SetAnimation("LikeHeart.json");

                Description = view.FindViewById<SuperTextView>(Resource.Id.description);
                Description?.SetTextInfo(Description);

                TimeTextView = view.FindViewById<TextView>(Resource.Id.time_text);
                ViewCommentsButton = view.FindViewById<TextView>(Resource.Id.ViewMoreComment);
                LikeCount = view.FindViewById<TextView>(Resource.Id.Likecount);
                CommentCount = view.FindViewById<TextView>(Resource.Id.Commentcount);
                CommentRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.RecylerComment);
                 
                StubLoader = view.FindViewById<LinearLayout>(Resource.Id.layout_stub);

                //Set Adapter Data 
                CommentsAdapter = new CommentsAdapter(Activity);
                var mLayoutManager = new LinearLayoutManager(Activity);
                CommentRecyclerView.SetLayoutManager(mLayoutManager);
                CommentRecyclerView.SetAdapter(CommentsAdapter);
                CommentRecyclerView.NestedScrollingEnabled = false;
                CommentsAdapter.AvatarClick += CommentsAdapterOnAvatarClick;
                CommentsAdapter.LikeClick += CommentsAdapterOnLikeClick;
                CommentsAdapter.ReplyClick += CommentsAdapterOnReplyClick;
                CommentsAdapter.MoreClick += CommentsAdapterOnMoreClick;

                ReadMoreOption = new StReadMoreOption.Builder()
                    .TextLength(200, StReadMoreOption.TypeCharacter)
                    .MoreLabel(MainContext.GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(MainContext.GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar(View view)
        {
            try
            {
                var toolBar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                MainContext.SetToolBar(toolBar, " ");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(View view ,bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    if (!string.IsNullOrEmpty(Json))
                    { 
                        ClickListeners = new SocialIoClickListeners(Activity);

                        if (!CommentCount.HasOnClickListeners)
                            CommentCount.Click += (sender, e) => ClickListeners.OnCommentPostClick(new GlobalClickEventArgs {  NewsFeedClass = DataObject }, "ImagePost");

                        if (!LikeCount.HasOnClickListeners)
                             LikeCount.Click += (sender, e) => ClickListeners.OnLikedPostClick(new LikeNewsFeedClickEventArgs { View  = view, NewsFeedClass = DataObject, LikeImgButton = LikeIcon });

                        if (!LikeIcon.HasOnClickListeners)
                        {
                            LikeIcon.Click += (sender, e) => ClickListeners.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = view, NewsFeedClass = DataObject, LikeImgButton = LikeIcon, LikeAnimationView = LikeAnimationView });
                        }

                        if (!Favicon.HasOnClickListeners)
                            Favicon.Click += (sender, e) => ClickListeners.OnFavNewsFeedClick(new FavNewsFeedClickEventArgs { NewsFeedClass = DataObject, FavImgButton = Favicon, FavAnimationView = FavAnimationView });

                        if (!UserAvatar.HasOnClickListeners)
                            UserAvatar.Click += (sender, e) => ClickListeners.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { View = view, NewsFeedClass = DataObject, Image = UserAvatar,  });
                      
                        if (!Fullname.HasOnClickListeners)
                            Fullname.Click += (sender, e) => ClickListeners.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { View = view, NewsFeedClass = DataObject, Image = UserAvatar,  });

                        if (!CommentIcon.HasOnClickListeners)
                            CommentIcon.Click += (sender, e) => ClickListeners.OnCommentClick(new GlobalClickEventArgs { NewsFeedClass = DataObject,  }, "ImagePost");

                        if (!ViewCommentsButton.HasOnClickListeners)
                            ViewCommentsButton.Click += (sender, e) => ClickListeners.OnCommentClick(new GlobalClickEventArgs { NewsFeedClass = DataObject,  }, "ImagePost");

                        if (!MoreIcon.HasOnClickListeners)
                            MoreIcon.Click += (sender, e) => ClickListeners.OnMoreClick(new GlobalClickEventArgs { NewsFeedClass = DataObject,  }, false, "ImagePost");

                        if (!ShareIcon.HasOnClickListeners)
                            ShareIcon.Click += (sender, e) => ClickListeners.OnShareClick(new GlobalClickEventArgs { NewsFeedClass = DataObject,  });

                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Get Data Post


        private void StubLayoutLoader()
        {
            try
            {
                DataObject = JsonConvert.DeserializeObject<PostsObject>(Json);
                if (DataObject != null)
                {
                    PostType = NewsFeedAdapter.GetPostType(DataObject); 
                    switch (PostType)
                    {
                        case NativeFeedType.Video:
                            SubPostView = LayoutInflater.From(MainContext)?.Inflate(Resource.Layout.ViewSubOneVideo, StubLoader, false);
                            StubLoader.RemoveAllViews();
                            StubLoader.AddView(SubPostView);
                            LoadPostTypeVideo(SubPostView);
                            break;
                        case NativeFeedType.Photo:
                        case NativeFeedType.Gif:
                            SubPostView = LayoutInflater.From(MainContext)?.Inflate(Resource.Layout.ViewSubOneImage, StubLoader, false);
                            StubLoader.RemoveAllViews();
                            StubLoader.AddView(SubPostView);
                            LoadPostTypeImage(SubPostView, false);
                            break;
                        case NativeFeedType.MultiPhoto:
                        case NativeFeedType.MultiPhoto2:
                        case NativeFeedType.MultiPhoto3:
                        case NativeFeedType.MultiPhoto4:
                        case NativeFeedType.MultiPhoto5:
                            SubPostView = LayoutInflater.From(MainContext)?.Inflate(Resource.Layout.ViewSubMultiImages, StubLoader, false);
                            StubLoader.RemoveAllViews();
                            StubLoader.AddView(SubPostView);
                            LoadPostTypeImage(SubPostView, true);
                            break;
                        case NativeFeedType.Youtube:
                            SubPostView = LayoutInflater.From(MainContext)?.Inflate(Resource.Layout.ViewSubYoutube, StubLoader, false);
                            StubLoader.RemoveAllViews();
                            StubLoader.AddView(SubPostView);
                            LoadPostTypeYoutube(StubLoader);
                            break;
                        case NativeFeedType.Vimeo:
                        case NativeFeedType.Dailymotion:
                        case NativeFeedType.PlayTube:
                            SubPostView = LayoutInflater.From(MainContext)?.Inflate(Resource.Layout.ViewSubFetched, StubLoader, false);
                            StubLoader.RemoveAllViews();
                            StubLoader.AddView(SubPostView);
                            LoadPostTypeFetched(StubLoader, PostType);
                            break;
                        case NativeFeedType.Funding:
                        case NativeFeedType.AdMob1:
                        case NativeFeedType.AdMob2:
                        case NativeFeedType.FbNativeAds:
                        case NativeFeedType.Nona:
                            break;
                    }
                }
                else
                {
                    StartApiService();
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadPostTypeVideo(View inflated)
        {
            try
            {
                //var videoPlayer = inflated.FindViewById<PlayerView>(Resource.Id.player_view);
                //var controlView = videoPlayer.FindViewById<PlayerControlView>(Resource.Id.exo_controller);
                var videoProgressBar = inflated.FindViewById<ProgressBar>(Resource.Id.progress_bar);

                videoProgressBar.Visibility = ViewStates.Gone;
                VideoActionsController = new VideoController(inflated, "Viewer_Video");

                if (VideoActionsController.Player == null)
                    VideoActionsController?.PlayVideo(DataObject.MediaSet[0]?.File);

                VideoActionsController.Player.PlayWhenReady = true;

                var postCardView = inflated.FindViewById<CardView>(Resource.Id.contentLayout);
                postCardView.Click += PostCardViewOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadPostTypeImage(View inflated, bool isMulti)
        {
            try
            {
                if (!isMulti) //One image
                {
                    ImageDisplay = inflated.FindViewById<ImageView>(Resource.Id.image);
                    var glideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Bumptech.Glide.Priority.High).Transform(new MultiTransformation(new RoundedCorners(50)));

                    var url = !string.IsNullOrEmpty(DataObject.MediaSet[0]?.File) ? DataObject.MediaSet[0].File : DataObject.MediaSet[0].Extra;
                    if (url.Contains(".gif"))
                        Glide.With(Context).Load(url).Apply(new RequestOptions().Error(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Bumptech.Glide.Priority.High).Transform(new MultiTransformation(new CenterCrop(), new RoundedCorners(50)))).Into(ImageDisplay);
                    else
                        Glide.With(Context).Load(url).Apply(glideRequestOptions).Into(ImageDisplay);

                    //GlideImageLoader.LoadImage(Activity, !string.IsNullOrEmpty(DataObject.MediaSet[0].Extra) ? DataObject.MediaSet[0].Extra : DataObject.MediaSet[0].File, ImageDisplay, ImageStyle.RoundedCrop, ImagePlaceholders.Color);

                    var postCardView = inflated.FindViewById<LinearLayout>(Resource.Id.contentLayout);
                    postCardView.Click += PostCardViewOnClick;

                }
                else //MultiImages
                {
                    var viewPagerView = inflated.FindViewById<ViewPager>(Resource.Id.pager);
                    var circleIndicatorView = inflated.FindViewById<CircleIndicator>(Resource.Id.indicator);
                    var list = DataObject.MediaSet.Select(image => image.File).ToList();

                    viewPagerView.Adapter = new MultiImagePagerAdapter(Activity, list);
                    viewPagerView.CurrentItem = 0;
                    circleIndicatorView.SetViewPager(viewPagerView);

                    var postCardView = inflated.FindViewById<FrameLayout>(Resource.Id.contentLayout);
                    postCardView.Click += PostCardViewOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadPostTypeYoutube(View inflated)
        {
            try
            {
                //wael 

                //var youTubeFragment = new YouTubePlayerSupportFragment();
                //ChildFragmentManager.BeginTransaction().Add(inflated.Id, youTubeFragment, youTubeFragment.Id.ToString() + DateTime.Now).Commit();
                //YouTubePlayerEventLoader = new YoutubePlayerController(Activity, DataObject.Youtube);
                //youTubeFragment.Initialize(AppSettings.YoutubeKey, YouTubePlayerEventLoader);

                //var postCardView = inflated.FindViewById<FrameLayout>(Resource.Id.root);
                //postCardView.Click += PostCardViewOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadPostTypeFetched(View inflated, NativeFeedType feedType)
        {
            try
            {
                var webView = inflated.FindViewById<PixelWebView>(Resource.Id.webview);
                webView.Visibility = ViewStates.Visible;
                switch (feedType)
                {
                    case NativeFeedType.Vimeo:
                        {
                            var fullEmbedUrl = "https://player.vimeo.com/video/" + DataObject.Vimeo + "?autoplay=1";

                            var vc = webView.LayoutParameters;
                            vc.Height = 700;
                            webView.LayoutParameters = vc;

                            webView?.LoadUrl(fullEmbedUrl);
                            break;
                        }
                    case NativeFeedType.Dailymotion:
                        {
                            var fullEmbedUrl = "https://www.dailymotion.com/embed/video/" + DataObject.Dailymotion + "?autoplay=1";

                            var vc = webView.LayoutParameters;
                            vc.Height = 600;
                            webView.LayoutParameters = vc;

                            webView?.LoadUrl(fullEmbedUrl);
                            break;
                        }
                    case NativeFeedType.PlayTube:
                        {
                            var playTubeUrl = ListUtils.SettingsSiteList?.PlaytubeUrl;

                            var fullEmbedUrl = playTubeUrl + "/embed/" + DataObject.Playtube;

                            var vc = webView.LayoutParameters;
                            vc.Height = 600;
                            webView.LayoutParameters = vc;

                            webView?.LoadUrl(fullEmbedUrl);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void ReadPassedData(string type)
        {
            try
            { 
                if (type == "ExploreAdapter")
                {
                    var time = Methods.Time.TimeAgo(Convert.ToInt32(DataObject.Time), false);
                    DisplayData(DataObject.Username, DataObject.Description, DataObject.Avatar, time , DataObject.Likes, DataObject.IsLiked, DataObject.IsSaved, DataObject.Votes);

                    if (DataObject.Comments != null && DataObject.Comments.Count > 0)
                    {
                        CommentsAdapter.CommentList = new ObservableCollection<CommentObject>(DataObject.Comments);
                        CommentsAdapter.NotifyDataSetChanged();
                    } 
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisplayData(string fullname, string description, string avatar, string time, long likesCount, bool? isLiked, bool? isSaved, long votesCount)
        {
            try
            {
                Fullname.Text = fullname;
                TimeTextView.Text = time;
                 
                if (DataObject.UserData.Verified == "1")
                    Fullname.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_small_vector, 0);

                if (DataObject.UserData.BusinessAccount == "1")
                    Fullname.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_dolar_small_vector, 0);


                GlideImageLoader.LoadImage(Activity, avatar, UserAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                if (string.IsNullOrEmpty(description) || string.IsNullOrWhiteSpace(description))
                {
                    Description.Visibility = ViewStates.Gone;
                }
                else
                {
                    ReadMoreOption.AddReadMoreTo(Description, new Java.Lang.String(description));
                    Description.Visibility = ViewStates.Visible;
                }
                 
                LikeCount.Text = likesCount + " " + " " + Activity.GetText(Resource.String.Lbl_Likes);
                CommentCount.Text = votesCount + " " + Activity.GetString(Resource.String.Lbl_Comments);

                if (isLiked != null && isLiked.Value)
                    LikeIcon.SetImageResource(Resource.Drawable.icon_heart_filled_post_vector);

                LikeIcon.Tag = isLiked != null && isLiked.Value ? "Like" : "Liked";
                 
                if (isSaved != null && !isSaved.Value)
                {
                    Favicon.SetImageResource(Resource.Drawable.icon_fav_post_vector); 
                }
                else
                {
                    Favicon.SetImageResource(Resource.Drawable.icon_star_filled_post_vector); 
                }

                Favicon.Tag = isSaved != null && isSaved.Value ? "Add" : "Added";
                 
                if (votesCount <= 4)
                {
                    ViewCommentsButton.Visibility = ViewStates.Gone;
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetPostById });
        }

        private async Task GetPostById()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    (var apiStatus, var respond) = await RequestsAsync.Post.FetchPostById(PostId);
                    if (apiStatus == 200)
                    {
                        if (respond is FetchPostsObject result)
                        {
                            if (result.Data != null)
                            {
                                DataObject = result.Data; 
                                PostType = NewsFeedAdapter.GetPostType(DataObject);
                                switch (PostType)
                                {
                                    case NativeFeedType.Video:
                                        SubPostView = LayoutInflater.From(MainContext)?.Inflate(Resource.Layout.ViewSubOneVideo, StubLoader, false);
                                        StubLoader.RemoveAllViews();
                                        StubLoader.AddView(SubPostView);
                                        LoadPostTypeVideo(SubPostView);
                                        break;
                                    case NativeFeedType.Photo:
                                    case NativeFeedType.Gif:
                                        SubPostView = LayoutInflater.From(MainContext)?.Inflate(Resource.Layout.ViewSubOneImage, StubLoader, false);
                                        StubLoader.RemoveAllViews();
                                        StubLoader.AddView(SubPostView);
                                        LoadPostTypeImage(SubPostView, false);
                                        break;
                                    case NativeFeedType.MultiPhoto:
                                    case NativeFeedType.MultiPhoto2:
                                    case NativeFeedType.MultiPhoto3:
                                    case NativeFeedType.MultiPhoto4:
                                    case NativeFeedType.MultiPhoto5:
                                        SubPostView = LayoutInflater.From(MainContext)?.Inflate(Resource.Layout.ViewSubMultiImages, StubLoader, false);
                                        StubLoader.RemoveAllViews();
                                        StubLoader.AddView(SubPostView);
                                        LoadPostTypeImage(SubPostView, true);
                                        break;
                                    case NativeFeedType.Youtube:
                                        SubPostView = LayoutInflater.From(MainContext)?.Inflate(Resource.Layout.ViewSubYoutube, StubLoader, false);
                                        StubLoader.RemoveAllViews();
                                        StubLoader.AddView(SubPostView);
                                        LoadPostTypeYoutube(StubLoader);
                                        break;
                                    case NativeFeedType.Vimeo:
                                    case NativeFeedType.Dailymotion:
                                    case NativeFeedType.PlayTube:
                                        SubPostView = LayoutInflater.From(MainContext)?.Inflate(Resource.Layout.ViewSubFetched, StubLoader, false);
                                        StubLoader.RemoveAllViews();
                                        StubLoader.AddView(SubPostView);
                                        LoadPostTypeFetched(StubLoader, PostType);
                                        break;
                                    case NativeFeedType.Funding:
                                    case NativeFeedType.AdMob1:
                                    case NativeFeedType.AdMob2:
                                    case NativeFeedType.FbNativeAds:
                                    case NativeFeedType.Nona:
                                        break;
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void PostCardViewOnClick(object sender, EventArgs e)
        { 
            try
            {
                if (DataObject?.IsLiked != null && !DataObject.IsLiked.Value)
                {
                    if (SystemClock.ElapsedRealtime() - UserDetails.TimestampLastClick < AppSettings.DoubleClickQualificationSpanInMillis)
                    { 
                        TapLikeAnimation?.PlayAnimation();

                        ClickListeners.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs {  LikeImgButton = LikeIcon, LikeAnimationView = LikeAnimationView , NewsFeedClass = DataObject });
                    }
                    UserDetails.TimestampLastClick = SystemClock.ElapsedRealtime();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event More
        private void CommentsAdapterOnMoreClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            {
               CommentObject = CommentsAdapter.GetItem(e.Position);
                if (CommentObject != null)
                {
                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Copy));

                    if (CommentObject.IsOwner)
                        arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Delete));

                    dialogList.Title(Activity.GetText(Resource.String.Lbl_More));
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(Activity.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Open Reply
        private void CommentsAdapterOnReplyClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            {
                ClickListeners ??= new SocialIoClickListeners(Activity);
                var item = CommentsAdapter.GetItem(e.Position);
                if (item != null)
                    ClickListeners.CommentReplyPostClick(new CommentReplyClickEventArgs { CommentObject = item, Position = e.Position, View = e.View });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Like 
        private void CommentsAdapterOnLikeClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            {
                var item = CommentsAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (!Methods.CheckConnectivity())
                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    else
                    {
                        var likeIcon = e.View.FindViewById<ImageView>(Resource.Id.likeIcon);
                        var likeCount = e.View.FindViewById<TextView>(Resource.Id.Like);

                        var interpolator = new MyBounceInterpolator(0.2, 20);
                        var animationScale = AnimationUtils.LoadAnimation(Activity, Resource.Animation.scale);
                        animationScale.Interpolator = interpolator;

                        item.IsLiked = item.IsLiked switch
                        {
                            1 => 0,
                            0 => 1,
                            _ => item.IsLiked
                        };
                        likeIcon.SetImageResource(item.IsLiked == 1 ? Resource.Drawable.ic_action_like_2 : Resource.Drawable.ic_action_like_1);
                        likeIcon.StartAnimation(animationScale);

                        if (item.IsLiked == 1)
                        {
                            if (UserDetails.SoundControl)
                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("reaction.mp3");

                            var x = item.Likes + 1;
                            likeCount.Text = Activity.GetString(Resource.String.Lbl_Likes) + " " + "(" + x + ")";
                        }
                        else
                        {
                            var x = item.Likes;

                            if (x > 0)
                                x--;
                            else
                                x = 0;

                            item.Likes = x;

                            likeCount.Text = Activity.GetString(Resource.String.Lbl_Likes) + " " + "(" + item.Likes + ")";
                        }

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Post.LikeComment(item.Id.ToString()) });
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Open Profile User 
        private void CommentsAdapterOnAvatarClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            {
                switch (PostType)
                {
                    case NativeFeedType.Video:
                    {
                        if (VideoActionsController.Player != null)
                            VideoActionsController.Player.PlayWhenReady = false;
                        break;
                    }
                }
                 
                var item = CommentsAdapter.GetItem(e.Position);
                if (item != null)
                {
                    AppTools.OpenProfile(Activity, item.UserId.ToString(), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public override void OnHiddenChanged(bool hidden)
        {
            try
            {
                if (hidden)
                {
                    switch (PostType)
                    {
                        case NativeFeedType.Video:
                        {
                            if (VideoActionsController.Player != null)
                                VideoActionsController.Player.PlayWhenReady = false;
                            break;
                        }
                    }
                }

                base.OnHiddenChanged(hidden);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_Copy))
                {
                    Methods.CopyToClipboard(Activity, Methods.FunString.DecodeString(CommentObject.Text));
                }
                else if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_Delete))
                {
                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                    dialog.Title(MainContext.GetText(Resource.String.Lbl_DeleteComment));
                    dialog.Content(MainContext.GetText(Resource.String.Lbl_AreYouSureDeleteComment));
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                    {
                        try
                        {
                            if (!Methods.CheckConnectivity())
                            {
                                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                                return;
                            }

                            var dataGlobal = CommentsAdapter?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                            if (dataGlobal != null)
                            {
                                var index = CommentsAdapter.CommentList.IndexOf(dataGlobal);
                                if (index > -1)
                                {
                                    CommentsAdapter.CommentList.RemoveAt(index);
                                    CommentsAdapter.NotifyItemRemoved(index);
                                }
                            }

                            Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CommentSuccessfullyDeleted), ToastLength.Short)?.Show();
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Post.DeleteComment(CommentObject.Id.ToString()) });
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

    }
}