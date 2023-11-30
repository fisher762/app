using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Com.Airbnb.Lottie;
using Com.Google.Android.Youtube.Player;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.Comment.Adapters;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Activities.Search;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.Library.Anjo.SuperTextLibrary;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.Posts.page
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode, ResizeableActivity = true)]
    public class YoutubePlayerPostViewActivity : YouTubeBaseActivity, StTools.IXAutoLinkOnClickListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView UserAvatar, LikeIcon, CommentIcon, Favicon, ShareIcon, MoreIcon, IconBack;
        private TextView Fullname, LikeCount, TimeTextView, ViewCommentsButton;
        private TextView CommentCount;
        private SuperTextView Description;
        private RecyclerView CommentRecyclerView;
        private StReadMoreOption ReadMoreOption;
        private CommentsAdapter CommentsAdapter; 
        private string UserId, FullName, Avatar, Json, PostId;
        private SocialIoClickListeners ClickListeners;
        private LottieAnimationView LikeAnimationView, FavAnimationView;
        private YoutubePlayerController YouTubePlayerEventLoader;
        private PostsObject DataObject; 
        private CommentObject CommentObject;
        private string TypeEventClick, MentionText;
        private CommentObject ItemCommentObject;
        private int PositionItemCommentObject;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.YoutubePlayerActivityLayout);

                Json = Intent.GetStringExtra("object") ?? "";
                UserId = Intent.GetStringExtra("userid") ?? "";
                Avatar = Intent.GetStringExtra("avatar") ?? "";
                FullName = Intent.GetStringExtra("fullname") ?? "";
                PostId = Intent.GetStringExtra("PostId") ?? "";

                ClickListeners = new SocialIoClickListeners(this);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar(); 
                ReadPassedData();

                AdsGoogle.Ad_Interstitial(this);
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
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                if (YouTubePlayerEventLoader.YoutubePlayer != null && YouTubePlayerEventLoader.YoutubePlayer.IsPlaying)
                    YouTubePlayerEventLoader.YoutubePlayer.Pause();
                 
                base.OnStop();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                base.OnTrimMemory(level);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                base.OnLowMemory();
                GC.Collect(GC.MaxGeneration);
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
                YouTubePlayerEventLoader?.YoutubePlayer?.Release();

                var bundle = new Bundle();

                switch (TypeEventClick)
                {
                    case "CommentReplyPostClick":
                        ClickListeners?.CommentReplyPostClick(new CommentReplyClickEventArgs { CommentObject = ItemCommentObject, Position = PositionItemCommentObject,  });
                        break;
                    case "OpenProfileCommentsClick":
                        AppTools.OpenProfile(this, ItemCommentObject.UserId.ToString(), ItemCommentObject);
                        break;
                    case "OnCommentClick":
                        ClickListeners?.OnCommentClick(new GlobalClickEventArgs { NewsFeedClass = DataObject }, "ImagePost");
                        break;
                    case "OnAvatarImageFeedClick":
                        ClickListeners?.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { NewsFeedClass = DataObject, Image = UserAvatar,  });
                        break;
                    case "OnLikedPostClick":
                        ClickListeners?.OnLikedPostClick(new LikeNewsFeedClickEventArgs {  NewsFeedClass = DataObject, LikeImgButton = LikeIcon });
                        break;
                    case "OnCommentPostClick":
                        ClickListeners?.OnCommentPostClick(new GlobalClickEventArgs { NewsFeedClass = DataObject }, "ImagePost");
                        break;
                    case "HashtagClick":
                        // Show All Post By Hash 
                        bundle.PutString("HashId", "");
                        bundle.PutString("HashName", MentionText);

                        var profileFragment = new HashTagPostFragment
                        {
                            Arguments = bundle
                        };

                        HomeActivity.GetInstance().OpenFragment(profileFragment);
                        break;
                    case "MentionClick":
                        bundle.PutString("Key", MentionText);

                        var searchFragment = new SearchFragment
                        {
                            Arguments = bundle
                        };

                        HomeActivity.GetInstance().OpenFragment(searchFragment);
                        break;
                }

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Menu 

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                IconBack = FindViewById<ImageView>(Resource.Id.back);
                IconBack.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White :  Color.Black);
                 
                Fullname = FindViewById<TextView>(Resource.Id.username);
                UserAvatar = FindViewById<ImageView>(Resource.Id.userAvatar);
                MoreIcon = FindViewById<ImageView>(Resource.Id.moreicon);
                LikeIcon = FindViewById<ImageView>(Resource.Id.Like);
                CommentIcon = FindViewById<ImageView>(Resource.Id.Comment);
                Favicon = FindViewById<ImageView>(Resource.Id.fav);
                ShareIcon = FindViewById<ImageView>(Resource.Id.share);
                 
                Description = FindViewById<SuperTextView>(Resource.Id.description);
                Description?.SetTextInfo(Description);

                TimeTextView = FindViewById<TextView>(Resource.Id.time_text);
                ViewCommentsButton = FindViewById<TextView>(Resource.Id.ViewMoreComment);
                LikeCount = FindViewById<TextView>(Resource.Id.Likecount);
                CommentCount = FindViewById<TextView>(Resource.Id.Commentcount);
                CommentRecyclerView = FindViewById<RecyclerView>(Resource.Id.RecylerComment);

                //Set Adapter Data 
                CommentsAdapter = new CommentsAdapter(this);
                var mLayoutManager = new LinearLayoutManager(this);
                CommentRecyclerView.SetLayoutManager(mLayoutManager);
                CommentRecyclerView.SetAdapter(CommentsAdapter);
                CommentRecyclerView.NestedScrollingEnabled = false;
                CommentsAdapter.AvatarClick += CommentsAdapterOnAvatarClick;
                CommentsAdapter.LikeClick += CommentsAdapterOnLikeClick;
                CommentsAdapter.ReplyClick += CommentsAdapterOnReplyClick;
                CommentsAdapter.MoreClick += CommentsAdapterOnMoreClick;

                ReadMoreOption = new StReadMoreOption.Builder()
                    .TextLength(200, StReadMoreOption.TypeCharacter)
                    .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();

                //Anim
                LikeAnimationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view_of_like); 
                FavAnimationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view_of_fav); 
                LikeAnimationView.SetAnimation("LikeHeart.json");
                FavAnimationView.SetAnimation("FavAnim.json");
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
                    toolbar.SetTitleTextColor(Color.White);
                    //SetSupportActionBar(toolbar);
                    //SupportActionBar.SetDisplayShowCustomEnabled(true);
                    //SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    //SupportActionBar.SetHomeButtonEnabled(true);
                    //SupportActionBar.SetDisplayShowHomeEnabled(true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void LoadPostTypeYoutube()
        {
            try
            { 
                var youTubeView = FindViewById<YouTubePlayerView>(Resource.Id.youtube_view);

                YouTubePlayerEventLoader = new YoutubePlayerController(this, DataObject.Youtube);
                youTubeView.Initialize(GetText(Resource.String.google_key), YouTubePlayerEventLoader); 
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
                    CommentCount.Click += CommentCountOnClick;
                    LikeCount.Click += LikeCountOnClick;
                    LikeIcon.Click += LikeIconOnClick;
                    Favicon.Click += FaviconOnClick;
                    UserAvatar.Click += UserAvatarOnClick;
                    Fullname.Click += UserAvatarOnClick;
                    CommentIcon.Click += CommentIconOnClick;
                    ViewCommentsButton.Click += CommentIconOnClick;
                    MoreIcon.Click += MoreIconOnClick;
                    ShareIcon.Click += ShareIconOnClick;
                    IconBack.Click += IconBackOnClick;
                }
                else
                {
                    CommentCount.Click -= CommentCountOnClick;
                    LikeCount.Click-= LikeCountOnClick;
                    LikeIcon.Click -= LikeIconOnClick;
                    Favicon.Click -= FaviconOnClick;
                    UserAvatar.Click -= UserAvatarOnClick;
                    Fullname.Click -= UserAvatarOnClick;
                    CommentIcon.Click -= CommentIconOnClick;
                    ViewCommentsButton.Click -= CommentIconOnClick;
                    MoreIcon.Click -= MoreIconOnClick;
                    ShareIcon.Click -= ShareIconOnClick;
                    IconBack.Click -= IconBackOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion
         
        #region Get Data Post

        public void ReadPassedData()
        {
            try
            {
                DataObject = JsonConvert.DeserializeObject<PostsObject>(Json);
                if (DataObject != null)
                {
                    LoadPostTypeYoutube();

                    var time = Methods.Time.TimeAgo(Convert.ToInt32(DataObject.Time), false);
                    DisplayData(DataObject);

                    if (DataObject.Comments != null && DataObject.Comments.Count > 0)
                    {
                        CommentsAdapter.CommentList = new ObservableCollection<CommentObject>(DataObject.Comments);
                        CommentsAdapter.NotifyDataSetChanged();
                    }
                }
                
                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DisplayData(PostsObject item)
        {
            try
            {
                GlideImageLoader.LoadImage(this, item.Avatar, UserAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                Fullname.Text = item.Username;

                if (item.UserData?.Verified == "1")
                    Fullname.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_small_vector, 0);

                if (item.UserData?.BusinessAccount == "1")
                    Fullname.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_dolar_small_vector, 0);

                var time = Methods.Time.TimeAgo(Convert.ToInt32(item.Time), false);
                TimeTextView.Text = time;

                if (!string.IsNullOrEmpty(item.Description) && !string.IsNullOrWhiteSpace(item.Description))
                {
                    Description.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());
                    ReadMoreOption.AddReadMoreTo(Description, new Java.Lang.String(item.Description));
                    Description.Visibility = ViewStates.Visible;
                }
                else
                {
                    Description.Visibility = ViewStates.Gone;
                }

                //if (item.Boosted == 1)
                //{
                //    IsPromoted.Text = Activity.GetString(Resource.String.Lbl_Promoted);
                //    IsPromoted.Visibility = ViewStates.Visible;
                //    TimeTextView.Text = "";
                //}

                LikeIcon.Tag = item.IsLiked != null && item.IsLiked.Value ? "Like" : "Liked";
                ClickListeners.SetLike(LikeIcon);

                Favicon.Tag = item.IsSaved != null && item.IsSaved.Value ? "Add" : "Added";
                ClickListeners.SetFav(Favicon);

                CommentCount.Text = item.Votes + " " + this.GetString(Resource.String.Lbl_Comments);
                LikeCount.Text = item.Likes + " " + this.GetText(Resource.String.Lbl_Likes);
                ViewCommentsButton.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
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
                                LoadPostTypeYoutube();

                                var time = Methods.Time.TimeAgo(Convert.ToInt32(DataObject.Time), false);
                                DisplayData(DataObject);

                                if (DataObject.Comments != null && DataObject.Comments.Count > 0)
                                {
                                    CommentsAdapter.CommentList = new ObservableCollection<CommentObject>(DataObject.Comments);
                                    CommentsAdapter.NotifyDataSetChanged();
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Close
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Finish();
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
                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));

                    if (CommentObject.IsOwner)
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Delete));

                    dialogList.Title(GetText(Resource.String.Lbl_More));
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
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
                ClickListeners ??= new SocialIoClickListeners(this);
                PositionItemCommentObject = e.Position;
                ItemCommentObject = CommentsAdapter.GetItem(e.Position);
                if (ItemCommentObject != null)
                {
                    TypeEventClick = "CommentReplyPostClick";
                    Finish(); 
                }
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
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    else
                    {
                        var likeIcon = e.View.FindViewById<ImageView>(Resource.Id.likeIcon);
                        var likeCount = e.View.FindViewById<TextView>(Resource.Id.Like);

                        var interpolator = new MyBounceInterpolator(0.2, 20);
                        var animationScale = AnimationUtils.LoadAnimation(this, Resource.Animation.scale);
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
                            likeCount.Text = GetString(Resource.String.Lbl_Likes) + " " + "(" + x + ")";
                        }
                        else
                        {
                            var x = item.Likes;

                            if (x > 0)
                                x--;
                            else
                                x = 0;

                            item.Likes = x;

                            likeCount.Text = GetString(Resource.String.Lbl_Likes) + " " + "(" + item.Likes + ")";
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
                if (YouTubePlayerEventLoader.YoutubePlayer != null && YouTubePlayerEventLoader.YoutubePlayer.IsPlaying)
                    YouTubePlayerEventLoader.YoutubePlayer.Pause();

                PositionItemCommentObject = e.Position;
                ItemCommentObject = CommentsAdapter.GetItem(e.Position);
                if (ItemCommentObject != null)
                {
                    TypeEventClick = "OpenProfileCommentsClick";
                    Finish(); 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ShareIconOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListeners?.OnShareClick(new GlobalClickEventArgs { NewsFeedClass = DataObject,  });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MoreIconOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListeners?.OnMoreClick(new GlobalClickEventArgs { NewsFeedClass = DataObject,  }, false, "ImagePost");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CommentIconOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeEventClick = "OnCommentClick";
                Finish(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UserAvatarOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeEventClick = "OnAvatarImageFeedClick";
                Finish(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FaviconOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListeners?.OnFavNewsFeedClick(new FavNewsFeedClickEventArgs { NewsFeedClass = DataObject, FavImgButton = Favicon, FavAnimationView = FavAnimationView });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikeIconOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListeners?.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = LikeCount, NewsFeedClass = DataObject, LikeImgButton = LikeIcon, LikeAnimationView = LikeAnimationView });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikeCountOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeEventClick = "OnLikedPostClick";
                Finish(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CommentCountOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeEventClick = "OnCommentPostClick";
                Finish(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (itemString.ToString() == GetText(Resource.String.Lbl_Copy))
                {
                    Methods.CopyToClipboard(this, Methods.FunString.DecodeString(CommentObject.Text));
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_Delete))
                {
                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                    dialog.Title(GetText(Resource.String.Lbl_DeleteComment));
                    dialog.Content(GetText(Resource.String.Lbl_AreYouSureDeleteComment));
                    dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                    {
                        try
                        {
                            if (!Methods.CheckConnectivity())
                            {
                                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
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

                            Toast.MakeText(this, GetString(Resource.String.Lbl_CommentSuccessfullyDeleted), ToastLength.Short)?.Show();
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Post.DeleteComment(CommentObject.Id.ToString()) });
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
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

        public void AutoLinkTextClick(StTools.XAutoLinkMode p0, string p1, Dictionary<string, string> userData)
        {
            try
            {
                var typetext = Methods.FunString.Check_Regex(p1);
                if (typetext == "Email")
                {
                    Methods.App.SendEmail(this, p1);
                }
                else if (typetext == "Website")
                {
                    var url = p1;
                    if (!p1.Contains("http"))
                    {
                        url = "http://" + p1;
                    }

                    var intent = new Intent(Application.Context, typeof(LocalWebViewActivity));
                    intent.PutExtra("URL", url);
                    intent.PutExtra("Type", url);
                    StartActivity(intent);
                }
                else if (typetext == "Hashtag")
                {
                    MentionText = Methods.FunString.DecodeString(p1);
                    TypeEventClick = "HashtagClick";
                    Finish(); 
                }
                else if (typetext == "Mention")
                {
                    MentionText = Methods.FunString.DecodeString(p1);
                    TypeEventClick = "MentionClick";
                    Finish(); 
                }
                else if (typetext == "Number")
                {
                    // IMethods.App.SaveContacts(_activity, autoLinkOnClickEventArgs.P1, "", "2");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}