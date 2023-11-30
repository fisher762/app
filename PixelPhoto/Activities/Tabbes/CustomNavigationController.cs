using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using PixelPhoto.Activities.Tabbes.Fragments;
using PixelPhoto.Activities.TikProfile;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Utils;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;

namespace PixelPhoto.Activities.Tabbes
{
    public class CustomNavigationController : Java.Lang.Object, View.IOnClickListener
    {
        private readonly Activity MainContext;

        private FrameLayout NotificationButton;
        private LinearLayout HomeButton, ProfileButton, DiscoverButton;
        private ImageView HomeImage, NotificationImage, DiscoverImage;
        public ImageView ProfileImage;
        private static int PageNumber;

        public readonly List<Fragment> FragmentListTab0 = new List<Fragment>();
        public readonly List<Fragment> FragmentListTab1 = new List<Fragment>();
        //public readonly List<Fragment> FragmentListTab2 = new List<Fragment>();
        public readonly List<Fragment> FragmentListTab3 = new List<Fragment>();
        public readonly List<Fragment> FragmentListTab4 = new List<Fragment>();

        private readonly HomeActivity Context;
        private LinearLayout MainLayout;
        

        public CustomNavigationController(Activity activity)
        {
            try
            {
                MainContext = activity;

                if (activity is HomeActivity cont)
                    Context = cont;

                Initialize();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Initialize()
        {
            try
            {
                MainLayout = MainContext.FindViewById<LinearLayout>(Resource.Id.llMain);
                HomeButton = MainLayout.FindViewById<LinearLayout>(Resource.Id.llHome);
                NotificationButton = MainLayout.FindViewById<FrameLayout>(Resource.Id.llNotification);
                ProfileButton = MainLayout.FindViewById<LinearLayout>(Resource.Id.llProfile);
                DiscoverButton = MainLayout.FindViewById<LinearLayout>(Resource.Id.llDiscover);

                HomeImage = MainLayout.FindViewById<ImageView>(Resource.Id.ivHome);
                NotificationImage = MainLayout.FindViewById<ImageView>(Resource.Id.ivNotification);
                ProfileImage = MainLayout.FindViewById<ImageView>(Resource.Id.ivProfile);
                DiscoverImage = MainLayout.FindViewById<ImageView>(Resource.Id.ivDiscover);

                HomeButton.SetOnClickListener(this);
                DiscoverButton.SetOnClickListener(this);
                NotificationButton.SetOnClickListener(this);
                ProfileButton.SetOnClickListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                if (Context.CircleMenu.IsOpened)
                    return;

                switch (v.Id)
                {
                    case Resource.Id.llHome:
                        EnableNavigationButton(HomeImage);
                        PageNumber = 0;
                        ShowFragment0();
                        AdsGoogle.Ad_AppOpenManager(MainContext);
                        break;
                    case Resource.Id.llDiscover:
                        EnableNavigationButton(DiscoverImage);
                        PageNumber = 1;
                        ShowFragment1();
                        AdsGoogle.Ad_Interstitial(MainContext);
                        break;
                    case Resource.Id.llNotification:
                        EnableNavigationButton(NotificationImage);
                        PageNumber = 3;
                        ShowFragment3();
                        ShowNotificationBadge(false);
                        AdsGoogle.Ad_RewardedVideo(MainContext);
                        break;
                    case Resource.Id.llProfile:
                        OpenProfileTab();
                        AdsGoogle.Ad_RewardedInterstitial(MainContext);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenProfileTab()
        {
            try
            {
                switch (AppSettings.ProfileTheme)
                {
                    case ProfileTheme.DefaultTheme:
                        Context.ProfileFragment ??= new ProfileFragment();
                        if (FragmentListTab4.LastOrDefault() != Context.ProfileFragment)
                            FragmentListTab4.Add(Context.ProfileFragment);
                        break;
                    case ProfileTheme.TikTheme:
                        Context.TikProfileFragment ??= new TikProfileFragment();
                        if (FragmentListTab4.LastOrDefault() != Context.TikProfileFragment)
                            FragmentListTab4.Add(Context.TikProfileFragment);
                        break;
                }

                EnableNavigationButton(ProfileImage);
                PageNumber = 4;
                ShowFragment4();
                AdsGoogle.Ad_RewardedVideo(MainContext);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void EnableNavigationButton(ImageView image)
        {
            try
            {
                DisableAllNavigationButton();
                image.Background = MainContext.GetDrawable(Resource.Drawable.shape_bg_bottom_navigation);

                if (image.Id == ProfileImage.Id)
                    return;

                image.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisableAllNavigationButton()
        {
            try
            {
                HomeImage.Background = null!;
                HomeImage.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                NotificationImage.Background = null!;
                NotificationImage.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                ProfileImage.Background = null!;
                //ProfileImage.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                DiscoverImage.Background = null!;
                DiscoverImage.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowNotificationBadge(bool showBadge)
        {
            try
            {
                var animationView2 = MainLayout.FindViewById<LottieAnimationView>(Resource.Id.animation_view2);

                if (showBadge)
                {
                    NotificationImage.SetImageDrawable(null);
                     
                    animationView2.SetAnimation("NotificationLotti.json");
                    animationView2.PlayAnimation();
                }
                else
                {
                    animationView2.Progress = 0;
                    animationView2.CancelAnimation();
                    NotificationImage.SetImageResource(Resource.Drawable.icon_notification_vector);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public Fragment GetSelectedTabBackStackFragment()
        {
            try
            {
                switch (PageNumber)
                {
                    case 0:
                    {
                        var currentFragment = FragmentListTab0[FragmentListTab0.Count - 2];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }
                    case 1:
                    {
                        var currentFragment = FragmentListTab1[FragmentListTab1.Count - 2];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }
                    //case 2:
                    //    {
                    //        var currentFragment = FragmentListTab2[FragmentListTab2.Count - 2];
                    //        if (currentFragment != null)
                    //            return currentFragment;
                    //        break;
                    //    }
                    case 3:
                    {
                        var currentFragment = FragmentListTab3[FragmentListTab3.Count - 2];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }
                    case 4:
                    {
                        var currentFragment = FragmentListTab4[FragmentListTab4.Count - 2];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }

                    default:
                        return null!;

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }

            return null!;
        }

        public int GetCountFragment()
        {
            try
            {
                switch (PageNumber)
                {
                    case 0:
                        return FragmentListTab0.Count > 1 ? FragmentListTab0.Count : 0;
                    case 1:
                        return FragmentListTab1.Count > 1 ? FragmentListTab1.Count : 0;
                    //case 2:
                    //    return FragmentListTab2.Count > 1 ? FragmentListTab2.Count : 0;
                    case 3:
                        return FragmentListTab3.Count > 1 ? FragmentListTab3.Count : 0;
                    case 4:
                        return FragmentListTab4.Count > 1 ? FragmentListTab4.Count : 0;
                    default:
                        return 0;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        private static void HideFragmentFromList(List<Fragment> fragmentList, FragmentTransaction ft)
        {
            try
            {
                if (fragmentList.Count < 0)
                    return;

                foreach (var fra in fragmentList.Where(fra => fra.IsVisible))
                {
                    ft.Hide(fra);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisplayFragment(Fragment newFragment)
        {
            try
            {
                var ft = Context.SupportFragmentManager.BeginTransaction();

                HideFragmentFromList(FragmentListTab0, ft);
                HideFragmentFromList(FragmentListTab1, ft);
                //HideFragmentFromList(FragmentListTab2, ft);
                HideFragmentFromList(FragmentListTab3, ft);
                HideFragmentFromList(FragmentListTab4, ft);

                switch (PageNumber)
                {
                    case 0:
                        {
                            if (!FragmentListTab0.Contains(newFragment))
                                FragmentListTab0.Add(newFragment);
                            break;
                        }
                    case 1:
                        {
                            if (!FragmentListTab1.Contains(newFragment))
                                FragmentListTab1.Add(newFragment);
                            break;
                        }
                    //case 2:
                    //{
                    //  if (!FragmentListTab2.Contains(newFragment))
                    //        FragmentListTab2.Add(newFragment);
                    //    break;
                    //}   
                    case 3:
                        {
                            if (!FragmentListTab3.Contains(newFragment))
                                FragmentListTab3.Add(newFragment);
                            break;
                        }
                    case 4:
                        {
                            if (!FragmentListTab4.Contains(newFragment))
                                FragmentListTab4.Add(newFragment);
                            break;
                        }
                }

                if (!newFragment.IsAdded)
                    ft.Add(Resource.Id.content, newFragment, newFragment.Tag);

                ft.Show(newFragment).Commit();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RemoveFragment(Fragment oldFragment)
        {
            try
            {
                var ft = Context.SupportFragmentManager.BeginTransaction();

                switch (PageNumber)
                {
                    case 0:
                        {
                            if (FragmentListTab0.Contains(oldFragment))
                                FragmentListTab0.Remove(oldFragment);
                            break;
                        }
                    case 1:
                        {
                            if (FragmentListTab1.Contains(oldFragment))
                                FragmentListTab1.Remove(oldFragment);
                            break;
                        }
                    //if (PageNumber == 2)
                    //    if (FragmentListTab2.Contains(oldFragment))
                    //        FragmentListTab2.Remove(oldFragment);
                    case 3:
                        {
                            if (FragmentListTab3.Contains(oldFragment))
                                FragmentListTab3.Remove(oldFragment);
                            break;
                        }
                    case 4:
                        {
                            if (FragmentListTab4.Contains(oldFragment))
                                FragmentListTab4.Remove(oldFragment);
                            break;
                        }
                }

                HideFragmentFromList(FragmentListTab0, ft);
                HideFragmentFromList(FragmentListTab1, ft);
                //HideFragmentFromList(FragmentListTab2, ft);
                HideFragmentFromList(FragmentListTab3, ft);
                HideFragmentFromList(FragmentListTab4, ft);

                if (oldFragment.IsAdded)
                    ft.Remove(oldFragment).Commit();

                ft = Context.SupportFragmentManager.BeginTransaction();
                switch (PageNumber)
                {
                    case 0:
                        {
                            var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                            ft.Show(currentFragment).Commit();
                            break;
                        }
                    case 1:
                        {
                            var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                            ft.Show(currentFragment).Commit();
                            break;
                        }
                    //case 2:
                    //    {
                    //        var currentFragment = FragmentListTab2[FragmentListTab2.Count - 1];
                    //        ft.Show(currentFragment).Commit();
                    //        break;
                    //    }
                    case 3:
                        {
                            var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                            ft.Show(currentFragment).Commit();
                            break;
                        }
                    case 4:
                        {
                            var currentFragment = FragmentListTab4[FragmentListTab4.Count - 1];
                            ft.Show(currentFragment).Commit();
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnBackStackClickFragment()
        {
            try
            {
                switch (PageNumber)
                {
                    case 0 when FragmentListTab0.Count > 1:
                        {
                            var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                            if (currentFragment != null)
                                RemoveFragment(currentFragment);
                            break;
                        }
                    case 0:
                        Context.Finish();
                        break;
                    case 1 when FragmentListTab1.Count > 1:
                        {
                            var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                            if (currentFragment != null)
                                RemoveFragment(currentFragment);
                            break;
                        }
                    case 1:
                        Context.Finish();
                        break;
                    //case 2 when FragmentListTab2.Count > 1:
                    //{
                    //    var currentFragment = FragmentListTab2[FragmentListTab2.Count - 1];
                    //    if (currentFragment != null)
                    //        RemoveFragment(currentFragment);
                    //    break;
                    //}
                    //case 2:
                    //    Context.Finish();
                    //    break;
                    case 3 when FragmentListTab3.Count > 1:
                        {
                            var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                            if (currentFragment != null)
                                RemoveFragment(currentFragment);
                            break;
                        }
                    case 3:
                        Context.Finish();
                        break;
                    case 4 when FragmentListTab4.Count > 1:
                        {
                            var currentFragment = FragmentListTab4[FragmentListTab4.Count - 1];
                            if (currentFragment != null)
                                RemoveFragment(currentFragment);
                            break;
                        }
                    case 4:
                        Context.Finish();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowFragment0()
        {
            try
            {
                if (FragmentListTab0.Count <= 0)
                    return;
                var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment);

                StopVideoExo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShowFragment1()
        {
            try
            {
                if (FragmentListTab1.Count <= 0) return;
                var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment);

                StopVideoExo();

                Context.InAppReview();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //private void ShowFragment2()
        //{
        //    try
        //    {
        //        if (FragmentListTab2.Count <= 0) return;
        //        var currentFragment = FragmentListTab2[FragmentListTab2.Count - 1];
        //        if (currentFragment != null)
        //            DisplayFragment(currentFragment);

        //        StopVideoExo();
        //    }
        //    catch (Exception e)
        //    {
        //       Methods.DisplayReportResultTrack(e);
        //    }
        //}

        private void ShowFragment3()
        {
            try
            {
                if (FragmentListTab3.Count <= 0) return;
                var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment);

                StopVideoExo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShowFragment4()
        {
            try
            {
                if (FragmentListTab4.Count <= 0) return;
                var currentFragment = FragmentListTab4[FragmentListTab4.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment);

                StopVideoExo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StopVideoExo()
        {
            try
            {
                Context?.NewsFeedFragment?.RecyclerFeed?.StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public Fragment GetSelectedTabLastStackFragment()
        {
            try
            {
                switch (PageNumber)
                {
                    case 0:
                    {
                        var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }
                    case 1:
                    {
                        var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }
                    case 3:
                    {
                        var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }
                    case 4:
                    {
                        var currentFragment = FragmentListTab4[FragmentListTab4.Count - 1];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }

                    default:
                        return null!;

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }

            return null!;
        }
         
    }
}