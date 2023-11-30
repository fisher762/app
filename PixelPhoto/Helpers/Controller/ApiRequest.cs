using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Auth.Api;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.AddPost;
using PixelPhoto.Activities.AddPost.Service;
using PixelPhoto.Activities.Chat.Services;
using PixelPhoto.Activities.Default;
using PixelPhoto.Activities.SettingsUser;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.OneSignal;
using PixelPhoto.SQLite;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.RestCalls;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Exception = System.Exception;
using File = Java.IO.File;

namespace PixelPhoto.Helpers.Controller
{
    internal static class ApiRequest
    {
        internal static readonly string ApiGetSearchGif = "https://api.giphy.com/v1/gifs/search?api_key=b9427ca5441b4f599efa901f195c9f58&limit=45&rating=g&q=";
        internal static readonly string ApiGeTrendingGif = "https://api.giphy.com/v1/gifs/trending?api_key=b9427ca5441b4f599efa901f195c9f58&limit=45&rating=g";

        public static async Task GetSettings_Api(Activity context)
        {
            if (Methods.CheckConnectivity())
            {
                (var apiStatus, var respond) = await Current.GetSettings();
                if (apiStatus == 200)
                {
                    if (respond is GetSettingsObject result)
                    {
                        if (result.Data != null)
                        {
                            ListUtils.SettingsSiteList = result.Data;

                            AppSettings.OneSignalAppId = result.Data.PushId;
                            OneSignalNotification.RegisterNotificationDevice();

                            var dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrReplaceSettingsAsync(result.Data);
                        }
                    }
                }
                //else Methods.DisplayReportResult(context, respond);
            }
        }

        public static async Task<(int, int)> GetCountNotifications()
        {
            var (respondCode, respondString) = await RequestsAsync.User.FetchNotifications("0", "15").ConfigureAwait(false);
            if (respondCode.Equals(200))
            {
                if (respondString is FetchNotificationsObject fetch)
                {
                    return (fetch.NewNotifications , fetch.NewMessages);
                }
            }
            return (0, 0);
        }
         
        public static async Task GetProfile_Api(Activity context)
        {
            if (Methods.CheckConnectivity())
            {
                (var apiStatus, var respond) = await RequestsAsync.User.FetchUserData(UserDetails.UserId);
                if (apiStatus == 200)
                {
                    if (respond is FetchUserDataObject result)
                    {
                        if (result.Data != null)
                        {
                            context?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    var dbDatabase = new SqLiteDatabase();
                                    dbDatabase.InsertOrUpdateToMyProfileTable(result.Data);

                                    var dataStory = HomeActivity.GetInstance().NewsFeedFragment.StoryAdapter?.StoryList?.FirstOrDefault(a => a.Type == "Your");
                                    if (dataStory != null)
                                    {
                                        dataStory.Avatar = result.Data.Avatar;
                                        HomeActivity.GetInstance().NewsFeedFragment.StoryAdapter.NotifyItemChanged(0);
                                    }
                                    Glide.With(context).Load(UserDetails.Avatar).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CircleCrop()).Preload();

                                    var profileImage = HomeActivity.GetInstance()?.FragmentBottomNavigator?.ProfileImage;
                                    if (profileImage != null)
                                        GlideImageLoader.LoadImage(context, UserDetails.Avatar, profileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            }); 
                        }
                    }
                }
                //else Methods.DisplayReportResult(context, respond);
            }
        }

        public static async Task<ObservableCollection<GifGiphyClass.Datum>> SearchGif(string searchKey, string offset)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return null!;
                }
                else
                {
                    var response = await RestHttp.Client.GetAsync(ApiGetSearchGif + searchKey + "&offset=" + offset);
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<GifGiphyClass>(json);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return data.meta.Status == 200 ? new ObservableCollection<GifGiphyClass.Datum>(data.Data) : null!;
                    }
                    else
                    {
                        return null!;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public static async Task<ObservableCollection<GifGiphyClass.Datum>> TrendingGif(string offset)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return null!;
                }
                else
                {
                    var response = await RestHttp.Client.GetAsync(ApiGeTrendingGif + "&offset=" + offset);
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<GifGiphyClass>(json);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return data.meta.Status == 200 ? new ObservableCollection<GifGiphyClass.Datum>(data.Data) : null!;
                    }
                    else
                    {
                        return null!;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }


        /////////////////////////////////////////////////////////////////
        private static bool RunLogout;

        public static async void Delete(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    RunLogout = true;

                    await RemoveData("Delete");

                    context?.RunOnUiThread(() =>
                    {
                        try
                        {
                            Methods.Path.DeleteAll_MyFolderDisk();

                            var dbDatabase = new SqLiteDatabase(); 
                            dbDatabase.DropAll();

                            Runtime.GetRuntime()?.RunFinalization();
                            Runtime.GetRuntime()?.Gc();
                            TrimCache(context);
                             
                            ListUtils.ClearAllList();

                            UserDetails.ClearAllValueUserDetails();

                            dbDatabase.CheckTablesStatus();

                            context.StopService(new Intent(context, typeof(PostService)));
                            context.StopService(new Intent(context, typeof(ChatApiService)));

                            MainSettings.SharedData.Edit()?.Clear()?.Commit();
                            MainSettings.InAppReview.Edit()?.Clear()?.Commit();

                            var intent = new Intent(context, typeof(FirstActivity));
                            intent.AddCategory(Intent.CategoryHome);
                            intent.SetAction(Intent.ActionMain);
                            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                            context.StartActivity(intent);
                            context.FinishAffinity();
                            context.Finish();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });

                    RunLogout = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async void Logout(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    RunLogout = true;

                    await RemoveData("Logout");

                    context?.RunOnUiThread(() =>
                    {
                        try
                        {
                            Methods.Path.DeleteAll_MyFolderDisk();

                            var dbDatabase = new SqLiteDatabase(); 
                            dbDatabase.DropAll();

                            Runtime.GetRuntime()?.RunFinalization();
                            Runtime.GetRuntime()?.Gc();
                            TrimCache(context);
                               
                            ListUtils.ClearAllList();

                            UserDetails.ClearAllValueUserDetails();

                            dbDatabase.CheckTablesStatus();

                            context.StopService(new Intent(context, typeof(PostService)));
                            context.StopService(new Intent(context, typeof(ChatApiService)));

                            MainSettings.SharedData.Edit()?.Clear()?.Commit();
                            MainSettings.InAppReview.Edit()?.Clear()?.Commit();

                            var intent = new Intent(context, typeof(FirstActivity));
                            intent.AddCategory(Intent.CategoryHome);
                            intent.SetAction(Intent.ActionMain);
                            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                            context.StartActivity(intent);
                            context.FinishAffinity();
                            context.Finish();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });

                    RunLogout = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static void TrimCache(Activity context)
        {
            try
            {
                var dir = context?.CacheDir;
                if (dir != null && dir.IsDirectory)
                {
                    DeleteDir(dir);
                }

                context?.DeleteDatabase(AppSettings.DatabaseName + "_.db");
                context?.DeleteDatabase(SqLiteDatabase.PathCombine);

                if (context?.IsDestroyed != false)
                    return;

                Glide.Get(context)?.ClearMemory();
                new Thread(() =>
                {
                    try
                    {
                        Glide.Get(context)?.ClearDiskCache();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }).Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static bool DeleteDir(File dir)
        {
            try
            {
                if (dir == null || !dir.IsDirectory) return dir != null && dir.Delete();
                var children = dir.List();
                if (children.Select(child => DeleteDir(new File(dir, child))).Any(success => !success))
                {
                    return false;
                }

                // The directory is now empty so delete it
                return dir.Delete();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        private static void Reset()
        {
            try
            {
                MentionActivity.MAdapter = null!;

                Current.AccessToken = string.Empty;

                HomeActivity.GetInstance()?.NewsFeedFragment?.RemoveHandler();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static async Task RemoveData(string type)
        {
            try
            {
                if (type == "Logout")
                {
                    if (Methods.CheckConnectivity())
                    {
                        await RequestsAsync.Auth.Logout();
                    }
                }
                else if (type == "Delete")
                {
                    Methods.Path.DeleteAll_FolderUser();

                    if (Methods.CheckConnectivity())
                    {
                        await RequestsAsync.Auth.DeleteAccount(UserDetails.Password);
                    }
                }

                if (AppSettings.ShowGoogleLogin && LoginActivity.MGoogleSignInClient != null)
                    if (Auth.GoogleSignInApi != null)
                    {
                        LoginActivity.MGoogleSignInClient.SignOut();
                        LoginActivity.MGoogleSignInClient = null!;
                    }

                if (AppSettings.ShowFacebookLogin)
                {
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }
                }

                OneSignalNotification.UnRegisterNotificationDevice();

                ListUtils.ClearAllList();
                Reset();

                UserDetails.ClearAllValueUserDetails();

                Methods.DeleteNoteOnSD();

                GC.Collect();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}