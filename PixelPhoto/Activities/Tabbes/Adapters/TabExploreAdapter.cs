using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Util;
using AndroidX.RecyclerView.Widget;
using PixelPhoto.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using PixelPhoto.Activities.Posts.page;
using PixelPhoto.Activities.Store;
using PixelPhoto.Activities.Store.Adapters;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Store;
using PixelPhotoClient.GlobalClass;

namespace PixelPhoto.Activities.Tabbes.Adapters
{
    public class TabExploreAdapter : RecyclerView.Adapter
    {
        public event EventHandler<TabExploreAdapterClickEventArgs> ItemClick;
        public event EventHandler<TabExploreAdapterClickEventArgs> ItemLongClick;
        
        private readonly Activity ActivityContext;
        private readonly HomeActivity GlobalContext;
        private SuggestionsAdapter SuggestionsAdapter;
        public HStoreAdapter StoreAdapter;
        private ExploreAdapter ExploreAdapter, FeaturedAdapter;
         
        public ObservableCollection<Classes.ExploreClass> ExploreList = new ObservableCollection<Classes.ExploreClass>();
        public RecyclerView.RecycledViewPool RecycledViewPool { get; set; }
         
        private AllExplorePostFragment AllExplorePostFragment;

        public TabExploreAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                GlobalContext = HomeActivity.GetInstance();
                RecycledViewPool = new RecyclerView.RecycledViewPool();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                if (viewType == (int)Classes.ItemType.User || viewType == (int)Classes.ItemType.Store || viewType == (int)Classes.ItemType.Post || viewType == (int)Classes.ItemType.Featured)
                {
                    //Setup your layout here >> TemplateRecyclerViewLayout
                    var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                    var vh = new TabExploreAdapterViewHolder(itemView, OnClick, OnLongClick);
                    vh.MRecycler.SetRecycledViewPool(RecycledViewPool);
                    return vh;
                }
                 
                if (viewType == (int)Classes.ItemType.EmptyPage)
                {
                    //Setup your layout here >> EmptyStateLayout
                    var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.EmptyStateLayout, parent, false);
                    var vh = new EmptyStateViewHolder(itemView);
                    return vh;
                }

                return null!;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                var item = ExploreList[position];
                if (item != null)
                {
                    if (!(viewHolder is TabExploreAdapterViewHolder holder)) return;

                    switch (item.Type)
                    {
                        case Classes.ItemType.User:
                        {
                            if (SuggestionsAdapter == null)
                            {
                                SuggestionsAdapter = new SuggestionsAdapter(ActivityContext, 1)
                                {
                                    SuggestionsList = new ObservableCollection<UserDataObject>()
                                };

                                var layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                holder.MRecycler.SetLayoutManager(layoutManager);
                                holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                var preLoader = new RecyclerViewPreloader<UserDataObject>(ActivityContext, SuggestionsAdapter, sizeProvider, 10);
                                holder. MRecycler.AddOnScrollListener(preLoader);
                                holder.MRecycler.SetAdapter(SuggestionsAdapter);
                                SuggestionsAdapter.ItemClick += SuggestionsAdapterOnItemClick;

                                var xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(layoutManager);
                                var mainScrollEvent = xamarinRecyclerViewOnScrollListener;
                                mainScrollEvent.LoadMoreEvent += SuggestionsMainScrollEventOnLoadMoreEvent;
                                holder.MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                                mainScrollEvent.IsLoading = false;

                                holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_SuggestionsForYou);
                                holder.MainLinear.Visibility = ViewStates.Visible;
                                holder.MainLinear.Click += SuggestionsMainLinearOnClick;
                            }

                            var countList = item.UserList.Count;
                            if (item.UserList.Count > 0)
                            {
                                if (countList > 0)
                                { 
                                    foreach (var user in from user in item.UserList let check = SuggestionsAdapter.SuggestionsList.FirstOrDefault(a => a.UserId == user.UserId) where check == null select user)
                                    {
                                        SuggestionsAdapter.SuggestionsList.Add(user);
                                    }

                                    SuggestionsAdapter.NotifyItemRangeInserted(countList, SuggestionsAdapter.SuggestionsList.Count - countList);
                                }
                                else
                                {
                                    SuggestionsAdapter.SuggestionsList = new ObservableCollection<UserDataObject>(item.UserList);
                                    SuggestionsAdapter.NotifyDataSetChanged();
                                }
                            }

                            holder.MoreText.Visibility = SuggestionsAdapter.SuggestionsList?.Count >= 4 ? ViewStates.Visible : ViewStates.Invisible;
                            break;
                        }
                        case Classes.ItemType.Store:
                        {
                            if (StoreAdapter == null)
                            {
                                StoreAdapter = new HStoreAdapter(ActivityContext)
                                {
                                    StoreList = new ObservableCollection<StoreDataObject>()
                                };

                                var layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                holder.MRecycler.SetLayoutManager(layoutManager);
                                holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                var preLoader = new RecyclerViewPreloader<StoreDataObject>(ActivityContext, StoreAdapter, sizeProvider, 10);
                                holder.MRecycler.AddOnScrollListener(preLoader);
                                holder.MRecycler.SetAdapter(StoreAdapter);
                                StoreAdapter.ItemClick += StoreAdapterOnItemClick;

                                holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Store);
                                holder.MainLinear.Visibility = ViewStates.Visible;
                                holder.MainLinear.Click += StoreMainLinearOnClick;
                            }

                            var countList = item.StoreList.Count;
                            if (item.StoreList.Count > 0)
                            {
                                if (countList > 0)
                                {
                                    foreach (var user in from user in item.StoreList let check = StoreAdapter.StoreList.FirstOrDefault(a => a.Id == user.Id) where check == null select user)
                                    {
                                        StoreAdapter.StoreList.Add(user);
                                    }

                                    StoreAdapter.NotifyItemRangeInserted(countList, StoreAdapter.StoreList.Count - countList);
                                }
                                else
                                {
                                    StoreAdapter.StoreList = new ObservableCollection<StoreDataObject>(item.StoreList);
                                    StoreAdapter.NotifyDataSetChanged();
                                }
                            }

                            holder.MoreText.Visibility = StoreAdapter.StoreList?.Count >= 4 ? ViewStates.Visible : ViewStates.Invisible;
                            break;
                        }
                        case Classes.ItemType.Post:
                        {
                            if (ExploreAdapter == null)
                            {
                                ExploreAdapter = new ExploreAdapter(ActivityContext)
                                {
                                    PostList = new ObservableCollection<PostsObject>()
                                };

                                var layoutManager = new GridLayoutManager(ActivityContext, 3);
                                layoutManager.SetSpanSizeLookup(new MySpanSizeLookup(8, 1, 2));
                                holder.MRecycler.AddItemDecoration(new GridSpacingItemDecoration(1, 1, true));
                                holder.MRecycler.SetLayoutManager(layoutManager);
                                holder.MRecycler.HasFixedSize = true;
                                holder.MRecycler.SetItemViewCacheSize(10);
                                holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                var preLoader = new RecyclerViewPreloader<PostsObject>(ActivityContext, ExploreAdapter, sizeProvider, 10);
                                holder.MRecycler.AddOnScrollListener(preLoader);
                                holder.MRecycler.SetAdapter(ExploreAdapter);
                                ExploreAdapter.ItemClick += ExploreAdapterOnItemClick;

                                holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Explore);
                                holder.MainLinear.Visibility = ViewStates.Visible;
                                holder.MainLinear.Click += ExploreMainLinearOnClick;
                            }

                            var countList = item.PostList.Count;
                            if (item.PostList.Count > 0)
                            {
                                if (countList > 0)
                                {
                                    foreach (var user in from user in item.PostList let check = ExploreAdapter.PostList.FirstOrDefault(a => a.PostId == user.PostId) where check == null select user)
                                    {
                                        ExploreAdapter.PostList.Add(user);
                                    }

                                    ExploreAdapter.NotifyItemRangeInserted(countList, ExploreAdapter.PostList.Count - countList);
                                }
                                else
                                {
                                    ExploreAdapter.PostList = new ObservableCollection<PostsObject>(item.PostList);
                                    ExploreAdapter.NotifyDataSetChanged();
                                }
                            }

                            holder.MoreText.Visibility = ExploreAdapter.PostList?.Count >= 4 ? ViewStates.Visible : ViewStates.Invisible;
                            break;
                        }
                        case Classes.ItemType.Featured:
                        {
                            if (FeaturedAdapter == null)
                            {
                                FeaturedAdapter = new ExploreAdapter(ActivityContext)
                                {
                                    PostList = new ObservableCollection<PostsObject>()
                                };

                                var layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                holder.MRecycler.SetLayoutManager(layoutManager);
                                holder.MRecycler.HasFixedSize = true;
                                holder.MRecycler.SetItemViewCacheSize(10);
                                holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                var preLoader = new RecyclerViewPreloader<PostsObject>(ActivityContext, FeaturedAdapter, sizeProvider, 10);
                                holder.MRecycler.AddOnScrollListener(preLoader);
                                holder.MRecycler.SetAdapter(FeaturedAdapter);
                                FeaturedAdapter.ItemClick += FeaturedAdapterOnItemClick;

                                holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_FeaturedPosts);
                                holder.MainLinear.Visibility = ViewStates.Visible;
                                holder.MoreText.Visibility = ViewStates.Invisible;
                            }
                              
                            var countList = item.PostList.Count;
                            if (item.PostList.Count > 0)
                            {
                                if (countList > 0)
                                {
                                    foreach (var user in from user in item.PostList let check = FeaturedAdapter.PostList.FirstOrDefault(a => a.PostId == user.PostId) where check == null select user)
                                    {
                                        FeaturedAdapter.PostList.Add(user);
                                    }

                                    FeaturedAdapter.NotifyItemRangeInserted(countList, FeaturedAdapter.PostList.Count - countList);
                                }
                                else
                                {
                                    FeaturedAdapter.PostList = new ObservableCollection<PostsObject>(item.PostList);
                                    FeaturedAdapter.NotifyDataSetChanged();
                                }
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Featured Post
         
        private void FeaturedAdapterOnItemClick(object sender, ExploreAdapterViewHolderClickEventArgs e)
        {
            try
            {
                var item = FeaturedAdapter.PostList[e.Position];
                if (item != null)
                {
                    GlobalContext.OpenNewsFeedItem(item.PostId.ToString(), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
        
        #region Explore Post
        
        private void ExploreMainLinearOnClick(object sender, EventArgs e)
        {
            try
            {
                AllExplorePostFragment  = new AllExplorePostFragment();
                GlobalContext.OpenFragment(AllExplorePostFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ExploreAdapterOnItemClick(object sender, ExploreAdapterViewHolderClickEventArgs e)
        {
            try
            {
                var item = ExploreAdapter.PostList[e.Position];
                if (item != null)
                {
                    GlobalContext.OpenNewsFeedItem(item.PostId.ToString(), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion
         
        #region Store

        private void StoreMainLinearOnClick(object sender, EventArgs e)
        {
            try
            { 
                var intent = new Intent(ActivityContext, typeof(StoreActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StoreAdapterOnItemClick(object sender, HStoreAdapterViewHolderClickEventArgs e)
        {
            try
            {
                var item = StoreAdapter.StoreList[e.Position];
                if (item != null)
                {
                    var intent = new Intent(ActivityContext, typeof(StoreViewActivity));
                    intent.PutExtra("StoreId", item.Id.ToString());
                    intent.PutExtra("storeData", JsonConvert.SerializeObject(item));
                    ActivityContext.StartActivity(intent); 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Suggestions User

        //Scroll
        private void SuggestionsMainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //wael
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //All Viewer suggestion
        private void SuggestionsMainLinearOnClick(object sender, EventArgs e)
        {
            try
            {
                var bundle = new Bundle();
                bundle.PutString("type", "suggestion");
                var profileFragment = new AllViewerFragment
                {
                    Arguments = bundle
                };

                GlobalContext.OpenFragment(profileFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open profile
        private void SuggestionsAdapterOnItemClick(object sender, SuggestionsAdapterClickEventArgs e)
        {
            try
            {
                var item = SuggestionsAdapter.SuggestionsList[e.Position];
                if (item != null)
                {
                    AppTools.OpenProfile(ActivityContext, item.UserId, item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        public override int ItemCount => ExploreList?.Count ?? 0;

        public Classes.ExploreClass GetItem(int position)
        {
            return ExploreList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                var item = ExploreList[position];
                if (item != null)
                {
                    return item.Type switch
                    {
                        Classes.ItemType.User => (int) Classes.ItemType.User,
                        Classes.ItemType.Store => (int) Classes.ItemType.Store,
                        Classes.ItemType.Post => (int) Classes.ItemType.Post,
                        Classes.ItemType.Featured => (int) Classes.ItemType.Featured,
                        Classes.ItemType.EmptyPage => (int) Classes.ItemType.EmptyPage,
                        _ => position
                    };
                }

                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        void OnClick(TabExploreAdapterClickEventArgs args) => ItemClick?.Invoke(ActivityContext, args);
        void OnLongClick(TabExploreAdapterClickEventArgs args) => ItemLongClick?.Invoke(ActivityContext, args);
    }

    public class TabExploreAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public RelativeLayout MainLinear { get; private set; }
        public TextView TitleText { get; private set; }
        public TextView MoreText { get; private set; }
        public RecyclerView MRecycler { get; private set; }

        #endregion

        public TabExploreAdapterViewHolder(View itemView, Action<TabExploreAdapterClickEventArgs> clickListener, Action<TabExploreAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                MainLinear = itemView.FindViewById<RelativeLayout>(Resource.Id.mainLinear);
                TitleText = itemView.FindViewById<TextView>(Resource.Id.headText);
                MoreText = itemView.FindViewById<TextView>(Resource.Id.moreText);
                MRecycler = itemView.FindViewById<RecyclerView>(Resource.Id.recycler);

                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);

                //Create an Event
                //itemView.Click += (sender, e) => clickListener(new TabExploreAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                //itemView.LongClick += (sender, e) => longClickListener(new TabExploreAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class EmptyStateViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public Button EmptyStateButton { get; private set; }
        public TextView EmptyStateIcon { get; private set; }
        public TextView DescriptionText { get; private set; }
        public TextView TitleText { get; private set; }

        #endregion

        public EmptyStateViewHolder(View itemView) : base(itemView)
        {
            try
            {
                MainView = itemView;

                EmptyStateIcon = (TextView)itemView.FindViewById(Resource.Id.emtyicon);
                TitleText = (TextView)itemView.FindViewById(Resource.Id.headText);
                DescriptionText = (TextView)itemView.FindViewById(Resource.Id.seconderyText);
                EmptyStateButton = (Button)itemView.FindViewById(Resource.Id.button);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Frown);
                EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoPost_TitleText);
                DescriptionText.Text = " ";
                EmptyStateButton.Visibility = ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    #region SpanSize

    public class MySpanSizeLookup : GridLayoutManager.SpanSizeLookup
    {
        private readonly int SpanPos;
        private readonly int SpanCnt1;
        private readonly int SpanCnt2;

        public MySpanSizeLookup(int spanPos, int spanCnt1, int spanCnt2)
        {
            SpanPos = spanPos;
            SpanCnt1 = spanCnt1;
            SpanCnt2 = spanCnt2;
        }

        public override int GetSpanSize(int position)
        {
            return position % SpanPos == 0 ? SpanCnt2 : SpanCnt1;
        }
    }

    #endregion SpanSize

    public class TabExploreAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}