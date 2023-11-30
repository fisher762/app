using System;
using System.Collections.Generic;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Org.Json;
using PixelPhoto.Activities.Base;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.SocialLogins;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.OneSignal;
using PixelPhoto.SQLite;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Auth;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.RestCalls;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using Task = System.Threading.Tasks.Task;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
 
namespace PixelPhoto.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class LoginActivity : BaseActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback 
    {
        #region Variables Basic

        private TextView ForgetPasswordTextView;
        private EditText EmailEditText , PasswordEditText;
        private Button LoginButton;
        private ProgressBar ProgressBar;
        private ImageView BackgroundImage;
        private Toolbar Toolbar;
        private LoginButton FbLoginButton;
        private SignInButton GoogleSignInButton;
        private LinearLayout RegisterLayout;
         
        private ICallbackManager MFbCallManager;
        private FbMyProfileTracker ProfileTracker; 
        public static GoogleSignInClient MGoogleSignInClient;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
               
                Methods.App.FullScreenApp(this);  
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                //Set Full screen 
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
                SetContentView(Resource.Layout.LoginLayout);
                 
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitSocialLogins();

                //Check and Get Settings
                GetSettingsSite();

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

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    // Check Created My Folder Or Not 
                    Methods.Path.Chack_MyFolder();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        // Check Created My Folder Or Not 
                        Methods.Path.Chack_MyFolder();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(100);

                    }
                }

                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                base.OnStop();
                if (AppSettings.ShowGoogleLogin && MGoogleSignInClient?.AsGoogleApiClient() != null && MGoogleSignInClient.AsGoogleApiClient().IsConnected)
                    MGoogleSignInClient.AsGoogleApiClient()?.Disconnect();
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
                ForgetPasswordTextView = FindViewById<TextView>(Resource.Id.txt_forgot_pass);
                EmailEditText = FindViewById<EditText>(Resource.Id.edt_email);
                PasswordEditText = FindViewById<EditText>(Resource.Id.edt_password);
                LoginButton = FindViewById<Button>(Resource.Id.SignInButton);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                FbLoginButton = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                GoogleSignInButton = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                RegisterLayout = FindViewById<LinearLayout>(Resource.Id.tvRegister);
                  
                if (AppSettings.DisplayImageOnLoginBackground)
                    GlideImageLoader.LoadImage(this, "loginBackground", BackgroundImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

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
                    Toolbar.Title = GetString(Resource.String.Lbl_SignIn);
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

        private void InitSocialLogins()
        {
            try
            {
                //#Facebook
                if (AppSettings.ShowFacebookLogin)
                {
                    //FacebookSdk.SdkInitialize(this);

                    ProfileTracker = new FbMyProfileTracker();
                    ProfileTracker.MOnProfileChanged += ProfileTrackerOnMOnProfileChanged;
                    ProfileTracker.StartTracking();

                    FbLoginButton = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    FbLoginButton.Visibility = ViewStates.Visible;
                    FbLoginButton.SetPermissions(new List<string>
                    {
                        "email",
                        "public_profile"
                    });

                    MFbCallManager = CallbackManagerFactory.Create();
                    FbLoginButton.RegisterCallback(MFbCallManager, this);

                    //FB accessToken
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }

                    var hashId = Methods.App.GetKeyHashesConfigured(this);
                    Console.WriteLine(hashId);
                }
                else
                {
                    FbLoginButton = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    FbLoginButton.Visibility = ViewStates.Gone;
                }

                //#Google
                if (AppSettings.ShowGoogleLogin)
                { 
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient = GoogleSignIn.GetClient(this, gso);

                    GoogleSignInButton = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    GoogleSignInButton.Click += MSignBtnOnClick;
                }
                else
                {
                    GoogleSignInButton = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    GoogleSignInButton.Visibility = ViewStates.Gone;
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
                    ForgetPasswordTextView.Click += ForgetPasswordButton_Click;
                    LoginButton.Click += SignInButtonOnClick;
                    RegisterLayout.Click += RegisterLayoutOnClick;
                }
                else
                {
                    ForgetPasswordTextView.Click -= ForgetPasswordButton_Click;
                    LoginButton.Click -= SignInButtonOnClick;
                    RegisterLayout.Click -= RegisterLayoutOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetSettingsSite()
        {
            try
            {
                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Event Click open Register Activity
        private void RegisterLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(RegisterActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Click Login
        private async void SignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (string.IsNullOrEmpty(EmailEditText.Text.Replace(" ", "")) || string.IsNullOrEmpty(PasswordEditText.Text))
                    {
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                        return; 
                    }
                     
                    HideKeyboard();

                    ProgressBar.Visibility = ViewStates.Visible;
                    LoginButton.Visibility = ViewStates.Gone;

                    (var apiStatus, var respond) = await RequestsAsync.Auth.Login(EmailEditText.Text.Replace(" ", ""), PasswordEditText.Text, UserDetails.DeviceId);
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

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetProfile_Api(this) });

                                StartActivity(new Intent(this, typeof(HomeActivity)));

                                FinishAffinity();
                            }
                        }
                    }
                    else if (apiStatus == 400)
                    {
                        if (respond is ErrorObject error)
                        {
                            var errorText = error.Error.ErrorText;
                            var errorId = error.Error.ErrorId;
                            if (errorId == "2")
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_2), GetText(Resource.String.Lbl_Ok));
                            else if (errorId == "4")
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_4), GetText(Resource.String.Lbl_Ok));
                            else if (errorId == "19")
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_19), GetText(Resource.String.Lbl_Ok));
                            else
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        LoginButton.Visibility = ViewStates.Visible;
                    }
                    else if (apiStatus == 404)
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        LoginButton.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond?.toString(), GetText(Resource.String.Lbl_Ok));
                    }

                }
                else
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    LoginButton.Visibility = ViewStates.Visible;
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                ProgressBar.Visibility = ViewStates.Gone;
                LoginButton.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
            }
        }

        //Event Click open ForgetPassword Activity
        private void ForgetPasswordButton_Click(object sender, EventArgs e)
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

        #endregion

        #region Social Logins

        private string FbAccessToken, GAccessToken;

        #region Facebook

        public void OnCancel()
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Gone;
                GoogleSignInButton.Visibility = ViewStates.Visible;

                SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnError(FacebookException error)
        {
            try
            {

                ProgressBar.Visibility = ViewStates.Gone;
                GoogleSignInButton.Visibility = ViewStates.Visible;

                // Handle exception
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

                SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnSuccess(Object result)
        {
            try
            {
                //var loginResult = result as LoginResult;
                //var id = AccessToken.CurrentAccessToken.UserId;

                ProgressBar.Visibility = ViewStates.Visible;
                GoogleSignInButton.Visibility = ViewStates.Gone;

                SetResult(Result.Ok);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void OnCompleted(JSONObject json, GraphResponse response)
        {
            try
            {
                //var data = json.ToString();
                //var result = JsonConvert.DeserializeObject<FacebookResult>(data);
                //FbEmail = result.Email;

                ProgressBar.Visibility = ViewStates.Visible;
                LoginButton.Visibility = ViewStates.Gone;

                var accessToken = AccessToken.CurrentAccessToken;
                if (accessToken != null)
                {
                    FbAccessToken = accessToken.Token;

                    //Login Api 
                    (var apiStatus, var respond) = await RequestsAsync.Auth.SocialLogin(FbAccessToken, "facebook");
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

                                StartActivity(new Intent(this, typeof(HomeActivity)));
                                Finish();
                            }
                        }
                    }
                    else if (apiStatus == 400)
                    {
                        if (respond is ErrorObject error)
                        {
                            var errorText = error.Error.ErrorText;
                            var errorId = error.Error.ErrorId;
                            if (errorId == "2")
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_2), GetText(Resource.String.Lbl_Ok));
                            else if (errorId == "4")
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_4), GetText(Resource.String.Lbl_Ok));
                            else if (errorId == "19")
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_19), GetText(Resource.String.Lbl_Ok));
                            else
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        LoginButton.Visibility = ViewStates.Visible;
                    }
                    else if (apiStatus == 404)
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        LoginButton.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_Login), GetText(Resource.String.Lbl_Ok));
                    } 
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                GoogleSignInButton.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ProfileTrackerOnMOnProfileChanged(object sender, ProfileChangedEventArgs e)
        {
            try
            {
                if (e.MProfile != null)
                    try
                    {
                        //FbFirstName = e.MProfile.FirstName;
                        //FbLastName = e.MProfile.LastName;
                        //FbName = e.MProfile.Name;
                        //FbProfileId = e.MProfile.Id;

                        var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
                        var parameters = new Bundle();
                        parameters.PutString("fields", "id,name,age_range,email");
                        request.Parameters = parameters;
                        request.ExecuteAsync();
                    }
                    catch (Exception ex)
                    {
                        Methods.DisplayReportResultTrack(ex);
                    }
                else
                    Toast.MakeText(this, GetString(Resource.String.Lbl_Null_Data_User), ToastLength.Short)?.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //======================================================

        #region Google

        //Event Click login using google
        private void MSignBtnOnClick(object sender, EventArgs e)
        {
            try
            {  
                if (MGoogleSignInClient == null)
                {  
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient ??= GoogleSignIn.GetClient(this, gso); 
                }

                var signInIntent = MGoogleSignInClient.SignInIntent;
                StartActivityForResult(signInIntent, 0);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        private async void SetContentGoogle(GoogleSignInAccount acct)
        {
            try
            {
                //Successful log in hooray!!
                if (acct != null)
                {
                    ProgressBar.Visibility = ViewStates.Visible;
                    LoginButton.Visibility = ViewStates.Gone;
                     
                    GAccessToken = acct.IdToken; 
                     
                    if (!string.IsNullOrEmpty(GAccessToken))
                    {
                        //Login Api 
                        (var apiStatus, var respond) = await RequestsAsync.Auth.SocialLogin(GAccessToken, "google");
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
                                   

                                    StartActivity(new Intent(this, typeof(HomeActivity)));
                                    Finish();
                                }
                            }
                        }
                        else if (apiStatus == 400)
                        {
                            if (respond is ErrorObject error)
                            {
                                var errorText = error.Error.ErrorText;
                                var errorId = error.Error.ErrorId;
                                if (errorId == "2") 
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_2), GetText(Resource.String.Lbl_Ok));
                                else if (errorId == "4")
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_4), GetText(Resource.String.Lbl_Ok));
                                else if (errorId == "19")
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_19), GetText(Resource.String.Lbl_Ok));
                                else
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                            }
                            else if (respond is ErrorGoogleObject error2)
                            {
                                var errorText = error2.Errors.ErrorText.Message; 
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                            }

                            ProgressBar.Visibility = ViewStates.Gone;
                            LoginButton.Visibility = ViewStates.Visible;
                        }
                        else if (apiStatus == 404)
                        {
                            ProgressBar.Visibility = ViewStates.Gone;
                            LoginButton.Visibility = ViewStates.Visible;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                GoogleSignInButton.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        //======================================================

        #endregion

        #region Permissions && Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                Console.WriteLine("Login_Activity >> onActivityResult:" + requestCode + ":" + resultCode + ":" + data);
                if (requestCode == 0)
                {
                    var task = await GoogleSignIn.GetSignedInAccountFromIntentAsync(data);
                    SetContentGoogle(task); 
                }
                else
                {
                    // Logins Facebook
                    MFbCallManager.OnActivityResult(requestCode, (int)resultCode, data);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 100)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        // Check Created My Folder Or Not 
                        Methods.Path.Chack_MyFolder();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long)?.Show();
                        FinishAffinity();
                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}