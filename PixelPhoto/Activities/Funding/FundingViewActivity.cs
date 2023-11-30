using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.Base;
using PixelPhoto.Activities.Posts.Extras;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.Payment;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Funding;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using PixelPhoto.Library.Anjo.Share;
using PixelPhoto.Library.Anjo.Share.Abstractions;
using Xamarin.PayPal.Android;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.Funding
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class FundingViewActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView ImageUser, ImageFunding, MoreButton;
        private TextView TxtUsername, TxtTime, TxtTitle, TxtDescription, TxtFundRaise;
        private ProgressBar ProgressBar;
        private Button BtnDonate;
        private FundingDataObject DataObject;
        private InitPayPalPayment InitPayPalPayment;
        private string CodeName , FundingId;
        private static FundingViewActivity Instance;

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
                SetContentView(Resource.Layout.FundingViewLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                if (AppSettings.ShowPaypal)
                    InitPayPalPayment = new InitPayPalPayment(this);

                LoadData();
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
                ImageUser = FindViewById<ImageView>(Resource.Id.imageAvatar);
                MoreButton = FindViewById<ImageView>(Resource.Id.moreButton);
                ImageFunding = FindViewById<ImageView>(Resource.Id.imageFunding);

                TxtUsername = FindViewById<TextView>(Resource.Id.username);
                TxtTime = FindViewById<TextView>(Resource.Id.time);
                TxtTitle = FindViewById<TextView>(Resource.Id.title);
                TxtDescription = FindViewById<TextView>(Resource.Id.description);
                TxtFundRaise = FindViewById<TextView>(Resource.Id.fund_raise);

                BtnDonate = FindViewById<Button>(Resource.Id.DonateButton);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

                TxtUsername.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                TxtTime.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                    toolbar.Title = " ";
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
                    MoreButton.Click += MoreButtonOnClick;
                    BtnDonate.Click += BtnDonateOnClick;
                }
                else
                {
                    MoreButton.Click -= MoreButtonOnClick;
                    BtnDonate.Click -= BtnDonateOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        
        public static FundingViewActivity GetInstance()
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

        private void MoreButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Share));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));

                var owner = DataObject.UserId.ToString() == UserDetails.UserId;
                if (owner)
                {
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Edit));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Delete));
                }
                else
                {
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Report));
                }

                dialogList.Title(GetText(Resource.String.Lbl_More));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //Event Menu >> Delete
        private void DeleteEvent()
        {
            try
            {
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                dialog.Title(Resource.String.Lbl_Warning);
                dialog.Content(GetText(Resource.String.Lbl_DeleteFunding));
                dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                {
                    try
                    {
                        // Send Api delete  
                        if (Methods.CheckConnectivity())
                        {
                            var adapterGlobal = PRecyclerView.GetInstance()?.NativeFeedAdapter;
                            var diff = adapterGlobal?.PostList;
                            var dataGlobal = diff?.FirstOrDefault(a => a.Type == "Funding");
                            if (dataGlobal != null)
                            {
                                adapterGlobal.NotifyItemChanged(diff.IndexOf(dataGlobal), "FundingRefresh");
                            }

                            var dataFunding = ListUtils.FundingList?.FirstOrDefault(a => a.Id == DataObject.Id);
                            if (dataFunding != null)
                            {
                                ListUtils.FundingList.Remove(dataFunding);
                                FundingActivity.GetInstance().MAdapter.NotifyItemRemoved(FundingActivity.GetInstance().MAdapter.FundingList.IndexOf(dataFunding));
                            }

                            var dataFunding2 = FundingActivity.GetInstance()?.MAdapter?.FundingList?.FirstOrDefault(a => a.Id == DataObject.Id);
                            if (dataFunding2 != null)
                            {
                                FundingActivity.GetInstance()?.MAdapter?.FundingList.Remove(dataFunding2);
                                FundingActivity.GetInstance().MAdapter.NotifyItemRemoved(FundingActivity.GetInstance().MAdapter.FundingList.IndexOf(dataFunding2));
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_FundingSuccessfullyDeleted), ToastLength.Short)?.Show();
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Funding.DeleteFunding(DataObject.Id.ToString()) });
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.ItemsCallback(this).Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Report
        private void ReportEvent()
        {
            try
            {
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                dialog.Title(Resource.String.Lbl_ReportThisFundRequest);
                dialog.Input(Resource.String.Lbl_Message, 0, false, async (materialDialog, s) =>
                {
                    try
                    {
                        if (s.Length <= 0) return;
                        var message = s.ToString();

                        (var apiStatus, var respond) = await RequestsAsync.Funding.ReportFundingAsync(DataObject.Id.ToString(), message);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageObject message1)
                            {
                                Console.WriteLine(message1.Message);
                                Toast.MakeText(this, GetText(Resource.String.Lbl_YourReportHasBeenSent), ToastLength.Long)?.Show(); 
                            }
                        }
                        else
                        { 
                            Methods.DisplayReportResult(this, respond);
                        } 
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.InputType(InputTypes.TextFlagMultiLine);
                dialog.PositiveText(GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                dialog.Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Edit
        private void EditEvent()
        {
            try
            {
                var intent = new Intent(this, typeof(EditFundingActivity));
                intent.PutExtra("FundingObject", JsonConvert.SerializeObject(DataObject));
                StartActivityForResult(intent, 253);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Copy Link
        private void CopyLinkEvent()
        {
            try
            {
                Methods.CopyToClipboard(this, Client.WebsiteUrl + "/funding/" + DataObject.HashedId);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private async void ShareEvent()
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = DataObject.Title,
                    Text = DataObject.Description,
                    Url = Client.WebsiteUrl + "/funding/" + DataObject.HashedId
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Donate 
        private void BtnDonateOnClick(object sender, EventArgs e)
        {
            try
            {
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                dialog.Title(Resource.String.Lbl_Donate);
                dialog.Input(Resource.String.Lbl_DonateCode, 0, false, (materialDialog, s) =>
                {
                    try
                    {
                        if (s.Length <= 0) return;
                        CodeName = s.ToString();
                      
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
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialog.InputType(InputTypes.ClassText);
                dialog.PositiveText(GetText(Resource.String.Lbl_Send)).OnPositive(this);
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show();
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

                if (requestCode == 253 && resultCode == Result.Ok)
                {
                    if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                    var item = JsonConvert.DeserializeObject<FundingDataObject>(data.GetStringExtra("itemData"));
                    if (item != null)
                    {
                        DataObject = item;

                        TxtUsername.Text = Methods.FunString.DecodeString(item.UserData.Name);

                        TxtTime.Text = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(Convert.ToInt32(item.Time), true);

                        TxtTitle.Text = Methods.FunString.DecodeString(item.Title);

                        if (!string.IsNullOrEmpty(item.Description) || !string.IsNullOrWhiteSpace(item.Description))
                        {
                            TxtDescription.Text = Methods.FunString.DecodeString(item.Description);
                            TxtDescription.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            TxtDescription.Visibility = ViewStates.Gone;
                        }
                        
                        ProgressBar.Progress = Convert.ToInt32(item.Bar);

                        //$0 Raised of $1000000
                        TxtFundRaise.Text = AppSettings.CurrencyFundingPriceStatic + item.Raised.ToString(CultureInfo.InvariantCulture) + " " + GetString(Resource.String.Lbl_RaisedOf) + " " + AppSettings.CurrencyFundingPriceStatic + item.Amount;
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
                                    (var apiStatus, var respond) = await RequestsAsync.Funding.FundingPay(DataObject.Id.ToString(), CodeName);
                                    if (apiStatus == 200)
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_Donated), ToastLength.Long)?.Show();
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

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                var text = itemString.ToString();
                if (text == GetString(Resource.String.Lbl_Share))
                {
                    ShareEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Copy))
                {
                    CopyLinkEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Edit))
                {
                    EditEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Delete))
                {
                    DeleteEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Report))
                {
                    ReportEvent();
                } 
                else if (text == GetString(Resource.String.Lbl_Paypal))
                {
                    InitPayPalPayment.BtnPaypalOnClick(CodeName);
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

        private void OpenIntentCreditCard()
        {
            try
            {
                var intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                intent.PutExtra("Id", DataObject.Id.ToString());
                intent.PutExtra("Price", CodeName);
                intent.PutExtra("payType", "Funding");
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
                intent.PutExtra("Id", DataObject.Id.ToString());
                intent.PutExtra("Price", CodeName);
                intent.PutExtra("payType", "Funding");
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Data Funding

        public void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetFundingById });
        }

        private async Task GetFundingById()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    (var apiStatus, var respond) = await RequestsAsync.Funding.GetFundingByIdAsync(FundingId);
                    if (apiStatus == 200)
                    {
                        if (respond is GetFundingByIdObject result)
                        {
                            GetDataFunding(result.Data);
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
                FundingId = Intent?.GetStringExtra("FundingId") ?? "";
                DataObject = JsonConvert.DeserializeObject<FundingDataObject>(Intent?.GetStringExtra("ItemObject") ?? "");
                if (DataObject != null)
                {
                    GetDataFunding(DataObject);
                }

                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetDataFunding(FundingDataObject dataObject)
        {
            try
            {
                if (dataObject != null)
                {
                    DataObject = dataObject;
                    FundingId = DataObject.Id.ToString();
                     
                    GlideImageLoader.LoadImage(this, dataObject.UserData.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    GlideImageLoader.LoadImage(this, dataObject.Image, ImageFunding, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    TxtUsername.Text = Methods.FunString.DecodeString(dataObject.UserData.Name);
                    TxtTime.Text = Methods.Time.TimeAgo(Convert.ToInt32(dataObject.Time), false);
                    TxtTitle.Text = Methods.FunString.DecodeString(dataObject.Title);

                    if (!string.IsNullOrEmpty(dataObject.Description) || !string.IsNullOrWhiteSpace(dataObject.Description))
                    {
                        TxtDescription.Text = Methods.FunString.DecodeString(dataObject.Description);
                        TxtDescription.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        TxtDescription.Visibility = ViewStates.Gone;
                    }

                    ProgressBar.Progress = Convert.ToInt32(dataObject.Bar);

                    //$0 Raised of $1000000
                    TxtFundRaise.Text = AppSettings.CurrencyFundingPriceStatic + dataObject.Raised.ToString(CultureInfo.InvariantCulture) + GetString(Resource.String.Lbl_RaisedOf) + " " + AppSettings.CurrencyFundingPriceStatic + dataObject.Amount;
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