using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using Java.Lang;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using Exception = System.Exception;

namespace PixelPhoto.Activities.Store
{
    public class FilterStoreDialogFragment : BottomSheetDialogFragment, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private TextView IconBack, IconTitle, IconTags, IconCategory, IconLicenseType, IconPrice;
        private EditText TxtTitle , TxtTags , TxtCategory , TxtLicenseType , TxtPriceMin , TxtPriceMax;
        private Button BtnApply;
        private string TypeDialog , LicenseTypeId , CategoryId;
        private readonly StoreActivity ContextStore;

        #endregion
         
        public FilterStoreDialogFragment(StoreActivity storeActivity)
        {
            ContextStore = storeActivity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark_Base) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Base);
                // clone the inflater using the ContextThemeWrapper
                var localInflater = inflater.CloneInContext(contextThemeWrapper);

                var view = localInflater?.Inflate(Resource.Layout.ButtomSheetStoreFilter, container, false);

                InitComponent(view);

                IconBack.Click += IconBackOnClick; 
                BtnApply.Click += BtnApplyOnClick;
                TxtCategory.Touch += TxtCategoryOnTouch;
                TxtLicenseType.Touch += TxtLicenseTypeOnTouch;

                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
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

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                IconBack = view.FindViewById<TextView>(Resource.Id.IconBack);

                IconTitle = view.FindViewById<TextView>(Resource.Id.IconTitle);
                TxtTitle = view.FindViewById<EditText>(Resource.Id.TitleEditText);

                IconTags = view.FindViewById<TextView>(Resource.Id.IconTags);
                TxtTags = view.FindViewById<EditText>(Resource.Id.TagsEditText);

                IconCategory = view.FindViewById<TextView>(Resource.Id.IconCategory);
                TxtCategory = view.FindViewById<EditText>(Resource.Id.CategoryEditText);

                IconLicenseType = view.FindViewById<TextView>(Resource.Id.IconLicenseType);
                TxtLicenseType = view.FindViewById<EditText>(Resource.Id.LicenseTypeEditText);

                IconPrice = view.FindViewById<TextView>(Resource.Id.IconPrice);
                TxtPriceMin = view.FindViewById<EditText>(Resource.Id.MinimumEditText);
                TxtPriceMax = view.FindViewById<EditText>(Resource.Id.MaximumEditText);

                BtnApply = view.FindViewById<Button>(Resource.Id.ApplyButton);
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, IonIconsFonts.ArrowBack);

                IconTitle.Visibility = ViewStates.Gone;
                IconTags.Visibility = ViewStates.Gone;
                IconCategory.Visibility = ViewStates.Gone;
                IconLicenseType.Visibility = ViewStates.Gone;
                IconPrice.Visibility = ViewStates.Gone;

                Methods.SetColorEditText(TxtTitle, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTags, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCategory, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLicenseType, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPriceMin, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPriceMax, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(TxtCategory);
                Methods.SetFocusable(TxtLicenseType);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save data 
        private void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                UserDetails.StoreTitle = TxtTitle.Text;
                UserDetails.StoreTags = TxtTags.Text;
                UserDetails.StoreCategory = CategoryId;
                UserDetails.StoreLicenseType = LicenseTypeId;
                UserDetails.StorePriceMin = TxtPriceMin.Text;
                UserDetails.StorePriceMax = TxtPriceMax.Text;

                ContextStore.StoreTab.MAdapter.StoreList.Clear();
                ContextStore.StoreTab.MAdapter.NotifyDataSetChanged();

                ContextStore.StoreTab.SwipeRefreshLayout.Refreshing = true;
                ContextStore.StoreTab.SwipeRefreshLayout.Enabled = true;

                ContextStore.StoreTab.MainScrollEvent.IsLoading = false;

                ContextStore.StoreTab.MRecycler.Visibility = ViewStates.Visible;
                ContextStore.StoreTab.EmptyStateLayout.Visibility = ViewStates.Gone;

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ContextStore.GetStore() });
                else
                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();

                Dismiss();
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

                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

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
                 
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

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

    }
}