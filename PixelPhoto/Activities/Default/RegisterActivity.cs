using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using PixelPhoto.Activities.Base;
using PixelPhoto.Activities.MyProfile;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.OneSignal;
using PixelPhoto.SQLite;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Auth;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.RestCalls;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class RegisterActivity : BaseActivity
    {

        #region Variables Basic

        private ImageView BackgroundImage;
        private Toolbar Toolbar;
        private EditText EmailEditText, UsernameEditText, PasswordEditText, ConfirmPasswordEditText;
        private Button RegisterButton;
        private LinearLayout TermsLayout, SignLayout;
        private ProgressBar ProgressBar;
         
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

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

                // Create your application here
                SetContentView(Resource.Layout.RegisterLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                //OneSignal Notification  
                //====================================== 
                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.RegisterNotificationDevice();

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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                UsernameEditText = FindViewById<EditText>(Resource.Id.edt_username);
                PasswordEditText = FindViewById<EditText>(Resource.Id.edt_password);
                ConfirmPasswordEditText = FindViewById<EditText>(Resource.Id.edt_Confirmpassword);
                RegisterButton = FindViewById<Button>(Resource.Id.SignInButton);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                TermsLayout = FindViewById<LinearLayout>(Resource.Id.termsLayout);
                SignLayout = FindViewById<LinearLayout>(Resource.Id.SignLayout);

                if (AppSettings.DisplayImageOnRegisterBackground)
                    GlideImageLoader.LoadImage(this, "RegisterBackground", BackgroundImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                ProgressBar.Visibility = ViewStates.Invisible;

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
                    Toolbar.Title = GetString(Resource.String.Lbl_Register);
                    Toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

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
                    RegisterButton.Click += RegisterButtonOnClick;
                    TermsLayout.Click += TermsLayoutOnClick;
                    SignLayout.Click += SignLayoutOnClick;
                }
                else
                {
                    RegisterButton.Click -= RegisterButtonOnClick;
                    TermsLayout.Click -= TermsLayoutOnClick;
                    SignLayout.Click -= SignLayoutOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Event Click open Login Activity
        private void SignLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(LoginActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Open Terms of Service
        private void TermsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var url = Client.WebsiteUrl + "/terms-of-use";
                new IntentController(this).OpenBrowserFromApp(url);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Click Register Button
        private async void RegisterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (!string.IsNullOrEmpty(EmailEditText.Text.Replace(" ", "")) || !string.IsNullOrEmpty(UsernameEditText.Text.Replace(" ", "")) || !string.IsNullOrEmpty(PasswordEditText.Text) || !string.IsNullOrEmpty(ConfirmPasswordEditText.Text))
                    { 
                        var check = Methods.FunString.IsEmailValid(EmailEditText.Text.Replace(" ", ""));
                        if (!check)
                        {
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                        }
                        else
                        {
                            if (PasswordEditText.Text != ConfirmPasswordEditText.Text)
                            {
                                ProgressBar.Visibility = ViewStates.Gone;
                                RegisterButton.Visibility = ViewStates.Visible;
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_Register_password), GetText(Resource.String.Lbl_Ok));
                            }
                            else
                            {
                                HideKeyboard();

                                ProgressBar.Visibility = ViewStates.Visible;
                                RegisterButton.Visibility = ViewStates.Gone;

                                var emailValidation = ListUtils.SettingsSiteList?.EmailValidation.ToLower();
                                (var apiStatus, var respond) = await RequestsAsync.Auth.Register(UsernameEditText.Text.Replace(" ", ""), PasswordEditText.Text, ConfirmPasswordEditText.Text, EmailEditText.Text.Replace(" ", ""), UserDetails.DeviceId, emailValidation);
                                if (apiStatus == 200)
                                {
                                    if (respond is AuthObject auth)
                                    {
                                        if (auth.Data != null)
                                        { 
                                            Current.AccessToken = auth.Data.AccessToken;

                                            UserDetails.Username = EmailEditText.Text;
                                            UserDetails.FullName = EmailEditText.Text;
                                            UserDetails.Password = PasswordEditText.Text;
                                            UserDetails.AccessToken = auth.Data.AccessToken;
                                            UserDetails.UserId = auth.Data.UserId;
                                            UserDetails.Status = "Active";
                                            UserDetails.Cookie = auth.Data.AccessToken;
                                            UserDetails.Email = EmailEditText.Text;

                                            //Insert user data to database
                                            var user = new DataTables.LoginTb
                                            {
                                                UserId = UserDetails.UserId,
                                                AccessToken = UserDetails.AccessToken,
                                                Cookie = UserDetails.Cookie,
                                                Username = EmailEditText.Text,
                                                Password = PasswordEditText.Text,
                                                Status = "Active",
                                                Lang = "",
                                                DeviceId = UserDetails.DeviceId,
                                            };
                                            ListUtils.DataUserLoginList.Add(user);

                                            var dbDatabase = new SqLiteDatabase();
                                            dbDatabase.InsertOrUpdateLogin_Credentials(user);

                                            StartActivity(new Intent(this, typeof(AddDataProfileActivity)));
                                            Finish();
                                        }
                                    }
                                    else if (respond is MessageObject mess)
                                    {
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_emailValidation), GetText(Resource.String.Lbl_Ok));
                                    }
                                }
                                else if (apiStatus == 400)
                                {
                                    if (respond is ErrorObject error)
                                    {
                                        var errorText = error.Error.ErrorText;
                                        var errorid = error.Error.ErrorId;
                                        if (errorid == "5")
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_8), GetText(Resource.String.Lbl_Ok));
                                        else if (errorid == "6")
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_6), GetText(Resource.String.Lbl_Ok));
                                        else if (errorid == "7")
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_7), GetText(Resource.String.Lbl_Ok));
                                        else if (errorid == "8")
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_8), GetText(Resource.String.Lbl_Ok));
                                        else if (errorid == "9")
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_9), GetText(Resource.String.Lbl_Ok));
                                        else if (errorid == "10")
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_10), GetText(Resource.String.Lbl_Ok));
                                        else if (errorid == "11")
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_11), GetText(Resource.String.Lbl_Ok));
                                        else if (errorid == "12")
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_12), GetText(Resource.String.Lbl_Ok));
                                        else if (errorid == "19")
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_19), GetText(Resource.String.Lbl_Ok));
                                        else
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                    }

                                    ProgressBar.Visibility = ViewStates.Gone;
                                    RegisterButton.Visibility = ViewStates.Visible;
                                }
                                else if (apiStatus == 404)
                                {
                                    ProgressBar.Visibility = ViewStates.Gone;
                                    RegisterButton.Visibility = ViewStates.Visible;
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond?.toString(), GetText(Resource.String.Lbl_Ok));
                                }
                            }
                            ProgressBar.Visibility = ViewStates.Gone;
                            RegisterButton.Visibility = ViewStates.Visible; 
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        RegisterButton.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    RegisterButton.Visibility = ViewStates.Visible;
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                RegisterButton.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));

                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}