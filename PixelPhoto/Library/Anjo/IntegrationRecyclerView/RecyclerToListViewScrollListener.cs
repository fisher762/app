﻿using System;
using Android.Widget;
using AndroidX.RecyclerView.Widget;

namespace PixelPhoto.Library.Anjo.IntegrationRecyclerView
{
    public class RecyclerToListViewScrollListener : RecyclerView.OnScrollListener
    {
        public static readonly int UNKNOWN_SCROLL_STATE = int.MinValue;
        private readonly AbsListView.IOnScrollListener scrollListener;
        private int lastFirstVisible = -1;
        private int lastVisibleCount = -1;
        private int lastItemCount = -1;

        public RecyclerToListViewScrollListener(AbsListView.IOnScrollListener scrollListener)
        {
            this.scrollListener = scrollListener;
        }

        public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
        {
            base.OnScrollStateChanged(recyclerView, newState);
            var listViewState = ScrollState.TouchScroll; //wael;

            switch (newState)
            {
                case RecyclerView.ScrollStateDragging:
                    listViewState = ScrollState.TouchScroll;
                    break;
                case RecyclerView.ScrollStateIdle:
                    listViewState = ScrollState.Idle;
                    break;
                case RecyclerView.ScrollStateSettling:
                    listViewState = ScrollState.Fling;
                    break; 
            }

            scrollListener.OnScrollStateChanged(null /*view*/,  listViewState);

        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);

            var layoutManager = (LinearLayoutManager)recyclerView.GetLayoutManager();

            var firstVisible = layoutManager.FindFirstVisibleItemPosition();
            var visibleCount = Math.Abs(firstVisible - layoutManager.FindLastVisibleItemPosition());
            var itemCount = recyclerView.GetAdapter().ItemCount;

            if (firstVisible != lastFirstVisible
                || visibleCount != lastVisibleCount
                || itemCount != lastItemCount)
            {
                scrollListener.OnScroll(null, firstVisible, visibleCount, itemCount);
                lastFirstVisible = firstVisible;
                lastVisibleCount = visibleCount;
                lastItemCount = itemCount;
            }
        }

    }
    
}