using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using Object = Java.Lang.Object;

namespace PixelPhoto.Adapters
{
    public class ViewPagerStringAdapter : PagerAdapter
    {

        public Context Context;
        public List<Classes.ViewPagerStrings> ListDescriptions;
        public LayoutInflater Inflater;

        public ViewPagerStringAdapter(Context context , List<Classes.ViewPagerStrings> listDescriptions)
        {
            Context = context;
            ListDescriptions = listDescriptions;
            Inflater = LayoutInflater.From(context);
        }

        public override Object InstantiateItem(ViewGroup view, int position)
        {
            try
            {
                var layout = Inflater.Inflate(Resource.Layout.FirstPageViewPagerLayout, view, false);
                var textRemember = layout.FindViewById<TextView>(Resource.Id.tv_remember);
                var tvDescription = layout.FindViewById<TextView>(Resource.Id.tv_description);

                tvDescription.Text = ListDescriptions[position].Description;
                textRemember.Text = ListDescriptions[position].Header;

                view.AddView(layout);

                return layout;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }

        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return view.Equals(@object);
        }

        public override int Count
        {
            get
            {
                if (ListDescriptions != null)
                {
                    return ListDescriptions.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            try
            {
                var view = (View)@object;
                container.RemoveView(view);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
            
        }
    }

}