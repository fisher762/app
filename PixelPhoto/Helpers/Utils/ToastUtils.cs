using System;
using Android.App;
using Android.Views;
using Android.Widget;

namespace PixelPhoto.Helpers.Utils
{
    public static class ToastUtils
    {
        public static void ShowToast(string text, ToastLength time, GravityFlags style = GravityFlags.Bottom)
        {
            try
            {
                var toast = Toast.MakeText(Application.Context, text, time);
                toast.SetGravity(style, 0, 0);
                toast.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}