using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Ads;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using PixelPhoto.Activities.Base;
using PixelPhoto.Helpers.Ads;
using PixelPhotoClient.RestCalls;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.SuggestionsUsers
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SuggestionsUsersActivity : BaseActivity
    { 
        #region Variables Basic

        private SuggestionsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private GridLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private TextView ActionButton;
        private AdView MAdView;

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
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                //Get Data Api
                StartApiService();
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
                MAdView?.Resume();
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
                MAdView?.Pause();
                base.OnPause();
                AddOrRemoveEvent(false);
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
                MAdView?.Destroy();

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
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                ActionButton = FindViewById<TextView>(Resource.Id.toolbar_title);
                ActionButton.Text = GetText(Resource.String.Lbl_Done);
                ActionButton.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                ActionButton.Visibility = ViewStates.Visible;

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new SuggestionsAdapter(this,2)
                {
                    SuggestionsList = new ObservableCollection<UserDataObject>()
                };
                LayoutManager = new GridLayoutManager(this, 3);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<UserDataObject>(this, MAdapter, sizeProvider, 10);
                 MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                var xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;

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
                    toolbar.Title = GetText(Resource.String.Lbl_SuggestionsUsers);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    if (AppSettings.SetTabDarkTheme)
                        toolbar.SetBackgroundResource(Resource.Drawable.linear_gradient_drawable_Dark);
                }
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
                    ActionButton.Click += ActionButtonOnClick; 
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {  
                    ActionButton.Click -= ActionButtonOnClick; 
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void ActionButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Application.Context, typeof(HomeActivity)));
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
 

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.SuggestionsList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.UserId) && !MainScrollEvent.IsLoading)
                    StartApiService(item.UserId);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.SuggestionsList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Api 

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
        }

        private async Task LoadDataAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                var countList = MAdapter.SuggestionsList.Count;
                (var apiStatus, var respond) = await RequestsAsync.User.FetchSuggestionsUsers( offset , "14");
                if (apiStatus != 200 || !(respond is GetUserObject result) || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = MAdapter.SuggestionsList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                MAdapter.SuggestionsList.Add(item);
                            }

                            RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.SuggestionsList.Count - countList); });
                        }
                        else
                        {
                            MAdapter.SuggestionsList = new ObservableCollection<UserDataObject>(result.Data);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.SuggestionsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short)?.Show();
                    }
                }

                RunOnUiThread(ShowEmptyPage);
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

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.SuggestionsList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoUsers);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
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