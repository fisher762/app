using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget; 
using PixelPhoto.Adapters;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AndroidX.AppCompat.App;
using AndroidX.ViewPager.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.OneSignal;
using Me.Relex.CircleIndicatorLib;

namespace PixelPhoto.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class FirstActivity : AppCompatActivity
    {
        #region Variables Basic

        private ImageView Bagroundimage;
        private ViewPager ViewPagerView;
        private ViewPagerStringAdapter ViewPagerStringAdapter;
        private Button LoginButton;
        private Button RegisterButton;
        private CircleIndicator CircleIndicatorView;
        private TextView WelecomTextView;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                // Create your application here
                SetContentView(Resource.Layout.FirstLayout);
                //Get Value 
                InitComponent();

                //Check and Get Settings
                GetSettingsSite();

                //OneSignal Notification  
                //====================================== 
                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.RegisterNotificationDevice();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    // Check Created My Folder Or Not 
                    Methods.Path.Chack_MyFolder();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        // Check Created My Folder Or Not 
                        Methods.Path.Chack_MyFolder();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(100);

                    }
                }
                AddOrRemoveEvent(true);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnPause()
        {
            try
            {
                AddOrRemoveEvent(false);
                base.OnPause();
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

        #region Functions

        private void InitComponent()
        {
            try
            {
                Bagroundimage = FindViewById<ImageView>(Resource.Id.bagroundimage);
                ViewPagerView = FindViewById<ViewPager>(Resource.Id.viewPager);
                CircleIndicatorView = FindViewById<CircleIndicator>(Resource.Id.indicator);
                LoginButton = FindViewById<Button>(Resource.Id.LoginButton);
                RegisterButton = FindViewById<Button>(Resource.Id.RegisterButton);
                WelecomTextView = FindViewById<TextView>(Resource.Id.tv_signin_wtnione);

                WelecomTextView.Text = GetString(Resource.String.Lbl_Welcome_to) + " " + AppSettings.ApplicationName;

                var stringsList = new List<Classes.ViewPagerStrings>
                {
                    new Classes.ViewPagerStrings { Description = GetString(Resource.String.Lbl_FirstDescription1), Header = GetString(Resource.String.Lbl_FirstHeader1) },
                    new Classes.ViewPagerStrings { Description = GetString(Resource.String.Lbl_FirstDescription2), Header = GetString(Resource.String.Lbl_FirstHeader2) },
                    new Classes.ViewPagerStrings { Description = GetString(Resource.String.Lbl_FirstDescription3), Header = GetString(Resource.String.Lbl_FirstHeader3) }
                };

                ViewPagerStringAdapter = new ViewPagerStringAdapter(this, stringsList);
                ViewPagerView.Adapter = ViewPagerStringAdapter;

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    var @params = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent) { Height = 250 };//left, top, right, bottom
                    ViewPagerView.LayoutParameters = @params;
                }

                CircleIndicatorView.SetViewPager(ViewPagerView);

                var glideRequestOptions = new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All);

                 Glide.With(this).AsBitmap().Load(Resource.Drawable.FirstBackground).Apply(glideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100)).Into(Bagroundimage);
                
                //GlideImageLoader.LoadImage(this, AppSettings.URlImageOnFirstBackground, Bagroundimage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
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
                    RegisterButton.Click += RegisterButton_Click;
                    LoginButton.Click += LoginButton_Click;
                }
                else
                {
                    RegisterButton.Click -= RegisterButton_Click;
                    LoginButton.Click -= LoginButton_Click;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetSettingsSite()
        {
            try
            {
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Event Click open Register Activity
        private void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(RegisterActivity)));
                //Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Click open Login Activity
        private void LoginButton_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(LoginActivity)));
               // Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions 

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 100)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        // Check Created My Folder Or Not 
                        Methods.Path.Chack_MyFolder();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long)?.Show();
                        FinishAffinity();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}