using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.Tabbes.Adapters
{
    public class ExploreAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<ExploreAdapterViewHolderClickEventArgs> ItemClick;
        public event EventHandler<ExploreAdapterViewHolderClickEventArgs> ItemLongClick;

        private readonly Context Context;
      
        public ObservableCollection<PostsObject> PostList = new ObservableCollection<PostsObject>();
        private readonly RequestBuilder FullGlideRequestBuilder;
        private readonly RequestOptions GlideRequestOptions;

        public ExploreAdapter(Context context)
        {
            try
            {
                Context = context;
                //FullGlideRequestBuilder = Glide.With(context).AsDrawable().SetDiskCacheStrategy(DiskCacheStrategy.Automatic).Override(500).Placeholder(new ColorDrawable(Color.ParseColor("#efefef")));
                GlideRequestOptions = new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All)
                    .Placeholder(new ColorDrawable(Color.ParseColor("#efefef"))).SetPriority(Priority.High) ;

                FullGlideRequestBuilder = Glide.With(Context).AsBitmap().Apply(GlideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));
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
                //Setup your layout here >> Style_Featured_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Featured_View, parent, false);
                var vh = new ExploreAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is ExploreAdapterViewHolder holder)
                {
                    var item = PostList[position];

                    if (item != null)
                    { 
                        var type = NewsFeedAdapter.GetPostType(item);
                        switch (type)
                        {
                            case NativeFeedType.Video:
                                holder.PlayIcon.Visibility = ViewStates.Visible;
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.TypeIcon, IonIconsFonts.Videocam); 
                                holder.TypeIcon.Visibility = ViewStates.Visible;
                                break;
                            case NativeFeedType.Gif:
                            {
                                var extra = item.MediaSet[0].Extra;
                                if (extra != null && (string.IsNullOrEmpty(item.MediaSet[0]?.Extra) && !extra.Contains("http")))
                                    item.MediaSet[0].Extra = item.MediaSet[0]?.File;

                                holder.TypeIcon.Text = Context.GetText(Resource.String.Lbl_Gif);
                                holder.PlayIcon.Visibility = ViewStates.Gone;
                                holder.TypeIcon.Visibility = ViewStates.Visible;
                                break;
                            }
                            case NativeFeedType.MultiPhoto:
                            case NativeFeedType.MultiPhoto2:
                            case NativeFeedType.MultiPhoto3:
                            case NativeFeedType.MultiPhoto4:
                            case NativeFeedType.MultiPhoto5:
                            case NativeFeedType.Photo:
                            {
                                var extra = item.MediaSet[0].Extra;
                                if (extra != null && (string.IsNullOrEmpty(item.MediaSet[0]?.Extra) && !extra.Contains("http")))
                                    item.MediaSet[0].Extra = item.MediaSet[0]?.File;
                             
                                holder.PlayIcon.Visibility = ViewStates.Gone;
                                holder.TypeIcon.Visibility = ViewStates.Gone;
                                break;
                            }
                            case NativeFeedType.Youtube:
                                holder.PlayIcon.Visibility = ViewStates.Visible;
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.TypeIcon, IonIconsFonts.LogoYoutube); 
                                holder.TypeIcon.Visibility = ViewStates.Visible;
                                break;
                            case NativeFeedType.Vimeo:
                                holder.PlayIcon.Visibility = ViewStates.Visible;
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.TypeIcon, IonIconsFonts.LogoVimeo);
                                holder.TypeIcon.Visibility = ViewStates.Visible;
                                break;
                            case NativeFeedType.Dailymotion:
                                holder.PlayIcon.Visibility = ViewStates.Visible;
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeBrands, holder.TypeIcon,  FontAwesomeIcon.Dailymotion);
                                holder.TypeIcon.Visibility = ViewStates.Visible;
                                break;
                            case NativeFeedType.PlayTube:
                                holder.PlayIcon.Visibility = ViewStates.Visible;
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.TypeIcon, IonIconsFonts.Videocam);
                                holder.TypeIcon.Visibility = ViewStates.Visible;
                                break;
                            case NativeFeedType.Funding: 
                            case NativeFeedType.AdMob1: 
                            case NativeFeedType.AdMob2: 
                            case NativeFeedType.FbNativeAds: 
                            case NativeFeedType.Nona:
                                break; 
                        }

                        if (type == NativeFeedType.Video)
                        {
                            if (!string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                            {
                                Glide.With(Context).AsBitmap().Apply(GlideRequestOptions).Load(item.MediaSet[0]?.Extra).Transition(new BitmapTransitionOptions().CrossFade(100)).Into(holder.Image);
                                //FullGlideRequestBuilder.Load(item.MediaSet[0]?.Extra).Into(holder.Image);
                            }
                            else
                            {
                                var fileName = item.MediaSet[0].File.Split('/').Last();
                                var fileNameWithoutExtension = fileName.Split('.').First();

                                item.MediaSet[0].Extra = Methods.Path.FolderDcimImage + "/" + fileNameWithoutExtension + ".png";

                                var vidoePlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");
                                if (vidoePlaceHolderImage == "File Dont Exists")
                                {
                                    var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(Context, item.MediaSet[0]?.File);
                                    if (bitmapImage != null)
                                    {
                                        Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, Methods.Path.FolderDcimImage);
                                        //FullGlideRequestBuilder.Load(bitmapImage).Into(holder.Image);
                                        Glide.With(Context).AsBitmap().Apply(GlideRequestOptions).Load(bitmapImage).Transition(new BitmapTransitionOptions().CrossFade(100)).Into(holder.Image);
                                    }
                                    else
                                    {
                                        Glide.With(Context)
                                            .AsBitmap()
                                            .Apply(new RequestOptions().Placeholder(Resource.Drawable.blackdefault).Error(Resource.Drawable.blackdefault))
                                            .Load(item.MediaSet[0]?.File) // or URI/path
                                            .Into(holder.Image); //image view to set thumbnail to 
                                    }
                                }
                                else
                                {
                                    //FullGlideRequestBuilder.Load(vidoePlaceHolderImage).Into(holder.Image);
                                    Glide.With(Context).AsBitmap().Apply(GlideRequestOptions).Load(vidoePlaceHolderImage).Transition(new BitmapTransitionOptions().CrossFade(100)).Into(holder.Image);
                                }
                            } 
                        }
                        else
                        {
                            var imageUri = item.MediaSet[0]?.File;
                            if (imageUri != null)
                                Glide.With(Context).AsBitmap().Apply(GlideRequestOptions).Load(imageUri).Transition(new BitmapTransitionOptions().CrossFade(100)).Into(holder.Image);
                            // FullGlideRequestBuilder.Load(imageUri).Into(holder.Image);
                           
                            // GlideImageLoader.LoadImage(ActivityContext, imageUri, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                        } 
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            } 
        }
        

        public override int ItemCount => PostList?.Count ?? 0;


        public PostsObject GetItem(int position)
        {
            return PostList[position];
        }

 
        void OnClick(ExploreAdapterViewHolderClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(ExploreAdapterViewHolderClickEventArgs args) => ItemLongClick?.Invoke(this, args);


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = PostList[p0];
                if (item == null)
                    return d;

                var type = NewsFeedAdapter.GetPostType(item);
                switch (type)
                {
                    case NativeFeedType.Video:
                    case NativeFeedType.Youtube:
                    case NativeFeedType.Vimeo:
                    case NativeFeedType.Dailymotion:
                    case NativeFeedType.PlayTube:
                        d.Add(item.MediaSet[0]?.Extra);
                        break;
                    case NativeFeedType.Gif:
                    case NativeFeedType.Photo:
                        if(!string.IsNullOrEmpty(item.MediaSet[0].Extra))
                            if (!item.MediaSet[0].Extra.Contains("http"))
                                 item.MediaSet[0].Extra = item.MediaSet[0]?.File;
                            


                        if (string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                            item.MediaSet[0].Extra = item.MediaSet[0]?.File;

                        d.Add(item.MediaSet[0]?.Extra);
                        break;
                    case NativeFeedType.MultiPhoto:
                    case NativeFeedType.MultiPhoto2:
                    case NativeFeedType.MultiPhoto3:
                    case NativeFeedType.MultiPhoto4:
                    case NativeFeedType.MultiPhoto5:
                        {
                        foreach (var image in item.MediaSet)
                        {
                            if (string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                            {
                                item.MediaSet[0].Extra = item.MediaSet[0]?.File;
                                image.Extra = image.File;
                            }
                             
                            d.Add(image.Extra);
                        }
                                
                        break;
                    } 
                }

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                var d = new List<string>();
                return d; 
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
           
            return FullGlideRequestBuilder.Load(p0.ToString());
            //return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }

    }

    public class ExploreAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic
        public View MainView { get; set; }
        public FrameLayout ViewFrm { get; private set; }
        public ImageView Image { get; private set; }
        public ImageView PlayIcon { get; private set; }
        public TextView TypeIcon { get; private set; }
        #endregion

        public ExploreAdapterViewHolder(View itemView, Action<ExploreAdapterViewHolderClickEventArgs> clickListener,Action<ExploreAdapterViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = (ImageView)itemView.FindViewById(Resource.Id.Image);
                TypeIcon = (TextView)itemView.FindViewById(Resource.Id.typeicon);
                PlayIcon = (ImageView)itemView.FindViewById(Resource.Id.playicon);
                ViewFrm = (FrameLayout)itemView.FindViewById(Resource.Id.viewfrm);
                //Create an Event
                ViewFrm.Click += (sender, e) => clickListener(new ExploreAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new ExploreAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class ExploreAdapterViewHolderClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}