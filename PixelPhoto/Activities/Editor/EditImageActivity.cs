using System;
using System.IO;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Media.Effect;
using Android.OS;
using AndroidX.AppCompat.App;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using TheArtOfDev.Edmodo.Cropper;
using Java.Lang;
using PixelPhoto.Activities.Editor.Adapters;
using PixelPhoto.Activities.Editor.Tools;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Model.Editor;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.NiceArt;
using PixelPhoto.NiceArt.Models;
using PixelPhoto.NiceArt.Utils;
using Warkiz.Widgets;
using AlertDialog = Android.App.AlertDialog;
using Exception = System.Exception;
using File = Java.IO.File;
using FileNotFoundException = Java.IO.FileNotFoundException;
using Uri = Android.Net.Uri;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.Editor
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditImageActivity : AppCompatActivity, INiceArt.IOnNiceArtEditorListener, IOnItemSelected, IProperties, IFilterListener, ITextEditor, INiceArt.IOnSaveListener, IOnSeekChangeListener
    {
        #region Variables Basic

        private readonly string Tag = typeof(EditImageActivity).Name;
        public NiceArtEditor MNiceArtEditor;
        private NiceArtEditorView MNiceArtEditorView;
        private PropertiesFragment MPropertiesFragment;
        private EmojisFragment MEmojisFragment;
        private StickerFragment MStickerFragment;
        private RecyclerView MRvTools, MRvFilters, MRvColor;
        private EditingToolsAdapter MEditingToolsAdapter;
        private FilterViewAdapter MFilterViewAdapter;
        private bool MIsFilterVisible;
        public View MView;
        private TextView ImgSave;
        private ImageView ImgClose, ImgDeleteAll, ImgUndo, ImgRedo;
        private ViewType ViewType;
        private ColorPickerAdapter ColorPickerAdapter;
        private ToolType MToolType;
        private IndicatorSeekBar SeekBarRotate;
        private string PathImage = "", IdImage = "0";

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    Window?.SetDecorFitsSystemWindows(false);

                    Window?.AddFlags(WindowManagerFlags.Fullscreen);
                    //context.Window?.RequestFeature(WindowFeatures.NoTitle);
                }
                else if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    var mContentView = Window?.DecorView;

                    if (mContentView != null)
                    {
#pragma warning disable 618
                        var uiOptions = (int)mContentView.SystemUiVisibility;
#pragma warning restore 618
                        var newUiOptions = uiOptions;

                        newUiOptions |= (int)SystemUiFlags.Fullscreen;
                        newUiOptions |= (int)SystemUiFlags.LayoutStable;
#pragma warning disable 618
                        mContentView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
#pragma warning restore 618
                    }

                    Window?.AddFlags(WindowManagerFlags.Fullscreen);

                    Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    Window?.SetStatusBarColor(Color.Transparent);
                }
                else if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                {
                    Window?.AddFlags(WindowManagerFlags.TranslucentStatus);
                }
                 
                // Create your application here
                SetContentView(Resource.Layout.EditImageLayout);

                PathImage = Intent?.GetStringExtra("PathImage") ?? "";
                IdImage = Intent?.GetStringExtra("IdImage") ?? "0";

                InitComponent(); 
                SetRecyclerViewAdapters();
                InitNiceArtEditor();
                  
                //Color
                MPropertiesFragment = new PropertiesFragment(MNiceArtEditor);
                MPropertiesFragment.SetPropertiesChangeListener(this);

                //Emojis
                MEmojisFragment = new EmojisFragment(MNiceArtEditor);

                //Sticker
                MStickerFragment = new StickerFragment(MNiceArtEditor);
                 
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();
                }
                else
                {
                    RequestPermissions(new string[]
                    {
                        Manifest.Permission.ReadExternalStorage,
                        Manifest.Permission.WriteExternalStorage,
                        Manifest.Permission.Camera,
                    }, 10);
                }

                if (!string.IsNullOrEmpty(PathImage))
                    SetImageNiceArt(Uri.Parse(PathImage));
                
                AdsGoogle.Ad_Interstitial(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        AppSettings.SetTabDarkTheme = false;
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        AppSettings.SetTabDarkTheme = true;
                        break;
                }

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Functions

        private void InitComponent()
        {
            try
            {
                MRvTools = FindViewById<RecyclerView>(Resource.Id.rvConstraintTools);
                MRvFilters = FindViewById<RecyclerView>(Resource.Id.rvFilterView);
                MRvColor = FindViewById<RecyclerView>(Resource.Id.add_text_color_picker_recycler_view);

                SeekBarRotate = FindViewById<IndicatorSeekBar>(Resource.Id.sbRotate);
                SeekBarRotate.OnSeekChangeListener = this;  

                ImgUndo = FindViewById<ImageView>(Resource.Id.imgUndo);
                ImgRedo = FindViewById<ImageView>(Resource.Id.imgRedo);
                ImgSave = FindViewById<TextView>(Resource.Id.imgSave);
                ImgClose = FindViewById<ImageView>(Resource.Id.imgClose);
                ImgDeleteAll = FindViewById<ImageView>(Resource.Id.imgdeleteall);

                MRvTools.Visibility = ViewStates.Visible;
                MRvFilters.Visibility = ViewStates.Gone;
                SeekBarRotate.Visibility = ViewStates.Gone;
                MRvColor.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InitNiceArtEditor()
        {
            try
            {
                MNiceArtEditorView = FindViewById<NiceArtEditorView>(Resource.Id.NiceArtView);
                var mEmojiTypeFace = Typeface.CreateFromAsset(Assets, "emojione-android.ttf");

                MNiceArtEditor = new NiceArtEditor.Builder(this, MNiceArtEditorView, ContentResolver)
                    .SetPinchTextScalable(true) // set false to disable pinch to zoom on text insertion.By default its true
                    .SetDefaultEmojiTypeface(mEmojiTypeFace) // set default font TypeFace to add emojis
                    .Build(); // build photo editor sdk

                MNiceArtEditor.SetOnNiceArtEditorListener(this);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetRecyclerViewAdapters()
        {
            try
            {
                //Tools
                var llmTools = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MRvTools.SetLayoutManager(llmTools);
                MEditingToolsAdapter = new EditingToolsAdapter(this);
                MRvTools.SetAdapter(MEditingToolsAdapter);
                 
                //Filters
                var llmFilters = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MRvFilters.SetLayoutManager(llmFilters);
                MFilterViewAdapter = new FilterViewAdapter(this, this, null);
                MRvFilters.SetAdapter(MFilterViewAdapter);

                //Color
                var layoutManager = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MRvColor.SetLayoutManager(layoutManager);
                MRvColor.HasFixedSize = true;
                ColorPickerAdapter = new ColorPickerAdapter(this, ColorType.ColorNormal);
                MRvColor.SetAdapter(ColorPickerAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    ImgUndo.Click += ImgUndoOnClick;
                    ImgRedo.Click += ImgRedoOnClick;
                    ImgSave.Click += ImgSaveOnClick;
                    ImgClose.Click += ImgCloseOnClick;
                    ImgDeleteAll.Click += ImgDeleteAllOnClick;
                    MEditingToolsAdapter.ItemClick += MEditingToolsAdapterOnItemClick;
                    ColorPickerAdapter.ItemClick += ColorPickerAdapterOnItemClick;
                }
                else
                {
                    ImgUndo.Click -= ImgUndoOnClick;
                    ImgRedo.Click -= ImgRedoOnClick;
                    ImgSave.Click -= ImgSaveOnClick;
                    ImgClose.Click -= ImgCloseOnClick;
                    ImgDeleteAll.Click -= ImgDeleteAllOnClick;
                    MEditingToolsAdapter.ItemClick -= MEditingToolsAdapterOnItemClick;
                    ColorPickerAdapter.ItemClick -= ColorPickerAdapterOnItemClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Delete All View <Reset>
        private void ImgDeleteAllOnClick(object sender, EventArgs e)
        {
            try
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(GetText(Resource.String.Lbl_Are_you_want_to_delete_all_changed));
                builder.SetPositiveButton(GetText(Resource.String.Lbl_Yes), delegate (object o, DialogClickEventArgs args) { MNiceArtEditor.SetFilterEffect(PhotoFilter.None); MNiceArtEditor.ClearAllViews(); MNiceArtEditorView.GetSource().ClearColorFilter(); });
                builder.SetNegativeButton(GetText(Resource.String.Lbl_No), delegate (object o, DialogClickEventArgs args) { });
                builder.Create().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Close Or Show Dialog 
        private void ImgCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                OnBackPressed();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save Image
        private void ImgSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                SaveImage();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Redo
        private void ImgRedoOnClick(object sender, EventArgs e)
        {
            try
            {
                MNiceArtEditor.Redo(ViewType);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Undo
        private void ImgUndoOnClick(object sender, EventArgs e)
        {
            try
            {
                MNiceArtEditor.Undo(ViewType);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Listener ViewType

        public void OnAddViewListener(ViewType viewType, int numberOfAddedViews)
        {
            try
            {
                ViewType = viewType;
                Console.WriteLine(Tag, "onAddViewListener() called with: viewType = [" + viewType + "], numberOfAddedViews = [" + numberOfAddedViews + "]");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnRemoveViewListener(int numberOfAddedViews)
        {
            try
            {
                Console.WriteLine(Tag, "onRemoveViewListener() called with: numberOfAddedViews = [" + numberOfAddedViews + "]");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnRemoveViewListener(ViewType viewType, int numberOfAddedViews)
        {
            try
            {
                ViewType = viewType;
                Console.WriteLine(Tag,
                    "onRemoveViewListener() called with: viewType = [" + viewType + "], numberOfAddedViews = [" +
                    numberOfAddedViews + "]");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnStartViewChangeListener(ViewType viewType)
        {
            try
            {
                ViewType = viewType;
                Console.WriteLine(Tag, "onStartViewChangeListener() called with: viewType = [" + viewType + "]");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnStopViewChangeListener(ViewType viewType)
        {
            try
            {
                ViewType = viewType;
                Console.WriteLine(Tag, "onStopViewChangeListener() called with: viewType = [" + viewType + "]");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }



        #endregion

        #region Brush

        public void OnColorChanged(string colorCode)
        {
            try
            {
                MNiceArtEditor.SetBrushColor(colorCode);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnOpacityChanged(int opacity)
        {
            try
            {
                MNiceArtEditor.SetOpacity(opacity);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnBrushSizeChanged(int brushSize)
        {
            try
            {
                MNiceArtEditor.SetBrushSize(brushSize);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Filter Effect

        private Bitmap Bitmap;
        //Set image with filter
        public void PrepareThumbnail(Bitmap bitmap)
        {
            Bitmap = bitmap;
            new Handler(Looper.MainLooper).Post(new Runnable(Run));
        }

        public void Run()
        {
            try
            {
                MFilterViewAdapter.SetupFilters(Bitmap);

                RunOnUiThread(() => { MFilterViewAdapter.NotifyDataSetChanged(); });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void MFilterViewAdapterOnItemClick(object o, FilterViewAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MFilterViewAdapter.GetItem(position);
                    if (item != null)
                    {
                        if (Filter != item.PhotoFilter)
                        {
                            Filter = item.PhotoFilter;
                            OnFilterSelected(item.PhotoFilter);
                        } 
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            } 
        }

        public void OnFilterSelected(PhotoFilter photoFilter)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                var (key, filter) = ConvertTypeFilterEffect(photoFilter);
                if (string.IsNullOrEmpty(key))
                {
                    SeekBarRotate.Visibility = ViewStates.Invisible;
                    SeekBarRotate.SetProgress(0);
                }
                else
                {
                    SeekBarRotate.Visibility = ViewStates.Visible;
                    SeekBarRotate.SetProgress(0);
                }

                MNiceArtEditor.SetFilterEffect(photoFilter);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowFilter(bool isVisible)
        {
            try
            {
                MIsFilterVisible = isVisible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private float SeekBarProgress;
        private PhotoFilter Filter;
        
        public void OnSeeking(SeekParams seekParams)
        {
            try
            {
                switch (seekParams.SeekBar.Id)
                {
                    case Resource.Id.sbRotate:
                        SeekBarProgress = seekParams.ProgressFloat;
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
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                var (key, filter) = ConvertTypeFilterEffect(Filter);
                if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(key))
                {
                    var count = SeekBarProgress / 100;

                    var customEffect = new CustomEffect.Builder(filter)
                        .SetParameter(key, count)
                        .Build();

                    MNiceArtEditor.SetFilterEffect(customEffect);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_select_effect_filter), ToastLength.Long)?.Show();
                    SeekBarRotate.SetProgress(0);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private (string, string) ConvertTypeFilterEffect(PhotoFilter filter)
        {
            try
            {
                var type = string.Empty;
                var key = string.Empty;

                switch (filter)
                {
                    case PhotoFilter.None:
                        break;
                    case PhotoFilter.AutoFix:
                        type = EffectFactory.EffectAutofix;
                        key = "scale";
                        break;
                    case PhotoFilter.BlackWhite:
                        type = EffectFactory.EffectBlackwhite;
                        key = "black";
                        break;
                    case PhotoFilter.Brightness:
                        type = EffectFactory.EffectBrightness;
                        key = "brightness";
                        break;
                    case PhotoFilter.Contrast:
                        type = EffectFactory.EffectContrast;
                        key = "contrast";
                        break;
                    case PhotoFilter.CrossProcess:
                        type = EffectFactory.EffectCrossprocess;
                        break;
                    case PhotoFilter.Documentary:
                        type = EffectFactory.EffectDocumentary;
                        break;
                    case PhotoFilter.DueTone:
                        type = EffectFactory.EffectDuotone;
                        break;
                    case PhotoFilter.FillLight:
                        type = EffectFactory.EffectFilllight;
                        key = "strength";
                        break;
                    case PhotoFilter.FishEye:
                        type = EffectFactory.EffectFisheye;
                        key = "scale";
                        break;
                    case PhotoFilter.FlipVertical:
                        type = EffectFactory.EffectGrain;
                        key = "vertical";
                        break;
                    case PhotoFilter.FlipHorizontal:
                        type = EffectFactory.EffectGrain;
                        key = "horizontal";
                        break;
                    case PhotoFilter.Grain:
                        type = EffectFactory.EffectGrain;
                        key = "strength";
                        break;
                    case PhotoFilter.Lomoish:
                        type = EffectFactory.EffectLomoish;
                        break;
                    case PhotoFilter.Negative:
                        type = EffectFactory.EffectNegative;
                        break;
                    case PhotoFilter.Posterize:
                        type = EffectFactory.EffectPosterize;
                        break;
                    case PhotoFilter.Rotate:
                        type = EffectFactory.EffectRotate;
                        key = "angle";
                        break;
                    case PhotoFilter.Saturate:
                        type = EffectFactory.EffectSaturate;
                        key = "scale";
                        break;
                    case PhotoFilter.Sepia:
                        type = EffectFactory.EffectSepia;
                        break;
                    case PhotoFilter.Sharpen:
                        type = EffectFactory.EffectSharpen;
                        break;
                    case PhotoFilter.Temperature:
                        type = EffectFactory.EffectTemperature;
                        key = "scale";
                        break;
                    case PhotoFilter.Tint:
                        type = EffectFactory.EffectTint;
                        break;
                    case PhotoFilter.Vignette:
                        type = EffectFactory.EffectVignette;
                        key = "scale";
                        break;
                }

                return (key, type);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return ("", "");
            }
        }

        #endregion

        #region Tools

        private void MEditingToolsAdapterOnItemClick(object sender, EditingToolsAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MEditingToolsAdapter.GetItem(position);
                    if (item != null)
                    {
                        MEditingToolsAdapter.Click_item(item);
                        OnToolSelected(item.MToolType);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnToolSelected(ToolType toolType)
        {
            try
            {
                MToolType = toolType;
                if (toolType == ToolType.Brush)
                {
                    MNiceArtEditor.SetBrushDrawingMode(true);
                    MPropertiesFragment.Show(SupportFragmentManager, MPropertiesFragment.Tag);
                }
                else if (toolType == ToolType.Text)
                {
                    var textEditorDialogFragment = new TextEditorFragment(this,null);
                    textEditorDialogFragment.Show(this, this, null, "", Resource.Color.white, ViewTextType.Add);
                    textEditorDialogFragment.SetOnTextEditorListener(this);
                }
                else if (toolType == ToolType.Eraser)
                {
                    MNiceArtEditor.BrushEraser();
                }
                else if (toolType == ToolType.Filter)
                {
                    MRvTools.Visibility = ViewStates.Gone;
                    MRvFilters.Visibility = ViewStates.Visible;
                    SeekBarRotate.Visibility = ViewStates.Invisible;
                    MRvColor.Visibility = ViewStates.Gone;
                    ShowFilter(true);
                }
                else if (toolType == ToolType.Emojis)
                {
                    MEmojisFragment.Show(SupportFragmentManager, MEmojisFragment.Tag);
                }
                else if (toolType == ToolType.Sticker)
                {
                    MStickerFragment.Show(SupportFragmentManager, MStickerFragment.Tag);
                }
                else if (toolType == ToolType.Image)
                {
                    ImgGalleryOnClick();
                }
                else if (toolType == ToolType.Color)
                {
                    MRvTools.Visibility = ViewStates.Gone;
                    MRvFilters.Visibility = ViewStates.Gone;
                    SeekBarRotate.Visibility = ViewStates.Gone;
                    MRvColor.Visibility = ViewStates.Visible;
                    ShowFilter(true);
                }
                else if (toolType == ToolType.FilterColor)
                {
                    MRvTools.Visibility = ViewStates.Gone;
                    MRvFilters.Visibility = ViewStates.Gone;
                    SeekBarRotate.Visibility = ViewStates.Gone;
                    MRvColor.Visibility = ViewStates.Visible;
                    ShowFilter(true);
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        //Open Gallery
        private void ImgGalleryOnClick()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDcimNiceArt, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Activity()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (CropImage.IsExplicitCameraPermissionRequired(this) &&
                        CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.Camera,
                            Manifest.Permission.ReadExternalStorage,
                            Manifest.Permission.WriteExternalStorage,
                        }, CropImage.PickImagePermissionsRequestCode);
                    }
                    else
                    {
                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDcimNiceArt,Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Text

        public void OnDone(string inputText, string colorCode, ViewTextType changeText, Typeface textTypeface)
        {
            try
            {
                if (changeText == ViewTextType.Add)
                    MNiceArtEditor.AddText(textTypeface, inputText, colorCode);
                else
                    MNiceArtEditor.EditText(MView, textTypeface, inputText, colorCode);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Change Text
        public void OnEditTextChangeListener(View rootView, string text, int colorCode)
        {
            try
            {
                MView = rootView;
                var textEditorDialogFragment = new TextEditorFragment(this,null);
                textEditorDialogFragment.Show(this, this, null, text, colorCode, ViewTextType.Change);
                textEditorDialogFragment.SetOnTextEditorListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Color && Filter Color

        private void ColorPickerAdapterOnItemClick(object sender, ColorPickerAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = ColorPickerAdapter.GetItem(position);
                    if (item != null)
                    {
                        if (MToolType == ToolType.Color)
                        {
                            MNiceArtEditorView.GetSource().ClearColorFilter();
                            MNiceArtEditor.ClearAllViews();
                            MNiceArtEditorView.GetSource().SetColorFilter(Color.ParseColor(item.ColorFirst));
                        }
                        else if (MToolType == ToolType.FilterColor)
                        {
                            MNiceArtEditorView.GetSource().ClearColorFilter();
                            MNiceArtEditorView.GetSource().SetColorFilter(Color.ParseColor(item.ColorFirst),PorterDuff.Mode.Multiply);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 101)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        SaveImage();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == 10)
                {
                    Methods.Path.Chack_MyFolder();
                }
                else if (requestCode == CropImage.PickImagePermissionsRequestCode)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDcimNiceArt,
                            Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == CropImage.CameraCapturePermissionsRequestCode)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                        CropImage.StartPickImageActivity(this);
                    else
                        Toast.MakeText(this, GetString(Resource.String.Lbl_Permission_is_denailed), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Result 
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            try
            {  
                if (requestCode == CropImage.CropImageActivityRequestCode && resultCode == Result.Ok)
                {
                    var result = CropImage.GetActivityResult(data);
                    if (result.IsSuccessful)
                    {
                        var resultUri = result.Uri;

                        if (!string.IsNullOrEmpty(resultUri.Path))
                        {
                            var source = ImageDecoder.CreateSource(ContentResolver, resultUri);
                            var bitmap = ImageDecoder.DecodeBitmap(source);
                            if (bitmap != null)
                                MNiceArtEditor?.AddImage(bitmap); 
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    } 
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Save

        private void SaveImage()
        {
            try
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "... ");

                        if ((int)Build.VERSION.SdkInt < 23)
                        {
                            if (!Directory.Exists(Methods.Path.FolderDcimNiceArt))
                                Directory.CreateDirectory(Methods.Path.FolderDcimNiceArt);

                            var file = new File(Methods.Path.FolderDcimNiceArt + File.Separator + "" + BitmapUtil.GetTimestamp(DateTime.Now) +
                                                ".png");
                            try
                            {
                                file.CreateNewFile();

                                var saveSettings = new SaveSettings.Builder()
                                    .SetClearViewsEnabled(true)
                                    .SetTransparencyEnabled(true)
                                    .Build();

                                MNiceArtEditor.SaveAsFile(file.Path, saveSettings, this);
                            }
                            catch (FileNotFoundException e)
                            {
                                //e.PrintStackTrace();
                                Methods.DisplayReportResultTrack(e);
                            }
                            catch (IOException e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        }
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                                CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                if (!Directory.Exists(Methods.Path.FolderDcimNiceArt))
                                    Directory.CreateDirectory(Methods.Path.FolderDcimNiceArt);

                                var file = new File(Methods.Path.FolderDcimNiceArt + File.Separator + "" +
                                                    BitmapUtil.GetTimestamp(DateTime.Now) + ".png");
                                try
                                {
                                    file.CreateNewFile();

                                    var saveSettings = new SaveSettings.Builder()
                                        .SetClearViewsEnabled(true)
                                        .SetTransparencyEnabled(true)
                                        .Build();

                                    MNiceArtEditor.SaveAsFile(file.Path, saveSettings, this);

                                }
                                catch (FileNotFoundException e)
                                {
                                    //e.PrintStackTrace();
                                    Methods.DisplayReportResultTrack(e);
                                }
                                catch (IOException e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            }
                            else
                            {
                                RequestPermissions(new string[]
                                {
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                }, 101);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e); 
                    } 
                }); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShowSaveDialog()
        {
            try
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(GetText(Resource.String.Lbl_Are_you_want_to_exit_without_saving_image));
                builder.SetPositiveButton(GetText(Resource.String.Lbl_Save), delegate (object sender, DialogClickEventArgs args) { SaveImage(); });
                builder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), delegate (object sender, DialogClickEventArgs args) { });
                builder.SetNeutralButton(GetText(Resource.String.Lbl_Discard), delegate (object sender, DialogClickEventArgs args) { var resultIntent = new Intent(); SetResult(Result.Canceled, resultIntent); Finish(); });
                builder.Create().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnSuccess(string imagePath, Bitmap savedResultBitmap)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        AndHUD.Shared.Dismiss(this);
                        //File pathOfFile = new File(imagePath);
                        //MNiceArtEditorView.GetSource().SetImageURI(Uri.FromFile(pathOfFile));

                        //Show image in Gallery
                        //var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                        //mediaScanIntent.SetData(Uri.FromFile(pathOfFile));
                        //SendBroadcast(mediaScanIntent);

                        // Tell the media scanner about the new file so that it is
                        // immediately available to the user.
                        MediaScannerConnection.ScanFile(this, new string[] { imagePath }, null, null);

                        // put the String to pass back into an Intent and close this activity
                        var resultIntent = new Intent();
                        resultIntent.PutExtra("ImagePath", imagePath);
                        resultIntent.PutExtra("ImageId", IdImage);
                        SetResult(Result.Ok, resultIntent);

                        Finish();
                        MNiceArtEditor.ClearAllViews(); 
                        MNiceArtEditorView.GetSource().ClearColorFilter(); 
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e); 
                    }
                }); 
            }
            catch (Exception e)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnFailure(string exception)
        {
            try
            {
                //Show a Error image with a message
                AndHUD.Shared.ShowError(this, GetText(Resource.String.Lbl_Failed_to_save_Image), MaskType.Clear, TimeSpan.FromSeconds(2));

                // put the String to pass back into an Intent and close this activity
                var resultIntent = new Intent();
                SetResult(Result.Canceled, resultIntent);

                Finish();
                AndHUD.Shared.Dismiss(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public void SetImageNiceArt(Uri uri)
        {
            try
            {
                MNiceArtEditor.ClearAllViews();
                MNiceArtEditorView.GetSource().ClearColorFilter();
                
                var file2 = new File(uri.Path);
                var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                 
                Glide.With(this)
                    .AsBitmap()
                    .Load(photoUri)
                    .Apply(new RequestOptions())
                    .Into(new MySimpleTarget(this, MNiceArtEditorView.GetSource()));

                //Bitmap bitMap = null!;
                //ImageDecoder.Source source = ImageDecoder.CreateSource(file2);
                //if (source != null)
                //{
                //    bitMap = ImageDecoder.DecodeBitmap(source);
                //    MNiceArtEditorView.GetSource().SetImageBitmap(bitMap);
                //} 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MySimpleTarget : CustomTarget
        {
            private readonly EditImageActivity Activity;
            private readonly ImageView Image;
            public MySimpleTarget(EditImageActivity activity , ImageView image)
            {
                try
                {
                    Activity = activity;
                    Image = image; 
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
            public override void OnLoadCleared(Drawable p0) { }

            public override void OnResourceReady(Object resource, ITransition transition)
            {
                try
                {
                    var llmFilters = new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false);
                    Activity.MRvFilters.SetLayoutManager(llmFilters);

                    if (resource is Bitmap bitmap)
                    {
                        Image.SetImageBitmap(bitmap);

                        Activity.MFilterViewAdapter = new FilterViewAdapter(Activity, Activity, bitmap);
                        Activity.MFilterViewAdapter.ItemClick += Activity.MFilterViewAdapterOnItemClick;
                        Activity.MRvFilters.SetAdapter(Activity.MFilterViewAdapter);

                        Activity.PrepareThumbnail(bitmap);
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            } 
        }

        public override void OnBackPressed()
        {
            try
            {
                if (MIsFilterVisible)
                {
                    MRvFilters.Visibility = ViewStates.Gone;
                    MRvTools.Visibility = ViewStates.Visible;
                    MRvColor.Visibility = ViewStates.Gone;
                    SeekBarRotate.Visibility = ViewStates.Gone;

                    ShowFilter(false);
                }
                else if (!MNiceArtEditor.IsCacheEmpty())
                {
                    ShowSaveDialog();
                }
                else
                {
                    base.OnBackPressed();
                    var resultIntent = new Intent();
                    SetResult(Result.Canceled, resultIntent);
                    Finish();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            { 
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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