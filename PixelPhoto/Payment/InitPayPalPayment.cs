using System;
using Android.App;
using Android.Content;
using Java.Math;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient;
using Xamarin.PayPal.Android;

namespace PixelPhoto.Payment
{
    public class InitPayPalPayment
    {
        private readonly Activity ActivityContext;
        private static PayPalConfiguration PayPalConfig;
        private PayPalPayment PayPalPayment;
        private Intent IntentService;
        public readonly int PayPalDataRequestCode = 7171;

        public InitPayPalPayment(Activity activity)
        {
            ActivityContext = activity;
        }

        //Paypal
        public void BtnPaypalOnClick(string price)
        {
            try
            {
                var init = InitPayPal(price);
                if (!init)
                    return;

                var intent = new Intent(ActivityContext, typeof(PaymentActivity));
                intent.PutExtra(PayPalService.ExtraPaypalConfiguration, PayPalConfig);
                intent.PutExtra(PaymentActivity.ExtraPayment, PayPalPayment);
                ActivityContext.StartActivityForResult(intent, PayPalDataRequestCode);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private bool InitPayPal(string price)
        {
            try
            {
                //PayerID
                var currency = AppSettings.CurrencyCodeStatic;
                var paypalClintId = "";
                var option = ListUtils.SettingsSiteList;
                if (option != null)
                {
                    currency = /*option.PaypalCurrency ??*/ AppSettings.CurrencyCodeStatic;
                    paypalClintId = option.PaypalId;
                }

                if (string.IsNullOrEmpty(paypalClintId))
                    return false;

                PayPalConfig = new PayPalConfiguration()
                    .ClientId(paypalClintId)
                    .LanguageOrLocale(AppSettings.Lang)
                    .MerchantName(AppSettings.ApplicationName)
                    .MerchantPrivacyPolicyUri(Android.Net.Uri.Parse(Client.WebsiteUrl + "/terms/privacy-policy"));

                switch (ListUtils.SettingsSiteList?.PaypalMode)
                {
                    case "sandbox":
                        PayPalConfig.Environment(PayPalConfiguration.EnvironmentSandbox);
                        break;
                    case "live":
                        PayPalConfig.Environment(PayPalConfiguration.EnvironmentProduction);
                        break;
                    default:
                        PayPalConfig.Environment(PayPalConfiguration.EnvironmentProduction);
                        break;
                }

                PayPalPayment = new PayPalPayment(new BigDecimal(price), currency, "Pay the card", PayPalPayment.PaymentIntentSale);

                IntentService = new Intent(ActivityContext, typeof(PayPalService));
                IntentService.PutExtra(PayPalService.ExtraPaypalConfiguration, PayPalConfig);
                ActivityContext.StartService(IntentService);
                return true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public void StopPayPalService()
        {
            try
            {
                if (PayPalConfig != null)
                {
                    ActivityContext.StopService(new Intent(ActivityContext, typeof(PayPalService)));
                    PayPalConfig = null!;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}