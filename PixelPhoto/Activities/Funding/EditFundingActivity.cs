using System;
using System.Linq;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidHUD;
using TheArtOfDev.Edmodo.Cropper;
using Java.IO;
using Newtonsoft.Json;
using PixelPhoto.Activities.Base;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Console = System.Console;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace PixelPhoto.Activities.Funding
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class EditFundingActivity : BaseActivity
    {
        #region Variables Basic

        private TextView TxtAdd;
        private ImageView ImgCover;
        private Button BtnSelectImage;
        private EditText TxtName, TxtAmount, TxtDescription;
        private string PathImage;
        private FundingDataObject DataObject;

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
                SetContentView(Resource.Layout.CreateFundingLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                GetDataFunding();
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
                ImgCover = FindViewById<ImageView>(Resource.Id.fundingCover);
                BtnSelectImage = FindViewById<Button>(Resource.Id.btn_selectimage);
                TxtName = FindViewById<EditText>(Resource.Id.fundingname);
                TxtAmount = FindViewById<EditText>(Resource.Id.fundingAmount);
                TxtDescription = FindViewById<EditText>(Resource.Id.description);

                TxtAdd.Text = GetText(Resource.String.Lbl_Save);
                TxtAdd.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                    toolbar.Title = GetString(Resource.String.Lbl_FundingRequests);
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
                }
                else
                {
                    TxtAdd.Click -= TxtAddOnClick;
                    BtnSelectImage.Click -= BtnSelectImageOnClick;
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

        //Save 
        private async void TxtAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtAmount.Text) || string.IsNullOrWhiteSpace(TxtAmount.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_amount), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtDescription.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_Description), ToastLength.Short)?.Show();
                        return;
                    }

                    if (PathImage == DataObject.Image)
                    {
                        PathImage = "";
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var (apiStatus, respond) = await RequestsAsync.Funding.EditFunding(DataObject.Id.ToString(),TxtName.Text, TxtDescription.Text, TxtAmount.Text, PathImage);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            Console.WriteLine(result.Message);

                            var adapter = HomeActivity.GetInstance()?.NewsFeedFragment?.NewsFeedAdapter?.HolderFunding?.FundingAdapters;
                            var data = adapter?.FundingList?.FirstOrDefault(a => a.Id == DataObject.Id);
                            if (data != null)
                            {
                                data.Id = DataObject.Id;
                                data.Title = TxtName.Text;
                                data.Description = TxtDescription.Text;
                                data.Amount = TxtAmount.Text;
                                if (PathImage != "")
                                    data.Image = PathImage;
                                 
                                var index = adapter.FundingList.IndexOf(data);
                                if (index >-1)
                                {
                                    ListUtils.FundingList[index] = data;

                                    adapter.NotifyItemChanged(index);

                                    var fundingActivity = FundingActivity.GetInstance();
                                    if (fundingActivity?.MAdapter != null)
                                    {
                                        fundingActivity.MAdapter.FundingList[index] = data;
                                        fundingActivity.MAdapter.NotifyItemChanged(index); 
                                    }
                                }

                                var intent = new Intent();
                                intent.PutExtra("itemData", JsonConvert.SerializeObject(data));
                                SetResult(Result.Ok, intent); 
                            }
                        }

                        AndHUD.Shared.Dismiss(this);
                        Toast.MakeText(this, GetString(Resource.String.Lbl_FundingSuccessfullyEdited), ToastLength.Short)?.Show();
                         
                        Finish(); 
                    }
                    else Methods.DisplayAndHudErrorResult(this, respond); 
                }
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
                                GlideImageLoader.LoadImage(this, resultUri.Path, ImgCover, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
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
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetDataFunding()
        {
            try
            {
                DataObject = JsonConvert.DeserializeObject<FundingDataObject>(Intent?.GetStringExtra("FundingObject"));
                if (DataObject != null)
                {
                    PathImage = DataObject.Image;
                    GlideImageLoader.LoadImage(this, DataObject.Image, ImgCover, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                      
                    TxtName.Text = Methods.FunString.DecodeString(DataObject.Title);
                    TxtDescription.Text = Methods.FunString.DecodeString(DataObject.Description);

                    TxtAmount.Text = DataObject.Amount; 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}