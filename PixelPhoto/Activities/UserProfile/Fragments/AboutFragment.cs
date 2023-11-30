using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Luseen.Autolinklibrary;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.RestCalls;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace PixelPhoto.Activities.UserProfile.Fragments
{
    public class AboutFragment : Fragment
    {
        #region  Variables Basic
      
        private HomeActivity GlobalContext;
        private TikUserProfileFragment ContextProfile;
        private AutoLinkTextView TxtAbout;
        public TextView TxtGender,TxtEmail, TxtCountry , TxtWebsite;
        private TextView IconAbout, IconGender, IconEmail, IconCountry , IconWebsite;
        public Button WebsiteButton, SocialGoogle, SocialFacebook, SocialTwitter;
        private Button SocialLinkedIn;
        private AdView MAdView;
        public LinearLayout SocialLinksLinear , WebsiteLinearLayout;

        public TextSanitizer TextSanitizerAutoLink;
        public string LinkedIn, Twitter, Facebook, Google, Website;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.AboutUserLayout, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                ContextProfile = (TikUserProfileFragment)ParentFragment;
                GlobalContext = (HomeActivity)Activity ?? HomeActivity.GetInstance();

                InitComponent(view);

                WebsiteButton.Click += WebsiteButtonOnClick;
                SocialFacebook.Click += BtnFacebookOnClick;
                SocialTwitter.Click += BtnTwitterOnClick;
                SocialGoogle.Click += BtnGoogleOnClick;
                //Website link click 
                TxtWebsite.Click += WebsiteButtonOnClick;

                ContextProfile.LoadProfile(ContextProfile.Json, ContextProfile.Type);
                StartApiService();
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                IconAbout = (TextView)view.FindViewById(Resource.Id.About_icon);
                TxtAbout = (AutoLinkTextView)view.FindViewById(Resource.Id.About_text);

                TextSanitizerAutoLink = new TextSanitizer(TxtAbout, Activity);
                  
                IconGender = (TextView)view.FindViewById(Resource.Id.gender_icon);
                TxtGender = (TextView)view.FindViewById(Resource.Id.gender_text);

                IconEmail = (TextView)view.FindViewById(Resource.Id.email_icon);
                TxtEmail = (TextView)view.FindViewById(Resource.Id.email_text);

                IconCountry = (TextView)view.FindViewById(Resource.Id.Country_icon);
                TxtCountry = (TextView)view.FindViewById(Resource.Id.Country_text);

                WebsiteLinearLayout = (LinearLayout)view.FindViewById(Resource.Id.websiteLinear);
                IconWebsite = (TextView)view.FindViewById(Resource.Id.website_icon);
                TxtWebsite = (TextView)view.FindViewById(Resource.Id.website_text);

                SocialLinksLinear = (LinearLayout) view.FindViewById(Resource.Id.Social_Links_Linear);
                SocialLinksLinear.Visibility = ViewStates.Gone;

                SocialGoogle = view.FindViewById<Button>(Resource.Id.social1);
                SocialFacebook = view.FindViewById<Button>(Resource.Id.social2);
                SocialTwitter = view.FindViewById<Button>(Resource.Id.social3);
                SocialLinkedIn = view.FindViewById<Button>(Resource.Id.social4);
                WebsiteButton = view.FindViewById<Button>(Resource.Id.website);

                SocialGoogle.Visibility = ViewStates.Gone;
                SocialFacebook.Visibility = ViewStates.Gone;
                SocialTwitter.Visibility = ViewStates.Gone;
                SocialLinkedIn.Visibility = ViewStates.Gone;
                WebsiteButton.Visibility = ViewStates.Gone;

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconAbout, IonIconsFonts.Information);
                IconAbout.SetTextColor(Color.ParseColor(AppSettings.MainColor));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconGender, IonIconsFonts.Male);
                IconGender.SetTextColor(Color.ParseColor("#c51162"));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconEmail, IonIconsFonts.Mail);
                IconEmail.SetTextColor(Color.ParseColor("#dd2c00"));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconCountry, IonIconsFonts.Globe);
                IconCountry.SetTextColor(Color.ParseColor("#388E3C"));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconWebsite, IonIconsFonts.LogoChrome);
                IconWebsite.SetTextColor(Color.ParseColor("#0073E5"));

                if (!AppSettings.ShowEmailAccount)
                {
                    IconEmail.Visibility = ViewStates.Gone;
                    TxtEmail.Visibility = ViewStates.Gone;
                }

                MAdView = view.FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, null);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Events

        private void BtnGoogleOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                    new IntentController(Activity).OpenAppOnGooglePlay(Google);
                else
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnTwitterOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                    new IntentController(Activity).OpenTwitterIntent(Twitter);
                else
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnFacebookOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                    new IntentController(Activity).OpenFacebookIntent(Activity,Facebook);
                else
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void WebsiteButtonOnClick(object sender, EventArgs e)
        {
            if (Methods.CheckConnectivity())
                new IntentController(Activity).OpenBrowserFromPhone(Website);
            else
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
        }

        #endregion

        #region Load Data Api

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetUserProfileApi });
        }
      
        private async Task GetUserProfileApi()
        {
            if (Methods.CheckConnectivity())
            {
                (var respondCode, var respondString) = await RequestsAsync.User.FetchUserData(ContextProfile.UserId);
                if (respondCode == 200)
                {
                    if (respondString is FetchUserDataObject result)
                    {
                        if (result.Data != null)
                        {
                            ContextProfile.UserinfoData = result.Data;
                            ContextProfile.Url = result.Data.Url;
                            Activity?.RunOnUiThread(() => { ContextProfile.LoadUserData(result.Data); });
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity, respondString);
            }
        }

        #endregion

    }
}