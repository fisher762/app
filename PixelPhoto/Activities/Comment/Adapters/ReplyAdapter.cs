using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Com.Luseen.Autolinklibrary;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using Refractored.Controls;

namespace PixelPhoto.Activities.Comment.Adapters
{
    public class ReplyAdapter : RecyclerView.Adapter
    {
        public event EventHandler<ReplyAdapterClickEventArgs> MoreClick;
        public event EventHandler<ReplyAdapterClickEventArgs> ReplyClick;
        public event EventHandler<ReplyAdapterClickEventArgs> LikeClick;
        public event EventHandler<ReplyAdapterClickEventArgs> AvatarClick;
        public event EventHandler<ReplyAdapterClickEventArgs> ItemClick;
        public event EventHandler<ReplyAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<ReplyObject> ReplyList = new ObservableCollection<ReplyObject>();

        public ReplyAdapter(Activity context)
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

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_PageCircle_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment, parent, false);
                var vh = new ReplyAdapterViewHolder(itemView, OnAvatarClick, OnLikeClick, OnReplyClick, OnMoreClick, OnClick, OnLongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is ReplyAdapterViewHolder holder)
                {
                    var item = ReplyList[position];
                    if (item != null)
                    {
                        var changer = new TextSanitizer(holder.CommentText, ActivityContext);
                        changer.Load(Methods.FunString.DecodeString(item.Text));

                        holder.TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.Time), false);

                        GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                        holder.ReplyButton.Text = ActivityContext.GetString(Resource.String.Lbl_Reply);

                        holder.LikeCount.Text = ActivityContext.GetString(Resource.String.Lbl_Likes) + " " + "(" + item.Likes + ")";
                          
                        holder.LikeIcon.SetImageResource(item.IsLiked == 1 ? Resource.Drawable.ic_action_like_2 : Resource.Drawable.ic_action_like_1); 
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public override int ItemCount => ReplyList?.Count ?? 0;

        public ReplyObject GetItem(int position)
        {
            return ReplyList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        void OnMoreClick(ReplyAdapterClickEventArgs args) => MoreClick?.Invoke(this, args);
        void OnReplyClick(ReplyAdapterClickEventArgs args) => ReplyClick?.Invoke(this, args);
        void OnLikeClick(ReplyAdapterClickEventArgs args) => LikeClick?.Invoke(this, args);
        void OnAvatarClick(ReplyAdapterClickEventArgs args) => AvatarClick?.Invoke(this, args);
        void OnClick(ReplyAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(ReplyAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class ReplyAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic


        public View MainView { get; private set; }

        public ImageView Image { get; private set; }
        public ImageView LikeIcon { get; private set; } 
        public ImageView MoreIcon { get; private set; } 
        public AutoLinkTextView CommentText { get; private set; } 
        public TextView TimeTextView { get; private set; }
        public TextView LikeCount { get; private set; }
        public TextView ReplyButton { get; private set; }
         
        #endregion

        public ReplyAdapterViewHolder(View itemView, Action<ReplyAdapterClickEventArgs> avatarClickListener, Action<ReplyAdapterClickEventArgs> likeListener, Action<ReplyAdapterClickEventArgs> replyListener, Action<ReplyAdapterClickEventArgs> MoreListener,
            Action<ReplyAdapterClickEventArgs> clickListener, Action<ReplyAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = itemView.FindViewById<CircleImageView>(Resource.Id.card_pro_pic);
                CommentText = itemView.FindViewById<AutoLinkTextView>(Resource.Id.active);
                LikeIcon = itemView.FindViewById<ImageView>(Resource.Id.likeIcon);
                MoreIcon = itemView.FindViewById<ImageView>(Resource.Id.moreicon);
                TimeTextView = itemView.FindViewById<TextView>(Resource.Id.time);
                LikeCount = itemView.FindViewById<TextView>(Resource.Id.Like);
                ReplyButton = itemView.FindViewById<TextView>(Resource.Id.reply);

                if (AppSettings.SetTabDarkTheme)
                    LikeIcon.SetBackgroundResource(Resource.Drawable.Shape_Circle_Black);

                //Event 
                ReplyButton.Click += (sender, e) => replyListener(new ReplyAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                Image.Click += (sender, e) => avatarClickListener(new ReplyAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                LikeIcon.Click += (sender, e) => likeListener(new ReplyAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                MoreIcon.Click += (sender, e) => MoreListener(new ReplyAdapterClickEventArgs { View = itemView, Position = AdapterPosition });

                itemView.Click += (sender, e) => clickListener(new ReplyAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new ReplyAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class ReplyAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
     
}