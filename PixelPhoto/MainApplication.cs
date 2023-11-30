using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using AndroidX.Lifecycle;
using Firebase;
using Java.Lang;
using Plugin.CurrentActivity;
using PixelPhoto.Activities;
using PixelPhoto.Activities.SettingsUser;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.OneSignal;
using PixelPhoto.SQLite;
using PixelPhotoClient;
using Xamarin.Android.Net; 
using Exception = System.Exception;
using Console = System.Console;
 
namespace PixelPhoto
{
    //You can specify additional application information in this attribute
    [Application(UsesCleartextTraffic = true)]
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
        public static MainApplication Instance;
        public Activity Activity;

        public MainApplication(IntPtr handle, JniHandleOwnership transer) : base(handle, transer)
        {
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();

                Instance = this;

                RegisterActivityLifecycleCallbacks(this);
                //A great place to initialize Xamarin.Insights and Dependency Services!

                var a = new Client(AppSettings.TripleDesAppServiceProvider);
                Console.WriteLine(a);

                var sqLiteDatabase = new SqLiteDatabase();
                sqLiteDatabase.CheckTablesStatus();
                sqLiteDatabase.Get_data_Login_Credentials();

                new Handler(Looper.MainLooper).PostDelayed(new Runnable(FirstRunExcite), 100); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FirstRunExcite()
        {
            try
            {
                //AppCenter.Start("9f8cf987-8bfc-4895-a18b-bd4a86ef9ae3", typeof(Analytics), typeof(Crashes));

                AdsGoogle.InitializeAdsGoogle.Initialize(this);

                if (AppSettings.ShowFbBannerAds || AppSettings.ShowFbInterstitialAds || AppSettings.ShowFbRewardVideoAds)
                    InitializeFacebook.Initialize(this);


                //Bypass Web Errors 
                //======================================
                if (AppSettings.TurnSecurityProtocolType3072On)
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    var client = new HttpClient(new AndroidClientHandler());
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls13;
                    Console.WriteLine(client);
                }
                 
                if (AppSettings.TurnTrustFailureOnWebException)
                {
                    //If you are Getting this error >>> System.Net.WebException: Error: TrustFailure /// then Set it to true
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    var b = new AesCryptoServiceProvider();
                    Console.WriteLine(b);
                }

                //OneSignal Notification  
                //======================================
                OneSignalNotification.RegisterNotificationDevice();

                //Init Settings
                MainSettings.Init();
                 
                //App restarted after crash
                AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentOnUnhandledExceptionRaiser;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
                TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

                AppCompatDelegate.CompatVectorFromResourcesEnabled = true;
                FirebaseApp.InitializeApp(this);

                var appLifecycleObserver = new Methods.AppLifecycleObserver();
                ProcessLifecycleOwner.Get().Lifecycle.AddObserver(appLifecycleObserver);  
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public static MainApplication GetPhotoApp()
        {
            return Instance;
        }

        public Context GetContext()
        {
            return Instance.GetContext();
        }

        private void AndroidEnvironmentOnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            try
            {
                var intent = new Intent(Activity, typeof(SplashScreenActivity));
                intent.AddCategory(Intent.CategoryHome);
                intent.PutExtra("crash", true);
                intent.SetAction(Intent.ActionMain);
                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);

                var pendingIntent = PendingIntent.GetActivity(GetInstance().BaseContext, 0, intent, PendingIntentFlags.OneShot);
                var mgr = (AlarmManager)GetInstance().BaseContext.GetSystemService(AlarmService);
                mgr.Set(AlarmType.Rtc, JavaSystem.CurrentTimeMillis() + 100, pendingIntent);

                Activity.Finish();
                JavaSystem.Exit(2);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                var message = e.Exception.Message;
                var stackTrace = e.Exception.StackTrace;

                Methods.DisplayReportResult(Activity, "Error Exception in MainApplication 1 \n" + stackTrace);
                Methods.DisplayReportResultTrack(e.Exception);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var message = e;
                Methods.DisplayReportResult(Activity, "Error Exception in MainApplication 2 \n" +  e);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public static MainApplication GetInstance()
        {
            return Instance;
        }


        public override void OnTerminate() // on stop
        {
            try
            {
                base.OnTerminate();
                UnregisterActivityLifecycleCallbacks(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            try
            {
                Activity = activity;
                CrossCurrentActivity.Current.Activity = activity;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivityDestroyed(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPaused(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityResumed(Activity activity)
        {
            try
            {
                Activity = activity;
                CrossCurrentActivity.Current.Activity = activity;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
            Activity = activity;
        }

        public void OnActivityStarted(Activity activity)
        {
            try
            {
                Activity = activity;
                CrossCurrentActivity.Current.Activity = activity;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivityStopped(Activity activity)
        {
            Activity = activity;
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
         
    }
}