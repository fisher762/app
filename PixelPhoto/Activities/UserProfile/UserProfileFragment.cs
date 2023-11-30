using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Luseen.Autolinklibrary;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.Chat;
using PixelPhoto.Activities.MyContacts;
using PixelPhoto.Activities.Posts.Extras;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using Com.Airbnb.Lottie;
using Google.Android.Material.AppBar;
using PixelPhoto.Activities.Posts.Adapters;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Helpers.Model;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;
using Object = Java.Lang.Object;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.UserProfile
{
    public class UserProfileFragment : Fragment, AppBarLayout.IOnOffsetChangedListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView UserProfileImage;
        private TextView TxtCountFollowers, TxtCountFollowing, TxtCountFav, TxtFollowers, TxtFollowing, TxtFav, Fullname, Username, IconBusinessAccount;
        private ImageView MoreButton;
        private AutoLinkTextView About;
        private LinearLayout AboutLiner, LinFollowers, LinFollowing;
        private Button WebsiteButton, SocialGoogle, SocialFacebook, SocialTwitter, SocialLinkedIn;
        private Button FollowButton, MessageButton; 
        private ViewStub EmptyStateLayout;
        private PRecyclerView ProfileFeedRecyclerView;
        private UserPostAdapter UserPostAdapter;
        private NewsFeedAdapter MAdapter;
         
        private AppBarLayout AppBarLayoutView;
        private HomeActivity MainContext; 
        private RecyclerViewOnScrollListener MainScrollEvent;
        private CommentObject UserinfoComment;
        private UserDataObject UserinfoData;
        private UserDataObject UserinfoOneSignal;
        private string UserId, FullName, Avatar, Type, Url, PPrivacy = "1";
        private bool SIsFollowing;
        private string Twitter, Facebook, Google, Website;
        private TextSanitizer TextSanitizerAutoLink;
        private View Inflated;
        private ImageView GridButton, ListButton;

        private GridLayoutManager GridLayout;
        private LinearLayoutManager ListLayout;
        private string NewsFeedStyle = "Grid";

        


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
                var view = inflater.Inflate(Resource.Layout.UserProfileLayout, container, false);
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
                var json = Arguments.GetString("userinfo") ?? "";
                UserId = Arguments.GetString("userid") ?? "";
                Avatar = Arguments.GetString("avatar") ?? "";
                FullName = Arguments.GetString("fullname") ?? "";

                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);

                SetNewsFeedStyleToGrid();

                MAdapter.ItemClick += MAdapterOnItemClick;
                UserPostAdapter.ItemClick += PixUserFeedAdapter_ItemClick;
                SocialGoogle.Click += BtnGoogleOnClick;
                SocialFacebook.Click += BtnFacebookOnClick;
                SocialTwitter.Click += BtnTwitterOnClick;
                WebsiteButton.Click += WebsiteButtonOnClick;
                FollowButton.Click += FollowButtonOnClick;
                MessageButton.Click += MessageButtonOnClick;
                LinFollowers.Click += LinFollowersOnClick;
                LinFollowing.Click += LinFollowingOnClick;
                MoreButton.Click += IconMoreOnClick;

                //Get Data Api
                LoadProfile(json, Type);


                AdsGoogle.Ad_RewardedVideo(Activity);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);

            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause(); 
                ProfileFeedRecyclerView?.StopVideo();
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
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDestroy()
        {
            try
            { 
                ProfileFeedRecyclerView?.ReleasePlayer();

                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                GridLayout = new GridLayoutManager(Activity, AppSettings.SetPostRowHorizontalCount);
                ListLayout = new LinearLayoutManager(Context);

                ProfileFeedRecyclerView = view.FindViewById<PRecyclerView>(Resource.Id.RecylerFeed);
                AppBarLayoutView = (AppBarLayout)view.FindViewById(Resource.Id.appBarLayout);  
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                LinFollowers = view.FindViewById<LinearLayout>(Resource.Id.layoutFollowers);
                LinFollowing = view.FindViewById<LinearLayout>(Resource.Id.layoutFollowing);

                UserProfileImage = (ImageView)view.FindViewById(Resource.Id.user_pic);
                Fullname = (TextView)view.FindViewById(Resource.Id.fullname);
                Username = (TextView)view.FindViewById(Resource.Id.username);

                IconBusinessAccount = (TextView)view.FindViewById(Resource.Id.card_name_icon);

                TxtCountFollowers = (TextView)view.FindViewById(Resource.Id.CountFollowers);
                TxtCountFollowing = (TextView)view.FindViewById(Resource.Id.CountFollowing);
                TxtCountFav = (TextView)view.FindViewById(Resource.Id.CountFav);
                TxtFollowers = view.FindViewById<TextView>(Resource.Id.txtFollowers);
                TxtFollowing = view.FindViewById<TextView>(Resource.Id.txtFollowing);
                TxtFav = view.FindViewById<TextView>(Resource.Id.txtFav);
                SocialGoogle = view.FindViewById<Button>(Resource.Id.social1);
                SocialFacebook = view.FindViewById<Button>(Resource.Id.social2);
                SocialTwitter = view.FindViewById<Button>(Resource.Id.social3);
                SocialLinkedIn = view.FindViewById<Button>(Resource.Id.social4);
                WebsiteButton = view.FindViewById<Button>(Resource.Id.website);
                FollowButton = view.FindViewById<Button>(Resource.Id.followButton);
                MessageButton = view.FindViewById<Button>(Resource.Id.messageButton);
                About = view.FindViewById<AutoLinkTextView>(Resource.Id.aboutdescUser);
                AboutLiner = view.FindViewById<LinearLayout>(Resource.Id.aboutliner);
                MoreButton = view.FindViewById<ImageView>(Resource.Id.moreButton);
                GridButton = view.FindViewById<ImageView>(Resource.Id.grid_pic);
                ListButton = view.FindViewById<ImageView>(Resource.Id.menu_pic);

                MAdapter ??= new NewsFeedAdapter(Activity, ProfileFeedRecyclerView);

                GridButton.Click += GridButton_Click;
                ListButton.Click += ListButton_Click;

                ProfileFeedRecyclerView.HasFixedSize = true;
                ProfileFeedRecyclerView.SetItemViewCacheSize(10);
                
                MoreButton.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                //UserProfileImage.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.layout_bg_profile_friends_dark : Resource.Drawable.layout_bg_profile_friends);

                TextSanitizerAutoLink = new TextSanitizer(About, Activity);

                var viewboxText = view.FindViewById<TextView>(Resource.Id.Appname);
                viewboxText.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                TxtCountFollowers.Text = "0";
                TxtCountFollowing.Text = "0";
                TxtCountFav.Text = "0";

                SocialLinkedIn.Visibility = ViewStates.Gone;
                AppBarLayoutView.AddOnOffsetChangedListener(this);
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

        private void ListButton_Click(object sender, EventArgs e)
        {
            SetNewsFeedStyleToList();
        }

        private void GridButton_Click(object sender, EventArgs e)
        {
            SetNewsFeedStyleToGrid();
        }

        private void SetNewsFeedStyleToGrid()
        {
            try
            {
                NewsFeedStyle = "Grid";

                ProfileFeedRecyclerView.SetLayoutManager(GridLayout);
                ProfileFeedRecyclerView.AddItemDecoration(new GridSpacingItemDecoration(1, 1, true));
                ProfileFeedRecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;
                UserPostAdapter ??= new UserPostAdapter(Activity);

                if (MAdapter != null && MAdapter?.PostList?.Count > 0)
                    UserPostAdapter.PostList = new ObservableCollection<PostsObject>(MAdapter.PostList);

                var sizeProvider = new ViewPreloadSizeProvider();
                var preLoader = new RecyclerViewPreloader<PostsObject>(Activity, UserPostAdapter, sizeProvider, 10);
                ProfileFeedRecyclerView.ClearOnScrollListeners();

                ProfileFeedRecyclerView.AddOnScrollListener(preLoader);
                ProfileFeedRecyclerView.SetAdapter(UserPostAdapter);

                EmptyStateLayout.Visibility = ViewStates.Gone;
                ProfileFeedRecyclerView.Visibility = ViewStates.Visible;

                var xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(GridLayout);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
                ProfileFeedRecyclerView.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void SetNewsFeedStyleToList()
        {
            try
            {
                NewsFeedStyle = "Linear";

                ProfileFeedRecyclerView.SetLayoutManager(ListLayout);
                MAdapter ??= new NewsFeedAdapter(Activity, ProfileFeedRecyclerView);

                var sizeProvider = new ViewPreloadSizeProvider();
                var preLoader = new RecyclerViewPreloader<PostsObject>(Activity, MAdapter, sizeProvider, 8);

                if (MAdapter != null && UserPostAdapter?.PostList?.Count > 0)
                    MAdapter.PostList = new ObservableCollection<PostsObject>(UserPostAdapter.PostList);

                ProfileFeedRecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;
                ProfileFeedRecyclerView.ClearOnScrollListeners();
                ProfileFeedRecyclerView.AddOnScrollListener(preLoader);
                ProfileFeedRecyclerView.SetAdapter(MAdapter);

                EmptyStateLayout.Visibility = ViewStates.Gone;
                ProfileFeedRecyclerView.Visibility = ViewStates.Visible;

                var xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(ListLayout);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
                ProfileFeedRecyclerView.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }
        #endregion

        #region Load Profile

        private void LoadProfile(string json, string type)
        {
            try
            {
                if (!string.IsNullOrEmpty(json))
                {
                    switch (type)
                    {
                        case "comment":
                            UserinfoComment = JsonConvert.DeserializeObject<CommentObject>(json);
                            LoadUserData(UserinfoComment);
                            break;
                        case "UserData":
                            UserinfoData = JsonConvert.DeserializeObject<UserDataObject>(json);
                            LoadUserData(UserinfoData);
                            Url = UserinfoData.Url;
                            break;
                        case "OneSignalNotification":
                            UserinfoOneSignal = JsonConvert.DeserializeObject<UserDataObject>(json);
                            LoadUserData(UserinfoOneSignal);
                            break;
                        default:
                            GlideImageLoader.LoadImage(Activity, Arguments.GetString("avatar"), UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                            Fullname.Text = Arguments.GetString("fullname");
                            break;
                    }
                }
                else
                {
                    GlideImageLoader.LoadImage(Activity, Arguments.GetString("avatar"), UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                    Fullname.Text = Arguments.GetString("fullname");
                }


                //Add Api 
                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadUserData(CommentObject cl)
        {
            try
            {
                GlideImageLoader.LoadImage(Activity, cl.Avatar, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                UserProfileImage.SetScaleType(ImageView.ScaleType.CenterCrop);
                Username.Text = "@" + cl.Username;
                Fullname.Text = Methods.FunString.DecodeString(cl.Name);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void LoadUserData(UserDataObject cl, bool friends = true)
        {
            try
            {
                PPrivacy = cl.PPrivacy;

                GlideImageLoader.LoadImage(Activity, cl.Avatar, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                UserProfileImage.SetScaleType(ImageView.ScaleType.CenterCrop);

                TextSanitizerAutoLink.Load(AppTools.GetAboutFinal(cl));
                AboutLiner.Visibility = ViewStates.Visible;

                Username.Text = "@" + cl.Username;
                Fullname.Text = AppTools.GetNameFinal(cl);


                if (cl.Verified == "1")
                    Fullname.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_vector, 0);

                if (cl.BusinessAccount == "1")
                    IconBusinessAccount.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_dolar_vector, 0);

                try
                {
                    TxtCountFav.Text = Methods.FunString.FormatPriceValue(int.Parse(cl.PostsCount));
                    TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(cl.Followers));
                    TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(cl.Following));
                } 
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");

                if (!string.IsNullOrEmpty(cl.Google))
                {
                    Google = cl.Google;
                    SocialGoogle.SetTypeface(font, TypefaceStyle.Normal);
                    SocialGoogle.Text = IonIconsFonts.LogoGoogle;
                    SocialGoogle.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Facebook))
                {
                    Facebook = cl.Facebook;
                    SocialFacebook.SetTypeface(font, TypefaceStyle.Normal);
                    SocialFacebook.Text = IonIconsFonts.LogoFacebook;
                    SocialFacebook.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Website))
                {
                    Website = cl.Website;
                    WebsiteButton.SetTypeface(font, TypefaceStyle.Normal);
                    WebsiteButton.Text = IonIconsFonts.Globe;
                    WebsiteButton.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Twitter))
                {
                    Twitter = cl.Twitter;
                    SocialTwitter.SetTypeface(font, TypefaceStyle.Normal);
                    SocialTwitter.Text = IonIconsFonts.LogoTwitter;
                    SocialTwitter.Visibility = ViewStates.Visible;
                }

                if (cl.IsFollowing != null)
                {
                    SIsFollowing = cl.IsFollowing.Value;
                    if (!friends) return;

                    if (cl.IsFollowing.Value) // My Friend
                    {
                        FollowButton.SetBackgroundResource(Resource.Drawable.Shape_Radius_Grey_Btn);
                        FollowButton.SetTextColor(Color.ParseColor("#000000"));
                        FollowButton.Text = Context.GetText(Resource.String.Lbl_Following);
                        FollowButton.Tag = "true";
                    }
                    else
                    {
                        //Not Friend
                        FollowButton.SetBackgroundResource(Resource.Drawable.Shape_Radius_Gradient_Btn);
                        FollowButton.SetTextColor(Color.ParseColor("#ffffff"));
                        FollowButton.Text = Context.GetText(Resource.String.Lbl_Follow);
                        FollowButton.Tag = "false";
                    }

                    MessageButton.Visibility = cl.CPrivacy == "1" || cl.CPrivacy == "2" && cl.IsFollowing.Value
                        ? ViewStates.Visible
                        : ViewStates.Invisible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetUserProfileApi });
        }
        private async Task GetUserProfileApi()
        {
            if (Methods.CheckConnectivity())
            {
                (var respondCode, var respondString) = await RequestsAsync.User.FetchUserData(UserId);
                if (respondCode == 200)
                {
                    if (respondString is FetchUserDataObject result)
                    {
                        if (result.Data != null)
                        {
                            UserinfoData = result.Data;
                            Url = UserinfoData.Url;
                            LoadUserData(result.Data);

                            LoadExploreFeed();
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity, respondString);
            }
        }

        #endregion

        #region Events

        private void IconMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Report));
                arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Block));
                arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_CopyLinkToProfile));

                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(Activity.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MessageButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Context, typeof(MessagesBoxActivity));
                intent.PutExtra("UserId", UserId);
                intent.PutExtra("TypeChat", Type);
                if (UserinfoData != null)
                    intent.PutExtra("UserItem", JsonConvert.SerializeObject(UserinfoData));

                Activity.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FollowButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {

                    if (FollowButton.Tag?.ToString() == "true")
                    {
                        FollowButton.Tag = "false";
                        FollowButton.Text = Context.GetText(Resource.String.Lbl_Follow);
                        FollowButton.SetBackgroundResource(Resource.Drawable.Shape_Radius_Gradient_Btn);
                        FollowButton.SetTextColor(Color.ParseColor("#ffffff"));

                        //--
                        if (UserinfoData != null)
                        {
                            var count = Convert.ToInt32(UserinfoData.Following);
                            if (count > 0)
                                count--;
                            else
                                count = 0;

                            TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(count);
                        } 
                    }
                    else
                    {
                        FollowButton.Tag = "true";
                        FollowButton.Text = Context.GetText(Resource.String.Lbl_Following);
                        FollowButton.SetBackgroundResource(Resource.Drawable.Shape_Radius_Grey_Btn);
                        FollowButton.SetTextColor(Color.ParseColor("#000000"));

                        //++
                        if (UserinfoData != null)
                        {
                            var count = Convert.ToInt32(UserinfoData.Following);
                            count++;

                            TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(count);
                        }
                    }
                     
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollow(UserId) });
                }
                else
                {
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LinFollowingOnClick(object sender, EventArgs e)
        {
            try
            {
                var bundle = new Bundle();

                bundle.PutString("UserId", UserId);
                bundle.PutString("UserType", "Following");

                var myContactsFragment = new MyContactsFragment
                {
                    Arguments = bundle
                };

                MainContext.OpenFragment(myContactsFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LinFollowersOnClick(object sender, EventArgs e)
        {
            try
            {
                var bundle = new Bundle();

                bundle.PutString("UserId", UserId);
                bundle.PutString("UserType", "Followers");

                var profileFragment = new MyContactsFragment
                {
                    Arguments = bundle
                };

                MainContext.OpenFragment(profileFragment);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }


        private void BtnGoogleOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                    new IntentController(Activity).OpenAppOnGooglePlay(Google);
                else
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnTwitterOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                    new IntentController(Activity).OpenTwitterIntent(Twitter);
                else
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnFacebookOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                    new IntentController(Activity).OpenFacebookIntent(Activity, Facebook);
                else
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void WebsiteButtonOnClick(object sender, EventArgs e)
        {
            if (Methods.CheckConnectivity())
                new IntentController(Activity).OpenBrowserFromPhone(Website);
            else
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
        }


        private void TryAgainButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    EmptyStateLayout.Visibility = ViewStates.Gone; 

                    StartApiServiceLoadFeedJson();
                }
                else
                {
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                    Toast.MakeText(Activity, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PixUserFeedAdapter_ItemClick(object sender, UserPostAdapterViewHolderClickEventArgs e)
        {
            try
            {
                var item = UserPostAdapter.PostList[e.Position];
                if (item != null)
                {
                    MainContext.OpenNewsFeedItem(item.PostId.ToString() , item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, Holders.PostAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = MAdapter?.PostList[e.Position];
                    if (item?.IsLiked != null && !item.IsLiked.Value)
                    {
                        if (SystemClock.ElapsedRealtime() - UserDetails.TimestampLastClick < AppSettings.DoubleClickQualificationSpanInMillis)
                        {
                            var likeIcon = e.View.FindViewById<ImageView>(Resource.Id.Like);
                            var likeAnimationView = e.View.FindViewById<LottieAnimationView>(Resource.Id.animation_view_of_like);
                            var tapLikeAnimation = e.View.FindViewById<LottieAnimationView>(Resource.Id.animation_like);
                            tapLikeAnimation.PlayAnimation();

                            MAdapter.OnPostItemClickListener.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = e.View, LikeImgButton = likeIcon, LikeAnimationView = likeAnimationView }, e.Position);
                        }
                        UserDetails.TimestampLastClick = SystemClock.ElapsedRealtime();
                    }
                }
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

                    try
                    {
                        MainContext.FragmentNavigatorBack();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }

                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Load Explore Feed

        private void StartApiServiceLoadFeedJson(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadFeedAsync(offset) });
        }

        private void LoadExploreFeed()
        {
            try
            {
                if (PPrivacy == "2" || PPrivacy == "1" && SIsFollowing)
                {
                    if (Methods.CheckConnectivity())
                    { 
                        StartApiServiceLoadFeedJson();
                    }
                    else
                    { 
                        Inflated ??= EmptyStateLayout.Inflate();

                        var x = new EmptyStateInflater();
                        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                            x.EmptyStateButton.Click += TryAgainButton_Click;
                        }
                    }
                }
                else
                { 
                    Inflated ??= EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.ProfilePrivate);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                        x.EmptyStateButton.Click += TryAgainButton_Click;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void PolluteUserDataObjectClass(FetchUserPostsByUserIdObject cl)
        {
            try
            {
                UserinfoData = cl.Data.UserData;
                GlideImageLoader.LoadImage(Activity, cl.Data.UserData.Avatar, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                 
                if (!string.IsNullOrEmpty(cl.Data.UserData.About))
                {
                    About.Text = Methods.FunString.DecodeString(cl.Data.UserData.About);
                    AboutLiner.Visibility = ViewStates.Visible;
                }

                TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(cl.Data.UserFollowers);
                TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(cl.Data.UserFollowing);
                TxtCountFav.Text = Methods.FunString.FormatPriceValue(cl.Data.TotalPosts);
                Username.Text = "@" + cl.Data.UserData.Username;
                Fullname.Text = Methods.FunString.DecodeString(cl.Data.UserData.Name);

                var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");

                if (!string.IsNullOrEmpty(cl.Data.UserData.Google))
                {
                    Google = cl.Data.UserData.Google;
                    SocialGoogle.SetTypeface(font, TypefaceStyle.Normal);
                    SocialGoogle.Text = IonIconsFonts.LogoGoogle;
                    SocialGoogle.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Data.UserData.Facebook))
                {
                    Facebook = cl.Data.UserData.Facebook;
                    SocialFacebook.SetTypeface(font, TypefaceStyle.Normal);
                    SocialFacebook.Text = IonIconsFonts.LogoFacebook;
                    SocialFacebook.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Data.UserData.Website))
                {
                    Website = cl.Data.UserData.Website;
                    WebsiteButton.SetTypeface(font, TypefaceStyle.Normal);
                    WebsiteButton.Text = IonIconsFonts.Globe;
                    WebsiteButton.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Data.UserData.Twitter))
                {
                    Twitter = cl.Data.UserData.Twitter;
                    SocialTwitter.SetTypeface(font, TypefaceStyle.Normal);
                    SocialTwitter.Text = IonIconsFonts.LogoTwitter;
                    SocialTwitter.Visibility = ViewStates.Visible;
                }
                 
                if (cl.Data.IsFollowing) // My Friend
                {
                    FollowButton.SetBackgroundResource(Resource.Drawable.Shape_Radius_Grey_Btn);
                    FollowButton.SetTextColor(Color.ParseColor("#000000"));
                    FollowButton.Text = Context.GetText(Resource.String.Lbl_Following);

                    FollowButton.Tag = "true";
                }
                else
                {
                    //Not Friend
                    FollowButton.SetBackgroundResource(Resource.Drawable.Shape_Radius_Gradient_Btn);
                    FollowButton.SetTextColor(Color.ParseColor("#ffffff"));
                    FollowButton.Text = Context.GetText(Resource.String.Lbl_Follow);
                    FollowButton.Tag = "false";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task LoadFeedAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                dynamic adapter = NewsFeedStyle switch
                {
                    "Linear" => MAdapter,
                    "Grid" => UserPostAdapter,
                    _ => UserPostAdapter
                };

                int countList = adapter.PostList.Count;
                (var apiStatus, var respond) = await RequestsAsync.Post.FetchUserPostsById(UserId, "24", offset);
                if (apiStatus != 200 || !(respond is FetchUserPostsByUserIdObject result) || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    PolluteUserDataObjectClass(result);

                    var respondList = result.Data.UserPosts.Count;
                    if (respondList > 0)
                    {
                        result.Data.UserPosts = AppTools.FilterPost(result.Data.UserPosts);

                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data.UserPosts
                                let check = NewsFeedStyle switch
                                {
                                    "Linear" => MAdapter?.PostList?.FirstOrDefault(a => a.PostId == item.PostId),
                                    "Grid" => UserPostAdapter?.PostList?.FirstOrDefault(a => a.PostId == item.PostId),
                                    _ => null
                                }
                                where (dynamic)check == null
                                select item)
                            { 
                                adapter.PostList.Add(item);
                            }

                            Activity?.RunOnUiThread(() => { adapter.NotifyItemRangeInserted(countList, adapter.PostList.Count - countList); });
                        }
                        else
                        {
                            adapter.PostList = new ObservableCollection<PostsObject>(result.Data.UserPosts);
                            Activity?.RunOnUiThread(() => { adapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (adapter.PostList.Count > 10 && !ProfileFeedRecyclerView.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short)?.Show();
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated ??= EmptyStateLayout.Inflate();
                var x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                 
                dynamic adapter = NewsFeedStyle switch
                {
                    "Linear" => MAdapter,
                    "Grid" => UserPostAdapter,
                    _ => UserPostAdapter
                };

                if (adapter?.PostList?.Count > 0)
                {
                    ProfileFeedRecyclerView.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone; 
                }
                else
                {
                    ProfileFeedRecyclerView.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoPost);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;

                    //Remove behavior from AppBarLayout
                    if (ViewCompat.IsLaidOut(AppBarLayoutView))
                    {
                        var appBarLayoutParams = (CoordinatorLayout.LayoutParams)AppBarLayoutView.LayoutParameters;
                        var behavior = (AppBarLayout.Behavior)appBarLayoutParams.Behavior;
                        behavior.SetDragCallback(new DragRemover());
                    }
                }
            }
            catch (Exception e)
            { 
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Drag

        private class DragRemover : AppBarLayout.Behavior.DragCallback
        {
            public override bool CanDrag(Object appBarLayout)
            {
                return false;
            }
        }

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            try
            {
                if (appBarLayout.TotalScrollRange + verticalOffset == 0)
                {


                }
                else
                {

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }



        #endregion

        #region Scroll

        private void OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                PostsObject item;
                switch (NewsFeedStyle)
                {
                    case "Linear":
                        item = MAdapter?.PostList?.LastOrDefault();
                        if (item != null && !string.IsNullOrEmpty(item.UserId.ToString()) && !MainScrollEvent.IsLoading)
                            StartApiServiceLoadFeedJson(item?.PostId.ToString());
                        break;
                    case "Grid":
                        item = UserPostAdapter?.PostList?.LastOrDefault();
                        if (item != null && !string.IsNullOrEmpty(item.UserId.ToString()) && !MainScrollEvent.IsLoading)
                            StartApiServiceLoadFeedJson(item?.PostId.ToString());
                        break;
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int p2, ICharSequence itemString)
        {
            try
            {
                if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_Report))
                {
                    if (Methods.CheckConnectivity())
                    {
                        //Sent Api >>
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.ReportUnReportUser(UserId, "4") });

                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_YourReportPost), ToastLength.Short)?.Show();
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
                else if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_Block))
                {
                    if (Methods.CheckConnectivity())
                    {
                        //Sent Api >>
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.BlockUnblock(UserId) });

                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Blocked_successfully), ToastLength.Short)?.Show();
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
                else if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_CopyLinkToProfile))
                {
                    Methods.CopyToClipboard(Activity, Url); 
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