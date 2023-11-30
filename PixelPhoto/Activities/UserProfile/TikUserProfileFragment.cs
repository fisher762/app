using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager2.Widget;
using Com.Tuyenmonkey.Textdecorator;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.Chat;
using PixelPhoto.Activities.MyContacts;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.UserProfile.Fragments;
using PixelPhoto.Adapters;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.UserProfile
{
    public class TikUserProfileFragment : Fragment, TabLayoutMediator.ITabConfigurationStrategy, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region  Variables Basic

        private MainTabAdapter Adapter;
        private HomeActivity GlobalContext;
        private TextView IconBack, TxtUserName, BtnMore, TxtFullName, TxtFollowingCount, TxtFollowersCount, TxtPostCount, BtnMessage;
        private Button FollowButton;
        private ImageView ImageUser;
        private LinearLayout FollowingLayout, FollowersLayout, PostLayout;
        private TabLayout TabLayout;
        private ViewPager2 ViewPager;
        private UserPostFragment PostTab;
        private AboutFragment AboutTab;
        private CollapsingToolbarLayout CollapsingToolbar;
        public string UserId, Json, Type, Url, PPrivacy = "1";
        private bool SIsFollowing;
        private CommentObject UserinfoComment;
        public UserDataObject UserinfoData;
        private UserDataObject UserinfoOneSignal;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity ?? HomeActivity.GetInstance();
            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.TikUserProfileLayout, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                Type = Arguments.GetString("type") ?? "";
                Json = Arguments.GetString("userinfo") ?? "";
                UserId = Arguments.GetString("userid") ?? "";

                InitComponent(view);
                InitToolbar(view);
                AddOrRemoveEvent(true);
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

                    try
                    {
                        GlobalContext.FragmentNavigatorBack();
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

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                CollapsingToolbar = view.FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = "";

                IconBack = view.FindViewById<TextView>(Resource.Id.back_btn);
                TxtUserName = view.FindViewById<TextView>(Resource.Id.username);
                BtnMore = view.FindViewById<TextView>(Resource.Id.more_btn);

                ImageUser = view.FindViewById<ImageView>(Resource.Id.user_image);
                TxtFullName = view.FindViewById<TextView>(Resource.Id.fullname);

                FollowingLayout = view.FindViewById<LinearLayout>(Resource.Id.following_layout);
                TxtFollowingCount = view.FindViewById<TextView>(Resource.Id.following_count_txt);

                FollowersLayout = view.FindViewById<LinearLayout>(Resource.Id.followers_layout);
                TxtFollowersCount = view.FindViewById<TextView>(Resource.Id.followers_count_txt);

                PostLayout = view.FindViewById<LinearLayout>(Resource.Id.post_layout);
                TxtPostCount = view.FindViewById<TextView>(Resource.Id.post_count_txt);

                BtnMessage = view.FindViewById<TextView>(Resource.Id.message_btn);
                FollowButton = view.FindViewById<Button>(Resource.Id.add_btn);

                ViewPager = view.FindViewById<ViewPager2>(Resource.Id.pager);
                TabLayout = view.FindViewById<TabLayout>(Resource.Id.tabs);

                TxtFollowingCount.Text = "0";
                TxtFollowersCount.Text = "0";
                TxtPostCount.Text = "0";
                  
                ViewPager.OffscreenPageLimit = 2;
                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach();

                TabLayout.GetTabAt(0).SetIcon(Resource.Drawable.ic_tab_more);
                TabLayout.GetTabAt(1).SetIcon(Resource.Drawable.ic_tab_user_profile);

                // set icon color pre-selected
                TabLayout.GetTabAt(0).Icon.SetColorFilter(new PorterDuffColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray, PorterDuff.Mode.SrcIn));
                TabLayout.GetTabAt(1).Icon.SetColorFilter(new PorterDuffColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray, PorterDuff.Mode.SrcIn));

                TabLayout.GetTabAt(0).Icon.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor(AppSettings.MainColor), PorterDuff.Mode.SrcIn));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, IonIconsFonts.ArrowBack);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, BtnMore, IonIconsFonts.More);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, BtnMessage, FontAwesomeIcon.PaperPlane);

                TxtUserName.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                BtnMore.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                TxtFullName.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                if (AppSettings.FlowDirectionRightToLeft)
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, IonIconsFonts.ArrowForward);

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
                GlobalContext.SetToolBar(toolBar, " ", false);
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
                    IconBack.Click += IconBackOnClick;
                    BtnMore.Click += BtnMoreOnClick;
                    BtnMessage.Click += BtnMessageOnClick;
                    FollowButton.Click += BtnAddOnClick;
                    FollowersLayout.Click += LinFollowersOnClick;
                    FollowingLayout.Click += LinFollowingOnClick;
                    TabLayout.TabSelected += TabLayoutOnTabSelected;
                    TabLayout.TabUnselected += TabLayoutOnTabUnselected;
                }
                else
                {
                    IconBack.Click -= IconBackOnClick;
                    BtnMore.Click -= BtnMoreOnClick;
                    BtnMessage.Click -= BtnMessageOnClick;
                    FollowButton.Click -= BtnAddOnClick;
                    FollowersLayout.Click -= LinFollowersOnClick;
                    FollowingLayout.Click -= LinFollowingOnClick;
                    TabLayout.TabSelected -= TabLayoutOnTabSelected;
                    TabLayout.TabUnselected -= TabLayoutOnTabUnselected;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tab

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                PostTab = new UserPostFragment();
                AboutTab = new AboutFragment();

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(PostTab, "");
                Adapter.AddFragment(AboutTab, "");

                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.OffscreenPageLimit = Adapter.ItemCount;

                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                viewPager.Adapter = Adapter;
                viewPager.Adapter.NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            try
            {
                if (position == 0)
                {
                    tab.SetIcon(Resource.Drawable.ic_tab_more);
                }
                else if (position == 1)
                {
                    tab.SetIcon(Resource.Drawable.ic_tab_user_profile);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void TabLayoutOnTabUnselected(object sender, TabLayout.TabUnselectedEventArgs e)
        {
            try
            {
                e.Tab.Icon.SetColorFilter(new PorterDuffColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray, PorterDuff.Mode.SrcIn));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TabLayoutOnTabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            try
            {
                e.Tab.Icon.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor(AppSettings.MainColor), PorterDuff.Mode.SrcIn));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion Set Tab

        #region Events

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.FragmentNavigatorBack();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        // More
        private void BtnMoreOnClick(object sender, EventArgs e)
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

        // Show Message 
        private void BtnMessageOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Context, typeof(MessagesBoxActivity));
                intent.PutExtra("UserId", UserId);
                intent.PutExtra("TypeChat", Type);
                if (UserinfoData != null)
                    intent.PutExtra("UserItem", JsonConvert.SerializeObject(UserinfoData));

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Activity.StartActivity(intent);
                }
                else
                {
                    //Check to see if any permission in our group is available, if one, then all are
                    if (Context.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        Context.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        Activity.StartActivity(intent);
                    }
                    else
                        new PermissionsController(Activity).RequestPermission(100);

                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (FollowButton.Tag?.ToString() == "true")
                    {
                        FollowButton.Tag = "false";
                        FollowButton.Text = Context.GetText(Resource.String.Lbl_Follow);
                        FollowButton.SetBackgroundResource(Resource.Drawable.buttonFlatNormal);
                        FollowButton.SetTextColor(Color.ParseColor("#ffffff"));
                    }
                    else
                    {
                        FollowButton.Tag = "true";
                        FollowButton.Text = Context.GetText(Resource.String.Lbl_Following);
                        FollowButton.SetBackgroundColor(Color.ParseColor("#efefef"));
                        FollowButton.SetTextColor(Color.ParseColor("#444444"));
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

        // Show Following
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

                GlobalContext.OpenFragment(myContactsFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Show Followers
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

                GlobalContext.OpenFragment(profileFragment);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region Load Profile Api 

        public void LoadProfile(string json, string type)
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
                            GlideImageLoader.LoadImage(Activity, Arguments.GetString("avatar"), ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                            TxtFullName.Text = Arguments.GetString("fullname");
                            break;
                    } 
                }
                else
                {
                    GlideImageLoader.LoadImage(Activity, Arguments.GetString("avatar"), ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                    TxtFullName.Text = Arguments.GetString("fullname");
                }

                //Add Api 
                LoadExploreFeed();
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
                GlideImageLoader.LoadImage(Activity, cl.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                TxtUserName.Text = "@" + cl.Username;
                TxtFullName.Text = Methods.FunString.DecodeString(cl.Name);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void LoadUserData(UserDataObject cl, bool friends = true)
        {
            try
            {
                PPrivacy = cl.PPrivacy;

                GlideImageLoader.LoadImage(Activity, cl.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                AboutTab.TextSanitizerAutoLink.Load(AppTools.GetAboutFinal(cl));
                AboutTab.TxtGender.Text = cl.Gender;
                AboutTab.TxtEmail.Text = cl.Email;
                if (string.IsNullOrEmpty(cl.Website))
                {
                    AboutTab.WebsiteLinearLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    AboutTab.TxtWebsite.Text = cl.Website;
                    AboutTab.WebsiteLinearLayout.Visibility = ViewStates.Visible;
                }
                 
                TxtUserName.Text = "@" + cl.Username;

                var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");
                TxtFullName.SetTypeface(font, TypefaceStyle.Normal);

                var textHighLighter = AppTools.GetNameFinal(cl);

                if (cl.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircle;

                if (cl.BusinessAccount == "1")
                {
                    textHighLighter += " " + IonIconsFonts.LogoUsd;
                }

                var decorator = TextDecorator.Decorate(TxtFullName, textHighLighter);

                if (cl.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircle);

                if (cl.BusinessAccount == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsBusiness, IonIconsFonts.LogoUsd);

                decorator.Build();

                TxtPostCount.Text = Methods.FunString.FormatPriceValue(int.Parse(cl.PostsCount));

                if (cl.Followers != null && int.TryParse(cl.Followers, out var numberFollowers))
                    TxtFollowersCount.Text = Methods.FunString.FormatPriceValue(numberFollowers);

                if (cl.Following != null && int.TryParse(cl.Following, out var numberFollowing))
                    TxtFollowingCount.Text = Methods.FunString.FormatPriceValue(numberFollowing);

                if (!string.IsNullOrEmpty(cl.Google))
                {
                    AboutTab.Google = cl.Google;
                    AboutTab.SocialGoogle.SetTypeface(font, TypefaceStyle.Normal);
                    AboutTab.SocialGoogle.Text = IonIconsFonts.LogoGoogle;
                    AboutTab.SocialGoogle.Visibility = ViewStates.Visible;
                    AboutTab.SocialLinksLinear.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Facebook))
                {
                    AboutTab.Facebook = cl.Facebook;
                    AboutTab.SocialFacebook.SetTypeface(font, TypefaceStyle.Normal);
                    AboutTab.SocialFacebook.Text = IonIconsFonts.LogoFacebook;
                    AboutTab.SocialFacebook.Visibility = ViewStates.Visible;
                    AboutTab.SocialLinksLinear.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Website))
                {
                    AboutTab.Website = cl.Website;
                    AboutTab.WebsiteButton.SetTypeface(font, TypefaceStyle.Normal);
                    AboutTab.WebsiteButton.Text = IonIconsFonts.Globe;
                    AboutTab.WebsiteButton.Visibility = ViewStates.Visible;
                    AboutTab.SocialLinksLinear.Visibility = ViewStates.Visible;
                }


                if (!string.IsNullOrEmpty(cl.Twitter))
                {
                    AboutTab.Twitter = cl.Twitter;
                    AboutTab.SocialTwitter.SetTypeface(font, TypefaceStyle.Normal);
                    AboutTab.SocialTwitter.Text = IonIconsFonts.LogoTwitter;
                    AboutTab.SocialTwitter.Visibility = ViewStates.Visible;
                    AboutTab.SocialLinksLinear.Visibility = ViewStates.Visible;
                }

                BtnMessage.Visibility = cl.IsFollowing != null && (cl.CPrivacy == "1" || cl.CPrivacy == "2" && cl.IsFollowing.Value) ? ViewStates.Visible : ViewStates.Invisible;

                if (cl.IsFollowing != null)
                    SIsFollowing = cl.IsFollowing.Value;

                if (!friends) return;
                if (cl.IsFollowing != null && cl.IsFollowing.Value) // My Friend
                {
                    FollowButton.SetBackgroundColor(Color.ParseColor("#efefef"));
                    FollowButton.SetTextColor(Color.ParseColor("#444444"));
                    FollowButton.Text = Context.GetText(Resource.String.Lbl_Following);
                    FollowButton.Tag = "true";
                }
                else
                {
                    //Not Friend
                    FollowButton.SetBackgroundResource(Resource.Drawable.buttonFlatNormal);
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


        #endregion

        #region Load Post

        public void LoadExploreFeed()
        {
            try
            {
                if (PPrivacy == "2" || PPrivacy == "1" && SIsFollowing)
                {
                    if (Methods.CheckConnectivity())
                    {
                        StartApiService();
                    }
                    else
                    {
                        PostTab.Inflated ??= PostTab.EmptyStateLayout.Inflate();

                        var x = new EmptyStateInflater();
                        x.InflateLayout(PostTab.Inflated, EmptyStateInflater.Type.NoConnection);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                            x.EmptyStateButton.Click += TryAgainButton_Click;
                        }
                    }
                }
                else
                {
                    ShowEmptyPage("ProfilePrivate");
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService(string offsetPost = "0")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetPost(offsetPost) });
            else
                Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
        }

        public async Task GetPost(string offset = "0")
        {
            if (PostTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                PostTab.MainScrollEvent.IsLoading = true;
                var countList = PostTab.MAdapter.PostList.Count;
                (var apiStatus, var respond) = await RequestsAsync.Post.FetchUserPostsById(UserId, "24", offset);
                if (apiStatus.Equals(200))
                {
                    if (respond is FetchUserPostsByUserIdObject result)
                    {
                        var respondList = result.Data.UserPosts.Count;
                        if (respondList > 0)
                        {
                            result.Data.UserPosts = AppTools.FilterPost(result.Data.UserPosts);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data.UserPosts let check = PostTab.MAdapter.PostList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                                { 
                                    PostTab.MAdapter.PostList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { PostTab.MAdapter.NotifyItemRangeInserted(countList, PostTab.MAdapter.PostList.Count - countList); });
                            }
                            else
                            {
                                PostTab.MAdapter.PostList = new ObservableCollection<PostsObject>(result.Data.UserPosts);
                                Activity?.RunOnUiThread(() => { PostTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (PostTab.MAdapter.PostList.Count > 10 && !PostTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short)?.Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity, respond);

                Activity?.RunOnUiThread(() => { ShowEmptyPage("GetPost"); });
            }
            else
            {
                PostTab.Inflated = PostTab.EmptyStateLayout.Inflate();
                var x = new EmptyStateInflater();
                x.InflateLayout(PostTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                PostTab.MainScrollEvent.IsLoading = false;
            }
            PostTab.MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage(string type)
        {
            try
            {
                if (PostTab.SwipeRefreshLayout != null) PostTab.SwipeRefreshLayout.Refreshing = false;
                PostTab.MainScrollEvent.IsLoading = false;

                if (type == "GetPost")
                { 
                    if (PostTab.MAdapter.PostList.Count > 0)
                    {
                        PostTab.MRecycler.Visibility = ViewStates.Visible;
                        PostTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        PostTab.MRecycler.Visibility = ViewStates.Gone;

                        PostTab.Inflated ??= PostTab.EmptyStateLayout.Inflate();

                        var x = new EmptyStateInflater();
                        x.InflateLayout(PostTab.Inflated, EmptyStateInflater.Type.NoPost);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                        }
                        PostTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else if (type == "ProfilePrivate")
                {
                    PostTab.MRecycler.Visibility = ViewStates.Gone;

                    PostTab.Inflated ??= PostTab.EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(PostTab.Inflated, EmptyStateInflater.Type.ProfilePrivate);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                        x.EmptyStateButton.Click += TryAgainButton_Click;
                    }
                }
            }
            catch (Exception e)
            {
                if (PostTab.SwipeRefreshLayout != null) PostTab.SwipeRefreshLayout.Refreshing = false;
                PostTab.MainScrollEvent.IsLoading = false;
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

        private void TryAgainButton_Click(object sender, EventArgs e)
        {
            StartApiService();
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