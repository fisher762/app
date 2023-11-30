using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using Com.Airbnb.Lottie;
using Liaoinstan.SpringViewLib.Widgets;
using PixelPhoto.Activities.Posts.Adapters;
using PixelPhoto.Activities.Posts.Extras;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.PullSwipeStyles;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace PixelPhoto.Activities.Posts.page
{
    public class AllExplorePostFragment : Fragment, SpringView.IOnFreshListener
    {
        #region Variables Basic

        private LinearLayout SortLayout;
        private UserPostAdapter UserPostAdapter;
        private ImageView GridButton, ListButton;
        private GridLayoutManager GridLayout;
        private string NewsFeedStyle = "Grid";

        private NewsFeedAdapter MAdapter;
        private PRecyclerView MRecycler;
        private ProgressBar ProgressBarLoader;
        private ViewStub EmptyStateLayout;
        private LinearLayoutManager MLayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private View Inflated;
        private HomeActivity MainContext;
        private SpringView SwipeRefreshLayout;
         
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
                var view = inflater.Inflate(Resource.Layout.MainRecylerViewLayout, container, false);
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

                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);
                SetNewsFeedStyleToGrid();

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
                MRecycler?.StopVideo();
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
                MRecycler?.ReleasePlayer();
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
                MRecycler = view.FindViewById<PRecyclerView>(Resource.Id.HashtagRecyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
                ProgressBarLoader = view.FindViewById<ProgressBar>(Resource.Id.sectionProgress);

                SwipeRefreshLayout = (SpringView)view.FindViewById(Resource.Id.material_style_ptr_frame);
                SwipeRefreshLayout.SetType(SpringView.Type.Overlap);
                SwipeRefreshLayout.Header = new DefaultHeader(Activity);
                SwipeRefreshLayout.Footer = new DefaultFooter(Activity);
                SwipeRefreshLayout.Enable = true;
                SwipeRefreshLayout.SetListener(this);
                 
                SortLayout = view.FindViewById<LinearLayout>(Resource.Id.sortLayout);
                SortLayout.Visibility = ViewStates.Visible;

                MAdapter = new NewsFeedAdapter(Activity, MRecycler);
                UserPostAdapter = new UserPostAdapter(Activity);
                 
                GridButton = view.FindViewById<ImageView>(Resource.Id.grid_pic);
                ListButton = view.FindViewById<ImageView>(Resource.Id.menu_pic);
                GridButton.Click += GridButton_Click;
                ListButton.Click += ListButton_Click;
                UserPostAdapter.ItemClick += UserPostAdapterOnItemClick;
                MAdapter.ItemClick += MAdapterOnItemClick; 
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
                var toolBar = view.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                MainContext.SetToolBar(toolBar, GetText(Resource.String.Lbl_Explore));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void SetNewsFeedStyleToGrid()
        {
            try
            {
                NewsFeedStyle = "Grid";
                 
                UserPostAdapter ??= new UserPostAdapter(Activity);
                GridLayout = new GridLayoutManager(Activity, AppSettings.SetPostRowHorizontalCount);
                MRecycler.SetLayoutManager(GridLayout);
                MRecycler.AddItemDecoration(new GridSpacingItemDecoration(1, 1, true));
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                if (MAdapter?.PostList?.Count > 0)
                    UserPostAdapter.PostList = new ObservableCollection<PostsObject>(MAdapter.PostList);

                var sizeProvider = new ViewPreloadSizeProvider();
                var preLoader = new RecyclerViewPreloader<PostsObject>(Activity, UserPostAdapter, sizeProvider, 10);
                MRecycler.ClearOnScrollListeners();
                 MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(UserPostAdapter);

                EmptyStateLayout.Visibility = ViewStates.Gone;
                MRecycler.Visibility = ViewStates.Visible;

                var xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(GridLayout);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }
         
        private void SetRecyclerViewAdapters()
        {
            try
            {
                NewsFeedStyle = "Linear";

                MAdapter ??= new NewsFeedAdapter(Activity, MRecycler);
                MLayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(MLayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                if (UserPostAdapter?.PostList?.Count > 0)
                    MAdapter.PostList = new ObservableCollection<PostsObject>(UserPostAdapter.PostList);

                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<PostsObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.ClearOnScrollListeners();
                 MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                var xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(MLayoutManager);
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

        #endregion

        #region Event

        //Show post by id
        private void UserPostAdapterOnItemClick(object sender, UserPostAdapterViewHolderClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                { 
                    var item = UserPostAdapter.PostList[e.Position];
                    if (item != null)
                    {
                        MainContext.OpenNewsFeedItem(item.PostId.ToString(), item);
                    }
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

        private void ListButton_Click(object sender, EventArgs e)
        {
            try
            {
                SetRecyclerViewAdapters();
                ShowEmptyPage();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GridButton_Click(object sender, EventArgs e)
        {
            try
            {
                SetNewsFeedStyleToGrid();
                ShowEmptyPage();
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
                        MRecycler?.ReleasePlayer();
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

        #region Scroll

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                PostsObject item;
                switch (NewsFeedStyle)
                {
                    case "Linear":
                        item = MAdapter?.PostList?.LastOrDefault();
                        if (item != null && !string.IsNullOrEmpty(item.UserId.ToString()) && !MainScrollEvent.IsLoading)
                            StartApiService(item?.PostId.ToString());
                        break;
                    case "Grid": 
                        item = UserPostAdapter?.PostList?.LastOrDefault();
                        if (item != null && !string.IsNullOrEmpty(item.UserId.ToString()) && !MainScrollEvent.IsLoading)
                            StartApiService(item?.PostId.ToString());
                        break;
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Api 

        private void StartApiService(string offsetExplore = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> {  () => LoadExploreAsync(offsetExplore) });
        }
         
        private async Task LoadExploreAsync(string offset = "0")
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
                            foreach (var item in from item in result.Data let check = NewsFeedStyle switch
                            {
                                "Linear" => MAdapter?.PostList?.FirstOrDefault(a => a.PostId == item.PostId),
                                "Grid" => UserPostAdapter?.PostList?.FirstOrDefault(a => a.PostId == item.PostId),
                                _ => null
                            } where (dynamic) check == null select item)
                            {
                                adapter.PostList.Add(item);
                            }

                            MainScrollEvent.IsLoading = false;
                            Activity?.RunOnUiThread(() =>{ adapter.NotifyItemRangeInserted(countList, adapter.PostList.Count); });
                        }
                        else
                        {
                            adapter.PostList = new ObservableCollection<PostsObject>(result.Data);
                            Activity?.RunOnUiThread(() => { adapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (adapter.PostList.Count > 10 && !MRecycler.CanScrollVertically(1))
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
                }

                dynamic adapter = NewsFeedStyle switch
                {
                    "Linear" => MAdapter,
                    "Grid" => UserPostAdapter,
                    _ => UserPostAdapter
                };
                if (adapter?.PostList?.Count > 0)
                { 
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

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

        #region Refresh

        public void OnRefresh()
        {
            try
            {
                dynamic adapter = NewsFeedStyle switch
                {
                    "Linear" => MAdapter,
                    "Grid" => UserPostAdapter,
                    _ => UserPostAdapter
                };

                adapter.PostList.Clear();
                adapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

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
                var item = NewsFeedStyle switch
                {
                    "Linear" => MAdapter?.PostList?.LastOrDefault(),
                    "Grid" => UserPostAdapter?.PostList?.LastOrDefault(),
                    _ => null
                };

                if (item != null && !string.IsNullOrEmpty(item.UserId.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(item.PostId.ToString());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

    }
}