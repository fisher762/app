using System;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Core.Content;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using PixelPhoto.Activities.Base;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.RestCalls;
using TheArtOfDev.Edmodo.Cropper;
using Console = System.Console;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace PixelPhoto.Payment
{
    [Activity(Icon ="@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class PaymentLocalActivity : BaseActivity
    {
        #region Variables Basic

        private TextView BankName, CardNumber, CardCode, CardCountry, CardName;
        private ImageView Image;
        private CircleButton ImageClose;
        private Button BtnAddImage, BtnApply;
        private string Id, Price, PayType, PathImage = "";

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
                SetContentView(Resource.Layout.PaymentLocalLayout);

                Id = Intent?.GetStringExtra("Id") ?? "";
                Price = Intent?.GetStringExtra("Price") ?? "";
                PayType = Intent?.GetStringExtra("payType") ?? ""; // Funding  

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

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions
         
        private void InitComponent()
        {
            try
            {
                BankName = (TextView)FindViewById(Resource.Id.bank_name);
                CardNumber = (TextView)FindViewById(Resource.Id.card_number);
                CardCode = (TextView)FindViewById(Resource.Id.card_code);
                CardCountry = (TextView)FindViewById(Resource.Id.card_country);
                CardName = (TextView)FindViewById(Resource.Id.card_name); 
                Image = (ImageView)FindViewById(Resource.Id.Image);
                 
                ImageClose = (CircleButton)FindViewById(Resource.Id.ImageCircle);
                BtnAddImage = (Button)FindViewById(Resource.Id.btn_AddPhoto);
                BtnApply = (Button)FindViewById(Resource.Id.ApplyButton);

                var bankDescription = ListUtils.SettingsSiteList?.BankDescription;
                bankDescription = bankDescription?.Replace("&lt;", "<").Replace("&gt;", ">");
                var splitText = bankDescription?.Split(new[] { "<p>", "</p>" }, StringSplitOptions.None);
                Console.WriteLine(splitText);
                  
                if (splitText?.Length > 0)
                { 
                    CardNumber.Text = splitText[1];
                    CardName.Text = splitText[3];
                    CardCode.Text = splitText[5];
                    CardCountry.Text = splitText[7];

                    var bankName = splitText[0].Split(new[] { ">", "</h4>" }, StringSplitOptions.None);
                    if (bankName.Length > 0)
                    {
                        BankName.Text = bankName[12];
                        BankName.Visibility = ViewStates.Visible;
                    }
                } 
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_BankTransfer);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                    SetSupportActionBar(toolbar);
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
                    ImageClose.Click += ImageCloseOnClick;
                    BtnAddImage.Click += BtnAddImageOnClick;
                    BtnApply.Click += BtnApplyOnClick;
                }
                else
                {
                    ImageClose.Click -= ImageCloseOnClick;
                    BtnAddImage.Click -= BtnAddImageOnClick;
                    BtnApply.Click -= BtnApplyOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Events

        private void BtnAddImageOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (string.IsNullOrEmpty(PathImage) || (string.IsNullOrWhiteSpace(PathImage)))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_ErrorPleaseSelectImage), ToastLength.Long)?.Show();
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var apiStatus = 0; dynamic respond = null!;
                    string description;

                    //if (PayType == "membership")
                    //{
                    //    description = "Account upgrade request for type" + Id + " = " + Price;
                    //    (apiStatus, respond) = await RequestsAsync.Global.UploadBankRecipeAsync(Id, Price, description, PathImage);
                    //}
                    //else if(PayType == "AddFunds")
                    //{
                    //    description = "Add to balance = " + Price; 
                    //    //wallet
                    //    (apiStatus, respond) = await RequestsAsync.Global.UploadBankRecipeAsync("wallet", Price, description, PathImage);
                    //}
                    //
                    if (PayType == "Funding")
                    {
                        description = "Donate to funding = " + Price;  
                        (apiStatus, respond) = await RequestsAsync.Global.UploadBankRecipeAsync("donate", Price, description, PathImage , Id);
                    }
                    else if (PayType == "Store")
                    {
                        description = "Buy Store = " + Price;  
                        (apiStatus, respond) = await RequestsAsync.Global.UploadBankRecipeAsync("store", Price, description, PathImage , Id);
                    }
                       
                    if (apiStatus == 200)
                    {
                        AndHUD.Shared.Dismiss(this);

                        Toast.MakeText(this, GetText(Resource.String.Lbl_YourWasReceiptSuccessfullyUploaded), ToastLength.Long)?.Show();
                        Finish();
                    }
                    else Methods.DisplayAndHudErrorResult(this, respond); 
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                } 
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ImageCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                PathImage = "";
                GlideImageLoader.LoadImage(this, "Grey_Offline", Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 108 || requestCode == CropImage.CropImageActivityRequestCode)
                {
                    if (Methods.CheckConnectivity())
                    {
                        var result = CropImage.GetActivityResult(data);
                        if (result.IsSuccessful)
                        {
                            var resultPathImage = result.Uri.Path;
                            if (!string.IsNullOrEmpty(resultPathImage))
                            {
                                PathImage = resultPathImage;

                                var file2 = new File(PathImage);
                                var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                Glide.With(this).Load(photoUri).Apply(new RequestOptions().CenterCrop()).Into(Image); 
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
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

                if (requestCode == 106)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        OpenDialogGallery();
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

        private void OpenDialogGallery()
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
                        //request Code 108
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}