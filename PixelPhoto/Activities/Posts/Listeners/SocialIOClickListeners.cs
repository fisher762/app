using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using PixelPhoto.Activities.Tabbes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.AddPost;
using PixelPhoto.Activities.Comment;
using PixelPhoto.Activities.Posts.Adapters;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Com.Airbnb.Lottie; 
using PixelPhoto.Helpers.Model;
using PixelPhotoClient;
using PixelPhoto.Library.Anjo.Share;
using PixelPhoto.Library.Anjo.Share.Abstractions;
using Com.Google.Android.Youtube.Player;
using PixelPhoto.Activities.Posts.page;
using Exception = System.Exception;

namespace PixelPhoto.Activities.Posts.Listeners
{
    public interface IOnPostItemClickListener
    {
        void OnLikeNewsFeedClick(LikeNewsFeedClickEventArgs e, int position);
        void OnFavNewsFeedClick(FavNewsFeedClickEventArgs e,int position);

        void OnShareClick(GlobalClickEventArgs args, int position);

        void OnCommentClick(GlobalClickEventArgs e, int position);
       
        void OnAvatarImageFeedClick(AvatarFeedClickEventArgs e, int position);

        void OnLikedLabelPostClick(LikeNewsFeedClickEventArgs e, int position);

        void OnCommentPostClick(GlobalClickEventArgs e, int position);

        void OnMoreClick(GlobalClickEventArgs args, bool isShow, int position);
        void ImagePostClick(GlobalClickEventArgs args, int position, int positionItem);
    }
    public class SocialIoClickListeners: Java.Lang.Object , MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        private readonly Activity MainContext;
        private string TypeDialog, NamePage;
        private GlobalClickEventArgs MoreFeedArgs;
         
        public SocialIoClickListeners(Activity context)
        {
            MainContext = context;
            TypeDialog = string.Empty;
        }

        //Add Like Or Remove 
        public async void OnLikeNewsFeedClick(LikeNewsFeedClickEventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (UserDetails.SoundControl)
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("reaction.mp3");

                    bool refs;

                    if (e.LikeImgButton.Tag?.ToString() == "Liked")
                    { 
                        e.LikeAnimationView.Progress=0;
                        e.LikeAnimationView.CancelAnimation();
                        e.LikeImgButton.SetImageResource(Resource.Drawable.icon_heart_post_vector);
                        e.LikeImgButton.Tag = "Like";
                        refs = false;
                    }
                    else
                    {
                        e.LikeAnimationView.PlayAnimation();
                        e.LikeImgButton.SetImageResource(Resource.Drawable.icon_heart_filled_post_vector);
                        e.LikeImgButton.Tag = "Liked";
                        refs = true;
                    }

                    e.NewsFeedClass.IsLiked = refs;

                    var likeCount = e.View.FindViewById<TextView>(Resource.Id.Likecount);
                    if (likeCount != null)
                    {
                        var likes = MainContext.GetText(Resource.String.Lbl_Likes);
                        long count = 0;
                        if (!refs && e.NewsFeedClass.Likes == 0)
                        {
                            e.NewsFeedClass.Likes = 0;
                            likeCount.Text = "0" + " " + likes;
                        }
                        else if (!refs && e.NewsFeedClass.Likes > 0)
                        {
                            count = e.NewsFeedClass.Likes - 1;
                            likeCount.Text = count + " " + likes;
                            e.NewsFeedClass.Likes = count;
                        }
                        else if (refs)
                        {
                            count = e.NewsFeedClass.Likes + 1;
                            likeCount.Text = count + " " + likes;
                            e.NewsFeedClass.Likes = count;
                        }

                        var list = HomeActivity.GetInstance()?.NewsFeedFragment?.NewsFeedAdapter?.PostList;
                        var dataPost = list?.FirstOrDefault(a => a.PostId == e.NewsFeedClass.PostId);
                        if (dataPost != null)
                        {
                            dataPost.Likes = count;
                            dataPost.IsLiked = refs;
                            //int index = list.IndexOf(dataPost);
                            //HomeActivity.GetInstance().NewsFeedFragment.NewsFeedAdapter.NotifyItemChanged(index,"like");
                        }
                    }

                    //Sent Api
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Post.AddLikeOrRemove(e.NewsFeedClass.PostId.ToString()) });

                    var tapLikeAnimation = e.View.FindViewById<LottieAnimationView>(Resource.Id.animation_like);
                    if (tapLikeAnimation != null)
                    {
                        await Task.Delay(1000);
                        tapLikeAnimation.Progress = 0;
                        tapLikeAnimation.CancelAnimation();
                    }
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Add Remove From Favorite
        public void OnFavNewsFeedClick(FavNewsFeedClickEventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {

                    bool refs;

                    if (e.FavImgButton.Tag?.ToString() == "Added")
                    {
                        e.FavAnimationView.Progress = 0;
                        e.FavAnimationView.CancelAnimation();
                        e.FavImgButton.SetImageResource(Resource.Drawable.icon_fav_post_vector);
                        e.FavImgButton.Tag = "Add";
                        refs = false;
                    }
                    else
                    {
                        e.FavAnimationView.PlayAnimation();
                        e.FavImgButton.SetImageResource(Resource.Drawable.icon_star_filled_post_vector);
                        e.FavImgButton.Tag = "Added";
                        refs = true;
                       
                    }
                    
                    e.NewsFeedClass.IsSaved = refs;
                      
                    switch (AppSettings.ProfileTheme)
                    {
                        case ProfileTheme.DefaultTheme:
                        {
                            var textFav = HomeActivity.GetInstance()?.ProfileFragment?.TxtCountFav?.Text;
                            if (textFav != null)
                            {
                                if (!textFav.Contains("K") || !textFav.Contains("M"))
                                {
                                    var count = Convert.ToInt32(textFav);
                                    if (!refs)
                                    {
                                        if (count > 0)
                                            count--;
                                        else
                                            count = 0;
                                    }
                                    else
                                        count++;

                                    var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                                    if (dataUser != null)
                                    {
                                        dataUser.Favourites = count.ToString();
                                        HomeActivity.GetInstance().ProfileFragment.TxtCountFav.Text = count.ToString();
                                    }
                                }
                            }

                            break;
                        }
                        case ProfileTheme.TikTheme:
                        {
                            var textFav = HomeActivity.GetInstance()?.TikProfileFragment?.TxtPostCount?.Text;
                            if (textFav != null)
                            {
                                if (!textFav.Contains("K") || !textFav.Contains("M"))
                                {
                                    var count = Convert.ToInt32(textFav);
                                    if (!refs)
                                    {
                                        if (count > 0)
                                            count--;
                                        else
                                            count = 0;
                                    }
                                    else
                                        count++;

                                    var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                                    if (dataUser != null)
                                    {
                                        dataUser.Favourites = count.ToString();
                                        HomeActivity.GetInstance().TikProfileFragment.TxtPostCount.Text = count.ToString();
                                    }
                                }
                            }

                            break;
                        }
                    }
                     
                    var list = ((HomeActivity) MainContext)?.NewsFeedFragment?.NewsFeedAdapter?.PostList;
                    var dataPost = list?.FirstOrDefault(a => a.PostId == e.NewsFeedClass.PostId);
                    if (dataPost != null)
                    { 
                        dataPost.IsSaved = refs;
                        //int index = list.IndexOf(dataPost);
                        //HomeActivity.GetInstance().NewsFeedFragment.NewsFeedAdapter.NotifyItemChanged(index, "favorite");
                    }

                    //Sent Api
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Post.AddRemoveFromFavorite(e.NewsFeedClass.PostId.ToString()) });
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Share
        public async void OnShareClick(GlobalClickEventArgs args)
        {
            try
            {
                if (!CrossShare.IsSupported)
                    return;
                 
                if (AppSettings.AllowDownloadMedia && (args.NewsFeedClass.Type == "image" || args.NewsFeedClass.Type == "video" || args.NewsFeedClass.Type == "gif"))
                {
                    if (args.NewsFeedClass?.MediaSet.Count >= 1)
                    { 
                        var urlImage = args.NewsFeedClass.MediaSet[0].File;
                        ShareFileImplementation.Activity = MainContext;
                        await ShareFileImplementation.ShareRemoteFile(urlImage, urlImage.Split('/').Last(), Client.WebsiteUrl + "/post/" + args.NewsFeedClass.PostId, MainContext.GetText(Resource.String.Lbl_Send_to)); 
                    } 
                } 
                else
                {
                    //string urlImage = args.NewsFeedClass.MediaSet[0].File;

                    await CrossShare.Current.Share(new ShareMessage
                    {
                        Title = "",
                        Text = !string.IsNullOrEmpty(args.NewsFeedClass.Description) ? args.NewsFeedClass.Description : AppSettings.ApplicationName,
                        Url = Client.WebsiteUrl + "/post/" + args.NewsFeedClass.PostId
                    }); 
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Open Comment Fragment
        public void OnCommentClick(GlobalClickEventArgs e, string nameFragment)
        {
            try
            {
                HomeActivity.GetInstance()?.OpenCommentFragment(new ObservableCollection<CommentObject>(e.NewsFeedClass.Comments), e.NewsFeedClass.PostId.ToString(), nameFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Profile user 
        public void OnAvatarImageFeedClick(AvatarFeedClickEventArgs e)
        {
            try
            {
                AppTools.OpenProfile(HomeActivity.GetInstance(), e.NewsFeedClass.UserData.UserId, e.NewsFeedClass.UserData);  
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open liked Post user
        public void OnLikedPostClick(LikeNewsFeedClickEventArgs e)
        {
            try
            {
                if (e.NewsFeedClass.Likes > 0)
                {
                    var bundle = new Bundle();
                    bundle.PutString("userinfo", JsonConvert.SerializeObject(e.NewsFeedClass));
                    bundle.PutString("PostId", e.NewsFeedClass.PostId.ToString());

                    var fragment = new LikesPostFragment
                    {
                        Arguments = bundle
                    };

                    HomeActivity.GetInstance()?.OpenFragment(fragment);
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_No_likes_yet), ToastLength.Short)?.Show();
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Comment Post user
        public void OnCommentPostClick(GlobalClickEventArgs e , string namePage)
        {
            try
            {
                if (e.NewsFeedClass.Comments.Count > 0)
                {
                    HomeActivity.GetInstance()?.OpenCommentFragment(new ObservableCollection<CommentObject>(e.NewsFeedClass.Comments), e.NewsFeedClass.PostId.ToString(), namePage);
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_NoComments_TitleText), ToastLength.Short)?.Show();
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void ImagePostClick(GlobalClickEventArgs e, int positionItem)
        {
            try
            {
                var bundle = new Bundle();
                bundle.PutString("postInfo", JsonConvert.SerializeObject(e.NewsFeedClass));
                bundle.PutString("PostId", e.NewsFeedClass.PostId.ToString());
                bundle.PutString("indexImage", positionItem.ToString());

                var fragment = new MultiImagesPostViewerFragment
                {
                    Arguments = bundle
                };

                HomeActivity.GetInstance()?.OpenFragment(fragment); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Show More :  DeletePost , EditPost , GoPost , Copy Link  , Report .. 
        public void OnMoreClick(GlobalClickEventArgs args, bool isShow = false, string namePage = "")
        {
            try
            {
                var dataUser = ListUtils.MyProfileList.FirstOrDefault();

                NamePage = namePage; 
                MoreFeedArgs = args;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                if (args.NewsFeedClass?.IsOwner != null && args.NewsFeedClass.IsOwner.Value)
                {
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_DeletePost));
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_EditPost));

                    switch (args.NewsFeedClass?.Boosted)
                    {
                        case 0 when dataUser?.IsPro == "1":
                            arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_BoostPost));
                            break;
                        case 1 when dataUser?.IsPro == "1":
                            arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_UnBoostPost));
                            break;
                    }
                }

                if (isShow)
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_GoToPost));

                arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_ReportThisPost));
                arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_Copy));

                dialogList.Title(MainContext.GetText(Resource.String.Lbl_Post));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(MainContext.GetText(Resource.String.Lbl_Close)).OnPositive(new MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                var text = itemString.ToString();
                if (text == MainContext.GetText(Resource.String.Lbl_DeletePost))
                {
                    OnMenuDeletePostOnClick(MoreFeedArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_EditPost))
                {
                    OnMenuEditPostOnClick(MoreFeedArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_GoToPost))
                {
                    OnMenuGoPostOnClick(MoreFeedArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_ReportThisPost))
                {
                    OnMenuReportPostOnClick(MoreFeedArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_Copy))
                {
                    OnMenuCopyOnClick(MoreFeedArgs.NewsFeedClass);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_BoostPost) || text == MainContext.GetString(Resource.String.Lbl_UnBoostPost))
                {
                    BoostPostEvent(MoreFeedArgs.NewsFeedClass);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (TypeDialog == "DeletePost" )
                {
                    var home = (HomeActivity)MainContext ?? HomeActivity.GetInstance();  
                    if (p1 == DialogAction.Positive)
                    {
                        home?.RunOnUiThread(() =>
                        {
                            try
                            {
                                var list = home.NewsFeedFragment?.NewsFeedAdapter?.PostList;
                                var dataPost = list?.FirstOrDefault(a => a.PostId == MoreFeedArgs.NewsFeedClass.PostId);
                                if (dataPost != null)
                                {
                                    var index = list.IndexOf(dataPost);
                                    if (index >= 0)
                                    {
                                        home.NewsFeedFragment.NewsFeedAdapter?.PostList?.Remove(dataPost);
                                        home.NewsFeedFragment.NewsFeedAdapter?.NotifyItemRemoved(index);
                                    }
                                }

                                if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                                {
                                    if (home.ProfileFragment != null)
                                    {
                                        var dataPostUser = home.ProfileFragment?.NewsFeedStyle switch
                                        {
                                            "Linear" => home.ProfileFragment?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == MoreFeedArgs.NewsFeedClass.PostId),
                                            "Grid" => home.ProfileFragment?.UserPostAdapter?.PostList?.FirstOrDefault(a => a.PostId == MoreFeedArgs.NewsFeedClass.PostId),
                                            _ => null
                                        };

                                        if (dataPostUser != null)
                                        {
                                            dynamic adapter = home.ProfileFragment?.NewsFeedStyle switch
                                            {
                                                "Linear" => home.ProfileFragment?.MAdapter,
                                                "Grid" => home.ProfileFragment?.UserPostAdapter,
                                                _ => home.ProfileFragment?.UserPostAdapter
                                            };
                                            if (adapter != null)
                                            {
                                                int index = adapter.PostList.IndexOf(dataPostUser);
                                                if (index >= 0)
                                                {
                                                    adapter.PostList.Remove(dataPostUser); 
                                                    adapter.NotifyItemRemoved(index);
                                                }
                                            } 
                                        }
                                    } 
                                }
                                else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                                {
                                    var dataPostProfile = home.TikProfileFragment?.MyPostTab?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == MoreFeedArgs.NewsFeedClass.PostId);
                                    if (dataPostProfile != null)
                                    {
                                        var index = home.TikProfileFragment.MyPostTab.MAdapter.PostList.IndexOf(dataPostProfile);
                                        if (index >= 0)
                                        {
                                            home.TikProfileFragment?.MyPostTab?.MAdapter?.PostList.Remove(dataPostProfile);
                                            home.TikProfileFragment?.MyPostTab?.MAdapter?.NotifyItemRemoved(index);
                                        }
                                    }
                                }

                                //Delete post from list
                                if (NamePage == "HashTags")
                                {
                                    var currentFragment = home.FragmentBottomNavigator.GetSelectedTabBackStackFragment(); 
                                    if (currentFragment is HashTagPostFragment frm)
                                    {
                                        var listHash = frm.MAdapter.PostList;
                                        var dataPostHash = listHash?.FirstOrDefault(a => a.PostId == MoreFeedArgs.NewsFeedClass.PostId);
                                        if (dataPostHash != null)
                                        {
                                            var index = listHash.IndexOf(dataPostHash);

                                            listHash.Remove(dataPostHash);

                                            if (index >= 0)
                                                frm.MAdapter.NotifyItemRemoved(index);
                                        }
                                    } 
                                }
                                else if (NamePage == "GifPost" || NamePage == "ImagePost" || NamePage == "MultiImagePost" || NamePage == "VideoPost" || NamePage == "YoutubePost")
                                {
                                    home.FragmentNavigatorBack();
                                    home.NewsFeedFragment.NewsFeedAdapter?.NotifyDataSetChanged();
                                }
                                else if (NamePage == "NewsFeedPost")
                                {

                                }

                                //SqLiteDatabase dbDatabase = new SqLiteDatabase(); 
                                //dbDatabase.RemoveOneNewsFeedPost(MoreFeedArgs.NewsFeedClass.PostId);
                                //db

                                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short)?.Show();

                                //Sent Api >>
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Post.DeletePosts(MoreFeedArgs.NewsFeedClass.PostId.ToString()) });
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e); 
                            }
                        }); 
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else
                {
                    if (p1 == DialogAction.Positive)
                    {
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //DeletePost
        public void OnMenuDeletePostOnClick(GlobalClickEventArgs feed)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "DeletePost";
                    MoreFeedArgs = feed;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                    dialog.Title(MainContext.GetText(Resource.String.Lbl_DeletePost));
                    dialog.Content(MainContext.GetText(Resource.String.Lbl_AreYouSureDeletePost));
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show(); 
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Edit Post
        public void OnMenuEditPostOnClick(GlobalClickEventArgs feed)
        {
            try
            { 
                var intent = new Intent(MainContext, typeof(EditPostActivity));
                intent.PutExtra("IdPost", feed.NewsFeedClass.PostId.ToString());
                intent.PutExtra("TextPost", feed.NewsFeedClass.Description);
                MainContext.StartActivityForResult(intent,2250);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //ReportPost
        public void OnMenuReportPostOnClick(GlobalClickEventArgs feed)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //Sent Api >>
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Post.ReportPosts(feed.NewsFeedClass.PostId.ToString()) });

                    Toast.MakeText(MainContext,MainContext.GetText(Resource.String.Lbl_YourReportPost), ToastLength.Short)?.Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Go to Post
        public void OnMenuGoPostOnClick(GlobalClickEventArgs feed)
        {
            try
            { 
                HomeActivity.GetInstance()?.OpenNewsFeedItem(feed.NewsFeedClass.PostId.ToString() ,feed.NewsFeedClass);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Copy
        public void OnMenuCopyOnClick(PostsObject feed)
        {
            try
            {
                Methods.CopyToClipboard(MainContext, Client.WebsiteUrl + "/post/" + feed.PostId); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //BoostPost 
        private async void BoostPostEvent(PostsObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    item.Boosted = 1;
                    //Sent Api >>
                    var (apiStatus, respond) = await RequestsAsync.Post.BoostPost(item.PostId.ToString());
                    if (apiStatus == 200)
                    {
                        if (respond is AddBoostPostObject actionsObject)
                        {
                            item.Boosted = actionsObject.Type;
                            Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_postSuccessfullyBoosted), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPlayYoutubeButtonClicked(YoutubeVideoClickEventArgs feed)
        {
            try
            {
                if (AppSettings.StartYoutubeAsIntent)
                {
                    var intent = YouTubeStandalonePlayer.CreateVideoIntent(MainContext, MainContext.GetText(Resource.String.google_key), feed.NewsFeedClass.Youtube);  
                    MainContext.StartActivity(intent);
                }
                else
                {
                    HomeActivity.GetInstance()?.OpenNewsFeedItem(feed.NewsFeedClass.PostId.ToString() , feed.NewsFeedClass);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void CommentReplyPostClick(CommentReplyClickEventArgs e)
        {
            try
            {
                var bundle = new Bundle();
                bundle.PutString("CommentId", e.CommentObject.Id.ToString());
                bundle.PutString("CommentObject", JsonConvert.SerializeObject(e.CommentObject));
                var mFragment = new ReplyCommentFragment
                {
                    Arguments = bundle
                };

                HomeActivity.GetInstance().OpenFragment(mFragment); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public bool SetLike(ImageView likeImgButton)
        {
            try
            {
                if (likeImgButton.Tag?.ToString() == "Liked")
                {

                    likeImgButton.SetImageResource(Resource.Drawable.icon_heart_post_vector);
                    likeImgButton.Tag = "Like";

                    return false;
                }
                else
                {
                    likeImgButton.SetImageResource(Resource.Drawable.icon_heart_filled_post_vector);
                    likeImgButton.Tag = "Liked";
                    return true;
                }

               
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        public bool SetFav(ImageView favButton)
        {
            try
            {
                if (favButton.Tag?.ToString() == "Added")
                {
                  
                    favButton.SetImageResource(Resource.Drawable.icon_fav_post_vector);
                    favButton.Tag = "Add";
                    return false;
                }
                else
                {
                    favButton.SetImageResource(Resource.Drawable.icon_star_filled_post_vector);
                    favButton.Tag = "Added";
                    return true;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        } 
    }
     
    public class GlobalClickEventArgs
    {
        public PostsObject NewsFeedClass { get; set; }
    }
     
    public class YoutubeVideoClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public PostsObject NewsFeedClass { get; set; }
        public Holders.YoutubeAdapterViewHolder Holder { get; set; }
    }

    public class AvatarFeedClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public PostsObject NewsFeedClass { get; set; }
        public ImageView Image { get; set; }
    }

    public class FavNewsFeedClickEventArgs : EventArgs
    {
        public PostsObject NewsFeedClass { get; set; }  
        public ImageView FavImgButton { get; set; }

        public LottieAnimationView FavAnimationView { get; set; }
    }
     
    public class CommentReplyClickEventArgs
    {
        public View View { get; set; }
        public  int Position { get; set; }
        public CommentObject CommentObject { get; set; }
    }

    public class LikeNewsFeedClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public PostsObject NewsFeedClass { get; set; }
        public TextView LikeButton { get; set; }
        public ImageView LikeImgButton { get; set; }

        public LottieAnimationView LikeAnimationView { get; set; }
    }
     
}