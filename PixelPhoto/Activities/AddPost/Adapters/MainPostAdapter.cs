using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;

namespace PixelPhoto.Activities.AddPost.Adapters
{
    public class MainPostAdapter : RecyclerView.Adapter
    {
        public Activity ActivityContext;
        public ObservableCollection<Classes.PostType> PostTypeList = new ObservableCollection<Classes.PostType>();

        public MainPostAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                GetOptionItem(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetOptionItem()
        {
            try
            {
                if (AppSettings.ShowGalleryImage && AppTools.CheckAllowedUploadInServer("Image"))
                    PostTypeList.Add(new Classes.PostType
                    {
                        Id = 1,
                        TypeText = ActivityContext.GetText(Resource.String.Lbl_ImageGallery),
                        Image = Resource.Drawable.pix_image_icon,
                        ImageColor = "#00b200"
                    });

                if (AppSettings.ShowGalleryVideo && AppTools.CheckAllowedUploadInServer("Video"))
                    PostTypeList.Add(new Classes.PostType
                    {
                        Id = 2,
                        TypeText = ActivityContext.GetText(Resource.String.Lbl_VideoGallery),
                        Image = Resource.Drawable.pix_video_icon,
                        ImageColor = "#D81B60"
                    });
                if (AppSettings.ShowMention)
                    PostTypeList.Add(new Classes.PostType
                    {
                        Id = 3,
                        TypeText = ActivityContext.GetText(Resource.String.Lbl_MentionContact),
                        Image = Resource.Drawable.ic__Attach_tag,
                        ImageColor = ""
                    });
                if (AppSettings.ShowCamera && AppTools.CheckAllowedUploadInServer("Image"))
                    PostTypeList.Add(new Classes.PostType
                    {
                        Id = 4,
                        TypeText = ActivityContext.GetText(Resource.String.Lbl_Camera),
                        Image = Resource.Drawable.ic__Attach_video,
                        ImageColor = ""
                    });
                if (AppSettings.ShowGif && AppTools.CheckAllowedUploadInServer("Image"))
                    PostTypeList.Add(new Classes.PostType
                    {
                        Id = 5,
                        TypeText = ActivityContext.GetText(Resource.String.Lbl_Gif),
                        Image = Resource.Drawable.pix_gif_icon,
                        ImageColor = ""
                    });
                if (AppSettings.ShowEmbedVideo)
                    PostTypeList.Add(new Classes.PostType
                    {
                        Id = 6,
                        TypeText = ActivityContext.GetText(Resource.String.Lbl_EmbedVideo),
                        Image = Resource.Drawable.pix_broken_link,
                        ImageColor = ""
                    });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public override int ItemCount => PostTypeList?.Count ?? 0;

        public event EventHandler<MainPostAdapterClickEventArgs> ItemClick;
        public event EventHandler<MainPostAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_AddPost_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AddPost_View, parent, false);
                var vh = new MainPostAdapterViewHolder(itemView, OnClick, OnLongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is MainPostAdapterViewHolder holder)
                {
                    var item = PostTypeList[position];
                    if (item != null)
                    {
                        holder.PostTypeText.Text = item.TypeText;
                        holder.PostImageIcon.SetImageResource(item.Image);

                        if (!string.IsNullOrEmpty(item.ImageColor))
                            holder.PostImageIcon.SetColorFilter(Color.ParseColor(item.ImageColor));
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public Classes.PostType GetItem(int position)
        {
            return PostTypeList[position];
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

        private void OnClick(MainPostAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(MainPostAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class MainPostAdapterViewHolder : RecyclerView.ViewHolder
    {
        public MainPostAdapterViewHolder(View itemView, Action<MainPostAdapterClickEventArgs> clickListener, Action<MainPostAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values         
                PostTypeText = (TextView)itemView.FindViewById(Resource.Id.type_name);
                PostImageIcon = (ImageView)itemView.FindViewById(Resource.Id.Iconimage);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new MainPostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                Console.WriteLine(longClickListener);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }


        public TextView PostTypeText { get; }
        public ImageView PostImageIcon { get; }

        #endregion
    }

    public class MainPostAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}