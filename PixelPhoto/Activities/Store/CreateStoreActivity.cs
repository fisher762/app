using System;
using System.Collections.Generic;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Google.Android.Material.FloatingActionButton;
using TheArtOfDev.Edmodo.Cropper;
using Java.IO;
using Java.Lang;
using PixelPhoto.Activities.Base;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.RestCalls;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace PixelPhoto.Activities.Store
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class CreateStoreActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic
   
        private TextView TxtAdd;
        private ImageView Image;
        private FloatingActionButton BtnSelectImage;
        private EditText TxtTitle, TxtCategory, TxtLicenseType, TxtPrice, TxtTags;
        private string PathImage , TypeDialog, CategoryId, LicenseTypeId;

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
                SetContentView(Resource.Layout.CreateStoreLayout);

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
          
        #endregion
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                TxtAdd = FindViewById<TextView>(Resource.Id.toolbar_title);
                Image = FindViewById<ImageView>(Resource.Id.image);

                BtnSelectImage = FindViewById<FloatingActionButton>(Resource.Id.btn_selectImage);

                TxtTitle = FindViewById<EditText>(Resource.Id.TitleEditText);
                TxtCategory = FindViewById<EditText>(Resource.Id.CategoryText);
                TxtLicenseType = FindViewById<EditText>(Resource.Id.LicenseTypeText);
                TxtPrice = FindViewById<EditText>(Resource.Id.PriceText);
                TxtTags = FindViewById<EditText>(Resource.Id.TagsText);

                TxtAdd.Text = GetText(Resource.String.Lbl_Submit);
                TxtAdd.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);


                Methods.SetColorEditText(TxtTitle, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTags, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCategory, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLicenseType, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPrice, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(TxtCategory);
                Methods.SetFocusable(TxtLicenseType);

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
                    toolbar.Title = GetString(Resource.String.Lbl_CreateStore);
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
                    TxtAdd.Click += TxtAddOnClick;
                    BtnSelectImage.Click += BtnSelectImageOnClick;
                    TxtCategory.Touch += TxtCategoryOnTouch;
                    TxtLicenseType.Touch += TxtLicenseTypeOnTouch;
                }
                else
                {
                    TxtAdd.Click -= TxtAddOnClick;
                    BtnSelectImage.Click -= BtnSelectImageOnClick;
                    TxtCategory.Touch -= TxtCategoryOnTouch;
                    TxtLicenseType.Touch -= TxtLicenseTypeOnTouch;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Add Image
        private void BtnSelectImageOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery(); //requestCode >> 500 => Image Gallery 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtCategoryOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "Category";

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                var arrayAdapter = AppTools.GetCategoryStoreList().Select(cat => cat.Value).ToList();

                dialogList.Title(GetText(Resource.String.Lbl_Category));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtLicenseTypeOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "LicenseType";

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                var arrayAdapter = AppTools.GetLicenseTypeStoreList().Select(cat => cat.Value).ToList();

                dialogList.Title(GetText(Resource.String.Lbl_LicenseType));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save 
        private async void TxtAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtTitle.Text) || string.IsNullOrWhiteSpace(TxtTitle.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_title), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtCategory.Text) || string.IsNullOrWhiteSpace(TxtCategory.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_category), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtLicenseType.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_licenseType), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtPrice.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_price), ToastLength.Short)?.Show();
                    return;
                }
                    
                if (string.IsNullOrEmpty(TxtTags.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_tags), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(PathImage))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short)?.Show();
                    return;
                }

                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                     
                var dict = new Dictionary<string, string>
                {
                    {"title", TxtTitle.Text},
                    {"tags", TxtTags.Text},
                    {"category", CategoryId}, 
                };
                
                var (apiStatus, respond) = await RequestsAsync.Store.UploadStoreImage(dict, LicenseTypeId, TxtPrice.Text, PathImage);
                if (apiStatus == 200)
                {
                    if (respond is MessageObject result)
                    {
                        Console.WriteLine(result.Message);

                        Toast.MakeText(this , GetText(Resource.String.Lbl_YourImageWasSuccessfullyUploaded), ToastLength.Short)?.Show();
                        AndHUD.Shared.Dismiss(this);

                        Finish();
                    }
                }
                else Methods.DisplayAndHudErrorResult(this, respond);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
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
                //If its from Camera or Gallery
                if (requestCode == CropImage.CropImageActivityRequestCode && resultCode == Result.Ok)
                {
                    var result = CropImage.GetActivityResult(data);

                    if (resultCode == Result.Ok)
                    {
                        if (result.IsSuccessful)
                        {
                            var resultUri = result.Uri;

                            if (!string.IsNullOrEmpty(resultUri.Path))
                            {
                                PathImage = resultUri.Path;
                                GlideImageLoader.LoadImage(this, resultUri.Path, Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
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
         
        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                var text = itemString.ToString();
                if (TypeDialog == "Category")
                {
                    CategoryId = AppTools.GetCategoryStoreList().FirstOrDefault(a => a.Value == text).Key;
                    TxtCategory.Text = text;
                }
                else if (TypeDialog == "LicenseType")
                {
                    LicenseTypeId = AppTools.GetLicenseTypeStoreList().FirstOrDefault(a => a.Value == text).Key;
                    TxtLicenseType.Text = text;
                }
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