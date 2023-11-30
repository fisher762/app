using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using PixelPhoto.Activities.Base;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.RestCalls;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.AddPost
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class EditPostActivity : BaseActivity
    {
        #region Variables Basic

        private Toolbar Toolbar;
        private TextView SaveTextView;
        private EditText DataEditText;
        private string TextPost, PostId;
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
                SetContentView(Resource.Layout.EditPostLayout);

                PostId = Intent?.GetStringExtra("IdPost");
                TextPost = Intent?.GetStringExtra("TextPost");
 
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                DataEditText.Text = TextPost;
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

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    var resultIntent = new Intent();
                    SetResult(Result.Canceled, resultIntent);
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
                SaveTextView = FindViewById<TextView>(Resource.Id.toolbar_title);
                DataEditText = FindViewById<EditText>(Resource.Id.editTxtEmail); 

                SaveTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                 
                Methods.SetColorEditText(DataEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                    Toolbar.Title = GetText(Resource.String.Lbl_EditPost);
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
                    SaveTextView.Click += SaveTextViewOnClick;
                }
                else
                {
                    SaveTextView.Click -= SaveTextViewOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private async void SaveTextViewOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (string.IsNullOrEmpty(DataEditText.Text))
                    {
                        DataEditText.Text = " ";
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    //Send Api
                    (var apiStatus, var respond) = await RequestsAsync.Post.EditPosts(PostId, DataEditText.Text);
                    if (apiStatus == 200)
                    {
                        AndHUD.Shared.Dismiss(this);
                        Toast.MakeText(this, GetString(Resource.String.Lbl_SuccessfullyEdited), ToastLength.Short)?.Show();

                        // put the String to pass back into an Intent and close this activity
                        RunOnUiThread(() =>
                        {
                            var resultIntent = new Intent();
                            resultIntent.PutExtra("PostId", PostId);
                            resultIntent.PutExtra("NewTextPost", DataEditText.Text);
                            SetResult(Result.Ok, resultIntent);
                            Finish();
                        });
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
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        } 
    }
}