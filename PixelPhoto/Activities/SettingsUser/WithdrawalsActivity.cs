using System;
using System.Globalization;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidHUD;
using PixelPhoto.Activities.Base;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.SettingsUser
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class WithdrawalsActivity : BaseActivity
    {
        #region Variables Basic
      
        private TextView TxtMyBalance,  IconAmount, IconPayPalEmail;
        private EditText TxtAmount, TxtPayPalEmail;
        private Button BtnRequestWithdrawal;
        private double CountBalance;
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
                SetContentView(Resource.Layout.WithdrawalsLayout);
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                Get_Data_User();
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
                TxtMyBalance = FindViewById<TextView>(Resource.Id.myBalance);

                IconAmount = FindViewById<TextView>(Resource.Id.IconAmount);
                TxtAmount = FindViewById<EditText>(Resource.Id.AmountEditText);

                IconPayPalEmail = FindViewById<TextView>(Resource.Id.IconPayPalEmail);
                TxtPayPalEmail = FindViewById<EditText>(Resource.Id.PayPalEmailEditText);

                BtnRequestWithdrawal = FindViewById<Button>(Resource.Id.RequestWithdrawalButton);

                Methods.SetColorEditText(TxtAmount, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPayPalEmail, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconAmount, FontAwesomeIcon.MoneyBillWave);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeBrands, IconPayPalEmail, FontAwesomeIcon.CcPaypal);

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);
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
                    toolbar.Title = GetText(Resource.String.Lbl_Withdrawals);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
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
                    BtnRequestWithdrawal.Click += BtnRequestWithdrawalOnClick;
                }
                else
                {
                    BtnRequestWithdrawal.Click -= BtnRequestWithdrawalOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private async void BtnRequestWithdrawalOnClick(object sender, EventArgs e)
        {
            try
            {
                if (CountBalance < Convert.ToDouble(TxtAmount.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_ThereIsNoBalance), ToastLength.Long)?.Show();
                    return;
                }

                if (Convert.ToDouble(TxtAmount.Text) < AppSettings.WithdrawalAmount)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CantRequestWithdrawals), ToastLength.Long)?.Show();
                    return;
                }
                 
                if (string.IsNullOrEmpty(TxtPayPalEmail.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtAmount.Text.Replace(" ", "")))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Long)?.Show();
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                     
                    var (apiStatus, respond) = await RequestsAsync.Global.WithdrawAsync(TxtPayPalEmail.Text, TxtAmount.Text , UserDetails.UserId);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            Console.WriteLine(result.Message);
                            AndHUD.Shared.Dismiss(this);
                            Toast.MakeText(this, GetText(Resource.String.Lbl_RequestSentWithdrawals), ToastLength.Long)?.Show();

                            Finish();
                        }
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

        #endregion

        private async void Get_Data_User()
        {
            try
            {
                if (ListUtils.MyProfileList.Count == 0)
                    await ApiRequest.GetProfile_Api(this);

                var local = ListUtils.MyProfileList.FirstOrDefault();
                if (local != null)
                {
                    CountBalance = Convert.ToDouble(local.Balance);
                    TxtMyBalance.Text = GetText(Resource.String.Lbl_Withdrawals_SubText1) + " " + AppSettings.CurrencyIconStatic + CountBalance.ToString(CultureInfo.InvariantCulture) + ", " 
                                        + GetText(Resource.String.Lbl_Withdrawals_SubText2) + " " + AppSettings.CurrencyIconStatic + "50" /*ListUtils.SettingsSiteList?.MWithdrawal*/;

                    TxtPayPalEmail.Text = local.PaypalEmail;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}