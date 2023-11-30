using System;
using Android.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using PixelPhoto.Helpers.Fonts;

namespace PixelPhoto.Helpers.Utils
{
   public class EmptyStateInflater
   {
        public Button EmptyStateButton;
        private TextView EmptyStateIcon;
        private TextView DescriptionText;
        private TextView TitleText;

       public enum Type
       {
           NoConnection,
           NoSearchResult,
           SomThingWentWrong,
           NoPost,
           NoComments,
           NoNotification,
           NoUsers,
           NoFollow,
           NoMessage,
           NoBlock,
           NoFunding,
           ProfilePrivate,
           Gify,
           NoActivities,
           NoStore,
           NoSessions,
           NoFriendRequest,
       }

        public void InflateLayout(View inflated , Type type)
        {
            try
            {          
                EmptyStateIcon = (TextView)inflated.FindViewById(Resource.Id.emtyicon);
                TitleText = (TextView)inflated.FindViewById(Resource.Id.headText);
                DescriptionText = (TextView)inflated.FindViewById(Resource.Id.seconderyText);
                EmptyStateButton = (Button)inflated.FindViewById(Resource.Id.button);
                
                if (type == Type.NoConnection)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Thunderstorm);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_TitleText);
                    DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_DescriptionText);
                    EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_Button);

                }
                else if (type == Type.NoSearchResult)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Search);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoSearchResult_TitleText);
                    DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoSearchResult_DescriptionText);
                    EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_NoSearchResult_Button);

                }
                else if (type == Type.SomThingWentWrong)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Close);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_TitleText);
                    DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_DescriptionText);
                    EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_Button);
                }
                else if (type == Type.NoPost)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Frown);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoPost_TitleText);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility =ViewStates.Gone;
                }
                else if (type == Type.NoComments)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Chatbubbles);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoComments_TitleText);
                    DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoComments_DescriptionText);
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoNotification)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Notifications);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoNotification_TitleText);
                    DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoNotification_DescriptionText);
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoUsers)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Person);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_TitleText);
                    DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_DescriptionText);
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoFollow)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Person);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoFollow_TitleText);
                    DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoFollow_DescriptionText);
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoMessage)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Chatboxes);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoMessage_TitleText);
                    DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoMessage_DescriptionText);
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoBlock)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Person);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_No_Blocked_Users);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoFunding)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.HandHoldingUsd);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NotHaveAnyFundingRequest);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.ProfilePrivate)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Lock);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_profilePrivate);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.Gify)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Gift);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Gif);
                    DescriptionText.Text = "";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoActivities)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Gift);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Activities);
                    DescriptionText.Text = "";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoStore)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, EmptyStateIcon, FontAwesomeIcon.Store);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_NoStore);
                    DescriptionText.Text = "";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                    EmptyStateIcon.Visibility = ViewStates.Visible;
                }
                else if (type == Type.NoSessions)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Fingerprint);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Sessions);
                    DescriptionText.Text = "";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                } 
                else if (type == Type.NoFriendRequest)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.UserPlus);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_FollowRequest);
                    DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoFollowRequest_DescriptionText);
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            } 
        }
    }
}