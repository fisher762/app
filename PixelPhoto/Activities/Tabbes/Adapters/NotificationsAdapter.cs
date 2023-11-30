using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.User;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.Tabbes.Adapters
{
    public class NotificationsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<NotificationsAdapterClickEventArgs> ItemClick;
        public event EventHandler<NotificationsAdapterClickEventArgs> ItemLongClick;
        public event EventHandler<AvatarNotificationsAdapterClickEventArgs> ItemImageClick;
        public Activity ActivityContext;        
        public ObservableCollection<NotificationDataObject> NotificationsList = new ObservableCollection<NotificationDataObject>();

        public NotificationsAdapter(Activity context)
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
                //Setup your layout here >> Notifications_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Notifications_view, parent, false);
                var vh = new NotificationsAdapterViewHolder(itemView, OnClick, OnLongClick, OnImageClick);
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
                if (viewHolder is NotificationsAdapterViewHolder holder)
                {
                    var item = NotificationsList[position];
                    if (item != null)
                    {
                        holder.UserNameNoitfy.Text = item.Username;

                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        switch (item.Type)
                        {
                            case "followed_u":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.PersonAdd)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.PersonAdd);

                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_followed_u);
                                break;
                            }
                            case "liked_ur_post":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.ThumbsUp)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.ThumbsUp);

                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_liked_ur_post);
                                break;
                            }
                            case "commented_ur_post":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.IosChatbubbles)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.IosChatbubbles);

                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_commented_ur_post);
                                break;
                            }
                            case "mentioned_u_in_comment":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.Pricetag)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.Pricetag);

                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_mentioned_u_in_comment);
                                break;
                            }
                            case "mentioned_u_in_post":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.At)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.At);

                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_mentioned_u_in_post);
                                break;
                            }
                            case "liked_ur_comment":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.ThumbsUp)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.ThumbsUp);
                             
                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_liked_ur_comment);
                                break;
                            }
                            case "reply_ur_comment":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.Pricetag)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify, IonIconsFonts.Pricetag);

                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_replied_ur_comment);
                                break;
                            }
                            case "shared_your_post":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.Share)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify, IonIconsFonts.Share);

                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_shared_ur_post);
                                break;
                            }
                            case "accept_request":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.PersonAdd)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify, IonIconsFonts.PersonAdd);

                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_accepted_request);
                                break;
                            }
                            default:
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.Notifications)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify, IonIconsFonts.Notifications);

                                holder.Description.Text = Methods.FunString.DecodeString(item.Text);
                                break;
                            }
                        }

                        if (!holder.ImageUser.HasOnClickListeners)
                            holder.ImageUser.Click += (sender, e) => OnImageClick(new AvatarNotificationsAdapterClickEventArgs { Class = item, Position = position, View = holder.MainView });
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public override int ItemCount => NotificationsList?.Count ?? 0;

        public NotificationDataObject GetItem(int position)
        {
            return NotificationsList[position];
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

        void OnImageClick(AvatarNotificationsAdapterClickEventArgs args) => ItemImageClick?.Invoke(this, args);
        void OnClick(NotificationsAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(NotificationsAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = NotificationsList[p0];
                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Avatar != "")
                {
                    d.Add(item.Avatar);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }
    }

    public class NotificationsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView ImageUser { get; private set; }
        public View CircleIcon { get; private set; }
        public TextView IconNotify { get; private set; }
        public TextView UserNameNoitfy { get; private set; }
        public TextView Description { get; private set; }

        #endregion

        public NotificationsAdapterViewHolder(View itemView, Action<NotificationsAdapterClickEventArgs> clickListener,Action<NotificationsAdapterClickEventArgs> longClickListener ,
            Action<AvatarNotificationsAdapterClickEventArgs> imageClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                ImageUser = (ImageView)itemView.FindViewById(Resource.Id.ImageUser);
                CircleIcon = itemView.FindViewById<View>(Resource.Id.CircleIcon);
                IconNotify = (TextView)itemView.FindViewById(Resource.Id.IconNotifications);
                UserNameNoitfy = (TextView)itemView.FindViewById(Resource.Id.NotificationsName);
                Description = (TextView)itemView.FindViewById(Resource.Id.NotificationsText);

                //Create an Event 
                itemView.Click += (sender, e) => clickListener(new NotificationsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new NotificationsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class NotificationsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

    public class AvatarNotificationsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public NotificationDataObject Class { get; set; }
    }

}