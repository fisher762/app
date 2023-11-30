using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.Store.Adapters;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Store;
using PixelPhotoClient.RestCalls;
using PixelPhoto.Library.Anjo.Share;
using PixelPhoto.Library.Anjo.Share.Abstractions;
using Xamarin.Facebook.Ads;
using Exception = System.Exception;

namespace PixelPhoto.Activities.Store.Fragment
{
    public class StoreFragment : AndroidX.Fragment.App.Fragment, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        public StoreAdapter MAdapter;
        private StoreActivity ContextStore;
        public SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        public View Inflated;
        public RecyclerViewOnScrollListener MainScrollEvent;
        private AdView BannerAd;
        private StoreDataObject StoreData;
        private string DialogType;
        private int Position;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.MainFragmentLayout, container, false);
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

                ContextStore = (StoreActivity)Activity;

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
                MAdapter = new StoreAdapter(Activity) { StoreList = new ObservableCollection<StoreDataObject>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MAdapter.MoreItemClick += MAdapterOnMoreItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<StoreDataObject>(Activity, MAdapter, sizeProvider, 10);
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

        #endregion

        #region Event

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.StoreList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                {
                    if (Methods.CheckConnectivity())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ContextStore.GetStore(item.Id.ToString()) });
                    }
                    else
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
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
                MAdapter.StoreList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ContextStore.GetStore() });
                else
                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnMoreItemClick(object sender, StoreAdapterViewHolderClickEventArgs e)
        {
            try
            {
                StoreData = MAdapter.StoreList[e.Position];
                if (StoreData != null)
                {
                    DialogType = "More";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    if (StoreData.UserId.ToString() == UserDetails.UserId)
                    {
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Edit));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Delete));
                    }

                    arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Share));

                    dialogList.Title(GetText(Resource.String.Lbl_More));
                    dialogList.Items(arrayAdapter);
                    dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void MAdapterOnItemClick(object sender, StoreAdapterViewHolderClickEventArgs e)
        {
            try
            {
                Position = e.Position;
                var item = MAdapter.StoreList[e.Position];
                if (item != null)
                {
                    var intent = new Intent(Context, typeof(StoreViewActivity));
                    intent.PutExtra("StoreId", item.Id.ToString());
                    intent.PutExtra("storeData", JsonConvert.SerializeObject(item));
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion 

        #region MaterialDialog

        public async void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                var text = itemString.ToString();
                if (text == GetText(Resource.String.Lbl_Edit))
                {
                    //Open Edit Store
                    var intent = new Intent(Context, typeof(EditStoreActivity));
                    intent.PutExtra("StoreId", StoreData.Id);
                    intent.PutExtra("StoreItem", JsonConvert.SerializeObject(StoreData));
                    ContextStore.StartActivityForResult(intent, 246);
                }
                else if (text == GetText(Resource.String.Lbl_Delete))
                {
                    DialogType = "Delete";

                    var dialog = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                    dialog.Title(Resource.String.Lbl_Warning);
                    dialog.Content(GetText(Resource.String.Lbl_DeleteStore));
                    dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else if (text == GetText(Resource.String.Lbl_Copy))
                {
                    Methods.CopyToClipboard(Activity, Client.WebsiteUrl + "/store/" + StoreData.Id);
                }
                else if (text == GetText(Resource.String.Lbl_Share))
                {
                    //Share Plugin same as video
                    if (!CrossShare.IsSupported) return;

                    await CrossShare.Current.Share(new ShareMessage
                    {
                        Title = Methods.FunString.DecodeString(StoreData.Title),
                        Text = "",
                        Url = Client.WebsiteUrl + "/store/" + StoreData.Id
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (DialogType == "Delete")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        // Send Api delete  
                        if (Methods.CheckConnectivity())
                        {
                            var data2 = MAdapter?.StoreList?.FirstOrDefault(a => a.Id == StoreData.Id);
                            if (data2 != null)
                            {
                                MAdapter.StoreList.Remove(data2);
                                MAdapter.NotifyItemRemoved(Position);
                            }

                            var data = HomeActivity.GetInstance()?.ExploreFragmentTheme2?.MAdapter?.StoreAdapter?.StoreList?.FirstOrDefault(a => a.Id == StoreData.Id);
                            if (data != null)
                            {
                                HomeActivity.GetInstance()?.ExploreFragmentTheme2?.MAdapter?.StoreAdapter?.StoreList.Remove(data);
                                HomeActivity.GetInstance()?.ExploreFragmentTheme2?.MAdapter?.StoreAdapter?.NotifyDataSetChanged();
                            }

                            Toast.MakeText(Context, GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short)?.Show();
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Store.DeleteStore(StoreData.Id.ToString()) });
                        }
                        else
                        {
                            Toast.MakeText(Context, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        }
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else
                {
                    if (p1 == DialogAction.Positive)
                    {

                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
    }
}