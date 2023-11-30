using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Liaoinstan.SpringViewLib.Widgets;
using PixelPhoto.Activities.Search;
using PixelPhoto.Activities.Store;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.PullSwipeStyles;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.Classes.Store;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace PixelPhoto.Activities.Tabbes.Fragments
{
    public class ExploreFragmentTheme2 : Fragment, SpringView.IOnFreshListener
    {
        #region Variables Basic

        public TabExploreAdapter MAdapter;
        private SpringView SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private AutoCompleteTextView SearchViewBox;
        private SearchFragment SearchFragment;
        private HomeActivity GlobalContext;
        private ProgressBar ProgressBarLoader;
        private ImageView ShoppingButton;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity ?? HomeActivity.GetInstance();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.TExploreLayout2, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                StartApiService();

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
                ShoppingButton = (ImageView)view.FindViewById(Resource.Id.shoppingButton);
                ShoppingButton.Click += ShoppingButtonOnClick;

                if (!AppSettings.ShowStore)
                    ShoppingButton.Visibility = ViewStates.Gone;
                
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                ProgressBarLoader = (ProgressBar)view.FindViewById(Resource.Id.sectionProgress);

                SwipeRefreshLayout = (SpringView)view.FindViewById(Resource.Id.material_style_ptr_frame);
                SwipeRefreshLayout.SetType(SpringView.Type.Overlap);
                SwipeRefreshLayout.Header = new DefaultHeader(Activity);
                SwipeRefreshLayout.Footer = new DefaultFooter(Activity);
                SwipeRefreshLayout.Enable = true;
                SwipeRefreshLayout.EnableFooter = false;
                SwipeRefreshLayout.SetListener(this);

                SearchViewBox = view.FindViewById<AutoCompleteTextView>(Resource.Id.searchViewBox);
                SearchViewBox.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black); 
                SearchViewBox.Click += SearchViewLinearLayoutOnClick;
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
                var toolbar = view.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                GlobalContext.SetToolBar(toolbar, " ", false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new TabExploreAdapter(Activity)
                {
                    ExploreList = new ObservableCollection<Classes.ExploreClass>()
                };
                LayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true; 
                MRecycler.SetAdapter(MAdapter); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events
         
        private void ShoppingButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Activity, typeof(StoreActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void SearchViewLinearLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var bundle = new Bundle();
                bundle.PutString("Key", "");
                SearchFragment = new SearchFragment
                {
                    Arguments = bundle
                };

                GlobalContext.OpenFragment(SearchFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        #endregion

        #region Refresh

        public void OnRefresh()
        {
            try
            {
                MAdapter.ExploreList.Clear();
                MAdapter.NotifyDataSetChanged();

                StartApiService(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnLoadMore()
        {
            try
            {
                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Data Api 

        private void StartApiService(string offsetSuggestion = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadSuggestionsAsync(offsetSuggestion), LoadExploreAllStoreAsync , () => LoadExploreAsync() ,  LoadFeaturedAsync  });
        }
         
        private async Task LoadSuggestionsAsync(string offset = "0")
        { 
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.User.FetchSuggestionsUsers(offset, "14");
                if (apiStatus != 200 || !(respond is GetUserObject result) || result.Data == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        var checkList = MAdapter.ExploreList.FirstOrDefault(q => q.Type == Classes.ItemType.User);
                        if (checkList == null)
                        {
                            var user = new Classes.ExploreClass
                            {
                                Id = 100,
                                UserList = new List<UserDataObject>(),
                                Type = Classes.ItemType.User
                            };

                            foreach (var item in from item in result.Data let check = user.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                user.UserList.Add(item);
                            }

                            MAdapter.ExploreList.Insert(0, user);
                        }
                        else
                        {
                            foreach (var item in from item in result.Data let check = checkList.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                checkList.UserList.Add(item);
                            }
                        }
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated ??= EmptyStateLayout.Inflate();
                var x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }
         
        private async Task LoadExploreAllStoreAsync()
        {
            if (!AppSettings.ShowStore)
                return;

            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Store.ExploreAllStore();
                if (apiStatus != 200 || !(respond is StoreObject result) || result.Data == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        var checkList = MAdapter.ExploreList.FirstOrDefault(q => q.Type == Classes.ItemType.Store);
                        if (checkList == null)
                        {
                            var store = new Classes.ExploreClass
                            {
                                Id = 200,
                                StoreList = new List<StoreDataObject>(),
                                Type = Classes.ItemType.Store
                            };

                            foreach (var item in from item in result.Data let check = store.StoreList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                store.StoreList.Add(item);
                            }

                            MAdapter.ExploreList.Insert(1,store);
                        }
                        else
                        {
                            foreach (var item in from item in result.Data let check = checkList.StoreList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                checkList.StoreList.Add(item);
                            }
                        }
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated ??= EmptyStateLayout.Inflate();
                var x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }
         
        private async Task LoadExploreAsync(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Post.FetchExplorePosts(offset, "5");
                if (apiStatus != 200 || !(respond is FetchListPostsObject result) || result.Data == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        result.Data = AppTools.FilterPost(result.Data);

                        var checkList = MAdapter.ExploreList.FirstOrDefault(q => q.Type == Classes.ItemType.Post);
                        if (checkList == null)
                        {
                            var post = new Classes.ExploreClass
                            {
                                Id = 300,
                                PostList = new List<PostsObject>(),
                                Type = Classes.ItemType.Post
                            };

                            foreach (var item in from item in result.Data let check = post.PostList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                            {
                                post.PostList.Add(item);
                            }

                            MAdapter.ExploreList.Add(post);
                        }
                        else
                        {
                            foreach (var item in from item in result.Data let check = checkList.PostList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                            {
                                checkList.PostList.Add(item);
                            }
                        }
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated ??= EmptyStateLayout.Inflate();
                var x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }
         
        private async Task LoadFeaturedAsync()
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Post.FetchFeaturedPosts();
                if (apiStatus != 200 || !(respond is FetchListPostsObject result) || result.Data == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        result.Data = AppTools.FilterPost(result.Data);

                        var checkList = MAdapter.ExploreList.FirstOrDefault(q => q.Type == Classes.ItemType.Featured);
                        if (checkList == null)
                        {
                            var post = new Classes.ExploreClass
                            {
                                Id = 300,
                                PostList = new List<PostsObject>(),
                                Type = Classes.ItemType.Featured
                            };

                            foreach (var item in from item in result.Data let check = post.PostList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                            {
                                post.PostList.Add(item);
                            }

                            MAdapter.ExploreList.Insert(2, post);
                        }
                        else
                        {
                            foreach (var item in from item in result.Data let check = checkList.PostList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                            {
                                checkList.PostList.Add(item);
                            }
                        }
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated ??= EmptyStateLayout.Inflate();
                var x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }
         
        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.OnFinishFreshAndLoad();

                if (ProgressBarLoader.Visibility == ViewStates.Visible)
                    ProgressBarLoader.Visibility = ViewStates.Gone;

                if (MAdapter.ExploreList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    var checkList = MAdapter.ExploreList.FirstOrDefault(q => q.Type == Classes.ItemType.Post);
                    if (checkList != null)
                    {
                        var emptyStateChecker = MAdapter.ExploreList.FirstOrDefault(a => a.Type == Classes.ItemType.EmptyPage);
                        if (emptyStateChecker != null)
                        {
                            MAdapter.ExploreList.Remove(emptyStateChecker);
                        }
                    }

                    MAdapter.NotifyDataSetChanged();
                }
                else
                {
                    var emptyStateChecker = MAdapter.ExploreList.FirstOrDefault(q => q.Type == Classes.ItemType.EmptyPage);
                    if (emptyStateChecker == null)
                    {
                        MAdapter.ExploreList.Add(new Classes.ExploreClass
                        {
                            Id = 400,
                            Type = Classes.ItemType.EmptyPage
                        });
                        MAdapter.NotifyDataSetChanged();

                        EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception e)
            {
                if (ProgressBarLoader.Visibility == ViewStates.Visible)
                    ProgressBarLoader.Visibility = ViewStates.Gone;

                SwipeRefreshLayout.OnFinishFreshAndLoad();
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
   
        #endregion

    }
}