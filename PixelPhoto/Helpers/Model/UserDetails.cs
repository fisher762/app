using System;
using Android.App;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient;

namespace PixelPhoto.Helpers.Model
{
    public static class UserDetails
    {
        public static string AccessToken = "";
        public static string UserId = "";
        public static string Username = "";
        public static string FullName = "";
        public static string Password = "";
        public static string Email = "";
        public static string Cookie = "";
        public static string Status = "";
        public static string Avatar = "";
        public static string Cover = "";
        public static string DeviceId = "";
        public static string Lang = "";
        public static string Lat = "";
        public static string Lng = "";
        public static string LangName = "";

        public static string StoreTitle = "";
        public static string StoreTags = "";
        public static string StoreCategory = "";
        public static string StoreLicenseType = "";
        public static string StorePriceMin = "";
        public static string StorePriceMax = "";

        public static bool SoundControl = true;
        public static long TimestampLastClick;


        public static bool NotificationPopup { get; set; } = true;
        
        public static string AndroidId = Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
         
        public static void ClearAllValueUserDetails()
        {
            try
            {
                AccessToken = string.Empty;
                UserId = string.Empty;
                Username = string.Empty;
                FullName = string.Empty;
                Password = string.Empty;
                Email = string.Empty;
                Cookie = string.Empty;
                Status = string.Empty;
                Avatar = string.Empty;
                Cover = string.Empty;
                DeviceId = string.Empty;
                Lang = string.Empty;
                Lat = string.Empty;
                Lng = string.Empty;
                LangName = string.Empty;
                TimestampLastClick = 0; 
                StoreTitle = string.Empty;
                StoreTags = string.Empty;
                StoreCategory = string.Empty;
                StoreLicenseType = string.Empty;
                StorePriceMin = string.Empty;
                StorePriceMax = string.Empty;

                Current.AccessToken = string.Empty;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}