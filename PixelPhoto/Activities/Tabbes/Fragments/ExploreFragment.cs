using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using Google.Android.Material.AppBar;
using Liaoinstan.SpringViewLib.Widgets;
using PixelPhoto.Activities.Search;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.PullSwipeStyles;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace PixelPhoto.Activities.Tabbes.Fragments
{
    public class ExploreFragment : Fragment, SpringView.IOnFreshListener
    {
        #region Variables Basic

        private RecyclerView SuggestionsRecylerView, ExploreRecylerView;
        private SuggestionsAdapter SuggestionsAdapter;
        private ExploreAdapter ExploreAdapter;
        private TextView  TxtMoreSuggested;
        private ProgressBar ProgressBarLoader;
        private LinearLayout MainLayoutSugettion;
        private AutoCompleteTextView SearchViewBox;
        private ViewStub EmptyStateLayout;
        private AppBarLayout AppBarLayout;
        private SearchFragment SearchFragment;
        private RecyclerViewOnScrollListener MainScrollEvent, SeconderScrollEvent;
        private HomeActivity GlobalContext;
        private SpringView SwipeRefreshLayout;
        private View Inflated;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity ?? HomeActivity.GetInstance();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.TExploreLayout, container, false); 
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
                SetRecyclerViewAdapters();

                StartApiService();

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

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                AppBarLayout = (AppBarLayout)view.FindViewById(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(false);

                TxtMoreSuggested = (TextView)view.FindViewById(Resource.Id.iv_more_folowers);
                TxtMoreSuggested.Click += TxtMoreSuggestedOnClick;

                SuggestionsRecylerView = (RecyclerView)view.FindViewById(Resource.Id.Recyler);
                ExploreRecylerView = (RecyclerView)view.FindViewById(Resource.Id.featuredRecyler);
                ProgressBarLoader = (ProgressBar)view.FindViewById(Resource.Id.sectionProgress);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                MainLayoutSugettion = (LinearLayout)view.FindViewById(Resource.Id.layoutSugettionSection);
                MainLayoutSugettion.Visibility = ViewStates.Invisible;

                SearchViewBox = view.FindViewById<AutoCompleteTextView>(Resource.Id.searchViewBox);
              
               // SearchViewLinearLayout = (LinearLayout)view.FindViewById(Resource.Id.searchviewLinearLayout);
                 

                SwipeRefreshLayout = (SpringView)view.FindViewById(Resource.Id.material_style_ptr_frame);
                SwipeRefreshLayout.SetType(SpringView.Type.Overlap);
                SwipeRefreshLayout.Header = new DefaultHeader(Activity);
                SwipeRefreshLayout.Footer = new DefaultFooter(Activity);
                SwipeRefreshLayout.Enable = true;
                SwipeRefreshLayout.EnableFooter = false;
                SwipeRefreshLayout.SetListener(this);

                if (AppSettings.SetTabDarkTheme)
                {
                    //SearchViewLinearLayout.SetBackgroundResource(Resource.Drawable.search_round_corners_dark); 
                    SearchViewBox.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);  
                } 
                SearchViewBox.Click += SearchViewLinearLayoutOnClick; 
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
                var toolbar = view.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                GlobalContext.SetToolBar(toolbar, " ", false);
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
                SuggestionsAdapter = new SuggestionsAdapter(Activity,1);
                var suggestionsLayoutManager = new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false);
                SuggestionsRecylerView.SetLayoutManager(suggestionsLayoutManager);
                SuggestionsRecylerView.HasFixedSize = true;
                SuggestionsRecylerView.SetItemViewCacheSize(10);
                SuggestionsRecylerView.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProviderSuggestions = new FixedPreloadSizeProvider(50, 50);
                var preLoaderSuggestions = new RecyclerViewPreloader<UserDataObject>(Activity, SuggestionsAdapter, sizeProviderSuggestions, 10);
                SuggestionsRecylerView.AddOnScrollListener(preLoaderSuggestions);
                SuggestionsAdapter.ItemClick += SuggestionsAdapterOnItemClick;
                SuggestionsRecylerView.SetAdapter(SuggestionsAdapter);
                  
                var xamarinRecyclerViewOnScrollListener2 = new RecyclerViewOnScrollListener(suggestionsLayoutManager);
                SeconderScrollEvent = xamarinRecyclerViewOnScrollListener2;
                SeconderScrollEvent.LoadMoreEvent += SeconderScrollEventOnLoadMoreEvent;
                SuggestionsRecylerView.AddOnScrollListener(xamarinRecyclerViewOnScrollListener2);
                SeconderScrollEvent.IsLoading = false;
                 
                //*******************************************************************
                 
                ExploreAdapter = new ExploreAdapter(Activity); 
                var mLayoutManager = new GridLayoutManager(Activity, 3);
                mLayoutManager.SetSpanSizeLookup(new MySpanSizeLookup(8, 1, 2));
                ExploreRecylerView.AddItemDecoration(new GridSpacingItemDecoration(1, 1, true));
                ExploreRecylerView.SetLayoutManager(mLayoutManager);
                ExploreRecylerView.HasFixedSize = true;
                ExploreRecylerView.SetItemViewCacheSize(10);
                ExploreRecylerView.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new ViewPreloadSizeProvider();
                var preLoader = new RecyclerViewPreloader<PostsObject>(Activity, ExploreAdapter, sizeProvider, 16);
                ExploreRecylerView.AddOnScrollListener(preLoader);
                ExploreAdapter.ItemClick += ExploreAdapterOnItemClick;
                ExploreRecylerView.SetAdapter(ExploreAdapter);
                 
                var xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(mLayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                ExploreRecylerView.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Events

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                var item = ExploreAdapter.PostList.LastOrDefault();
                               
                if (item != null && !string.IsNullOrEmpty(item.PostId.ToString()) && !MainScrollEvent.IsLoading)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> {  () => LoadExploreAsync(item.PostId.ToString()) });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SeconderScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                var item = SuggestionsAdapter.SuggestionsList.LastOrDefault();
                           
               if (item != null && !string.IsNullOrEmpty(item.UserId) && !SeconderScrollEvent.IsLoading)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadSuggestionsAsync(item.UserId) }); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //All Viewer suggestion
        private void TxtMoreSuggestedOnClick(object sender, EventArgs e)
        {
            try
            {
                var bundle = new Bundle();
                bundle.PutString("type", "suggestion");
                var profileFragment = new AllViewerFragment
                {
                    Arguments = bundle
                };

                GlobalContext.OpenFragment(profileFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchViewLinearLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var bundle = new Bundle();
                bundle.PutString("Key", "");
                SearchFragment = new SearchFragment
                {
                    Arguments = bundle
                };

                GlobalContext.OpenFragment(SearchFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ExploreAdapterOnItemClick(object sender, ExploreAdapterViewHolderClickEventArgs e)
        {
            try
            {
                var item = ExploreAdapter.PostList[e.Position];
                if (item != null)
                {
                    GlobalContext.OpenNewsFeedItem(item.PostId.ToString(), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SuggestionsAdapterOnItemClick(object sender, SuggestionsAdapterClickEventArgs e)
        {
            try
            {
                var item = SuggestionsAdapter.SuggestionsList[e.Position];
                if (item != null)
                {
                    AppTools.OpenProfile(Activity, item.UserId, item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region SpanSize

        private class MySpanSizeLookup : GridLayoutManager.SpanSizeLookup
        {
            private readonly int SpanPos;
            private readonly int SpanCnt1;
            private readonly int SpanCnt2;

            public MySpanSizeLookup(int spanPos, int spanCnt1, int spanCnt2)
            {
                SpanPos = spanPos;
                SpanCnt1 = spanCnt1;
                SpanCnt2 = spanCnt2;
            }

            public override int GetSpanSize(int position)
            {
                return position % SpanPos == 0 ? SpanCnt2 : SpanCnt1;
            }
        }

        #endregion SpanSize

        #region Refresh

        public void OnRefresh()
        {
            try
            {
                SuggestionsAdapter.SuggestionsList.Clear();
                SuggestionsAdapter.NotifyDataSetChanged();

                ExploreAdapter.PostList.Clear();
                ExploreAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;
                SeconderScrollEvent.IsLoading = false;

                StartApiService(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnLoadMore()
        {
            try
            {
                var item = ExploreAdapter.PostList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.PostId.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService("0", item.PostId.ToString());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Data Api 

        private void StartApiService(string offsetSuggestion = "0" , string offsetExplore = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadSuggestionsAsync(offsetSuggestion), () => LoadExploreAsync(offsetExplore) });
        }

        private async Task LoadSuggestionsAsync(string offset = "0")
        {
            if (SeconderScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                SeconderScrollEvent.IsLoading = true;

                var countList = SuggestionsAdapter.SuggestionsList.Count;
                (var apiStatus, var respond) = await RequestsAsync.User.FetchSuggestionsUsers(offset,"14" );
                if (apiStatus != 200 || !(respond is GetUserObject result) || result.Data == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = SuggestionsAdapter.SuggestionsList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                SuggestionsAdapter.SuggestionsList.Add(item);
                            }

                            Activity?.RunOnUiThread(() => { SuggestionsAdapter.NotifyItemRangeInserted(countList, respondList); });
                        }
                        else
                        {
                            SuggestionsAdapter.SuggestionsList = new ObservableCollection<UserDataObject>(result.Data);
                            Activity?.RunOnUiThread(() =>
                            {
                                SuggestionsAdapter.NotifyDataSetChanged();

                                if (MainLayoutSugettion.Visibility != ViewStates.Visible)
                                    MainLayoutSugettion.Visibility = ViewStates.Visible; 
                            });
                        }
                    }
                    else
                    {
                        if (SuggestionsAdapter.SuggestionsList.Count > 10 && !SuggestionsRecylerView.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short)?.Show();
                    }
                }
                SeconderScrollEvent.IsLoading = false;
            }
            else
            { 
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            SeconderScrollEvent.IsLoading = false;
        }

        private async Task LoadExploreAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                var countList = ExploreAdapter.PostList.Count;
                var (apiStatus, respond) = await RequestsAsync.Post.FetchExplorePosts(offset, "25");
                if (apiStatus != 200 || !(respond is FetchListPostsObject result) || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        result.Data = AppTools.FilterPost(result.Data);

                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = ExploreAdapter.PostList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                            { 
                                ExploreAdapter.PostList.Add(item);
                            }
                            MainScrollEvent.IsLoading = false;
                            Activity?.RunOnUiThread(() => { ExploreAdapter.NotifyItemRangeInserted(countList, ExploreAdapter.PostList.Count); });
                        }
                        else
                        {
                            ExploreAdapter.PostList = new ObservableCollection<PostsObject>(result.Data);
                            Activity?.RunOnUiThread(() => { ExploreAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (ExploreAdapter.PostList.Count > 10 && !ExploreRecylerView.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short)?.Show();
                    }
                }
                MainScrollEvent.IsLoading = false;
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
                SwipeRefreshLayout.OnFinishFreshAndLoad();

                if (ProgressBarLoader.Visibility == ViewStates.Visible)
                {
                    ProgressBarLoader.Visibility = ViewStates.Gone; 
                    AppBarLayout.SetExpanded(true);
                }
                 
                if (ExploreAdapter.PostList.Count > 0)
                {
                    ExploreRecylerView.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    ExploreRecylerView.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoPost);
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
                ProgressBarLoader.Visibility = ViewStates.Gone;
                SwipeRefreshLayout.OnFinishFreshAndLoad();
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