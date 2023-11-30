using Android.Widget;
using System;
using System.Linq;
using System.Threading.Tasks;
using PixelPhoto.Activities.Chat;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.Classes.Messages;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;

namespace PixelPhoto.Helpers.Controller
{
    public static class MessageController
    {
        //############# DON'T MODIFY HERE #############
        //========================= Functions =========================

        public static async Task SendMessageTask(int userId ,string text , string path, string hashId , UserDataObject userData)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Messages.SendMessage(userId.ToString(), text, path, hashId);
                if (apiStatus == 200)
                {
                    if (respond is SendMessageObject result)
                    {
                        if (result.Data != null)
                        {
                            UpdateLastIdMessage(result.Data, userData);
                        }
                    }
                }
                else if (apiStatus == 400)
                {
                    if (respond is ErrorObject error)
                    {
                        var errorText = error.Error.ErrorText;
                        ToastUtils.ShowToast(errorText, ToastLength.Short);
                    }
                }
                else if (apiStatus == 404)
                {
                    var error = respond.ToString();
                    ToastUtils.ShowToast(error, ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void UpdateLastIdMessage(MessageDataObject messages , UserDataObject userData)
        {
            try
            {
                var checker = MessagesBoxActivity.Instance?.MAdapter.MessageList.FirstOrDefault(a => a.Id == messages.HashId);
                if (checker != null)
                {
                    checker.Id = messages.Id;
                    checker.FromId = messages.FromId;
                    checker.ToId = messages.ToId;
                    checker.Text = messages.Text;
                    checker.MediaFile = messages.MediaFile;
                    checker.MediaType = messages.MediaType;
                    checker.DeletedFs1 = messages.DeletedFs1;
                    checker.DeletedFs2 = messages.DeletedFs2;
                    checker.Seen = messages.Seen;
                    checker.Time = messages.Time;
                    checker.Extra = messages.Extra;
                    checker.TimeText = messages.TimeText;
                    checker.Position = "Right";

                    if (string.IsNullOrEmpty(checker.MessageType))
                    {
                        checker.MessageType = !string.IsNullOrEmpty(checker.MediaFile) ? "Media" : "Text";
                    } 

                    var dataUser = LastChatActivity.MAdapter.UserList?.FirstOrDefault(a => a.UserId == messages.ToId);
                    if (dataUser != null)
                    { 
                        dataUser.LastMessage = messages.Text;

                        var index = LastChatActivity.MAdapter.UserList?.IndexOf(LastChatActivity.MAdapter.UserList?.FirstOrDefault(x => x.UserId == messages.ToId));
                        if (index > -1)
                        {
                            LastChatActivity.MAdapter.Move(dataUser);
                            LastChatActivity.MAdapter.Update(dataUser);
                        } 
                    }
                    else
                    {
                        if (userData != null)
                        {
                            LastChatActivity.MAdapter.Insert(new ChatDataObject
                            {
                                UserId = checker.ToId,
                                Username = userData.Username,
                                Avatar = userData.Avatar,
                                Time = userData.LastSeen,
                                LastMessage = messages.Text,
                                NewMessage = 0,
                                TimeText = checker.TimeText,
                                UserData = userData,
                            });
                        } 
                    }
 
                    var dbDatabase = new SqLiteDatabase();
                    var message = new MessageDataObject
                    {
                        Id = messages.Id,
                        FromId = messages.FromId,
                        ToId = messages.ToId,
                        Text = messages.Text,
                        MediaFile = messages.MediaFile,
                        MediaType = messages.MediaType,
                        DeletedFs1 = messages.DeletedFs1,
                        DeletedFs2 = messages.DeletedFs2,
                        Seen = messages.Seen,
                        Time = messages.Time,
                        Extra = messages.Extra,
                        TimeText = messages.TimeText,
                        Position = "Right",
                        MessageType = !string.IsNullOrEmpty(messages.MediaFile) ? "Media" : "Text"
                    }; 

                    //Update All data users to database
                    dbDatabase.InsertOrUpdateToOneMessages(message);

                    MessagesBoxActivity.Instance?.UpdateOneMessage(message);

                    if (UserDetails.SoundControl)
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}