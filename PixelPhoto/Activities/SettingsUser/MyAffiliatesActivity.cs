using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using PixelPhoto.Activities.Base;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient;
using PixelPhoto.Library.Anjo.Share;
using PixelPhoto.Library.Anjo.Share.Abstractions;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.SettingsUser
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MyAffiliatesActivity : BaseActivity
    {
        #region Variables Basic

        private ImageView ImageUser;
        private TextView TxtLink, TxtMyAffiliates;
        private Button BtnShare;

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
                SetContentView(Resource.Layout.MyAffiliatesLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                AdsGoogle.Ad_AdMobNative(this);
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
                DestroyBasic();
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
                ImageUser = FindViewById<ImageView>(Resource.Id.ImageUser);
                TxtLink = FindViewById<TextView>(Resource.Id.linkText);
                TxtMyAffiliates = FindViewById<TextView>(Resource.Id.myAffiliatesText);
                BtnShare = FindViewById<Button>(Resource.Id.cont);

                GlideImageLoader.LoadImage(this, UserDetails.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                //https://demo.pixelphotoscript.com/?ref=waelanjo
                TxtLink.Text = Client.WebsiteUrl + "/?ref=" + UserDetails.Username;

                if (!string.IsNullOrEmpty(ListUtils.SettingsSiteList?.AmountPercentRef) && ListUtils.SettingsSiteList?.AmountPercentRef != "0")
                {
                    TxtMyAffiliates.Text = GetString(Resource.String.Lbl_EarnUpTo) + "%" + ListUtils.SettingsSiteList?.AmountPercentRef + " " + GetString(Resource.String.Lbl_forEachUserYourReferToUs) + " !";
                }
                else if (!string.IsNullOrEmpty(ListUtils.SettingsSiteList?.AmountRef) && ListUtils.SettingsSiteList?.AmountRef != "0")
                { 
                    TxtMyAffiliates.Text = GetString(Resource.String.Lbl_EarnUpTo) + " " + AppSettings.CurrencyIconStatic + ListUtils.SettingsSiteList?.AmountRef + " " + GetString(Resource.String.Lbl_forEachUserYourReferToUs) + " !";
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
                    toolbar.Title = GetText(Resource.String.Lbl_MyAffiliates);
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
                    BtnShare.Click += BtnShareOnClick;
                    TxtLink.LongClick += TxtLinkOnLongClick;
                }
                else
                {
                    BtnShare.Click -= BtnShareOnClick;
                    TxtLink.LongClick -= TxtLinkOnLongClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        private void DestroyBasic()
        {
            try
            {
                ImageUser = null!;
                TxtLink = null!;
                TxtMyAffiliates = null!;
                BtnShare = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        //Copy
        private void TxtLinkOnLongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                Methods.CopyToClipboard(this, TxtLink.Text);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Share
        private async void BtnShareOnClick(object sender, EventArgs e)
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = UserDetails.Username,
                    Text = "",
                    Url = TxtLink.Text
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}