﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Java.Util;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.BlockedUsers.Adapter
{
    public class BlockedUsersAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<BlockedUsersAdapterClickEventArgs> DeleteButtonItemClick;
        public event EventHandler<BlockedUsersAdapterClickEventArgs> AddButtonItemClick;

        public event EventHandler<BlockedUsersAdapterClickEventArgs> ItemClick;
        public event EventHandler<BlockedUsersAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> UserList = new ObservableCollection<UserDataObject>();
        
        public BlockedUsersAdapter(Activity activity)
        {
            try
            {
                //HasStableIds = true;
                ActivityContext = activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => UserList?.Count ?? 0;
 
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_BlockedUsersView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_FriendRequestsView, parent, false);
                var vh = new BlockedUsersAdapterViewHolder(itemView, DeleteButtonClick, AddButtonClick, Click, LongClick);
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
                if (viewHolder is BlockedUsersAdapterViewHolder holder)
                {
                    var item = UserList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        holder.Name.Text = Methods.FunString.SubStringCutOf(AppTools.GetNameFinal(item),35);
                         
                        if (item.Verified == "1")
                            holder.Name.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_small_vector, 0);

                        //holder.About.Text = ActivityContext.GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(item.LastSeen), false);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public UserDataObject GetItem(int position)
        {
            return UserList[position];
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
        private void AddButtonClick(BlockedUsersAdapterClickEventArgs args)
        {
            AddButtonItemClick?.Invoke(this, args);
        }
        private void DeleteButtonClick(BlockedUsersAdapterClickEventArgs args)
        {
            DeleteButtonItemClick?.Invoke(this, args);
        }

        private void Click(BlockedUsersAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(BlockedUsersAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
         
        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserList[p0];
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
            return Glide.With(ActivityContext).Load(p0.ToString()).Apply(new RequestOptions().CircleCrop().SetDiskCacheStrategy(DiskCacheStrategy.All));
        }

    }

    public class BlockedUsersAdapterViewHolder : RecyclerView.ViewHolder
    {
        public BlockedUsersAdapterViewHolder(View itemView, Action<BlockedUsersAdapterClickEventArgs> deleteButtonClickListener, Action<BlockedUsersAdapterClickEventArgs> addButtonClickListener, Action<BlockedUsersAdapterClickEventArgs> clickListener,
            Action<BlockedUsersAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = itemView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = itemView.FindViewById<TextView>(Resource.Id.card_name);
                About = itemView.FindViewById<TextView>(Resource.Id.card_dist);
                AddButton = itemView.FindViewById<CircleButton>(Resource.Id.Add_button);
                DeleteButton = itemView.FindViewById<CircleButton>(Resource.Id.delete_button);
                AddButton.Visibility = ViewStates.Gone;
                About.Visibility = ViewStates.Gone;

                //Event
                AddButton.Click += (sender, e) => addButtonClickListener(new BlockedUsersAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                DeleteButton.Click += (sender, e) => deleteButtonClickListener(new BlockedUsersAdapterClickEventArgs { View = itemView, Position = AdapterPosition });

                //itemView.Click += (sender, e) => clickListener(new BlockedUsersAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                //itemView.LongClick += (sender, e) => longClickListener(new BlockedUsersAdapterClickEventArgs { View = itemView, Position = AdapterPosition }); 
                Console.WriteLine(clickListener);
                Console.WriteLine(longClickListener);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView About { get; private set; }
        public CircleButton AddButton { get; private set; }
        public CircleButton DeleteButton { get; private set; }

        #endregion
    }

    public class BlockedUsersAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

}