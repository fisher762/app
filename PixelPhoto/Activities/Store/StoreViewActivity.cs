using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.Base;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.Payment;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Store;
using PixelPhotoClient.RestCalls;
using PixelPhoto.Library.Anjo.Share;
using PixelPhoto.Library.Anjo.Share.Abstractions;
using PixelPhotoClient.GlobalClass;
using Xamarin.PayPal.Android;
using Exception = System.Exception;

namespace PixelPhoto.Activities.Store
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class StoreViewActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView StoreImage, IconBack, StoreAvatar;
        private TextView TxtUserName,TxtMore, TxtStoreTitle, TxtCategory, TxtImageType, TxtViews, TxtDownloads, TxtSells, TxtTags, TxtLicenseType;
        private Button BuyButton;
        //private LinearLayout BuyLayout;
        private StoreDataObject StoreData;
        private string StoreId ,DialogType, Price, LicenseTypeText, LicenseTypeId;
        private InitPayPalPayment InitPayPalPayment;
        private static StoreViewActivity Instance;
        private string TypeEventClick = "";

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
                SetContentView(Resource.Layout.StoreViewLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();

                if (AppSettings.ShowPaypal)
                    InitPayPalPayment = new InitPayPalPayment(this);

                LoadData();

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

        protected override void OnDestroy()
        {
            try
            {
                if (AppSettings.ShowPaypal)
                    InitPayPalPayment?.StopPayPalService();

                if (TypeEventClick == "OpenProfile")
                {
                    AppTools.OpenProfile(this, StoreData.UserId.ToString(), new UserDataObject
                    {
                        UserId = StoreData.UserId.ToString(),
                        Username = StoreData.Username,
                        Avatar = StoreData.Avatar, 
                    });
                }
                 
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
                StoreImage = FindViewById<ImageView>(Resource.Id.storeCoverImage);
                IconBack = FindViewById<ImageView>(Resource.Id.iv_back);

                StoreAvatar = FindViewById<ImageView>(Resource.Id.storeAvatar);
                TxtUserName = FindViewById<TextView>(Resource.Id.userName);
               

                TxtStoreTitle = FindViewById<TextView>(Resource.Id.storeTitle);

                TxtMore = FindViewById<TextView>(Resource.Id.toolbar_title);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtMore, IonIconsFonts.More);
                TxtMore.SetTextSize(ComplexUnitType.Sp, 20f);
                TxtMore.Visibility = ViewStates.Visible;

                TxtCategory = FindViewById<TextView>(Resource.Id.Category);
                TxtImageType = FindViewById<TextView>(Resource.Id.ImageType);
                TxtViews = FindViewById<TextView>(Resource.Id.Views);
                TxtDownloads = FindViewById<TextView>(Resource.Id.Downloads);
                TxtSells = FindViewById<TextView>(Resource.Id.Sells);
                TxtTags = FindViewById<TextView>(Resource.Id.Tags);

                //BuyLayout = FindViewById<LinearLayout>(Resource.Id.BuyLayouts);
                TxtLicenseType = FindViewById<TextView>(Resource.Id.TextLicenseType);
                BuyButton = FindViewById<Button>(Resource.Id.BuyButton);
                BuyButton.Tag = "Buy";
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
                    StoreImage.Click += StoreImageOnClick;
                    IconBack.Click += IconBackOnClick;
                    TxtMore.Click += TxtMoreOnClick;
                    BuyButton.Click += BuyButtonOnClick;
                    StoreAvatar.Click += StoreAvatarOnClick;
                    TxtUserName.Click += StoreAvatarOnClick;
                }
                else
                {
                    StoreImage.Click -= StoreImageOnClick;
                    IconBack.Click -= IconBackOnClick;
                    TxtMore.Click -= TxtMoreOnClick;
                    BuyButton.Click -= BuyButtonOnClick;
                    StoreAvatar.Click -= StoreAvatarOnClick;
                    TxtUserName.Click -= StoreAvatarOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public static StoreViewActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        #endregion

        #region Events

        private void StoreAvatarOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeEventClick = "OpenProfile";
                Finish();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void StoreImageOnClick(object sender, EventArgs e)
        {
            try
            {
                var media = AppTools.GetFile("", Methods.Path.FolderDiskImage, StoreData.Thumb.Split('/').Last(), StoreData.Thumb);
                if (media.Contains("http"))
                {
                    var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(media));
                    StartActivity(intent);
                }
                else
                {
                    var file2 = new Java.IO.File(media);
                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                    var intent = new Intent(Intent.ActionPick);
                    intent.SetAction(Intent.ActionView);
                    intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                    intent.SetDataAndType(photoUri, "image/*");
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                DialogType = "More";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                if (StoreData.UserId.ToString() == UserDetails.UserId)
                {
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Edit));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Delete));
                }

                arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Share));

                dialogList.Title(GetText(Resource.String.Lbl_More));
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

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
               Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BuyButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (BuyButton.Tag?.ToString() == "Buy" && Price != "0")
                {
                    DialogType = "Payment";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    if (AppSettings.ShowPaypal)
                        arrayAdapter.Add(GetString(Resource.String.Lbl_Paypal));

                    if (AppSettings.ShowCreditCard)
                        arrayAdapter.Add(GetString(Resource.String.Lbl_CreditCard));

                    if (AppSettings.ShowBankTransfer)
                        arrayAdapter.Add(GetString(Resource.String.Lbl_BankTransfer));

                    dialogList.Items(arrayAdapter);
                    dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else if (BuyButton.Tag?.ToString() == "Open")
                {
                    var media = AppTools.GetFile("", Methods.Path.FolderDiskImage, StoreData.Thumb.Split('/').Last(), StoreData.Thumb);
                    if (media.Contains("http"))
                    {
                        var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(media));
                        StartActivity(intent);
                    }
                    else
                    {
                        var file2 = new Java.IO.File(media);
                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                        var intent = new Intent(Intent.ActionPick);
                        intent.SetAction(Intent.ActionView);
                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                        intent.SetDataAndType(photoUri, "image/*");
                        StartActivity(intent);
                    }
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (requestCode == 246 && resultCode == Result.Ok)
                {
                    var storesItem = data.GetStringExtra("StoresItem") ?? "";
                    if (string.IsNullOrEmpty(storesItem)) return;
                    var dataObject = JsonConvert.DeserializeObject<StoreDataObject>(storesItem);
                    if (dataObject != null)
                    {
                        StoreData = dataObject;

                        StoreData.Tags = dataObject.Tags;
                        StoreData.Title = dataObject.Title;
                        StoreData.CategoryName = dataObject.CategoryName;
                        StoreData.Category = dataObject.Category;
                        StoreData.Thumb = dataObject.Thumb;

                        GlideImageLoader.LoadImage(this, dataObject.Thumb, StoreImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                        TxtStoreTitle.Text = Methods.FunString.DecodeString(dataObject.Title);
                        TxtCategory.Text = Methods.FunString.DecodeString(dataObject.CategoryName);
                        TxtTags.Text = StoreData.Tags;
                    }
                }
                else if (requestCode == InitPayPalPayment?.PayPalDataRequestCode)
                {
                    switch (resultCode)
                    {
                        case Result.Ok:
                            var confirmObj = data.GetParcelableExtra(PaymentActivity.ExtraResultConfirmation);
                            var configuration = Android.Runtime.Extensions.JavaCast<PaymentConfirmation>(confirmObj);
                            if (configuration != null)
                            {
                                //string createTime = configuration.ProofOfPayment.CreateTime;
                                //string intent = configuration.ProofOfPayment.Intent;
                                //string paymentId = configuration.ProofOfPayment.PaymentId;
                                //string state = configuration.ProofOfPayment.State;
                                //string transactionId = configuration.ProofOfPayment.TransactionId;

                                if (Methods.CheckConnectivity())
                                {
                                    (var apiStatus, var respond) = await RequestsAsync.Store.StoreBuy(StoreData.Id.ToString(), LicenseTypeId, Price);
                                    if (apiStatus == 200)
                                    {
                                        var fileName = StoreData.FullFile.Split('/').Last();
                                        AppTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDcimImage, fileName, StoreData.FullFile);

                                        BuyButton.Text = GetText(Resource.String.Lbl_Open);
                                        BuyButton.Tag = "Open";

                                        Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentCompletedSuccessfully), ToastLength.Long)?.Show();
                                    }
                                    else Methods.DisplayReportResult(this, respond);
                                }
                                else
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                }
                            }
                            break;
                        case Result.Canceled:
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long)?.Show();
                            break;
                    }
                }
                else if (requestCode == PaymentActivity.ResultExtrasInvalid)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Invalid), ToastLength.Long)?.Show();
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public async void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                var text = itemString.ToString();
                if (text == GetText(Resource.String.Lbl_Edit))
                {
                    //Open Edit Store
                    var intent = new Intent(this, typeof(EditStoreActivity));
                    intent.PutExtra("StoreId", StoreData.Id);
                    intent.PutExtra("StoreItem", JsonConvert.SerializeObject(StoreData));
                    StartActivityForResult(intent, 246);
                }
                else if (text == GetText(Resource.String.Lbl_Delete))
                {
                    DialogType = "Delete";

                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                    dialog.Title(Resource.String.Lbl_Warning);
                    dialog.Content(GetText(Resource.String.Lbl_DeleteStore));
                    dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else if (text == GetText(Resource.String.Lbl_Copy))
                {
                    Methods.CopyToClipboard(this, Client.WebsiteUrl + "/store/" + StoreData.Id);
                }
                else if (text == GetText(Resource.String.Lbl_Share))
                {
                    //Share Plugin same as video
                    if (!CrossShare.IsSupported) return;

                    await CrossShare.Current.Share(new ShareMessage
                    {
                        Title = Methods.FunString.DecodeString(StoreData.Title),
                        Text = "",
                        Url = Client.WebsiteUrl + "/store/" + StoreData.Id
                    });
                }
                if (text == GetString(Resource.String.Lbl_Paypal))
                {
                    InitPayPalPayment?.BtnPaypalOnClick(Price);
                }
                else if (text == GetString(Resource.String.Lbl_CreditCard))
                {
                    OpenIntentCreditCard();
                }
                else if (text == GetString(Resource.String.Lbl_BankTransfer))
                {
                    OpenIntentBankTransfer();
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
                if (DialogType == "Delete")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        // Send Api delete  
                        if (Methods.CheckConnectivity())
                        {
                            var data = HomeActivity.GetInstance()?.ExploreFragmentTheme2?.MAdapter?.StoreAdapter?.StoreList?.FirstOrDefault(a => a.Id == StoreData.Id);
                            if (data != null)
                            {
                                HomeActivity.GetInstance()?.ExploreFragmentTheme2?.MAdapter?.StoreAdapter?.StoreList.Remove(data);
                                HomeActivity.GetInstance()?.ExploreFragmentTheme2?.MAdapter?.StoreAdapter?.NotifyDataSetChanged();
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short)?.Show();
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Store.DeleteStore(StoreData.Id.ToString()) });

                            //close 
                            Finish();
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        }
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else
                {
                    if (p1 == DialogAction.Positive)
                    {

                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentCreditCard()
        {
            try
            {
                var intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                intent.PutExtra("Id", StoreData.Id.ToString());
                intent.PutExtra("Price", Price);
                intent.PutExtra("payType", "Store");
                intent.PutExtra("LicenseType", LicenseTypeId);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentBankTransfer()
        {
            try
            {
                var intent = new Intent(this, typeof(PaymentLocalActivity));
                intent.PutExtra("Id", StoreData.Id.ToString());
                intent.PutExtra("Price", Price);
                intent.PutExtra("payType", "Store");
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Data Store

        public void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetStoreById });
        }

        private async Task GetStoreById()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    (var apiStatus, var respond) = await RequestsAsync.Store.GetStoreById(StoreId);
                    if (apiStatus == 200)
                    {
                        if (respond is GetStoreObject result)
                        { 
                            GetDataStore(result.Data);
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadData()
        {
            try
            {
                StoreId = Intent?.GetStringExtra("StoreId") ?? "";
                StoreData = JsonConvert.DeserializeObject<StoreDataObject>(Intent?.GetStringExtra("storeData") ?? "");
                if (StoreData != null)
                { 
                    GetDataStore(StoreData);
                }

                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetDataStore(StoreDataObject storeData)
        {
            try
            {
                if (storeData != null)
                {
                    StoreData = storeData;
                    StoreId = StoreData.Id.ToString();
                    
                    GlideImageLoader.LoadImage(this, storeData.Thumb, StoreImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                    GlideImageLoader.LoadImage(this, storeData.Avatar, StoreAvatar, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                    TxtUserName.Text = Methods.FunString.DecodeString(storeData.Username);

                    TxtStoreTitle.Text = Methods.FunString.DecodeString(storeData.Title);
                    TxtCategory.Text = Methods.FunString.DecodeString(storeData.CategoryName);
                    TxtImageType.Text = storeData.Type;
                    TxtViews.Text = storeData.Views.ToString();
                    TxtDownloads.Text = storeData.Downloads.ToString();
                    TxtSells.Text = AppSettings.CurrencyIconStatic + storeData.Sells;
                    TxtTags.Text = storeData.Tags;

                    if (storeData.LicenseOptions?.LicenseOptionsClass != null)
                    {
                        if (storeData.LicenseOptions?.LicenseOptionsClass.RightsManagedLicense > 0)
                        {
                            LicenseTypeId = "rights_managed_license";
                            LicenseTypeText = GetText(Resource.String.Lbl_rights_managed_license);
                            Price = storeData.LicenseOptions?.LicenseOptionsClass.RightsManagedLicense.Value.ToString();
                        }
                        else if (storeData.LicenseOptions?.LicenseOptionsClass.EditorialUseLicense > 0)
                        {
                            LicenseTypeId = "editorial_use_license";
                            LicenseTypeText = GetText(Resource.String.Lbl_editorial_use_license);
                            Price = storeData.LicenseOptions?.LicenseOptionsClass.EditorialUseLicense.Value.ToString();
                        }
                        else if (storeData.LicenseOptions?.LicenseOptionsClass.RoyaltyFreeLicense > 0)
                        {
                            LicenseTypeId = "royalty_free_license";
                            LicenseTypeText = GetText(Resource.String.Lbl_royalty_free_license);
                            Price = storeData.LicenseOptions?.LicenseOptionsClass.RoyaltyFreeLicense.Value.ToString();
                        }
                        else if (storeData.LicenseOptions?.LicenseOptionsClass.RoyaltyFreeExtendedLicense > 0)
                        {
                            LicenseTypeId = "royalty_free_extended_license";
                            LicenseTypeText = GetText(Resource.String.Lbl_royalty_free_extended_license);
                            Price = storeData.LicenseOptions?.LicenseOptionsClass.RoyaltyFreeExtendedLicense.Value.ToString();
                        }
                        else if (storeData.LicenseOptions?.LicenseOptionsClass.CreativeCommonsLicense > 0)
                        {
                            LicenseTypeId = "creative_commons_license";
                            LicenseTypeText = GetText(Resource.String.Lbl_creative_commons_license);
                            Price = storeData.LicenseOptions?.LicenseOptionsClass.CreativeCommonsLicense.Value.ToString();
                        }
                        else if (storeData.LicenseOptions?.LicenseOptionsClass.PublicDomain > 0)
                        {
                            LicenseTypeId = "public_domain";
                            LicenseTypeText = GetText(Resource.String.Lbl_public_domain);
                            Price = storeData.LicenseOptions?.LicenseOptionsClass.PublicDomain.Value.ToString();
                        }
                        else if (storeData.LicenseOptions?.LicenseOptionsClass.Empty > 0)
                        {
                            LicenseTypeId = "";
                            LicenseTypeText = "";
                            Price = storeData.LicenseOptions?.LicenseOptionsClass.Empty.Value.ToString();
                        }
                        else
                        {
                            LicenseTypeId = "";
                            LicenseTypeText = "";
                            Price = "0";
                        }
                    }
                    else
                    {
                        LicenseTypeId = "";
                        LicenseTypeText = "";
                        Price = "0";
                    }

                    TxtLicenseType.Text = string.IsNullOrEmpty(LicenseTypeText) ? AppSettings.CurrencyIconStatic + Price : LicenseTypeText + " : " + AppSettings.CurrencyIconStatic + Price;

                    if (storeData.UserId.ToString() != UserDetails.UserId && storeData.IsPurchased != null && !storeData.IsPurchased.Value)
                    {
                        TxtLicenseType.Visibility = ViewStates.Visible;
                        BuyButton.Text = GetText(Resource.String.Lbl_Buy);
                        BuyButton.Tag = "Buy";
                    }
                    else 
                    {
                        TxtLicenseType.Visibility = ViewStates.Gone;
                        BuyButton.Text = GetText(Resource.String.Lbl_Open);
                        BuyButton.Tag = "Open";
                        AppTools.GetFile("", Methods.Path.FolderDiskImage, storeData.FullFile.Split('/').Last(), storeData.Thumb); 
                    } 

                    AppTools.GetFile("", Methods.Path.FolderDiskImage, storeData.Thumb.Split('/').Last(), storeData.Thumb);
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