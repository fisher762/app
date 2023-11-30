using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;

namespace PixelPhoto.Activities.SettingsUser.Adapters
{
    public class SectionItem
    {
        public int Id { get; set; }
        public string SectionName { get; set; }
        public string Icon { get; set; }
        public Color IconColor { get; set; }
        public int BadgeCount { get; set; }
        public bool BadgeVisibility { get; set; }
    }

    public class MoreSectionAdapter : RecyclerView.Adapter
    {
        private readonly Activity ActivityContext;
        public ObservableCollection<SectionItem> SectionList = new ObservableCollection<SectionItem>();

        public MoreSectionAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                SetItem();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => SectionList?.Count ?? 0;

        public event EventHandler<MoreSectionAdapterClickEventArgs> ItemClick;
        public event EventHandler<MoreSectionAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> ChannelSubscribed_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MoreSection_view, parent, false);
                var vh = new MoreSectionAdapterViewHolder(itemView, OnClick, OnLongClick);
                return vh;
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
                if (viewHolder is MoreSectionAdapterViewHolder holder)
                { 
                    var item = SectionList[position];
                    if (item != null)
                    {
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.Icon, item.Icon);
                        holder.Icon.SetTextColor(item.IconColor);
                        holder.Name.Text = item.SectionName; 
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public SectionItem GetItem(int position)
        {
            return SectionList[position];
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
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void OnClick(MoreSectionAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(MoreSectionAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
         
        public void SetItem()
        {
            try
            {
                if (AppSettings.ShowSettingsGeneralAccount)
                    SectionList.Add(new SectionItem
                    {
                        Id = 1,
                        SectionName = ActivityContext.GetText(Resource.String.Lbl_General),
                        BadgeCount = 0,
                        BadgeVisibility = false,
                        Icon = IonIconsFonts.Settings,
                        IconColor = Color.ParseColor("#4CAF50")
                    }); 
                   
                SectionList.Add(new SectionItem
                    {
                        Id = 2,
                        SectionName = ActivityContext.GetText(Resource.String.Lbl_Profile),
                        BadgeCount = 0,
                        BadgeVisibility = false,
                        Icon = IonIconsFonts.Contact,
                        IconColor = Color.ParseColor("#c83e40")
                    });
                if (AppSettings.ShowSettingsPassword)
                    SectionList.Add(new SectionItem
                    {
                        Id = 3,
                        SectionName = ActivityContext.GetText(Resource.String.Lbl_Change_Password),
                        BadgeCount = 0,
                        BadgeVisibility = false,
                        Icon = IonIconsFonts.Key,
                        IconColor = Color.ParseColor("#176764")
                    });
                if (AppSettings.ShowSettingsAccountPrivacy)
                    SectionList.Add(new SectionItem
                    {
                        Id = 4,
                        SectionName = ActivityContext.GetText(Resource.String.Lbl_AccountPrivacy),
                        BadgeCount = 0,
                        BadgeVisibility = false,
                        Icon = IonIconsFonts.Eye,
                        IconColor = Color.ParseColor("#ca4b8e")
                    });
                if (AppSettings.ShowSettingsNotifications)
                    SectionList.Add(new SectionItem
                    {
                        Id = 5,
                        SectionName = ActivityContext.GetText(Resource.String.Lbl_Notifications),
                        BadgeCount = 0,
                        BadgeVisibility = false,
                        Icon = IonIconsFonts.Notifications,
                        IconColor = Color.ParseColor("#795548")
                    });
                if (AppSettings.ShowSettingsManageSessions)
                    SectionList.Add(new SectionItem
                    {
                        Id = 6,
                        SectionName = ActivityContext.GetText(Resource.String.Lbl_ManageSessions), 
                        BadgeCount = 0,
                        BadgeVisibility = false,
                        Icon = IonIconsFonts.FingerPrint,
                        IconColor = Color.ParseColor("#f57d3c")
                    });
                if (AppSettings.ShowSettingsBusinessAccount)
                    SectionList.Add(new SectionItem
                    {
                        Id = 7,
                        SectionName = ActivityContext.GetText(Resource.String.Lbl_BusinessAccount), 
                        BadgeCount = 0,
                        BadgeVisibility = false,
                        Icon = IonIconsFonts.Briefcase,
                        IconColor = Color.ParseColor("#00b289")
                    });
                if (AppSettings.ShowSettingsVerification)
                    SectionList.Add(new SectionItem
                    {
                        Id = 8,
                        SectionName = ActivityContext.GetText(Resource.String.Lbl_Verification), 
                        BadgeCount = 0,
                        BadgeVisibility = false,
                        Icon = IonIconsFonts.CheckmarkCircle,
                        IconColor = Color.ParseColor("#55acee")
                    });
                if (AppSettings.ShowSettingsBlockedUsers)
                    SectionList.Add(new SectionItem
                    {
                        Id = 9,
                        SectionName = ActivityContext.GetText(Resource.String.Lbl_Blocked_Users),
                        BadgeCount = 0,
                        BadgeVisibility = false,
                        Icon = IonIconsFonts.RemoveCircle,
                        IconColor = Color.ParseColor("#3f51b5")
                    });
                if (AppSettings.ShowSettingsWithdrawals)
                    SectionList.Add(new SectionItem
                    {
                        Id = 10,
                        SectionName = ActivityContext.GetText(Resource.String.Lbl_Withdrawals),
                        BadgeCount = 0,
                        BadgeVisibility = false,
                        Icon = IonIconsFonts.LogoUsd,
                        IconColor = Color.ParseColor("#415966")
                    });
                if (AppSettings.ShowSettingsMyAffiliates)
                    SectionList.Add(new SectionItem
                    {
                        Id = 11,
                        SectionName = ActivityContext.GetText(Resource.String.Lbl_MyAffiliates),
                        BadgeCount = 0,
                        BadgeVisibility = false,
                        Icon = IonIconsFonts.IosPeople,
                        IconColor = Color.ParseColor("#555d3c")
                    });
                if (AppSettings.ShowSettingsDeleteAccount)
                    SectionList.Add(new SectionItem
                    {
                        Id = 12,
                        SectionName = ActivityContext.GetText(Resource.String.Lbl_DeleteAccount),
                        BadgeCount = 0,
                        BadgeVisibility = false,
                        Icon = IonIconsFonts.IosTrash,
                        IconColor = Color.ParseColor("#f44336")
                    });
                SectionList.Add(new SectionItem
                {
                    Id = 13,
                    SectionName = ActivityContext.GetText(Resource.String.Lbl_Theme),
                    BadgeCount = 0,
                    BadgeVisibility = false,
                    Icon = IonIconsFonts.IosMoon,
                    IconColor = Color.ParseColor("#888888")
                });
                SectionList.Add(new SectionItem
                {
                    Id = 14,
                    SectionName = ActivityContext.GetText(Resource.String.Lbl_Logout),
                    BadgeCount = 0,
                    BadgeVisibility = false,
                    Icon = IonIconsFonts.LogOut,
                    IconColor = Color.ParseColor("#d50000")
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    } 

    public class MoreSectionAdapterViewHolder : RecyclerView.ViewHolder
    {
        public MoreSectionAdapterViewHolder(View itemView, Action<MoreSectionAdapterClickEventArgs> clickListener,Action<MoreSectionAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                LinearLayoutMain = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                LinearLayoutImage = itemView.FindViewById<RelativeLayout>(Resource.Id.imagecontainer);

                Icon = itemView.FindViewById<TextView>(Resource.Id.Icon);
                Name = itemView.FindViewById<TextView>(Resource.Id.section_name);

                itemView.Click += (sender, e) => clickListener(new MoreSectionAdapterClickEventArgs
                    {View = itemView, Position = AdapterPosition});
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public View MainView { get; }

        public LinearLayout LinearLayoutMain { get; private set; }
        public RelativeLayout LinearLayoutImage { get; private set; }
        public TextView Icon { get; private set; }
        public TextView Name { get; private set; }

      
    }

    public class MoreSectionAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}