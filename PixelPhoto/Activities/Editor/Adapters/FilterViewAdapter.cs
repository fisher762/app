using System;
using System.Collections.ObjectModel;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using PixelPhoto.Helpers.Model.Editor;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.NiceArt;
using PixelPhoto.NiceArt.Models;

namespace PixelPhoto.Activities.Editor.Adapters
{
    public class FilterViewAdapter : RecyclerView.Adapter
    {
        public readonly ObservableCollection<MyFilter> MPairList = new ObservableCollection<MyFilter>();
        private Bitmap ImagePath;

        private IFilterListener MFilterListener;
        private ImageFilterView ViewSelected;
        private readonly EditImageActivity ActivityContext;

        public FilterViewAdapter(EditImageActivity context, IFilterListener filterListener, Bitmap imagePath)
        {
            try
            {
                ActivityContext = context;

                MFilterListener = filterListener;
                ImagePath = imagePath;

                ViewSelected = new ImageFilterView(ActivityContext)
                {
                    Id = NiceArtEditorView.GlFilterId,
                    Visibility = ViewStates.Visible
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => MPairList?.Count ?? 0;

        public event EventHandler<FilterViewAdapterClickEventArgs> ItemClick;
        public event EventHandler<FilterViewAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> row_filter_view
                var itemView = LayoutInflater.From(parent.Context)
                    .Inflate(Resource.Layout.RowFilterView, parent, false);
                var vh = new FilterViewAdapterViewHolder(itemView, ActivityContext, OnClick, OnLongClick);
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
                if (viewHolder is FilterViewAdapterViewHolder holder)
                {
                    var item = MPairList[position];
                    if (item != null)
                    {
                        if (item.NameImage != null)
                        {
                            holder.NewphotoView.GetSource().SetImageBitmap(item.NameImage);
                            holder.NewNiceArtEditor.SetFilterEffect(item.PhotoFilter);
                        }

                        holder.MTxtFilterName.Text = item.PhotoFilter.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Clear()
        {
            try
            {
                MPairList.Clear();
                NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetupFilters(Bitmap imagePath)
        {
            try
            {
                ImagePath = imagePath;

                if (MPairList.Count > 0)
                    Clear();

                MPairList.Add(new MyFilter
                {
                    Id = 1,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.None
                });
                MPairList.Add(new MyFilter
                {
                    Id = 2,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.AutoFix
                });
                MPairList.Add(new MyFilter
                {
                    Id = 3,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Brightness
                });
                MPairList.Add(new MyFilter
                {
                    Id = 4,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Contrast
                });
                MPairList.Add(new MyFilter
                {
                    Id = 5,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Documentary
                });
                MPairList.Add(new MyFilter
                {
                    Id = 6,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.DueTone
                });
                MPairList.Add(new MyFilter
                {
                    Id = 7,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.FillLight
                });
                MPairList.Add(new MyFilter
                {
                    Id = 8,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.FishEye
                });
                MPairList.Add(new MyFilter
                {
                    Id = 9,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Grain
                });
                MPairList.Add(new MyFilter
                {
                    Id = 10,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.GrayScale
                });
                MPairList.Add(new MyFilter
                {
                    Id = 11,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Lomoish
                });
                MPairList.Add(new MyFilter
                {
                    Id = 12,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Negative
                });
                MPairList.Add(new MyFilter
                {
                    Id = 13,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Posterize
                });
                MPairList.Add(new MyFilter
                {
                    Id = 14,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Saturate
                });
                MPairList.Add(new MyFilter
                {
                    Id = 15,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Sepia
                });
                MPairList.Add(new MyFilter
                {
                    Id = 16,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Sharpen
                });
                MPairList.Add(new MyFilter
                {
                    Id = 17,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Temperature
                });
                MPairList.Add(new MyFilter
                {
                    Id = 18,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Tint
                });
                MPairList.Add(new MyFilter
                {
                    Id = 19,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Vignette
                });
                MPairList.Add(new MyFilter
                {
                    Id = 20,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.CrossProcess
                });
                MPairList.Add(new MyFilter
                {
                    Id = 21,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.BlackWhite
                });
                MPairList.Add(new MyFilter
                {
                    Id = 22,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.FlipHorizontal
                });
                MPairList.Add(new MyFilter
                {
                    Id = 23,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.FlipVertical
                });
                MPairList.Add(new MyFilter
                {
                    Id = 24,
                    NameImage = ImagePath,
                    PhotoFilter = PhotoFilter.Rotate
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public MyFilter GetItem(int position)
        {
            return MPairList[position];
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

        public void OnClick(FilterViewAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        public void OnLongClick(FilterViewAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class FilterViewAdapterViewHolder : RecyclerView.ViewHolder /*, IOnNiceArtEditorListener*/
    {
        public FilterViewAdapterViewHolder(View itemView, EditImageActivity activityContext,
            Action<FilterViewAdapterClickEventArgs> clickListener,
            Action<FilterViewAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                NewphotoView = itemView.FindViewById<NiceArtEditorView>(Resource.Id.imgFilterView);
                MTxtFilterName = itemView.FindViewById<TextView>(Resource.Id.txtFilterName);

                itemView.Click += (sender, e) => clickListener(new FilterViewAdapterClickEventArgs
                    {View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new FilterViewAdapterClickEventArgs
                    {View = itemView, Position = AdapterPosition});

                var mEmojiTypeFace = Typeface.CreateFromAsset(activityContext.Assets, "emojione-android.ttf");

                NewNiceArtEditor = new NiceArtEditor.Builder(activityContext, NewphotoView, activityContext.ContentResolver)
                        .SetPinchTextScalable(true) // set false to disable pinch to zoom on text insertion.By default its true
                        .SetDefaultEmojiTypeface(mEmojiTypeFace) // set default font TypeFace to add emojis
                        .Build(); // build NiceArt Editor sdk

                //NewNiceArtEditor.SetOnNiceArtEditorListener(this); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnEditTextChangeListener(View rootView, string text, int colorCode)
        {
        }

        public void OnAddViewListener(ViewType viewType, int numberOfAddedViews)
        {
        }

        public void OnRemoveViewListener(int numberOfAddedViews)
        {
        }

        public void OnRemoveViewListener(ViewType viewType, int numberOfAddedViews)
        {
        }

        public void OnStartViewChangeListener(ViewType viewType)
        {
        }

        public void OnStopViewChangeListener(ViewType viewType)
        {
        }

        #region Variables Basic

        public View MainView { get; }

        public TextView MTxtFilterName { get; private set; }
        public NiceArtEditorView NewphotoView { get; private set; }
        public NiceArtEditor NewNiceArtEditor { get; private set; }

        #endregion
    }

    public class FilterViewAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}