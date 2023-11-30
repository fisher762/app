using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Messages;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;

namespace PixelPhoto.Activities.Chat.Services
{
    [Service(Exported = false)]
    public class ChatApiService : JobIntentService 
    {
        private static Handler MainHandler;
        private ResultReceiver ResultSender;
        private PostUpdaterHelper PostUpdater;

        public override IBinder OnBind(Intent intent)
        {
            return null!;
        }

        protected override void OnHandleWork(Intent p0)
        {
             
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                PostUpdater = new PostUpdaterHelper(new Handler(Looper.MainLooper), ResultSender);

                MainHandler ??= new Handler(Looper.MainLooper);
                MainHandler.PostDelayed(PostUpdater, AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);
            try
            {
                var rec = intent.GetParcelableExtra("receiverTag");
                ResultSender = (ResultReceiver)rec;
                if (PostUpdater != null)
                    PostUpdater.ResultSender = ResultSender;
                else
                    MainHandler.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

            //MainHandler.PostDelayed(new PostUpdaterHelper(Application.Context, new Handler(), ResultSender), AppSettings.RefreshChatActivitiesSeconds);

            return StartCommandResult.Sticky;
        }
    }

    public class PostUpdaterHelper : Java.Lang.Object, IRunnable
    {
        private static Handler MainHandler;
        public ResultReceiver ResultSender;

        public PostUpdaterHelper(Handler mainHandler, ResultReceiver resultSender)
        {
            MainHandler = mainHandler;
            ResultSender = resultSender;
        }

        public async void Run()
        {
            try
            { 
                if (string.IsNullOrEmpty(Methods.AppLifecycleObserver.AppState))
                    Methods.AppLifecycleObserver.AppState = "Background";

                //Toast.MakeText(Application.Context, "Started", ToastLength.Short)?.Show(); 
                if (Methods.AppLifecycleObserver.AppState == "Background")
                {
                    try
                    {
                        if (string.IsNullOrEmpty(Client.WebsiteUrl))
                        {
                            var a = new Client(AppSettings.TripleDesAppServiceProvider);
                            Console.WriteLine(a);
                        }

                        var dbDatabase = new SqLiteDatabase();

                        if (string.IsNullOrEmpty(Current.AccessToken))
                        {
                            var login = dbDatabase.Get_data_Login_Credentials();
                            Console.WriteLine(login);
                        }

                        if (string.IsNullOrEmpty(Current.AccessToken))
                            return;

                        (var apiStatus, var respond) = await RequestsAsync.Messages.GetChats("30", "0"); 
                        if (apiStatus != 200 || !(respond is GetChatsObject result))
                        {
                            // Methods.DisplayReportResult(Activity, respond);
                        }
                        else
                        {
                            //Toast.MakeText(Application.Context, "ResultSender 1 \n" + data, ToastLength.Short)?.Show();
                             
                            if (result.Data.Count > 0)
                            {
                                ListUtils.ChatList = new ObservableCollection<ChatDataObject>(result.Data);
                                //Insert All data users to database
                                dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                        // Toast.MakeText(Application.Context, "Exception  " + e, ToastLength.Short)?.Show();
                    }
                }
                else
                {
                    (var apiStatus, var respond) = await RequestsAsync.Messages.GetChats("30", "0");
                    if (apiStatus != 200 || !(respond is GetChatsObject result))
                    {
                       // Methods.DisplayReportResult(Activity, respond);
                    }
                    else
                    {
                        var b = new Bundle();
                        b.PutString("Json", JsonConvert.SerializeObject(result));
                        ResultSender?.Send(0, b);

                        //Toast.MakeText(Application.Context, "ResultSender 2 \n" + data, ToastLength.Short)?.Show();

                        Console.WriteLine("Allen Post + started");
                    }
                }

                MainHandler.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                //Toast.MakeText(Application.Context, "ResultSender failed", ToastLength.Short)?.Show();
                MainHandler.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
} 