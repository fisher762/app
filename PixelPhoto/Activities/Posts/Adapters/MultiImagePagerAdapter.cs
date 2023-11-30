using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Java.Lang;
using PixelPhoto.Helpers.Utils;
using Exception = Java.Lang.Exception;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.Posts.Adapters
{
    public class MultiImagePagerAdapter : PagerAdapter
    {
        private readonly List<string> Images;
        private readonly LayoutInflater Inflater;
        private readonly Activity Context;
        private readonly RequestBuilder FullGlideRequestBuilder;
        public MultiImagePagerAdapter(Activity context, List<string> images)
        {
            Context = context;
            Images = images;
            Inflater = LayoutInflater.From(context);
            FullGlideRequestBuilder = Glide.With(context).AsDrawable().SetDiskCacheStrategy(DiskCacheStrategy.Automatic).Placeholder(new ColorDrawable(Color.ParseColor("#efefef")));


            try
            {
                foreach (var image in from item in images let image = images.IndexOf(item) where !string.IsNullOrEmpty(item) && image > -1 select image)
                {
                    FullGlideRequestBuilder.Load(Images[image]);
                }
            }
            catch (System.Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
            
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return view.Equals(@object);
        }

        public override int Count => Images?.Count ?? 0;

        public override Object InstantiateItem(ViewGroup view, int position)
        {
            try
            {
                var imageLayout = Inflater.Inflate(Resource.Layout.ViewSubOneImage, view, false);
                var imageView = imageLayout.FindViewById<ImageView>(Resource.Id.image);

                FullGlideRequestBuilder.Load(Images[position]).Into(imageView);
                

                view.AddView(imageLayout, 0);
                return imageLayout;
            }
            catch (System.Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            } 
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            try
            {
                container?.RemoveView((View)@object);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void RestoreState(IParcelable state, ClassLoader loader)
        {
            
        }

       
    }
}