using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using PixelPhoto.Activities.Base;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.RestCalls;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class ForgetPasswordActivity : BaseActivity
    {
        #region Variables Basic

        private ImageView BackgroundImage;
        private Toolbar Toolbar;
        private EditText EmailEditText;
        private Button SendButton;
        private ProgressBar ProgressBar;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Methods.App.FullScreenApp(this); 
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                 
                // Create your application here
                SetContentView(Resource.Layout.ForgetPasswordLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar(); 
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
                BackgroundImage = FindViewById<ImageView>(Resource.Id.backgroundimage);
                EmailEditText = FindViewById<EditText>(Resource.Id.edt_email);
                SendButton = FindViewById<Button>(Resource.Id.SignInButton);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

                if (AppSettings.DisplayImageOnForgetPasswordBackground)
                    GlideImageLoader.LoadImage(this, "ForgetPasswordBackground", BackgroundImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable, false);
                Methods.SetColorEditText(EmailEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                ProgressBar.Visibility = ViewStates.Gone;
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
                Toolbar.Title = GetString(Resource.String.Lbl_Forget_password);
                Toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);

                SetSupportActionBar(Toolbar);
                SupportActionBar.SetDisplayShowCustomEnabled(true);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(true);
                SupportActionBar.SetDisplayShowHomeEnabled(true);
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
                    SendButton.Click += SendButtonOnClick;
                }
                else
                {
                    SendButton.Click -= SendButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Events

        private async void SendButtonOnClick(object sender, EventArgs e)
        {

            try
            {
                if (!string.IsNullOrEmpty(EmailEditText.Text))
                {
                    if (Methods.CheckConnectivity())
                    {
                        var check = Methods.FunString.IsEmailValid(EmailEditText.Text);
                        if (!check)
                        {
                            Methods.DialogPopup.InvokeAndShowDialog(this,GetText(Resource.String.Lbl_VerificationFailed),GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                        }
                        else
                        {
                            ProgressBar.Visibility = ViewStates.Visible;
                            SendButton.Visibility = ViewStates.Gone;
                            var (apiStatus, respond) = await RequestsAsync.Auth.ForgetPassword(EmailEditText.Text);
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    Console.WriteLine(result.Message);

                                    ProgressBar.Visibility = ViewStates.Gone;
                                    SendButton.Visibility = ViewStates.Visible;
                                    Methods.DialogPopup.InvokeAndShowDialog(this,
                                        GetText(Resource.String.Lbl_Security),
                                        GetText(Resource.String.Lbl_Email_Has_Been_Send),
                                        GetText(Resource.String.Lbl_Ok));
                                }
                            }
                            else if (apiStatus == 400)
                            {
                                if (respond is ErrorObject error)
                                {
                                    var errorText = error.Error.ErrorText;
                                    var errorId = error.Error.ErrorId;
                                    if (errorId == "5")
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorForgotPassword_5), GetText(Resource.String.Lbl_Ok));
                                    else if (errorId == "13")
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorForgotPassword_13), GetText(Resource.String.Lbl_Ok));
                                    else if (errorId == "14")
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorForgotPassword_14), GetText(Resource.String.Lbl_Ok));
                                    else if (errorId == "19")
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_19), GetText(Resource.String.Lbl_Ok));
                                    else
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                }

                                ProgressBar.Visibility = ViewStates.Gone;
                                SendButton.Visibility = ViewStates.Visible;
                            }
                            else if (apiStatus == 404)
                            {
                                //var Error = Respond.ToString();
                                ProgressBar.Visibility = ViewStates.Gone;
                                SendButton.Visibility = ViewStates.Visible;
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond?.ToString(), GetText(Resource.String.Lbl_Ok));
                            }
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        SendButton.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed),GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                ProgressBar.Visibility = ViewStates.Gone;
                SendButton.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed),exception.ToString(), GetText(Resource.String.Lbl_Ok));
            } 
        }
        
        #endregion 
    }
}