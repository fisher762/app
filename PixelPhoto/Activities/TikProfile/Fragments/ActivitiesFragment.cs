using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.TikProfile.Adapters;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.User;
using Xamarin.Facebook.Ads;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace PixelPhoto.Activities.TikProfile.Fragments
{
    public class ActivitiesFragment : Fragment
    {
        #region Variables Basic

        private HomeActivity GlobalContext;
        private TikProfileFragment ContextProfile;
        public LastActivitiesAdapter MAdapter;
        public SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        public View Inflated;
        public RecyclerViewOnScrollListener MainScrollEvent;
        private AdView BannerAd;
        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark_Base) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Base);
                var localInflater = inflater.CloneInContext(contextThemeWrapper);
                var view = localInflater?.Inflate(Resource.Layout.MainFragmentLayout, container, false);
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

                ContextProfile = (TikProfileFragment)ParentFragment;
                GlobalContext = (HomeActivity)Activity ?? HomeActivity.GetInstance();

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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDestroy()
        {
            try
            {
                BannerAd?.Destroy();
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
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                var adContainer = view.FindViewById<LinearLayout>(Resource.Id.bannerContainer);
                BannerAd = AdsFacebook.InitAdView(Activity, adContainer); 
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
                MAdapter = new LastActivitiesAdapter(Activity) { LastActivitiesList = new ObservableCollection<ActivitiesObject.Activity>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<ActivitiesObject.Activity>(Activity, MAdapter, sizeProvider, 8);
                 MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                EmptyStateLayout.Visibility = ViewStates.Gone;
                MRecycler.Visibility = ViewStates.Visible;

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

        #region Event

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.LastActivitiesList.LastOrDefault();
                if (item == null || string.IsNullOrEmpty(item.Id)) return;
                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ContextProfile.GetActivities(item.Id) });
                else
                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, LastActivitiesAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (item.Type.Contains("liked__post") || item.Type.Contains("commented_on_post"))
                    {
                        if (item.PostData != null) GlobalContext.OpenNewsFeedItem(item.PostId ,item.PostData.Value.PostDataClass);
                    }  
                    else if (item.UserData != null) 
                        AppTools.OpenProfile(Activity, item.UserId, item.UserData); 
                }
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
                MAdapter.LastActivitiesList.Clear();
                MAdapter.NotifyDataSetChanged();

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ContextProfile.GetActivities() });
                else
                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}