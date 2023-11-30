using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Newtonsoft.Json;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;

namespace PixelPhoto.Activities.AddPost.Service
{
    //public class FileUpload
    //{
    //    public string IdPost { set; get; }
    //    public string PagePost { set; get; }
    //    public string Content { set; get; }
    //    public string PostPrivacy { set; get; }
    //    public string PostFeelingType { set; get; }
    //    public string PostFeelingText { set; get; }
    //    public string PlaceText { set; get; }
    //    public ObservableCollection<Attachments> AttachmentList { set; get; }
    //    public string IdColor { set; get; }
    //    public string AlbumName { set; get; }

    //    //Story
    //    public string StoryTitle { set; get; }
    //    public string StoryDescription { set; get; }
    //    public string StoryFilePath { set; get; }
    //    public string StoryFileType { set; get; }
    //    public string StoryThumbnail { set; get; }
    //}

    [Service(Exported = false)]
    public class PostService : Android.App.Service
    {
        #region Variables Basic

        public static string ActionPost;
        public static string ActionStory;
        private static string UrlPost;
        private static PostService Service;
       
        private HomeActivity GlobalContextTabbed;
        //private FileUpload DataPost;

        #endregion

        #region General

        public static PostService GetPostService()
        {
            return Service;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null!;
        }
         
        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                Service = this;
                 
                GlobalContextTabbed = HomeActivity.GetInstance();
                MNotificationManager = (NotificationManager)GetSystemService(NotificationService);

                Create_Progress_Notification();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                base.OnStartCommand(intent, flags, startId);

                var action = intent.Action;
                var data = intent.GetStringExtra("DataPost") ?? "";
                UrlPost = intent.GetStringExtra("UrlPost") ?? "";
                if (!string.IsNullOrEmpty(data))
                {
                    if (action == ActionPost)
                    {
                        var dataPost = JsonConvert.DeserializeObject<PostsObject>(data);
                        if (dataPost != null)
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => AddPostMediaApi(dataPost, UrlPost) });
                    }
                    else if (action == ActionStory)
                    {
                        //DataPost = JsonConvert.DeserializeObject<FileUpload>(data);
                        //if (DataPost != null)
                        //{
                        //    AddStory();
                        //    UpdateNotification("Story"); 
                        //}
                    }
                }

                return StartCommandResult.Sticky;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return StartCommandResult.NotSticky;
            }
        }
         
        #region Add Post 

        private async Task AddPostUrlVideoApi(PostsObject dataPosts, string url)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    (var apiStatus, var respond) = await RequestsAsync.Post.PostVideoFrom(url, dataPosts.Description);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageIdObject messageIdObject)
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Post_Added), ToastLength.Long)?.Show();

                            var id = messageIdObject.Id;
                            var dataPost = GlobalContextTabbed.NewsFeedFragment.NewsFeedAdapter?.PostList?.FirstOrDefault(a => a.PostId == dataPosts.PostId);
                            if (dataPost != null)
                            {
                                dataPost.PostId = id;
                            }

                            if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                            {
                                if (GlobalContextTabbed.ProfileFragment != null)
                                {
                                    var dataPostUser = GlobalContextTabbed.ProfileFragment?.NewsFeedStyle switch
                                    {
                                        "Linear" => GlobalContextTabbed.ProfileFragment?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == id),
                                        "Grid" => GlobalContextTabbed.ProfileFragment?.UserPostAdapter?.PostList?.FirstOrDefault(a => a.PostId == id),
                                        _ => null
                                    };

                                    if (dataPostUser != null)
                                    {
                                        //dynamic adapter = ProfileFragment?.NewsFeedStyle switch
                                        //{
                                        //    "Linear" => ProfileFragment?.MAdapter,
                                        //    "Grid" => ProfileFragment?.UserPostAdapter,
                                        //    _ => ProfileFragment?.MAdapter
                                        //};

                                        dataPostUser.PostId = id;
                                        // adapter?.NotifyItemChanged(adapter.IndexOf(dataPostUser));
                                    }
                                }
                            }
                            else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                            {
                                var dataPostUser = GlobalContextTabbed.TikProfileFragment?.MyPostTab?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == dataPosts.PostId);
                                if (dataPostUser != null)
                                {
                                    dataPostUser.PostId = id;
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(GlobalContextTabbed, respond);
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                RemoveNotification();
            }
            catch (Exception e)
            {
                RemoveNotification();
                Methods.DisplayReportResultTrack(e);
            } 
        }

        private async Task AddPostMediaApi(PostsObject dataPosts, string url)
        {
            try
            {
                if (dataPosts != null)
                {
                    var content = dataPosts.Description;
                    var typePost = dataPosts.Type;

                    if (typePost == Classes.TypePostEnum.Image.ToString()) // image
                    {
                        typePost = "images[]";
                    }
                    else if (typePost == Classes.TypePostEnum.Video.ToString()) // video
                    {
                        typePost = "video";
                    }
                    else if (typePost == Classes.TypePostEnum.Gif.ToString()) // gif
                    {
                        typePost = "gif_url";
                    }
                    else if (typePost == Classes.TypePostEnum.EmbedVideo.ToString()) // EmbedVideo
                    {
                        typePost = "youtube";
                    }

                    if (typePost == "youtube")
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => AddPostUrlVideoApi(dataPosts, url) });
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(content) && dataPosts.MediaSet.Count == 0)
                        {
                            Toast.MakeText(this, GetString(Resource.String.Lbl_YouCannot_PostanEmptyPost), ToastLength.Long)?.Show();
                        }
                        else
                        {
                            if (Methods.CheckConnectivity())
                            {
                                var list = new List<Attachments>();
                                if (typePost != "gif_url")
                                {
                                    list.AddRange(dataPosts.MediaSet.Select(item => new Attachments
                                    {
                                        FileUrl = item.File,
                                        TypeAttachment = typePost,
                                        Id = item.Id,
                                        FileSimple = item.File,
                                        Thumb = new Attachments.VideoThumb { FileUrl = item.Extra, },
                                    }));
                                }
                                else
                                {
                                    url = dataPosts.MediaSet[0].File;
                                }

                                (var apiStatus, var respond) = await RequestsAsync.Post.PostMedia(dataPosts.Type, list, content, url);
                                if (apiStatus == 200)
                                {
                                    if (respond is MessageIdObject messageIdObject)
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_Post_Added), ToastLength.Long)?.Show();

                                        // put the String to pass back into an Intent and close this activity 
                                        var id = messageIdObject.Id;
                                        var dataPost = GlobalContextTabbed.NewsFeedFragment?.NewsFeedAdapter?.PostList?.FirstOrDefault(a => a.PostId == dataPosts.PostId);
                                        if (dataPost != null)
                                        {
                                            dataPost.PostId = id;
                                        }

                                        if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                                        {
                                            if (GlobalContextTabbed.ProfileFragment != null)
                                            {
                                                var dataPostUser = GlobalContextTabbed.ProfileFragment?.NewsFeedStyle switch
                                                {
                                                    "Linear" => GlobalContextTabbed.ProfileFragment?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == id),
                                                    "Grid" => GlobalContextTabbed.ProfileFragment?.UserPostAdapter?.PostList?.FirstOrDefault(a => a.PostId == id),
                                                    _ => null
                                                };

                                                if (dataPostUser != null)
                                                {
                                                    //dynamic adapter = ProfileFragment?.NewsFeedStyle switch
                                                    //{
                                                    //    "Linear" => ProfileFragment?.MAdapter,
                                                    //    "Grid" => ProfileFragment?.UserPostAdapter,
                                                    //    _ => ProfileFragment?.MAdapter
                                                    //};

                                                    dataPostUser.PostId = id;
                                                    // adapter?.NotifyItemChanged(adapter.IndexOf(dataPostUser));
                                                }
                                            }
                                        }
                                        else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                                        {
                                            var dataPostUser = GlobalContextTabbed.TikProfileFragment.MyPostTab?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == dataPosts.PostId);
                                            if (dataPostUser != null)
                                            {
                                                dataPostUser.PostId = id;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_PostFailedUpload), ToastLength.Short)?.Show();

                                    var dataPost = GlobalContextTabbed.NewsFeedFragment?.NewsFeedAdapter?.PostList?.FirstOrDefault(a => a.PostId == dataPosts.PostId);
                                    if (dataPost != null)
                                    {
                                        GlobalContextTabbed.NewsFeedFragment?.NewsFeedAdapter?.PostList.Remove(dataPost);
                                        var index = GlobalContextTabbed.NewsFeedFragment.NewsFeedAdapter.PostList.IndexOf(dataPosts);
                                        GlobalContextTabbed.NewsFeedFragment?.NewsFeedAdapter?.NotifyItemRemoved(index);
                                        GlobalContextTabbed.NewsFeedFragment?.NewsFeedAdapter?.NotifyDataSetChanged();
                                    }

                                    Methods.DisplayReportResult(GlobalContextTabbed, respond);
                                }
                            }
                            else
                            {
                                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                            }
                        }
                        RemoveNotification();
                    }
                }
            }
            catch (Exception e)
            {
                RemoveNotification();
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Notification

        private readonly string NotificationChannelId = "pixelPhoto_ch_1";

        private NotificationManager MNotificationManager;
        private NotificationCompat.Builder NotificationBuilder;
        private RemoteViews NotificationView;
        private void Create_Progress_Notification()
        {
            try
            {
                MNotificationManager = (NotificationManager)GetSystemService(NotificationService);

                NotificationView = new RemoteViews(PackageName, Resource.Layout.ViewProgressNotification);

                var resultIntent = new Intent();
                var resultPendingIntent = PendingIntent.GetActivity(this, 0, resultIntent, PendingIntentFlags.UpdateCurrent);
                NotificationBuilder = new NotificationCompat.Builder(this, NotificationChannelId);
                NotificationBuilder.SetSmallIcon(Resource.Mipmap.icon);
                NotificationBuilder.SetColor(ContextCompat.GetColor(this, Resource.Color.accent));
                NotificationBuilder.SetCustomContentView(NotificationView)
                    .SetOngoing(true)
                    .SetContentIntent(resultPendingIntent)
                    .SetDefaults(NotificationCompat.DefaultAll)
                    .SetPriority((int)NotificationPriority.High);

                NotificationBuilder.SetVibrate(new[] { 0L });
                NotificationBuilder.SetVisibility(NotificationCompat.VisibilityPublic);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var importance = NotificationImportance.High;
                    var notificationChannel = new NotificationChannel(NotificationChannelId, AppSettings.ApplicationName, importance);
                    notificationChannel.EnableLights(false);
                    notificationChannel.EnableVibration(false);
                    NotificationBuilder.SetChannelId(NotificationChannelId);

                    MNotificationManager?.CreateNotificationChannel(notificationChannel);
                }

                MNotificationManager?.Notify(2020, NotificationBuilder.Build());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RemoveNotification()
        {
            try
            {
                MNotificationManager.CancelAll();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpdateNotification(string type)
        {
            try
            {
                switch (type)
                {
                    case "Post":
                        NotificationView.SetTextViewText(Resource.Id.title, GetString(Resource.String.Lbl_UploadingPost));
                        break;
                    case "Story":
                        NotificationView.SetTextViewText(Resource.Id.title, GetString(Resource.String.Lbl_UploadingStory));
                        break;
                }

                MNotificationManager?.Notify(2020, NotificationBuilder.Build());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion 
    }
     
    //public class ImageCompressionAsyncTask : AsyncTask<string, object, string>
    //{
    //    Context mContext; 
    //    public ImageCompressionAsyncTask(Context context)
    //    {
    //        mContext = context;
    //    }

    //    protected override string RunInBackground(params string[] paths)
    //    {
    //        string filePath = SiliCompressor.With(mContext).Compress(paths[0], new File(paths[1]));
    //        return filePath;
    //    }

    //    protected override void OnPostExecute(string result)
    //    {
    //        base.OnPostExecute(result);


    //        File imageFile = new File(result);
    //        var compressUri = Android.Net.Uri.FromFile(imageFile);

    //        try
    //        {
    //            var bitmap = MediaStore.Images.Media.GetBitmap(mContext.ContentResolver, compressUri);
                 
    //            string name = imageFile.Name;
    //            var length = imageFile.Length() / 1024f; // Size in KB
    //            var compressWidth = bitmap.Width;
    //            var compressHieght = bitmap.Height;

    //            Console.WriteLine("Name: " + name + " Size: " + length + " Width: " + compressWidth + " Height: " + compressHieght);
    //        }
    //        catch (IOException e)
    //        {
    //            Methods.DisplayReportResultTrack(e);
    //        }
    //    }
    //}

    //public class VideoCompressAsyncTask : AsyncTask<string, string, string>
    //{
    //    Context mContext;
    //    public VideoCompressAsyncTask(Context context)
    //    {
    //        mContext = context;
    //    }

    //    protected override string RunInBackground(params string[] paths)
    //    {
    //        string filePath = null!;
    //        try
    //        {
    //            //This bellow is just a temporary solution to test that method call works
    //            var b = bool.Parse(paths[0]);
    //            if (b)
    //            {
    //                filePath = SiliCompressor.With(mContext).CompressVideo(paths[1], paths[2]);
    //            }
    //            else
    //            {
    //                Android.Net.Uri videoContentUri = Android.Net.Uri.Parse(paths[1]);
    //                // Example using the bitrate and video size parameters = >> filePath = SiliCompressor.with(mContext).compressVideo(videoContentUri, paths[2], 1280,720,1500000);*/
    //                filePath = SiliCompressor.With(mContext).CompressVideo(videoContentUri.ToString(), paths[2]);
    //            }
    //        }
    //        catch (URISyntaxException e)
    //        {
    //            Methods.DisplayReportResultTrack(e);
    //        }
    //        catch (Exception e)
    //        {
    //            Methods.DisplayReportResultTrack(e);
    //        }
    //        return filePath;
    //    }

    //    protected override void OnPostExecute(string compressedFilePath)
    //    {
    //        try
    //        {
    //            base.OnPostExecute(compressedFilePath);

    //            File imageFile = new File(compressedFilePath);
    //            float length = imageFile.Length() / 1024f; // Size in KB
    //            string value;
    //            if (length >= 1024)
    //                value = length / 1024f + " MB";
    //            else
    //                value = length + " KB";

    //            Console.WriteLine("Name: " + imageFile.Name + " Size: " + value);

    //            Console.WriteLine("Silicompressor Path: " + compressedFilePath);
    //        }
    //        catch (Exception e)
    //        {
    //            Methods.DisplayReportResultTrack(e);
    //        }
    //    }

    //}

}