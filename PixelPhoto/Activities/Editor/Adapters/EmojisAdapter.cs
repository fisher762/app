using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.NiceArt;

namespace PixelPhoto.Activities.Editor.Adapters
{
    public class EmojisAdapter : RecyclerView.Adapter
    {
        private readonly Activity ActivityContext;
        public LayoutInflater Inflater;
        public ObservableCollection<string> MEmojisList = new ObservableCollection<string>();


        public EmojisAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                GetEmojis();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => MEmojisList?.Count ?? 0;

        public event EventHandler<EmojisAdapterClickEventArgs> ItemClick;
        public event EventHandler<EmojisAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager) 
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> row_emoji
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.RowEmoji, parent, false);
                var vh = new EmojisAdapterViewHolder(itemView, OnClick, OnLongClick);
                return vh;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is EmojisAdapterViewHolder holder)
                {
                    var item = MEmojisList[position];
                    if (item != null) holder.TxtEmoji.Text = item;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public void GetEmojis()
        {
            try
            {
                MEmojisList = new ObservableCollection<string>(NiceArtEditor.GetEmojis(ActivityContext));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public string GetItem(int position)
        {
            return MEmojisList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        public void OnClick(EmojisAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        public void OnLongClick(EmojisAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class EmojisAdapterViewHolder : RecyclerView.ViewHolder
    {
        public EmojisAdapterViewHolder(View itemView, Action<EmojisAdapterClickEventArgs> clickListener,
            Action<EmojisAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                TxtEmoji = itemView.FindViewById<TextView>(Resource.Id.txtEmoji);

                itemView.Click += (sender, e) => clickListener(new EmojisAdapterClickEventArgs
                    {View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new EmojisAdapterClickEventArgs
                    {View = itemView, Position = AdapterPosition});
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public TextView TxtEmoji { get; private set; }
        public View MainView { get; }
    }

    public class EmojisAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}