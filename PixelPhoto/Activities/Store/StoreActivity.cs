using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Tabs;
using PixelPhoto.Activities.Base;
using PixelPhoto.Activities.Store.Fragment;
using PixelPhoto.Adapters;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Store;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.Store
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class StoreActivity : BaseActivity, TabLayoutMediator.ITabConfigurationStrategy
    {
        #region Variables Basic

        private MainTabAdapter Adapter;
        private ViewPager2 ViewPager;
        public StoreFragment StoreTab;
        public MyStoreFragment MyStoreTab;
        public MyDownloadsStoreFragment MyDownloadsTab;
        private TabLayout TabLayout;
        private FloatingActionButton ActionButton;
        private TextView FilterButton;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
              
                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                
                // Create your application here
                SetContentView(Resource.Layout.StoreLayout);
                 
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                StartApiService();
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
                AddOrRemoveStore(true);
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
                AddOrRemoveStore(false);
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
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                FilterButton = (TextView)FindViewById(Resource.Id.toolbar_title);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, FilterButton, IonIconsFonts.Options);
                FilterButton.SetTextSize(ComplexUnitType.Sp, 25f);
                FilterButton.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                FilterButton.Visibility = ViewStates.Visible;
                 
                ViewPager = FindViewById<ViewPager2>(Resource.Id.viewpager);
                TabLayout = FindViewById<TabLayout>(Resource.Id.tabs);

                ActionButton = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                ActionButton.Visibility = ViewStates.Visible;
                ActionButton.SetImageResource(Resource.Drawable.ic_add);
                 
                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach();

                TabLayout.SetTabTextColors(AppSettings.SetTabDarkTheme ? Color.White : Color.Black, Color.ParseColor(AppSettings.MainColor));
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
                    toolbar.Title = GetString(Resource.String.Lbl_Store);

                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);

                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveStore(bool addStore)
        {
            try
            {
                // true +=  // false -=
                if (addStore)
                {
                    FilterButton.Click += FilterButtonOnClick;
                    ActionButton.Click += ActionButtonOnClick;
                }
                else
                {
                    FilterButton.Click -= FilterButtonOnClick;
                    ActionButton.Click -= ActionButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void DestroyBasic()
        {
            try
            {
                ViewPager = null!;
                TabLayout = null!;
                ActionButton = null!;
                FilterButton = null!;  
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tap

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                StoreTab = new StoreFragment();
                MyStoreTab = new MyStoreFragment();
                MyDownloadsTab = new MyDownloadsStoreFragment();

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(StoreTab, GetText(Resource.String.Lbl_Store));
                Adapter.AddFragment(MyStoreTab, GetText(Resource.String.Lbl_MyStore));
                Adapter.AddFragment(MyDownloadsTab, GetText(Resource.String.Lbl_MyDownloads));

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
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Events

        //Create new store
        private void ActionButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(CreateStoreActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FilterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var storeFilter = new FilterStoreDialogFragment(this);
                storeFilter.Show(SupportFragmentManager, storeFilter.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Store

        private void StartApiService(string offsetStore = "0", string offsetMyStore = "0")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetStore(offsetStore), () => GetMyStore(offsetMyStore) , GetMyDownloadsStore });
            else
                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
        }

        public async Task GetStore(string offset = "0")
        {
            if (StoreTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                StoreTab.MainScrollEvent.IsLoading = true;
                var countList = StoreTab.MAdapter.StoreList.Count;

                (var apiStatus, var respond) = await RequestsAsync.Store.ExploreAllStore(offset, UserDetails.StoreTitle, UserDetails.StoreTags, UserDetails.StoreCategory, UserDetails.StoreLicenseType, UserDetails.StorePriceMin, UserDetails.StorePriceMax);
                if (apiStatus != 200 || !(respond is StoreObject result) || result.Data == null)
                {
                    StoreTab.MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = StoreTab.MAdapter.StoreList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                StoreTab.MAdapter.StoreList.Add(item);
                            }

                            RunOnUiThread(() => { StoreTab.MAdapter.NotifyItemRangeInserted(countList, StoreTab.MAdapter.StoreList.Count - countList); });
                        }
                        else
                        {
                            StoreTab.MAdapter.StoreList = new ObservableCollection<StoreDataObject>(result.Data);
                            RunOnUiThread(() => { StoreTab.MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (StoreTab.MAdapter.StoreList.Count > 10 && !StoreTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreStore), ToastLength.Short)?.Show();
                    }
                }
                
                RunOnUiThread(() => ShowEmptyPage("GetStore"));
            }
            else
            {
                StoreTab.Inflated = StoreTab.EmptyStateLayout.Inflate();
                var x = new EmptyStateInflater();
                x.InflateLayout(StoreTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                StoreTab.MainScrollEvent.IsLoading = false;
            }
        }

        public async Task GetMyStore(string offset = "0")
        {
            if (MyStoreTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MyStoreTab.MainScrollEvent.IsLoading = true;
                var countList = MyStoreTab.MAdapter.StoreList.Count;

                (var apiStatus, var respond) = await RequestsAsync.Store.ExploreUserStore(UserDetails.UserId , offset);
                if (apiStatus != 200 || !(respond is StoreObject result) || result.Data == null)
                {
                    StoreTab.MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = MyStoreTab.MAdapter.StoreList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                MyStoreTab.MAdapter.StoreList.Add(item);
                            }

                            RunOnUiThread(() => { MyStoreTab.MAdapter.NotifyItemRangeInserted(countList, MyStoreTab.MAdapter.StoreList.Count - countList); });
                        }
                        else
                        {
                            MyStoreTab.MAdapter.StoreList = new ObservableCollection<StoreDataObject>(result.Data);
                            RunOnUiThread(() => { MyStoreTab.MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MyStoreTab.MAdapter.StoreList.Count > 10 && !MyStoreTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreStore), ToastLength.Short)?.Show();
                    }
                } 

                RunOnUiThread(() => ShowEmptyPage("GetMyStore"));
            }
            else
            {
                MyStoreTab.Inflated = MyStoreTab.EmptyStateLayout.Inflate();
                var x = new EmptyStateInflater();
                x.InflateLayout(MyStoreTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MyStoreTab.MainScrollEvent.IsLoading = false;
            }
        }
         
        public async Task GetMyDownloadsStore()
        {
            if (Methods.CheckConnectivity())
            {
                var countList = MyDownloadsTab.MAdapter.StoreList.Count;

                (var apiStatus, var respond) = await RequestsAsync.Store.MyStoreDownloads();
                if (apiStatus != 200 || !(respond is StoreObject result) || result.Data == null)
                {
                    StoreTab.MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = MyDownloadsTab.MAdapter.StoreList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                MyDownloadsTab.MAdapter.StoreList.Add(item);
                            }

                            RunOnUiThread(() => { MyDownloadsTab.MAdapter.NotifyItemRangeInserted(countList, MyDownloadsTab.MAdapter.StoreList.Count - countList); });
                        }
                        else
                        {
                            MyDownloadsTab.MAdapter.StoreList = new ObservableCollection<StoreDataObject>(result.Data);
                            RunOnUiThread(() => { MyDownloadsTab.MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MyDownloadsTab.MAdapter.StoreList.Count > 10 && !MyDownloadsTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreStore), ToastLength.Short)?.Show();
                    }
                } 

                RunOnUiThread(() => ShowEmptyPage("GetMyDownloads"));
            }
            else
            {
                MyDownloadsTab.Inflated = MyDownloadsTab.EmptyStateLayout.Inflate();
                var x = new EmptyStateInflater();
                x.InflateLayout(MyDownloadsTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }
         
        private void ShowEmptyPage(string type)
        {
            try
            {
                switch (type)
                {
                    case "GetStore":
                    {
                        StoreTab.MainScrollEvent.IsLoading = false;
                        StoreTab.SwipeRefreshLayout.Refreshing = false;

                        if (StoreTab.MAdapter.StoreList.Count > 0)
                        {
                            StoreTab.MRecycler.Visibility = ViewStates.Visible;
                            StoreTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            StoreTab.MRecycler.Visibility = ViewStates.Gone;

                            StoreTab.Inflated = StoreTab.EmptyStateLayout.Inflate();

                            var x = new EmptyStateInflater();
                            x.InflateLayout(StoreTab.Inflated, EmptyStateInflater.Type.NoStore);
                            if (!x.EmptyStateButton.HasOnClickListeners)
                            {
                                x.EmptyStateButton.Click += null!;
                            }
                            StoreTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                        }

                        break;
                    }
                    case "GetMyStore":
                    {
                        MyStoreTab.MainScrollEvent.IsLoading = false;
                        MyStoreTab.SwipeRefreshLayout.Refreshing = false;

                        if (MyStoreTab.MAdapter.StoreList.Count > 0)
                        {
                            MyStoreTab.MRecycler.Visibility = ViewStates.Visible;
                            MyStoreTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            MyStoreTab.MRecycler.Visibility = ViewStates.Gone;

                            MyStoreTab.Inflated = MyStoreTab.EmptyStateLayout.Inflate();

                            var x = new EmptyStateInflater();
                            x.InflateLayout(MyStoreTab.Inflated, EmptyStateInflater.Type.NoStore);
                            if (!x.EmptyStateButton.HasOnClickListeners)
                            {
                                x.EmptyStateButton.Click += null!;
                            }
                            MyStoreTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                        }

                        break;
                    }
                    case "GetMyDownloads":
                    {
                        MyDownloadsTab.SwipeRefreshLayout.Refreshing = false;

                        if (MyDownloadsTab.MAdapter.StoreList.Count > 0)
                        {
                            MyDownloadsTab.MRecycler.Visibility = ViewStates.Visible;
                            MyDownloadsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            MyDownloadsTab.MRecycler.Visibility = ViewStates.Gone;

                            MyDownloadsTab.Inflated = MyDownloadsTab.EmptyStateLayout.Inflate();

                            var x = new EmptyStateInflater();
                            x.InflateLayout(MyDownloadsTab.Inflated, EmptyStateInflater.Type.NoStore);
                            if (!x.EmptyStateButton.HasOnClickListeners)
                            {
                                x.EmptyStateButton.Click += null!;
                            }
                            MyDownloadsTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                StoreTab.MainScrollEvent.IsLoading = false;
                StoreTab.SwipeRefreshLayout.Refreshing = false;
                MyStoreTab.MainScrollEvent.IsLoading = false;
                MyStoreTab.SwipeRefreshLayout.Refreshing = false;
                MyDownloadsTab.SwipeRefreshLayout.Refreshing = false;
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