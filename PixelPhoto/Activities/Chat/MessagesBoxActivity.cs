using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using AndroidX.AppCompat.App;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AT.Markushi.UI;
using TheArtOfDev.Edmodo.Cropper;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Java.IO;
using Newtonsoft.Json;
using PixelPhoto.Activities.Chat.Adapters;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.Classes.Messages;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using UserDataObject = PixelPhotoClient.GlobalClass.UserDataObject;
using Uri = Android.Net.Uri;
using ActionMode = AndroidX.AppCompat.View.ActionMode;

namespace PixelPhoto.Activities.Chat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MessagesBoxActivity : AppCompatActivity, IOnClickListenerSelectedMessages
    {
        #region Variables Basic

        private AppCompatImageView ChatEmojisImage;
        private RelativeLayout RootView;
        private EmojiconEditText EmoticonEditTextView;
        private CircleButton ChatSendButton, ImageButton;
        private static Toolbar TopChatToolBar;
        private RecyclerView ChatBoxRecyclerView;
        private LinearLayoutManager MLayoutManager;
        public UserMessagesAdapter MAdapter;
        public static MessagesBoxActivity Instance;
        private string LastSeenUser = "", TypeChat = "", TaskWork = "";
        private string BeforeMessageId, FirstMessageId;
        private static int Userid;// to_id
        private static Timer Timer;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private ChatDataObject DataUser;
        private UserDataObject UserInfoData;
        private CommentObject UserInfoComment;
        private ActionMode.ICallback ModeCallback;  
        private static ActionMode ActionMode;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Methods.App.FullScreenApp(this); 
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                Window?.SetSoftInputMode(SoftInput.AdjustResize);
                base.OnCreate(savedInstanceState);

                Window?.SetBackgroundDrawableResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.chatBackground3_Dark : Resource.Drawable.chatBackground3);

                // Set our view from the "MessagesBox_Layout" layout resource
                SetContentView(Resource.Layout.MessagesBoxLayout);

                Instance = this;

                var data = Intent?.GetStringExtra("UserId") ?? "Data not available";
                if (data != "Data not available" && !string.IsNullOrEmpty(data)) Userid = int.Parse(data); // to_id

                try
                {
                    var type = Intent?.GetStringExtra("TypeChat") ?? "Data not available";
                    if (type != "Data not available" && !string.IsNullOrEmpty(type))
                    {
                        TypeChat = type;
                        var json = Intent?.GetStringExtra("UserItem");
                        dynamic item;
                        switch (type)
                        {
                            case "LastChat":
                                item = JsonConvert.DeserializeObject<ChatDataObject>(json);
                                if (item != null) DataUser = item;
                                break;
                            case "comment":
                                item = JsonConvert.DeserializeObject<CommentObject>(json);
                                if (item != null) UserInfoComment = item;
                                break;
                            case "following":
                            case "followers":
                            case "suggestion":
                            case "search":
                            case "Notification":
                            case "new":
                            case "NewsFeedPost":
                            case "OneSignalNotification":
                            case "UserData":
                                item = JsonConvert.DeserializeObject<UserDataObject>(json);
                                if (item != null) UserInfoData = item;
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                var emojisIcon = new EmojIconActions(this, RootView, EmoticonEditTextView, ChatEmojisImage);
                emojisIcon.ShowEmojIcon();

                //Set Title ToolBar and data chat user
                loadData_ItemUser();
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

                if (Timer != null)
                {
                    Timer.Enabled = true;
                    Timer.Start();
                }
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

                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        AppSettings.SetTabDarkTheme = false;
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        AppSettings.SetTabDarkTheme = true;
                        break;
                }

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                Window.SetBackgroundDrawableResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.chatBackground3_Dark : Resource.Drawable.chatBackground3);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #region Functions

        private void InitComponent()
        {
            try
            {
                RootView = FindViewById<RelativeLayout>(Resource.Id.rootChatWindowView);

                ChatEmojisImage = FindViewById<AppCompatImageView>(Resource.Id.emojiicon);
                EmoticonEditTextView = FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
                ChatSendButton = FindViewById<CircleButton>(Resource.Id.sendButton);
                ImageButton = FindViewById<CircleButton>(Resource.Id.imageButton);
                ChatBoxRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyler);
                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);

                ChatSendButton.Tag = "Text";
                ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);

                ModeCallback = new ActionModeCallback(this);

                 
                Methods.SetColorEditText(EmoticonEditTextView, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                TopChatToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (TopChatToolBar != null)
                {
                    TopChatToolBar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    TopChatToolBar.SetSubtitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    TopChatToolBar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);

                    SetSupportActionBar(TopChatToolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
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
                ChatBoxRecyclerView.SetItemAnimator(null);
                MAdapter = new UserMessagesAdapter(this);
                MLayoutManager = new LinearLayoutManager(this);
                ChatBoxRecyclerView.SetLayoutManager(MLayoutManager);
                ChatBoxRecyclerView.SetAdapter(MAdapter);
                MAdapter.SetOnClickListener(this);
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
                    ImageButton.Click += ImageButtonOnClick;
                    ChatSendButton.Touch += Chat_sendButton_Touch;
                }
                else
                {
                    ImageButton.Click -= ImageButtonOnClick;
                    ChatSendButton.Touch -= Chat_sendButton_Touch;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set ToolBar and data chat user

        //Set ToolBar and data chat user
        private void loadData_ItemUser()
        {
            try
            {
                if (DataUser != null)
                {
                    SupportActionBar.Title = DataUser.UserData.Name;
                    SupportActionBar.Subtitle = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(DataUser.UserData.LastSeen), false);
                    LastSeenUser = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(DataUser.UserData.LastSeen), false);
                }
                else if (UserInfoData != null)
                {
                    SupportActionBar.Title = UserInfoData.Name;
                    SupportActionBar.Subtitle = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(UserInfoData.LastSeen), false);
                    LastSeenUser = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(UserInfoData.LastSeen), false);
                }
                else if (UserInfoComment != null)
                {
                    SupportActionBar.Title = UserInfoComment.Name;
                    SupportActionBar.Subtitle = "";
                    LastSeenUser = "";
                }

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetUserProfileApi });

                Get_Messages();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task GetUserProfileApi()
        {
            if (Methods.CheckConnectivity())
            {
                (var respondCode, var respondString) = await RequestsAsync.User.FetchUserData(Userid.ToString());
                if (respondCode == 200)
                {
                    if (respondString is FetchUserDataObject result)
                    {
                        if (result.Data != null)
                        {
                            UserInfoData = result.Data;

                            RunOnUiThread(() =>
                            {
                                SupportActionBar.Title = UserInfoData.Name;
                                SupportActionBar.Subtitle = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(UserInfoData.LastSeen), false);
                                LastSeenUser = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(UserInfoData.LastSeen), false);
                            });
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respondString);
            }
        }

        private UserDataObject ConvertData()
        {
            try
            {
                UserDataObject userData = null!;
                if (DataUser != null)
                {
                    userData = new UserDataObject
                    {
                        UserId = DataUser.UserData.UserId,
                        Username = DataUser.UserData.Username,
                        Email = DataUser.UserData.Email,
                        IpAddress = DataUser.UserData.IpAddress,
                        Fname = DataUser.UserData.Fname,
                        Lname = DataUser.UserData.Lname,
                        Gender = DataUser.UserData.Gender,
                        Language = DataUser.UserData.Language,
                        Avatar = DataUser.UserData.Avatar,
                        Cover = DataUser.UserData.Cover,
                        CountryId = DataUser.UserData.CountryId,
                        About = DataUser.UserData.About,
                        Google = DataUser.UserData.Google,
                        Facebook = DataUser.UserData.Facebook,
                        Twitter = DataUser.UserData.Twitter,
                        Website = DataUser.UserData.Website,
                        Active = DataUser.UserData.Active,
                        Admin = DataUser.UserData.Admin,
                        Verified = DataUser.UserData.Verified,
                        LastSeen = DataUser.UserData.LastSeen,
                        Registered = DataUser.UserData.Registered,
                        IsPro = DataUser.UserData.IsPro,
                        Posts = DataUser.UserData.Posts,
                        PPrivacy = DataUser.UserData.PPrivacy,
                        CPrivacy = DataUser.UserData.CPrivacy,
                        NOnLike = DataUser.UserData.NOnLike,
                        NOnMention = DataUser.UserData.NOnMention,
                        NOnComment = DataUser.UserData.NOnComment,
                        NOnFollow = DataUser.UserData.NOnFollow,
                        StartupAvatar = DataUser.UserData.StartupAvatar,
                        StartupInfo = DataUser.UserData.StartupInfo,
                        StartupFollow = DataUser.UserData.StartupFollow,
                        Src = DataUser.UserData.Src,
                        SearchEngines = DataUser.UserData.SearchEngines,
                        Mode = DataUser.UserData.Mode,
                        Name = DataUser.UserData.Name,
                        Uname = DataUser.UserData.Uname,
                        Url = DataUser.UserData.Url,
                        TimeText = DataUser.UserData.TimeText,
                        IsFollowing = DataUser.UserData.IsFollowing,
                        IsBlocked = DataUser.UserData.IsBlocked,
                    };
                }
                else if (UserInfoData != null)
                {
                    userData = UserInfoData;
                }

                return userData;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }


        #endregion

        #region Get Messages

        //Get Messages Local Or Api
        private void Get_Messages()
        {
            try
            {
                BeforeMessageId = "0";
                MAdapter.MessageList.Clear();
                MAdapter.NotifyDataSetChanged();

                var dbDatabase = new SqLiteDatabase();
                var localList = dbDatabase.GetMessagesList(UserDetails.UserId, Userid.ToString(), BeforeMessageId);
                if (localList == "1") //Database.. Get Messages Local
                {
                    MAdapter.NotifyDataSetChanged();

                    //Scroll Down >> 
                    ChatBoxRecyclerView.ScrollToPosition(MAdapter.MessageList.Count - 1);
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;
                }
                else //Or server.. Get Messages Api
                {
                    SwipeRefreshLayout.Refreshing = true;
                    SwipeRefreshLayout.Enabled = true;
                    GetMessages_API();
                }

                //Set Event Scroll
                var onScrollListener = new XamarinRecyclerViewOnScrollListener(MLayoutManager, SwipeRefreshLayout);
                onScrollListener.LoadMoreEvent += Messages_OnScroll_OnLoadMoreEvent;
                ChatBoxRecyclerView.AddOnScrollListener(onScrollListener);
                TaskWork = "Working";

                //Run timer
                Timer = new Timer { Interval = AppSettings.MessageRequestSpeed, Enabled = true };
                Timer.Elapsed += TimerOnElapsed_MessageUpdater;
                Timer.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Messages From API 
        private async void GetMessages_API()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;
                    ToastUtils.ShowToast(GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    BeforeMessageId = "0";

                    var (apiStatus, respond) = await RequestsAsync.Messages.GetUserMessages(Userid.ToString());
                    if (apiStatus == 200)
                    {
                        if (respond is GetUserMessagesObject result)
                        {
                            if (result.Data.Messages.Count > 0)
                            {
                                var list = AppTools.FilterMessage(result.Data.Messages);

                                MAdapter.MessageList = new ObservableCollection<MessageDataObject>(list.OrderBy(a => a.Time));
                                MAdapter.NotifyDataSetChanged();

                                //Insert to DataBase
                                var dbDatabase = new SqLiteDatabase();
                                dbDatabase.InsertOrReplaceMessages(MAdapter.MessageList);

                                //Scroll Down >> 
                                ChatBoxRecyclerView.ScrollToPosition(MAdapter.MessageList.Count - 1);

                                SwipeRefreshLayout.Refreshing = false;
                                SwipeRefreshLayout.Enabled = false;
                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
            }
        }

        #endregion

        //Timer Message Updater >> Get New Message
        private void TimerOnElapsed_MessageUpdater(object sender, ElapsedEventArgs e)
        {
            try
            {
                //Code get last Message id where Updater >>
                MessageUpdater();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Updater

        private async void MessageUpdater()
        {
            try
            {
                if (TaskWork == "Working")
                {
                    TaskWork = "Stop";

                    if (!Methods.CheckConnectivity())
                    {
                        SwipeRefreshLayout.Refreshing = false;
                        ToastUtils.ShowToast(GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }
                    else
                    {
                        var countList = MAdapter.MessageList.Count;
                        var afterId = MAdapter.MessageList.LastOrDefault()?.Id ?? "";
                        var (apiStatus, respond) = await RequestsAsync.Messages.GetUserMessages(Userid.ToString(), "30", "", afterId);
                        if (apiStatus == 200)
                        {
                            if (respond is GetUserMessagesObject result)
                            {
                                var responseList = result.Data.Messages.Count;
                                if (responseList > 0)
                                {
                                    var list = AppTools.FilterMessage(result.Data.Messages);
                                    if (countList > 0)
                                    {
                                        foreach (var item in from item in list let check = MAdapter.MessageList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                        {
                                            MAdapter.MessageList.Add(item);
                                            RunOnUiThread(() => { MAdapter.NotifyItemInserted(MAdapter.MessageList.IndexOf(item)); });
                                        }
                                    }
                                    else
                                    {
                                        MAdapter.MessageList = new ObservableCollection<MessageDataObject>(list);
                                    }

                                    RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            var lastCountItem = MAdapter.ItemCount;
                                            if (countList > 0)
                                                MAdapter.NotifyItemRangeInserted(lastCountItem, MAdapter.MessageList.Count - 1);
                                            else
                                                MAdapter.NotifyDataSetChanged();

                                            //Insert to DataBase
                                            var dbDatabase = new SqLiteDatabase();
                                            dbDatabase.InsertOrReplaceMessages(MAdapter.MessageList);

                                            //Scroll Down >> 
                                            ChatBoxRecyclerView.ScrollToPosition(MAdapter.MessageList.Count - 1);

                                            var lastMessage = MAdapter.MessageList.LastOrDefault();
                                            if (lastMessage != null)
                                            {
                                                var dataUser = LastChatActivity.MAdapter.UserList?.FirstOrDefault(a => a.UserId == lastMessage?.FromId);
                                                if (dataUser != null)
                                                {
                                                    dataUser.LastMessage = lastMessage.Text;

                                                    LastChatActivity.MAdapter.Move(dataUser);
                                                    LastChatActivity.MAdapter.Update(dataUser);
                                                }
                                                if (UserDetails.SoundControl)
                                                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_GetMesseges.mp3");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e); 
                                        }
                                    });
                                }
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    TaskWork = "Working";
                }
            }
            catch (Exception e)
            {
                TaskWork = "Working";
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UpdateOneMessage(MessageDataObject messages)
        {
            try
            {
                var checker = MAdapter.MessageList.FirstOrDefault(a => a.Id == messages.Id);
                if (checker != null)
                {
                    checker.Id = messages.Id;
                    checker.FromId = messages.FromId;
                    checker.ToId = messages.ToId;
                    checker.Text = messages.Text;
                    checker.MediaFile = messages.MediaFile;
                    checker.MediaType = messages.MediaType;
                    checker.DeletedFs1 = messages.DeletedFs1;
                    checker.DeletedFs2 = messages.DeletedFs2;
                    checker.Seen = messages.Seen;
                    checker.Time = messages.Time;
                    checker.Extra = messages.Extra;
                    checker.TimeText = messages.TimeText;
                    checker.Position = messages.Position;

                    MAdapter.NotifyItemChanged(MAdapter.MessageList.IndexOf(checker));
                    
                    //Scroll Down >> 
                    ChatBoxRecyclerView.ScrollToPosition(MAdapter.ItemCount - 1);
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load More

        private async void LoadMoreMessages()
        {
            try
            {
                //Run Load More Api 
                var local = LoadMoreMessagesDatabase();
                if (local == "1") return;

                var api = await LoadMoreMessagesApi();
                if (api == "1") return;

                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private string LoadMoreMessagesDatabase()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var localList = dbDatabase.GetMessageList(UserDetails.UserId, Userid.ToString(), FirstMessageId);
                if (localList?.Count > 0) //Database.. Get Messages Local
                { 
                    localList = new List<DataTables.MessageTb>(localList.OrderByDescending(a => a.Id));

                    foreach (var m in localList.Select(messages => new MessageDataObject
                    {
                        Id = messages.Id,
                        FromId = messages.FromId,
                        ToId = messages.ToId,
                        Text = messages.Text,
                        MediaFile = messages.MediaFile,
                        MediaType = messages.MediaType,
                        DeletedFs1 = messages.DeletedFs1,
                        DeletedFs2 = messages.DeletedFs2,
                        Seen = messages.Seen,
                        Time = messages.Time,
                        Extra = messages.Extra,
                        TimeText = messages.TimeText,
                        Position = messages.Position,
                        MessageType = !string.IsNullOrEmpty(messages.MediaFile) ? "Media" : "Text",
                    }))
                    {
                        MAdapter.MessageList.Insert(0, m);
                        MAdapter.NotifyItemInserted(MAdapter.MessageList.IndexOf(m)); 
                    }
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "0";
            }
        }

        private async Task<string> LoadMoreMessagesApi()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    SwipeRefreshLayout.Refreshing = false;
                    ToastUtils.ShowToast(GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var countList = MAdapter.MessageList.Count;
                    var (apiStatus, respond) = await RequestsAsync.Messages.GetUserMessages(Userid.ToString(), "30", FirstMessageId);
                    if (apiStatus == 200)
                    {
                        if (respond is GetUserMessagesObject result)
                        {
                            if (result.Data.Messages.Count > 0)
                            {
                                var list = AppTools.FilterMessage(result.Data.Messages);
                                var listApi = new ObservableCollection<MessageDataObject>();

                                foreach (var messages in list.OrderBy(a => a.Time))
                                {
                                    var message = new MessageDataObject
                                    {
                                        Id = messages.Id,
                                        FromId = messages.FromId,
                                        ToId = messages.ToId,
                                        Text = messages.Text,
                                        MediaFile = messages.MediaFile,
                                        MediaType = messages.MediaType,
                                        DeletedFs1 = messages.DeletedFs1,
                                        DeletedFs2 = messages.DeletedFs2,
                                        Seen = messages.Seen,
                                        Time = messages.Time,
                                        Extra = messages.Extra,
                                        TimeText = messages.TimeText,
                                        Position = messages.Position,
                                        MessageType = messages.MessageType,
                                    };

                                    MAdapter.MessageList.Insert(0, message);
                                    
                                    listApi.Insert(0, message);

                                    var dbDatabase = new SqLiteDatabase();
                                    // Insert data user in database
                                    dbDatabase.InsertOrReplaceMessages(listApi);
                                }

                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        var lastCountItem = MAdapter.ItemCount;
                                        if (countList > 0)
                                            MAdapter.NotifyItemRangeInserted(lastCountItem, MAdapter.MessageList.Count - 1);
                                        else
                                            MAdapter.NotifyDataSetChanged();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                                 
                                return "1";
                            }
                            return "0";
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
                return "0";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return "0";
            }
        }

        #endregion

        #region Events

        //Send Message type => "right_text"
        private void OnClick_OfSendButton()
        {
            try
            {
                var timeNow = DateTime.Now.ToShortTimeString();
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var time2 = Convert.ToString(unixTimestamp);

                if (string.IsNullOrEmpty(EmoticonEditTextView.Text))
                {

                }
                else
                {
                    //Here on This function will send Text Messages to the user 

                    //remove \n in a string
                    var replacement = Regex.Replace(EmoticonEditTextView.Text, @"\t|\n|\r", "");

                    if (Methods.CheckConnectivity())
                    {
                        var message = new MessageDataObject
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            ToId = Userid.ToString(),
                            Text = replacement,
                            MediaFile = "",
                            MediaType = "",
                            DeletedFs1 = "",
                            DeletedFs2 = "",
                            Seen = "0",
                            Time = time2,
                            Extra = "",
                            TimeText = timeNow,
                            Position = "Right",
                            MessageType = "Text"
                        };

                        MAdapter.MessageList.Add(message);
                        MAdapter.NotifyItemInserted(MAdapter.MessageList.IndexOf(message));
                        //Scroll Down >> 
                        ChatBoxRecyclerView.ScrollToPosition(MAdapter.ItemCount - 1);
                          
                        var userData = ConvertData();
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MessageController.SendMessageTask(Userid, EmoticonEditTextView.Text, "", time2, userData) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }

                    EmoticonEditTextView.Text = "";
                }

                ChatSendButton.Tag = "Text";
                ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event click send messages type text
        private void Chat_sendButton_Touch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action == MotionEventActions.Down)
                {
                    OnClick_OfSendButton();
                }
                e.Handled = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event click send messages type image
        private void ImageButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Scroll

        //Event Scroll #Messages
        private void Messages_OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                //Start Loader Get from Database or API Request >>
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;

                FirstMessageId = "0";

                //Code get first Message id where LoadMore >>
                var mes = MAdapter.MessageList.FirstOrDefault();
                if (mes != null)
                {
                    FirstMessageId = mes.Id;
                }

                if (FirstMessageId != "0")
                {
                    LoadMoreMessages();
                }

                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class XamarinRecyclerViewOnScrollListener : RecyclerView.OnScrollListener
        {
            public delegate void LoadMoreEventHandler(object sender, EventArgs e);

            public event LoadMoreEventHandler LoadMoreEvent;

            private readonly LinearLayoutManager LayoutManager;
            private readonly SwipeRefreshLayout SwipeRefreshLayout;

            public XamarinRecyclerViewOnScrollListener(LinearLayoutManager layoutManager, SwipeRefreshLayout swipeRefreshLayout)
            {
                LayoutManager = layoutManager;
                SwipeRefreshLayout = swipeRefreshLayout;
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                try
                {
                    base.OnScrolled(recyclerView, dx, dy);

                    var visibleItemCount = recyclerView.ChildCount;
                    var totalItemCount = recyclerView.GetAdapter().ItemCount;

                    var pastVisibleItems = LayoutManager.FindFirstVisibleItemPosition();
                    if (pastVisibleItems == 0 && visibleItemCount != totalItemCount)
                    {
                        //Load More  from API Request
                        if (LoadMoreEvent != null) LoadMoreEvent(this, null);
                        //Start Load More messages From Database
                    }
                    else
                    {
                        if (SwipeRefreshLayout.Refreshing)
                        {
                            SwipeRefreshLayout.Refreshing = false;
                            SwipeRefreshLayout.Enabled = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        #endregion

        #region Menu

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MessagesBox_Menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.menu_block:
                    OnMenuBlockClick();
                    break;
                case Resource.Id.menu_clear_chat:
                    OnMenuClearChatClick();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        //Block User
        private void OnMenuBlockClick()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.BlockUnblock(Userid.ToString()) });

                    Toast.MakeText(this, GetText(Resource.String.Lbl_Blocked_successfully), ToastLength.Short)?.Show();
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnMenuClearChatClick()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    MAdapter.MessageList.Clear();
                    MAdapter.NotifyDataSetChanged();

                    var userDelete = LastChatActivity.MAdapter.UserList?.FirstOrDefault(a => a.UserId == Userid.ToString());
                    if (userDelete != null)
                    {
                        LastChatActivity.MAdapter.Remove(userDelete);
                    }

                    var dbDatabase = new SqLiteDatabase();
                    dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, Userid.ToString());

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Messages.ClearMessages(Userid.ToString()) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Toolbar & Selected

        private class ActionModeCallback : Java.Lang.Object, ActionMode.ICallback
        {
            private readonly MessagesBoxActivity Activity;
            public ActionModeCallback(MessagesBoxActivity activity)
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
                else if (id == Resource.Id.action_copy)
                {
                    CopyItems();
                    mode.Finish();
                    return true;
                }
                else if (id == Android.Resource.Id.Home)
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = true;
                        Timer.Start();
                    }

                    Activity.MAdapter.ClearSelections();

                    TopChatToolBar.Visibility = ViewStates.Visible;
                    ActionMode.Finish();

                    return true;
                }
                return false;
            }

            public bool OnCreateActionMode(ActionMode mode, IMenu menu)
            {
                SetSystemBarColor(Activity, AppSettings.MainColor);
                mode.MenuInflater.Inflate(Resource.Menu.menuChat, menu);
                return true;
            }

            public void SetSystemBarColor(Activity act, string color)
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
                    Activity.MAdapter.ClearSelections();
                    ActionMode = null!;
                    SetSystemBarColor(Activity, AppSettings.MainColor);

                    if (Timer != null)
                    {
                        Timer.Enabled = true;
                        Timer.Start();
                    }

                    TopChatToolBar.Visibility = ViewStates.Visible;
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

            //Delete Messages 
            private void DeleteItems()
            {
                try
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = true;
                        Timer.Start();
                    }

                    if (TopChatToolBar.Visibility != ViewStates.Visible)
                        TopChatToolBar.Visibility = ViewStates.Visible;

                    if (Methods.CheckConnectivity())
                    {
                        var selectedItemPositions = Activity.MAdapter.GetSelectedItems();
                        var selectedItemId = new List<int>();
                        for (var i = selectedItemPositions.Count - 1; i >= 0; i--)
                        {
                            var datItem = Activity.MAdapter.GetItem(selectedItemPositions[i]);
                            if (datItem != null)
                            {
                                selectedItemId.Add(int.Parse(datItem.Id));
                                Activity.MAdapter.RemoveData(selectedItemPositions[i], datItem);
                            }
                        }

                        //Send Api Delete By id
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Messages.DeleteMessages(Userid.ToString(), selectedItemId) });

                        Activity.MAdapter.NotifyDataSetChanged();
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

            //Copy Messages
            private void CopyItems()
            {
                try
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = true;
                        Timer.Start();
                    }

                    if (TopChatToolBar.Visibility != ViewStates.Visible)
                        TopChatToolBar.Visibility = ViewStates.Visible;

                    var allText = "";
                    var selectedItemPositions = Activity.MAdapter.GetSelectedItems();
                    for (var i = selectedItemPositions.Count - 1; i >= 0; i--)
                    {
                        var datItem = Activity.MAdapter.GetItem(selectedItemPositions[i]);
                        if (datItem != null)
                        {
                            allText = allText + " \n" + datItem.Text;
                        }
                    }

                    Methods.CopyToClipboard(Activity, allText);

                    Activity.MAdapter.NotifyDataSetChanged(); 
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            } 
        }

        public void OnItemClick(View view, MessageDataObject obj, int pos)
        {
            try
            {
                if (MAdapter.GetSelectedItemCount() > 0) // Add Select New Item 
                {
                    EnableActionMode(pos);
                }
                else
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = true;
                        Timer.Start();
                    }

                    if (TopChatToolBar.Visibility != ViewStates.Visible)
                        TopChatToolBar.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnItemLongClick(View view, MessageDataObject obj, int pos)
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
                    if (Timer != null)
                    {
                        Timer.Enabled = true;
                        Timer.Start();
                    }

                    TopChatToolBar.Visibility = ViewStates.Visible;
                    ActionMode.Finish();
                }
                else
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = false;
                        Timer.Stop();
                    }

                    TopChatToolBar.Visibility = ViewStates.Gone;
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
         
        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 108 || requestCode == CropImage.CropImageActivityRequestCode)
                {
                    if (Methods.CheckConnectivity())
                    {
                        var result = CropImage.GetActivityResult(data);
                        if (result.IsSuccessful)
                        { 
                            var resultPathImage = result.Uri.Path;
                            if (!string.IsNullOrEmpty(resultPathImage))
                            {
                                var timeNow = DateTime.Now.ToShortTimeString();
                                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                var time2 = Convert.ToString(unixTimestamp);

                                //Sent image 
                                if (Methods.CheckConnectivity())
                                {
                                    var message = new MessageDataObject
                                    {
                                        Id = time2,
                                        FromId = UserDetails.UserId,
                                        ToId = Userid.ToString(),
                                        Text = "",
                                        MediaFile = resultPathImage,
                                        MediaType = "media",
                                        DeletedFs1 = "",
                                        DeletedFs2 = "",
                                        Seen = "0",
                                        Time = time2,
                                        Extra = "",
                                        TimeText = timeNow,
                                        Position = "Right",
                                        MessageType = "Media"
                                    };

                                    MAdapter.MessageList.Add(message);
                                    MAdapter.NotifyItemInserted(MAdapter.MessageList.IndexOf(message));
                                    //Scroll Down >> 
                                    ChatBoxRecyclerView.ScrollToPosition(MAdapter.ItemCount - 1);
                                      
                                    var userData = ConvertData();
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MessageController.SendMessageTask(Userid, "", resultPathImage, time2, userData) });
                                }
                                else
                                {
                                    ToastUtils.ShowToast(GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                                } 
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        OpenDialogGallery();
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
         
        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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

        protected override void OnDestroy()
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }
                 
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        private void OpenDialogGallery()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Activity()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        //request Code 108
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}