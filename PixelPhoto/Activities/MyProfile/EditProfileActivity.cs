using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidHUD;
using PixelPhoto.Activities.Base;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.MyProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class EditProfileActivity : BaseActivity
    {
        #region Variables Basic

        private Toolbar Toolbar;
        private TextView SaveTextView;
        private EditText FirstNameEditText, LastNameEditText, AboutEditText, WebsiteEditText, FacebookEditText, GoogleEditText, TwitterEditText;
        private PublisherAdView PublisherAdView;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);  
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base); 

                // Create your application here
                SetContentView(Resource.Layout.EditProfileLayout);
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                LoadMyData();

                AdsGoogle.Ad_Interstitial(this);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
                PublisherAdView?.Resume();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                PublisherAdView?.Pause();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        protected override void OnDestroy()
        {
            try
            {
                PublisherAdView?.Destroy();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                SaveTextView = FindViewById<TextView>(Resource.Id.toolbar_title);
                FirstNameEditText = FindViewById<EditText>(Resource.Id.firstNameText);
                LastNameEditText = FindViewById<EditText>(Resource.Id.lasttNameText);
                AboutEditText = FindViewById<EditText>(Resource.Id.aboutText);
                WebsiteEditText = FindViewById<EditText>(Resource.Id.websiteText);
                FacebookEditText = FindViewById<EditText>(Resource.Id.facebookText);
                GoogleEditText = FindViewById<EditText>(Resource.Id.googleText);
                TwitterEditText = FindViewById<EditText>(Resource.Id.twitterText);
                SaveTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);


                SetColorEditText(FirstNameEditText);
                SetColorEditText(LastNameEditText);
                SetColorEditText(AboutEditText);
                SetColorEditText(WebsiteEditText);
                SetColorEditText(FacebookEditText);
                SetColorEditText(GoogleEditText);
                SetColorEditText(TwitterEditText);

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetColorEditText(EditText edit)
        {
            try
            {
                Methods.SetColorEditText(edit, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                {
                    Toolbar.Title = GetString(Resource.String.Lbl_EditProfile);
                    Toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    Toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);

                    SetSupportActionBar(Toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                }
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
                    SaveTextView.Click += SaveDataUserOnClick;
                }
                else
                {
                    SaveTextView.Click -= SaveDataUserOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events
         
        //Event Save data 
        private async void SaveDataUserOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var dictionary = new Dictionary<string, string>();

                    if (!string.IsNullOrEmpty(FirstNameEditText.Text))
                        dictionary.Add("fname", FirstNameEditText.Text);

                    if (!string.IsNullOrEmpty(LastNameEditText.Text))
                        dictionary.Add("lname", LastNameEditText.Text);

                    if (!string.IsNullOrEmpty(AboutEditText.Text))
                        dictionary.Add("about", AboutEditText.Text);

                    if (!string.IsNullOrEmpty(WebsiteEditText.Text))
                        dictionary.Add("website", WebsiteEditText.Text);

                    if (!string.IsNullOrEmpty(FacebookEditText.Text) && !FacebookEditText.Text.Contains("https://www.facebook.com/"))
                        dictionary.Add("facebook", "https://www.facebook.com/" + FacebookEditText.Text);
                    else if (!string.IsNullOrEmpty(FacebookEditText.Text))
                        dictionary.Add("facebook", FacebookEditText.Text);

                    if (!string.IsNullOrEmpty(GoogleEditText.Text) && !GoogleEditText.Text.Contains("https://plus.google.com/u/0/"))
                        dictionary.Add("google", "https://plus.google.com/u/0/" + GoogleEditText.Text);
                    else if (!string.IsNullOrEmpty(GoogleEditText.Text))
                        dictionary.Add("google", GoogleEditText.Text);
                     
                    if (!string.IsNullOrEmpty(TwitterEditText.Text) && !TwitterEditText.Text.Contains("https://twitter.com/"))
                        dictionary.Add("twitter", "https://twitter.com/" + TwitterEditText.Text);
                    else if (!string.IsNullOrEmpty(TwitterEditText.Text))
                        dictionary.Add("twitter", TwitterEditText.Text);
                     
                    if (dictionary.Count > 0)
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        //Send Api
                        (var respondCode, var respond) = await RequestsAsync.User.SaveSettings(dictionary);
                        if (respondCode == 200)
                        {
                            AndHUD.Shared.Dismiss(this);
                            Toast.MakeText(this, GetString(Resource.String.Lbl_SuccessfullyEdited), ToastLength.Short)?.Show();

                            var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                            if (dataUser != null)
                            {
                                dataUser.Fname = FirstNameEditText.Text;
                                dataUser.Lname = LastNameEditText.Text;
                                dataUser.About = AboutEditText.Text;
                                dataUser.Facebook = FacebookEditText.Text;
                                dataUser.Google = GoogleEditText.Text;
                                dataUser.Twitter = TwitterEditText.Text;
                                dataUser.Name = FirstNameEditText.Text + " " + LastNameEditText.Text;

                                var dbDatabase = new SqLiteDatabase();
                                dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);
                            }
                              
                            var resultIntent = new Intent();
                            SetResult(Result.Ok, resultIntent);
                            Finish();
                        }
                        else Methods.DisplayAndHudErrorResult(this, respond); 
                    } 
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }
         
        #endregion

        #region LoadMyData

        private async void LoadMyData()
        {
            try
            {
                 new SqLiteDatabase().GetMyProfile();

                if (ListUtils.MyProfileList.Count == 0)
                    await ApiRequest.GetProfile_Api(this);

                var myData = ListUtils.MyProfileList.FirstOrDefault();
                if (myData != null)
                {
                    FirstNameEditText.Text = myData.Fname;
                    LastNameEditText.Text = myData.Lname;
                    AboutEditText.Text = Methods.FunString.DecodeString(myData.About);
                    WebsiteEditText.Text = myData.Website;
                    FacebookEditText.Text = myData.Facebook;
                    GoogleEditText.Text = myData.Google;
                    TwitterEditText.Text = myData.Twitter;
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