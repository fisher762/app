using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using Java.Lang;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;

namespace PixelPhoto.Activities.SettingsUser.Privacy
{
    public class SettingPrivacyPrefcFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private Preference PrivacyCanViewProfile, PrivacyCanMessagePref;
        //private SwitchPreferenceCompat PrivacyShareShowYourProfilePref;
        private string CanViewProfile = "0", CanMessagePref = "0";
        private readonly Activity ActivityContext;
        private string TypeDialog;
       
        #endregion

        #region General

        public SettingPrivacyPrefcFragment(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.SettingsThemeDark) : new ContextThemeWrapper(Activity, Resource.Style.SettingsTheme);

                // clone the inflater using the ContextThemeWrapper
                var localInflater = inflater.CloneInContext(contextThemeWrapper);

                var view = base.OnCreateView(localInflater, container, savedInstanceState);

                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            try
            {

                // Create your fragment here
                AddPreferencesFromResource(Resource.Xml.SettingsPrefs_Privacy);

                InitComponent();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

                //Add OnChange event to Preferences
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                PreferenceScreen.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);

                //Close OnChange event to Preferences
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
                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

                PrivacyCanViewProfile = FindPreference("CanViewProfile_key");
                PrivacyCanMessagePref = FindPreference("CanDirectMessage_key");
                //PrivacyShareShowYourProfilePref = (SwitchPreferenceCompat)FindPreference("ShowYourProfile_key");
                //PrivacyShareShowYourProfilePref.IconSpaceReserved = false;

                //Update Preferences data on Load
                OnSharedPreferenceChanged(MainSettings.SharedData, "CanViewProfile_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "CanDirectMessage_key");
                //OnSharedPreferenceChanged(MainSettings.SharedData, "ShowYourProfile_key");
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
                     //PrivacyShareShowYourProfilePref.PreferenceChange += PrivacyShareShowYourProfilePrefOnPreferenceChange;

                }
                else
                {
                     //PrivacyShareShowYourProfilePref.PreferenceChange -= PrivacyShareShowYourProfilePrefOnPreferenceChange;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        /*private void PrivacyShareShowYourProfilePrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                    var etp = (SwitchPreferenceCompat)sender;
                    var value = eventArgs.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked)
                    {
                        if (dataUser != null) dataUser.SearchEngines = "1";
                        ShowYourProfilePref = "1";
                    }
                    else
                    {
                        if (dataUser != null) dataUser.SearchEngines = "0";
                        ShowYourProfilePref = "0";
                    }

                    if (dataUser != null) dataUser.SearchEngines = ShowYourProfilePref;

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"search_engines", ShowYourProfilePref}
                        };
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.SaveSettings(dataPrivacy) });
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }*/
         
        #endregion

        //On Change 
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            try
            {
                var dataUser = ListUtils.MyProfileList.FirstOrDefault();

                if (key.Equals("CanViewProfile_key"))
                {
                    // Set summary to be the user-description for the selected value
                    var etp = FindPreference("CanViewProfile_key");

                    var getValue = MainSettings.SharedData.GetString("CanViewProfile_key", dataUser?.PPrivacy ?? string.Empty);
                    if (getValue == "0")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_No_body);
                        CanViewProfile = "0";
                    }
                    else if (getValue == "1")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_Followers);
                        CanViewProfile = "1";
                    }
                    else if (getValue == "2")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_Everyone);
                        CanViewProfile = "2";
                    }
                    else
                    { 
                        CanViewProfile = getValue;
                    }
                }

                else if (key.Equals("CanDirectMessage_key"))
                {
                    // Set summary to be the user-description for the selected value
                    var etp = FindPreference("CanDirectMessage_key");

                    var getValue = MainSettings.SharedData.GetString("CanDirectMessage_key", dataUser?.CPrivacy ?? string.Empty);
                    if (getValue == "0")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_Everyone);
                        CanMessagePref = "0";
                    }
                    else if (getValue == "1")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_People_i_Follow);
                        CanMessagePref = "1";
                    } 
                    else
                    {
                        CanMessagePref = getValue;
                    }
                }  
                /*else if (key.Equals("ShowYourProfile_key"))
                {
                    var getValue = MainSettings.SharedData.GetBoolean("ShowYourProfile_key", true);
                    //PrivacyShareShowYourProfilePref.Checked = getValue;
                }*/
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public override bool OnPreferenceTreeClick(Preference preference)
        {
            try
            {
                switch (preference.Key)
                {
                    case "CanViewProfile_key":
                    {
                        TypeDialog = "CanViewProfile";

                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                        dialogList.Title(Resource.String.Lbl_CanViewProfile);

                        arrayAdapter.Add(GetText(Resource.String.Lbl_No_body)); //>> value = 0
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Followers)); //>> value = 1
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 2

                        dialogList.Items(arrayAdapter);
                        dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(new MyMaterialDialog());
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                        break;
                    }
                    case "CanDirectMessage_key":
                    {
                        TypeDialog = "CanDirectMessage";

                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                        dialogList.Title(Resource.String.Lbl_CanDirectMessage);

                        arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 0
                        arrayAdapter.Add(GetText(Resource.String.Lbl_People_i_Follow)); //>> value = 1

                        dialogList.Items(arrayAdapter);
                        dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(new MyMaterialDialog());
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                        break;
                    }
                }

                return base.OnPreferenceTreeClick(preference);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return base.OnPreferenceTreeClick(preference);
            }
        }

        #region MaterialDialog
         
        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                var text = itemString.ToString();
                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                 
                switch (TypeDialog)
                {
                    case "CanViewProfile":
                    {
                        if (text == GetString(Resource.String.Lbl_No_body))
                        {
                            MainSettings.SharedData.Edit()?.PutString("CanViewProfile_key", "0")?.Commit();
                            PrivacyCanViewProfile.Summary = text;
                            CanViewProfile = "0";
                        }
                        else if (text == GetString(Resource.String.Lbl_Followers))
                        {
                            MainSettings.SharedData.Edit()?.PutString("CanViewProfile_key", "1")?.Commit();
                            PrivacyCanViewProfile.Summary = text;
                            CanViewProfile = "1";
                        }
                        else if (text == GetString(Resource.String.Lbl_Everyone))
                        {
                            MainSettings.SharedData.Edit()?.PutString("CanViewProfile_key", "2")?.Commit();
                            PrivacyCanViewProfile.Summary = text;
                            CanViewProfile = "2";
                        }

                        if (Methods.CheckConnectivity())
                        { 
                            if (dataUser != null) dataUser.PPrivacy = CanViewProfile;

                            var dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);

                            var dataPrivacy = new Dictionary<string, string>
                            {
                                {"p_privacy", CanViewProfile}
                            };
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.SaveSettings(dataPrivacy) });
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }

                        break;
                    }
                    case "CanDirectMessage":
                    {
                        if (text == GetString(Resource.String.Lbl_Everyone))
                        {
                            MainSettings.SharedData.Edit()?.PutString("CanDirectMessage_key", "0")?.Commit();
                            PrivacyCanMessagePref.Summary = text;
                            CanMessagePref = "0";
                        }
                        else if (text == GetString(Resource.String.Lbl_People_i_Follow))
                        {
                            MainSettings.SharedData.Edit()?.PutString("CanDirectMessage_key", "1")?.Commit();
                            PrivacyCanMessagePref.Summary = text;
                            CanMessagePref = "1";
                        }

                        if (Methods.CheckConnectivity())
                        { 
                            if (dataUser != null) dataUser.CPrivacy = CanMessagePref;

                            var dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);

                            var dataPrivacy = new Dictionary<string, string>
                            {
                                {"c_privacy", CanMessagePref}
                            };
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.SaveSettings(dataPrivacy) });
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }

                        break;
                    }
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