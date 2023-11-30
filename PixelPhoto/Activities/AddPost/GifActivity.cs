using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using PixelPhoto.Activities.AddPost.Adapters;
using PixelPhoto.Activities.Base;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using SearchView = AndroidX.AppCompat.Widget.SearchView;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.AddPost
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class GifActivity : BaseActivity
    {
        #region Variables Basic

        private GifAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private string SearchKey = "";
        private SearchView SearchView;
        private Toolbar ToolBar;
        private RecyclerViewOnScrollListener MainScrollEvent;
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
                base.OnResume();
                AddOrRemoveEvent(true);
                MAdView?.Resume();
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
                MAdView?.Pause();
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

        #region Menu 
         
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            try
            {
                MenuInflater.Inflate(Resource.Menu.SearchGif_Menu, menu);

                var item = menu.FindItem(Resource.Id.searchUserBar);
                var searchItem = (SearchView)item?.ActionView;

                SearchView = searchItem?.JavaCast<SearchView>();
                SearchView.SetIconifiedByDefault(true);
                SearchView.QueryTextChange += SearchView_OnTextChange;
                SearchView.QueryTextSubmit += SearchView_OnTextSubmit;

                //Change text colors
                var editText = (EditText)SearchView.FindViewById(Resource.Id.search_src_text);
                Methods.SetColorEditText(editText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }

            return base.OnCreateOptionsMenu(menu);
        }

        private void SearchView_OnTextSubmit(object sender, SearchView.QueryTextSubmitEventArgs e)
        {
            try
            { 
                MainScrollEvent.IsLoading = false;

                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;

                SearchKey = e.NewText;

                MAdapter.GifList.Clear();
                MAdapter.NotifyDataSetChanged();

                if (!Methods.CheckConnectivity())
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGif() });

                //Hide keyboard programmatically in MonoDroid
                e.Handled = true;

                SearchView.ClearFocus();

                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(ToolBar.WindowToken, 0);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchView_OnTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            try
            {
                SearchKey = e.NewText;
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
                SwipeRefreshLayout?.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout?.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);

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
                ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = GetText(Resource.String.Lbl_SelectGif);
                    ToolBar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(ToolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    if (AppSettings.SetTabDarkTheme)
                        ToolBar.SetBackgroundResource(Resource.Drawable.linear_gradient_drawable_Dark);
                }
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
                MAdapter = new GifAdapter(this)
                {
                    GifList = new ObservableCollection<GifGiphyClass.Datum>()
                };
                LayoutManager = new GridLayoutManager(this, 2);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.AddItemDecoration(new GridSpacingItemDecoration(1, 2, true));
                MRecycler.SetAdapter(MAdapter);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    MAdapter.ItemClick -= MAdapterOnItemClick;
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

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.GifList.Clear();
                MAdapter.NotifyDataSetChanged();
 
                StartApiService();
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
                if (string.IsNullOrEmpty(SearchKey) || string.IsNullOrWhiteSpace(SearchKey))
                {
                    StartApiService(MAdapter.ItemCount.ToString());
                }
                else
                {
                    if (!Methods.CheckConnectivity())
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGif(MAdapter.ItemCount.ToString()) });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, GifAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    // G_fixed_height_small_url, // UrlGif - view  >> 
                    // G_fixed_height_small_mp4, //MediaGif - sent >>
                    var resultIntent = new Intent();
                    resultIntent.PutExtra("MediaGif", item.Images.FixedHeightSmall.Mp4);
                    resultIntent.PutExtra("UrlGif", item.Images.FixedHeightSmall.Url);
                    SetResult(Result.Ok, resultIntent);
                    Finish();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Gifs 

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadTrendingGif(offset) });
        }

        private async Task LoadGif(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                var countList = MAdapter.GifList.Count;
                var respond = await ApiRequest.SearchGif(SearchKey, offset);
                if (respond != null)
                {
                    if (countList > 0)
                    {
                        foreach (var item in respond)
                        {
                            var check = MAdapter.GifList.FirstOrDefault(a => a.Id == item.Id);
                            if (check == null)
                            {
                                MAdapter.GifList.Add(item);
                            }
                        }
                    }
                    else
                    {
                        MAdapter.GifList = new ObservableCollection<GifGiphyClass.Datum>(respond);
                    }

                    RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
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
            }
        }


        private async Task LoadTrendingGif(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                var countList = MAdapter.GifList.Count;
                var respond = await ApiRequest.TrendingGif(offset);
                if (respond != null)
                {
                    if (countList > 0)
                    {
                        foreach (var item in respond)
                        {
                            var check = MAdapter.GifList.FirstOrDefault(a => a.Id == item.Id);
                            if (check == null)
                            {
                                MAdapter.GifList.Add(item);
                            }
                        }
                    }
                    else
                    {
                        MAdapter.GifList = new ObservableCollection<GifGiphyClass.Datum>(respond); 
                    }

                    RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
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
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.GifList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.Gify);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
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