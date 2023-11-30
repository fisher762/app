using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Com.Luseen.Autolinklibrary;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Messages;
using Path = System.IO.Path;

namespace PixelPhoto.Activities.Chat.Adapters
{
    public class UserMessagesAdapter : RecyclerView.Adapter
    {
        private readonly MessagesBoxActivity ActivityContext;
        public ObservableCollection<MessageDataObject> MessageList = new ObservableCollection<MessageDataObject>();
        public event EventHandler<UserMessagesAdapterClickEventArgs> ItemClick;
        public event EventHandler<UserMessagesAdapterClickEventArgs> ItemLongClick;

        private readonly SparseBooleanArray SelectedItems;
        private IOnClickListenerSelectedMessages OnClickListener;
        private int CurrentSelectedIdx = -1;
        private readonly RequestOptions Options;

        public UserMessagesAdapter(MessagesBoxActivity context)
        {
            try
            {
                ActivityContext = context;
                SelectedItems = new SparseBooleanArray();
                Options = new RequestOptions().Apply(RequestOptions.CircleCropTransform()
                    .CenterCrop()
                    .SetPriority(Priority.High).Override(200)
                    .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                    .Error(Resource.Drawable.ImagePlacholder)
                    .Placeholder(Resource.Drawable.ImagePlacholder));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> 
                var itemView = MessageList[viewType];
                if (itemView != null)
                { 
                    if (itemView.FromId == UserDetails.UserId && itemView.MessageType == "Text" && itemView.Position.ToLower() == "right")
                    {
                        var row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_view, parent, false);
                        var textViewHolder = new TextViewHolder(row, OnClick, OnLongClick, ActivityContext);
                        return textViewHolder;
                    }

                    if (itemView.ToId == UserDetails.UserId && itemView.MessageType == "Text" && itemView.Position.ToLower() == "left")
                    {
                        var row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_view, parent, false);
                        var textViewHolder = new TextViewHolder(row, OnClick, OnLongClick, ActivityContext);
                        return textViewHolder;
                    }

                    if (itemView.FromId == UserDetails.UserId && itemView.MessageType == "Media" && itemView.Position.ToLower() == "right")
                    {
                        var row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_image, parent, false);
                        var imageViewHolder = new ImageViewHolder(row);
                        return imageViewHolder;
                    }

                    if (itemView.ToId == UserDetails.UserId && itemView.MessageType == "Media" && itemView.Position.ToLower() == "left")
                    {
                        var row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_image, parent, false);
                        var imageViewHolder = new ImageViewHolder(row);
                        return imageViewHolder;
                    }
                    else 
                    {
                        var row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_view, parent, false);
                        var textViewHolder = new TextViewHolder(row, OnClick, OnLongClick, ActivityContext);
                        return textViewHolder;
                    }
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            try
            {
                var type = GetItemViewType(position);
                var item = MessageList[type];
                if (item != null)
                {
                    switch (item.MessageType)
                    {
                        case "Text":
                        { 
                            if (vh is TextViewHolder holder) 
                                LoadTextOfChatItem(holder, position, item);

                            break;
                        }
                        case "Media":
                        {
                            if (vh is ImageViewHolder holder)
                                LoadImageOfChatItem(holder, position, item);

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

        private void LoadImageOfChatItem(ImageViewHolder holder, int position, MessageDataObject message)
        {
            try
            {
                if (holder == null )
                {
                    return;
                }
                Console.WriteLine(position);
                var imageUrl = message.MediaFile.Contains("media/upload/photos/") && !message.MediaFile.Contains(Client.WebsiteUrl) ? Client.WebsiteUrl + "/" + message.MediaFile : message.MediaFile;

                string fileSavedPath;

                var time = Methods.Time.UnixTimeStampToDateTime(Convert.ToInt32(message.Time));
                holder.Time.Text = time.ToShortTimeString();

                if (imageUrl.Contains("http://") || imageUrl.Contains("https://"))
                {
                    var fileName = imageUrl.Split('/').Last();
                    var imageFile = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileName);
                    if (imageFile == "File Dont Exists")
                    {
                        holder.LoadingProgressView.Indeterminate = false;
                        holder.LoadingProgressView.Visibility = ViewStates.Visible;

                        var filePath = Path.Combine(Methods.Path.FolderDcimImage);
                        var mediaFile = filePath + "/" + fileName;
                        fileSavedPath = mediaFile;

                        var webClient = new WebClient();

                        webClient.DownloadDataAsync(new Uri(imageUrl));
                        //webClient.DownloadProgressChanged += (sender, args) =>
                        //{
                        //    var progress = args.ProgressPercentage;
                        //    Console.WriteLine(progress);
                        //};

                        webClient.DownloadDataCompleted += (s, e) =>
                        {
                            try
                            {
                                if (!Directory.Exists(filePath))
                                    Directory.CreateDirectory(filePath);

                                File.WriteAllBytes(mediaFile, e.Result);

                                //var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                                //mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(mediaFile)));
                                //ActivityContext.SendBroadcast(mediaScanIntent);

                                // Tell the media scanner about the new file so that it is
                                // immediately available to the user.
                                MediaScannerConnection.ScanFile(Application.Context, new[] { mediaFile }, null, null);

                                var file = Android.Net.Uri.FromFile(new Java.IO.File(mediaFile));
                                Glide.With(ActivityContext).Load(file.Path).Apply(Options).Into(holder.ImageView);

                                holder.LoadingProgressView.Indeterminate = false;
                                holder.LoadingProgressView.Visibility = ViewStates.Gone;
                            }
                            catch (Exception ex)
                            {
                                Methods.DisplayReportResultTrack(ex);
                            }
                        };
                    }
                    else
                    {
                        fileSavedPath = imageFile;

                        var file = Android.Net.Uri.FromFile(new Java.IO.File(imageFile));
                        Glide.With(ActivityContext).Load(file.Path).Apply(Options).Into(holder.ImageView);

                        holder.LoadingProgressView.Indeterminate = false;
                        holder.LoadingProgressView.Visibility = ViewStates.Gone;
                    }
                }
                else
                {
                    fileSavedPath = imageUrl;

                    var file = Android.Net.Uri.FromFile(new Java.IO.File(imageUrl));
                    Glide.With(ActivityContext).Load(file.Path).Apply(Options).Into(holder.ImageView);

                    //GlideImageLoader.LoadImage(ActivityContext, imageUrl, holder.ImageView, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    holder.LoadingProgressView.Indeterminate = false;
                    holder.LoadingProgressView.Visibility = ViewStates.Gone;
                }

                if (!holder.ImageView.HasOnClickListeners)
                {
                    holder.ImageView.Click += (sender, args) =>
                    {
                        try
                        {
                            var imageFile = Methods.MultiMedia.CheckFileIfExits(fileSavedPath);

                            if (imageFile != "File Dont Exists")
                            {
                                var file2 = new Java.IO.File(fileSavedPath);
                                var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);

                                var intent = new Intent();
                                intent.SetAction(Intent.ActionView);
                                intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                                intent.SetDataAndType(photoUri, "image/*");
                                ActivityContext.StartActivity(intent);
                            }
                            else
                            {
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    };
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOnClickListener(IOnClickListenerSelectedMessages onClickListener)
        {
            OnClickListener = onClickListener;
        }

        public void LoadTextOfChatItem(TextViewHolder holder, int position, MessageDataObject item)
        {
            try
            {
                if (holder == null)
                {
                    return;
                }

                if (holder.Time.Text != item.Time)
                {
                    var time = Methods.Time.UnixTimeStampToDateTime(Convert.ToInt32(item.Time));
                    holder.Time.Text = time.ToShortTimeString();
                    holder.TextSanitizerAutoLink.Load(item.Text , item.Position);

                    holder.LytParent.Activated = SelectedItems.Get(position, false);

                    if (!holder.LytParent.HasOnClickListeners)
                    {
                        holder.LytParent.Click += delegate
                        {
                            try
                            {
                                if (OnClickListener == null) return;

                                OnClickListener.OnItemClick(holder.LytParent, item, position);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        };

                        holder.LytParent.LongClick += delegate
                        {
                            try
                            {
                                if (OnClickListener == null) return;

                                OnClickListener.OnItemLongClick(holder.LytParent, item, position);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        };
                    }
                      
                    ToggleCheckedBackground(holder, position); 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public override int ItemCount => MessageList?.Count ?? 0;
          
        public MessageDataObject GetItem(int position)
        {
            return MessageList[position];
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
         
        private void OnClick(UserMessagesAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(UserMessagesAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        #region Toolbar & Selected

        private void ToggleCheckedBackground(TextViewHolder holder, int position)
        {
            try
            {
                if (SelectedItems.Get(position, false))
                {
                    holder.MainView.SetBackgroundColor(Color.LightBlue);
                    if (CurrentSelectedIdx == position) ResetCurrentItems();
                }
                else
                {
                    holder.MainView.SetBackgroundColor(Color.Transparent);
                    if (CurrentSelectedIdx == position) ResetCurrentItems();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ResetCurrentItems()
        {
            try
            {
                CurrentSelectedIdx = -1;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public int GetSelectedItemCount()
        {
            return SelectedItems.Size();
        }

        public List<int> GetSelectedItems()
        {
            var items = new List<int>(SelectedItems.Size());
            for (var i = 0; i < SelectedItems.Size(); i++)
            {
                items.Add(SelectedItems.KeyAt(i));
            }
            return items;
        }

        public void RemoveData(int position, MessageDataObject users)
        {
            try
            {
                var index = MessageList.IndexOf(MessageList.FirstOrDefault(a => a.Id == users.Id));
                if (index != -1)
                {
                    MessageList.Remove(users);
                    NotifyItemRemoved(index);
                }
                 
                ResetCurrentItems();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ClearSelections()
        {
            try
            {
                SelectedItems.Clear();
                NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ToggleSelection(int pos)
        {
            try
            {
                CurrentSelectedIdx = pos;
                if (SelectedItems.Get(pos, false))
                {
                    SelectedItems.Delete(pos);
                }
                else
                {
                    SelectedItems.Put(pos, true);
                }
                NotifyItemChanged(pos);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
    }

    public class UserMessagesAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
         
        #endregion

        public UserMessagesAdapterViewHolder(View itemView, Action<UserMessagesAdapterClickEventArgs> clickListener,
            Action<UserMessagesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                 
                //Create an Event
                itemView.Click += (sender, e) => clickListener(new UserMessagesAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new UserMessagesAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        } 
    }

    public class TextViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public LinearLayout LytParent { get; private set; }
        public TextView Time { get; private set; }
        public View MainView { get; private set; }
        public AutoLinkTextView AutoLinkTextView { get; set; }
        public TextSanitizer TextSanitizerAutoLink { get; private set; }

        #endregion

        public TextViewHolder(View itemView, Action<UserMessagesAdapterClickEventArgs> clickListener, Action<UserMessagesAdapterClickEventArgs> longClickListener, Activity activity) : base(itemView)
        {
            try
            {
                MainView = itemView;

                LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                AutoLinkTextView = itemView.FindViewById<AutoLinkTextView>(Resource.Id.active);
                Time = itemView.FindViewById<TextView>(Resource.Id.time);

                AutoLinkTextView.SetTextIsSelectable(true);

                TextSanitizerAutoLink ??= new TextSanitizer(AutoLinkTextView, activity);

                itemView.Click += (sender, e) => clickListener(new UserMessagesAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new UserMessagesAdapterClickEventArgs { View = itemView, Position = AdapterPosition });

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class ImageViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public LinearLayout LytParent { get; set; }
        public ImageView ImageView { get; set; }
        public ProgressBar LoadingProgressView { get; set; }
        public TextView Time { get; set; }

        #endregion

        public ImageViewHolder(View itemView) : base(itemView)
        {
            try
            {
                MainView = itemView;
                LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                ImageView = itemView.FindViewById<ImageView>(Resource.Id.imgDisplay);
                LoadingProgressView = itemView.FindViewById<ProgressBar>(Resource.Id.loadingProgressview);
                Time = itemView.FindViewById<TextView>(Resource.Id.time);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class UserMessagesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}