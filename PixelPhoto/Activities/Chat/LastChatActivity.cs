using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using Google.Android.Material.FloatingActionButton;
using Newtonsoft.Json;
using PixelPhoto.Activities.Base;
using PixelPhoto.Activities.Chat.Adapters;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.Classes.Messages;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using ActionMode = AndroidX.AppCompat.View.ActionMode;
using Object = Java.Lang.Object;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.Chat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class LastChatActivity : BaseActivity , IOnClickListenerSelected 
    {
        #region Variables Basic
         
        public static LastChatAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private FloatingActionButton FloatingAction;
        private static ActionMode ActionMode;
        private static Toolbar ToolBar;
        private AdView MAdView;
        private ActionModeCallback ModeCallback;
        private string UserId = "";
        private static LastChatActivity Instance;

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

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
 
                GetLastChatLocal();

                AdsGoogle.Ad_Interstitial(this);
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
                ListUtils.ChatList = MAdapter.UserList; 
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
                var mainLayout = FindViewById<CoordinatorLayout>(Resource.Id.mainLayout);
                mainLayout.SetPadding(0,0,0,0);
                 
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                FloatingAction = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                FloatingAction.Visibility = ViewStates.Visible;
                FloatingAction.SetImageResource(Resource.Drawable.ic_contacts);
            
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
                MAdapter = new LastChatAdapter(this);
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<UserDataObject>(this, MAdapter, sizeProvider, 10);
                 MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
                MAdapter.SetOnClickListener(this);

                ModeCallback = new ActionModeCallback(this);

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
                ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = GetText(Resource.String.Lbl_Chats);
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
 
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    FloatingAction.Click += FloatingActionButtonViewOnClick;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    FloatingAction.Click -= FloatingActionButtonViewOnClick;
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public static LastChatActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }
       
        #endregion

        #region Scroll

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                //Event Scroll #LastChat
                var item = MAdapter.UserList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
                {
                    StartApiService(item.Id); 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Toolbar & Selected

        private class ActionModeCallback : Object, ActionMode.ICallback
        {
            private readonly LastChatActivity Activity;
            public ActionModeCallback(LastChatActivity activity)
            {
                Activity = activity;
            }

            public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
            {
                var id = item.ItemId;
                if (id == Resource.Id.action_delete)
                {
                    DeleteItems();
                    mode.Finish();
                    return true;
                }

                if (id == Android.Resource.Id.Home)
                {
                    HomeActivity.GetInstance()?.SetService();

                    MAdapter.ClearSelections();


                    ActionMode.Finish();

                    return true;
                }

                return false;
            }

            public bool OnCreateActionMode(ActionMode mode, IMenu menu)
            {
                SetSystemBarColor(Activity, AppSettings.MainColor);
                mode.MenuInflater.Inflate(Resource.Menu.menu_delete, menu);
                return true;
            }

            private void SetSystemBarColor(Activity act, string color)
            {
                try
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        var window = act.Window;
                        window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        window.SetStatusBarColor(Color.ParseColor(color));
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnDestroyActionMode(ActionMode mode)
            {
                try
                {
                    MAdapter.ClearSelections();
                    ActionMode = null!;
                    SetSystemBarColor(Activity, AppSettings.MainColor);

                    HomeActivity.GetInstance()?.SetService();

                    ToolBar.Visibility = ViewStates.Visible;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
            {
                return false;
            }

            //Delete Chat 
            private void DeleteItems()
            {
                try
                {
                    HomeActivity.GetInstance()?.SetService();

                    if (ToolBar.Visibility != ViewStates.Visible)
                        ToolBar.Visibility = ViewStates.Visible;

                    if (Methods.CheckConnectivity())
                    {
                        var selectedItemPositions = MAdapter.GetSelectedItems();
                        for (var i = selectedItemPositions.Count - 1; i >= 0; i--)
                        {
                            var datItem = MAdapter.GetItem(selectedItemPositions[i]);
                            if (datItem != null)
                            {
                                MAdapter.RemoveData(selectedItemPositions[i], datItem);
                                //Send Api Delete 
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Messages.DeleteChat(datItem.UserId) });
                            }
                        }

                        MAdapter.NotifyDataSetChanged();
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        //Event 
        public void OnItemClick(View view, ChatDataObject obj, int pos)
        {
            try
            {
                if (MAdapter.GetSelectedItemCount() > 0) // Add Select  New Item 
                {
                    EnableActionMode(pos);
                }
                else
                {
                    HomeActivity.GetInstance()?.SetService();

                    if (ToolBar.Visibility != ViewStates.Visible)
                        ToolBar.Visibility = ViewStates.Visible;

                    // read the item which removes bold from the row >> event click open ChatBox by user id
                    var item = MAdapter.GetItem(pos);
                    if (item != null)
                    {
                        UserId = item.UserId;

                        item.NewMessage = 0;
                        var intent = new Intent(this, typeof(MessagesBoxActivity));
                        intent.PutExtra("UserId", item.UserId);
                        intent.PutExtra("TypeChat", "LastChat");
                        intent.PutExtra("UserItem", JsonConvert.SerializeObject(item));

                        // Check if we're running on Android 5.0 or higher
                        if ((int)Build.VERSION.SdkInt < 23)
                        {
                            StartActivity(intent);
                            MAdapter.NotifyItemChanged(pos);
                        }
                        else
                        {
                            //Check to see if any permission in our group is available, if one, then all are
                            if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                                CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                StartActivity(intent);
                                MAdapter.NotifyItemChanged(pos);
                            }
                            else
                                new PermissionsController(this).RequestPermission(100);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnItemLongClick(View view, ChatDataObject obj, int pos)
        {
            EnableActionMode(pos);
        }

        private void EnableActionMode(int position)
        {
            ActionMode ??= StartSupportActionMode(ModeCallback);
            ToggleSelection(position);
        }

        private void ToggleSelection(int position)
        {
            try
            {
                MAdapter.ToggleSelection(position);
                var count = MAdapter.GetSelectedItemCount();

                if (count == 0)
                {
                    HomeActivity.GetInstance()?.SetService();

                    ToolBar.Visibility = ViewStates.Visible;
                    ActionMode.Finish();
                }
                else
                {
                    HomeActivity.GetInstance()?.SetService(false);

                    ToolBar.Visibility = ViewStates.Gone;
                    ActionMode.SetTitle(count);
                    ActionMode.Invalidate();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Permissions

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 100)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        var item = MAdapter.UserList.FirstOrDefault(a => a.UserId.ToString() == UserId);
                        if (item != null)
                        {
                            var intent = new Intent(this, typeof(MessagesBoxActivity));
                            intent.PutExtra("UserId", item.UserId);
                            intent.PutExtra("TypeChat", "LastChat");
                            intent.PutExtra("UserItem", JsonConvert.SerializeObject(item));
                            StartActivity(intent);

                            MAdapter.NotifyItemChanged(MAdapter.UserList.IndexOf(item));

                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long)?.Show();
                    }
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
                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                var database = new SqLiteDatabase();
                database.ClearLastChat();
                database.ClearAll_Messages();
                

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Add New Chat
        private void FloatingActionButtonViewOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(NewChatActivity));
                intent.PutExtra("UserId", UserDetails.UserId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

        #region Load Data Api 

        private void GetLastChatLocal()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                ListUtils.ChatList = new ObservableCollection<ChatDataObject>();
                var list = dbDatabase.GetAllLastChat();
                if (list.Count > 0)
                {
                    ListUtils.ChatList = new ObservableCollection<ChatDataObject>(list);
                    MAdapter.UserList = ListUtils.ChatList;
                    MAdapter.BindEnd();
                }
                else
                {
                    SwipeRefreshLayout.Refreshing = true;

                    StartApiService();
                }
                 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

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
                var countList = MAdapter.UserList.Count;
               
                var (apiStatus, respond) = await RequestsAsync.Messages.GetChats("25", offset); 
                if (apiStatus != 200 || !(respond is GetChatsObject result) || result.Data == null)
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
                            LoadDataJsonLastChat(result);
                        }
                        else
                        {
                            ListUtils.ChatList = new ObservableCollection<ChatDataObject>(result.Data);
                            MAdapter.UserList = new ObservableCollection<ChatDataObject>(result.Data);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });

                            var dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
                             
                        }
                    }
                    else
                    {
                        if (MAdapter.UserList.Count > 10 && !MRecycler.CanScrollVertically(1))
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

        public void LoadDataJsonLastChat(GetChatsObject result)
        {
            try
            {
                if (MAdapter != null)
                {
                    if (MAdapter.UserList?.Count > 0)
                    {
                        foreach (var user in result.Data)
                        {
                            var checkUser = MAdapter.UserList.FirstOrDefault(a => a.UserId == user.UserId);
                            if (checkUser != null)
                            {
                                //checkUser.UserId = user.UserId;
                                //checkUser.Id = user.Id;
                                if (checkUser.Username != null && checkUser.Username != user.Username) checkUser.Username = user.Username;
                                if (checkUser.Avatar != user.Avatar) checkUser.Avatar = user.Avatar;
                                if (checkUser.Time != user.Time) checkUser.Time = user.Time;
                                if (checkUser.NewMessage != user.NewMessage) checkUser.NewMessage = user.NewMessage;
                                if (checkUser.TimeText != user.TimeText) checkUser.TimeText = user.TimeText;

                                if (checkUser.LastMessage == user.LastMessage) continue;

                                checkUser.LastMessage = user.LastMessage;
                                var index = MAdapter.UserList.IndexOf(MAdapter.UserList.FirstOrDefault(a => a.UserId == user.UserId));
                                if (index > -1)
                                {
                                    RunOnUiThread(() =>
                                    {
                                        MAdapter.Move(checkUser);
                                    });
                                }
                            }
                            else
                            {
                                RunOnUiThread(() =>
                                {
                                    MAdapter.Insert(user);

                                    var dataUser = MAdapter.UserList.IndexOf(MAdapter.UserList.FirstOrDefault(a => a.Id == user.Id));
                                    if (dataUser > -1)
                                        MAdapter.NotifyItemChanged(dataUser);
                                });
                            }
                        }
                    }
                    else
                    {
                        MAdapter.UserList = new ObservableCollection<ChatDataObject>(result.Data);
                        MAdapter.BindEnd();
                    }

                    ListUtils.ChatList = MAdapter.UserList;
                }

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
               
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;  
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.UserList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoMessage);
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