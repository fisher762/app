using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.MyContacts.Adapters
{
    public class ContactsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public enum TypeTextSecondary
        {
            About,
            LastSeen,
            None
        }
        public event EventHandler<ContactsAdapterClickEventArgs> ItemClick;
        public event EventHandler<ContactsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> UserList = new ObservableCollection<UserDataObject>();
        private readonly bool ShowButton;
        private readonly TypeTextSecondary Type;

        public ContactsAdapter(Activity activity , bool showButton , TypeTextSecondary type)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = activity;
                ShowButton = showButton;
                Type = type;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => UserList?.Count ?? 0;

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                var users = UserList[position];
                if (payloads.Count > 0)
                { 
                    if (viewHolder is ContactsAdapterViewHolder holder)
                    { 
                        var data = (string)payloads[0];
                        if (data == "true")
                        {
                            holder.Button.SetBackgroundResource(Resource.Xml.background_signup2);
                            holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                            holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Following);
                            holder.Button.Tag = "true";
                            users.IsFollowing = true;
                        }
                        else
                        {
                            holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                            holder.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Follow);
                            holder.Button.Tag = "false";
                            users.IsFollowing = false; 
                        }
                    } 
                }
                else
                {
                    base.OnBindViewHolder(viewHolder, position, payloads);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }


        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HContact_view
                var itemView = LayoutInflater.From(parent.Context)
                    .Inflate(Resource.Layout.Style_HContact_view, parent, false);
                var vh = new ContactsAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is ContactsAdapterViewHolder holder)
                {
                    var item = UserList[position];
                    if (item != null)
                    {
                        Initialize(holder, item);

                        if (ShowButton)
                        {
                            if (!holder.Button.HasOnClickListeners)
                                holder.Button.Click += (sender, e) => FollowButtonClick(new FollowFollowingClickEventArgs { View = holder.ItemView, UserClass = item, Position = position, ButtonFollow = holder.Button });
                        }
                        else
                        {
                            holder.Button.Visibility = ViewStates.Gone;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Initialize(ContactsAdapterViewHolder holder, UserDataObject users)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, users.Avatar, holder.Image, ImageStyle.CircleCrop,ImagePlaceholders.Drawable);

                holder.Name.Text = Methods.FunString.SubStringCutOf(AppTools.GetNameFinal(users), 25);

                if (Type == TypeTextSecondary.None)
                {
                    holder.About.Visibility = ViewStates.Gone;
                }
                else
                {
                    holder.About.Text = Type == TypeTextSecondary.About ? Methods.FunString.SubStringCutOf(AppTools.GetAboutFinal(users), 25) : ActivityContext.GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(users.LastSeen), false);
                }

                if (ShowButton)
                {
                    switch (users.IsFollowing)
                    {
                        // My Friend
                        case true:
                        {
                            holder.Button.SetBackgroundResource(Resource.Xml.background_signup2);
                            holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                            holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Following);
                            holder.Button.Tag = "true";
                            break;
                        }  
                        case false:
                        {
                            holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                            holder.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Follow);
                            holder.Button.Tag = "false"; 
                            break;
                        }  
                    }
                     
                    if (users.UserId == UserDetails.UserId)
                        holder.Button.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void FollowButtonClick(FollowFollowingClickEventArgs e)
        {
            try
            {
                if (e.UserClass != null)
                {
                    NotifyItemChanged(e.Position, e.ButtonFollow.Tag?.ToString() == "false" ? "true" : "false");

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollow(e.UserClass.UserId) });
                    else
                        Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            } 
        }

        public UserDataObject GetItem(int position)
        {
            return UserList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return Int64.Parse(UserList[position].UserId);
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
         
        private void Click(ContactsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(ContactsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
         
        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserList[p0];
                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Avatar != "")
                {
                    d.Add(item.Avatar);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        } 
    }

    public class ContactsAdapterViewHolder : RecyclerView.ViewHolder
    {
        public ContactsAdapterViewHolder(View itemView, Action<ContactsAdapterClickEventArgs> clickListener,
            Action<ContactsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = itemView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = itemView.FindViewById<TextView>(Resource.Id.card_name);
                About = itemView.FindViewById<TextView>(Resource.Id.card_dist);
                Button = itemView.FindViewById<Button>(Resource.Id.cont);

                //Event
                itemView.Click += (sender, e) => clickListener(new ContactsAdapterClickEventArgs {View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new ContactsAdapterClickEventArgs {View = itemView, Position = AdapterPosition});
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        private View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView About { get; private set; }
        public Button Button { get; private set; }

        #endregion
    }

    public class ContactsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
     
    public class FollowFollowingClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public UserDataObject UserClass { get; set; }
        public Button ButtonFollow { get; set; }
    }
}