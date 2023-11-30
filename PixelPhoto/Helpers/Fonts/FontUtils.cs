using System;
using Android.App;
using Android.Graphics;
using Android.Widget;
using PixelPhoto.Helpers.Utils;

namespace PixelPhoto.Helpers.Fonts
{
    public static class FontUtils
    {
        //Changes the TextView To IconFrameWork Fonts
        public static void SetTextViewIcon(FontsIconFrameWork type, TextView textViewUi, string iconUnicode)
        {
            try
            {
                if (type == FontsIconFrameWork.IonIcons)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode)) textViewUi.Text = iconUnicode;
                    //return font;
                }
                else if (type == FontsIconFrameWork.FontAwesomeSolid)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-solid-900.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode)) textViewUi.Text = iconUnicode;
                    //return font;
                }
                else if (type == FontsIconFrameWork.FontAwesomeRegular)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-regular-400.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode)) textViewUi.Text = iconUnicode;
                    //return font;
                }
                else if (type == FontsIconFrameWork.FontAwesomeBrands)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-brands-400.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode)) textViewUi.Text = iconUnicode;
                    //return font;
                }
                else if (type == FontsIconFrameWork.FontAwesomeLight)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-light-300.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode)) textViewUi.Text = iconUnicode;
                    //return font;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Set_TextViewIcon Function ERROR " + e);
                Methods.DisplayReportResultTrack(e);
                //return null!;
            }
        }
    }
}