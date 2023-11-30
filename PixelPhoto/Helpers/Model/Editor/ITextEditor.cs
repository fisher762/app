using Android.Graphics;
using PixelPhoto.NiceArt.Models;

namespace PixelPhoto.Helpers.Model.Editor
{
    public interface ITextEditor
    {
        /// <summary>
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="colorCode"></param>
        /// <param name="changeText"></param>
        /// <param name="textTypeface"></param>
        void OnDone(string inputText, string colorCode, ViewTextType changeText, Typeface textTypeface);
    }
}