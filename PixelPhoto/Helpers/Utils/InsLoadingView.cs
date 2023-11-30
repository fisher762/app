using System;
using System.Collections.Generic;
using Android.Animation;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Firebase;

namespace PixelPhoto.Helpers.Utils
{
    public class InsLoadingView : ImageView, ValueAnimator.IAnimatorUpdateListener, Animator.IAnimatorListener
    {
        private new static readonly string Tag = "InsLoadingView";
        private static readonly bool Debug = BuildConfig.Debug;
        private static readonly float ArcWidth = 12;
        private static readonly int MinWidth = 300;
        private static readonly float CircleDia = 0.9f;
        private static readonly float StrokeWidth = 0.025f;
        private static readonly float ArcChangeAngle = 0.2f;
        private static readonly Color ClickedColor = Color.LightGray;

        public enum Status { Loading, Clicked, UnClicked }

        private static List<Status> SStatusArray;

        //sStatusArray = new SparseArray<>(3);
        //sStatusArray.put(0, Status.LOADING);
        //sStatusArray.put(1, Status.CLICKED);
        //sStatusArray.put(2, Status.UNCLICKED);

        private Status MStatus = Status.Loading;
        private int MRotateDuration = 10000;
        private int MCircleDuration = 2000;
        private readonly float BitmapDia = CircleDia - StrokeWidth;
        private readonly float MRotateDegree = 0;
        private float MCircleWidth;
        private bool MIsFirstCircle = true;
        private ValueAnimator MRotateAnim;
        private ValueAnimator MCircleAnim;
        private ValueAnimator MTouchAnim;
        private Color MStartColor = Color.ParseColor("#F700C2");
        private Color MEndColor = Color.ParseColor("#FFD900");
        private readonly float MScale = 1f;
        private Paint MBitmapPaint;
        private Paint MTrackPaint;
        private RectF MBitmapRectF;
        private RectF MTrackRectF;
 
        protected InsLoadingView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public InsLoadingView(Context context) : base(context)
        {
            Init(context, null);
            
        }

        public InsLoadingView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context, attrs);
        }

        public InsLoadingView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context, attrs);
        }

        public InsLoadingView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context, attrs);
        }

        private void Init(Context context, IAttributeSet attrs)
        {
            try
            {
                SetStatusArray();

                if (attrs != null)
                    ParseAttrs(context, attrs);

                OnCreateAnimators();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetStatusArray()
        {
            try
            {
                SStatusArray = new List<Status>();
                SStatusArray.Add( Status.Loading);
                SStatusArray.Add(Status.Clicked);
                SStatusArray.Add(Status.UnClicked);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ParseAttrs(Context context, IAttributeSet attrs)
        {
            try
            {
                var typedArray = context.ObtainStyledAttributes(attrs, Resource.Styleable.InsLoadingViewAttr);
                var startColor = typedArray.GetColor(Resource.Styleable.InsLoadingViewAttr_start_color, MStartColor);
                var endColor = typedArray.GetColor(Resource.Styleable.InsLoadingViewAttr_end_color, MEndColor);
                var circleDuration = typedArray.GetInt(Resource.Styleable.InsLoadingViewAttr_circle_duration, MCircleDuration);
                var rotateDuration = typedArray.GetInt(Resource.Styleable.InsLoadingViewAttr_rotate_duration, MRotateDuration);
                var status = typedArray.GetInt(Resource.Styleable.InsLoadingViewAttr_status, 0);
                if (Debug)
                {
                    Console.WriteLine(Tag, "parseAttrs start_color: " + startColor);
                    Console.WriteLine(Tag, "parseAttrs end_color: " + endColor);
                    Console.WriteLine(Tag, "parseAttrs rotate_duration: " + rotateDuration);
                    Console.WriteLine(Tag, "parseAttrs circle_duration: " + circleDuration);
                    Console.WriteLine(Tag, "parseAttrs status: " + status);
                }
                typedArray.Recycle();
                if (circleDuration != MCircleDuration)
                {
                    SetCircleDuration(circleDuration);
                }
                if (rotateDuration != MRotateDuration)
                {
                    SetRotateDuration(rotateDuration);
                }
                SetStartColor(startColor);
                SetEndColor(endColor);
                if (SStatusArray != null) SetStatus(SStatusArray[status]);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void InitPaints()
        {
            try
            {
                MBitmapPaint ??= GetBitmapPaint();

                MTrackPaint ??= GetTrackPaint();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }    
        }

        private void InitRectFs()
        {
            try
            {
                MBitmapRectF ??= new RectF(Width * (1 - BitmapDia), Width * (1 - BitmapDia),
                    Width * BitmapDia, Height * BitmapDia);

                MTrackRectF ??= new RectF(Width * (1 - CircleDia), Width * (1 - CircleDia),
                    Width * CircleDia, Height * CircleDia);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public InsLoadingView SetCircleDuration(int circleDuration)
        {
            try
            {
                this.MCircleDuration = circleDuration;
                MCircleAnim.SetDuration(MCircleDuration);
                return this;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return this;
            }
        }

        public InsLoadingView SetRotateDuration(int rotateDuration)
        {
            try
            {
                this.MRotateDuration = rotateDuration;
                MRotateAnim.SetDuration(MRotateDuration);
                return this;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return this;
            }
        }

        public void SetStatus(Status status)
        {
            this.MStatus = status;
        }

        public Status GetStatus()
        {
            return MStatus;
        }

        public void SetStartColor(Color startColor)
        {
            try
            {
                MStartColor = startColor;
                MTrackPaint = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetEndColor(Color endColor)
        {
            try
            {
                MEndColor = endColor;
                MTrackPaint = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            try
            {
                var widthSpecMode = MeasureSpec.GetMode(widthMeasureSpec);
                var widthSpecSize = MeasureSpec.GetSize(widthMeasureSpec);
                var heightSpecMode = MeasureSpec.GetMode(heightMeasureSpec);
                var heightSpecSize = MeasureSpec.GetSize(heightMeasureSpec);
                if (Debug)
                {
                    Console.WriteLine(Tag, String.Format("onMeasure widthMeasureSpec: %s -- %s", widthSpecMode, widthSpecSize));
                    Console.WriteLine(Tag, String.Format("onMeasure heightMeasureSpec: %s -- %s", heightSpecMode, heightSpecSize));
                }
                int width;
                if (widthSpecMode == MeasureSpecMode.Exactly && heightSpecMode == MeasureSpecMode.Exactly)
                {
                    width = Math.Min(widthSpecSize, heightSpecSize);
                }
                else
                {
                    width = Math.Min(widthSpecSize, heightSpecSize);
                    width = Math.Min(width, MinWidth);
                }
                SetMeasuredDimension(width, width); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            try
            {
                InitPaints();
                InitRectFs();
                canvas.Scale(MScale, MScale, CenterX(), CenterY());
                DrawBitmap(canvas);
                switch (MStatus)
                {
                    case Status.Loading:
                        DrawTrack(canvas, MTrackPaint);
                        break;
                    case Status.UnClicked:
                        DrawCircle(canvas, MTrackPaint);
                        break;
                    case Status.Clicked:
                        DrawClickedCircle(canvas);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnVisibilityChanged(View changedView, ViewStates visibility)
        {
            try
            {
                if (Debug)
                {
                    Console.WriteLine(Tag, "onVisibilityChanged");
                }
                if (visibility == ViewStates.Visible)
                {
                    StartAnim();
                }
                else
                {
                    EndAnim();
                }

                base.OnVisibilityChanged(changedView, visibility);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            var result = false;
            if (Debug)
            {
                Console.WriteLine(Tag, "onTouchEvent: " + e.Action);
            }

            switch (e.Action)
            {
                case MotionEventActions.Down:
                {
                    StartDownAnim();
                    result = true;
                    break;
                }
                case MotionEventActions.Up:
                {
                    StartUpAnim();
                    break;
                }
                case MotionEventActions.Cancel:
                {
                    StartUpAnim();
                    break;
                }
            }

            return base.OnTouchEvent(e) || result;
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            try
            {
                if (Debug)
                {
                    Console.WriteLine(Tag, "onSizeChanged");
                }
                MBitmapRectF = null!;
                MTrackRectF = null!;
                MBitmapPaint = null!;
                MTrackPaint = null!;

                base.OnSizeChanged(w, h, oldw, oldh);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void SetImageDrawable(Drawable drawable)
        {
            try
            {
                if (Debug)
                {
                    Console.WriteLine(Tag, "setImageDrawable");
                }
                MBitmapPaint = null!;

                base.SetImageDrawable(drawable);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

        }

        private float CenterX()
        {
            return Width / 2;
        }

        private float CenterY()
        {
            return Height / 2;
        }

        private void OnCreateAnimators()
        {
            try
            {
                MRotateAnim = ValueAnimator.OfFloat(0, 180, 360);
                MRotateAnim.AddUpdateListener(this);
                MRotateAnim.SetInterpolator(new LinearInterpolator());
                MRotateAnim.SetDuration(MRotateDuration);
                MRotateAnim.RepeatCount  = (-1);
                MCircleAnim = ValueAnimator.OfFloat(0, 360);
                MCircleAnim.SetInterpolator(new DecelerateInterpolator());
                MCircleAnim.SetDuration(MCircleDuration);
                MCircleAnim.RepeatCount = (-1);
                MCircleAnim.AddUpdateListener(this);
                MCircleAnim.AddListener(this);
                MTouchAnim = new ValueAnimator();
                MTouchAnim.SetInterpolator(new DecelerateInterpolator());
                MTouchAnim.SetDuration(200);
                MTouchAnim.AddUpdateListener(this);
                StartAnim();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            try
            {
                if (MIsFirstCircle)
                {
                    MCircleWidth = (float)animation.AnimatedValue;
                }
                else
                {
                    MCircleWidth = (float)animation.AnimatedValue - 360;
                }
                PostInvalidate();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnAnimationCancel(Animator animation)
        {
             
        }

        public void OnAnimationEnd(Animator animation)
        {
            
        }

        public void OnAnimationRepeat(Animator animation)
        {
            MIsFirstCircle = !MIsFirstCircle;
        }

        public void OnAnimationStart(Animator animation)
        {
            
        }

        private void DrawBitmap(Canvas canvas)
        {
            canvas.DrawOval(MBitmapRectF, MBitmapPaint);
        }

        private void DrawTrack(Canvas canvas, Paint paint)
        {
            try
            {
                canvas.Rotate(MRotateDegree, CenterX(), CenterY());
                canvas.Rotate(ArcWidth, CenterX(), CenterY());

                if (Debug)
                {
                    Console.WriteLine(Tag, "circleWidth:" + MCircleWidth);
                }
                if (MCircleWidth < 0)
                {
                    //a
                    var startArg = MCircleWidth + 360;
                    canvas.DrawArc(MTrackRectF, startArg, 360 - startArg, false, paint);
                    var adjustCircleWidth = MCircleWidth + 360;
                    float width = 8;
                    while (adjustCircleWidth > ArcWidth)
                    {
                        width = width - ArcChangeAngle;
                        adjustCircleWidth = adjustCircleWidth - ArcWidth;
                        canvas.DrawArc(MTrackRectF, adjustCircleWidth, width, false, paint);
                    }
                }
                else
                {
                    //b
                    for (var i = 0; i <= 4; i++)
                    {
                        if (ArcWidth * i > MCircleWidth)
                        {
                            break;
                        }
                        canvas.DrawArc(MTrackRectF, MCircleWidth - ArcWidth * i, 8 + i, false, paint);
                    }
                    if (MCircleWidth > ArcWidth * 4)
                    {
                        canvas.DrawArc(MTrackRectF, 0, MCircleWidth - ArcWidth * 4, false, paint);
                    }
                    float adjustCircleWidth = 360;
                    var width = 8 * (360 - MCircleWidth) / 360;
                    if (Debug)
                    {
                        Console.WriteLine(Tag, "width:" + width);
                    }
                    while (width > 0 && adjustCircleWidth > ArcWidth)
                    {
                        width = width - ArcChangeAngle;
                        adjustCircleWidth = adjustCircleWidth - ArcWidth;
                        canvas.DrawArc(MTrackRectF, adjustCircleWidth, width, false, paint);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DrawCircle(Canvas canvas, Paint paint)
        {
            try
            {
                var rectF = new RectF(Width * (1 - CircleDia), Width * (1 - CircleDia), Width * CircleDia, Height * CircleDia);
                canvas.DrawOval(rectF, paint);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DrawClickedCircle(Canvas canvas)
        {
            try
            {
                var paintClicked = new Paint
                {
                    Color = (ClickedColor)
                };
                SetPaintStroke(paintClicked);
                DrawCircle(canvas, paintClicked);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartDownAnim()
        {
            try
            {
                MTouchAnim.SetFloatValues(MScale, 0.9f);
                MTouchAnim.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartUpAnim()
        {
            try
            {
                MTouchAnim.SetFloatValues(MScale, 1);
                MTouchAnim.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartAnim()
        {
            try
            {
                MRotateAnim.Start();
                MCircleAnim.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void EndAnim()
        {
            try
            {
                MRotateAnim.End();
                MCircleAnim.End();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private Paint GetTrackPaint()
        {
            try
            {
                var paint = new Paint();
                Shader shader = new LinearGradient(0f, 0f, (Width * CircleDia * (360 - ArcWidth * 4) / 360),Height * StrokeWidth, MStartColor, MEndColor, Shader.TileMode.Clamp);
                paint.SetShader(shader);
                SetPaintStroke(paint);
                return paint;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        private void SetPaintStroke(Paint paint)
        {
            try
            {
                paint.SetStyle(Paint.Style.Stroke);
                paint.StrokeWidth = (Height * StrokeWidth);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private Paint GetBitmapPaint()
        {
            try
            {
                var paint = new Paint();
                var drawable = Drawable;
                var matrix = new Matrix();
                if (null == drawable)
                {
                    return paint;
                }
                var bitmap = DrawableToBitmap(drawable);
                var shader = new BitmapShader(bitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);
                var size = Math.Min(bitmap.Width, bitmap.Height);
                var scale = Width * 1.0f / size;
                matrix.SetScale(scale, scale);
                if (bitmap.Width > bitmap.Height)
                {
                    matrix.PostTranslate(-(bitmap.Width * scale - Width) / 2, 0);
                }
                else
                {
                    matrix.PostTranslate(0, -(bitmap.Height * scale - Height) / 2);
                }
                shader.SetLocalMatrix(matrix);
                paint.SetShader(shader);
                return paint;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        private Bitmap DrawableToBitmap(Drawable drawable)
        {
            try
            {
                if (drawable is BitmapDrawable bitmapDrawable) { 
                    return bitmapDrawable.Bitmap;
                }
                var w = drawable.IntrinsicWidth;
                var h = drawable.IntrinsicHeight;
                var bitmap = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);
                var canvas = new Canvas(bitmap);
                drawable.SetBounds(0, 0, w, h);
                drawable.Draw(canvas);
                return bitmap;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }
    }
}