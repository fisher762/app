using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using PixelPhoto.Activities.MyContacts.Adapters;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using Fragment = AndroidX.Fragment.App.Fragment;


namespace PixelPhoto.Activities.Search
{
    public class SearchUserFragment : Fragment
    {
        #region Variables Basic

        public ContactsAdapter MAdapter;
        private SearchFragment ContextSearch;
        private SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        public ProgressBar ProgressBarLoader;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        public View Inflated;
        public RecyclerViewOnScrollListener MainScrollEvent;
        public string Offset = "0";

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // Use this to return your custom view for this Fragment
                var view = inflater.Inflate(Resource.Layout.SearchUsersLayout, container, false);
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

                ContextSearch = (SearchFragment)ParentFragment;

                //Get Value 
                InitComponent(view);
                SetRecyclerViewAdapters();

                ContextSearch.Search();
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
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
                ProgressBarLoader = (ProgressBar)view.FindViewById(Resource.Id.sectionProgress);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.search_swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                  
                ProgressBarLoader.Visibility = ViewStates.Gone;
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
                MAdapter = new ContactsAdapter(Activity, true, ContactsAdapter.TypeTextSecondary.LastSeen)
                {
                    UserList = new ObservableCollection<UserDataObject>()
                }; 
                LayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<UserDataObject>(Activity, MAdapter, sizeProvider, 10);
                 MRecycler.AddOnScrollListener(preLoader);
                MAdapter.ItemClick += MAdapterOnItemClick;
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

        #endregion

        #region Events

        private void MAdapterOnItemClick(object sender, ContactsAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position >= 0)
                {
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        AppTools.OpenProfile(Activity, item.UserId, item);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Scroll

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                var item = MAdapter.UserList.LastOrDefault();
                if (item != null && MAdapter.UserList.Count > 10 && !MainScrollEvent.IsLoading)
                {
                    Offset = item.UserId;

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ContextSearch.StartSearchRequest });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

    }
}