using Android.OS;
using Android.Views;
using Android.Widget;
using PixelPhoto.Activities.Search.Adapters;
using PixelPhoto.Activities.Tabbes;
using System;
using System.Collections.ObjectModel;
using Android.Graphics;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using PixelPhoto.Activities.Posts.page;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.User;
using Fragment = AndroidX.Fragment.App.Fragment;
 
namespace PixelPhoto.Activities.Search
{
    public class SearchHashtagFragment : Fragment
    {
        #region Variables Basic

        public SearchHashtagAdapter MAdapter;
        private HomeActivity GlobalContext;
        private SearchFragment ContextSearch;
        private SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private ProgressBar ProgressBarLoader;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        public View Inflated;
         
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
             
                GlobalContext = (HomeActivity)Activity ?? HomeActivity.GetInstance();
                ContextSearch = (SearchFragment)ParentFragment;

                //Get Value 
                InitComponent(view);
                SetRecyclerViewAdapters(); 
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
                MAdapter = new SearchHashtagAdapter(Activity)
                {
                    HashTagsList = new ObservableCollection<SearchUsersHastagsObject.HashObject>()
                };
                LayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MAdapter.ItemClick += MAdapterOnItemClick;
                MRecycler.SetAdapter(MAdapter); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnItemClick(object sender, SearchHashtagAdapterAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position >= 0)
                {
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        // Show All Post By Hash
                        var bundle = new Bundle();
                        bundle.PutString("HashId", item.Id.ToString());
                        bundle.PutString("HashName", Methods.FunString.DecodeString(item.Tag));

                        var profileFragment = new HashTagPostFragment
                        {
                            Arguments = bundle
                        };

                        GlobalContext.OpenFragment(profileFragment);
                    }
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