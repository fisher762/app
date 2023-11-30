using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AT.Markushi.UI;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Java.Lang;
using PixelPhoto.Activities.Comment.Adapters;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Activities.Posts.page;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.Tabbes.Fragments;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

 
namespace PixelPhoto.Activities.Comment
{ 
    public class CommentsFragment : Fragment, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private RelativeLayout RootView;
        private EmojiconEditText EmojisIconEditTextView;
        private AppCompatImageView EmojisIcon;
        private CircleButton SendButton;
        private RecyclerView CommentRecyclerView;
        private SwipeRefreshLayout XSwipeRefreshLayout;
        private LinearLayoutManager MLayoutManager;
        private CommentsAdapter CommentsAdapter; 
        private ProgressBar ProgressBarLoader;
        private ViewStub EmptyStateLayout;
        private View Inflated;  
        private RecyclerViewOnScrollListener MainScrollEvent;
        private string PostId ,FragmentName;
        private TextView ViewBoxText;
        private HomeActivity MainContext;
        private CommentObject CommentObject;

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
                Activity?.Window?.SetSoftInputMode(SoftInput.AdjustResize);

                var view = inflater.Inflate(Resource.Layout.CommentsFragment, container, false);
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

                PostId = Arguments.GetString("PostId");
                FragmentName = Arguments.GetString("PrevFragment");
                 
                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                SendButton.Click += SendButton_Click;
                XSwipeRefreshLayout.Refresh += XSwipeRefreshLayoutOnRefresh;
                CommentsAdapter.AvatarClick += CommentsAdapterOnAvatarClick;
                CommentsAdapter.LikeClick += CommentsAdapterOnLikeClick;
                CommentsAdapter.ReplyClick += CommentsAdapterOnReplyClick;
                CommentsAdapter.MoreClick += CommentsAdapterOnMoreClick;

                //Get Data Api
                StartApiService();
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
                RootView = view.FindViewById<RelativeLayout>(Resource.Id.root);
                EmojisIcon = view.FindViewById<AppCompatImageView>(Resource.Id.emojiicon);
                EmojisIconEditTextView = view.FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
                SendButton = view.FindViewById<CircleButton>(Resource.Id.sendButton);
                CommentRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyler);
                XSwipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                ProgressBarLoader = view.FindViewById<ProgressBar>(Resource.Id.sectionProgress);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
                ViewBoxText = view.FindViewById<TextView>(Resource.Id.viewbox);
                ViewBoxText.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                XSwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                XSwipeRefreshLayout.Refreshing = false;
                XSwipeRefreshLayout.Enabled = true;
                XSwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                 
                ProgressBarLoader.Visibility = ViewStates.Visible;
                 
                var emojis = new EmojIconActions(Activity, RootView, EmojisIconEditTextView, EmojisIcon);
                emojis.ShowEmojIcon();

                EmojisIconEditTextView.Hint = GetText(Resource.String.Lbl_YourComment);
                Methods.SetColorEditText(EmojisIconEditTextView, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                CommentsAdapter = new CommentsAdapter(Activity);
                MLayoutManager = new LinearLayoutManager(Activity);
                CommentRecyclerView.SetLayoutManager(MLayoutManager);
                CommentRecyclerView.SetAdapter(CommentsAdapter);

                var xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(MLayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
                CommentRecyclerView.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
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
                var toolBar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                MainContext.SetToolBar(toolBar, " ");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Event More
        private void CommentsAdapterOnMoreClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            {
                CommentObject = CommentsAdapter.GetItem(e.Position);
                if (CommentObject != null)
                {
                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Copy));
                   
                    if (CommentObject.IsOwner)
                        arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Delete));
                    
                    dialogList.Title(Activity.GetText(Resource.String.Lbl_More));
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(Activity.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //Event Open Reply
        private void CommentsAdapterOnReplyClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            {
                var item = CommentsAdapter.GetItem(e.Position);
                if (item != null)
                    new SocialIoClickListeners(Activity).CommentReplyPostClick(new CommentReplyClickEventArgs { CommentObject = item, Position = e.Position, View = e.View });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Like 
        private void CommentsAdapterOnLikeClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            {
                var item = CommentsAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (!Methods.CheckConnectivity())
                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    else
                    {
                        var likeIcon = e.View.FindViewById<ImageView>(Resource.Id.likeIcon);
                        var likeCount = e.View.FindViewById<TextView>(Resource.Id.Like);

                        var interpolator = new MyBounceInterpolator(0.2, 20);
                        var animationScale = AnimationUtils.LoadAnimation(Activity, Resource.Animation.scale);
                        animationScale.Interpolator = interpolator;

                        item.IsLiked = item.IsLiked switch
                        {
                            1 => 0,
                            0 => 1,
                            _ => item.IsLiked
                        };

                        likeIcon.SetImageResource(item.IsLiked == 1 ? Resource.Drawable.ic_action_like_2 : Resource.Drawable.ic_action_like_1);
                        likeIcon.StartAnimation(animationScale);

                        if (item.IsLiked == 1)
                        {
                            if (UserDetails.SoundControl)
                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("reaction.mp3");

                            var x = item.Likes + 1;
                            likeCount.Text = Activity.GetString(Resource.String.Lbl_Likes) + " " + "(" + x + ")";
                        }
                        else
                        {
                            var x = item.Likes;

                            if (x > 0)
                                x--;
                            else
                                x = 0;

                            item.Likes = x;

                            likeCount.Text = Activity.GetString(Resource.String.Lbl_Likes) + " " + "(" + item.Likes + ")";
                        }

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Post.LikeComment(item.Id.ToString()) });
                    }
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Open Profile User 
        private void CommentsAdapterOnAvatarClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            {
                var item = CommentsAdapter.GetItem(e.Position);
                if (item != null)
                {
                    AppTools.OpenProfile(Activity, item.UserId.ToString(), item); 
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Refresh
        private void XSwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                CommentsAdapter.CommentList.Clear();
                CommentsAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Add New Comment 
        private async void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(EmojisIconEditTextView.Text) || string.IsNullOrWhiteSpace(EmojisIconEditTextView.Text))
                    return;

                if (Methods.CheckConnectivity())
                {
                    CommentRecyclerView.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    //Comment Code 
                    var time = Methods.Time.TimeAgo(DateTime.Now, false);

                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var time2 = unixTimestamp.ToString();

                    var comment = new CommentObject
                    {
                        Text = EmojisIconEditTextView.Text,
                        Time = unixTimestamp,
                        TimeText = time,
                        Avatar = UserDetails.Avatar,
                        Username = UserDetails.Username,
                        UserId = int.Parse(UserDetails.UserId),
                        PostId = int.Parse(PostId), 
                        Id = int.Parse(time2),
                        IsOwner = true,
                        Name = UserDetails.FullName,
                        Likes = 0,
                        Replies = 0,
                        IsLiked = 0,
                    };
                    CommentsAdapter.CommentList.Add(comment);

                    var lastItem = CommentsAdapter.CommentList.IndexOf(CommentsAdapter.CommentList.Last());
                    if (lastItem > -1)
                    {
                        CommentsAdapter.NotifyItemInserted(lastItem);
                        CommentRecyclerView.ScrollToPosition(lastItem);
                    }
                     
                    //Api request  
                    var (respondCode, respondString) = await RequestsAsync.Post.AddComment(PostId, EmojisIconEditTextView.Text);
                    if (respondCode.Equals(200))
                    {
                        if (respondString is AddCommentObject @object)
                        {
                            var dataComment = CommentsAdapter.CommentList.FirstOrDefault(a => a.Id == int.Parse(time2));
                            if (dataComment != null)
                                dataComment.Id = @object.Id;

                            Activity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    var listHome = MainContext.NewsFeedFragment?.NewsFeedAdapter?.PostList;
                                    var dataPostHome = listHome?.FirstOrDefault(a => a.PostId == Convert.ToInt32(PostId));
                                    if (dataPostHome != null)
                                    {
                                        if (dataPostHome.Votes >= 0)
                                            dataPostHome.Votes++;

                                        var index = listHome.IndexOf(dataPostHome);
                                        if (index >= 0)
                                            MainContext.NewsFeedFragment.NewsFeedAdapter.NotifyItemChanged(index);
                                    }
                                     
                                    if (FragmentName == "ImagePost")
                                    {
                                        var currentFragment = MainContext.FragmentBottomNavigator.GetSelectedTabBackStackFragment();

                                        if (!(currentFragment is GlobalPostViewerFragment frm))
                                            return;

                                        frm.CommentsAdapter.CommentList = CommentsAdapter.CommentList;
                                        frm.CommentsAdapter?.NotifyDataSetChanged();
                                        frm.CommentCount.Text = CommentsAdapter.CommentList.Count + " " + GetText(Resource.String.Lbl_Comments);

                                    }
                                    else if (FragmentName == "HashTags")
                                    {

                                        var currentFragment = MainContext.FragmentBottomNavigator.GetSelectedTabBackStackFragment();

                                        if (currentFragment is HashTagPostFragment frm)
                                        {
                                            var listHash = frm.MAdapter.PostList;
                                            var dataPostHash = listHash?.FirstOrDefault(a => a.PostId == Convert.ToInt32(PostId));
                                            if (dataPostHash != null)
                                            {
                                                if (dataPostHash.Votes >= 0)
                                                    dataPostHash.Votes++;

                                                var index = listHash.IndexOf(dataPostHash);
                                                if (index >= 0)
                                                    frm.MAdapter.NotifyItemChanged(index);
                                            }
                                        }
                                    }
                                    else if (FragmentName == "NewsFeedPost")
                                    {
                                        var currentFragment = MainContext.FragmentBottomNavigator.GetSelectedTabBackStackFragment();

                                        if (currentFragment is NewsFeedFragment frm)
                                        {
                                            var listHash = frm.NewsFeedAdapter.PostList;
                                            var dataPostHash = listHash?.FirstOrDefault(a => a.PostId == Convert.ToInt32(PostId));
                                            if (dataPostHash != null)
                                            {
                                                if (dataPostHash.Votes >= 0)
                                                    dataPostHash.Votes++;

                                                var index = listHash.IndexOf(dataPostHash);
                                                if (index >= 0)
                                                    frm.NewsFeedAdapter.NotifyItemChanged(index);
                                            }
                                        }
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                        }
                    }
                    else Methods.DisplayReportResult(Activity, respondString);

                    //Hide keyboard
                    EmojisIconEditTextView.Text = "";
                    EmojisIconEditTextView.ClearFocus();
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
         
        #endregion
          
        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    MainContext?.FragmentNavigatorBack();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion
         
        #region Scroll

        private void OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                var item = CommentsAdapter.CommentList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id.ToString());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        #region Load Data Api 

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
        }

        private async Task LoadDataAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                var countList = CommentsAdapter.CommentList.Count;
                (var apiStatus, var respond) = await RequestsAsync.Post.FetchComments(PostId,offset, "24");
                if (apiStatus != 200 || !(respond is FetchCommentsObject result) || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = CommentsAdapter.CommentList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                CommentsAdapter.CommentList.Add(item);
                            }

                            Activity?.RunOnUiThread(() => { CommentsAdapter.NotifyItemRangeInserted(countList, CommentsAdapter.CommentList.Count - countList); });
                        }
                        else
                        {
                            CommentsAdapter.CommentList = new ObservableCollection<CommentObject>(result.Data);
                            Activity?.RunOnUiThread(() => { CommentsAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (CommentsAdapter.CommentList.Count > 10 && !CommentRecyclerView.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreComment), ToastLength.Short)?.Show();
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
                XSwipeRefreshLayout.Refreshing = false;

                if (ProgressBarLoader.Visibility == ViewStates.Visible)
                    ProgressBarLoader.Visibility = ViewStates.Gone;

                if (CommentsAdapter.CommentList.Count > 0)
                {
                    CommentRecyclerView.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    CommentRecyclerView.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    var x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoComments);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                ProgressBarLoader.Visibility = ViewStates.Gone;
                MainScrollEvent.IsLoading = false;
                XSwipeRefreshLayout.Refreshing = false;
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
         
        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_Copy))
                {
                    Methods.CopyToClipboard(Activity, Methods.FunString.DecodeString(CommentObject.Text));
                }
                else if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_Delete))
                { 
                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                    dialog.Title(MainContext.GetText(Resource.String.Lbl_DeleteComment));
                    dialog.Content(MainContext.GetText(Resource.String.Lbl_AreYouSureDeleteComment));
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                    {
                        try
                        {
                            if (!Methods.CheckConnectivity())
                            {
                                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                                return;
                            }

                            var dataGlobal = CommentsAdapter?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                            if (dataGlobal != null)
                            { 
                                var index = CommentsAdapter.CommentList.IndexOf(dataGlobal);
                                if (index > -1)
                                {
                                    CommentsAdapter.CommentList.RemoveAt(index);
                                    CommentsAdapter.NotifyItemRemoved(index);
                                }
                            }

                            Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CommentSuccessfullyDeleted), ToastLength.Short)?.Show();
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Post.DeleteComment(CommentObject.Id.ToString()) });
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
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