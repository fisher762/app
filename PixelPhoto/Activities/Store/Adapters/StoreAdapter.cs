using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
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
using PixelPhotoClient.Classes.Store;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.Store.Adapters
{
    public class StoreAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<StoreAdapterViewHolderClickEventArgs> MoreItemClick;
        public event EventHandler<StoreAdapterViewHolderClickEventArgs> ItemClick;
        public event EventHandler<StoreAdapterViewHolderClickEventArgs> ItemLongClick;

        public ObservableCollection<StoreDataObject> StoreList = new ObservableCollection<StoreDataObject>();
        private readonly RequestBuilder FullGlideRequestBuilder;
        private readonly Activity ActivityContext;

        public StoreAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                var glideRequestOptions = new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).Placeholder(new ColorDrawable(Color.ParseColor("#efefef"))).SetPriority(Priority.High);
                FullGlideRequestBuilder = Glide.With(context).AsBitmap().Apply(glideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));
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
                //Setup your layout here >> Style_StoreView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_StoreView, parent, false);
                var vh = new StoreAdapterViewHolder(itemView, MoreOnClick, OnClick, OnLongClick);
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
                if (viewHolder is StoreAdapterViewHolder holder)
                {
                    var item = StoreList[position];
                    if (item != null)
                    {
                        FullGlideRequestBuilder.Load(item.Thumb).Into(holder.Image);

                        holder.TxtCategory.Text = item.CategoryName;
                        holder.TxtTitle.Text = item.Title;

                        string licenseType, price;
                        if (item.LicenseOptions?.LicenseOptionsClass != null)
                        {
                            if (item.LicenseOptions?.LicenseOptionsClass.RightsManagedLicense > 0)
                            {
                                licenseType = ActivityContext.GetText(Resource.String.Lbl_rights_managed_license);
                                price = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.RightsManagedLicense.Value;
                            }
                            else if (item.LicenseOptions?.LicenseOptionsClass.EditorialUseLicense > 0)
                            {
                                licenseType = ActivityContext.GetText(Resource.String.Lbl_editorial_use_license);
                                price = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.EditorialUseLicense.Value;
                            }
                            else if (item.LicenseOptions?.LicenseOptionsClass.RoyaltyFreeLicense > 0)
                            {
                                licenseType = ActivityContext.GetText(Resource.String.Lbl_royalty_free_license);
                                price = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.RoyaltyFreeLicense.Value;
                            }
                            else if (item.LicenseOptions?.LicenseOptionsClass.RoyaltyFreeExtendedLicense > 0)
                            {
                                licenseType = ActivityContext.GetText(Resource.String.Lbl_royalty_free_extended_license);
                                price = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.RoyaltyFreeExtendedLicense.Value;
                            }
                            else if (item.LicenseOptions?.LicenseOptionsClass.CreativeCommonsLicense > 0)
                            {
                                licenseType = ActivityContext.GetText(Resource.String.Lbl_creative_commons_license);
                                price = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.CreativeCommonsLicense.Value;
                            }
                            else if (item.LicenseOptions?.LicenseOptionsClass.PublicDomain > 0)
                            {
                                licenseType = ActivityContext.GetText(Resource.String.Lbl_public_domain);
                                price = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.PublicDomain.Value;
                            }
                            else if (item.LicenseOptions?.LicenseOptionsClass.Empty > 0)
                            {
                                licenseType = "";
                                price = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.Empty.Value;
                            }
                            else  
                            {
                                licenseType = "";
                                price = AppSettings.CurrencyIconStatic + "0";
                            } 
                        }
                        else
                        {
                            licenseType = "";
                            price = AppSettings.CurrencyIconStatic + "0";
                        }

                        holder.TxtLicenseType.Text = string.IsNullOrEmpty(licenseType) ? price : licenseType + " : " + price;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override int ItemCount => StoreList?.Count ?? 0;

        public StoreDataObject GetItem(int position)
        {
            return StoreList[position];
        }

        void MoreOnClick(StoreAdapterViewHolderClickEventArgs args) => MoreItemClick?.Invoke(this, args);
        void OnClick(StoreAdapterViewHolderClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(StoreAdapterViewHolderClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = StoreList[p0];
                if (item == null)
                    return d;

                if (!string.IsNullOrEmpty(item.Thumb)) d.Add(item.Thumb);

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
    }

    public class StoreAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic
        public View MainView { get; set; }

        public ImageView Image { get; private set; }
        public TextView IconMore { get; private set; }
        public TextView TxtTitle { get; private set; }
        public TextView TxtCategory { get; private set; }
        public TextView TxtLicenseType { get; private set; }

        #endregion

        public StoreAdapterViewHolder(View itemView, Action<StoreAdapterViewHolderClickEventArgs> moreClickListener, Action<StoreAdapterViewHolderClickEventArgs> clickListener, Action<StoreAdapterViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = itemView.FindViewById<ImageView>(Resource.Id.image);

                IconMore = itemView.FindViewById<TextView>(Resource.Id.iconMore);
                TxtTitle = itemView.FindViewById<TextView>(Resource.Id.title);
                TxtCategory = itemView.FindViewById<TextView>(Resource.Id.category);
                TxtLicenseType = itemView.FindViewById<TextView>(Resource.Id.LicenseType);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconMore, IonIconsFonts.More);
                 
                //Create an Event
                IconMore.Click += (sender, e) => moreClickListener(new StoreAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.Click += (sender, e) => clickListener(new StoreAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new StoreAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class StoreAdapterViewHolderClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}