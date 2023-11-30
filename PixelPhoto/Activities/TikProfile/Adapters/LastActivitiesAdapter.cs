using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.User;
using IList = System.Collections.IList;

namespace PixelPhoto.Activities.TikProfile.Adapters
{
    public class LastActivitiesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<LastActivitiesAdapterClickEventArgs> ItemClick;
        public event EventHandler<LastActivitiesAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<ActivitiesObject.Activity> LastActivitiesList = new ObservableCollection<ActivitiesObject.Activity>();

        public LastActivitiesAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => LastActivitiesList?.Count ?? 0;
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_LastActivities_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_LastActivities_View, parent, false);
                var vh = new LastActivitiesAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is LastActivitiesAdapterViewHolder holder)
                {
                    var item = LastActivitiesList[position];
                    if (item != null)
                    {
                        Initialize(holder, item);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Initialize(LastActivitiesAdapterViewHolder holder, ActivitiesObject.Activity item)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.ActivitiesImage, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                var replace = "";
                if (item.Type == "followed_user")
                {
                    holder.Icon.SetImageResource(Resource.Drawable.ic_add);
                    holder.Icon.SetColorFilter(Color.ParseColor("#333333"), PorterDuff.Mode.Multiply);

                    if (item.Text.Contains("started following"))
                    {
                        if (UserDetails.LangName.Contains("fr"))
                        {
                            var split = item.Text.Split("started following").Last().Replace("post", "");
                            replace = item.UserData.Name + " " + ActivityContext.GetString(Resource.String.Lbl_StartedFollowing) + " " + split;
                        }
                        else
                            replace = item.Text.Replace("started following", ActivityContext.GetString(Resource.String.Lbl_StartedFollowing));
                    } 
                    else if (item.Text.Contains("is following"))
                    {
                        if (UserDetails.LangName.Contains("fr"))
                        {
                            var split = item.Text.Split("is following").Last().Replace("post", "");
                            replace = item.UserData.Name + " " + ActivityContext.GetString(Resource.String.Lbl_IsFollowing) + " " + split;
                        }
                        else
                            replace = item.Text.Replace("is following", ActivityContext.GetString(Resource.String.Lbl_IsFollowing));
                    }
                }
                else if (item.Type == "liked__post")
                {
                    holder.Icon.SetImageResource(Resource.Drawable.ic_action_like_2);

                    if (UserDetails.LangName.Contains("fr"))
                    {
                        var split = item.Text.Split("liked").Last().Replace("post", "");
                        replace = item.UserData.Name + " " + ActivityContext.GetString(Resource.String.Lbl_Liked) + " " + ActivityContext.GetString(Resource.String.Lbl_Post) + " " + split;
                    }
                    else
                        replace = item.Text.Replace("liked", ActivityContext.GetString(Resource.String.Lbl_Liked)).Replace("post", ActivityContext.GetString(Resource.String.Lbl_Post));
                }  
                else if (item.Type == "commented_on_post")
                {
                    holder.Icon.SetImageResource(Resource.Drawable.ic_action_comment);
                    holder.Icon.SetColorFilter(Color.ParseColor("#333333"), PorterDuff.Mode.Multiply);

                    if (UserDetails.LangName.Contains("fr"))
                    {
                        var split = item.Text.Split("commented on").Last().Replace("post", "");
                        replace = item.UserData.Name + " " + ActivityContext.GetString(Resource.String.Lbl_CommentedOn) + " " + ActivityContext.GetString(Resource.String.Lbl_Post) + " " + split;
                    }
                    else
                    {
                        replace = item.Text.Replace("commented on", ActivityContext.GetString(Resource.String.Lbl_CommentedOn)).Replace("post", ActivityContext.GetString(Resource.String.Lbl_Post));
                    }
                }

                holder.ActivitiesEvent.Text = !string.IsNullOrEmpty(replace) ? replace : item.Text;

                // holder.Username.Text = item.UserData.Name; 
                holder.Username.Visibility = ViewStates.Gone;

                holder.Time.Text = Methods.Time.TimeAgo(int.Parse(item.Time), false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void BindEnd()
        {
            try
            {
                NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Function 
        public void Add(ActivitiesObject.Activity item)
        {
            try
            {
                var check = LastActivitiesList.FirstOrDefault(a => a.Id == item.Id);
                if (check == null)
                {
                    LastActivitiesList.Add(item);
                    NotifyItemInserted(LastActivitiesList.IndexOf(LastActivitiesList.Last()));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Clear()
        {
            try
            {
                LastActivitiesList.Clear();
                NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public ActivitiesObject.Activity GetItem(int position)
        {
            return LastActivitiesList[position];
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


        private void Click(LastActivitiesAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(LastActivitiesAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = LastActivitiesList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (!string.IsNullOrEmpty(item.UserData.Avatar))
                        d.Add(item.UserData.Avatar);

                    return d;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }


    }

    public class LastActivitiesAdapterViewHolder : RecyclerView.ViewHolder
    {
        public LastActivitiesAdapterViewHolder(View itemView, Action<LastActivitiesAdapterClickEventArgs> clickListener, Action<LastActivitiesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                ActivitiesImage = (ImageView)itemView.FindViewById(Resource.Id.Image);
                Username = itemView.FindViewById<TextView>(Resource.Id.LastActivitiesUserName);
                ActivitiesEvent = itemView.FindViewById<TextView>(Resource.Id.LastActivitiesText);
                Icon = itemView.FindViewById<ImageView>(Resource.Id.ImageIcon);
                Time = itemView.FindViewById<TextView>(Resource.Id.Time);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new LastActivitiesAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new LastActivitiesAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; set; }

        public ImageView ActivitiesImage { get; private set; }
        public TextView Username { get; private set; }
        public TextView ActivitiesEvent { get; private set; }
        public ImageView Icon { get; private set; }
        public TextView Time { get; private set; }
        #endregion
    }

    public class LastActivitiesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}