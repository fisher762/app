using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.GlobalClass;
using SQLite;

namespace PixelPhoto.SQLite
{
    public class DataTables
    {
        [Table("LoginTb")]
        public class LoginTb
        {
            [PrimaryKey, AutoIncrement] public int AutoId { get; set; }

            public string UserId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string AccessToken { get; set; }
            public string Cookie { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
            public string Lang { get; set; }
            public string DeviceId { get; set; }
        }

        [Table("SettingsTb")]
        public class SettingsTb : GetSettingsObject.Config
        {
            [PrimaryKey, AutoIncrement] public int AutoId { get; set; } 
        }

        [Table("MyProfileTb")]
        public class MyProfileTb : UserDataObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdMyProfileTb { get; set; }  
        }

        [Table("LastChatTb")]
        public class LastChatTb
        {
            [PrimaryKey, AutoIncrement] public int AutoIdLastChat { get; set; }

            public string UserId { get; set; }
            public string Username { get; set; }
            public string Avatar { get; set; }
            public string Time { get; set; }
            public string Id { get; set; }
            public string LastMessage { get; set; }
            public int NewMessage { get; set; }
            public string TimeText { get; set; }
            public string UserDataJson { get; set; }
        }

        [Table("MessageTb")]
        public class MessageTb
        {
            [PrimaryKey, AutoIncrement] public int AutoIdMessage { get; set; }

            public string Id { get; set; }
            public string FromId { get; set; }
            public string ToId { get; set; }
            public string Text { get; set; }
            public string MediaFile { get; set; }
            public string MediaType { get; set; }
            public string DeletedFs1 { get; set; }
            public string DeletedFs2 { get; set; }
            public string Seen { get; set; }
            public string Time { get; set; }
            public string Extra { get; set; }
            public string TimeText { get; set; }
            public string Position { get; set; }
            public string MessageType { get; set; }
        }
    }
}