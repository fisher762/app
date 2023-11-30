using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using PixelPhoto.Library.Anjo.StoriesProgressView;
using Newtonsoft.Json;
using PixelPhoto.Activities.Base;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Story;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace PixelPhoto.Activities.Story
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class ViewStoryActivity : BaseActivity, StoriesProgressView.IStoriesListener, View.IOnTouchListener, Android.Media.MediaPlayer.IOnCompletionListener, Android.Media.MediaPlayer.IOnPreparedListener
    {
        #region Variables Basic

        private ImageView StoryImageView, UserImageView;
        private VideoView StoryVideoView;
        private string UserId = "", StoryId = "";
        private StoriesProgressView StoriesProgress;
        private FetchStoriesObject.StoriesDataObject DataStories;
        private View ReverseView, SkipView;
        private TextView CaptionStoryTextView, UsernameTextView, LastSeenTextView, DeleteIconView;
        private int Counter;
        private long PressTime;
        private readonly long Limit = 500L;
        private Toolbar Toolbar;
        private HomeActivity GlobalContext;
        private LinearLayout StoryaboutLayout;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                 
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    Window?.SetDecorFitsSystemWindows(false);

                    Window?.AddFlags(WindowManagerFlags.Fullscreen);
                    //context.Window?.RequestFeature(WindowFeatures.NoTitle);
                }
                else if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    var mContentView = Window?.DecorView;

                    if (mContentView != null)
                    {
#pragma warning disable 618
                        var uiOptions = (int)mContentView.SystemUiVisibility;
#pragma warning restore 618
                        var newUiOptions = uiOptions;

                        newUiOptions |= (int)SystemUiFlags.Fullscreen;
                        newUiOptions |= (int)SystemUiFlags.HideNavigation;
#pragma warning disable 618
                        mContentView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
#pragma warning restore 618
                    }

                    Window?.AddFlags(WindowManagerFlags.Fullscreen);

                    Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    Window?.SetStatusBarColor(Color.Transparent);
                }
                else if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                {
                    Window?.AddFlags(WindowManagerFlags.TranslucentStatus);
                }
                 
                Methods.App.FullScreenApp(this);  
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.ViewStoryLayout);

                UserId = Intent?.GetStringExtra("UserId") ?? "";

                GlobalContext = HomeActivity.GetInstance();

                //Get Value And Set Toolbar
                InitComponent();
                InitVideoView();
                InitToolbar();
                
                LoadData();
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
         
        protected override void OnDestroy()
        {
            try
            {
                // Very important !
                StoriesProgress?.Destroy();
               
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnDestroy();
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
                StoryaboutLayout = FindViewById<LinearLayout>(Resource.Id.storyaboutLayout);
                StoryImageView = FindViewById<ImageView>(Resource.Id.imagstoryDisplay);
                StoriesProgress = FindViewById<StoriesProgressView>(Resource.Id.stories);
                CaptionStoryTextView = FindViewById<TextView>(Resource.Id.storyaboutText);
                UserImageView = FindViewById<ImageView>(Resource.Id.imageAvatar);
                UsernameTextView = FindViewById<TextView>(Resource.Id.username);
                LastSeenTextView = FindViewById<TextView>(Resource.Id.time);
                DeleteIconView = FindViewById<TextView>(Resource.Id.DeleteIcon);
                ReverseView = FindViewById<View>(Resource.Id.reverse);
                SkipView = FindViewById<View>(Resource.Id.skip);

                StoriesProgress.Visibility = ViewStates.Visible;
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, DeleteIconView,FontAwesomeIcon.TrashAlt);

                ReverseView.SetOnTouchListener(this);
                SkipView.SetOnTouchListener(this); 
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
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                { 
                    Toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(Toolbar);
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
         
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                { 
                    DeleteIconView.Click += DeleteIconViewOnClick;
                    ReverseView.Click += ReverseViewOnClick;
                    SkipView.Click += SkipViewOnClick;
                   
                }
                else
                { 
                    DeleteIconView.Click -= DeleteIconViewOnClick;
                    ReverseView.Click -= ReverseViewOnClick;
                    SkipView.Click -= SkipViewOnClick;
                  
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitVideoView()
        {
            try
            {
                StoryVideoView = FindViewById<VideoView>(Resource.Id.VideoView);

                StoryVideoView.SetOnPreparedListener(this);
                StoryVideoView.SetOnCompletionListener(this);
                StoryVideoView.SetAudioAttributes(new AudioAttributes.Builder().SetUsage(AudioUsageKind.Media).SetContentType(AudioContentType.Movie).Build());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Events

        //delete story
        private async void DeleteIconViewOnClick(object sender, EventArgs e)
        {
            try
            {
                StoriesProgress.Pause();

                if (StoryVideoView.IsPlaying)
                    StoryVideoView.Pause();

                if (Methods.CheckConnectivity())
                {
                    (var respondCode, var respondString) = await RequestsAsync.Story.DeleteStory(StoryId);
                    if (respondCode == 200)
                    {
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                var story = GlobalContext.NewsFeedFragment.StoryAdapter?.StoryList.FirstOrDefault(a => a.UserId == Convert.ToInt32(UserId));
                                if (story == null) return;
                                var item = story.Stories.FirstOrDefault(q => q.Id == int.Parse(StoryId));
                                if (item != null)
                                {
                                    story.Stories.Remove(item);

                                    GlobalContext.NewsFeedFragment.StoryAdapter.NotifyItemChanged(GlobalContext.NewsFeedFragment.StoryAdapter.StoryList.IndexOf(story));

                                    if (story.Stories.Count == 0)
                                    {
                                        GlobalContext.NewsFeedFragment.StoryAdapter?.StoryList.Remove(story);
                                        GlobalContext.NewsFeedFragment.StoryAdapter.NotifyDataSetChanged();
                                    }
                                }
                                Toast.MakeText(this, GetString(Resource.String.Lbl_Deleted), ToastLength.Short)?.Show();

                                StoriesProgress?.Destroy();
                                StoriesProgress = null!;
                                Finish();
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        }); 
                    }
                    else Methods.DisplayReportResult(this, respondString);
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
        }

        private void SkipViewOnClick(object sender, EventArgs e)
        {
            try
            {
                StoriesProgress.Skip();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ReverseViewOnClick(object sender, EventArgs e)
        {
            try
            {
                StoriesProgress.Reverse();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

        private async void LoadData()
        {
            try
            {
                DataStories = JsonConvert.DeserializeObject<FetchStoriesObject.StoriesDataObject>(Intent?.GetStringExtra("DataItem"));
                if (DataStories != null)
                {
                    GlideImageLoader.LoadImage(this, DataStories.Avatar, UserImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    
                    DeleteIconView.Visibility = DataStories.Owner ? ViewStates.Visible : ViewStates.Invisible;
                     
                    var count = DataStories.Stories.Count;
                    StoriesProgress.Visibility = ViewStates.Visible;
                    StoriesProgress.SetStoriesCount(count); // <- set stories
                    StoriesProgress.SetStoriesListener(this); // <- set listener 
                    //StoriesProgress.SetStoryDuration(10000L); // <- set a story duration   

                    var fistStory = DataStories.Stories.FirstOrDefault();
                    if (fistStory != null)
                    {
                        StoriesProgress.SetStoriesCountWithDurations(DataStories.DurationsList.ToArray());
                        
                        await SetStory(fistStory);
                        
                        StoriesProgress.StartStories(); // <- start progress 
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private string MediaFile;
  
        private async Task SetStory(FetchStoriesObject.StoryObject story)
        {
            try
            {
                StoryId = story.Id.ToString();

                if (DataStories.Owner)
                {
                    UsernameTextView.Text = DataStories.Name + "  " + Methods.Time.ReplaceTime(story.TimeText);
                    LastSeenTextView.Text = GetText(Resource.String.Lbl_SeenBy) + " " + story.Views; 
                }
                else
                {
                    UsernameTextView.Text = DataStories.Name;
                    LastSeenTextView.Text = story.TimeText;
                }
              
                if (string.IsNullOrEmpty(story.Caption) || string.IsNullOrWhiteSpace(story.Caption))
                {
                    StoryaboutLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    StoryaboutLayout.Visibility = ViewStates.Visible;
                    CaptionStoryTextView.Text = Methods.FunString.DecodeString(story.Caption);
                }
                MediaFile = story.MediaFile;

                if (StoryVideoView == null)
                    InitVideoView();

                var type = Methods.AttachmentFiles.Check_FileExtension(MediaFile);
                if (type == "Video")
                { 
                    //Show a progress
                    //RunOnUiThread(() => { try { AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading)); }catch (Exception e) { Methods.DisplayReportResultTrack(e); } });

                    var fileName = MediaFile.Split('/').Last();
                    MediaFile = AppTools.GetFile(DateTime.Now.Day.ToString() ,Methods.Path.FolderDiskStory, fileName, MediaFile);

                    StoryImageView.Visibility = ViewStates.Gone;
                    StoryVideoView.Visibility = ViewStates.Visible;
                    if (MediaFile.Contains("http"))
                    {
                        StoryVideoView.SetVideoURI(Uri.Parse(MediaFile));
                        StoryVideoView.Start();
                    }
                    else
                    {
                        var file = Uri.FromFile(new File(MediaFile));
                        StoryVideoView.SetVideoPath(file.Path);
                        StoryVideoView.Start();
                    }

                    await Task.Delay(500);
                }
                else
                {
                    StoryImageView.Visibility = ViewStates.Visible;
                    StoryVideoView.Visibility = ViewStates.Gone;

                    Glide.With(this).Load(story.MediaFile).Apply(new RequestOptions()).Into(StoryImageView);

                   // GlideImageLoader.LoadImage(this,story.MediaFile, StoryImageView, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public void OnPrepared(Android.Media.MediaPlayer mp)
        {
            try
            {
                //RunOnUiThread(() => { AndHUD.Shared.Dismiss(this); });

                StoryVideoView.Start();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnCompletion(Android.Media.MediaPlayer mp)
        {
            try
            {
                mp.Release();
                StoryVideoView?.StopPlayback();
                StoryVideoView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public async void OnNext()
        {
            try
            {
                StoryVideoView?.StopPlayback();
                StoryVideoView = null!;

                if (Counter + 1 > DataStories.Stories.Count)
                {
                    OnComplete();
                    return;
                }  

                var dataStory = DataStories.Stories[++Counter];
                if (dataStory != null)
                {
                    await SetStory(dataStory);
                }
                else
                {
                    OnComplete();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnPrev()
        {
            try
            {
                if (Counter - 1 < 0) return;
                var dataStory = DataStories.Stories[--Counter];
                if (dataStory != null)
                {
                    await SetStory(dataStory);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnComplete()
        {
            try
            {
                AdsGoogle.Ad_Interstitial(this);
                StoriesProgress?.Destroy();
                StoriesProgress = null!;
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    PressTime = CurrentTimeMillis();
                    StoriesProgress.Pause();

                    return false;

                case MotionEventActions.Up:
                    var now = CurrentTimeMillis();
                    StoriesProgress.Resume();

                    return Limit < now - PressTime;
            }

            return false;
        }

    }
}