using System;
using Android.App;
using Android.Widget;
using Com.Google.Android.Youtube.Player;
using PixelPhoto.Helpers.Utils; 

namespace PixelPhoto.Helpers.Controller
{
    public class YoutubePlayerController : Java.Lang.Object, IYouTubePlayerOnInitializedListener
    {
        public IYouTubePlayer YoutubePlayer;
        private readonly string VideoId;
        private readonly Activity ActivityContext;

        public YoutubePlayerController(Activity activityContext, string videoId)
        {
            try
            {
                ActivityContext = activityContext;
                VideoId = videoId;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OnInitializationFailure(IYouTubePlayerProvider p0, YouTubeInitializationResult errorReason)
        {
            try
            {
                if (errorReason.IsUserRecoverableError)
                    errorReason.GetErrorDialog(ActivityContext, 1).Show();
                else
                    Toast.MakeText(ActivityContext, errorReason.ToString(), ToastLength.Short)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OnInitializationSuccess(IYouTubePlayerProvider p0, IYouTubePlayer player, bool p2)
        {
            try
            {
                YoutubePlayer = player;
                YoutubePlayer.SetPlayerStyle(YouTubePlayerPlayerStyle.Default);
                YoutubePlayer.LoadVideo(VideoId);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}