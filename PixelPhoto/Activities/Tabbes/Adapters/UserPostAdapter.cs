using Android.App;
using Android.Views;
using Android.Widget;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using PixelPhoto.Helpers.CacheLoaders;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.Tabbes.Adapters
{
    public class UserPostAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<UserPostAdapterViewHolderClickEventArgs> ItemClick;
        public event EventHandler<UserPostAdapterViewHolderClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PostsObject> PostList = new ObservableCollection<PostsObject>();
        private readonly RequestBuilder FullGlideRequestBuilder;

        public UserPostAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                FullGlideRequestBuilder = Glide.With(context).AsDrawable().SetDiskCacheStrategy(DiskCacheStrategy.Automatic).Error(Resource.Drawable.ImagePlacholder).Placeholder(new ColorDrawable(Color.ParseColor("#efefef")));
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
                var vh = new UserPostAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is UserPostAdapterViewHolder holder)
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
                                holder.TypeIcon.Text = ActivityContext.GetText(Resource.String.Lbl_Gif);
                                holder.TypeIcon.Visibility = ViewStates.Visible;
                                break;
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
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeBrands, holder.TypeIcon, FontAwesomeIcon.Dailymotion);
                                holder.TypeIcon.Visibility = ViewStates.Visible;
                                break;
                            case NativeFeedType.PlayTube:
                                holder.PlayIcon.Visibility = ViewStates.Visible; 
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.TypeIcon, IonIconsFonts.Videocam);
                                holder.TypeIcon.Visibility = ViewStates.Visible;
                                break;
                            case NativeFeedType.Photo:
                                holder.PlayIcon.Visibility = ViewStates.Gone;
                                holder.TypeIcon.Visibility = ViewStates.Gone;
                                break;
                            case NativeFeedType.MultiPhoto:
                            case NativeFeedType.MultiPhoto2:
                            case NativeFeedType.MultiPhoto3:
                            case NativeFeedType.MultiPhoto4:
                            case NativeFeedType.MultiPhoto5:
                                holder.PlayIcon.Visibility = ViewStates.Gone;
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.TypeIcon, IonIconsFonts.Albums);
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
                            if (item.MediaSet?.Count > 0)
                            {
                                if (!string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                                {
                                    FullGlideRequestBuilder.Load(item.MediaSet[0]?.Extra).Into(holder.Image);
                                }
                                else
                                {
                                    var fileName = item.MediaSet[0].File.Split('/').Last();
                                    var fileNameWithoutExtension = fileName.Split('.').First();

                                    item.MediaSet[0].Extra = Methods.Path.FolderDcimImage + "/" + fileNameWithoutExtension + ".png";

                                    var vidoePlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");
                                    if (vidoePlaceHolderImage == "File Dont Exists")
                                    {
                                        var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(ActivityContext, item.MediaSet[0]?.File);
                                        if (bitmapImage != null)
                                        {
                                            Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, Methods.Path.FolderDcimImage);
                                            FullGlideRequestBuilder.Load(bitmapImage).Into(holder.Image);
                                        }
                                        else
                                        {
                                            Glide.With(ActivityContext)
                                                .AsBitmap()
                                                .Apply(new RequestOptions().Placeholder(Resource.Drawable.blackdefault).Error(Resource.Drawable.blackdefault))
                                                .Load(item.MediaSet[0]?.File) // or URI/path
                                                .Into(holder.Image); //image view to set thumbnail to 
                                        }
                                    }
                                    else
                                    {
                                        FullGlideRequestBuilder.Load(vidoePlaceHolderImage).Into(holder.Image);
                                    }
                                }
                            } 
                        }
                        else
                        {
                            //FullGlideRequestBuilder.Load(!string.IsNullOrEmpty(item.MediaSet[0].Extra) ? item.MediaSet[0].Extra : item.MediaSet[0].File).Into(holder.Image);
                            if (item.MediaSet?.Count > 0)
                                GlideImageLoader.LoadImage(ActivityContext, !string.IsNullOrEmpty(item.MediaSet[0]?.File) ? item.MediaSet[0].File : item.MediaSet[0].Extra, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Color);
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

        private void OnClick(UserPostAdapterViewHolderClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(UserPostAdapterViewHolderClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

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
                        d.Add(item.MediaSet[0]?.Extra);
                        break;
                    case NativeFeedType.Gif:
                    case NativeFeedType.Photo:
                        if (!string.IsNullOrEmpty(item.MediaSet[0].Extra))
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

    public class UserPostAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic
        public View MainView { get; private set; }
        public FrameLayout ViewFrm { get; private set; }
        public ImageView Image { get; private set; }
        public ImageView PlayIcon { get; private set; }
        public TextView TypeIcon { get; private set; }
        #endregion

        public UserPostAdapterViewHolder(View itemView, Action<UserPostAdapterViewHolderClickEventArgs> clickListener, Action<UserPostAdapterViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = (ImageView)itemView.FindViewById(Resource.Id.Image);
                TypeIcon = (TextView)itemView.FindViewById(Resource.Id.typeicon);
                PlayIcon = (ImageView)itemView.FindViewById(Resource.Id.playicon);
                ViewFrm = (FrameLayout)itemView.FindViewById(Resource.Id.viewfrm);

                //Create an Event
                ViewFrm.Click += (sender, e) => clickListener(new UserPostAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new UserPostAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class UserPostAdapterViewHolderClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}