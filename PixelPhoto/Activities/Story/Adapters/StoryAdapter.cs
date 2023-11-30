using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Story;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;


namespace PixelPhoto.Activities.Story.Adapters
{
    public class StoryAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    { 
        public event EventHandler<StoryAdapterClickEventArgs> ItemClick;
        public event EventHandler<StoryAdapterClickEventArgs> ItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<FetchStoriesObject.StoriesDataObject> StoryList = new ObservableCollection<FetchStoriesObject.StoriesDataObject>();

        public StoryAdapter(Activity context)
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
                //Setup your layout here >> Style_Story_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Story_view, parent, false);
                var vh = new StoryAdapterViewHolder(itemView, OnLongClick);
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

                if (viewHolder is StoryAdapterViewHolder holder)
                {
                    var item = StoryList[position];
                    if (item != null)
                    {
                        holder.Circleindicator.SetStatus(InsLoadingView.Status.Loading);
                        holder.Circleindicator.SetStartColor(Color.ParseColor(AppSettings.StartColor));
                        holder.Circleindicator.SetEndColor(Color.ParseColor(AppSettings.EndColor));

                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Circleindicator, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
 
                        holder.Name.Text = item.Username;

                        if (item.Type != "Your")
                        {
                            holder.Circleindicator.SetStatus(InsLoadingView.Status.UnClicked);
                            holder.IconCircle.Visibility = ViewStates.Gone;
                            holder.IconStory.Visibility = ViewStates.Gone;
                        }

                        if (!holder.Circleindicator.HasOnClickListeners)
                            holder.Circleindicator.Click += (sender, e) => OnClick(new StoryAdapterClickEventArgs { View = holder.MainView, Position = position });

                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        public override int ItemCount => StoryList?.Count ?? 0;
        public FetchStoriesObject.StoriesDataObject GetItem(int position)
        {
            return StoryList[position];
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
        public  void OnClick(StoryAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        
        void OnLongClick(StoryAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = StoryList[p0];
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

    public class StoryAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public TextView Name { get;private set; }
        public InsLoadingView Circleindicator { get; private set; }
        public View IconCircle { get; private set; }
        public TextView IconStory { get; private set; }

        #endregion

        public StoryAdapterViewHolder(View itemView, Action<StoryAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Name = itemView.FindViewById<TextView>(Resource.Id.Txt_Username);
                Circleindicator= itemView.FindViewById<InsLoadingView>(Resource.Id.profile_indicator);
                IconCircle = itemView.FindViewById<View>(Resource.Id.IconCircle);
                IconStory = itemView.FindViewById<TextView>(Resource.Id.IconStory);
              
                //Event
                //itemView.Click += (sender, e) => clickListener(new StoryAdapterClickEventArgs { View = itemView, Position = AdapterPosition }); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class StoryAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}