using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.Lang;
using PixelPhoto.Activities.Base;
using PixelPhoto.Library.Anjo.StoriesProgressView;
using PixelPhoto.Activities.Editor;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils; 
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.Classes.Story;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using File = Java.IO.File;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri; 

namespace PixelPhoto.Activities.Story
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AddStoryActivity : BaseActivity
    {
        #region Variables Basic

        private ImageView StoryImageView;
        private VideoView StoryVideoView;
        private CircleButton PlayIconVideo, AddStoryButton;
        private EditText EmojisIconEditText;
        private RelativeLayout RootView;
        private string PathStory = "", Type = "", Thumbnail = UserDetails.Avatar;
        private StoriesProgressView StoriesProgress;
        private long Duration;
        private HomeActivity GlobalContext;
        private TextView TxtEdit;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Window.SetSoftInputMode(SoftInput.AdjustResize);
                Methods.App.FullScreenApp(this);  
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                base.OnCreate(savedInstanceState);

                // Create your application here
                SetContentView(Resource.Layout.AddStoryLayout);
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                GlobalContext = HomeActivity.GetInstance();

                var dataUri = Intent?.GetStringExtra("Uri") ?? "Data not available";
                if (dataUri != "Data not available" && !string.IsNullOrEmpty(dataUri)) PathStory = dataUri; // Uri file 
                var dataType = Intent?.GetStringExtra("Type") ?? "Data not available";
                if (dataType != "Data not available" && !string.IsNullOrEmpty(dataType)) Type = dataType; // Type file  

                if (Type == "image")
                    SetImageStory(PathStory);
                else
                    SetVideoStory(PathStory); 
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
                StoriesProgress.Destroy();

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                DestroyBasic();

                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                TxtEdit = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtEdit.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                TxtEdit.Visibility = ViewStates.Gone;

                StoryImageView = FindViewById<ImageView>(Resource.Id.imagstoryDisplay);
                StoryVideoView = FindViewById<VideoView>(Resource.Id.VideoView);
                PlayIconVideo = FindViewById<CircleButton>(Resource.Id.Videoicon_button);
                EmojisIconEditText = FindViewById<EditText>(Resource.Id.captionText);
                AddStoryButton = FindViewById<CircleButton>(Resource.Id.sendButton);
                RootView = FindViewById<RelativeLayout>(Resource.Id.storyDisplay);

                StoriesProgress = FindViewById<StoriesProgressView>(Resource.Id.stories);
                StoriesProgress.Visibility = ViewStates.Gone;

                Methods.SetColorEditText(EmojisIconEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                PlayIconVideo.Visibility = ViewStates.Gone;
                PlayIconVideo.Tag = "Play";
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_AddStory);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                    SetSupportActionBar(toolbar);
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
                    TxtEdit.Click += ToolbarTitleOnClick;
                    AddStoryButton.Click += AddStoryButtonOnClick;
                    StoryVideoView.Completion += StoryVideoViewOnCompletion;
                    PlayIconVideo.Click += PlayIconVideoOnClick;
                }
                else
                {
                    TxtEdit.Click -= ToolbarTitleOnClick;
                    AddStoryButton.Click -= AddStoryButtonOnClick;
                    StoryVideoView.Completion -= StoryVideoViewOnCompletion;
                    PlayIconVideo.Click -= PlayIconVideoOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetImageStory(string url)
        {
            try
            {
                TxtEdit.Visibility = ViewStates.Visible;
                if (StoryImageView.Visibility == ViewStates.Gone)
                    StoryImageView.Visibility = ViewStates.Visible;

                var file = Uri.FromFile(new File(url));

                Glide.With(this).Load(file.Path).Apply(new RequestOptions()).Into(StoryImageView);

                // GlideImageLoader.LoadImage(this, file.Path, StoryImageView, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                if (StoryVideoView.Visibility == ViewStates.Visible)
                    StoryVideoView.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetVideoStory(string url)
        {
            try
            {
                TxtEdit.Visibility = ViewStates.Gone;
                if (StoryImageView.Visibility == ViewStates.Visible)
                    StoryImageView.Visibility = ViewStates.Gone;

                if (StoryVideoView.Visibility == ViewStates.Gone)
                    StoryVideoView.Visibility = ViewStates.Visible;

                PlayIconVideo.Visibility = ViewStates.Visible;
                PlayIconVideo.Tag = "Play";
                PlayIconVideo.SetImageResource(Resource.Drawable.ic_play_arrow);

                if (StoryVideoView.IsPlaying)
                    StoryVideoView.Suspend();

                if (url.Contains("http"))
                {
                    StoryVideoView.SetVideoURI(Uri.Parse(url));
                }
                else
                {
                    var file = Uri.FromFile(new File(url));
                    StoryVideoView.SetVideoPath(file.Path);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                StoryImageView = null!;
                StoryVideoView = null!;
                PlayIconVideo = null!;
                EmojisIconEditText = null!;
                AddStoryButton = null!;
                RootView = null!;
                StoriesProgress = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void ToolbarTitleOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(EditImageActivity));
                intent.PutExtra("PathImage", PathStory);
                intent.PutExtra("IdImage", "");
                StartActivityForResult(intent, 2000);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PlayIconVideoOnClick(object sender, EventArgs e)
        {
            try
            {
                if (PlayIconVideo.Tag?.ToString() == "Play")
                {
                    MediaMetadataRetriever retriever;
                    if (PathStory.Contains("http"))
                    {
                        StoryVideoView.SetVideoURI(Uri.Parse(PathStory));

                        retriever = new MediaMetadataRetriever();
                        if ((int)Build.VERSION.SdkInt >= 14)
                            retriever.SetDataSource(PathStory, new Dictionary<string, string>());
                        else
                            retriever.SetDataSource(PathStory);
                    }
                    else
                    {
                        var file = Uri.FromFile(new File(PathStory));
                        StoryVideoView.SetVideoPath(file.Path);

                        retriever = new MediaMetadataRetriever();
                        //if ((int)Build.VERSION.SdkInt >= 14)
                        //    retriever.SetDataSource(file.Path, new Dictionary<string, string>());
                        //else
                        //    retriever.SetDataSource(file.Path);
                        retriever.SetDataSource(file.Path);
                    }
                    StoryVideoView.Start();

                    Duration = Long.ParseLong(retriever.ExtractMetadata(MetadataKey.Duration));
                    retriever.Release();

                    StoriesProgress.Visibility = ViewStates.Visible;
                    StoriesProgress.SetStoriesCount(1); // <- set stories
                    StoriesProgress.SetStoryDuration(Duration); // <- set a story duration
                    StoriesProgress.StartStories(); // <- start progress

                    PlayIconVideo.Tag = "Stop";
                    PlayIconVideo.SetImageResource(Resource.Drawable.ic_stop_white_24dp);
                }
                else
                {
                    StoriesProgress.Visibility = ViewStates.Gone;
                    StoriesProgress.Pause();

                    StoryVideoView.Pause();

                    PlayIconVideo.Tag = "Play";
                    PlayIconVideo.SetImageResource(Resource.Drawable.ic_play_arrow);
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void StoryVideoViewOnCompletion(object sender, EventArgs e)
        {
            try
            {
                StoriesProgress.Visibility = ViewStates.Gone;
                StoriesProgress.Pause();
                StoryVideoView.Pause();

                PlayIconVideo.Tag = "Play";
                PlayIconVideo.SetImageResource(Resource.Drawable.ic_play_arrow);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //add
        private async void AddStoryButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var time = Methods.Time.TimeAgo(DateTime.Now, false);
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var time2 = unixTimestamp.ToString();

                    var attach = new Attachments
                    {
                        Id = 0,
                        TypeAttachment = "file",
                        FileSimple = PathStory,
                        FileUrl = PathStory,
                    };

                    (var respondCode, var respondString) = await RequestsAsync.Story.CreateStory(attach, EmojisIconEditText.Text);
                    if (respondCode == 200)
                    {
                        if (respondString is CreateStoryObject storyObject)
                        {
                            AndHUD.Shared.Dismiss(this);
                            Toast.MakeText(this, GetString(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short)?.Show();

                            long idType = Type switch
                            {
                                "image" => 1,
                                "video" => 2,
                                _ => 1
                            };

                            var check = GlobalContext?.NewsFeedFragment?.StoryAdapter?.StoryList?.FirstOrDefault(a => a.UserId == Convert.ToInt32(UserDetails.UserId));
                            if (check != null)
                            {
                                var item = new FetchStoriesObject.StoryObject
                                {
                                    UserId = Convert.ToInt32(UserDetails.UserId),
                                    Id = storyObject.Id,
                                    Caption = EmojisIconEditText.Text,
                                    Owner = true,
                                    TimeText = time,
                                    Time = unixTimestamp,
                                    Type = idType,
                                    MediaFile = PathStory 
                                };

                                if (idType == 1)
                                {
                                    if (check.DurationsList == null)
                                        check.DurationsList = new List<long> { AppSettings.StoryDuration };
                                    else
                                        check.DurationsList.Add(AppSettings.StoryDuration); 
                                }
                                else
                                {
                                    var duration = AppTools.GetDuration(PathStory);

                                    if (check.DurationsList == null)
                                        check.DurationsList = new List<long> { Long.ParseLong(duration) };
                                    else
                                        check.DurationsList.Add(Long.ParseLong(duration));
                                }
                                 
                                check.Stories.Add(item); 
                            }
                            else
                            {
                                var userData = ListUtils.MyProfileList.FirstOrDefault();
                                var storiesList = new List<FetchStoriesObject.StoryObject>
                                {
                                    new FetchStoriesObject.StoryObject
                                    {
                                        UserId = Convert.ToInt32(UserDetails.UserId),
                                        Id = storyObject.Id,
                                        Caption = EmojisIconEditText.Text,
                                        Owner = true,
                                        TimeText = time,
                                        Time = unixTimestamp,
                                        Type = idType,
                                        MediaFile = PathStory,
                                        Duration = Duration.ToString(),
                                    }
                                };
                                 
                                var item = new FetchStoriesObject.StoriesDataObject
                                {
                                    Id = storyObject.Id,
                                    Avatar = userData?.Avatar ?? UserDetails.Avatar,
                                    Type = "",
                                    Username = UserDetails.FullName,
                                    Owner = true,
                                    UserId = Convert.ToInt32(UserDetails.UserId),
                                    Stories = new List<FetchStoriesObject.StoryObject>(storiesList),
                                    Name = AppTools.GetNameFinal(userData) ?? UserDetails.Username,
                                    Time = time2,
                                    Caption = EmojisIconEditText.Text,
                                    MediaFile = PathStory,
                                    Duration = Duration.ToString(),
                                };
 
                                if (idType == 1)
                                {
                                    if (item.DurationsList == null)
                                        item.DurationsList = new List<long> { AppSettings.StoryDuration };
                                    else
                                        item.DurationsList.Add(AppSettings.StoryDuration);
                                }
                                else
                                {
                                    var duration = AppTools.GetDuration(PathStory);

                                    if (item.DurationsList == null)
                                        item.DurationsList = new List<long> { Long.ParseLong(duration) };
                                    else
                                        item.DurationsList.Add(Long.ParseLong(duration));
                                }
                                 
                                GlobalContext?.NewsFeedFragment?.StoryAdapter?.StoryList?.Add(item);
                            }

                            GlobalContext?.NewsFeedFragment?.StoryAdapter?.NotifyDataSetChanged();

                            Finish();
                        } 
                    }
                    else Methods.DisplayAndHudErrorResult(this, respondString);
                     
                    //var item = new FileUpload()
                    //{
                    //    StoryFileType = Type,
                    //    StoryFilePath = PathStory,
                    //    StoryDescription = EmojisIconEditText.Text,
                    //    StoryTitle = EmojisIconEditText.Text,
                    //    StoryThumbnail = Thumbnail,
                    //};

                    //Intent intent = new Intent(this, typeof(PostService));
                    //intent.SetAction(PostService.ActionStory);
                    //intent.PutExtra("DataPost", JsonConvert.SerializeObject(item));
                    //StartService(intent);

                    //Finish(); 
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 2000 && resultCode == Result.Ok) // => NiceArtEditor
                {
                    //var imageId = data.GetStringExtra("ImageId") ?? "0";
                    var imagePath = data.GetStringExtra("ImagePath") ?? "Data not available";
                    if (imagePath != "Data not available" && !string.IsNullOrEmpty(imagePath))
                    {
                        try
                        {
                            PathStory = imagePath;
                            SetImageStory(imagePath);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
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