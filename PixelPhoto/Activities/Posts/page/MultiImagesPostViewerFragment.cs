using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using Com.Airbnb.Lottie;
using Newtonsoft.Json;
using PixelPhoto.Activities.Posts.Adapters;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Activities.Search;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.Library.Anjo.SuperTextLibrary;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.Posts.page
{
    public class MultiImagesPostViewerFragment : Fragment , StTools.IXAutoLinkOnClickListener
    {
        #region Variables Basic

        private ViewPager ViewPager;

        private ImageView UserAvatar, LikeIcon, CommentIcon, Favicon, ShareIcon, MoreIcon;
        private TextView Fullname, LikeCount, TimeTextView, CommentCount;
         
        private LottieAnimationView LikeAnimationView, FavAnimationView;

        private SuperTextView Description; 
        private SocialIoClickListeners ClickListeners;
        private int IndexImage;
        private string PostId; 
        private HomeActivity MainContext; 
        private StReadMoreOption ReadMoreOption;
        private PostsObject DataObject;

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
                Context contextThemeWrapper = new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark_Base);
                // clone the inflater using the ContextThemeWrapper
                var localInflater = inflater.CloneInContext(contextThemeWrapper);

                var view = localInflater?.Inflate(Resource.Layout.MultiImagesPostViewerLayout, container, false);
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

                ClickListeners = new SocialIoClickListeners(Activity);

                InitComponent(view);
                InitToolbar(view);
                AddOrRemoveEvent(view, true);
                DataPostLoader();

                AdsGoogle.Ad_Interstitial(Activity);
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
                ViewPager = (ViewPager)view.FindViewById(Resource.Id.view_pager);

                //<!--Including User Post Owner Layout -->
                UserAvatar = view.FindViewById<ImageView>(Resource.Id.userAvatar);
                Fullname = view.FindViewById<TextView>(Resource.Id.username);
                TimeTextView = view.FindViewById<TextView>(Resource.Id.time_text);
                MoreIcon = view.FindViewById<ImageView>(Resource.Id.moreicon);

                //<!--Including Post Actions -->
                LikeIcon = view.FindViewById<ImageView>(Resource.Id.Like);
                ShareIcon = view.FindViewById<ImageView>(Resource.Id.share);
                CommentIcon = view.FindViewById<ImageView>(Resource.Id.Comment);
                Favicon = view.FindViewById<ImageView>(Resource.Id.fav);
                CommentCount = view.FindViewById<TextView>(Resource.Id.Commentcount); 
                LikeCount = view.FindViewById<TextView>(Resource.Id.Likecount);
                Description = view.FindViewById<SuperTextView>(Resource.Id.tv_description);
                Description?.SetTextInfo(Description);
                 
                LikeAnimationView = view.FindViewById<LottieAnimationView>(Resource.Id.animation_view_of_like);
                FavAnimationView = view.FindViewById<LottieAnimationView>(Resource.Id.animation_view_of_fav);
                 
                LikeAnimationView.SetAnimation("LikeHeart.json");
                FavAnimationView.SetAnimation("FavAnim.json");
                Favicon.Tag = "Add";
                 
                ReadMoreOption = new StReadMoreOption.Builder()
                    .TextLength(200, StReadMoreOption.TypeCharacter)
                    .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(GetText(Resource.String.Lbl_ReadLess))
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

        private void AddOrRemoveEvent(View view, bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    if (!CommentCount.HasOnClickListeners)
                        CommentCount.Click += (sender, e) => ClickListeners.OnCommentPostClick(new GlobalClickEventArgs { NewsFeedClass = DataObject }, "ImagePost");

                    if (!LikeCount.HasOnClickListeners)
                        LikeCount.Click += (sender, e) => ClickListeners.OnLikedPostClick(new LikeNewsFeedClickEventArgs { View = view, NewsFeedClass = DataObject, LikeImgButton = LikeIcon });

                    if (!LikeIcon.HasOnClickListeners)
                        LikeIcon.Click += (sender, e) => ClickListeners.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = view, NewsFeedClass = DataObject, LikeImgButton = LikeIcon, LikeAnimationView = LikeAnimationView });
                     
                    if (!Favicon.HasOnClickListeners)
                        Favicon.Click += (sender, e) => ClickListeners.OnFavNewsFeedClick(new FavNewsFeedClickEventArgs {  NewsFeedClass = DataObject, FavImgButton = Favicon, FavAnimationView = FavAnimationView });

                    if (!UserAvatar.HasOnClickListeners)
                        UserAvatar.Click += (sender, e) => ClickListeners.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { View = view, NewsFeedClass = DataObject, Image = UserAvatar,  });

                    if (!Fullname.HasOnClickListeners)
                        Fullname.Click += (sender, e) => ClickListeners.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { View = view, NewsFeedClass = DataObject, Image = UserAvatar,  });

                    if (!CommentIcon.HasOnClickListeners)
                        CommentIcon.Click += (sender, e) => ClickListeners.OnCommentClick(new GlobalClickEventArgs { NewsFeedClass = DataObject,  }, "ImagePost");
                     
                    if (!MoreIcon.HasOnClickListeners)
                        MoreIcon.Click += (sender, e) => ClickListeners.OnMoreClick(new GlobalClickEventArgs { NewsFeedClass = DataObject,  }, false, "ImagePost");

                    if (!ShareIcon.HasOnClickListeners)
                        ShareIcon.Click += (sender, e) => ClickListeners.OnShareClick(new GlobalClickEventArgs { NewsFeedClass = DataObject,  });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Get Data Post
         
        private void DataPostLoader()
        {
            try
            {
                IndexImage = Convert.ToInt32(Arguments.GetString("indexImage") ?? "0");

                PostId = Arguments.GetString("PostId") ?? "";
                DataObject = JsonConvert.DeserializeObject<PostsObject>(Arguments.GetString("postInfo") ?? "");
                if (DataObject != null)
                {
                    var photos = new ObservableCollection<string>(DataObject.MediaSet.Select(image => image.File).ToList());
                     
                    ViewPager.Adapter = new TouchImageAdapter(Activity, photos);
                    ViewPager.CurrentItem = IndexImage;
                    ViewPager.Adapter.NotifyDataSetChanged();
                      
                    DisplayData(DataObject);
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
                if (item == null)
                    return;

                GlideImageLoader.LoadImage(Activity, item.Avatar, UserAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

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

                CommentCount.Text = item.Votes + " " + Activity.GetString(Resource.String.Lbl_Comments);
                LikeCount.Text = item.Likes + " " + Activity.GetText(Resource.String.Lbl_Likes); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
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
                                var photos = new ObservableCollection<string>(DataObject.MediaSet.Select(image => image.File).ToList());

                                ViewPager.Adapter = new TouchImageAdapter(Activity, photos);
                                ViewPager.CurrentItem = IndexImage;
                                ViewPager.Adapter.NotifyDataSetChanged();
                                 
                                DisplayData(DataObject);
                            }
                        }
                    }
                    else Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
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
                    Methods.App.SendEmail(Activity, p1);
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
                    Activity.StartActivity(intent);
                }
                else if (typetext == "Hashtag")
                {
                    // Show All Post By Hash
                    var bundle = new Bundle();
                    bundle.PutString("HashId", "");
                    bundle.PutString("HashName", Methods.FunString.DecodeString(p1));

                    var profileFragment = new HashTagPostFragment
                    {
                        Arguments = bundle
                    };

                    HomeActivity.GetInstance().OpenFragment(profileFragment);
                }
                else if (typetext == "Mention")
                {
                    var bundle = new Bundle();
                    bundle.PutString("Key", Methods.FunString.DecodeString(p1));

                    var searchFragment = new SearchFragment
                    {
                        Arguments = bundle
                    };

                    HomeActivity.GetInstance().OpenFragment(searchFragment);
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