using System;
using System.Linq;
using System.Net;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.BottomSheet;
using PixelPhoto.Activities.Editor.Adapters;
using PixelPhoto.Helpers.Model.Editor;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.NiceArt;

namespace PixelPhoto.Activities.Editor.Tools
{
    public class StickerFragment : BottomSheetDialogFragment
    {
       // private readonly BottomSheetBehavior.BottomSheetCallback MBottomSheetBehaviorCallback = new MyBottomSheetCallBack();
        private readonly NiceArtEditor NiceArtEditor;

        private StickerAdapter StickerAdapter;

        public StickerFragment(NiceArtEditor mNiceArtEditor)
        {
            // Required empty public constructor
            NiceArtEditor = mNiceArtEditor;
        }

        public override void SetupDialog(Dialog dialog, int style)
        {
            try
            {
                base.SetupDialog(dialog, style);
                var contentView = View.Inflate(Context, Resource.Layout.StickerEmojiDialog, null);
                dialog.SetContentView(contentView);
                //var @params = (CoordinatorLayout.LayoutParams) ((View) contentView.Parent).LayoutParameters;
                //var behavior = @params.Behavior;

                //if (behavior != null && behavior.GetType() == typeof(BottomSheetBehavior))
                //    ((BottomSheetBehavior) behavior).SetBottomSheetCallback(MBottomSheetBehaviorCallback);

                //((View)contentView.Parent).SetBackgroundColor(Resources?.GetColor(Color.Transparent));

                var rvEmoji = contentView.FindViewById<RecyclerView>(Resource.Id.rvEmoji);

                var gridLayoutManager = new GridLayoutManager(Context, 3);
                rvEmoji.SetLayoutManager(gridLayoutManager);
                StickerAdapter = new StickerAdapter(Activity);
                StickerAdapter.ItemClick += StickerAdapterOnItemClick;
                rvEmoji.SetAdapter(StickerAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnStart()
        {
            try
            {
                base.OnStart();
                var dialog = Dialog;
                //Make dialog full screen with transparent background
                if (dialog != null)
                {
                    var width = ViewGroup.LayoutParams.MatchParent;
                    var height = ViewGroup.LayoutParams.MatchParent;
                    dialog.Window.SetLayout(width, height);
                    dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StickerAdapterOnItemClick(object sender, StickerAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = StickerAdapter.GetItem(position);
                    if (item != null)
                    {
                        var image = Stickers.StickerList.FirstOrDefault(a => a.Key == item).Value;
                        if (image != null)
                        {
                            NiceArtEditor?.AddImage(image);
                            Dismiss();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public Bitmap GetImageBitmapFromUrl(string url)
        {
            if (Methods.CheckConnectivity())
                using (var webClient = new WebClient())
                {
                    var imageBytes = webClient.DownloadData(url);
                    if (imageBytes != null && imageBytes.Length > 0)
                        return BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }

            return null!;
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

       

        //public class MyBottomSheetCallBack : BottomSheetBehavior.BottomSheetCallback
        //{
        //    public override void OnSlide(View bottomSheet, float slideOffset)
        //    {
        //        try
        //        {
        //            //Sliding
        //            if (slideOffset == StateHidden) Dispose();
        //        }
        //        catch (Exception e)
        //        {
        //            Methods.DisplayReportResultTrack(e);
        //        }
        //    }

        //    public override void OnStateChanged(View bottomSheet, int newState)
        //    {
        //        //State changed
        //    }
        //}
    }
}