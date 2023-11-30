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
using AT.Markushi.UI;
using Com.Luseen.Autolinklibrary;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.Comment.Adapters;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Refractored.Controls;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PixelPhoto.Activities.Comment
{
    public class ReplyCommentFragment : Fragment, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region  Variables Basic
         
        private HomeActivity MainContext; 
        private ImageView Image, LikeIcon;
        private AutoLinkTextView CommentText;
        private TextView TimeTextView, LikeCount, ReplyButton; 
        private RelativeLayout RootView;
        private EmojiconEditText EmojisIconEditTextView;
        private AppCompatImageView EmojisIcon;
        private CircleButton SendButton;
        private RecyclerView MRecycler;
        private ReplyAdapter MAdapter;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private string CommentId;
        private CommentObject UserinfoComment;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private TextView ViewBoxText;
        private ReplyObject ReplyObject;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            MainContext = (HomeActivity)Activity ?? HomeActivity.GetInstance();
            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.ReplyCommentLayout, container, false);
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

                UserinfoComment = JsonConvert.DeserializeObject<CommentObject>(Arguments.GetString("CommentObject"));
                CommentId = Arguments.GetString("CommentId");

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                SendButton.Click += SendButtonOnClick;
                LikeIcon.Click += LikeIconOnClick;

                MAdapter.AvatarClick += MAdapterOnAvatarClick;
                MAdapter.LikeClick += MAdapterOnLikeClick;
                MAdapter.ReplyClick += MAdapterOnReplyClick;
                MAdapter.MoreClick += MAdapterOnMoreClick;

                GetReply();
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
                Image = view.FindViewById<CircleImageView>(Resource.Id.card_pro_pic);
                CommentText = view.FindViewById<AutoLinkTextView>(Resource.Id.active);
                LikeIcon = view.FindViewById<ImageView>(Resource.Id.likeIcon);
                TimeTextView = view.FindViewById<TextView>(Resource.Id.time);
                LikeCount = view.FindViewById<TextView>(Resource.Id.Like);
                ReplyButton = view.FindViewById<TextView>(Resource.Id.reply);
                 
                RootView = view.FindViewById<RelativeLayout>(Resource.Id.root);
                EmojisIcon = view.FindViewById<AppCompatImageView>(Resource.Id.emojiicon);
                EmojisIconEditTextView = view.FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
                SendButton = view.FindViewById<CircleButton>(Resource.Id.sendButton);
                ViewBoxText = view.FindViewById<TextView>(Resource.Id.viewbox);
                ViewBoxText.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
                 
                var emojis = new EmojIconActions(Activity, RootView, EmojisIconEditTextView, EmojisIcon);
                emojis.ShowEmojIcon();

                EmojisIconEditTextView.Hint = GetText(Resource.String.Lbl_YourComment);
                Methods.SetColorEditText(EmojisIconEditTextView, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                 
                if (AppSettings.SetTabDarkTheme)
                    LikeIcon.SetBackgroundResource(Resource.Drawable.Shape_Circle_Black); 
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
                var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                    MainContext.SetToolBar(toolbar, " ");
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
                MAdapter = new ReplyAdapter(Activity) { ReplyList =  new ObservableCollection<ReplyObject>()};
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                var xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
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
                    MainContext?.FragmentNavigatorBack();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Events

        private async void SendButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(EmojisIconEditTextView.Text) || string.IsNullOrWhiteSpace(EmojisIconEditTextView.Text))
                    return;

                if (Methods.CheckConnectivity())
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    //Comment Code 
                    var timeNow = DateTime.Now.ToShortTimeString();
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var time2 = Convert.ToString(unixTimestamp);

                    var comment = new ReplyObject
                    {
                        Id = unixTimestamp,
                        Text = EmojisIconEditTextView.Text, 
                        Avatar = UserDetails.Avatar,
                        Username = UserDetails.Username,
                        UserId = int.Parse(UserDetails.UserId),   
                        IsOwner = true,
                        CommentId = int.Parse(CommentId),
                        IsLiked = 0,
                        Likes = 0,
                        TextTime = timeNow,
                        Time = unixTimestamp,
                        UserData = ListUtils.MyProfileList.FirstOrDefault()
                    };
                    MAdapter.ReplyList.Add(comment);
                  
                    var lastItem = MAdapter.ReplyList.IndexOf(MAdapter.ReplyList.Last());
                    if (lastItem > -1)
                    {
                        MAdapter.NotifyItemInserted(lastItem);
                        MRecycler.ScrollToPosition(lastItem);
                    }
                   
                    //Api request  
                    var (respondCode, respondString) = await RequestsAsync.Post.AddReplyComment(CommentId, comment.Text);
                    if (respondCode.Equals(200))
                    {
                        if (respondString is  AddReplyObject @object)
                        {
                            var dataComment = MAdapter.ReplyList.FirstOrDefault(a => a.Id == int.Parse(time2));
                            if (dataComment != null)
                            {
                                dataComment = @object.Data;
                                dataComment.Id = @object.Data.Id;
                            }

                            MAdapter.NotifyItemChanged(MAdapter.ReplyList.IndexOf(dataComment));
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

        //event More
        private void MAdapterOnMoreClick(object sender, ReplyAdapterClickEventArgs e)
        {
            try
            {
                ReplyObject = MAdapter.GetItem(e.Position);
                if (ReplyObject != null)
                {
                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Copy));

                    if (ReplyObject.IsOwner) 
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
         
        //Open Profile 
        private void MAdapterOnReplyClick(object sender, ReplyAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    EmojisIconEditTextView.Text = "";
                    EmojisIconEditTextView.Text = "@" + item.UserData.Username + " ";
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Like Reply 
        private void MAdapterOnLikeClick(object sender, ReplyAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
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

                            item.Likes++;
                            likeCount.Text = Activity.GetString(Resource.String.Lbl_Likes) + " " + "(" + item.Likes + ")";
                        }
                        else
                        {
                            if (item.Likes > 0)
                                item.Likes--;
                            else
                                item.Likes = 0;

                            likeCount.Text = Activity.GetString(Resource.String.Lbl_Likes) + " " + "(" + item.Likes + ")";
                        }

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Post.LikeReply(item.Id.ToString()) });
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Profile 
        private void MAdapterOnAvatarClick(object sender, ReplyAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    AppTools.OpenProfile(Activity, item.UserId.ToString(), item.UserData);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        private void LikeIconOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                else
                { 
                    UserinfoComment.IsLiked = UserinfoComment.IsLiked;

                    LikeIcon.SetImageResource(UserinfoComment.IsLiked == 1 ? Resource.Drawable.ic_action_like_2 : Resource.Drawable.ic_action_like_1);
                    //LikeIcon.StartAnimation(animationScale);

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Post.LikeComment(UserinfoComment.Id.ToString()) });
                }
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
                var item = MAdapter.ReplyList.LastOrDefault();
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

        private void GetReply()
        {
            try
            {
                if (UserinfoComment != null)
                {
                    var changer = new TextSanitizer(CommentText, Activity);
                    changer.Load(Methods.FunString.DecodeString(UserinfoComment.Text));

                    TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(UserinfoComment.Time), false);

                    GlideImageLoader.LoadImage(Activity, UserinfoComment.Avatar, Image, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                    ReplyButton.Visibility = ViewStates.Invisible;

                    LikeCount.Text = Activity.GetString(Resource.String.Lbl_Likes) + " " + "(" + UserinfoComment.Likes + ")";
                    LikeIcon.SetImageResource(UserinfoComment.IsLiked == 1 ? Resource.Drawable.ic_action_like_2 : Resource.Drawable.ic_action_like_1);
                }

                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

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
                var countList = MAdapter.ReplyList.Count;
                (var apiStatus, var respond) = await RequestsAsync.Post.FetchReplyComment(CommentId, "24", offset);
                if (apiStatus != 200 || !(respond is FetchReplyObject result) || result.Data == null)
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
                            foreach (var item in from item in result.Data let check = MAdapter.ReplyList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                MAdapter.ReplyList.Add(item);
                            }

                            Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.ReplyList.Count - countList); });
                        }
                        else
                        {
                            MAdapter.ReplyList = new ObservableCollection<ReplyObject>(result.Data);
                            Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.ReplyList.Count > 10 && !MRecycler.CanScrollVertically(1))
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
                if (MAdapter.ReplyList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

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
                MainScrollEvent.IsLoading = false;
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
                    Methods.CopyToClipboard(Activity, Methods.FunString.DecodeString(ReplyObject.Text));
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

                            var dataGlobal = MAdapter?.ReplyList?.FirstOrDefault(a => a.Id == ReplyObject?.Id);
                            if (dataGlobal != null)
                            {
                                var index = MAdapter.ReplyList.IndexOf(dataGlobal);
                                if (index > -1)
                                {
                                    MAdapter.ReplyList.RemoveAt(index);
                                    MAdapter.NotifyItemRemoved(index);
                                }
                            }
                             
                            Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CommentSuccessfullyDeleted), ToastLength.Short)?.Show();
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Post.DeleteReply(ReplyObject.Id.ToString()) });
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