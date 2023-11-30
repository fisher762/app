using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.UI;
using PixelPhoto.Activities.Posts.Extras;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.MediaPlayers;

namespace PixelPhoto.Activities.Posts.page
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", LaunchMode = LaunchMode.SingleInstance,ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Locale |ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout |ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
    public class FullScreenVideoActivity : AppCompatActivity
    {

        public static PlayerView FullscreenPlayerView;
        private PlayerControlView ControlView;
        public static ImageView MFullScreenIcon;
        public PlayerEvents PlayerListener;
        private FrameLayout MFullScreenButton;
         
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme( AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base );

                // Create your application here
                //Set Full screen 
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

                Window.AddFlags(WindowManagerFlags.Fullscreen);

                //newUiOptions |= (int)SystemUiFlags.LowProfile;
                //newUiOptions |= (int)SystemUiFlags.Immersive;

                //ScreenOrientation.Portrait >>  Make to run your application only in portrait mode
                //ScreenOrientation.Landscape >> Make to run your application only in LANDSCAPE mode 
                //RequestedOrientation = ScreenOrientation.Landscape;

                SetContentView(Resource.Layout.FullScreenDialog_Layout);
                FullscreenPlayerView = FindViewById<PlayerView>(Resource.Id.player_view2);

                ControlView = FullscreenPlayerView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);
                 
                MFullScreenIcon = ControlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                MFullScreenButton = ControlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);

                MFullScreenButton.Click += MFullScreenButtonOnClick;
                PRecyclerView.GetInstance().PlayFullScreen();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MFullScreenButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                ReleasePlayer(); 
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
                GC.Collect(0);
                base.OnResume();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public override void OnBackPressed()
        {
            try
            {
                ReleasePlayer(); 
                base.OnBackPressed();
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


        public void StopVideo()
        {
            try
            {
                if (FullscreenPlayerView.Player == null) return;
                if (FullscreenPlayerView.Player.PlaybackState == IPlayer.StateEnded)
                {
                    FullscreenPlayerView.Player.PlayWhenReady = false;
                    FullscreenPlayerView.Player.Stop();
                    HomeActivity.GetInstance()?.SetOffWakeLock();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ReleasePlayer()
        {
            try
            {
                StopVideo();
                 
                FullscreenPlayerView = null!;
                MFullScreenIcon = null!;

                GC.Collect();
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}