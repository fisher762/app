using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Java.Lang;
using PixelPhoto.Helpers.Utils;
using Exception = Java.Lang.Exception;

namespace PixelPhoto.Library.Anjo.SuperTextLibrary
{
    public class XLinkTouchMovementMethod : LinkMovementMethod
    {
        private XTouchableSpan PressedSpan;

        public override bool OnTouchEvent(TextView textView, ISpannable spannable, MotionEvent e)
        {
            try
            {
                var action = e.Action;
                if (action == MotionEventActions.Down)
                {
                    PressedSpan = GetPressedSpan(textView, spannable, e);
                    if (PressedSpan != null)
                    {
                        PressedSpan.SetPressed(true);
                        Selection.SetSelection(spannable, spannable.GetSpanStart(PressedSpan), spannable.GetSpanEnd(PressedSpan));
                    }
                }
                else if (action == MotionEventActions.Move)
                {
                    var touchedSpan = GetPressedSpan(textView, spannable, e);
                    if (PressedSpan != null && touchedSpan != PressedSpan)
                    {
                        PressedSpan.SetPressed(false);
                        PressedSpan = null!;
                        Selection.RemoveSelection(spannable);
                    }
                }
                else
                {
                    if (PressedSpan != null)
                    {
                        PressedSpan.SetPressed(false);
                        base.OnTouchEvent(textView, spannable, e);
                    }

                    PressedSpan = null!;
                    Selection.RemoveSelection(spannable);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }

            return true;

        }

        private XTouchableSpan GetPressedSpan(TextView textView, ISpannable spannable, MotionEvent e)
        { 
            try
            {
                var x = (int)e.GetX();
                var y = (int)e.GetY();

                x -= textView.TotalPaddingLeft;
                y -= textView.TotalPaddingTop;

                x += textView.ScrollX;
                y += textView.ScrollY;

                var layout = textView.Layout;
                var verticalLine = layout.GetLineForVertical(y);
                var horizontalOffset = layout.GetOffsetForHorizontal(verticalLine, x);

                var link = spannable.GetSpans(horizontalOffset, horizontalOffset, Class.FromType(typeof(XTouchableSpan)));

                if (link?.Length > 0)
                {
                    var sdfs = (XTouchableSpan)link[0];
                    return sdfs;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }

            return null!;
        }

    }
}