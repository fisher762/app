using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidHUD;
using PixelPhoto.Activities.Base;
using PixelPhoto.Activities.Default;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.SettingsUser
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SettingPasswordActivity : BaseActivity
    {
        #region Variables Basic

        private Toolbar Toolbar;
        private TextView SaveTextView, LinkTextView;
        private EditText CurrentPasswordEditText, NewPsswordEditText, RepeatPasswordEditText;
       
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);  
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.SettingPasswordLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar(); 
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
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
           
        #region Functions

        private void InitComponent()
        {
            try
            {
                SaveTextView = FindViewById<TextView>(Resource.Id.toolbar_title);
                LinkTextView = FindViewById<TextView>(Resource.Id.linkText);
                CurrentPasswordEditText = FindViewById<EditText>(Resource.Id.currentPasswordText);
                NewPsswordEditText = FindViewById<EditText>(Resource.Id.newPsswordText);
                RepeatPasswordEditText = FindViewById<EditText>(Resource.Id.repeatPasswordText);
                SaveTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                SetColorEditText(CurrentPasswordEditText);
                SetColorEditText(NewPsswordEditText);
                SetColorEditText(RepeatPasswordEditText);
                 
                AdsGoogle.Ad_AdMobNative(this);
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
                    Toolbar.Title = GetText(Resource.String.Lbl_Change_Password);
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
                    SaveTextView.Click += SaveTextViewOnClick;
                    LinkTextView.Click += LinkTextViewOnClick;
                }
                else
                {
                    SaveTextView.Click -= SaveTextViewOnClick;
                    LinkTextView.Click -= LinkTextViewOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

       
        #endregion

        #region Events

        private void LinkTextViewOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(ForgetPasswordActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Save New Password
        private async void SaveTextViewOnClick(object sender, EventArgs e)
        {
            try
            {
                if (CurrentPasswordEditText.Text == "" || NewPsswordEditText.Text == "" || RepeatPasswordEditText.Text == "")
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Long)?.Show();
                    return;
                }

                if (NewPsswordEditText.Text != RepeatPasswordEditText.Text || CurrentPasswordEditText.Text != UserDetails.Password)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Your_password_dont_match), ToastLength.Long)?.Show();
                }
                else
                {
                    if (Methods.CheckConnectivity())
                    {
                        var dictionary = new Dictionary<string, string>
                        {
                            {"old_password", CurrentPasswordEditText.Text},
                            {"new_password", NewPsswordEditText.Text},
                            {"conf_password", RepeatPasswordEditText.Text}
                        };

                        if (dictionary.Count > 0)
                        {
                            //Show a progress
                            AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                            //Send Api
                            (var respondCode, var respondString) = await RequestsAsync.User.SaveSettings(dictionary);
                            if (respondCode == 200)
                            {
                                AndHUD.Shared.Dismiss(this);
                                Toast.MakeText(this, GetString(Resource.String.Lbl_YourInfoWasUpdated), ToastLength.Short)?.Show();

                                UserDetails.Password = NewPsswordEditText.Text;
                              
                                Finish();
                            }
                            else Methods.DisplayAndHudErrorResult(this, respondString); 
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion
         
    }
}