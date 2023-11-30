using Android.App;
using Android.Graphics;
using Android.Views;
using PixelPhoto.Activities.Posts.Adapters;
using PixelPhoto.Activities.Posts.Extras;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.Content;
using Android.Gms.Ads;
using Android.Gms.Ads.Formats;
using Android.OS;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using PixelPhoto.Activities.Posts.page;
using PixelPhoto.Activities.Search;
using PixelPhoto.Library.Anjo.SuperTextLibrary;
using Xamarin.Facebook.Ads;
using static PixelPhoto.Activities.Posts.Adapters.Holders;
using Exception = System.Exception;
using NativeAd = Xamarin.Facebook.Ads.NativeAd;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.Tabbes.Adapters
{
    public enum NativeFeedType
    {
        Photo = 21010, MultiPhoto2 = 21011, MultiPhoto3 = 21012, MultiPhoto4 = 21013, MultiPhoto5 = 21014, MultiPhoto = 21015, Video = 21016, Youtube = 21017, Gif = 21018, Funding = 21019, Vimeo = 21020, Dailymotion = 21021,
        PlayTube = 21022, AdMob1 = 210100, AdMob2 = 2101001, FbNativeAds = 210111, Nona = 210110
    }
     
    public class NewsFeedAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, StTools.IXAutoLinkOnClickListener, UnifiedNativeAd.IOnUnifiedNativeAdLoadedListener, IOnPostItemClickListener
    {
        public event EventHandler<PostAdapterClickEventArgs> ItemClick;
        public event EventHandler<PostAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        private StReadMoreOption ReadMoreOption { get; set; }
        public readonly SocialIoClickListeners ClickListeners;
        public ObservableCollection<PostsObject> PostList = new ObservableCollection<PostsObject>();
        
        private RecyclerView MainRecyclerView { get; set; }
        public readonly RequestBuilder FullGlideRequestBuilder;
        public FundingViewHolder HolderFunding { get; set; }
        public readonly IOnPostItemClickListener OnPostItemClickListener;
        public List<NativeAd> MAdItems;
        public NativeAdsManager MNativeAdsManager;
        private RecyclerView.RecycledViewPool RecycledViewPool { get; set; }

        public NewsFeedAdapter(Activity context, PRecyclerView recyclerView)
        {
            try
            {
                ActivityContext = context;
                MainRecyclerView = recyclerView;

                //Constructor stuff
                RecycledViewPool = new RecyclerView.RecycledViewPool(); 
                var mLayoutManager = new PreCachingLayoutManager(ActivityContext)
                {
                    Orientation = LinearLayoutManager.Vertical
                };

                mLayoutManager.SetPreloadItemCount(5);

                MainRecyclerView.SetLayoutManager(mLayoutManager);
                MainRecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;

                MAdItems = new List<NativeAd>();
                BindAdFb();
                 
                var glideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High).Transform(new MultiTransformation(new RoundedCorners(50)));
                FullGlideRequestBuilder = Glide.With(context).AsBitmap().Apply(glideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100)).Thumbnail(0.5f).SetUseAnimationPool(false);
                 
                //FullGlideRequestBuilder = Glide.With(context).AsDrawable().SetDiskCacheStrategy(DiskCacheStrategy.Automatic).SkipMemoryCache(true).Override(550).Placeholder(new ColorDrawable(Color.ParseColor("#efefef")));

                var sizeProvider = new FixedPreloadSizeProvider(300, 300);
                var preLoader = new RecyclerViewPreloader<PostsObject>(context, this, sizeProvider, 10);
                MainRecyclerView.AddOnScrollListener(preLoader);

                MainRecyclerView.SetAdapter(this);

                PostList = new ObservableCollection<PostsObject>();

                ReadMoreOption = new StReadMoreOption.Builder()
                    .TextLength(200, StReadMoreOption.TypeCharacter)
                    .MoreLabel(ActivityContext.GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(ActivityContext.GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();

                ClickListeners = new SocialIoClickListeners(context);
                OnPostItemClickListener = this; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public PostsObject GetItem(int position)
        {
            var item = PostList[position];
            return item;
        }

        public static NativeFeedType GetPostType(PostsObject item)
        {
            try
            {
                switch (item.Type)
                {
                    case "video":
                    case "Video":
                        return NativeFeedType.Video;
                    case "youtube":
                    case "Youtube":
                        return NativeFeedType.Youtube;
                    case "vimeo":
                    case "Vimeo":
                        return NativeFeedType.Vimeo;
                    case "dailymotion":
                    case "Dailymotion":
                        return NativeFeedType.Dailymotion;
                    case "playtube":
                    case "Playtube":
                        return NativeFeedType.PlayTube;
                    case "gif":
                    case "Gif":
                        return NativeFeedType.Gif;
                    case "Funding":
                        return NativeFeedType.Funding;
                    case "AdMob1":
                        return NativeFeedType.AdMob1;
                    case "AdMob2":
                        return NativeFeedType.AdMob2;
                    case "FbNativeAds":
                        return NativeFeedType.FbNativeAds;
                    case "image":
                    case "Image":
                    {
                        switch (item.MediaSet.Count)
                        {
                            case 1:
                                return NativeFeedType.Photo;
                            case 2:
                                return NativeFeedType.MultiPhoto2;
                            case 3:
                                return NativeFeedType.MultiPhoto3;
                            case 4:
                                return NativeFeedType.MultiPhoto4;
                        }

                        if (item.MediaSet.Count >= 5)
                        {
                            return NativeFeedType.MultiPhoto5;
                        }

                        return item.MediaSet.Count > 1 ? NativeFeedType.MultiPhoto : NativeFeedType.Photo;
                    } 
                    default:
                        {
                            if (item.Type == "image" || item.Type == "Image")
                            {
                                switch (item.MediaSet.Count)
                                {
                                    case 1:
                                        return NativeFeedType.Photo;
                                    case 2:
                                        return NativeFeedType.MultiPhoto2;
                                    case 3:
                                        return NativeFeedType.MultiPhoto3;
                                    case 4:
                                        return NativeFeedType.MultiPhoto4;
                                }

                                if (item.MediaSet.Count >= 5)
                                {
                                    return NativeFeedType.MultiPhoto5;
                                } 

                                return item.MediaSet.Count > 1 ? NativeFeedType.MultiPhoto : NativeFeedType.Photo;
                            }
                            else
                            {
                                return NativeFeedType.Photo;
                            }
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return NativeFeedType.Nona;
            }
        }
         
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                View itemView;

                switch (viewType)
                {
                    case (int)NativeFeedType.Video:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_VideoFeed, parent, false);
                        var vh = new VideoAdapterViewHolder(itemView, OnClick, OnLongClick, OnPostItemClickListener, this);
                        return vh;
                    }
                    case (int)NativeFeedType.Youtube:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_YoutubeFeed, parent, false);
                        var vh = new YoutubeAdapterViewHolder(itemView, OnClick, OnLongClick, OnPostItemClickListener, this);
                        return vh;
                    }
                    case (int)NativeFeedType.Vimeo:
                    case (int)NativeFeedType.Dailymotion:
                    case (int)NativeFeedType.PlayTube:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_FetchedFeed, parent, false);
                        var vh = new FetchedFeedViewHolder(itemView, OnClick, OnLongClick, OnPostItemClickListener);
                        return vh;
                    }
                    case (int)NativeFeedType.Gif:
                    case (int)NativeFeedType.Photo:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_PhotoFeed, parent, false);
                        var vh = new PhotoAdapterViewHolder(itemView, OnClick, OnLongClick, OnPostItemClickListener);
                        return vh;
                    }
                    case (int)NativeFeedType.Funding:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                        var vh = new FundingViewHolder(ActivityContext, itemView);
                        RecycledViewPool = new RecyclerView.RecycledViewPool();
                        vh.StoryRecyclerView.SetRecycledViewPool(RecycledViewPool);
                        return vh;
                    }
                    case (int)NativeFeedType.AdMob1:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.PostType_AdMob, parent, false);
                        var vh = new AdMobAdapterViewHolder(itemView , this);
                        return vh;
                    }
                    case (int)NativeFeedType.AdMob2:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.PostType_AdMob2, parent, false);
                        var vh = new AdMobAdapterViewHolder(itemView, this);
                        return vh;
                    }
                    case (int)NativeFeedType.FbNativeAds:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.PostType_FbNativeAd, parent, false);
                        var vh = new FbAdNativeAdapterViewHolder(ActivityContext ,itemView, this);
                        return vh;
                    }
                    case (int)NativeFeedType.MultiPhoto:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MultiPhotoFeed, parent, false);
                        var vh = new MultiPhotoAdapterViewHolder(itemView, OnClick, OnLongClick, OnPostItemClickListener);
                        return vh;
                    }
                    case (int)NativeFeedType.MultiPhoto2:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Photo2Feed, parent, false);
                        var vh = new Photo2AdapterViewHolder(itemView, OnClick, OnLongClick, OnPostItemClickListener ,  2);
                        return vh;
                    }
                    case (int)NativeFeedType.MultiPhoto3:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Photo3Feed, parent, false);
                        var vh = new Photo2AdapterViewHolder(itemView, OnClick, OnLongClick, OnPostItemClickListener,  3);
                        return vh;
                    }
                    case (int)NativeFeedType.MultiPhoto4:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Photo4Feed, parent, false);
                        var vh = new Photo2AdapterViewHolder(itemView, OnClick, OnLongClick, OnPostItemClickListener, 4);
                        return vh;
                    }
                    case (int)NativeFeedType.MultiPhoto5:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Photo5Feed, parent, false);
                        var vh = new Photo2AdapterViewHolder(itemView, OnClick, OnLongClick, OnPostItemClickListener, 5);
                        return vh;
                    }
                    default:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_PhotoFeed, parent, false);
                        var vh = new PhotoAdapterViewHolder(itemView, OnClick, OnLongClick, OnPostItemClickListener);
                        return vh;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override int ItemCount => PostList?.Count ?? 0;

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {

                if (payloads.Count > 0)
                {
                    if (payloads[0].ToString() == "FundingRefresh")
                    {
                        if (viewHolder is FundingViewHolder holder)
                        {
                            holder.RefreshData();
                        }
                    }
                    else
                    {
                        base.OnBindViewHolder(viewHolder, position, payloads);
                    }
                }
                else
                {
                    base.OnBindViewHolder(viewHolder, position, payloads);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (position >= 0)
                {
                    var itemViewType = viewHolder.ItemViewType;

                    var item = PostList[position];
                    if (item == null)
                        return;
                     
                    switch (itemViewType)
                    { 
                        case (int)NativeFeedType.Video:
                            {
                                if (viewHolder is VideoAdapterViewHolder holder)
                                {
                                    if (item.MediaSet?.Count > 0)
                                    {
                                        if (!string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                                        {
                                            FullGlideRequestBuilder.Load(item.MediaSet[0]?.Extra).Into(holder.VideoImage);
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
                                                    FullGlideRequestBuilder.Load(bitmapImage).Into(holder.VideoImage);
                                                }
                                                else
                                                {
                                                    Glide.With(ActivityContext)
                                                        .AsBitmap()
                                                        .Placeholder(Resource.Drawable.blackdefault)
                                                        .Error(Resource.Drawable.blackdefault)
                                                        .Load(item.MediaSet[0]?.File) // or URI/path
                                                        .Into(holder.VideoImage); //image view to set thumbnail to 
                                                }
                                            }
                                            else
                                            {
                                                FullGlideRequestBuilder.Load(vidoePlaceHolderImage).Into(holder.VideoImage);
                                            } 
                                        } 
                                    }

                                    SetDataDynamicForViewHolder(holder.GlobalPostViews, item);
                                    holder.GlobalPostViews.ViewCount.Text = item.Views + " " + ActivityContext.GetText(Resource.String.Lbl_Views);
                                    holder.MainView.Tag = holder; 
                                }
                                break;
                            }
                        case (int)NativeFeedType.Youtube:
                            {
                                if (viewHolder is YoutubeAdapterViewHolder holder)
                                {
                                    SetDataDynamicForViewHolder(holder.GlobalPostViews, item);

                                    if (item.MediaSet?.Count > 0)
                                    {
                                        FullGlideRequestBuilder.Load(item.MediaSet[0].Extra).Into(holder.Image); 
                                    }

                                    if (!holder.PlayButton.HasOnClickListeners)
                                    {
                                        holder.PlayButton.Click += (sender, args) =>
                                            ClickListeners.OnPlayYoutubeButtonClicked(new YoutubeVideoClickEventArgs
                                            {
                                                Holder = holder,
                                                NewsFeedClass = item,
                                                Position = position,
                                                View = holder.MainView
                                            });
                                    }
                                }
                                break;
                            }
                        case (int)NativeFeedType.Photo:
                        case (int)NativeFeedType.Gif:
                            {
                                if (viewHolder is PhotoAdapterViewHolder holder)
                                {
                                    SetDataDynamicForViewHolder(holder.GlobalPostViews, item);

                                    if (item.MediaSet?.Count > 0)
                                    {
                                        var url = !string.IsNullOrEmpty(item.MediaSet[0]?.File) ? item.MediaSet[0].File : item.MediaSet[0].Extra;

                                        switch (itemViewType)
                                        {
                                            case (int)NativeFeedType.Photo:
                                                FullGlideRequestBuilder.Load(url).Into(holder.Image);
                                                break;
                                            case (int)NativeFeedType.Gif:
                                                Glide.With(ActivityContext).Load(url).Apply(new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Transform(new MultiTransformation(new RoundedCorners(50)))).Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image); 
                                                break;
                                        } 
                                    }
                                }
                                break;
                            } 
                        case (int)NativeFeedType.MultiPhoto:
                            {
                                if (viewHolder is MultiPhotoAdapterViewHolder holder)
                                {
                                    SetDataDynamicForViewHolder(holder.GlobalPostViews, item);
                                    if (item.MediaSet?.Count > 0)
                                    {
                                        var list = item.MediaSet.Select(image => image.File).ToList();

                                        holder.ViewPagerLayout.Adapter = new MultiImagePagerAdapter(ActivityContext, list);

                                        holder.ViewPagerLayout.CurrentItem = 0;
                                        holder.CircleIndicatorView.SetViewPager(holder.ViewPagerLayout);
                                    } 
                                }
                                break;
                            }
                        case (int)NativeFeedType.MultiPhoto2:
                            {
                                if (viewHolder is Photo2AdapterViewHolder holder)
                                {
                                    SetDataDynamicForViewHolder(holder.GlobalPostViews, item);
                                    if (item.MediaSet?.Count > 0)
                                    {
                                        var list = item.MediaSet.Select(image => image.File).ToList();

                                        FullGlideRequestBuilder.Load(list[0]).Into(holder.Image);
                                        FullGlideRequestBuilder.Load(list[1]).Into(holder.Image2);

                                        //Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Load(list[0]).Transition(new BitmapTransitionOptions().CrossFade(100)) .Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image);
                                        //Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Load(list[1]).Transition(new BitmapTransitionOptions().CrossFade(100)).Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image2);
                                    }
                                }
                                break;
                            }
                        case (int)NativeFeedType.MultiPhoto3:
                            {
                                if (viewHolder is Photo2AdapterViewHolder holder)
                                {
                                    SetDataDynamicForViewHolder(holder.GlobalPostViews, item);
                                    if (item.MediaSet?.Count > 0)
                                    {
                                        var list = item.MediaSet.Select(image => image.File).ToList();

                                        FullGlideRequestBuilder.Load(list[0]).Into(holder.Image);
                                        FullGlideRequestBuilder.Load(list[1]).Into(holder.Image2);
                                        FullGlideRequestBuilder.Load(list[2]).Into(holder.Image3);
                                         
                                        //Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Load(list[0]).Transition(new BitmapTransitionOptions().CrossFade(100)).Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image);
                                        //Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Load(list[1]).Transition(new BitmapTransitionOptions().CrossFade(100)).Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image2);
                                        //Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Load(list[2]).Transition(new BitmapTransitionOptions().CrossFade(100)).Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image3);
                                    } 
                                }
                                break;
                            }
                        case (int)NativeFeedType.MultiPhoto4:
                            {
                                if (viewHolder is Photo2AdapterViewHolder holder)
                                {
                                    SetDataDynamicForViewHolder(holder.GlobalPostViews, item);
                                    if (item.MediaSet?.Count > 0)
                                    {
                                        var list = item.MediaSet.Select(image => image.File).ToList();

                                        FullGlideRequestBuilder.Load(list[0]).Into(holder.Image);
                                        FullGlideRequestBuilder.Load(list[1]).Into(holder.Image2);
                                        FullGlideRequestBuilder.Load(list[2]).Into(holder.Image3);
                                        FullGlideRequestBuilder.Load(list[3]).Into(holder.Image4);

                                        //Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Load(list[0]).Transition(new BitmapTransitionOptions().CrossFade(100)).Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image);
                                        //Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Load(list[1]).Transition(new BitmapTransitionOptions().CrossFade(100)).Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image2);
                                        //Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Load(list[2]).Transition(new BitmapTransitionOptions().CrossFade(100)).Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image3);
                                        //Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Load(list[3]).Transition(new BitmapTransitionOptions().CrossFade(100)).Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image4);
                                    }
                                }
                                break;
                            }
                        case (int)NativeFeedType.MultiPhoto5:
                            {
                                if (viewHolder is Photo2AdapterViewHolder holder)
                                {
                                    SetDataDynamicForViewHolder(holder.GlobalPostViews, item);
                                    if (item.MediaSet?.Count > 0)
                                    {
                                        var list = item.MediaSet.Select(image => image.File).ToList();

                                        FullGlideRequestBuilder.Load(list[0]).Into(holder.Image);
                                        FullGlideRequestBuilder.Load(list[1]).Into(holder.Image2);
                                        FullGlideRequestBuilder.Load(list[2]).Into(holder.Image3);

                                        //Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Load(list[0]).Transition(new BitmapTransitionOptions().CrossFade(100)).Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image);
                                        //Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Load(list[1]).Transition(new BitmapTransitionOptions().CrossFade(100)).Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image2);
                                        //Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Load(list[2]).Transition(new BitmapTransitionOptions().CrossFade(100)).Thumbnail(0.5f).SetUseAnimationPool(false).Into(holder.Image3);

                                        //90000000 
                                        holder.Image3.SetColorFilter(Color.Argb(120, 0,0,0)); 
                                        holder.CountImageLabel.Text = "+" + (list.Count - 2);
                                    }
                                }
                                break;
                            }

                        case (int)NativeFeedType.Vimeo:
                            {
                                if (viewHolder is FetchedFeedViewHolder holder)
                                {
                                    SetDataDynamicForViewHolder(holder.GlobalPostViews, item);
                                     
                                    var fullEmbedUrl = "https://player.vimeo.com/video/" + item.Vimeo;

                                    var vc = holder.WebView.LayoutParameters;
                                    vc.Height = 700;
                                    holder.WebView.LayoutParameters = vc;

                                    holder.WebView.LoadUrl(fullEmbedUrl); 
                                }
                                break;
                            }
                        case (int)NativeFeedType.Dailymotion:
                            {
                                if (viewHolder is FetchedFeedViewHolder holder)
                                {
                                    SetDataDynamicForViewHolder(holder.GlobalPostViews, item);

                                    var fullEmbedUrl = "https://www.dailymotion.com/embed/video/" + item.Dailymotion;

                                    var vc = holder.WebView.LayoutParameters;
                                    vc.Height = 600;
                                    holder.WebView.LayoutParameters = vc;

                                    holder.WebView.LoadUrl(fullEmbedUrl);
                                }
                                break;
                            }
                        case (int)NativeFeedType.PlayTube:
                            {
                                if (viewHolder is FetchedFeedViewHolder holder)
                                {
                                    SetDataDynamicForViewHolder(holder.GlobalPostViews, item);
                                     
                                    var fullEmbedUrl = ListUtils.SettingsSiteList?.PlaytubeUrl + "/embed/" + item.Playtube;

                                    var vc = holder.WebView.LayoutParameters;
                                    vc.Height = 600;
                                    holder.WebView.LayoutParameters = vc;

                                    holder.WebView.LoadUrl(fullEmbedUrl);
                                }
                                break;
                            }
                        case (int)NativeFeedType.Funding:
                        case (int)NativeFeedType.AdMob1:
                        case (int)NativeFeedType.AdMob2:
                        case (int)NativeFeedType.FbNativeAds:
                            { 
                                break;
                            } 
                    } 
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void BindAdFb()
        {
            try
            {
                if (AppSettings.ShowFbNativeAds && MAdItems?.Count == 0)
                {
                    MNativeAdsManager = new NativeAdsManager(ActivityContext, AppSettings.AdsFbNativeKey, 5);
                    MNativeAdsManager.LoadAds();
                    MNativeAdsManager.SetListener(new MyNativeAdsManagerListener(this));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private TemplateView Template;
        public void BindAdMob(AdMobAdapterViewHolder holder)
        {
            try
            {
                Template = holder.MianAlert;

                var builder = new AdLoader.Builder(holder.MainView.Context, AppSettings.AdAdMobNativeKey);
                builder.ForUnifiedNativeAd(this);

                var videoOptions = new VideoOptions.Builder()
                    .SetStartMuted(true)
                    .Build();

                var adOptions = new NativeAdOptions.Builder()
                    .SetVideoOptions(videoOptions)
                    .Build();

                builder.WithNativeAdOptions(adOptions);

                var adLoader = builder.WithAdListener(new AdListener()).Build();
                adLoader.LoadAd(new AdRequest.Builder().Build());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnUnifiedNativeAdLoaded(UnifiedNativeAd ad)
        {
            try
            {
                if (Template.GetTemplateTypeName() == TemplateView.NativeContentAd)
                {
                    Template.NativeContentAdView(ad);
                }
                else
                {
                    var styles = new NativeTemplateStyle.Builder().Build();

                    Template.SetStyles(styles);
                    Template.SetNativeAd(ad);
                }

                ActivityContext?.RunOnUiThread(() =>
                {
                    Template.Visibility = ViewStates.Visible;
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
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
                var type = GetPostType(PostList[position]);
                return (int)type;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void OnClick(PostAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(PostAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        private void SetDataDynamicForViewHolder(GlobalPostViews views, PostsObject item)
        {
            try
            {
                if (item == null)
                    return;
                 
                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, views.UserAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                views.Username.Text = item.Username;

                if (item.UserData?.Verified == "1")
                    views.Username.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_small_vector, 0);

                if (item.UserData?.BusinessAccount == "1")
                    views.Username.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_dolar_small_vector, 0);
                 
                var time = Methods.Time.TimeAgo(Convert.ToInt32(item.Time), false);
                views.TimeText.Text = time;

                if (!string.IsNullOrEmpty(item.Description) && !string.IsNullOrWhiteSpace(item.Description))
                {
                    views.Description.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());
                    ReadMoreOption.AddReadMoreTo(views.Description, new Java.Lang.String(item.Description));
                    views.Description.Visibility = ViewStates.Visible;
                }
                else
                {
                    views.Description.Visibility = ViewStates.Gone;
                }

                if (item.Boosted == 1)
                {
                    views.IsPromoted.Text = ActivityContext.GetString(Resource.String.Lbl_Promoted);
                    views.IsPromoted.Visibility = ViewStates.Visible;
                    views.TimeText.Text = "";
                }

                views.Likeicon.Tag = item.IsLiked != null && item.IsLiked.Value ? "Like" : "Liked";
                ClickListeners.SetLike(views.Likeicon);

                views.Favicon.Tag = item.IsSaved != null && item.IsSaved.Value ? "Add" : "Added";
                ClickListeners.SetFav(views.Favicon);

                views.CommentCount.Text = item.Votes + " " + ActivityContext.GetString(Resource.String.Lbl_Comments);
                views.LikeCount.Text = item.Likes + " " + ActivityContext.GetText(Resource.String.Lbl_Likes);

                if (item.Votes > 0)
                {
                    views.ViewMoreComment.Visibility = ViewStates.Visible;
                    views.ViewMoreComment.Text = ActivityContext.GetString(Resource.String.Lbl_ShowAllComments);
                }
                else
                {
                    views.ViewMoreComment.Visibility = ViewStates.Gone;
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (holder != null)
                {
                    switch (holder)
                    {
                        case VideoAdapterViewHolder viewHolder:
                            if (viewHolder.VideoImage != null) Glide.With(ActivityContext).Clear(viewHolder.VideoImage);
                            break;
                        case YoutubeAdapterViewHolder viewHolder:
                            if (viewHolder.Image != null) Glide.With(ActivityContext).Clear(viewHolder.Image);
                            break;
                        case PhotoAdapterViewHolder viewHolder:
                            if (viewHolder.Image != null) Glide.With(ActivityContext).Clear(viewHolder.Image);
                            break;
                        case Photo2AdapterViewHolder viewHolder:
                            if (viewHolder.Image != null) Glide.With(ActivityContext).Clear(viewHolder.Image);
                            if (viewHolder.Image2 != null) Glide.With(ActivityContext).Clear(viewHolder.Image2);
                            if (viewHolder.Image3 != null) Glide.With(ActivityContext).Clear(viewHolder.Image3);
                            if (viewHolder.Image4 != null) Glide.With(ActivityContext).Clear(viewHolder.Image4);
                            break;
                    }
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = PostList[p0];
                if (item == null || item.MediaSet?.Count == 0 || item.MediaSet == null)
                    return d;

                var type = GetPostType(item);
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
        }

        public void OnLikeNewsFeedClick(LikeNewsFeedClickEventArgs eventArgs, int position)
        {
            try
            {
                var item = PostList[position];
                if (item != null)
                {
                    eventArgs.NewsFeedClass = item;
                    ClickListeners.OnLikeNewsFeedClick(eventArgs);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnFavNewsFeedClick(FavNewsFeedClickEventArgs e, int position)
        {
            try
            {
                var item = PostList[position];
                if (item != null)
                {
                    e.NewsFeedClass = item;
                    ClickListeners.OnFavNewsFeedClick(e);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnShareClick(GlobalClickEventArgs e, int position)
        {
            try
            {
                var item = PostList[position];
                if (item != null)
                {
                    e.NewsFeedClass = item;
                    ClickListeners.OnShareClick(e);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnCommentClick(GlobalClickEventArgs e, int position)
        {
            try
            {
                var item = PostList[position];
                if (item != null)
                {
                    e.NewsFeedClass = item;
                    ClickListeners.OnCommentClick(e, "Newsfeed");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnAvatarImageFeedClick(AvatarFeedClickEventArgs e, int position)
        {
            try
            {
                var item = PostList[position];
                if (item != null)
                {
                    e.NewsFeedClass = item;
                    ClickListeners.OnAvatarImageFeedClick(e);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnLikedLabelPostClick(LikeNewsFeedClickEventArgs e, int position)
        {
            try
            {
                var item = PostList[position];
                if (item != null)
                {
                    e.NewsFeedClass = item;
                    ClickListeners.OnLikedPostClick(e);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnCommentPostClick(GlobalClickEventArgs e, int position)
        {
            try
            {
                var item = PostList[position];
                if (item != null)
                {
                    e.NewsFeedClass = item;
                    ClickListeners.OnCommentPostClick(e, null);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnMoreClick(GlobalClickEventArgs e, bool isShow = false, int position = 0)
        {
            try
            {
                var item = PostList[position];
                if (item != null)
                {
                    e.NewsFeedClass = item;
                    ClickListeners.OnMoreClick(e, true);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ImagePostClick(GlobalClickEventArgs e, int position , int positionItem)
        {
            try
            {
                var item = PostList[position];
                if (item != null)
                {
                    e.NewsFeedClass = item;
                    ClickListeners.ImagePostClick(e, positionItem);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void AutoLinkTextClick(StTools.XAutoLinkMode p0, string p1, Dictionary<string, string> userData)
        {
            try
            {
                var typetext = Methods.FunString.Check_Regex(p1);
                if (typetext == "Email")
                {
                    Methods.App.SendEmail(ActivityContext, p1);
                }
                else if (typetext == "Website")
                {
                    var url = p1;
                    if (!p1.Contains("http"))
                    {
                        url = "http://" + p1;
                    }

                    var intent = new Intent(Application.Context, typeof(LocalWebViewActivity));
                    intent.PutExtra("URL", url);
                    intent.PutExtra("Type", url);
                    ActivityContext.StartActivity(intent);
                }
                else if (typetext == "Hashtag")
                {
                    // Show All Post By Hash
                    var bundle = new Bundle();
                    bundle.PutString("HashId", "");
                    bundle.PutString("HashName", Methods.FunString.DecodeString(p1));

                    var profileFragment = new HashTagPostFragment
                    {
                        Arguments = bundle
                    };

                    HomeActivity.GetInstance().OpenFragment(profileFragment);
                }
                else if (typetext == "Mention")
                {
                    var bundle = new Bundle();
                    bundle.PutString("Key", Methods.FunString.DecodeString(p1));

                    var searchFragment = new SearchFragment
                    {
                        Arguments = bundle
                    };

                    HomeActivity.GetInstance().OpenFragment(searchFragment);
                }
                else if (typetext == "Number")
                {
                    // IMethods.App.SaveContacts(_activity, autoLinkOnClickEventArgs.P1, "", "2");
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        } 
    }
}