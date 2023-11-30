//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PixelPhoto 31/Feb/2020 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

//For the accuracy of the icon and logo, please use this website " https://appicon.co/ " and add images according to size in folders " mipmap " 
// https://edit.lottiefiles.com/?src=https%3A%2F%2Fassets3.lottiefiles.com%2Fpackages%2Flf20_I8GZgX.json

using PixelPhoto.Helpers.Utils;

namespace PixelPhoto
{ 
    internal static class AppSettings
    {
        //Main Settings >>>>>
        //*********************************************************
        public static string TripleDesAppServiceProvider = "8wNUQ/ugPV9CroRq8MFXLsusZcZ/27TgTM1rpndn0mRM6M4Csv9a7E2x4R47UDEyG3GuV8PfvgHQ1yiJM+RLGGPsortNFHQ/K/8g2hbJHQJAsMhZCS/7roVHOjECmrGdIjjGzrnSwffMNoPu1ao2cSl5ZbWsSSQz6EDIV+FPv3ppPmuefsYCxl0YDb5MBswQMHdj12zSWvO+c1bir7GbsvURFSEeHOFja5D4zcVEvnY=";

        public static string Version = "2.2";
        public static string ApplicationName = "PixelPhoto";
        public static string DatabaseName = "PixelPhotoSocial";
        public static bool SetTheme2 = true;
         
        //Main Colors >>
        //*********************************************************
        public static string MainColor = "#73348D";
        public static string StartColor = MainColor;
        public static string EndColor = "#D83880";

        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string Lang = ""; //Default language ar_AE

        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "e06a3585-d0ac-44ef-b2df-0c24abc23682";  
         
        //********************************************************* 
        public static bool ImageCropping = true;

        public static bool SetApisReportMode = false;

        //allow download or not when share
        public static bool AllowDownloadMedia = true; 

        //Set Theme Welcome Pages 
        //*********************************************************
        public static bool DisplayImageOnRegisterBackground = false;
        public static bool DisplayImageOnForgetPasswordBackground = false;
        public static bool DisplayImageOnLoginBackground = false;

        public static bool ShowFirstPage = true; 

        //*********************************************************

        //AdMob >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowAdMobBanner = true; 
        public static bool ShowAdMobInterstitial = true; 
        public static bool ShowAdMobRewardVideo = true; 
        public static bool ShowAdMobNative = true; 
        public static bool ShowAdMobNativePost = true;
        public static bool ShowAdMobAppOpen = true; //#New
        public static bool ShowAdMobRewardedInterstitial = true; //#New

        public static string AdInterstitialKey = "ca-app-pub-5135691635931982/3468020887"; 
        public static string AdRewardVideoKey = "ca-app-pub-5135691635931982/6449407444";
        public static string AdAdMobNativeKey = "ca-app-pub-5135691635931982/3440100725";
        public static string AdAdMobAppOpenKey = "ca-app-pub-5135691635931982/3509722172"; //#New
        public static string AdRewardedInterstitialKey = "ca-app-pub-5135691635931982/8005153346"; //#New

        //Three times after entering the ad is displayed
        public static int ShowAdMobInterstitialCount = 3; 
        public static int ShowAdMobRewardedVideoCount = 3; 
        public static int ShowAdMobNativeCount = 20;
        public static int ShowAdMobAppOpenCount = 3; //#New
        public static int ShowAdMobRewardedInterstitialCount = 3; //#New

        //FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowFbBannerAds = false; 
        public static bool ShowFbInterstitialAds = false; 
        public static bool ShowFbRewardVideoAds = false; 
        public static bool ShowFbNativeAds = false; 

        //YOUR_PLACEMENT_ID
        public static string AdsFbBannerKey = "250485588986218_554026418632132"; 
        public static string AdsFbInterstitialKey = "250485588986218_554026125298828"; 
        public static string AdsFbRewardVideoKey = "250485588986218_554072818627492"; 
        public static string AdsFbNativeKey = "250485588986218_554706301897477"; 

        //Three times after entering the ad is displayed
        public static int ShowFbNativeAdsCount = 25; 

        //Set Theme Full Screen App
        //*********************************************************
        public static bool EnableFullScreenApp = false;

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file 
        //Facebook >> ../values/analytic.xml .. line 10-11 
        //Google >> ../values/analytic.xml .. line 15 
        //*********************************************************
        public static bool ShowFacebookLogin = true;
        public static bool ShowGoogleLogin = true;

        public static readonly string ClientId = "428358750506-5p97f2vp91pn52oculdc4kck72hl973f.apps.googleusercontent.com";

        //########################### 

        //Last_Messages Page >>
        //********************************************************* 
        public static bool RunSoundControl = true;
        public static int RefreshChatActivitiesSeconds = 6000; // 6 Seconds
        public static int MessageRequestSpeed = 3000; // 3 Seconds
        public static int AvatarPostSize = 60;
        public static int ImagePostSize = 300;
        public static int SetPostRowHorizontalCount = 3;
        //Add Post
        public static bool ShowGalleryImage = true;
        public static bool ShowGalleryVideo = true;
        public static bool ShowMention = true;
        public static bool ShowCamera = true;
        public static bool ShowGif = true;
        public static bool ShowEmbedVideo = true;

        public static bool ShowStore = true;   //#New

        public static bool ShowFunding = true;  
        public static int ShowFundingCount = 5; 
        public static bool ShowFullScreenVideoPost = true;
         
        public static readonly long DoubleClickQualificationSpanInMillis = 500L; 

        //Set a story duration >> 10 Sec
        public static long StoryDuration = 10000L;

        //Profile Page >>
        //*********************************************************  
        public static ProfileTheme ProfileTheme = ProfileTheme.DefaultTheme;
        public static bool ShowEmailAccount = false;
         
        //Settings Page >> General Account 
        public static bool ShowSettingsGeneralAccount = true;
        public static bool ShowSettingsAccountPrivacy = true;
        public static bool ShowSettingsPassword = true;
        public static bool ShowSettingsBlockedUsers = true;
        public static bool ShowSettingsNotifications = true;
        public static bool ShowSettingsDeleteAccount = true;
        public static bool ShowSettingsWithdrawals = true;  
        public static bool ShowSettingsMyAffiliates = true;  
        public static bool ShowSettingsManageSessions = true;  
        public static bool ShowSettingsBusinessAccount = true;  
        public static bool ShowSettingsVerification = true;

        public static bool ShowSettingsRateApp = true;  
        public static int ShowRateAppCount = 5; 

        public static bool ShowSettingsUpdateManagerApp = false; 

        //Set Theme Tab
        //*********************************************************
        public static bool SetTabDarkTheme = false; 

        //Bypass Web Errors  
        //*********************************************************
        public static bool TurnTrustFailureOnWebException = true;
        public static bool TurnSecurityProtocolType3072On = true;

        //Show custom error reporting page
        public static bool RenderPriorityFastPostLoad = true;

        //*********************************************************
        public static bool StartYoutubeAsIntent { get; set; }

        //Currency
        public static readonly string CurrencyIconStatic = "$"; 
        public static readonly string CurrencyCodeStatic = "USD"; 
        public static readonly string CurrencyFundingPriceStatic = "$"; 

        //The minimum payment amount
        public static readonly int WithdrawalAmount = 50; 
         
        public static bool ShowPaypal = true; 
        public static bool ShowBankTransfer = true; 
        public static bool ShowCreditCard = true; 

        public static bool AutoPlayVideo = true; 
        //*********************************************************   
    }
} 
