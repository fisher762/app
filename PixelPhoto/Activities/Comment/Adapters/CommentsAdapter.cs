using Android.App;
using Android.Views;
using Android.Widget;
using Com.Luseen.Autolinklibrary;
using PixelPhoto.Helpers.CacheLoaders;
using System;
using System.Collections.ObjectModel;
using AndroidX.RecyclerView.Widget;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using Refractored.Controls;

namespace PixelPhoto.Activities.Comment.Adapters
{ 
    public class CommentsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<CommentAdapterClickEventArgs> MoreClick;
        public event EventHandler<CommentAdapterClickEventArgs> ReplyClick;
        public event EventHandler<CommentAdapterClickEventArgs> LikeClick;
        public event EventHandler<CommentAdapterClickEventArgs> AvatarClick;
        public event EventHandler<CommentAdapterClickEventArgs> ItemClick;
        public event EventHandler<CommentAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<CommentObject> CommentList = new ObservableCollection<CommentObject>();
        public CommentsAdapter(Activity context)
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
                var vh = new CommentAdapterViewHolder(itemView, OnAvatarClick, OnLikeClick, OnReplyClick, OnMoreClick,  OnClick, OnLongClick);
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
                if (viewHolder is CommentAdapterViewHolder holder)
                {
                    var item = CommentList[position];
                    if (item != null)
                    { 
                        var changer = new TextSanitizer(holder.CommentText, ActivityContext);
                        changer.Load(Methods.FunString.DecodeString(item.Text));

                        holder.TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.Time), false);

                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                        if (item.Replies != 0)
                            holder.ReplyButton.Text = ActivityContext.GetString(Resource.String.Lbl_Reply) + " " + "(" + item.Replies + ")"; 
                        else
                            holder.ReplyButton.Text = ActivityContext.GetString(Resource.String.Lbl_Reply);

                        holder.LikeCount.Text = ActivityContext.GetString(Resource.String.Lbl_Likes) + " " + "(" + item.Likes + ")";
                          
                        holder.LikeIcon.SetImageResource(item.IsLiked == 1 ? Resource.Drawable.ic_action_like_2 : Resource.Drawable.ic_action_like_1); 
                    }

                    if (AppSettings.SetTabDarkTheme)
                        holder.LikeIcon.SetBackgroundResource(Resource.Drawable.Shape_Circle_Black);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public override int ItemCount => CommentList?.Count ?? 0;

        public CommentObject GetItem(int position)
        {
            return CommentList[position];
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

        void OnMoreClick(CommentAdapterClickEventArgs args) => MoreClick?.Invoke(this, args);
        void OnReplyClick(CommentAdapterClickEventArgs args) => ReplyClick?.Invoke(this, args);
        void OnLikeClick(CommentAdapterClickEventArgs args) => LikeClick?.Invoke(this, args);
        void OnAvatarClick(CommentAdapterClickEventArgs args) => AvatarClick?.Invoke(this, args);
        void OnClick(CommentAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(CommentAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args); 
    }

    public class CommentAdapterViewHolder : RecyclerView.ViewHolder
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

        public CommentAdapterViewHolder(View itemView, Action<CommentAdapterClickEventArgs> avatarClickListener, Action<CommentAdapterClickEventArgs> likeListener, Action<CommentAdapterClickEventArgs> replyListener, Action<CommentAdapterClickEventArgs> moreListener,
            Action<CommentAdapterClickEventArgs> clickListener, Action<CommentAdapterClickEventArgs> longClickListener) : base(itemView)
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

                //Event
                ReplyButton.Click += (sender, e) => replyListener(new CommentAdapterClickEventArgs { View = itemView, Position = AdapterPosition  });
                Image.Click += (sender, e) => avatarClickListener(new CommentAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                LikeIcon.Click += (sender, e) => likeListener(new CommentAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                MoreIcon.Click += (sender, e) => moreListener(new CommentAdapterClickEventArgs { View = itemView, Position = AdapterPosition });

                itemView.Click += (sender, e) => clickListener(new CommentAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new CommentAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class CommentAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; } 
    }
     
}