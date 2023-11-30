using System;
using System.Collections.Generic;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Core.Content;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using TheArtOfDev.Edmodo.Cropper;
using Java.IO;
using Java.Lang;
using PixelPhoto.Activities.Base;
using PixelPhoto.Activities.SuggestionsUsers;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.RestCalls;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace PixelPhoto.Activities.MyProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AddDataProfileActivity : BaseActivity, View.IOnClickListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private Toolbar Toolbar;
        private TextView SaveTextView;
        private ImageView UserImage;
        private EditText FirstNameEditText,LastNameEditText, AboutEditText, GenderEditText, WebsiteEditText, FacebookEditText, GoogleEditText;
        private CircleButton AddImageActionButton;
         
        private string ResultPathImage;
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
                SetContentView(Resource.Layout.AddDataProfileLayout);

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
                UserImage = FindViewById<ImageView>(Resource.Id.userimg);
                FirstNameEditText = FindViewById<EditText>(Resource.Id.firstNameText);
                LastNameEditText = FindViewById<EditText>(Resource.Id.lasttNameText);
                AboutEditText = FindViewById<EditText>(Resource.Id.aboutText);
                GenderEditText = FindViewById<EditText>(Resource.Id.genderText);
                WebsiteEditText = FindViewById<EditText>(Resource.Id.websiteText);
                FacebookEditText = FindViewById<EditText>(Resource.Id.facebookText);
                GoogleEditText = FindViewById<EditText>(Resource.Id.googleText);
                AddImageActionButton = FindViewById<CircleButton>(Resource.Id.imageButton);
                
                SaveTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                SetColorEditText(FirstNameEditText);
                SetColorEditText(LastNameEditText);
                SetColorEditText(AboutEditText);
                SetColorEditText(GenderEditText);
                SetColorEditText(WebsiteEditText);
                SetColorEditText(FacebookEditText);
                SetColorEditText(GoogleEditText); 

                GenderEditText.SetOnClickListener(this);

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
                    Toolbar.Title = GetString(Resource.String.Lbl_Add_Your_Data);
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
                    AddImageActionButton.Click += AddImageActionButtonOnClick;
                }
                else
                {
                    SaveTextView.Click -= SaveDataUserOnClick;
                    AddImageActionButton.Click -= AddImageActionButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void OpenDialog()
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Male));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Female));

                dialogList.Title(GetText(Resource.String.Lbl_Gender));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        //Set Value Gender
        public void OnClick(View v)
        {
            try
            {
                OpenDialog();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                GenderEditText.Text = itemString.ToString();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        //Event Save data and Go To SuggestionsUsers  
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

                    if (!string.IsNullOrEmpty(GenderEditText.Text))
                        dictionary.Add("gender", GenderEditText.Text.ToLower());
                     
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

                            StartActivity(new Intent(this, typeof(SuggestionsUsersActivity)));
                            Finish();
                        }
                        else Methods.DisplayAndHudErrorResult(this, respondString); 
                    }
                    else
                    {
                        StartActivity(new Intent(this, typeof(SuggestionsUsersActivity)));
                        Finish();
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
         
        //Event Open Image 
        private void AddImageActionButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Activity()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this); 
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                        
                    }   
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion
         
        #region Permissions && Result >> Save Image 

        //Result 
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 108 || requestCode == CropImage.CropImageActivityRequestCode && resultCode == Result.Ok)
                {
                    var result = CropImage.GetActivityResult(data);

                    if (result.IsSuccessful)
                    {
                        ResultPathImage = result.Uri.Path;

                        if (!string.IsNullOrEmpty(ResultPathImage))
                        { 
                            var file2 = new File(ResultPathImage);
                            var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                            Glide.With(this).Load(photoUri).Apply(new RequestOptions().CircleCrop()).Into(UserImage);
                              
                            if (Methods.CheckConnectivity())
                            {
                                (var apiStatus, var respond) = await RequestsAsync.User.SaveImageUser(ResultPathImage).ConfigureAwait(false);
                                if (apiStatus == 200)
                                {
                                    if (respond is MessageObject messageObject)
                                    {
                                        Console.WriteLine(messageObject.Message);
                                    }
                                }
                                else Methods.DisplayAndHudErrorResult(this, respond);
                            }
                            else
                            {
                                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                            }  
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                    }
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

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long)?.Show();
                    }
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