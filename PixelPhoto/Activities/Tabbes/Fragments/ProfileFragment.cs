using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using PixelPhoto.Activities.Favorites;
using PixelPhoto.Activities.MyContacts;
using PixelPhoto.Activities.MyProfile;
using PixelPhoto.Activities.Posts.Extras;
using PixelPhoto.Activities.SettingsUser;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.PullSwipeStyles;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AndroidX.RecyclerView.Widget;
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using Com.Airbnb.Lottie;
using Com.Luseen.Autolinklibrary;
using Liaoinstan.SpringViewLib.Widgets;
using PixelPhoto.Activities.FriendRequest;
using PixelPhoto.Activities.Posts.Adapters;
using PixelPhoto.Activities.Posts.Listeners;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace PixelPhoto.Activities.Tabbes.Fragments
{
    public class ProfileFragment : Fragment, SpringView.IOnFreshListener
    {
        #region Variables Basic

        public ImageView UserProfileImage, ImgFollowRequest;
        private ImageView ImgSetting;
        public string NewsFeedStyle = "Grid";

        public TextView TxtCountFav;
        private TextView TxtCountFollowers, TxtCountFollowing, TxtFollowers, TxtFollowing, TxtFav;
        private TextView TxtName, TxtUsername, IconBusinessAccount;
        private AutoLinkTextView TxtAbout;
        private Button BtnEditProfile;
        private LinearLayout LinFollowers, LinFollowing, LinFavorites;
        private PRecyclerView ProfileFeedRecyclerView;
        public UserPostAdapter UserPostAdapter;
        public NewsFeedAdapter MAdapter;

        private RecyclerViewOnScrollListener MainScrollEvent;
        private TextSanitizer TextSanitizerAutoLink;
        private HomeActivity GlobalContext;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private SpringView SwipeRefreshLayout;
        private ImageView GridButton, ListButton, AddPostButton;

        private GridLayoutManager GridLayout;
        private LinearLayoutManager ListLayout;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                GlobalContext = (HomeActivity)Activity ?? HomeActivity.GetInstance(); 
                HasOptionsMenu = true;
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
                var view = inflater.Inflate(Resource.Layout.TProfileLayout, container, false);
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
                AddOrRemoveEvent(true);

                LoadProfile();

                SetNewsFeedStyleToGrid();

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
                GridLayout = new GridLayoutManager(Context, AppSettings.SetPostRowHorizontalCount);
                ListLayout = new LinearLayoutManager(Context);

                UserProfileImage = (ImageView)view.FindViewById(Resource.Id.user_pic);
                ImgSetting = (ImageView)view.FindViewById(Resource.Id.settingsbutton);
                ImgFollowRequest = (ImageView)view.FindViewById(Resource.Id.followrequestbutton);
                  
                ImgSetting.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                SwipeRefreshLayout = (SpringView)view.FindViewById(Resource.Id.material_style_ptr_frame);
                ProfileFeedRecyclerView = view.FindViewById<PRecyclerView> (Resource.Id.RecylerFeed);

                TxtName = (TextView)view.FindViewById(Resource.Id.card_name);
                IconBusinessAccount = (TextView)view.FindViewById(Resource.Id.card_name_icon);
                TxtUsername = (TextView)view.FindViewById(Resource.Id.card_dist);
                BtnEditProfile = (Button)view.FindViewById(Resource.Id.cont);

                TxtCountFollowers = (TextView)view.FindViewById(Resource.Id.CountFollowers);
                TxtCountFollowing = (TextView)view.FindViewById(Resource.Id.CountFollowing);
                TxtCountFav = (TextView)view.FindViewById(Resource.Id.CountFav);

                TxtFollowers = view.FindViewById<TextView>(Resource.Id.txtFollowers);
                TxtFollowing = view.FindViewById<TextView>(Resource.Id.txtFollowing);
                TxtFav = view.FindViewById<TextView>(Resource.Id.txtFav);

                LinFollowers = view.FindViewById<LinearLayout>(Resource.Id.layoutFollowers);
                LinFollowing = view.FindViewById<LinearLayout>(Resource.Id.layoutFollowing);
                LinFavorites = view.FindViewById<LinearLayout>(Resource.Id.layoutFavorites);

                GridButton = view.FindViewById<ImageView>(Resource.Id.grid_pic);
                ListButton = view.FindViewById<ImageView>(Resource.Id.menu_pic);
                AddPostButton = view.FindViewById<ImageView>(Resource.Id.addPost_pic);
                GridButton.Click += GridButton_Click;
                ListButton.Click += ListButton_Click;
                AddPostButton.Click += AddPostButton_Click;

                TxtAbout = (AutoLinkTextView)view.FindViewById(Resource.Id.description);
                TextSanitizerAutoLink = new TextSanitizer(TxtAbout, Activity);

                TxtCountFollowers.Text = "0";
                TxtCountFollowing.Text = "0";
                TxtCountFav.Text = "0";

                EmptyStateLayout = (ViewStub)view.FindViewById(Resource.Id.viewStub);
                 
                ProfileFeedRecyclerView.HasFixedSize = true;
                ProfileFeedRecyclerView.SetItemViewCacheSize(10);
                
                SwipeRefreshLayout = (SpringView)view.FindViewById(Resource.Id.material_style_ptr_frame);
                SwipeRefreshLayout.SetType(SpringView.Type.Overlap);
                SwipeRefreshLayout.Header = new DefaultHeader(Activity);
                SwipeRefreshLayout.Footer = new DefaultFooter(Activity);
                SwipeRefreshLayout.Enable = true;
                SwipeRefreshLayout.SetListener(this);

                MAdapter = new NewsFeedAdapter(Activity, ProfileFeedRecyclerView);
                UserPostAdapter = new UserPostAdapter(Activity);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetNewsFeedStyleToGrid()
        {
            try
            {
                NewsFeedStyle = "Grid";

                ProfileFeedRecyclerView.SetLayoutManager(GridLayout);
                ProfileFeedRecyclerView.AddItemDecoration(new GridSpacingItemDecoration(1, 1, true));
                ProfileFeedRecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;
                UserPostAdapter ??= new UserPostAdapter(Activity);

                if (MAdapter != null && MAdapter?.PostList?.Count > 0)
                    UserPostAdapter.PostList = new ObservableCollection<PostsObject>(MAdapter.PostList);

                var sizeProvider = new ViewPreloadSizeProvider();
                var preLoader = new RecyclerViewPreloader<PostsObject>(Activity, UserPostAdapter, sizeProvider, 10);
                ProfileFeedRecyclerView.ClearOnScrollListeners();

                ProfileFeedRecyclerView.AddOnScrollListener(preLoader);
                ProfileFeedRecyclerView.SetAdapter(UserPostAdapter);

                EmptyStateLayout.Visibility = ViewStates.Gone;
                ProfileFeedRecyclerView.Visibility = ViewStates.Visible;

                var xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(GridLayout);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
                ProfileFeedRecyclerView.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void SetNewsFeedStyleToList()
        {
            try
            {
                NewsFeedStyle = "Linear";

                ProfileFeedRecyclerView.SetLayoutManager(ListLayout);
                MAdapter ??= new NewsFeedAdapter(Activity, ProfileFeedRecyclerView);

                var sizeProvider = new ViewPreloadSizeProvider();
                var preLoader = new RecyclerViewPreloader<PostsObject>(Activity, MAdapter, sizeProvider, 8);

                if (MAdapter != null && UserPostAdapter?.PostList?.Count > 0)
                    MAdapter.PostList = new ObservableCollection<PostsObject>(UserPostAdapter.PostList);

                ProfileFeedRecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;
                ProfileFeedRecyclerView.ClearOnScrollListeners();
                ProfileFeedRecyclerView.AddOnScrollListener(preLoader);
                ProfileFeedRecyclerView.SetAdapter(MAdapter);

                EmptyStateLayout.Visibility = ViewStates.Gone;
                ProfileFeedRecyclerView.Visibility = ViewStates.Visible;

                var xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(ListLayout);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
                ProfileFeedRecyclerView.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    ImgSetting.Click += ImgSettingOnClick;
                    ImgFollowRequest.Click += ImgFollowRequestOnClick;
                    UserProfileImage.Click += ImgChangeOnClick;
                    BtnEditProfile.Click += BtnEditProfileOnClick;
                    LinFollowers.Click += LinFollowersOnClick;
                    LinFollowing.Click += LinFollowingOnClick;
                    LinFavorites.Click += LinFavoritesOnClick;
                    UserPostAdapter.ItemClick += UserPostAdapterOnItemClick;
                    MAdapter.ItemClick += MAdapterOnItemClick;
                }
                else
                {
                    ImgSetting.Click -= ImgSettingOnClick;

                    UserProfileImage.Click -= ImgChangeOnClick;
                    BtnEditProfile.Click -= BtnEditProfileOnClick;
                    LinFollowers.Click -= LinFollowersOnClick;
                    LinFollowing.Click -= LinFollowingOnClick;
                    LinFavorites.Click -= LinFavoritesOnClick;
                    UserPostAdapter.ItemClick -= UserPostAdapterOnItemClick;
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void ListButton_Click(object sender, EventArgs e)
        {
            try
            {
                SetNewsFeedStyleToList();
                ShowEmptyPage();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GridButton_Click(object sender, EventArgs e)
        {
            try
            {
                SetNewsFeedStyleToGrid();
                ShowEmptyPage();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AddPostButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Activity is HomeActivity context)
                {
                    if (!context.CircleMenu.IsOpened)
                    {
                        context.CircleMenu.Visibility = ViewStates.Visible;
                        context.CircleMenu.OpenMenu();
                    }
                    else
                    {
                        context.CircleMenu.CloseMenu();
                        context.CircleMenu.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Load Explore Feed
        private void EmptyStateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    StartApiService();
                }
                else
                {
                    Toast.MakeText(Activity, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Show Favorites
        private void LinFavoritesOnClick(object sender, EventArgs e)
        {
            try
            {
                var bundle = new Bundle();

                var myFavoritesFragment = new FavoritesFragment
                {
                    Arguments = bundle
                };

                GlobalContext.OpenFragment(myFavoritesFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Show Following
        private void LinFollowingOnClick(object sender, EventArgs e)
        {
            try
            {
                var bundle = new Bundle();

                bundle.PutString("UserId", UserDetails.UserId);
                bundle.PutString("UserType", "Following");

                var myContactsFragment = new MyContactsFragment
                {
                    Arguments = bundle
                };

                GlobalContext.OpenFragment(myContactsFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Show Followers
        private void LinFollowersOnClick(object sender, EventArgs e)
        {
            try
            {
                var bundle = new Bundle();

                bundle.PutString("UserId", UserDetails.UserId);
                bundle.PutString("UserType", "Followers");

                var profileFragment = new MyContactsFragment
                {
                    Arguments = bundle
                };

                GlobalContext.OpenFragment(profileFragment);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            } 
        }

        //Show post by id
        private void UserPostAdapterOnItemClick(object sender, UserPostAdapterViewHolderClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = UserPostAdapter?.PostList?[e.Position];
                    if (item != null)
                    {
                        GlobalContext.OpenNewsFeedItem(item.PostId.ToString(), item);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void MAdapterOnItemClick(object sender, Holders.PostAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = MAdapter?.PostList[e.Position];
                    if (item?.IsLiked != null && !item.IsLiked.Value)
                    {
                        if (SystemClock.ElapsedRealtime() - UserDetails.TimestampLastClick < AppSettings.DoubleClickQualificationSpanInMillis)
                        {
                            var likeIcon = e.View.FindViewById<ImageView>(Resource.Id.Like);
                            var likeAnimationView = e.View.FindViewById<LottieAnimationView>(Resource.Id.animation_view_of_like);
                            var tapLikeAnimation = e.View.FindViewById<LottieAnimationView>(Resource.Id.animation_like);
                            tapLikeAnimation.PlayAnimation();

                            MAdapter.OnPostItemClickListener.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = e.View, LikeImgButton = likeIcon, LikeAnimationView = likeAnimationView }, e.Position);
                        }
                        UserDetails.TimestampLastClick = SystemClock.ElapsedRealtime();
                    }
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        // Show EditProfile
        private void BtnEditProfileOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.StartActivityForResult(new Intent(Context, typeof(EditProfileActivity)), 3000);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Show Img Change
        private void ImgChangeOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.OpenDialogGallery("MyProfile");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Show Setting
        private void ImgSettingOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Context, typeof(SettingsUserActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Follow Request
        private void ImgFollowRequestOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Context, typeof(FriendRequestActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load My Profile Api 

        public async void LoadProfile()
        {
            try
            {
                if (ListUtils.MyProfileList.Count == 0)
                    await ApiRequest.GetProfile_Api(Activity);

                var data = ListUtils.MyProfileList.FirstOrDefault();
                if (data != null)
                {
                    GlideImageLoader.LoadImage(Activity, data.Avatar, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                    TxtName.Text = AppTools.GetNameFinal(data);
                    TxtUsername.Text = "@" + data.Username;

                    TextSanitizerAutoLink.Load(AppTools.GetAboutFinal(data));

                    TxtCountFav.Text = Methods.FunString.FormatPriceValue(int.Parse(data.Favourites));
                     
                    if (data.Verified == "1")
                        TxtName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_vector, 0);

                    if (data.BusinessAccount == "1")
                        IconBusinessAccount.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_dolar_vector, 0);
                     
                    if (data.Followers != null && int.TryParse(data.Followers, out var numberFollowers))
                        TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(numberFollowers);

                    if (data.Following != null && int.TryParse(data.Following, out var numberFollowing))
                        TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(numberFollowing);

                    switch (data.PPrivacy)
                    {
                        case "0":
                        case "1":
                            ImgFollowRequest.Visibility = ViewStates.Visible;
                            break;
                        case "2":
                            ImgFollowRequest.Visibility = ViewStates.Gone;
                            break;
                        default:
                            ImgFollowRequest.Visibility = ViewStates.Visible;
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

        #region Load Explore Feed

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
            {
                EmptyStateLayout.Visibility = ViewStates.Visible;
                ProfileFeedRecyclerView.Visibility = ViewStates.Gone;

                Inflated ??= EmptyStateLayout.Inflate();
                var x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButton_Click;
                }
            }
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadFeedAsync(offset) });
        }

        private async Task LoadFeedAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                dynamic adapter = NewsFeedStyle switch
                {
                    "Linear" => MAdapter,
                    "Grid" => UserPostAdapter,
                    _ => UserPostAdapter
                };

                int countList = adapter.PostList.Count;
                var (apiStatus, respond) = await RequestsAsync.Post.FetchUserPostsById(UserDetails.UserId, "24", offset);
                if (apiStatus != 200 || !(respond is FetchUserPostsByUserIdObject result) || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    Activity?.RunOnUiThread(() =>
                    {
                        TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(result.Data.UserFollowers);
                        TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(result.Data.UserFollowing);
                    });
                     
                    var respondList = result.Data.UserPosts.Count;
                    if (respondList > 0)
                    {
                        result.Data.UserPosts = AppTools.FilterPost(result.Data.UserPosts);
                       
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data.UserPosts
                                let check = NewsFeedStyle switch
                                {
                                    "Linear" => MAdapter?.PostList?.FirstOrDefault(a => a.PostId == item.PostId),
                                    "Grid" => UserPostAdapter?.PostList?.FirstOrDefault(a => a.PostId == item.PostId),
                                    _ => null
                                }
                                where (dynamic)check == null
                                select item)
                            {
                                adapter.PostList.Add(item);
                            }

                            Activity?.RunOnUiThread(() => { adapter.NotifyItemRangeInserted(countList, adapter.PostList.Count - countList); });
                        }
                        else
                        {
                            adapter.PostList = new ObservableCollection<PostsObject>(result.Data.UserPosts);
                            Activity?.RunOnUiThread(() => { adapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (adapter.PostList.Count > 10 && !ProfileFeedRecyclerView.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short)?.Show();
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
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.OnFinishFreshAndLoad();

                dynamic adapter = NewsFeedStyle switch
                {
                    "Linear" => MAdapter,
                    "Grid" => UserPostAdapter,
                    _ => UserPostAdapter
                };

                if (adapter?.PostList?.Count > 0)
                {
                    ProfileFeedRecyclerView.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    ProfileFeedRecyclerView.Visibility = ViewStates.Visible;

                    Inflated ??= EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoPost);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
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

        #region Scroll

        private void OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                PostsObject item;
                switch (NewsFeedStyle)
                {
                    case "Linear":
                        item = MAdapter?.PostList?.LastOrDefault();
                        if (item != null && !string.IsNullOrEmpty(item.UserId.ToString()) && !MainScrollEvent.IsLoading)
                            StartApiService(item?.PostId.ToString());
                        break;
                    case "Grid":
                        item = UserPostAdapter?.PostList?.LastOrDefault();
                        if (item != null && !string.IsNullOrEmpty(item.UserId.ToString()) && !MainScrollEvent.IsLoading)
                            StartApiService(item?.PostId.ToString());
                        break;
                } 
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
                LoadProfile();

                dynamic adapter = NewsFeedStyle switch
                {
                    "Linear" => MAdapter,
                    "Grid" => UserPostAdapter,
                    _ => UserPostAdapter
                };

                adapter.PostList.Clear();
                adapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

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
                var item = NewsFeedStyle switch
                {
                    "Linear" => MAdapter?.PostList?.LastOrDefault(),
                    "Grid" => UserPostAdapter?.PostList?.LastOrDefault(),
                    _ => null
                };

                if (item != null && !string.IsNullOrEmpty(item.PostId.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(item.PostId.ToString());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
    }
}