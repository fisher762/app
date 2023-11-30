using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using PixelPhoto.Activities.AddPost.Service;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;

namespace PixelPhoto.Activities.SettingsUser
{
    public static class MainSettings
    {
        public static ISharedPreferences SharedData, SharedNightMode, InAppReview;

        public static readonly string PrefKeyInAppReview = "In_App_Review";

        public static readonly string LightMode = "light";
        public static readonly string DarkMode = "dark";
        public static readonly string DefaultMode = "default";

        public static  void Init()
        {
            try
            {
                SharedData = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                InAppReview = Application.Context.GetSharedPreferences("In_App_Review", FileCreationMode.Private);
                SharedNightMode = Application.Context.GetSharedPreferences("Night_Mode_key", FileCreationMode.Private);

                UserDetails.SoundControl = SharedData.GetBoolean("checkBox_PlaySound_key", AppSettings.RunSoundControl);
                 
                var getValue = SharedNightMode.GetString("Night_Mode_key", string.Empty);
                ApplyTheme(getValue);

                PostService.ActionPost = Application.Context.PackageName + ".action.ACTION_POST";
                PostService.ActionStory = Application.Context.PackageName + ".action.ACTION_STORY";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private static void ApplyTheme(string themePref)
        {
            try
            {
                if (themePref == LightMode)
                {
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                    AppSettings.SetTabDarkTheme = false;
                }
                else if (themePref == DarkMode)
                {
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                    AppSettings.SetTabDarkTheme = true;
                }
                else if (themePref == DefaultMode)
                {
                    AppCompatDelegate.DefaultNightMode = (int)Build.VERSION.SdkInt >= 29 ? AppCompatDelegate.ModeNightFollowSystem : AppCompatDelegate.ModeNightAutoBattery;

                    var currentNightMode = Application.Context.Resources?.Configuration?.UiMode & UiMode.NightMask;
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
                }
                else
                {
                    if (AppSettings.SetTabDarkTheme) return;

                    var currentNightMode = Application.Context.Resources?.Configuration?.UiMode & UiMode.NightMask;
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
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}