using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Store;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.Store.Adapters
{ 
    public class HStoreAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<HStoreAdapterViewHolderClickEventArgs> ItemClick;
        public event EventHandler<HStoreAdapterViewHolderClickEventArgs> ItemLongClick;

        public ObservableCollection<StoreDataObject> StoreList = new ObservableCollection<StoreDataObject>();
        private readonly RequestBuilder FullGlideRequestBuilder;
        private readonly Activity ActivityContext;

        public HStoreAdapter(Activity context)
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
                //Setup your layout here >> Style_HStoreItem
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HStoreItem, parent, false);
                var vh = new HStoreAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is HStoreAdapterViewHolder holder)
                {
                    var item = StoreList[position]; 
                    if (item != null)
                    { 
                        FullGlideRequestBuilder.Load(item.Thumb).Into(holder.Image);
                         
                        holder.TxtCategory.Text = item.CategoryName;
                        holder.TxtResolution.Text = ""; //Resolution
                        holder.TxtViews.Text = item.Views + " " + ActivityContext.GetText(Resource.String.Lbl_Views); 
                        //holder.TxtPrice.Text = AppSettings.CurrencyIconStatic + item.Sells; 
                        holder.TxtExtenstion.Text = item.FullFile.Split('.').Last(); 
                        holder.TxtDownloads.Text = item.Downloads.ToString();
                         
                        if (item.LicenseOptions?.LicenseOptionsClass != null)
                        {
                            if (item.LicenseOptions?.LicenseOptionsClass.RightsManagedLicense > 0)
                            {
                                //LicenseType = GetText(Resource.String.Lbl_rights_managed_license);
                                holder.TxtPrice.Text = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.RightsManagedLicense.Value;
                            }
                            else if (item.LicenseOptions?.LicenseOptionsClass.EditorialUseLicense > 0)
                            {
                                //LicenseType = GetText(Resource.String.Lbl_editorial_use_license);
                                holder.TxtPrice.Text = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.EditorialUseLicense.Value;
                            }
                            else if (item.LicenseOptions?.LicenseOptionsClass.RoyaltyFreeLicense > 0)
                            {
                                //LicenseType = GetText(Resource.String.Lbl_royalty_free_license);
                                holder.TxtPrice.Text = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.RoyaltyFreeLicense.Value;
                            }
                            else if (item.LicenseOptions?.LicenseOptionsClass.RoyaltyFreeExtendedLicense > 0)
                            {
                                //LicenseType = GetText(Resource.String.Lbl_royalty_free_extended_license);
                                holder.TxtPrice.Text = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.RoyaltyFreeExtendedLicense.Value;
                            }
                            else if (item.LicenseOptions?.LicenseOptionsClass.CreativeCommonsLicense > 0)
                            {
                                //LicenseType = GetText(Resource.String.Lbl_creative_commons_license);
                                holder.TxtPrice.Text = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.CreativeCommonsLicense.Value;
                            }
                            else if (item.LicenseOptions?.LicenseOptionsClass.PublicDomain > 0)
                            {
                                //LicenseType = GetText(Resource.String.Lbl_public_domain);
                                holder.TxtPrice.Text = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.PublicDomain.Value;
                            }
                            else if (item.LicenseOptions?.LicenseOptionsClass.Empty > 0)
                            {
                                //licenseType = "";
                                holder.TxtPrice.Text = AppSettings.CurrencyIconStatic + item.LicenseOptions?.LicenseOptionsClass.Empty.Value;
                            }
                            else
                            {
                                //licenseType = "";
                                holder.TxtPrice.Text = AppSettings.CurrencyIconStatic + "0";
                            }
                        }
                        else
                        {
                            //licenseType = "";
                            holder.TxtPrice.Text = AppSettings.CurrencyIconStatic + "0";
                        }
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
         
        void OnClick(HStoreAdapterViewHolderClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(HStoreAdapterViewHolderClickEventArgs args) => ItemLongClick?.Invoke(this, args);
         
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

    public class HStoreAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic
        public View MainView { get; set; }
       
        public ImageView Image { get; private set; }
        public TextView TxtCategory { get; private set; }
        public TextView TxtViews { get; private set; }
        public TextView TxtResolution { get; private set; }
        public TextView TxtPrice { get; private set; }
        public TextView TxtExtenstion { get; private set; }
        public TextView TxtDownloads { get; private set; }

        #endregion

        public HStoreAdapterViewHolder(View itemView, Action<HStoreAdapterViewHolderClickEventArgs> clickListener,Action<HStoreAdapterViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = itemView.FindViewById<ImageView>(Resource.Id.Image);

                TxtPrice = itemView.FindViewById<TextView>(Resource.Id.txprice);
                TxtExtenstion = itemView.FindViewById<TextView>(Resource.Id.txExtenstion);
                TxtDownloads = itemView.FindViewById<TextView>(Resource.Id.txDownloads);
                TxtCategory = itemView.FindViewById<TextView>(Resource.Id.txcategory);
                TxtViews = itemView.FindViewById<TextView>(Resource.Id.txViews);
                TxtResolution = itemView.FindViewById<TextView>(Resource.Id.txResolution);

                TxtResolution.Visibility = ViewStates.Gone;

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new HStoreAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new HStoreAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class HStoreAdapterViewHolderClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}