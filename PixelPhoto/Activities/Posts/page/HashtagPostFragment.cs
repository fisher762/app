using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
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
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace PixelPhoto.Activities.Posts.page
{
    public class HashTagPostFragment : Fragment, SpringView.IOnFreshListener
    {
        #region Variables Basic

        private LinearLayout SortLayout;

        public NewsFeedAdapter MAdapter;
        private PRecyclerView MRecycler;
        private ProgressBar ProgressBarLoader;
        private ViewStub EmptyStateLayout;
        private LinearLayoutManager MLayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private View Inflated;
        private string HashId , HashName;
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

                HashId = Arguments.GetString("HashId");
                HashName = Arguments.GetString("HashName");

                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

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
                SortLayout.Visibility = ViewStates.Gone; 
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

                if (!HashName.Contains("#"))
                    HashName = "#" + HashName;

                MainContext.SetToolBar(toolBar, HashName);
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
                MAdapter ??= new NewsFeedAdapter(Activity, MRecycler);
                MAdapter.ItemClick += MAdapterOnItemClick;

                MLayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(MLayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                 
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<PostsObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.ClearOnScrollListeners();
                 MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetXAdapter(MAdapter, false);

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
                var item = MAdapter.PostList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.PostId.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(item.PostId.ToString());
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
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
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

                var countList = MAdapter.PostList.Count;
                var decodeHash = HashName.Replace("#", "");
                var (apiStatus, respond) = await RequestsAsync.Post.FetchPostsByHashtag(decodeHash, offset, "25");
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
                            foreach (var item in from item in result.Data let check = MAdapter.PostList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                            {
                                MAdapter.PostList.Add(item);
                            }

                            Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.PostList.Count - countList); });
                        }
                        else
                        {
                            MAdapter.PostList = new ObservableCollection<PostsObject>(result.Data);
                            Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.PostList.Count > 10 && !MRecycler.CanScrollVertically(1))
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

                SwipeRefreshLayout.OnFinishFreshAndLoad();

                if (ProgressBarLoader.Visibility == ViewStates.Visible)
                    ProgressBarLoader.Visibility = ViewStates.Gone;

                if (MAdapter.PostList.Count > 0)
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
                MAdapter.PostList.Clear();
                MAdapter.NotifyDataSetChanged();

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
                var item = MAdapter.PostList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.PostId.ToString()) && !MainScrollEvent.IsLoading)
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