using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using Java.IO;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Global;
using Console = System.Console;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.AddPost.Adapters
{
    public class AttachmentsAdapter : RecyclerView.Adapter
    {
        private readonly Activity ActivityContext;
        public readonly ObservableCollection<Attachments> AttachmentsList = new ObservableCollection<Attachments>();

        public AttachmentsAdapter(Activity context)
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

        public override int ItemCount => AttachmentsList?.Count ?? 0;

        public event EventHandler<AttachmentsAdapterClickEventArgs> DeleteItemClick;
        public event EventHandler<AttachmentsAdapterClickEventArgs> ItemEditClick;
        public event EventHandler<AttachmentsAdapterClickEventArgs> ItemClick;
        public event EventHandler<AttachmentsAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Attachment_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Attachment_View, parent, false);
                var vh = new AttachmentsAdapterViewHolder(itemView, DeleteClick, OnClick, OnLongClick, OnEditClick);
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
                if (viewHolder is AttachmentsAdapterViewHolder holder)
                {
                    var item = AttachmentsList[position];
                    if (item != null)
                    {
                        if (item.FileSimple.Contains("http") || item.FileSimple.Contains("EmbedVideo_File"))
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.FileSimple, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        }
                        else
                        {
                            if (item.TypeAttachment == "video" && !string.IsNullOrEmpty(item.Thumb.FileUrl) && !new File(item.Thumb.FileUrl).Exists())
                            {
                                var file2 = new File(item.FileUrl);
                                //var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);

                                Glide.With(ActivityContext)
                                    .AsBitmap()
                                    .Apply(new RequestOptions().Placeholder(Resource.Drawable.blackdefault).Error(Resource.Drawable.blackdefault))
                                    .Load(file2) // or URI/path
                                    .Into(new MySimpleTarget(this, holder, position));  //image view to set thumbnail to
                            }
                            else
                            {
                                Glide.With(ActivityContext).Load(new File(item.FileUrl)).Apply(new RequestOptions().Placeholder(Resource.Drawable.Blue_Color).Error(Resource.Drawable.Blue_Color)).Into(holder.Image);
                            }
                        }

                        if (item.TypeAttachment.Contains("images"))
                        {
                            holder.AttachType.Visibility = ViewStates.Visible;
                        }
                        else if (item.TypeAttachment == "video")
                        {
                            holder.AttachType.SetImageResource(Resource.Drawable.ic_file_video);
                            holder.AttachType.Visibility = ViewStates.Visible;
                            holder.AttachType.Enabled = false;
                        }
                        else  
                        {
                            holder.AttachType.Visibility = ViewStates.Gone;
                        } 
                    } 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private class MySimpleTarget : CustomTarget
        {
            private readonly AttachmentsAdapter MAdapter;
            private readonly AttachmentsAdapterViewHolder ViewHolder;
            private readonly int Position;
            public MySimpleTarget(AttachmentsAdapter adapter, AttachmentsAdapterViewHolder viewHolder, int position)
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

            public override void OnResourceReady(Object resource, ITransition transition)
            {
                try
                {
                    if (MAdapter.AttachmentsList?.Count > 0)
                    {
                        var item = MAdapter.AttachmentsList[Position];
                        if (item != null)
                        {
                            var fileName = item.FileUrl.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();

                            var pathImage = Methods.Path.FolderDcimImage + "/" + fileNameWithoutExtension + ".png";

                            var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");
                            if (videoImage == "File Dont Exists")
                            {
                                if (resource is Bitmap bitmap)
                                {
                                    Methods.MultiMedia.Export_Bitmap_As_Image(bitmap, fileNameWithoutExtension, Methods.Path.FolderDcimImage);

                                    var file2 = new File(pathImage);
                                    var photoUri = FileProvider.GetUriForFile(MAdapter.ActivityContext, MAdapter.ActivityContext.PackageName + ".fileprovider", file2);

                                    Glide.With(MAdapter.ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(ViewHolder.Image);

                                    item.Thumb = new Attachments.VideoThumb
                                    {
                                        FileUrl = photoUri.ToString()
                                    };
                                }
                            }
                            else
                            {
                                var file2 = new File(pathImage);
                                var photoUri = FileProvider.GetUriForFile(MAdapter.ActivityContext, MAdapter.ActivityContext.PackageName + ".fileprovider", file2);

                                Glide.With(MAdapter.ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(ViewHolder.Image);

                                item.Thumb = new Attachments.VideoThumb
                                {
                                    FileUrl = photoUri.ToString()
                                };
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnLoadCleared(Drawable p0) { }
        }


        // Function 
        public void Add(Attachments item)
        {
            try
            {
                AttachmentsList.Add(item);
                NotifyItemInserted(AttachmentsList.IndexOf(item));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Remove(Attachments item)
        {
            try
            {
                var index = AttachmentsList.IndexOf(AttachmentsList.FirstOrDefault(a => a.Id == item.Id));
                if (index != -1)
                {
                    AttachmentsList.Remove(item);
                    NotifyItemRemoved(index);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void RemoveAll()
        {
            try
            {
                AttachmentsList.Clear();
                NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public Attachments GetItem(int position)
        {
            return AttachmentsList[position];
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

        private void DeleteClick(AttachmentsAdapterClickEventArgs args)
        {
            DeleteItemClick?.Invoke(this, args);
        }

        private void OnEditClick(AttachmentsAdapterClickEventArgs args)
        {
            ItemEditClick?.Invoke(this, args);
        }

        private void OnClick(AttachmentsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(AttachmentsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class AttachmentsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView AttachType { get; private set; }
        public ImageView Image { get; private set; }
        public CircleButton ImageDelete { get; private set; }

        #endregion

        public AttachmentsAdapterViewHolder(View itemView, Action<AttachmentsAdapterClickEventArgs> clickDeleteListener, Action<AttachmentsAdapterClickEventArgs> clickListener,Action<AttachmentsAdapterClickEventArgs> longClickListener, Action<AttachmentsAdapterClickEventArgs> editClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values         
                AttachType = (ImageView)itemView.FindViewById(Resource.Id.AttachType);
                Image = (ImageView)itemView.FindViewById(Resource.Id.Image);

                ImageDelete = itemView.FindViewById<CircleButton>(Resource.Id.ImageCircle);

                //Create an Event
                ImageDelete.Click += (sender, e) => clickDeleteListener(new AttachmentsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.Click += (sender, e) => clickListener(new AttachmentsAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
                AttachType.Click += (sender, e) => editClickListener(new AttachmentsAdapterClickEventArgs { Position = AdapterPosition, View = itemView });

                Console.WriteLine(longClickListener);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }

    public class AttachmentsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}