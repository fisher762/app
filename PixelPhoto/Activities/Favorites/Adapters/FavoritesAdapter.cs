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
using PixelPhotoClient.GlobalClass;
using IList = System.Collections.IList;

namespace PixelPhoto.Activities.Favorites.Adapters
{
    public class FavoritesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<FavoritesAdapterViewHolderClickEventArgs> ItemClick;
        public event EventHandler<FavoritesAdapterViewHolderClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;

        public ObservableCollection<PostsObject> FavoritesList = new ObservableCollection<PostsObject>();

        public FavoritesAdapter(Activity context)
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
                //Setup your layout here >> Style_LastActivities_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Featured_View, parent, false);
                var vh = new FavoritesAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is FavoritesAdapterViewHolder holder)
                {
                    var item = FavoritesList[position];
                    if (item != null)
                    {
                        switch (item.Type)
                        {
                            case "video":
                            {
                                holder.PlayIcon.Visibility = ViewStates.Visible;

                                if (holder.PlayIcon.Visibility != ViewStates.Visible)
                                    holder.PlayIcon.Visibility = ViewStates.Visible;

                                if (holder.TypeIcon.Text != IonIconsFonts.Videocam)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.TypeIcon, IonIconsFonts.Videocam);

                                if (holder.TypeIcon.Visibility != ViewStates.Visible)
                                    holder.TypeIcon.Visibility = ViewStates.Visible;
                                break;
                            }
                            case "gif":
                            {
                                if (holder.TypeIcon.Text != "GIF")
                                {
                                    holder.TypeIcon.Text = "GIF";
                                }

                                if (holder.TypeIcon.Visibility != ViewStates.Visible)
                                    holder.TypeIcon.Visibility = ViewStates.Visible;

                                holder.PlayIcon.Visibility = ViewStates.Gone;
                                break;
                            }
                            case "youtube":
                            {
                                holder.PlayIcon.Visibility = ViewStates.Visible;

                                if (holder.PlayIcon.Visibility != ViewStates.Visible)
                                    holder.PlayIcon.Visibility = ViewStates.Visible;

                                if (holder.TypeIcon.Text != IonIconsFonts.LogoYoutube)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.TypeIcon, IonIconsFonts.LogoYoutube);

                                if (holder.TypeIcon.Visibility != ViewStates.Visible)
                                    holder.TypeIcon.Visibility = ViewStates.Visible;
                                break;
                            }
                        }

                        GlideImageLoader.LoadImage(ActivityContext, item.MediaSet[0].File, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        public override int ItemCount => FavoritesList?.Count ?? 0;
         
        public PostsObject GetItem(int position)
        {
            return FavoritesList[position];
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

        void OnClick(FavoritesAdapterViewHolderClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(FavoritesAdapterViewHolderClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = FavoritesList[p0];
                if (item == null)
                    return Collections.SingletonList(p0);

                if (item?.MediaSet[0]?.Extra != "")
                {
                    d.Add(item.MediaSet[0].Extra);
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

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        } 
    }

    public class FavoritesAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic
        public View MainView { get; private set; }

        public ImageView Image { get; private set; }
        public ImageView PlayIcon { get; private set; }
        public TextView TypeIcon { get; private set; }
        #endregion

        public FavoritesAdapterViewHolder(View itemView, Action<FavoritesAdapterViewHolderClickEventArgs> clickListener, Action<FavoritesAdapterViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = (ImageView)itemView.FindViewById(Resource.Id.Image);
                TypeIcon = (TextView)itemView.FindViewById(Resource.Id.typeicon);
                PlayIcon = (ImageView)itemView.FindViewById(Resource.Id.playicon);
                //Create an Event
                itemView.Click += (sender, e) => clickListener(new FavoritesAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new FavoritesAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class FavoritesAdapterViewHolderClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}