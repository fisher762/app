using Android.Views;
using Android.Widget;
using PixelPhotoClient.Classes.User;
using System;
using System.Collections.ObjectModel;
using Android.App;
using AndroidX.RecyclerView.Widget;
using PixelPhoto.Helpers.Utils;

namespace PixelPhoto.Activities.Search.Adapters
{
    public class SearchHashtagAdapter : RecyclerView.Adapter
    {
        public event EventHandler<SearchHashtagAdapterAdapterClickEventArgs> ItemClick;
        public event EventHandler<SearchHashtagAdapterAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<SearchUsersHastagsObject.HashObject> HashTagsList = new ObservableCollection<SearchUsersHastagsObject.HashObject>();
        
        public SearchHashtagAdapter(Activity context)
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

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HContact_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_PixHashtag, parent, false);
                var vh = new SearchHashtagAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is SearchHashtagAdapterViewHolder holder)
                {
                    var item = HashTagsList[position];
                    if (item != null)
                    {
                        holder.HashTagsCount.Text = item.UseNum + " " + ActivityContext.GetText(Resource.String.Lbl_Posts);
                        holder.Name.Text = Methods.FunString.DecodeString(item.Tag);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public override int ItemCount => HashTagsList?.Count ?? 0;

        public SearchUsersHastagsObject.HashObject GetItem(int position)
        {
            return HashTagsList[position];
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
         
        void OnClick(SearchHashtagAdapterAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(SearchHashtagAdapterAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class SearchHashtagAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public ImageView Image { get; set; }
        public TextView Name { get; private set; }
        public TextView Icon { get; private set; }
        public TextView HashTagsCount { get; private set; }

        #endregion

        public SearchHashtagAdapterViewHolder(View itemView, Action<SearchHashtagAdapterAdapterClickEventArgs> clickListener,  Action<SearchHashtagAdapterAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
               // Image = view.FindViewById<ImageView>(Resource.Id.ImageDisplay);
                Name = itemView.FindViewById<TextView>(Resource.Id.card_name);
                Icon = itemView.FindViewById<TextView>(Resource.Id.Icon);
                HashTagsCount = itemView.FindViewById<TextView>(Resource.Id.card_dist);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new SearchHashtagAdapterAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new SearchHashtagAdapterAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class SearchHashtagAdapterAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}