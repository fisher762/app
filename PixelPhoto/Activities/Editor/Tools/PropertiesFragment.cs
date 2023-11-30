using System;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.BottomSheet;
using PixelPhoto.Activities.Editor.Adapters;
using PixelPhoto.Helpers.Model.Editor;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.NiceArt;
using Warkiz.Widgets;

namespace PixelPhoto.Activities.Editor.Tools
{
    public class PropertiesFragment : BottomSheetDialogFragment, IOnSeekChangeListener
    {
        private ColorPickerAdapter ColorPickerAdapter;
        private IProperties MProperties;
        private NiceArtEditor NiceArtEditor;

        public PropertiesFragment(NiceArtEditor mNiceArtEditor)
        {
            // Required empty public constructor
            NiceArtEditor = mNiceArtEditor;
        }
         
        public void OnSeeking(SeekParams seekParams)
        {
            try
            {
                switch (seekParams.SeekBar.Id)
                {
                    case Resource.Id.sbOpacity:
                        MProperties?.OnOpacityChanged(seekParams.Progress);
                        break;
                    case Resource.Id.sbSize:
                        MProperties?.OnBrushSizeChanged(seekParams.Progress);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            } 
        }

        public void OnStartTrackingTouch(IndicatorSeekBar seekBar)
        {
           
        }

        public void OnStopTrackingTouch(IndicatorSeekBar seekBar)
        {
            
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark_Base) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Base);

                // clone the inflater using the ContextThemeWrapper
                var localInflater = inflater.CloneInContext(contextThemeWrapper);

                var view = localInflater?.Inflate(Resource.Layout.PropertiesDialog, container, false); 
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }

            // Use this to return your custom view for this Fragment
            //return inflater.Inflate(Resource.Layout.properties_dialog, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);
                var rvColor = view.FindViewById<RecyclerView>(Resource.Id.rvColors);
                var sbOpacity = view.FindViewById<IndicatorSeekBar>(Resource.Id.sbOpacity);
                var sbBrushSize = view.FindViewById<IndicatorSeekBar>(Resource.Id.sbSize);

                sbOpacity.OnSeekChangeListener = (this);
                sbBrushSize.OnSeekChangeListener = (this);

                var layoutManager = new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false);
                rvColor.SetLayoutManager(layoutManager);
                rvColor.HasFixedSize = true;
                ColorPickerAdapter = new ColorPickerAdapter(Activity, ColorType.ColorNormal);
                ColorPickerAdapter.ItemClick += ColorPickerAdapterOnItemClick;
                rvColor.SetAdapter(ColorPickerAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ColorPickerAdapterOnItemClick(object sender, ColorPickerAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position > -1)
                {
                    var item = ColorPickerAdapter.GetItem(position);
                    if (item != null)
                    {
                        if (MProperties == null) return;
                        Dismiss();
                        MProperties.OnColorChanged(item.ColorFirst);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public void SetPropertiesChangeListener(IProperties properties)
        {
            try
            {
                MProperties = properties;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnColorPickerClickListener(string colorCode)
        {
            try
            {
                if (MProperties != null)
                {
                    Dismiss();
                    MProperties.OnColorChanged(colorCode);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
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

        
    }
}