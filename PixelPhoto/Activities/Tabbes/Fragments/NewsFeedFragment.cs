using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Com.Airbnb.Lottie;
using Google.Android.Material.AppBar;
using Java.Lang;
using Liaoinstan.SpringViewLib.Widgets;
using Newtonsoft.Json;
using PixelPhoto.Activities.Chat;
using PixelPhoto.Activities.Posts.Adapters;
using PixelPhoto.Activities.Posts.Extras;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Activities.Story;
using PixelPhoto.Activities.Story.Adapters;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.PullSwipeStyles;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Funding;
using PixelPhotoClient.Classes.Story;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace PixelPhoto.Activities.Tabbes.Fragments
{
    public class NewsFeedFragment : Fragment, SpringView.IOnFreshListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region  Variables Basic

        private HomeActivity MainContext;
        private AppBarLayout AppBarLayout;
        private RecyclerView StoryRecycler;
        private TextView TxtAppName;
        public ImageView ImageChat;
        private SpringView SwipeRefreshLayout;
        public PRecyclerView RecyclerFeed;
        private ProgressBar ProgressBar;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        public StoryAdapter StoryAdapter;
        public NewsFeedAdapter NewsFeedAdapter;
        private Handler MainHandler;
          
        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            MainContext =(HomeActivity)Activity ?? HomeActivity.GetInstance();
            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.TNewsFeed, container, false);
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

                StartApiService("Add");

                //Start Updating the news feed every few minus 
                MainContext.GetOneSignalNotification();
                StartHandler();
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
                RecyclerFeed?.StopVideo();
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
                RecyclerFeed?.ReleasePlayer();
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
                AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(false);

                StoryRecycler = view.FindViewById<RecyclerView>(Resource.Id.StoryRecyler);

                TxtAppName = view.FindViewById<TextView>(Resource.Id.Appname);
                TxtAppName.Text = AppSettings.ApplicationName;
                TxtAppName.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                ImageChat = view.FindViewById<ImageView>(Resource.Id.chatbutton);
                ImageChat.Click += ImageChatOnClick;

                SwipeRefreshLayout = (SpringView)view.FindViewById(Resource.Id.material_style_ptr_frame);
                SwipeRefreshLayout.SetType(SpringView.Type.Overlap);
                SwipeRefreshLayout.Header = new DefaultHeader(Activity);
                SwipeRefreshLayout.Footer = new DefaultFooter(Activity);
                SwipeRefreshLayout.Enable = true;
                SwipeRefreshLayout.SetListener(this);
                SwipeRefreshLayout.OnFinishFreshAndLoad();//check this

                RecyclerFeed = view.FindViewById<PRecyclerView>(Resource.Id.RecylerFeed);

                ProgressBar = view.FindViewById<ProgressBar>(Resource.Id.sectionProgress);
                ProgressBar.Visibility = ViewStates.Visible;

                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
                EmptyStateLayout.Visibility = ViewStates.Gone;
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
                MainContext.SetToolBar(toolbar, " ", false);
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
                //Pro Recycler View 
                StoryAdapter = new StoryAdapter(Activity);
                StoryRecycler.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                StoryRecycler.SetItemViewCacheSize(20);
                StoryRecycler.HasFixedSize = true;
                StoryRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProviderPro = new FixedPreloadSizeProvider(10, 10);
                var preLoaderPro = new RecyclerViewPreloader<FetchStoriesObject.StoriesDataObject>(Activity, StoryAdapter, sizeProviderPro, 10);
                StoryRecycler.AddOnScrollListener(preLoaderPro);
                StoryAdapter.ItemClick += StoryAdapterOnItemClick;
                StoryRecycler.SetAdapter(StoryAdapter);

                NewsFeedAdapter = new NewsFeedAdapter(Activity, RecyclerFeed);
                RecyclerFeed.SetXAdapter(NewsFeedAdapter, true);
                NewsFeedAdapter.ItemClick += NewsFeedAdapterOnItemClick; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Events
         
        private void NewsFeedAdapterOnItemClick(object sender, Holders.PostAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = NewsFeedAdapter?.PostList[e.Position];
                    if (item?.IsLiked != null && !item.IsLiked.Value)
                    {
                        if (SystemClock.ElapsedRealtime() - UserDetails.TimestampLastClick < AppSettings.DoubleClickQualificationSpanInMillis)
                        {
                            var likeIcon = e.View.FindViewById<ImageView>(Resource.Id.Like);
                            var likeAnimationView = e.View.FindViewById<LottieAnimationView>(Resource.Id.animation_view_of_like);
                            var tapLikeAnimation = e.View.FindViewById<LottieAnimationView>(Resource.Id.animation_like);
                            tapLikeAnimation.PlayAnimation();
                            
                            NewsFeedAdapter.OnPostItemClickListener.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = e.View, LikeImgButton = likeIcon, LikeAnimationView = likeAnimationView }, e.Position);
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

        private void ImageChatOnClick(object sender, EventArgs e)
        {
            try
            {
                //Convert to fragment 
                Context.StartActivity(new Intent(Context, typeof(LastChatActivity)));
                MainContext.ShowOrHideBadgeViewMessenger();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StoryAdapterOnItemClick(object sender, StoryAdapterClickEventArgs e)
        {
            try
            {
                var circleIndicator = e.View.FindViewById<InsLoadingView>(Resource.Id.profile_indicator);
                circleIndicator.SetStatus(InsLoadingView.Status.Clicked);

                //Open View Story Or Create New Story
                var item = StoryAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (item.Type == "Your")
                    {
                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        arrayAdapter.Add(Activity.GetText(Resource.String.text));
                        arrayAdapter.Add(Activity.GetText(Resource.String.image));
                        arrayAdapter.Add(Activity.GetText(Resource.String.video));

                        dialogList.Title(Activity.GetText(Resource.String.Lbl_AddStory));
                        dialogList.Items(arrayAdapter);
                        dialogList.PositiveText(Activity.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                    }
                    else
                    {
                        var intent = new Intent(Activity, typeof(ViewStoryActivity));
                        intent.PutExtra("UserId", item.UserId.ToString());
                        intent.PutExtra("DataItem", JsonConvert.SerializeObject(item));
                        Activity.StartActivity(intent);
                    }
                }
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
                NewsFeedAdapter.PostList.Clear();
                NewsFeedAdapter.NotifyDataSetChanged();

                StoryAdapter.StoryList.Clear();
                StoryAdapter.NotifyDataSetChanged();

                ListUtils.FundingList.Clear();

                StartApiService("Add");
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
                if (NewsFeedAdapter.PostList.Count <= 5)
                    return;

                var item = NewsFeedAdapter.PostList.LastOrDefault();
                if (item != null)
                {
                    var lastItem = NewsFeedAdapter.PostList.IndexOf(item);

                    item = NewsFeedAdapter.PostList[lastItem];

                    if (item.Type == "AdMob1" || item.Type == "AdMob2" || item.Type == "FbNativeAds" || item.Type == "Funding")
                    {
                        item = NewsFeedAdapter.PostList.LastOrDefault(a => a.Type != "AdMob1" && a.Type != "AdMob2" && a.Type != "FbNativeAds" && a.Type != "Funding");
                    }
                }

                var offset = "0";

                if (item != null)
                    offset = item.PostId.ToString();

                if (!Methods.CheckConnectivity())
                    Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RecyclerFeed.FetchNewsFeedApiPosts("Add", offset) });
                 
                SwipeRefreshLayout.OnFinishFreshAndLoad();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (itemString.ToString() == Activity.GetText(Resource.String.image))
                {
                    MainContext.TypeOpen = "StoryImage";
                    MainContext.OpenDialogGallery("StoryImage");
                }
                else if (itemString.ToString() == Activity.GetText(Resource.String.video))
                {
                    MainContext.TypeOpen = "StoryVideo";
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 501 => video Gallery
                        new IntentController(Activity).OpenIntentVideoGallery();
                    }
                    else
                    {
                        if (Activity.CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                            && Activity.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                            && Activity.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            //requestCode >> 501 => video Gallery
                            new IntentController(Activity).OpenIntentVideoGallery();
                        }
                        else
                        {
                            new PermissionsController(Activity).RequestPermission(108);
                        }
                    }
                } 
                else if (itemString.ToString() == Activity.GetText(Resource.String.text))
                {
                    ((HomeActivity)Activity).OpenEditColor();
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
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        #region Load Data Api

        private void StartApiService(string typeRun, string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                // check if the app in background
                Toast.MakeText(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
            {
                if (AppSettings.ShowFunding)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadFunding });

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadStory, () => RecyclerFeed.FetchNewsFeedApiPosts(typeRun, offset) });
            }
        }

        private async Task LoadStory()
        {
            try
            {
                Activity?.RunOnUiThread(() =>
                {
                    try
                    {
                        var dataOwner = StoryAdapter.StoryList.FirstOrDefault(a => a.Type == "Your");
                        if (dataOwner == null)
                        {
                            StoryAdapter.StoryList.Insert(0, new FetchStoriesObject.StoriesDataObject
                            {
                                Avatar = UserDetails.Avatar,
                                Type = "Your",
                                Username = Context.GetText(Resource.String.Lbl_YourStory),
                                Owner = true,
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });

                if (Methods.CheckConnectivity())
                {
                    var countList = StoryAdapter.StoryList.Count;
                    (var apiStatus, var respond) = await RequestsAsync.Story.FetchStories();
                    if (apiStatus != 200 || !(respond is FetchStoriesObject result) || result.Data == null)
                    {
                        Methods.DisplayReportResult(Activity, respond);
                    }
                    else
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {  
                            foreach (var item in result.Data)
                            {
                                var check = StoryAdapter.StoryList.FirstOrDefault(a => a.UserId == item.UserId);
                                if (check == null)
                                {
                                    foreach (var item1 in item.Stories)
                                    {
                                        item.DurationsList ??= new List<long>();

                                        var type1 = Methods.AttachmentFiles.Check_FileExtension(item1.MediaFile);
                                        if (type1 != "Video")
                                        {
                                            Glide.With(Context).Load(item1.MediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                            item.DurationsList.Add(AppSettings.StoryDuration);
                                        }
                                        else
                                        { 
                                            var fileName = item1.MediaFile.Split('/').Last();
                                            item1.MediaFile = AppTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, item1.MediaFile);

                                            if (Long.ParseLong(item1.Duration) == 0)
                                            {
                                                item1.Duration = AppTools.GetDuration(item1.MediaFile);
                                                item.DurationsList.Add(Long.ParseLong(item1.Duration));
                                            }
                                            else
                                            {
                                                item.DurationsList.Add(Long.ParseLong(item1.Duration));
                                            }
                                        }
                                    }
                                     
                                    StoryAdapter.StoryList.Add(item);
                                }
                                else
                                { 
                                    foreach (var item2 in item.Stories)
                                    {
                                        item.DurationsList ??= new List<long>();

                                        var type = Methods.AttachmentFiles.Check_FileExtension(item2.MediaFile);
                                        if (type != "Video")
                                        {
                                            Glide.With(Context).Load(item2.MediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                            item.DurationsList.Add(AppSettings.StoryDuration);
                                        }
                                        else
                                        {
                                            var fileName = item2.MediaFile.Split('/').Last();
                                            item2.MediaFile = AppTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, item2.MediaFile);

                                            if (Long.ParseLong(item2.Duration) == 0)
                                            {
                                                item2.Duration = AppTools.GetDuration(item2.MediaFile);
                                                item.DurationsList.Add(Long.ParseLong(item2.Duration));
                                            }
                                            else
                                            {
                                                item.DurationsList.Add(Long.ParseLong(item2.Duration));
                                            }
                                        }
                                    }

                                    check.Stories = item.Stories; 
                                }
                            }

                            if (countList > 0)
                            { 
                                Activity?.RunOnUiThread(() => { StoryAdapter.NotifyItemRangeInserted(countList, StoryAdapter.StoryList.Count - countList); });
                            }
                            else
                            {
                                Activity?.RunOnUiThread(() => { StoryAdapter.NotifyDataSetChanged(); });
                            }
                        }
                    } 
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
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.OnFinishFreshAndLoad();

                if (ProgressBar.Visibility == ViewStates.Visible)
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    AppBarLayout.SetExpanded(true);
                }

                if (NewsFeedAdapter.PostList.Count > 0)
                {
                    RecyclerFeed.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    RecyclerFeed.Visibility = ViewStates.Gone;

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
                ProgressBar.Visibility = ViewStates.Gone;
                SwipeRefreshLayout.OnFinishFreshAndLoad();
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService("Add");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task LoadFunding()
        {
            if (Methods.CheckConnectivity())
            {
                var countList = ListUtils.FundingList.Count;

                var (respondCode, respondString) = await RequestsAsync.Funding.FetchFunding("15", "0");
                if (respondCode.Equals(200))
                {
                    if (respondString is FetchFundingObject respond)
                    {
                        var respondList = respond.Data.Count;
                        if (respondList > 0)
                        {
                            if (countList == 0)
                            {
                                ListUtils.FundingList = new ObservableCollection<FundingDataObject>(respond.Data);
                            }
                            else
                            {
                                foreach (var item in from item in respond.Data let check = ListUtils.FundingList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    ListUtils.FundingList.Add(item);
                                }
                            }
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity, respondString);
            }
        }

        #endregion
         
        private static bool IsCanceledHandler;
        public void StartHandler()
        {
            try
            {
                MainHandler ??= new Handler(Looper.MainLooper);
                MainHandler?.PostDelayed(new PostUpdaterHelper(MainContext, RecyclerFeed, new Handler(Looper.MainLooper)), 30000);
                IsCanceledHandler = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveHandler()
        {
            try
            {
                MainHandler?.RemoveCallbacks(new PostUpdaterHelper(MainContext, RecyclerFeed, new Handler(Looper.MainLooper)));
                MainHandler = null;
                IsCanceledHandler = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class PostUpdaterHelper : Java.Lang.Object, IRunnable
        {
            private readonly PRecyclerView MainRecyclerView;
            private readonly Handler MainHandler;
            private readonly HomeActivity Activity;

            public PostUpdaterHelper(HomeActivity activity, PRecyclerView mainRecyclerView, Handler mainHandler)
            {
                MainRecyclerView = mainRecyclerView;
                MainHandler = mainHandler;
                Activity = activity;
            }

            public async void Run()
            {
                try
                {
                    if (string.IsNullOrEmpty(Current.AccessToken) || !Methods.CheckConnectivity() || IsCanceledHandler)
                        return;

                    await Activity.NewsFeedFragment.RecyclerFeed.FetchNewsFeedApiPosts(Activity.NewsFeedFragment.NewsFeedAdapter.PostList.Count > 0 ? "Insert" : "Add");
                    await Activity.Get_Notifications();
                    //await ApiRequest.GetProfile_Api(Activity); 
                    MainHandler.PostDelayed(new PostUpdaterHelper(Activity, MainRecyclerView, MainHandler), 30000);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }
    }
}