using System;
using System.Collections.Generic;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Newtonsoft.Json;
using PixelPhoto.Activities.Comment;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Widget;
using TheArtOfDev.Edmodo.Cropper;
using Java.IO;
using PixelPhoto.Activities.AddPost;
using PixelPhoto.Activities.Editor;
using PixelPhoto.Activities.Story;
using PixelPhoto.Activities.Tabbes.Fragments;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.OneSignal;
using PixelPhoto.SQLite;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using PixelPhoto.Activities.Chat;
using PixelPhoto.Activities.TikProfile;
using PixelPhotoClient.Classes.Messages;
using Android.Views.Animations;
using AndroidX.AppCompat.App;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Com.Google.Android.Play.Core.Install.Model;
using Com.Hitomi.Cmlibrary;
using Java.Lang;
using PixelPhoto.Activities.AddPost.Service;
using PixelPhoto.Activities.Chat.Services;
using PixelPhoto.Activities.Posts.page;
using PixelPhoto.Activities.SettingsUser;
using PixelPhoto.Library.Anjo.SuperTextLibrary;
using Q.Rorbin.Badgeview;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace PixelPhoto.Activities.Tabbes
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
    public class HomeActivity : AppCompatActivity, IOnMenuSelectedListener, IOnMenuStatusChangeListener, ServiceResultReceiver.IReceiver
    {
        #region Variables Basic

        public NewsFeedFragment NewsFeedFragment;
        public ExploreFragment ExploreFragment;
        public ExploreFragmentTheme2 ExploreFragmentTheme2;
        private NotificationFragment NotificationsFragment;
        public ProfileFragment ProfileFragment;
        public TikProfileFragment TikProfileFragment;
        public CircleMenu CircleMenu;
        private static HomeActivity Instance;
        private ServiceResultReceiver Receiver;
        private readonly Handler ExitHandler = new Handler(Looper.MainLooper);
        private bool RecentlyBackPressed;
        public string TypeOpen;
        public CustomNavigationController FragmentBottomNavigator;
        private FrameLayout FloatingAction;
        private PowerManager.WakeLock Wl;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Window?.SetSoftInputMode(SoftInput.AdjustNothing);

                base.OnCreate(savedInstanceState);
                Xamarin.Essentials.Platform.Init(this, savedInstanceState); // add this line to your code, it may also be called: bundle

                Methods.App.FullScreenApp(this);  
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                 
                AddFlagsWakeLock();
                 
                // Create your application here
                SetContentView(Resource.Layout.TabbedMainLayout);

                Instance = this;

                FloatingAction = FindViewById<FrameLayout>(Resource.Id.FloatingAction);

                //Get Value  
                SetupBottomNavigationView();
                SetupAddPostView();

                new Handler(Looper.MainLooper).Post(new Runnable(GetGeneralAppData));
                SetService(); 
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
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
        protected override void OnStop()
        {
            try
            {
                base.OnStop();
                NewsFeedFragment?.RecyclerFeed?.StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

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
                NewsFeedFragment?.RecyclerFeed?.ReleasePlayer(); 
                OffWakeLock();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

                FragmentBottomNavigator?.DisableAllNavigationButton();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region General App Data

        private void GetGeneralAppData()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();

                var data = ListUtils.DataUserLoginList.FirstOrDefault();
                if (data != null && data.Status != "Active")
                {
                    data.Status = "Active";
                    UserDetails.Status = "Active";
                    sqlEntity.InsertOrUpdateLogin_Credentials(data);
                }

                var dataUser = sqlEntity.GetMyProfile();
                if (dataUser != null)
                {
                    Glide.With(this).Load(UserDetails.Avatar).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CircleCrop()).Preload();
                    GlideImageLoader.LoadImage(this, UserDetails.Avatar, FragmentBottomNavigator.ProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                }

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetProfile_Api(this) });
                 
                LoadConfigSettings();

                InAppUpdate();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadConfigSettings()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var settingsData = dbDatabase.GetSettings();
                if (settingsData != null)
                    ListUtils.SettingsSiteList = settingsData;

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private void InAppUpdate()
        {
            try
            {
                if (AppSettings.ShowSettingsUpdateManagerApp)
                    UpdateManagerApp.CheckUpdateApp(this, 4711, Intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static int CountRateApp;
        public void InAppReview()
        {
            try
            {
                var inAppReview = MainSettings.InAppReview.GetBoolean(MainSettings.PrefKeyInAppReview, false);
                if (!inAppReview && AppSettings.ShowSettingsRateApp)
                {
                    if (CountRateApp == AppSettings.ShowRateAppCount)
                    {
                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                        dialog.Title(GetText(Resource.String.Lbl_RateOurApp));
                        dialog.Content(GetText(Resource.String.Lbl_RateOurAppContent));
                        dialog.PositiveText(GetText(Resource.String.Lbl_Rate)).OnPositive((materialDialog, action) =>
                        {
                            try
                            {
                                var store = new StoreReviewApp();
                                store.OpenStoreReviewPage(PackageName);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();

                        MainSettings.InAppReview?.Edit()?.PutBoolean(MainSettings.PrefKeyInAppReview, true)?.Commit();
                    }
                    else
                    {
                        CountRateApp++;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Timer
         
        public async Task Get_Notifications()
        {
            try
            {
                if (FragmentBottomNavigator != null)
                {
                    var (countNotifications, countMessages) = await ApiRequest.GetCountNotifications();
                    if (countNotifications != 0)
                    {
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                FragmentBottomNavigator.ShowNotificationBadge(true);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }

                    if (countMessages != 0)
                    {
                        ShowOrHideBadgeViewMessenger(countMessages, true);
                    }
                    else
                    {
                        ShowOrHideBadgeViewMessenger();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowOrHideBadgeViewMessenger(int countMessages = 0, bool show = false)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        if (show)
                        {
                            if (NewsFeedFragment?.ImageChat != null)
                            {
                                var gravity = (int)(GravityFlags.End | GravityFlags.Bottom);
                                var badge = new QBadgeView(this);
                                badge.BindTarget(NewsFeedFragment?.ImageChat);
                                badge.SetBadgeNumber(countMessages);
                                badge.SetBadgeGravity(gravity);
                                badge.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                badge.SetGravityOffset(10, true);
                            }
                        }
                        else
                        {
                            new QBadgeView(this).BindTarget(NewsFeedFragment?.ImageChat).Hide(true);
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    FloatingAction.Click += FloatingAction_Click;
                }
                else
                {
                    FloatingAction.Click -= FloatingAction_Click;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetToolBar(Toolbar toolbar, string title, bool showIconBack = true)
        {
            try
            {
                if (toolbar != null)
                {
                    if (!string.IsNullOrEmpty(title))
                        toolbar.Title = title;

                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(showIconBack);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    if (AppSettings.SetTabDarkTheme)
                        toolbar.SetBackgroundResource(Resource.Drawable.linear_gradient_drawable_Dark);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public static HomeActivity GetInstance()
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

        #region Set Navigation And Show Fragment

        private void SetupBottomNavigationView()
        {
            try
            {
                FragmentBottomNavigator = new CustomNavigationController(this);

                NewsFeedFragment = new NewsFeedFragment();
                FragmentBottomNavigator.FragmentListTab0.Add(NewsFeedFragment);
              
                if (AppSettings.SetTheme2)
                {
                    ExploreFragmentTheme2 = new ExploreFragmentTheme2();
                    FragmentBottomNavigator.FragmentListTab1.Add(ExploreFragmentTheme2);
                }
                else
                {
                    ExploreFragment = new ExploreFragment();
                    FragmentBottomNavigator.FragmentListTab1.Add(ExploreFragment);
                }
                NotificationsFragment = new NotificationFragment();
                FragmentBottomNavigator.FragmentListTab3.Add(NotificationsFragment);

                switch (AppSettings.ProfileTheme)
                {
                    case ProfileTheme.DefaultTheme:
                        ProfileFragment = new ProfileFragment();
                        FragmentBottomNavigator.FragmentListTab4.Add(ProfileFragment);
                        break;
                    case ProfileTheme.TikTheme:
                        TikProfileFragment = new TikProfileFragment();
                        FragmentBottomNavigator.FragmentListTab4.Add(TikProfileFragment);
                        break;
                }
                 
                FragmentBottomNavigator.ShowFragment0();

                GlideImageLoader.LoadImage(this, UserDetails.Avatar, FragmentBottomNavigator.ProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
            }
            catch (Exception e)
            {
                FragmentBottomNavigator.ShowFragment0();
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Back Pressed 

        public override void OnBackPressed()
        {
            try
            {
                if (CircleMenu.IsOpened)
                {
                    CircleMenu.CloseMenu();
                    CircleMenu.Visibility = ViewStates.Gone;
                    return;
                }

                if (FragmentBottomNavigator.GetCountFragment() > 0)
                {
                    NewsFeedFragment?.RecyclerFeed?.ReleasePlayer();
                    FragmentBottomNavigator.OnBackStackClickFragment(); 
                }
                else
                {
                    if (RecentlyBackPressed)
                    {
                        ExitHandler?.RemoveCallbacks(() => { RecentlyBackPressed = false; });
                        RecentlyBackPressed = false;
                        MoveTaskToBack(true);
                        Finish();
                    }
                    else
                    {
                        RecentlyBackPressed = true;
                        Toast.MakeText(this, GetString(Resource.String.press_again_exit), ToastLength.Long)?.Show();
                        ExitHandler?.PostDelayed(() => { RecentlyBackPressed = false; }, 2000L);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void FragmentNavigatorBack()
        {
            try
            {
                OnBackPressed();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events Open Fragment

        private void SetupAddPostView()
        {
            try
            {
                CircleMenu = FindViewById<CircleMenu>(Resource.Id.circle_menu);
                CircleMenu.Visibility = ViewStates.Gone;
                //CircleMenu.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Color.CircleMenu_colorDark : Resource.Color.CircleMenu_color);
                CircleMenu.SetMainMenu(Color.ParseColor("#444444"), Resource.Drawable.pix_add_icon, Resource.Drawable.ic_action_close);

                if (AppSettings.ShowGalleryImage)
                    CircleMenu.AddSubMenu(Color.ParseColor("#444444"), Resource.Drawable.pix_action_image);

                if (AppSettings.ShowGalleryVideo)
                    CircleMenu.AddSubMenu(Color.ParseColor("#444444"), Resource.Drawable.ic_action_video_icon);

                if (AppSettings.ShowGif)   
                    CircleMenu.AddSubMenu(Color.ParseColor("#444444"), Resource.Drawable.ic_action_gif_icon);

                if (AppSettings.ShowEmbedVideo)
                    CircleMenu.AddSubMenu(Color.ParseColor("#444444"), Resource.Drawable.ic_action_broken_link);

                CircleMenu.SetOnMenuSelectedListener(this);
                CircleMenu.SetOnMenuStatusChangeListener(this);
                CircleMenu.CloseMenu();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenFragment(Fragment frg)
        {
            try
            { 
                FragmentBottomNavigator.DisplayFragment(frg);
                NewsFeedFragment?.RecyclerFeed?.StopVideo(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenNewsFeedItem(string postId ,PostsObject item)
        {
            try
            { 
                var bundle = new Bundle();
                bundle.PutString("type", "ExploreAdapter");
                bundle.PutString("PostId", postId);

                if (item != null)
                    bundle.PutString("object", JsonConvert.SerializeObject(item));

                switch (item?.Type?.ToLower())
                {
                    case "youtube":
                    {
                        var intPost = new Intent(this, typeof(YoutubePlayerPostViewActivity));
                        intPost.PutExtra("type", "ExploreAdapter");
                        intPost.PutExtra("PostId", postId);
                        intPost.PutExtra("object", JsonConvert.SerializeObject(item));
                        StartActivity(intPost);

                        //YoutubePlayerPostViewFragment youtubePlayerPostViewFragment = new YoutubePlayerPostViewFragment { Arguments = bundle };
                        //OpenFragment(youtubePlayerPostViewFragment);
                        break;
                    }
                    case "image":
                    {
                        bundle = new Bundle();
                        bundle.PutString("postInfo", JsonConvert.SerializeObject(item));
                        bundle.PutString("PostId", postId);
                        bundle.PutString("indexImage", "0");

                        var fragment = new MultiImagesPostViewerFragment
                        {
                            Arguments = bundle
                        };

                        OpenFragment(fragment);
                        break;
                    }
                    default:
                    {
                        //One fragment inflates all the views instead of 5 fragments 
                        var globalPostFragment = new GlobalPostViewerFragment { Arguments = bundle };
                        OpenFragment(globalPostFragment);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void OpenCommentFragment(ObservableCollection<CommentObject> commentobject, string postid, string nameFragment)
        {
            try
            { 
                var bundle = new Bundle();
                bundle.PutString("PostId", postid);
                bundle.PutString("PrevFragment", nameFragment);
                bundle.PutString("json", commentobject != null ? JsonConvert.SerializeObject(commentobject) : "");

                var commentsFragment = new CommentsFragment
                {
                    Arguments = bundle
                };

                OpenFragment(commentsFragment);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Circle Menu

        private int PType;
        public void OnMenuSelected(int p0)
        {
            try
            {
                PType = p0; 
                 
                var intPost = new Intent(this, typeof(AddPostActivity));
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    if (p0 == 0) // image
                    {
                        intPost.PutExtra("TypePost", "Image");
                    }
                    else if (p0 == 1) // video
                    {
                        intPost.PutExtra("TypePost", "Video");
                    }
                    else if (p0 == 2) // gif
                    {
                        intPost.PutExtra("TypePost", "Gif");
                    }
                    else if (p0 == 3) // broken_link
                    {
                        intPost.PutExtra("TypePost", "EmbedVideo");
                    }

                    StartActivityForResult(intPost, 2500);
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        if (p0 == 0) // image
                        {
                            intPost.PutExtra("TypePost", "Image");
                        }
                        else if (p0 == 1) // video
                        {
                            intPost.PutExtra("TypePost", "Video");
                        }
                        else if (p0 == 2) // gif
                        {
                            intPost.PutExtra("TypePost", "Gif");
                        }
                        else if (p0 == 3) // broken_link
                        {
                            intPost.PutExtra("TypePost", "EmbedVideo");
                        }

                        StartActivityForResult(intPost, 2500);

                    }
                    else
                    {
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.Camera,
                            Manifest.Permission.ReadExternalStorage,
                            Manifest.Permission.WriteExternalStorage,
                        }, 555);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnMenuClosed()
        {
            try
            {
                CircleMenu.Visibility = ViewStates.Gone; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnMenuOpened()
        {

        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (requestCode == 2500 && resultCode == Result.Ok) //Add Post
                {
                    RunOnUiThread(() =>
                    {
                        //    if (FragmentBottomNavigator.PageNumber != 0)
                        //        NavigationTabBar.SetModelIndex(0, true);

                        var url = data.GetStringExtra("PostUrl") ?? "";
                        var json = data.GetStringExtra("PostData") ?? "";

                        var dataPost = JsonConvert.DeserializeObject<PostsObject>(json);
                        if (dataPost != null)
                        {
                            NewsFeedFragment?.NewsFeedAdapter?.PostList.Insert(0, dataPost);
                            NewsFeedFragment?.NewsFeedAdapter?.NotifyItemInserted(0);
                            NewsFeedFragment?.RecyclerFeed?.ScrollToPosition(0); // >> go to post
                            NewsFeedFragment?.ShowEmptyPage();

                            var intent = new Intent(this, typeof(PostService));
                            intent.SetAction(PostService.ActionPost);
                            intent.PutExtra("DataPost", json);
                            intent.PutExtra("UrlPost", url);
                            StartService(intent);
                             
                            if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                            {
                                var userPost = new PostsObject
                                {
                                    Time = dataPost.Time,
                                    Name = dataPost.Name,
                                    Avatar = dataPost.Avatar,
                                    Comments = dataPost.Comments,
                                    Dailymotion = dataPost.Dailymotion,
                                    Description = dataPost.Description,
                                    IsLiked = dataPost.IsLiked,
                                    IsOwner = dataPost.IsOwner,
                                    IsSaved = dataPost.IsSaved,
                                    IsShouldHide = dataPost.IsShouldHide,
                                    IsVerified = dataPost.IsVerified,
                                    Likes = dataPost.Likes,
                                    MediaSet = dataPost.MediaSet,
                                    PostId = dataPost.PostId,
                                    Type = dataPost.Type,
                                    UserId = dataPost.UserId,
                                    Username = dataPost.Username,
                                    TimeText = dataPost.TimeText,
                                    Votes = dataPost.Votes,
                                    Link = dataPost.Link,
                                    Mp4 = dataPost.Mp4,
                                    Playtube = dataPost.Playtube,
                                    Registered = dataPost.Registered,
                                    Reported = dataPost.Reported,
                                    Views = dataPost.Views,
                                    Vimeo = dataPost.Vimeo,
                                    Youtube = dataPost.Youtube,
                                };


                                if (ProfileFragment != null)
                                {
                                    var dataPostUser = ProfileFragment?.NewsFeedStyle switch
                                    {
                                        "Linear" => ProfileFragment?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == dataPost.PostId),
                                        "Grid" => ProfileFragment?.UserPostAdapter?.PostList?.FirstOrDefault(a => a.PostId == dataPost.PostId),
                                        _ => null
                                    };

                                    if (dataPostUser == null)
                                    {
                                        dynamic adapter = ProfileFragment?.NewsFeedStyle switch
                                        {
                                            "Linear" => ProfileFragment?.MAdapter,
                                            "Grid" => ProfileFragment?.UserPostAdapter,
                                            _ => ProfileFragment?.MAdapter
                                        };
                                        if (adapter != null)
                                        {
                                            adapter.PostList.Insert(0, userPost);
                                            adapter.NotifyItemInserted(0);
                                        }
                                    }
                                }
                            }
                            else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                            {
                                var list = TikProfileFragment?.MyPostTab?.MAdapter?.PostList;
                                if (list != null)
                                {
                                    var dataPostUser = list.FirstOrDefault(a => a.PostId == dataPost.PostId);
                                    if (dataPostUser == null)
                                    {
                                        var userPost = new PostsObject
                                        {
                                            Time = dataPost.Time,
                                            Name = dataPost.Name,
                                            Avatar = dataPost.Avatar,
                                            Comments = dataPost.Comments,
                                            Dailymotion = dataPost.Dailymotion,
                                            Description = dataPost.Description,
                                            IsLiked = dataPost.IsLiked,
                                            IsOwner = dataPost.IsOwner,
                                            IsSaved = dataPost.IsSaved,
                                            IsShouldHide = dataPost.IsShouldHide,
                                            IsVerified = dataPost.IsVerified,
                                            Likes = dataPost.Likes,
                                            MediaSet = dataPost.MediaSet,
                                            PostId = dataPost.PostId,
                                            Type = dataPost.Type,
                                            UserId = dataPost.UserId,
                                            Username = dataPost.Username,
                                            TimeText = dataPost.TimeText,
                                            Votes = dataPost.Votes,
                                            Link = dataPost.Link,
                                            Mp4 = dataPost.Mp4,
                                            Playtube = dataPost.Playtube,
                                            Registered = dataPost.Registered,
                                            Reported = dataPost.Reported,
                                            Views = dataPost.Views,
                                            Vimeo = dataPost.Vimeo,
                                            Youtube = dataPost.Youtube,
                                        };

                                        list.Insert(0, userPost);
                                        TikProfileFragment?.MyPostTab?.MAdapter?.NotifyItemInserted(list.IndexOf(list.FirstOrDefault()));
                                    }
                                }
                            }
                        }
                    });
                }
                else if (requestCode == 2250 && resultCode == Result.Ok) //Edit Post
                {
                    var id = Convert.ToInt32(data.GetStringExtra("PostId") ?? "0");
                    var text = data.GetStringExtra("NewTextPost") ?? " ";

                    RunOnUiThread(() =>
                    {
                        var dataPost = NewsFeedFragment.NewsFeedAdapter?.PostList?.FirstOrDefault(a => a.PostId == id);
                        if (dataPost != null)
                        {
                            dataPost.Description = text;
                            NewsFeedFragment.NewsFeedAdapter.NotifyItemChanged(NewsFeedFragment.NewsFeedAdapter.PostList.IndexOf(dataPost));
                        }

                        if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                        {
                            if (ProfileFragment != null)
                            {
                                var dataPostUser = ProfileFragment?.NewsFeedStyle switch
                                {
                                    "Linear" => ProfileFragment?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == id),
                                    "Grid" => ProfileFragment?.UserPostAdapter?.PostList?.FirstOrDefault(a => a.PostId == id),
                                    _ => null
                                };

                                if (dataPostUser != null)
                                {
                                    dynamic adapter = ProfileFragment?.NewsFeedStyle switch
                                    {
                                        "Linear" => ProfileFragment?.MAdapter,
                                        "Grid" => ProfileFragment?.UserPostAdapter,
                                        _ => ProfileFragment?.MAdapter
                                    };

                                    dataPostUser.Description = text;

                                    int index = adapter.PostList.IndexOf(dataPostUser);
                                    if (index >= 0)
                                    {
                                        adapter.NotifyItemChanged(index);
                                    }
                                }
                            }
                        }
                        else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                        {
                            var dataPostUser = TikProfileFragment?.MyPostTab?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == id);
                            if (dataPostUser != null)
                            {
                                dataPostUser.Description = text;
                                TikProfileFragment.MyPostTab?.MAdapter.NotifyItemChanged(TikProfileFragment.MyPostTab.MAdapter.PostList.IndexOf(dataPostUser));
                            }
                        }

                        var currentFragment = FragmentBottomNavigator.GetSelectedTabLastStackFragment();
                        if (currentFragment is GlobalPostViewerFragment frmImage)
                        {
                            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
                            {
                                frmImage.Description.Visibility = ViewStates.Gone;
                            }
                            else
                            {
                                var description = Methods.FunString.DecodeString(text);
                                var readMoreOption = new StReadMoreOption.Builder()
                                    .TextLength(250, StReadMoreOption.TypeCharacter)
                                    .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                                    .LessLabel(GetText(Resource.String.Lbl_ReadLess))
                                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                                    .LabelUnderLine(true)
                                    .Build();
                                readMoreOption.AddReadMoreTo(frmImage.Description, new Java.Lang.String(description));
                            } 
                        }
                        else if (currentFragment is HashTagPostFragment frmHashTag)
                        {
                            var dataHash = frmHashTag?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == id);
                            if (dataHash != null)
                            {
                                dataHash.Description = text;
                                frmHashTag?.MAdapter.NotifyItemChanged(frmHashTag.MAdapter.PostList.IndexOf(dataHash));
                            }
                        }
                    });
                }
                else if (requestCode == 503 && resultCode == Result.Ok) // Add story using camera
                {
                    try
                    {
                        if (string.IsNullOrEmpty(IntentController.CurrentPhotoPath))
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short)?.Show();
                        }
                        else
                        {
                            if (Methods.MultiMedia.CheckFileIfExits(IntentController.CurrentPhotoPath) != "File Dont Exists")
                            {
                                var check = AppTools.CheckMimeTypesWithServer(IntentController.CurrentPhotoPath);
                                if (!check)
                                {
                                    //this file not supported on the server , please select another file 
                                    Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short)?.Show();
                                    return;
                                }

                                //Do something with your Uri
                                var intent = new Intent(this, typeof(AddStoryActivity));
                                intent.PutExtra("Uri", IntentController.CurrentPhotoPath);
                                intent.PutExtra("Type", "image");
                                StartActivity(intent);
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short)?.Show();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short)?.Show();
                    }
                }
                else if (requestCode == 501 && resultCode == Result.Ok) // Add video story
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var check = AppTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            //this file not supported on the server , please select another file 
                            Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short)?.Show();
                            return;
                        }

                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            var intent = new Intent(this, typeof(AddStoryActivity));
                            intent.PutExtra("Uri", filepath);
                            intent.PutExtra("Type", "video");
                            StartActivity(intent);
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short)?.Show();
                    }
                }
                else if (requestCode == 500 && resultCode == Result.Ok) // Add image story
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var check = AppTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            //this file not supported on the server , please select another file 
                            Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short)?.Show();
                            return;
                        }

                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Image")
                        {
                            if (!string.IsNullOrEmpty(filepath))
                            {
                                //Do something with your Uri
                                var intent = new Intent(this, typeof(AddStoryActivity));
                                intent.PutExtra("Uri", filepath);
                                intent.PutExtra("Type", "image");
                                StartActivity(intent);
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                            }
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == CropImage.CropImageActivityRequestCode && resultCode == Result.Ok)
                {
                    if (Methods.CheckConnectivity())
                    {
                        var result = CropImage.GetActivityResult(data);
                        if (!result.IsSuccessful)
                            return;

                        switch (TypeOpen)
                        {
                            case "MyProfile":
                            {
                                var resultPathImage = result.Uri.Path;
                                switch (AppSettings.ProfileTheme)
                                {
                                    case ProfileTheme.DefaultTheme when ProfileFragment.UserProfileImage == null:
                                        return;
                                    case ProfileTheme.DefaultTheme:
                                    {
                                        if (!string.IsNullOrEmpty(resultPathImage))
                                            GlideImageLoader.LoadImage(this, resultPathImage, ProfileFragment.UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                        break;
                                    }
                                    case ProfileTheme.TikTheme when TikProfileFragment.ImageUser == null:
                                        return;
                                    case ProfileTheme.TikTheme:
                                    {
                                        if (!string.IsNullOrEmpty(resultPathImage))
                                            GlideImageLoader.LoadImage(this, resultPathImage, TikProfileFragment.ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                        break;
                                    }
                                }

                                if (!string.IsNullOrEmpty(resultPathImage))
                                {
                                    var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                                    if (dataUser != null)
                                    {
                                        dataUser.Avatar = resultPathImage;

                                        var dbDatabase = new SqLiteDatabase();
                                        dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);
                                    }

                                    var dataStory = NewsFeedFragment.StoryAdapter?.StoryList?.FirstOrDefault(a => a.Type == "Your");
                                    if (dataStory != null)
                                    {
                                        dataStory.Avatar = resultPathImage;
                                        NewsFeedFragment.StoryAdapter.NotifyItemChanged(0);
                                    }

                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.SaveImageUser(resultPathImage) });
                                }
                                else
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                                }

                                break;
                            }
                            case "StoryImage":
                            {
                                var resultPathImage = result.Uri.Path;
                                var check = AppTools.CheckMimeTypesWithServer(resultPathImage);
                                if (!check)
                                {
                                    //this file not supported on the server , please select another file 
                                    Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short)?.Show();
                                    return;
                                }

                                var type = Methods.AttachmentFiles.Check_FileExtension(resultPathImage);
                                if (type == "Image")
                                {
                                    if (!string.IsNullOrEmpty(resultPathImage))
                                    {
                                        //Do something with your Uri
                                        var intent = new Intent(this, typeof(AddStoryActivity));
                                        intent.PutExtra("Uri", resultPathImage);
                                        intent.PutExtra("Type", "image");
                                        StartActivity(intent);
                                    }
                                    else
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == 2200 && resultCode == Result.Ok) // => NiceArtEditor add story text
                {
                    RunOnUiThread(() =>
                    {
                        try
                        {
                            var imagePath = data.GetStringExtra("ImagePath") ?? "Data not available";
                            if (imagePath != "Data not available" && !string.IsNullOrEmpty(imagePath))
                            {
                                //Do something with your Uri
                                var intent = new Intent(this, typeof(AddStoryActivity));
                                intent.PutExtra("Uri", imagePath);
                                intent.PutExtra("Type", "image");
                                StartActivity(intent);
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                }
                else if (requestCode == 4711)
                {
                    switch (resultCode) // The switch block will be triggered only with flexible update since it returns the install result codes
                    {
                        case Result.Ok:
                            // In app update success
                            if (UpdateManagerApp.AppUpdateTypeSupported == AppUpdateType.Immediate)
                                Toast.MakeText(this, "App updated", ToastLength.Short)?.Show();
                            break;
                        case Result.Canceled:
                            Toast.MakeText(this, "In app update cancelled", ToastLength.Short)?.Show();
                            break;
                        case (Result)ActivityResult.ResultInAppUpdateFailed:
                            Toast.MakeText(this, "In app update failed", ToastLength.Short)?.Show();
                            break;
                    }
                }

                else if (requestCode == 3000)
                {
                    ProfileFragment?.LoadProfile();
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108)
                {
                    switch (TypeOpen)
                    {
                        case "StoryImage" when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                            OpenDialogGallery("StoryImage");
                            break;
                        case "StoryImage":
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long)?.Show();
                            break;
                        case "MyProfile" when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                            OpenDialogGallery("MyProfile");
                            break;
                        case "MyProfile":
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long)?.Show();
                            break;
                        case "StoryVideo" when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                            //requestCode >> 501 => video Gallery
                            new IntentController(this).OpenIntentVideoGallery();
                            break;
                        case "StoryVideo":
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long)?.Show();
                            break;
                        case "StoryCamera" when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                            //requestCode >> 503 => Camera Gallery
                            new IntentController(this).OpenIntentCamera();
                            break;
                        case "StoryCamera":
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long)?.Show();
                            break;
                    }
                }
                else if (requestCode == 555) // => Open AddPostActivity
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        var intPost = new Intent(this, typeof(AddPostActivity));
                        switch (PType)
                        {
                            // image
                            case 0:
                                intPost.PutExtra("TypePost", "Image");
                                break;
                            // video
                            case 1:
                                intPost.PutExtra("TypePost", "Video");
                                break;
                            // gif
                            case 2:
                                intPost.PutExtra("TypePost", "Gif");
                                break;
                            // broken_link
                            case 3:
                                intPost.PutExtra("TypePost", "EmbedVideo");
                                break;
                        }

                        StartActivityForResult(intPost, 2500);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == 110) 
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        Window.AddFlags(WindowManagerFlags.KeepScreenOn);
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

        #region WakeLock System

        private void AddFlagsWakeLock()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.WakeLock) == Permission.Granted)
                    {
                        Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                    }
                    else
                    {
                        //request Code 110
                        new PermissionsController(this).RequestPermission(110);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOnWakeLock()
        {
            try
            {
                var pm = (PowerManager)GetSystemService(PowerService);
                Wl = pm.NewWakeLock(WakeLockFlags.ScreenDim, "My Tag");
                Wl.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOffWakeLock()
        {
            try
            {
                var pm = (PowerManager)GetSystemService(PowerService);
                Wl = pm.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                Wl.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OffWakeLock()
        {
            try
            {
                // ..screen will stay on during this section..
                Wl?.Release();
                Wl = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void FloatingAction_Click(object sender, EventArgs e)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        var interpolator = new MyBounceInterpolator(0.2, 10);

                        var animationScale = AnimationUtils.LoadAnimation(this, Resource.Animation.scale);

                        animationScale.Interpolator = interpolator;

                        FloatingAction.StartAnimation(animationScale);

                        if (!CircleMenu.IsOpened)
                        {
                            CircleMenu.Visibility = ViewStates.Visible;
                            CircleMenu.OpenMenu();
                        }
                        else
                        {
                            CircleMenu.CloseMenu();
                            CircleMenu.Visibility = ViewStates.Gone;
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void OpenEditColor()
        {
            try
            {
                var intent = new Intent(this, typeof(EditColorActivity));
                StartActivityForResult(intent, 2200);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Service Chat

        public void SetService(bool run = true)
        {
            try
            {
                if (run)
                {
                    try
                    {
                        Receiver = new ServiceResultReceiver(new Handler(Looper.MainLooper));
                        Receiver.SetReceiver(this);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }

                    var intent = new Intent(this, typeof(ChatApiService));
                    intent.PutExtra("receiverTag", Receiver);
                    StartService(intent);
                }
                else
                {
                    var intentService = new Intent(this, typeof(ChatApiService));
                    StopService(intentService);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnReceiveResult(int resultCode, Bundle resultData)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<GetChatsObject>(resultData.GetString("Json"));
                if (result != null)
                {
                    LastChatActivity.GetInstance()?.LoadDataJsonLastChat(result);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

                // Toast.MakeText(Application.Context, "Exception  " + e, ToastLength.Short)?.Show();
            }
        }

        #endregion
         
        public void GetOneSignalNotification()
        {
            try
            {
                var type = Intent?.GetStringExtra("TypeNotification") ?? "";
                if (!string.IsNullOrEmpty(type))
                {
                    switch (type)
                    {
                        case "accept_request":
                        case "followed_u":
                            {
                                AppTools.OpenProfile(this, OneSignalNotification.Userid, OneSignalNotification.UserData);
                                break;
                            }
                        case "commented_ur_post":
                        case "mentioned_u_in_comment":
                        case "mentioned_u_in_post":
                        case "liked_ur_comment":
                        case "reply_ur_comment":
                        case "shared_your_post":
                        case "liked_ur_post":
                            {
                                OpenNewsFeedItem(OneSignalNotification.PostData.PostId.ToString(), OneSignalNotification.PostData);
                                break;
                            }
                        default:
                            {
                                AppTools.OpenProfile(this, OneSignalNotification.Userid, OneSignalNotification.UserData);
                                break;
                            }
                    }
                }

                var deepLinks = Intent?.GetStringExtra("DeepLinks") ?? "";
                if (!string.IsNullOrEmpty(deepLinks))
                {
                    var Id = Intent?.GetStringExtra("Id") ?? "";
                    switch (deepLinks)
                    {
                        case "OpenPost":
                            {
                                OpenNewsFeedItem(Id, null);
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void OpenDialogGallery(string type)
        {
            try
            {
                TypeOpen = type;
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