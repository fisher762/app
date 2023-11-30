using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager.Widget;
using Com.Airbnb.Lottie; 
using Me.Relex.CircleIndicatorLib;
using Newtonsoft.Json;
using PixelPhoto.Activities.Funding;
using PixelPhoto.Activities.Funding.Adapters;
using PixelPhoto.Activities.Posts.Extras;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.Library.Anjo.SuperTextLibrary;
using PixelPhoto.Library.Ui;
using PixelPhotoClient.GlobalClass;

namespace PixelPhoto.Activities.Posts.Adapters
{
    public class Holders
    {
        public class PostAdapterClickEventArgs : EventArgs
        {
            public View View { get; set; }
            public int Position { get; set; }
        }


        #region ViewHolder

        public class GlobalPostViews : Java.Lang.Object
        {
            public View MainView { get; private set; }
            public ImageView UserAvatar { get; private set; }
            public TextView Username { get; private set; }
            public ImageView Moreicon { get; private set; }
            public ImageView Likeicon { get; private set; }
            public ImageView Commenticon { get; private set; }
            public ImageView Favicon { get; private set; }
            public ImageView ShareIcon { get; private set; }
            public SuperTextView Description { get; private set; }
            public TextView TimeText { get; private set; }
            public TextView CommentCount { get; private set; }
            public TextView ViewMoreComment { get; private set; }
            public TextView ViewCount { get; private set; }
            public TextView LikeCount { get; private set; }
            public TextView IsPromoted { get; private set; }
            public LottieAnimationView TapLikeAnimation { get; private set; }
            public LottieAnimationView LikeAnimationView { get; private set; }
            public LottieAnimationView FavAnimationView { get; private set; }

            public GlobalPostViews(View itemView)
            {
                try
                {
                    MainView = itemView;
                    //<!--Including User Post Owner Layout -->
                    UserAvatar = itemView.FindViewById<ImageView>(Resource.Id.userAvatar);
                    Username = itemView.FindViewById<TextView>(Resource.Id.username);
                    TimeText = itemView.FindViewById<TextView>(Resource.Id.time_text);
                    Moreicon = itemView.FindViewById<ImageView>(Resource.Id.moreicon);

                    //<!--Including Post Actions -->
                    Likeicon = itemView.FindViewById<ImageView>(Resource.Id.Like);
                    ShareIcon = itemView.FindViewById<ImageView>(Resource.Id.share);
                    Commenticon = itemView.FindViewById<ImageView>(Resource.Id.Comment);
                    Favicon = itemView.FindViewById<ImageView>(Resource.Id.fav);
                    CommentCount = itemView.FindViewById<TextView>(Resource.Id.Commentcount);
                    ViewMoreComment = itemView.FindViewById<TextView>(Resource.Id.ViewMoreComment);
                    ViewCount = itemView.FindViewById<TextView>(Resource.Id.ViewCount);
                    LikeCount = itemView.FindViewById<TextView>(Resource.Id.Likecount);
                    Description = itemView.FindViewById<SuperTextView>(Resource.Id.description);
                    Description?.SetTextInfo(Description);
                    //Description.SetAutoLinkOnClickListener(this, new Dictionary<string, string>()); //wael

                    IsPromoted = itemView.FindViewById<TextView>(Resource.Id.promoted);
                    TapLikeAnimation  = itemView.FindViewById<LottieAnimationView>(Resource.Id.animation_like);
                    LikeAnimationView = itemView.FindViewById<LottieAnimationView>(Resource.Id.animation_view_of_like);
                    FavAnimationView = itemView.FindViewById<LottieAnimationView>(Resource.Id.animation_view_of_fav);
                     
                    TapLikeAnimation.SetAnimation("LikeHeart.json");
                    LikeAnimationView.SetAnimation("LikeHeart.json");
                    FavAnimationView.SetAnimation("FavAnim.json");
                    Favicon.Tag = "Add"; 
                }
                catch (Exception e)
                {
                   Methods.DisplayReportResultTrack(e);  
                }
            } 
        }

        public class VideoAdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            #region Variables Basic

            public View MainView { get; private set; }

            public ImageView VideoImage { get; private set; }
            public FrameLayout MediaContainerLayout { get; private set; }
            public ProgressBar VideoProgressBar { get; private set; }
            public ImageView PlayControl { get; private set; }


            private readonly IOnPostItemClickListener IoClickListeners;
            private readonly NewsFeedAdapter PostAdapter;

            public GlobalPostViews GlobalPostViews { get; private set; }
            public string VideoUrl { get; set; }

            #endregion Variables Basic

            public VideoAdapterViewHolder(View itemView, Action<PostAdapterClickEventArgs> clickListener, Action<PostAdapterClickEventArgs> longClickListener
                , IOnPostItemClickListener socialIoClickListeners , NewsFeedAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    itemView.Tag = "Video";

                    PostAdapter = postAdapter;
                    IoClickListeners = socialIoClickListeners;
                    GlobalPostViews = new GlobalPostViews(itemView);

                    GlobalPostViews.ViewCount.Visibility = ViewStates.Visible;

                    GlobalPostViews.Likeicon.SetOnClickListener(this);
                    GlobalPostViews.UserAvatar.SetOnClickListener(this);
                    GlobalPostViews.Favicon.SetOnClickListener(this);
                    GlobalPostViews.LikeCount.SetOnClickListener(this);
                    GlobalPostViews.CommentCount.SetOnClickListener(this);
                    GlobalPostViews.Commenticon.SetOnClickListener(this);
                    GlobalPostViews.ViewMoreComment.SetOnClickListener(this);
                    GlobalPostViews.Moreicon.SetOnClickListener(this);
                    GlobalPostViews.ShareIcon.SetOnClickListener(this);
                    GlobalPostViews.Username.SetOnClickListener(this);
                     
                    MediaContainerLayout = itemView.FindViewById<FrameLayout>(Resource.Id.media_container);
                    VideoImage = itemView.FindViewById<ImageView>(Resource.Id.image);
                    VideoProgressBar = itemView.FindViewById<ProgressBar>(Resource.Id.progressBar);
                    PlayControl = itemView.FindViewById<ImageView>(Resource.Id.Play_control);

                    PlayControl.SetOnClickListener(this);

                    //Create an Event
                    itemView.Click += (sender, e) => clickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                    itemView.LongClick += (sender, e) => longClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                }
                catch (Exception e)
                {
                   Methods.DisplayReportResultTrack(e); 
                }
            }

            public void OnClick(View v)
            { 
                try
                {
                    var pos = AdapterPosition;
                    if (pos != RecyclerView.NoPosition)
                    { 
                        if (v.Id == GlobalPostViews.Likeicon.Id)
                            IoClickListeners.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs {  View = MainView , LikeImgButton = GlobalPostViews.Likeicon, LikeAnimationView = GlobalPostViews.LikeAnimationView }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.UserAvatar.Id || v.Id == GlobalPostViews.Username.Id)
                            IoClickListeners.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { View = MainView, Image = GlobalPostViews.UserAvatar }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Favicon.Id)
                            IoClickListeners.OnFavNewsFeedClick(new FavNewsFeedClickEventArgs { FavImgButton = GlobalPostViews.Favicon, FavAnimationView = GlobalPostViews.FavAnimationView }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.LikeCount.Id)
                            IoClickListeners.OnLikedLabelPostClick(new LikeNewsFeedClickEventArgs { View = MainView, LikeButton = GlobalPostViews.LikeCount }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.CommentCount.Id)
                            IoClickListeners.OnCommentPostClick(new GlobalClickEventArgs {  }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Commenticon.Id)
                            IoClickListeners.OnCommentClick(new GlobalClickEventArgs {  }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.ViewMoreComment.Id)
                            IoClickListeners.OnCommentClick(new GlobalClickEventArgs {  }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Moreicon.Id)
                            IoClickListeners.OnMoreClick(new GlobalClickEventArgs {  }, true, AdapterPosition);
                        else if (v.Id == GlobalPostViews.ShareIcon.Id)
                            IoClickListeners.OnShareClick(new GlobalClickEventArgs {  }, AdapterPosition);

                        else if (v.Id == PlayControl.Id)
                        {
                            var item = PostAdapter.PostList[AdapterPosition]; 
                            PRecyclerView.GetInstance()?.PlayVideo(!PRecyclerView.GetInstance().CanScrollVertically(1), this, item);
                        } 
                    }
                }
                catch (Exception e)
                {
                     Methods.DisplayReportResultTrack(e); 
                }
            }
        }

        public class YoutubeAdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            private readonly NewsFeedAdapter PostAdapter;

            #region Variables Basic

            public View MainView { get; private set; }
            public ImageView Image { get; private set; }
            public ImageView PlayButton { get; private set; }

            private readonly IOnPostItemClickListener IoClickListeners;
            public GlobalPostViews GlobalPostViews { get; private set; }
            #endregion Variables Basic

            public YoutubeAdapterViewHolder(View itemView, Action<PostAdapterClickEventArgs> clickListener, Action<PostAdapterClickEventArgs> longClickListener
                , IOnPostItemClickListener socialIoClickListeners, NewsFeedAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    itemView.Tag = "Video";

                    IoClickListeners = socialIoClickListeners;
                    PostAdapter = postAdapter;
                    GlobalPostViews = new GlobalPostViews(itemView);

                    GlobalPostViews.Likeicon.SetOnClickListener(this);
                    GlobalPostViews.UserAvatar.SetOnClickListener(this);
                    GlobalPostViews.Favicon.SetOnClickListener(this);
                    GlobalPostViews.LikeCount.SetOnClickListener(this);
                    GlobalPostViews.CommentCount.SetOnClickListener(this);
                    GlobalPostViews.Commenticon.SetOnClickListener(this);
                    GlobalPostViews.ViewMoreComment.SetOnClickListener(this);
                    GlobalPostViews.Moreicon.SetOnClickListener(this);
                    GlobalPostViews.ShareIcon.SetOnClickListener(this);
                    GlobalPostViews.Username.SetOnClickListener(this);

                    Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                    PlayButton = itemView.FindViewById<ImageView>(Resource.Id.playbutton);

                    PlayButton.SetOnClickListener(this);

                    //Create an Event
                    itemView.Click += (sender, e) => clickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                    itemView.LongClick += (sender, e) => longClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                    
                }
                catch (Exception e)
                {
                   Methods.DisplayReportResultTrack(e); 
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    var pos = AdapterPosition;
                    if (pos != RecyclerView.NoPosition)
                    {
                        if (v.Id == GlobalPostViews.Likeicon.Id)
                            IoClickListeners.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = MainView, LikeImgButton = GlobalPostViews.Likeicon, LikeAnimationView = GlobalPostViews.LikeAnimationView }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.UserAvatar.Id || v.Id == GlobalPostViews.Username.Id)
                            IoClickListeners.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { View = MainView, Image = GlobalPostViews.UserAvatar }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Favicon.Id)
                            IoClickListeners.OnFavNewsFeedClick(new FavNewsFeedClickEventArgs { FavImgButton = GlobalPostViews.Favicon, FavAnimationView = GlobalPostViews.FavAnimationView }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.LikeCount.Id)
                            IoClickListeners.OnLikedLabelPostClick(new LikeNewsFeedClickEventArgs { View = MainView, LikeButton = GlobalPostViews.LikeCount }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.CommentCount.Id)
                            IoClickListeners.OnCommentPostClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Commenticon.Id)
                            IoClickListeners.OnCommentClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.ViewMoreComment.Id)
                            IoClickListeners.OnCommentClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Moreicon.Id)
                            IoClickListeners.OnMoreClick(new GlobalClickEventArgs { }, true, AdapterPosition);
                        else if (v.Id == GlobalPostViews.ShareIcon.Id)
                            IoClickListeners.OnShareClick(new GlobalClickEventArgs { }, AdapterPosition);

                        else if (v.Id == PlayButton.Id)
                        {
                            var item = PostAdapter.PostList[AdapterPosition];
                            PostAdapter.ClickListeners.OnPlayYoutubeButtonClicked(new YoutubeVideoClickEventArgs {Holder = this, NewsFeedClass = item, Position = AdapterPosition,  });
                        } 
                    }
                }
                catch (Exception e)
                {
                     Methods.DisplayReportResultTrack(e); 
                }
            }
        }

        public class Photo2AdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        { 
            public View MainView { get; private set; }

            public ImageView Image { get; private set; }
            public ImageView Image2 { get; private set; }
            public ImageView Image3 { get; private set; }
            public ImageView Image4 { get; private set; }
            public TextView CountImageLabel { get; private set; }

            private readonly IOnPostItemClickListener IoClickListeners;
            public GlobalPostViews GlobalPostViews { get; private set; }

            public Photo2AdapterViewHolder(View itemView, Action<PostAdapterClickEventArgs> clickListener, Action<PostAdapterClickEventArgs> longClickListener, IOnPostItemClickListener socialIoClickListeners , int count) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    itemView.SetLayerType(LayerType.Hardware, null);
                     
                    itemView.Tag = "Image";
                    IoClickListeners = socialIoClickListeners;
                    GlobalPostViews = new GlobalPostViews(itemView);

                    GlobalPostViews.Likeicon.SetOnClickListener(this);
                    GlobalPostViews.UserAvatar.SetOnClickListener(this);
                    GlobalPostViews.Favicon.SetOnClickListener(this);
                    GlobalPostViews.LikeCount.SetOnClickListener(this);
                    GlobalPostViews.CommentCount.SetOnClickListener(this);
                    GlobalPostViews.Commenticon.SetOnClickListener(this);
                    GlobalPostViews.ViewMoreComment.SetOnClickListener(this);
                    GlobalPostViews.Moreicon.SetOnClickListener(this);
                    GlobalPostViews.ShareIcon.SetOnClickListener(this);
                    GlobalPostViews.Username.SetOnClickListener(this);

                    switch (count)
                    {
                        case 2:
                            Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                            Image2 = itemView.FindViewById<ImageView>(Resource.Id.image2);
                            break;
                        case 3:
                            Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                            Image2 = itemView.FindViewById<ImageView>(Resource.Id.image2);
                            Image3 = itemView.FindViewById<ImageView>(Resource.Id.image3);
                            break;
                        case 4:
                            Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                            Image2 = itemView.FindViewById<ImageView>(Resource.Id.image2);
                            Image3 = itemView.FindViewById<ImageView>(Resource.Id.image3);
                            Image4 = itemView.FindViewById<ImageView>(Resource.Id.image4);
                            break;
                        case 5:
                            Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                            Image2 = itemView.FindViewById<ImageView>(Resource.Id.image2);
                            Image3 = itemView.FindViewById<ImageView>(Resource.Id.image3);
                            CountImageLabel = itemView.FindViewById<TextView>(Resource.Id.counttext);
                            break;
                    }

                    Image?.SetOnClickListener(this);
                    Image2?.SetOnClickListener(this);
                    Image3?.SetOnClickListener(this);
                    Image4?.SetOnClickListener(this);
                    CountImageLabel?.SetOnClickListener(this);

                    //Create an Event
                    itemView.Click += (sender, e) => clickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                    itemView.LongClick += (sender, e) => longClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                }
                catch (Exception e)
                {
                   Methods.DisplayReportResultTrack(e); 
                }
            }

            public void OnClick(View v)
            { 
                try
                {
                    var pos = AdapterPosition;
                    if (pos != RecyclerView.NoPosition)
                    {
                        if (v.Id == GlobalPostViews.Likeicon.Id)
                            IoClickListeners.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = MainView, LikeImgButton = GlobalPostViews.Likeicon, LikeAnimationView = GlobalPostViews.LikeAnimationView }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.UserAvatar.Id || v.Id == GlobalPostViews.Username.Id)
                            IoClickListeners.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { View = MainView, Image = GlobalPostViews.UserAvatar }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Favicon.Id)
                            IoClickListeners.OnFavNewsFeedClick(new FavNewsFeedClickEventArgs { FavImgButton = GlobalPostViews.Favicon, FavAnimationView = GlobalPostViews.FavAnimationView }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.LikeCount.Id)
                            IoClickListeners.OnLikedLabelPostClick(new LikeNewsFeedClickEventArgs { View = MainView, LikeButton = GlobalPostViews.LikeCount }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.CommentCount.Id)
                            IoClickListeners.OnCommentPostClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Commenticon.Id)
                            IoClickListeners.OnCommentClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.ViewMoreComment.Id)
                            IoClickListeners.OnCommentClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Moreicon.Id)
                            IoClickListeners.OnMoreClick(new GlobalClickEventArgs { }, true, AdapterPosition);
                        else if (v.Id == GlobalPostViews.ShareIcon.Id)
                            IoClickListeners.OnShareClick(new GlobalClickEventArgs { }, AdapterPosition);

                        else if (v.Id == Image?.Id)
                            IoClickListeners.ImagePostClick(new GlobalClickEventArgs {  }, AdapterPosition, 0);
                        else if (v.Id == Image2?.Id)
                            IoClickListeners.ImagePostClick(new GlobalClickEventArgs {  }, AdapterPosition, 1);
                        else if (v.Id == Image3?.Id)
                            IoClickListeners.ImagePostClick(new GlobalClickEventArgs {  }, AdapterPosition, 2);
                        else if (v.Id == Image4?.Id)
                            IoClickListeners.ImagePostClick(new GlobalClickEventArgs {  }, AdapterPosition, 3);
                        else if (v.Id == CountImageLabel?.Id)
                            IoClickListeners.ImagePostClick(new GlobalClickEventArgs {  }, AdapterPosition, 4);
                    }
                }
                catch (Exception e)
                {
                     Methods.DisplayReportResultTrack(e); 
                }
            }
        }
        
        public class PhotoAdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener 
        { 
            public View MainView { get; private set; }
             
            public ImageView Image { get; private set; }
            private readonly IOnPostItemClickListener IoClickListeners;
            public GlobalPostViews GlobalPostViews { get; private set; }

            public PhotoAdapterViewHolder(View itemView, Action<PostAdapterClickEventArgs> clickListener, Action<PostAdapterClickEventArgs> longClickListener, IOnPostItemClickListener socialIoClickListeners) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    itemView.SetLayerType(LayerType.Hardware, null);

                    itemView.Tag = "Image";
                    IoClickListeners = socialIoClickListeners;
                    GlobalPostViews = new GlobalPostViews(itemView);

                    GlobalPostViews.Likeicon.SetOnClickListener(this);
                    GlobalPostViews.UserAvatar.SetOnClickListener(this);
                    GlobalPostViews.Favicon.SetOnClickListener(this);
                    GlobalPostViews.LikeCount.SetOnClickListener(this);
                    GlobalPostViews.CommentCount.SetOnClickListener(this);
                    GlobalPostViews.Commenticon.SetOnClickListener(this);
                    GlobalPostViews.ViewMoreComment.SetOnClickListener(this);
                    GlobalPostViews.Moreicon.SetOnClickListener(this);
                    GlobalPostViews.ShareIcon.SetOnClickListener(this);
                    GlobalPostViews.Username.SetOnClickListener(this);

                    Image = itemView.FindViewById<ImageView>(Resource.Id.image);

                    Image?.SetOnClickListener(this);

                    //Create an Event
                    itemView.Click += (sender, e) => clickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                    itemView.LongClick += (sender, e) => longClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                }
                catch (Exception e)
                {
                   Methods.DisplayReportResultTrack(e); 
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    var pos = AdapterPosition;
                    if (pos != RecyclerView.NoPosition)
                    {
                        if (v.Id == GlobalPostViews.Likeicon.Id)
                            IoClickListeners.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = MainView, LikeImgButton = GlobalPostViews.Likeicon, LikeAnimationView = GlobalPostViews.LikeAnimationView }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.UserAvatar.Id || v.Id == GlobalPostViews.Username.Id)
                            IoClickListeners.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { View = MainView, Image = GlobalPostViews.UserAvatar }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Favicon.Id)
                            IoClickListeners.OnFavNewsFeedClick(new FavNewsFeedClickEventArgs { FavImgButton = GlobalPostViews.Favicon, FavAnimationView = GlobalPostViews.FavAnimationView }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.LikeCount.Id)
                            IoClickListeners.OnLikedLabelPostClick(new LikeNewsFeedClickEventArgs { View = MainView, LikeButton = GlobalPostViews.LikeCount }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.CommentCount.Id)
                            IoClickListeners.OnCommentPostClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Commenticon.Id)
                            IoClickListeners.OnCommentClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.ViewMoreComment.Id)
                            IoClickListeners.OnCommentClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Moreicon.Id)
                            IoClickListeners.OnMoreClick(new GlobalClickEventArgs { }, true, AdapterPosition);
                        else if (v.Id == GlobalPostViews.ShareIcon.Id)
                            IoClickListeners.OnShareClick(new GlobalClickEventArgs { }, AdapterPosition);

                        else if (v.Id == Image?.Id)
                            IoClickListeners.ImagePostClick(new GlobalClickEventArgs {  }, AdapterPosition, 0);
                    }
                }
                catch (Exception e)
                {
                     Methods.DisplayReportResultTrack(e); 
                }
            }
        }

        public class MultiPhotoAdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            #region Variables Basic

            public View MainView { get; private set; }
            public ViewPager ViewPagerLayout { get; private set; }
            public CircleIndicator CircleIndicatorView { get; private set; }

            private readonly IOnPostItemClickListener IoClickListeners;
            public GlobalPostViews GlobalPostViews { get; private set; }
            #endregion Variables Basic

            public MultiPhotoAdapterViewHolder(View itemView, Action<PostAdapterClickEventArgs> clickListener, Action<PostAdapterClickEventArgs> longClickListener, IOnPostItemClickListener socialIoClickListeners) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    itemView.Tag = "Image";


                    ViewPagerLayout = itemView.FindViewById<ViewPager>(Resource.Id.pager);
                    CircleIndicatorView = itemView.FindViewById<CircleIndicator>(Resource.Id.indicator);

                    IoClickListeners = socialIoClickListeners;
                    GlobalPostViews = new GlobalPostViews(itemView);

                    GlobalPostViews.Likeicon.SetOnClickListener(this);
                    GlobalPostViews.UserAvatar.SetOnClickListener(this);
                    GlobalPostViews.Favicon.SetOnClickListener(this);
                    GlobalPostViews.LikeCount.SetOnClickListener(this);
                    GlobalPostViews.CommentCount.SetOnClickListener(this);
                    GlobalPostViews.Commenticon.SetOnClickListener(this);
                    GlobalPostViews.ViewMoreComment.SetOnClickListener(this);
                    GlobalPostViews.Moreicon.SetOnClickListener(this);
                    GlobalPostViews.ShareIcon.SetOnClickListener(this);
                    GlobalPostViews.Username.SetOnClickListener(this);

                    //Create an Event
                    itemView.Click += (sender, e) => clickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                    itemView.LongClick += (sender, e) => longClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                }
                catch (Exception e)
                {
                   Methods.DisplayReportResultTrack(e); 
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    var pos = AdapterPosition;
                    if (pos != RecyclerView.NoPosition)
                    {
                        if (v.Id == GlobalPostViews.Likeicon.Id)
                            IoClickListeners.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = MainView, LikeImgButton = GlobalPostViews.Likeicon, LikeAnimationView = GlobalPostViews.LikeAnimationView }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.UserAvatar.Id || v.Id == GlobalPostViews.Username.Id)
                            IoClickListeners.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { View = MainView, Image = GlobalPostViews.UserAvatar }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Favicon.Id)
                            IoClickListeners.OnFavNewsFeedClick(new FavNewsFeedClickEventArgs { FavImgButton = GlobalPostViews.Favicon, FavAnimationView = GlobalPostViews.FavAnimationView }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.LikeCount.Id)
                            IoClickListeners.OnLikedLabelPostClick(new LikeNewsFeedClickEventArgs { View = MainView, LikeButton = GlobalPostViews.LikeCount }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.CommentCount.Id)
                            IoClickListeners.OnCommentPostClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Commenticon.Id)
                            IoClickListeners.OnCommentClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.ViewMoreComment.Id)
                            IoClickListeners.OnCommentClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Moreicon.Id)
                            IoClickListeners.OnMoreClick(new GlobalClickEventArgs { }, true, AdapterPosition);
                        else if (v.Id == GlobalPostViews.ShareIcon.Id)
                            IoClickListeners.OnShareClick(new GlobalClickEventArgs { }, AdapterPosition);
                    }
                }
                catch (Exception e)
                {
                     Methods.DisplayReportResultTrack(e); 
                }
            }
        }


        public class FetchedFeedViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            public PixelWebView WebView { get; private set; }

            private readonly IOnPostItemClickListener IoClickListeners;
            public GlobalPostViews GlobalPostViews { get; private set; }

            public FetchedFeedViewHolder(View itemView, Action<PostAdapterClickEventArgs> clickListener, Action<PostAdapterClickEventArgs> longClickListener, IOnPostItemClickListener socialIoClickListeners) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    itemView.Tag = "Fetched";

                    WebView = itemView.FindViewById<PixelWebView>(Resource.Id.webview);

                    IoClickListeners = socialIoClickListeners;
                    GlobalPostViews = new GlobalPostViews(itemView);

                    GlobalPostViews.Likeicon.SetOnClickListener(this);
                    GlobalPostViews.UserAvatar.SetOnClickListener(this);
                    GlobalPostViews.Favicon.SetOnClickListener(this);
                    GlobalPostViews.LikeCount.SetOnClickListener(this);
                    GlobalPostViews.CommentCount.SetOnClickListener(this);
                    GlobalPostViews.Commenticon.SetOnClickListener(this);
                    GlobalPostViews.ViewMoreComment.SetOnClickListener(this);
                    GlobalPostViews.Moreicon.SetOnClickListener(this);
                    GlobalPostViews.ShareIcon.SetOnClickListener(this);
                    GlobalPostViews.Username.SetOnClickListener(this);

                    //Create an Event
                    itemView.Click += (sender, e) => clickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                    itemView.LongClick += (sender, e) => longClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    var pos = AdapterPosition;
                    if (pos != RecyclerView.NoPosition)
                    {
                        if (v.Id == GlobalPostViews.Likeicon.Id)
                            IoClickListeners.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = MainView, LikeImgButton = GlobalPostViews.Likeicon, LikeAnimationView = GlobalPostViews.LikeAnimationView }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.UserAvatar.Id || v.Id == GlobalPostViews.Username.Id)
                            IoClickListeners.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { View = MainView, Image = GlobalPostViews.UserAvatar }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Favicon.Id)
                            IoClickListeners.OnFavNewsFeedClick(new FavNewsFeedClickEventArgs { FavImgButton = GlobalPostViews.Favicon, FavAnimationView = GlobalPostViews.FavAnimationView }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.LikeCount.Id)
                            IoClickListeners.OnLikedLabelPostClick(new LikeNewsFeedClickEventArgs { View = MainView, LikeButton = GlobalPostViews.LikeCount }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.CommentCount.Id)
                            IoClickListeners.OnCommentPostClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Commenticon.Id)
                            IoClickListeners.OnCommentClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.ViewMoreComment.Id)
                            IoClickListeners.OnCommentClick(new GlobalClickEventArgs { }, AdapterPosition);
                        else if (v.Id == GlobalPostViews.Moreicon.Id)
                            IoClickListeners.OnMoreClick(new GlobalClickEventArgs { }, true, AdapterPosition);
                        else if (v.Id == GlobalPostViews.ShareIcon.Id)
                            IoClickListeners.OnShareClick(new GlobalClickEventArgs { }, AdapterPosition);
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }


        public class FundingViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
           
            public RecyclerView StoryRecyclerView { get; private set; }
            public FundingAdapters FundingAdapters { get; private set; }
            public TextView AboutHead { get; private set; }
            public TextView AboutMore { get; private set; }

            public RelativeLayout MainLinear { get; private set; }
            private readonly Activity ActivityContext;

            public FundingViewHolder(Activity activity, View itemView) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    ActivityContext = activity;

                    MainLinear = (RelativeLayout)itemView.FindViewById(Resource.Id.mainLinear);

                    StoryRecyclerView = itemView.FindViewById<RecyclerView>(Resource.Id.recycler);
                    AboutHead = itemView.FindViewById<TextView>(Resource.Id.headText);
                    AboutMore = itemView.FindViewById<TextView>(Resource.Id.moreText);

                    AboutHead.Text = activity.GetString(Resource.String.Lbl_Funding);
                    if (ListUtils.FundingList.Count > 0)
                        AboutMore.Text = ListUtils.FundingList.Count.ToString();

                    AboutMore.SetOnClickListener(this);

                    if (FundingAdapters != null)
                        return;
                     
                    StoryRecyclerView.SetLayoutManager(new LinearLayoutManager(itemView.Context, LinearLayoutManager.Horizontal, false));
                    FundingAdapters = new FundingAdapters(activity)
                    {
                        FundingList = new ObservableCollection<FundingDataObject>(ListUtils.FundingList.Take(12))
                    };
                    StoryRecyclerView.SetAdapter(FundingAdapters);
                    FundingAdapters.ItemClick += FundingAdaptersOnItemClick;

                    if (FundingAdapters?.FundingList?.Count > 4)
                    {
                        AboutMore.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        AboutMore.Visibility = ViewStates.Invisible;
                    }
                }
                catch (Exception e)
                {
                   Methods.DisplayReportResultTrack(e); 
                }
            }

            private void FundingAdaptersOnItemClick(object sender, FundingAdaptersViewHolderClickEventArgs e)
            {
                try
                {
                    var item = FundingAdapters.GetItem(e.Position);
                    if (item != null)
                    {
                        var intent = new Intent(ActivityContext, typeof(FundingViewActivity));
                        intent.PutExtra("FundingId", item.Id.ToString());
                        intent.PutExtra("ItemObject", JsonConvert.SerializeObject(item));
                        ActivityContext.StartActivity(intent);
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public void RefreshData()
            {
                FundingAdapters.NotifyDataSetChanged();
            }
             
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        if (v.Id == AboutMore.Id)
                        {
                            var intent = new Intent(ActivityContext, typeof(FundingActivity));
                            ActivityContext.StartActivity(intent);
                        }
                    }
                }
                catch (Exception e)
                {
                   Methods.DisplayReportResultTrack(e);  
                }
            } 
        }

        public class AdMobAdapterViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public TemplateView MianAlert { get; private set; }

            public AdMobAdapterViewHolder(View itemView , NewsFeedAdapter newsFeedAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    MianAlert = itemView.FindViewById<TemplateView>(Resource.Id.my_template);
                    MianAlert.Visibility = ViewStates.Gone;

                    newsFeedAdapter.BindAdMob(this);
                }
                catch (Exception e)
                {
                   Methods.DisplayReportResultTrack(e); 
                }
            }
        }


        public class FbAdNativeAdapterViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public LinearLayout NativeAdLayout { get; private set; }

            public FbAdNativeAdapterViewHolder(Activity activity, View itemView, NewsFeedAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    NativeAdLayout = itemView.FindViewById<LinearLayout>(Resource.Id.native_ad_container);
                    NativeAdLayout.Visibility = ViewStates.Gone;

                    if (postAdapter.MAdItems.Count > 0)
                    {
                        var ad = postAdapter.MAdItems.FirstOrDefault();
                        AdsFacebook.InitNative(activity, NativeAdLayout, ad);
                        postAdapter.MAdItems.Remove(ad);
                    }
                    else
                        AdsFacebook.InitNative(activity, NativeAdLayout, null);
                    postAdapter.BindAdFb();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }


        #endregion

    }
}