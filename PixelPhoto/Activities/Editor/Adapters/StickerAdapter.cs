using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Model.Editor;
using PixelPhoto.Helpers.Utils;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.Editor.Adapters
{
    public class StickerAdapter : RecyclerView.Adapter
    {
        private readonly Activity ActivityContext;
        public LayoutInflater Inflater;
        public Dictionary<string, Bitmap> MstickerList = new Dictionary<string, Bitmap>();
        private readonly RequestOptions Options;

        public StickerAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                GetStickers();

              Options = new RequestOptions().Apply(RequestOptions.CenterCropTransform()
                    .CenterCrop()
                    .SetPriority(Priority.High).Override(AppSettings.ImagePostSize)
                    .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                    .Error(Resource.Drawable.ImagePlacholder)
                    .Placeholder(Resource.Drawable.ImagePlacholder));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => MstickerList?.Count ?? 0;

        public event EventHandler<StickerAdapterClickEventArgs> ItemClick;
        public event EventHandler<StickerAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> row_sticker
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.RowSticker, parent, false);
                var vh = new StickerAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is StickerAdapterViewHolder holder)
                {
                    var item = MstickerList.ElementAt(position).Key;
                    if (item != null)
                    { 
                        GlideImageLoader.LoadImage(ActivityContext, item, holder.ImgSticker, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                        Glide.With(ActivityContext)
                            .AsBitmap()
                            .Load(item)
                            .Apply(Options)
                            .Into(new MySimpleTarget(this, holder, position)); 
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private class MySimpleTarget : CustomTarget
        {
            private readonly StickerAdapter MAdapter;
            private StickerAdapterViewHolder ViewHolder;
            private readonly int Position;
            public MySimpleTarget(StickerAdapter adapter, StickerAdapterViewHolder viewHolder, int position)
            {
                try
                {
                    MAdapter = adapter;
                    ViewHolder = viewHolder;
                    Position = position;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
            public override void OnLoadCleared(Drawable p0) { }

            public override void OnResourceReady(Object resource, ITransition transition)
            {
                try
                {
                    if (MAdapter.MstickerList?.Count > 0)
                    {
                        var url = Stickers.StickerList.ElementAt(Position).Key;
                    

                        //var bitmap = Stickers.StickerList.ElementAt(Position).Value;
                        var data = MAdapter.MstickerList.FirstOrDefault(pair => pair.Key == url);
                        if (data.Value == null)
                        {
                            if (resource is Bitmap bitmap)
                            {
                                var index = Stickers.StickerList.FirstOrDefault(a => a.Key == url);
                                if (index.Key != null) Stickers.StickerList[index.Key] = bitmap;
                            }
                        } 
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            } 
        }

        public void GetStickers()
        {
            try
            {
                MstickerList = Stickers.StickerList;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public string GetItem(int position)
        {
            return MstickerList.ElementAt(position).Key;
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

        public void OnClick(StickerAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        public void OnLongClick(StickerAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class StickerAdapterViewHolder : RecyclerView.ViewHolder
    {
        public StickerAdapterViewHolder(View itemView, Action<StickerAdapterClickEventArgs> clickListener,Action<StickerAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                ImgSticker = itemView.FindViewById<ImageView>(Resource.Id.imgSticker);

                itemView.Click += (sender, e) => clickListener(new StickerAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new StickerAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public ImageView ImgSticker { get; private set; }
        public View MainView { get; }
    }

    public class StickerAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}