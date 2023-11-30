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
using AndroidX.ViewPager2.Widget;
using Com.Tuyenmonkey.Textdecorator;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using PixelPhoto.Activities.Favorites;
using PixelPhoto.Activities.MyContacts;
using PixelPhoto.Activities.MyProfile;
using PixelPhoto.Activities.SettingsUser;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.TikProfile.Fragments;
using PixelPhoto.Adapters;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.TikProfile
{
    public class TikProfileFragment : Fragment, TabLayoutMediator.ITabConfigurationStrategy
    {
        #region  Variables Basic

        private HomeActivity GlobalContext;
        private MainTabAdapter Adapter;
        public TextView TxtPostCount;
        private TextView TxtUserName, BtnSettings, TxtFullName, TxtFollowingCount, TxtFollowersCount, BtnEditProfile;
        public ImageView ImageUser;
        private LinearLayout FollowingLayout, FollowersLayout, PostLayout;
        private TabLayout TabLayout;
        private ViewPager2 ViewPager;
        public MyPostFragment MyPostTab;
        public ActivitiesFragment ActivitiesTab;
        private CollapsingToolbarLayout CollapsingToolbar;
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
                var view = inflater.Inflate(Resource.Layout.TikProfileLayout, container, false);
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

                InitComponent(view);
                InitToolbar(view);
                AddOrRemoveEvent(true);

                LoadProfile();
                StartApiService();
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

                TxtUserName = view.FindViewById<TextView>(Resource.Id.username);
                BtnSettings = view.FindViewById<TextView>(Resource.Id.setting_btn);

                ImageUser = view.FindViewById<ImageView>(Resource.Id.user_image);
                TxtFullName = view.FindViewById<TextView>(Resource.Id.fullname);

                FollowingLayout = view.FindViewById<LinearLayout>(Resource.Id.following_layout);
                TxtFollowingCount = view.FindViewById<TextView>(Resource.Id.following_count_txt);

                FollowersLayout = view.FindViewById<LinearLayout>(Resource.Id.followers_layout);
                TxtFollowersCount = view.FindViewById<TextView>(Resource.Id.followers_count_txt);

                PostLayout = view.FindViewById<LinearLayout>(Resource.Id.post_layout);
                TxtPostCount = view.FindViewById<TextView>(Resource.Id.post_count_txt);

                BtnEditProfile = view.FindViewById<TextView>(Resource.Id.edit_profile_btn);

                ViewPager = view.FindViewById<ViewPager2>(Resource.Id.pager);
                TabLayout = view.FindViewById<TabLayout>(Resource.Id.tabs);

                TxtFollowingCount.Text = "0";
                TxtFollowersCount.Text = "0";
                TxtPostCount.Text = "0";
               
                if (AppSettings.SetTabDarkTheme)
                {
                    TxtFullName.SetTextColor(Color.White);
                    BtnSettings.SetTextColor(Color.White);
                }
                 
                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach();

                TabLayout.GetTabAt(0).SetIcon(Resource.Drawable.ic_tab_more);
                TabLayout.GetTabAt(1).SetIcon(Resource.Drawable.ic_action_like_1);

                // set icon color pre-selected
                TabLayout.GetTabAt(0).Icon.SetColorFilter(new PorterDuffColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray, PorterDuff.Mode.SrcIn));
                TabLayout.GetTabAt(1).Icon.SetColorFilter(new PorterDuffColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray, PorterDuff.Mode.SrcIn));

                TabLayout.GetTabAt(0).Icon.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor(AppSettings.MainColor), PorterDuff.Mode.SrcIn));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, BtnSettings, IonIconsFonts.Settings);
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
                    BtnSettings.Click += ImgSettingOnClick;
                    ImageUser.Click += ImageUserOnClick;
                    BtnEditProfile.Click += BtnEditProfileOnClick;
                    FollowersLayout.Click += LinFollowersOnClick;
                    FollowingLayout.Click += LinFollowingOnClick;
                    PostLayout.Click += LinFavoritesOnClick;
                    TabLayout.TabSelected += TabLayoutOnTabSelected;
                    TabLayout.TabUnselected += TabLayoutOnTabUnselected;
                }
                else
                {
                    BtnSettings.Click -= ImgSettingOnClick;
                    ImageUser.Click -= ImageUserOnClick;
                    BtnEditProfile.Click -= BtnEditProfileOnClick;
                    FollowersLayout.Click -= LinFollowersOnClick;
                    FollowingLayout.Click -= LinFollowingOnClick;
                    PostLayout.Click -= LinFavoritesOnClick;
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
                MyPostTab = new MyPostFragment();
                ActivitiesTab = new ActivitiesFragment();

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(MyPostTab, "");
                Adapter.AddFragment(ActivitiesTab, "");

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
                tab.SetText(Adapter.GetFragment(position));
                if (position == 0)
                {
                    tab.SetIcon(Resource.Drawable.ic_tab_more);
                } 
                else if (position == 1)
                {
                    tab.SetIcon(Resource.Drawable.ic_action_like_1);
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

        // Show Setting
        private void ImgSettingOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Context, typeof(SettingsUserActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Change Image
        private void ImageUserOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.OpenDialogGallery("MyProfile");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Show EditProfile
        private void BtnEditProfileOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.StartActivityForResult(new Intent(Context, typeof(EditProfileActivity)), 3000);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Show Favorites
        private void LinFavoritesOnClick(object sender, EventArgs e)
        {
            try
            {
                var bundle = new Bundle();

                var myFavoritesFragment = new FavoritesFragment
                {
                    Arguments = bundle
                };

                GlobalContext.OpenFragment(myFavoritesFragment);
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

                bundle.PutString("UserId", UserDetails.UserId);
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

                bundle.PutString("UserId", UserDetails.UserId);
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
         
        #region Load My Profile Api 

        private async void LoadProfile()
        {
            try
            {
                if (ListUtils.MyProfileList.Count == 0)
                    await ApiRequest.GetProfile_Api(Activity);

                var data = ListUtils.MyProfileList.FirstOrDefault();
                if (data != null)
                {
                    GlideImageLoader.LoadImage(Activity, data.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                    //TxtFullName.Text = AppTools.GetNameFinal(data);
                    CollapsingToolbar.Title = AppTools.GetNameFinal(data);
                    TxtUserName.Text = "@" + data.Username;

                    TxtPostCount.Text = Methods.FunString.FormatPriceValue(int.Parse(data.Favourites));

                    if (data.Followers != null && int.TryParse(data.Followers, out var numberFollowers))
                        TxtFollowersCount.Text = Methods.FunString.FormatPriceValue(numberFollowers);

                    if (data.Following != null && int.TryParse(data.Following, out var numberFollowing))
                        TxtFollowingCount.Text = Methods.FunString.FormatPriceValue(numberFollowing);

                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");
                    TxtFullName.SetTypeface(font, TypefaceStyle.Normal);

                    var textHighLighter = AppTools.GetNameFinal(data);

                    if (data.Verified == "1")
                        textHighLighter += " " + IonIconsFonts.CheckmarkCircle;

                    if (data.BusinessAccount == "1")
                    {
                        var textIsPro = " " + IonIconsFonts.LogoUsd;
                        textHighLighter += textIsPro;
                    }

                    var decorator = TextDecorator.Decorate(TxtFullName, textHighLighter);

                    if (data.Verified == "1")
                        decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircle);

                    if (data.BusinessAccount == "1")
                        decorator.SetTextColor(Resource.Color.Post_IsBusiness, IonIconsFonts.LogoUsd);

                    decorator.Build();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Post

        private void StartApiService(string offsetPost = "0", string offsetActivities = "0")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetPost(offsetPost), () => GetActivities(offsetActivities) });
            else
                Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
        }

        public async Task GetPost(string offset = "0")
        {
            if (MyPostTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MyPostTab.MainScrollEvent.IsLoading = true;

                var countList = MyPostTab.MAdapter.PostList.Count;
                (var apiStatus, var respond) = await RequestsAsync.Post.FetchUserPostsById(UserDetails.UserId, "24", offset);
                if (apiStatus.Equals(200))
                {
                    if (respond is FetchUserPostsByUserIdObject result)
                    {
                        Activity?.RunOnUiThread(() =>
                        {
                            TxtFollowersCount.Text = Methods.FunString.FormatPriceValue(result.Data.UserFollowers);
                            TxtFollowingCount.Text = Methods.FunString.FormatPriceValue(result.Data.UserFollowing);
                        });

                        var respondList = result.Data.UserPosts.Count;
                        if (respondList > 0)
                        {
                            result.Data.UserPosts = AppTools.FilterPost(result.Data.UserPosts);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data.UserPosts let check = MyPostTab.MAdapter.PostList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                                { 
                                    MyPostTab.MAdapter.PostList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { MyPostTab.MAdapter.NotifyItemRangeInserted(countList, MyPostTab.MAdapter.PostList.Count - countList); });
                            }
                            else
                            {
                                MyPostTab.MAdapter.PostList = new ObservableCollection<PostsObject>(result.Data.UserPosts);
                                Activity?.RunOnUiThread(() => { MyPostTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MyPostTab.MAdapter.PostList.Count > 10 && !MyPostTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    MyPostTab.MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }

                Activity?.RunOnUiThread(() => { ShowEmptyPage("GetPost"); });
            }
            else
            {
                MyPostTab.Inflated = MyPostTab.EmptyStateLayout.Inflate();
                var x = new EmptyStateInflater();
                x.InflateLayout(MyPostTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MyPostTab.MainScrollEvent.IsLoading = false;
            }
            MyPostTab.MainScrollEvent.IsLoading = false;
        }

        public async Task GetActivities(string offset = "0")
        {
            if (ActivitiesTab.MainScrollEvent.IsLoading)
                return;

            ActivitiesTab.MainScrollEvent.IsLoading = true;
            var countList = ActivitiesTab.MAdapter.LastActivitiesList.Count;
            (var apiStatus, var respond) = await RequestsAsync.User.FetchActivities("20", offset);
            if (apiStatus.Equals(200))
            {
                if (respond is ActivitiesObject result)
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = ActivitiesTab.MAdapter.LastActivitiesList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                ActivitiesTab.MAdapter.LastActivitiesList.Add(item);
                            }

                            Activity?.RunOnUiThread(() => { ActivitiesTab.MAdapter.NotifyItemRangeInserted(countList, ActivitiesTab.MAdapter.LastActivitiesList.Count - countList); });
                        }
                        else
                        {
                            ActivitiesTab.MAdapter.LastActivitiesList = new ObservableCollection<ActivitiesObject.Activity>(result.Data);
                            Activity?.RunOnUiThread(() => { ActivitiesTab.MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (ActivitiesTab.MAdapter.LastActivitiesList.Count > 10 && !ActivitiesTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity?.RunOnUiThread(() => { ShowEmptyPage("GetActivities"); });
            ActivitiesTab.MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage(string type)
        {
            try
            { 
                if (type == "GetPost")
                {
                    MyPostTab.MainScrollEvent.IsLoading = false;
                    MyPostTab.SwipeRefreshLayout.Refreshing = false;

                    if (MyPostTab.MAdapter.PostList.Count > 0)
                    {
                        MyPostTab.MRecycler.Visibility = ViewStates.Visible;
                        MyPostTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        MyPostTab.MRecycler.Visibility = ViewStates.Gone;

                        MyPostTab.Inflated ??= MyPostTab.EmptyStateLayout.Inflate();

                        var x = new EmptyStateInflater();
                        x.InflateLayout(MyPostTab.Inflated, EmptyStateInflater.Type.NoPost);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                        }
                        MyPostTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else if (type == "GetActivities")
                {
                    ActivitiesTab.MainScrollEvent.IsLoading = false;
                    ActivitiesTab.SwipeRefreshLayout.Refreshing = false;

                    if (ActivitiesTab.MAdapter.LastActivitiesList.Count > 0)
                    {
                        ActivitiesTab.MRecycler.Visibility = ViewStates.Visible;
                        ActivitiesTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        ActivitiesTab.MRecycler.Visibility = ViewStates.Gone;

                        ActivitiesTab.Inflated ??= ActivitiesTab.EmptyStateLayout.Inflate();

                        var x = new EmptyStateInflater();
                        x.InflateLayout(ActivitiesTab.Inflated, EmptyStateInflater.Type.NoActivities);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                        }
                        ActivitiesTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
            }
            catch (Exception e)
            {
                MyPostTab.MainScrollEvent.IsLoading = false;
                MyPostTab.SwipeRefreshLayout.Refreshing = false;

                ActivitiesTab.MainScrollEvent.IsLoading = false;
                ActivitiesTab.SwipeRefreshLayout.Refreshing = false;
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

    }
}