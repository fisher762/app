using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using PixelPhoto.Activities.Default;
using PixelPhoto.Activities.Tabbes;
using Android.Widget;
using AndroidX.AppCompat.App;
using Java.Lang;
using PixelPhoto.Activities.Funding;
using PixelPhoto.Activities.Store;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using Exception = System.Exception;

namespace PixelPhoto.Activities
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/SplashScreenTheme", NoHistory = true, MainLauncher = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault }, DataSchemes = new[] { "http", "https" }, DataHost = "@string/ApplicationUrlWeb", AutoVerify = false)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault }, DataSchemes = new[] { "http", "https" }, DataHost = "@string/ApplicationUrlWeb", DataPathPrefixes = new[] { "", "/welcome/", "/signup/", "/post/", "/store/", "/funding/" }, AutoVerify = false)]
    public class SplashScreenActivity : AppCompatActivity
    { 
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                 
                new Handler(Looper.MainLooper).Post(new Runnable(FirstRunExcite));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FirstRunExcite()
        {
            try
            { 
                if (!string.IsNullOrEmpty(AppSettings.Lang))
                {
                    LangController.SetApplicationLang(this, AppSettings.Lang);
                }
                else
                {
                    #pragma warning disable 618
                    UserDetails.LangName = (int)Build.VERSION.SdkInt < 25 ? Resources?.Configuration?.Locale?.Language.ToLower() : Resources?.Configuration?.Locales.Get(0)?.Language.ToLower() ?? Resources?.Configuration?.Locale?.Language.ToLower();
                    #pragma warning restore 618

                    LangController.SetApplicationLang(this, UserDetails.LangName);
                }

                if (!string.IsNullOrEmpty(UserDetails.AccessToken))
                {
                    if (Intent?.Data?.Path != null)
                    {
                        if (Intent.Data.Path.Contains("signup") && UserDetails.Status != "Active" && UserDetails.Status != "Pending")
                        {
                            StartActivity(new Intent(this, typeof(RegisterActivity)));
                        }
                        else if (Intent.Data.Path.Contains("welcome") && UserDetails.Status != "Active" && UserDetails.Status != "Pending")
                        {
                            StartActivity(new Intent(this, AppSettings.ShowFirstPage ? typeof(FirstActivity) : typeof(LoginActivity)));
                        }  
                        else if (Intent.Data.Path.Contains("post") && (UserDetails.Status == "Active" || UserDetails.Status == "Pending"))
                        {
                            var postId = Intent.Data.Path.Split("/").Last().Replace("/", "");

                            var intent = new Intent(this, typeof(HomeActivity));
                            intent.PutExtra("DeepLinks", "OpenPost"); 
                            intent.PutExtra("Id", postId); 
                            StartActivity(intent);
                        }  
                        else if (Intent.Data.Path.Contains("store") && (UserDetails.Status == "Active" || UserDetails.Status == "Pending"))
                        {
                            var storeId = Intent.Data.Path.Split("/").Last().Replace("/", "");
                             
                            var intent = new Intent(this, typeof(StoreViewActivity));
                            intent.PutExtra("StoreId", storeId);
                            //intent.PutExtra("storeData", JsonConvert.SerializeObject(item));
                            StartActivity(intent);

                        } 
                        else if (Intent.Data.Path.Contains("funding") && (UserDetails.Status == "Active" || UserDetails.Status == "Pending"))
                        {
                            var fundingId = Intent.Data.Path.Split("/").Last().Replace("/", "");

                            var intent = new Intent(this, typeof(FundingViewActivity));
                            intent.PutExtra("FundingId", fundingId);
                            //intent.PutExtra("ItemObject", JsonConvert.SerializeObject(item));
                            StartActivity(intent);
                        } 
                        else
                        {
                            switch (UserDetails.Status)
                            {
                                case "Active":
                                case "Pending":
                                    StartActivity(new Intent(this, typeof(HomeActivity)));
                                    break;
                                default:
                                    StartActivity(new Intent(this, AppSettings.ShowFirstPage ? typeof(FirstActivity) : typeof(LoginActivity)));
                                    break;
                            }
                        }
                    }
                    else
                    {
                        switch (UserDetails.Status)
                        {
                            case "Active":
                            case "Pending":
                                StartActivity(new Intent(this, typeof(HomeActivity)));
                                break;
                            default:
                                StartActivity(new Intent(this, AppSettings.ShowFirstPage ? typeof(FirstActivity) : typeof(LoginActivity)));
                                break;
                        }
                    } 
                }
                else
                {
                    StartActivity(new Intent(this, AppSettings.ShowFirstPage ? typeof(FirstActivity) : typeof(LoginActivity)));
                }

                OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
                Finish(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                Toast.MakeText(this, exception.Message, ToastLength.Short)?.Show();
            } 
        }
    }
}