using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Com.OneSignal.Abstractions;
using Newtonsoft.Json;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using OSNotification = Com.OneSignal.Abstractions.OSNotification;
using OSNotificationPayload = Com.OneSignal.Abstractions.OSNotificationPayload;

namespace PixelPhoto.OneSignal
{
    public class OneSignalNotification
    { 
        public static string Userid;
        public static NotificationDataObject NotificationInfo;
        public static UserDataObject UserData;
        public static PostsObject PostData;

        public static void RegisterNotificationDevice()
        {
            try
            {
                if (AppSettings.ShowNotification && UserDetails.NotificationPopup)
                {
                    if (!string.IsNullOrEmpty(AppSettings.OneSignalAppId) || !string.IsNullOrWhiteSpace(AppSettings.OneSignalAppId))
                    {
                        Com.OneSignal.OneSignal.Current.StartInit(AppSettings.OneSignalAppId)
                            .InFocusDisplaying(OSInFocusDisplayOption.Notification)
                            .HandleNotificationReceived(HandleNotificationReceived)
                            .HandleNotificationOpened(HandleNotificationOpened)
                            .EndInit();
                        Com.OneSignal.OneSignal.Current.IdsAvailable(IdsAvailable);
                        Com.OneSignal.OneSignal.Current.RegisterForPushNotifications();
                        Com.OneSignal.OneSignal.Current.SetSubscription(true);
                        AppSettings.ShowNotification = true;
                    }
                }
                else
                {
                    UnRegisterNotificationDevice();
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public static void UnRegisterNotificationDevice()
        {
            try
            {
                Com.OneSignal.OneSignal.Current.SetSubscription(false);
                AppSettings.ShowNotification = false;
                UserDetails.DeviceId = string.Empty;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private static void IdsAvailable(string userId, string pushToken)
        {
            try
            {
                UserDetails.DeviceId = userId;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private static void HandleNotificationReceived(OSNotification notification)
        {
            try
            {
                var payload = notification.payload;
                var additionalData = payload.additionalData;
                Console.WriteLine(additionalData);
                //string message = payload.body;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static void HandleNotificationOpened(OSNotificationOpenedResult result)
        {
            try
            {
                var payload = result.notification.payload;
                var additionalData = payload.additionalData;
                var message = payload.body;
                Console.WriteLine(message);
                var actionId = result.action.actionID;

                if (additionalData != null)
                {
                    foreach (var item in additionalData)
                    {
                        switch (item.Key)
                        {
                            case "user_id":
                                Userid = item.Value.ToString();
                                break;
                            case "post_data":
                                PostData = JsonConvert.DeserializeObject<PostsObject>(item.Value.ToString());
                                break;
                            case "notification_info":
                                NotificationInfo = JsonConvert.DeserializeObject<NotificationDataObject>(item.Value.ToString());
                                break;
                            case "user_data":
                                UserData = JsonConvert.DeserializeObject<UserDataObject>(item.Value.ToString());
                                break;
                            case "url":
                            {
                                var url = item.Value.ToString();
                                break;
                            }
                        }
                    }

                    //to : do
                    //go to activity or fragment depending on data
                    var intent = new Intent(Application.Context, typeof(HomeActivity));
                    intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    intent.AddFlags(ActivityFlags.SingleTop);
                    intent.SetAction(Intent.ActionView);
                    intent.PutExtra("TypeNotification", NotificationInfo.Type);
                    Application.Context.StartActivity(intent);

                    if (additionalData.ContainsKey("discount"))
                    {
                        // Take user to your store..
                    }
                }

                if (actionId != null)
                {
                    // actionSelected equals the id on the button the user pressed.
                    // actionSelected will equal "__DEFAULT__" when the notification itself was tapped when buttons were present. 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}