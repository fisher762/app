using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Adapters;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.Search
{
    public class SearchFragment : Fragment, TabLayoutMediator.ITabConfigurationStrategy, TextView.IOnEditorActionListener
    {
        #region Variables Basic

        private MainTabAdapter Adapter;
        private AppBarLayout AppBarLayout;
        private SearchUserFragment UserTab;
        private SearchHashtagFragment HashTagTab;
        private TabLayout TabLayout;
        private ViewPager2 ViewPager;
        private AutoCompleteTextView SearchView;
        private string SearchText = "a";
        private HomeActivity MainContext;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            MainContext = (HomeActivity)Activity ?? HomeActivity.GetInstance();
            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.SearchTabbedLayout, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                SearchText = Arguments?.GetString("Key") ?? "";

                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);

                Search();

                AdsGoogle.Ad_Interstitial(Activity);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);

            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                SearchView = view.FindViewById<AutoCompleteTextView>(Resource.Id.searchViewBox);
               
                SearchView.RequestFocus();
                SearchView.SetOnEditorActionListener(this);

                TabLayout = view.FindViewById<TabLayout>(Resource.Id.Searchtabs);
                ViewPager = view.FindViewById<ViewPager2>(Resource.Id.Searchviewpager);

                AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.mainAppBarLayout);
                AppBarLayout.SetExpanded(true);

                //Set Tab 
                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach();  
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar(View view)
        {
            try
            {
                var toolBar = view.FindViewById<Toolbar>(Resource.Id.Searchtoolbar);
                MainContext.SetToolBar(toolBar, "");

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Events

       

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchView.ClearFocus();

                UserTab.MAdapter.UserList.Clear();
                UserTab.MAdapter.NotifyDataSetChanged();

                HashTagTab.MAdapter.HashTagsList.Clear();
                HashTagTab.MAdapter.NotifyDataSetChanged();

                if (string.IsNullOrEmpty(SearchText) || string.IsNullOrWhiteSpace(SearchText))
                {
                    SearchText = "a";
                }

                ViewPager.SetCurrentItem(0, true);

                if (Methods.CheckConnectivity())
                {
                    if (UserTab.MAdapter.UserList.Count > 0)
                    {
                        UserTab.MAdapter.UserList.Clear();
                        UserTab.MAdapter.NotifyDataSetChanged();
                    }

                    UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    UserTab.ProgressBarLoader.Visibility = ViewStates.Visible;
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartSearchRequest });

                }
                else
                {
                    UserTab.Inflated ??= UserTab.EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(UserTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    UserTab.ProgressBarLoader.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Set Tab

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                UserTab = new SearchUserFragment();
                HashTagTab = new SearchHashtagFragment();

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(UserTab, GetText(Resource.String.Lbl_Users));
                Adapter.AddFragment(HashTagTab, GetText(Resource.String.Lbl_HashTags));
                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.OffscreenPageLimit = Adapter.ItemCount;

                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                viewPager.Adapter = Adapter;
                viewPager.Adapter.NotifyDataSetChanged();
                 
                TabLayout.SetTabTextColors(AppSettings.SetTabDarkTheme ? Color.White : Color.Black, Color.ParseColor(AppSettings.MainColor));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            try
            {
                tab.SetText(Adapter.GetFragment(position));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    try
                    {
                        MainContext.FragmentNavigatorBack();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }

                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Load Data Search 

        public void Search()
        {
            try
            {
                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (Methods.CheckConnectivity())
                    {
                        UserTab.MAdapter.UserList.Clear();
                        UserTab.MAdapter.NotifyDataSetChanged();

                        HashTagTab.MAdapter.HashTagsList.Clear();
                        HashTagTab.MAdapter.NotifyDataSetChanged();

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartSearchRequest });
                    }
                }
                else
                {

                    UserTab.Inflated ??= UserTab.EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(UserTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    UserTab.ProgressBarLoader.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async Task StartSearchRequest()
        {
            if (UserTab.MainScrollEvent.IsLoading)
                return;

            UserTab.MainScrollEvent.IsLoading = true;

            var countSongsList = UserTab.MAdapter.UserList.Count;
            var countHashTagsList = HashTagTab.MAdapter.HashTagsList.Count;

            (var apiStatus, var respond) = await RequestsAsync.User.SearchUsersHashtags(SearchText, UserTab.Offset);
            if (apiStatus == 200)
            {
                if (respond is SearchUsersHastagsObject result)
                {
                    // User
                    var respondSongsList = result.DataSearch?.Users?.Count;
                    if (respondSongsList > 0)
                    {
                        if (countSongsList > 0)
                        {
                            foreach (var item in from item in result.DataSearch?.Users let check = UserTab.MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                UserTab.MAdapter.UserList.Add(item);
                            }

                            //Activity?.RunOnUiThread(() => { UserTab.MAdapter.NotifyItemRangeInserted(countSongsList - 1, UserTab.MAdapter.UserList.Count - countSongsList); });
                        }
                        else
                        {
                            UserTab.MAdapter.UserList = new ObservableCollection<UserDataObject>(result.DataSearch?.Users);
                        }
                        Activity?.RunOnUiThread(() => { UserTab.MAdapter.NotifyDataSetChanged();
                            SearchView.ClearFocus();
                           
                        });

                    }
                    else
                    {
                        if (UserTab.MAdapter.UserList.Count > 10 && !UserTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short)?.Show();
                    }

                    var respondHashTagsList = result.DataSearch?.Hash?.Count;
                    if (respondHashTagsList > 0)
                    {
                        if (countHashTagsList > 0)
                        {
                            foreach (var item in from item in result.DataSearch?.Hash let check = HashTagTab.MAdapter.HashTagsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                HashTagTab.MAdapter.HashTagsList.Add(item);
                            }

                            //Activity?.RunOnUiThread(() => { HashTagTab.MAdapter.NotifyItemRangeInserted(countHashTagsList - 1, HashTagTab.MAdapter.HashTagsList.Count - countHashTagsList); });
                        }
                        else
                        {
                            HashTagTab.MAdapter.HashTagsList = new ObservableCollection<SearchUsersHastagsObject.HashObject>(result.DataSearch?.Hash);
                        }
                        Activity?.RunOnUiThread(() => { HashTagTab.MAdapter.NotifyDataSetChanged(); });
                    }
                    else
                    {
                        if (HashTagTab.MAdapter.HashTagsList.Count > 10 && !HashTagTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreHash), ToastLength.Short)?.Show();
                    }
                } 
            }
            else 
            {
                UserTab.MainScrollEvent.IsLoading = false;
                Methods.DisplayReportResult(Activity, respond);
            }
             
            Activity?.RunOnUiThread(ShowEmptyPage); 
        }

        private void ShowEmptyPage()
        {
            try
            {
                UserTab.MainScrollEvent.IsLoading = false;
                UserTab.ProgressBarLoader.Visibility = ViewStates.Gone;

                if (UserTab.MAdapter.UserList.Count > 0)
                {
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    UserTab.Inflated ??= UserTab.EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(UserTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }


                if (HashTagTab.MAdapter.HashTagsList.Count > 0)
                {
                    HashTagTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    HashTagTab.Inflated ??= HashTagTab.EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(HashTagTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    HashTagTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                } 
            }
            catch (Exception e)
            {
                UserTab.MainScrollEvent.IsLoading = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void TryAgainButton_Click(object sender, EventArgs e)
        {
            try
            {
                SearchText = "a";

                ViewPager.SetCurrentItem(0, true);

                UserTab.ProgressBarLoader.Visibility = ViewStates.Visible;
                UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;

                Search();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public bool OnEditorAction(TextView v, [GeneratedEnum] ImeAction actionId, KeyEvent e)
        {
            if (actionId == ImeAction.Search)
            {
                SearchText = v.Text;

                SearchView.ClearFocus();
                v.ClearFocus();
                UserTab.MAdapter.UserList.Clear();
                UserTab.MAdapter.NotifyDataSetChanged();

                HashTagTab.MAdapter.HashTagsList.Clear();
                HashTagTab.MAdapter.NotifyDataSetChanged();

                if (Methods.CheckConnectivity())
                {
                    if (UserTab.MAdapter.UserList.Count > 0)
                    {
                        UserTab.MAdapter.UserList.Clear();
                        UserTab.MAdapter.NotifyDataSetChanged();
                    }

                    if (HashTagTab.MAdapter.HashTagsList.Count > 0)
                    {
                        HashTagTab.MAdapter.HashTagsList.Clear();
                        HashTagTab.MAdapter.NotifyDataSetChanged();
                    }

                    UserTab.ProgressBarLoader.Visibility = ViewStates.Visible;
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartSearchRequest });
                }
                else
                {
                    UserTab.Inflated ??= UserTab.EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(UserTab.Inflated, EmptyStateInflater.Type.NoConnection);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    UserTab.ProgressBarLoader.Visibility = ViewStates.Gone;
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }

                SearchView.ClearFocus();
                v.ClearFocus();
                
                //var input = (InputMethodManager)Activity.GetSystemService(Android.Content.Context.InputMethodService);

                
                return true;
            }

            return false;
        }

        #endregion
    }
}