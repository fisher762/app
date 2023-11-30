using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PixelPhoto.Activities.Chat;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.OneSignal;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.Classes.Messages;
using PixelPhotoClient.GlobalClass;
using SQLite;
using Exception = System.Exception;

namespace PixelPhoto.SQLite
{
    public class SqLiteDatabase
    {
        //############# DON'T MODIFY HERE #############
        private static readonly string Folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public static readonly string PathCombine = Path.Combine(Folder, AppSettings.DatabaseName + "_.db");
        private SQLiteConnection Connection;

        //Open Connection in Database
        //*********************************************************

        #region Connection

        private SQLiteConnection OpenConnection()
        {
            try
            {
                Connection = new SQLiteConnection(PathCombine);
                return Connection;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                  return OpenConnection();
                else
                    Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public void CheckTablesStatus()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    Connection.CreateTable<DataTables.LoginTb>();
                    Connection.CreateTable<DataTables.SettingsTb>();
                    Connection.CreateTable<DataTables.MyProfileTb>();
                    Connection.CreateTable<DataTables.LastChatTb>();
                    Connection.CreateTable<DataTables.MessageTb>();
                    //Connection.Dispose();
                    //Connection.Close();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    CheckTablesStatus();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Close Connection in Database
        //public void Dispose()
        //{
        //    try
        //    {
        //        using (Connection = OpenConnection())
        //        {
        //            if (Connection == null) return;

        //            Connection.Dispose();
        //            Connection.Close();
        //            GC.SuppressFinalize(this);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}
         
        //Delete table
        public void DropAll()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    Connection.DropTable<DataTables.LoginTb>();
                    Connection.DropTable<DataTables.SettingsTb>();
                    Connection.DropTable<DataTables.MyProfileTb>();
                    Connection.DropTable<DataTables.LastChatTb>();
                    Connection.DropTable<DataTables.MessageTb>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DropAll();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion Connection

        //########################## End SQLite_Entity ##########################

        //Start SQL_Commander >>  General
        //*********************************************************

        #region General

        public void InsertRow(object row)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    Connection.Insert(row);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UpdateRow(object row)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    Connection.Update(row);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DeleteRow(object row)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    Connection.Delete(row);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InsertListOfRows(List<object> row)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    Connection.InsertAll(row);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion General

        //Start SQL_Commander >>  Custom
        //*********************************************************

        #region Login

        //Insert Or Update data Login
        public void InsertOrUpdateLogin_Credentials(DataTables.LoginTb db)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    var dataUser = Connection.Table<DataTables.LoginTb>().FirstOrDefault();
                    if (dataUser != null)
                    {
                        dataUser.UserId = UserDetails.UserId;
                        dataUser.AccessToken = UserDetails.AccessToken;
                        dataUser.Cookie = UserDetails.Cookie;
                        dataUser.Username = UserDetails.Username;
                        dataUser.Password = UserDetails.Password;
                        dataUser.Status = UserDetails.Status;
                        dataUser.Lang = AppSettings.Lang;
                        dataUser.DeviceId = UserDetails.DeviceId;
                        dataUser.Email = UserDetails.Email;

                        Connection.Update(dataUser);
                    }
                    else
                    {
                        Connection.Insert(db);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateLogin_Credentials(db);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data Login
        public DataTables.LoginTb Get_data_Login_Credentials()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return null!;

                    var dataUser = Connection.Table<DataTables.LoginTb>().FirstOrDefault();
                    if (dataUser != null)
                    {
                        UserDetails.Username = dataUser.Username;
                        UserDetails.FullName = dataUser.Username;
                        UserDetails.Password = dataUser.Password;
                        UserDetails.AccessToken = dataUser.AccessToken;
                        UserDetails.UserId = dataUser.UserId;
                        UserDetails.Status = dataUser.Status;
                        UserDetails.Cookie = dataUser.Cookie;
                        UserDetails.Email = dataUser.Email;
                        AppSettings.Lang = dataUser.Lang;
                        Current.AccessToken = dataUser.AccessToken;
                        ListUtils.DataUserLoginList.Add(dataUser);
                     
                        return dataUser;
                    }
                    else
                    {
                        return null!;
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_data_Login_Credentials();
                else
                { 
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                }
            }
        }

        #endregion

        #region Settings

        public void InsertOrReplaceSettingsAsync(GetSettingsObject.Config settingsData)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                     
                    var dataSettings = Connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                    if (dataSettings != null)
                    {
                        dataSettings.SiteUrl = settingsData.SiteUrl;
                        dataSettings.SiteName = settingsData.SiteName;
                        dataSettings.Theme = settingsData.Validation;
                        dataSettings.FfmpegSys = settingsData.FfmpegSys;
                        dataSettings.FfmpegBinary = settingsData.FfmpegBinary;
                        dataSettings.MaxVideoDuration = settingsData.MaxVideoDuration;
                        dataSettings.YtApi = settingsData.YtApi;
                        dataSettings.GiphyApi = settingsData.GiphyApi;
                        dataSettings.UploadImages = settingsData.UploadImages;
                        dataSettings.UploadVideos = settingsData.UploadVideos;
                        dataSettings.ImportVideos = settingsData.ImportVideos;
                        dataSettings.ImportImages = settingsData.ImportImages;
                        dataSettings.StorySystem = settingsData.StorySystem;
                        dataSettings.SignupSystem = settingsData.SignupSystem;
                        dataSettings.DeleteAccount = settingsData.DeleteAccount;
                        dataSettings.Recaptcha = settingsData.Recaptcha;
                        dataSettings.RecaptchaKey = settingsData.RecaptchaKey;
                        dataSettings.SiteDesc = settingsData.SiteDesc;
                        dataSettings.SiteEmail = settingsData.SiteEmail;
                        dataSettings.MetaKeywords = settingsData.MetaKeywords;
                        dataSettings.Obscene = settingsData.Obscene;
                        dataSettings.MaxUpload = settingsData.MaxUpload;
                        dataSettings.CaptionLen = settingsData.CaptionLen;
                        dataSettings.CommentLen = settingsData.CommentLen;
                        dataSettings.Language = settingsData.Language;
                        dataSettings.SmtpOrMail = settingsData.SmtpOrMail;
                        dataSettings.SmtpHost = settingsData.SmtpHost;
                        dataSettings.SmtpUsername = settingsData.SmtpUsername;
                        dataSettings.SmtpPassword = settingsData.SmtpPassword;
                        dataSettings.SmtpPort = settingsData.SmtpPort;
                        dataSettings.SmtpEncryption = settingsData.SmtpEncryption;
                        dataSettings.FbLogin = settingsData.FbLogin;
                        dataSettings.TwLogin = settingsData.TwLogin;
                        dataSettings.GlLogin = settingsData.GlLogin;
                        dataSettings.FacebookAppId = settingsData.FacebookAppId;
                        dataSettings.FacebookAppKey = settingsData.FacebookAppKey;
                        dataSettings.TwitterAppId = settingsData.TwitterAppId;
                        dataSettings.TwitterAppKey = settingsData.TwitterAppKey;
                        dataSettings.GoogleAppId = settingsData.GoogleAppId;
                        dataSettings.GoogleAppKey = settingsData.GoogleAppKey;
                        dataSettings.SiteDocs = settingsData.SiteDocs;
                        dataSettings.LastCreatedSitemap = settingsData.LastCreatedSitemap;
                        dataSettings.LastBackup = settingsData.LastBackup;
                        dataSettings.StoryDuration = settingsData.StoryDuration;
                        dataSettings.LastCleanDb = settingsData.LastCleanDb;
                        dataSettings.EmailValidation = settingsData.EmailValidation;
                        dataSettings.AmazoneS3 = settingsData.AmazoneS3;
                        dataSettings.BucketName = settingsData.BucketName;
                        dataSettings.AmazoneS3Key = settingsData.AmazoneS3Key;
                        dataSettings.AmazoneS3SKey = settingsData.AmazoneS3SKey;
                        dataSettings.Region = settingsData.Region;
                        dataSettings.Ad1 = settingsData.Ad1;
                        dataSettings.Ad2 = settingsData.Ad2;
                        dataSettings.Ad3 = settingsData.Ad3;
                        dataSettings.GoogleAnalytics = settingsData.GoogleAnalytics;
                        dataSettings.FtpUpload = settingsData.FtpUpload;
                        dataSettings.FtpHost = settingsData.FtpHost;
                        dataSettings.FtpUsername = settingsData.FtpUsername;
                        dataSettings.FtpPassword = settingsData.FtpPassword;
                        dataSettings.FtpPort = settingsData.FtpPort;
                        dataSettings.FtpEndpoint = settingsData.FtpEndpoint;
                        dataSettings.AppApiId = settingsData.AppApiId;
                        dataSettings.AppApiKey = settingsData.AppApiKey;
                        dataSettings.WowonderAppId = settingsData.WowonderAppId;
                        dataSettings.WowonderAppKey = settingsData.WowonderAppKey;
                        dataSettings.WowonderDomainUri = settingsData.WowonderDomainUri;
                        dataSettings.WowonderLogin = settingsData.WowonderLogin;
                        dataSettings.LastRun = settingsData.LastRun;
                        dataSettings.ConfigRun = settingsData.ConfigRun;
                        dataSettings.WowonderDomainIcon = settingsData.WowonderDomainIcon;
                        dataSettings.ServerKey = settingsData.ServerKey;
                        dataSettings.PlaytubeUrl = settingsData.PlaytubeUrl;
                        dataSettings.RecaptchaSiteKey = settingsData.RecaptchaSiteKey;
                        dataSettings.RecaptchaSecretKey = settingsData.RecaptchaSecretKey;
                        dataSettings.Watermark = settingsData.Watermark;
                        dataSettings.WatermarkLink = settingsData.WatermarkLink;
                        dataSettings.Mp4Links = settingsData.Mp4Links;
                        dataSettings.PlaytubeLinks = settingsData.PlaytubeLinks;
                        dataSettings.FaceFilter = settingsData.FaceFilter;
                        dataSettings.Header = settingsData.Header;
                        dataSettings.Footer = settingsData.Footer;
                        dataSettings.AdCPrice = settingsData.AdCPrice;
                        dataSettings.AffiliateSystem = settingsData.AffiliateSystem;
                        dataSettings.AdVPrice = settingsData.AdVPrice;
                        dataSettings.AffiliateType = settingsData.AffiliateType;
                        dataSettings.AmountPercentRef = settingsData.AmountPercentRef;
                        dataSettings.AmountRef = settingsData.AmountRef;
                        dataSettings.BankDescription = settingsData.BankDescription;
                        dataSettings.BankPayment = settingsData.BankPayment;
                        dataSettings.BankTransferNote = settingsData.BankTransferNote;
                        dataSettings.BlogSystem = settingsData.BlogSystem;
                        dataSettings.BoostedPosts = settingsData.BoostedPosts;
                        dataSettings.BusinessAccount = settingsData.BusinessAccount;
                        dataSettings.ClickableUrl = settingsData.ClickableUrl;
                        dataSettings.CreditCard = settingsData.CreditCard;
                        dataSettings.Currency = settingsData.Currency;
                        dataSettings.DonatePercentage = settingsData.DonatePercentage;
                        dataSettings.FaviconExtension = settingsData.FaviconExtension;
                        dataSettings.GoogleMap = settingsData.GoogleMap;
                        dataSettings.GoogleMapApi = settingsData.GoogleMapApi;
                        dataSettings.ImageSellSystem = settingsData.ImageSellSystem;
                        dataSettings.LogoExtension = settingsData.LogoExtension;
                        dataSettings.MinImageHeight = settingsData.MinImageHeight;
                        dataSettings.MinImageWidth = settingsData.MinImageWidth;
                        dataSettings.PaypalId = settingsData.PaypalId;
                        dataSettings.PaypalMode = settingsData.PaypalMode;
                        dataSettings.PaypalSecret = settingsData.PaypalSecret;
                        dataSettings.ProPrice = settingsData.ProPrice;
                        dataSettings.ProSystem = settingsData.ProSystem;
                        dataSettings.Push = settingsData.Push;
                        dataSettings.PushId = settingsData.PushId;
                        dataSettings.PushKey = settingsData.PushKey;
                        dataSettings.RaiseMoney = settingsData.RaiseMoney;
                        dataSettings.RaiseMoneyType = settingsData.RaiseMoneyType;
                        dataSettings.StripeId = settingsData.StripeId;
                        dataSettings.StripeSecret = settingsData.StripeSecret;
                        dataSettings.UserAds = settingsData.UserAds;
                        dataSettings.Validation = settingsData.Validation;
                        dataSettings.Version = settingsData.Version;
                        dataSettings.WatermarkAnchor = settingsData.WatermarkAnchor;
                        dataSettings.WatermarkOpacity = settingsData.WatermarkOpacity;
                        dataSettings.WithdrawSystem = settingsData.WithdrawSystem;

                        Connection.Update(dataSettings);
                    }
                    else
                    {
                        var stTb = new DataTables.SettingsTb
                        {
                            SiteUrl = settingsData.SiteUrl,
                            SiteName = settingsData.SiteName,
                            Theme = settingsData.Validation,
                            FfmpegSys = settingsData.FfmpegSys,
                            FfmpegBinary = settingsData.FfmpegBinary,
                            MaxVideoDuration = settingsData.MaxVideoDuration,
                            YtApi = settingsData.YtApi,
                            GiphyApi = settingsData.GiphyApi,
                            UploadImages = settingsData.UploadImages,
                            UploadVideos = settingsData.UploadVideos,
                            ImportVideos = settingsData.ImportVideos,
                            ImportImages = settingsData.ImportImages,
                            StorySystem = settingsData.StorySystem,
                            SignupSystem = settingsData.SignupSystem,
                            DeleteAccount = settingsData.DeleteAccount,
                            Recaptcha = settingsData.Recaptcha,
                            RecaptchaKey = settingsData.RecaptchaKey,
                            SiteDesc = settingsData.SiteDesc,
                            SiteEmail = settingsData.SiteEmail,
                            MetaKeywords = settingsData.MetaKeywords,
                            Obscene = settingsData.Obscene,
                            MaxUpload = settingsData.MaxUpload,
                            CaptionLen = settingsData.CaptionLen,
                            CommentLen = settingsData.CommentLen,
                            Language = settingsData.Language,
                            SmtpOrMail = settingsData.SmtpOrMail,
                            SmtpHost = settingsData.SmtpHost,
                            SmtpUsername = settingsData.SmtpUsername,
                            SmtpPassword = settingsData.SmtpPassword,
                            SmtpPort = settingsData.SmtpPort,
                            SmtpEncryption = settingsData.SmtpEncryption,
                            FbLogin = settingsData.FbLogin,
                            TwLogin = settingsData.TwLogin,
                            GlLogin = settingsData.GlLogin,
                            FacebookAppId = settingsData.FacebookAppId,
                            FacebookAppKey = settingsData.FacebookAppKey,
                            TwitterAppId = settingsData.TwitterAppId,
                            TwitterAppKey = settingsData.TwitterAppKey,
                            GoogleAppId = settingsData.GoogleAppId,
                            GoogleAppKey = settingsData.GoogleAppKey,
                            SiteDocs = settingsData.SiteDocs,
                            LastCreatedSitemap = settingsData.LastCreatedSitemap,
                            LastBackup = settingsData.LastBackup,
                            StoryDuration = settingsData.StoryDuration,
                            LastCleanDb = settingsData.LastCleanDb,
                            EmailValidation = settingsData.EmailValidation,
                            AmazoneS3 = settingsData.AmazoneS3,
                            BucketName = settingsData.BucketName,
                            AmazoneS3Key = settingsData.AmazoneS3Key,
                            AmazoneS3SKey = settingsData.AmazoneS3SKey,
                            Region = settingsData.Region,
                            Ad1 = settingsData.Ad1,
                            Ad2 = settingsData.Ad2,
                            Ad3 = settingsData.Ad3,
                            GoogleAnalytics = settingsData.GoogleAnalytics,
                            FtpUpload = settingsData.FtpUpload,
                            FtpHost = settingsData.FtpHost,
                            FtpUsername = settingsData.FtpUsername,
                            FtpPassword = settingsData.FtpPassword,
                            FtpPort = settingsData.FtpPort,
                            FtpEndpoint = settingsData.FtpEndpoint,
                            AppApiId = settingsData.AppApiId,
                            AppApiKey = settingsData.AppApiKey,
                            WowonderAppId = settingsData.WowonderAppId,
                            WowonderAppKey = settingsData.WowonderAppKey,
                            WowonderDomainUri = settingsData.WowonderDomainUri,
                            WowonderLogin = settingsData.WowonderLogin,
                            LastRun = settingsData.LastRun,
                            ConfigRun = settingsData.ConfigRun,
                            WowonderDomainIcon = settingsData.WowonderDomainIcon,
                            ServerKey = settingsData.ServerKey,
                            PlaytubeUrl = settingsData.PlaytubeUrl,
                            RecaptchaSiteKey = settingsData.RecaptchaSiteKey,
                            RecaptchaSecretKey = settingsData.RecaptchaSecretKey,
                            Watermark = settingsData.Watermark,
                            WatermarkLink = settingsData.WatermarkLink,
                            Mp4Links = settingsData.Mp4Links,
                            PlaytubeLinks = settingsData.PlaytubeLinks,
                            FaceFilter = settingsData.FaceFilter,
                            Header = settingsData.Header,
                            Footer = settingsData.Footer,
                            AdCPrice = settingsData.AdCPrice,
                            AffiliateSystem = settingsData.AffiliateSystem,
                            AdVPrice = settingsData.AdVPrice,
                            AffiliateType = settingsData.AffiliateType,
                            AmountPercentRef = settingsData.AmountPercentRef,
                            AmountRef = settingsData.AmountRef,
                            BankDescription = settingsData.BankDescription,
                            BankPayment = settingsData.BankPayment,
                            BankTransferNote = settingsData.BankTransferNote,
                            BlogSystem = settingsData.BlogSystem,
                            BoostedPosts = settingsData.BoostedPosts,
                            BusinessAccount = settingsData.BusinessAccount,
                            ClickableUrl = settingsData.ClickableUrl,
                            CreditCard = settingsData.CreditCard,
                            Currency = settingsData.Currency,
                            DonatePercentage = settingsData.DonatePercentage,
                            FaviconExtension = settingsData.FaviconExtension,
                            GoogleMap = settingsData.GoogleMap,
                            GoogleMapApi = settingsData.GoogleMapApi,
                            ImageSellSystem = settingsData.ImageSellSystem,
                            LogoExtension = settingsData.LogoExtension,
                            MinImageHeight = settingsData.MinImageHeight,
                            MinImageWidth = settingsData.MinImageWidth,
                            PaypalId = settingsData.PaypalId,
                            PaypalMode = settingsData.PaypalMode,
                            PaypalSecret = settingsData.PaypalSecret,
                            ProPrice = settingsData.ProPrice,
                            ProSystem = settingsData.ProSystem,
                            Push = settingsData.Push,
                            PushId = settingsData.PushId,
                            PushKey = settingsData.PushKey,
                            RaiseMoney = settingsData.RaiseMoney,
                            RaiseMoneyType = settingsData.RaiseMoneyType,
                            StripeId = settingsData.StripeId,
                            StripeSecret = settingsData.StripeSecret,
                            UserAds = settingsData.UserAds,
                            Validation = settingsData.Validation,
                            Version = settingsData.Version,
                            WatermarkAnchor = settingsData.WatermarkAnchor,
                            WatermarkOpacity = settingsData.WatermarkOpacity,
                            WithdrawSystem = settingsData.WithdrawSystem,
                        };
                        Connection.Insert(stTb);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrReplaceSettingsAsync(settingsData);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public GetSettingsObject.Config GetSettings()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return null!;
                     
                    var result = Connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                    if (result != null)
                    {
                        var settingsData = new GetSettingsObject.Config
                        {
                            SiteUrl = result.SiteUrl,
                            SiteName = result.SiteName,
                            Theme = result.Validation,
                            FfmpegSys = result.FfmpegSys,
                            FfmpegBinary = result.FfmpegBinary,
                            MaxVideoDuration = result.MaxVideoDuration,
                            YtApi = result.YtApi,
                            GiphyApi = result.GiphyApi,
                            UploadImages = result.UploadImages,
                            UploadVideos = result.UploadVideos,
                            ImportVideos = result.ImportVideos,
                            ImportImages = result.ImportImages,
                            StorySystem = result.StorySystem,
                            SignupSystem = result.SignupSystem,
                            DeleteAccount = result.DeleteAccount,
                            Recaptcha = result.Recaptcha,
                            RecaptchaKey = result.RecaptchaKey,
                            SiteDesc = result.SiteDesc,
                            SiteEmail = result.SiteEmail,
                            MetaKeywords = result.MetaKeywords,
                            Obscene = result.Obscene,
                            MaxUpload = result.MaxUpload,
                            CaptionLen = result.CaptionLen,
                            CommentLen = result.CommentLen,
                            Language = result.Language,
                            SmtpOrMail = result.SmtpOrMail,
                            SmtpHost = result.SmtpHost,
                            SmtpUsername = result.SmtpUsername,
                            SmtpPassword = result.SmtpPassword,
                            SmtpPort = result.SmtpPort,
                            SmtpEncryption = result.SmtpEncryption,
                            FbLogin = result.FbLogin,
                            TwLogin = result.TwLogin,
                            GlLogin = result.GlLogin,
                            FacebookAppId = result.FacebookAppId,
                            FacebookAppKey = result.FacebookAppKey,
                            TwitterAppId = result.TwitterAppId,
                            TwitterAppKey = result.TwitterAppKey,
                            GoogleAppId = result.GoogleAppId,
                            GoogleAppKey = result.GoogleAppKey,
                            SiteDocs = result.SiteDocs,
                            LastCreatedSitemap = result.LastCreatedSitemap,
                            LastBackup = result.LastBackup,
                            StoryDuration = result.StoryDuration,
                            LastCleanDb = result.LastCleanDb,
                            EmailValidation = result.EmailValidation,
                            AmazoneS3 = result.AmazoneS3,
                            BucketName = result.BucketName,
                            AmazoneS3Key = result.AmazoneS3Key,
                            AmazoneS3SKey = result.AmazoneS3SKey,
                            Region = result.Region,
                            Ad1 = result.Ad1,
                            Ad2 = result.Ad2,
                            Ad3 = result.Ad3,
                            GoogleAnalytics = result.GoogleAnalytics,
                            FtpUpload = result.FtpUpload,
                            FtpHost = result.FtpHost,
                            FtpUsername = result.FtpUsername,
                            FtpPassword = result.FtpPassword,
                            FtpPort = result.FtpPort,
                            FtpEndpoint = result.FtpEndpoint,
                            AppApiId = result.AppApiId,
                            AppApiKey = result.AppApiKey,
                            WowonderAppId = result.WowonderAppId,
                            WowonderAppKey = result.WowonderAppKey,
                            WowonderDomainUri = result.WowonderDomainUri,
                            WowonderLogin = result.WowonderLogin,
                            LastRun = result.LastRun,
                            ConfigRun = result.ConfigRun,
                            WowonderDomainIcon = result.WowonderDomainIcon,
                            ServerKey = result.ServerKey,
                            PlaytubeUrl = result.PlaytubeUrl,
                            RecaptchaSiteKey = result.RecaptchaSiteKey,
                            RecaptchaSecretKey = result.RecaptchaSecretKey,
                            Watermark = result.Watermark,
                            WatermarkLink = result.WatermarkLink,
                            Mp4Links = result.Mp4Links,
                            PlaytubeLinks = result.PlaytubeLinks,
                            FaceFilter = result.FaceFilter,
                            Header = result.Header,
                            Footer = result.Footer,
                            AdCPrice = result.AdCPrice,
                            AffiliateSystem = result.AffiliateSystem,
                            AdVPrice = result.AdVPrice,
                            AffiliateType = result.AffiliateType,
                            AmountPercentRef = result.AmountPercentRef,
                            AmountRef = result.AmountRef,
                            BankDescription = result.BankDescription,
                            BankPayment = result.BankPayment,
                            BankTransferNote = result.BankTransferNote,
                            BlogSystem = result.BlogSystem,
                            BoostedPosts = result.BoostedPosts,
                            BusinessAccount = result.BusinessAccount,
                            ClickableUrl = result.ClickableUrl,
                            CreditCard = result.CreditCard,
                            Currency = result.Currency,
                            DonatePercentage = result.DonatePercentage,
                            FaviconExtension = result.FaviconExtension,
                            GoogleMap = result.GoogleMap,
                            GoogleMapApi = result.GoogleMapApi,
                            ImageSellSystem = result.ImageSellSystem,
                            LogoExtension = result.LogoExtension,
                            MinImageHeight = result.MinImageHeight,
                            MinImageWidth = result.MinImageWidth,
                            PaypalId = result.PaypalId,
                            PaypalMode = result.PaypalMode,
                            PaypalSecret = result.PaypalSecret,
                            ProPrice = result.ProPrice,
                            ProSystem = result.ProSystem,
                            Push = result.Push,
                            PushId = result.PushId,
                            PushKey = result.PushKey,
                            RaiseMoney = result.RaiseMoney,
                            RaiseMoneyType = result.RaiseMoneyType,
                            StripeId = result.StripeId,
                            StripeSecret = result.StripeSecret,
                            UserAds = result.UserAds,
                            Validation = result.Validation,
                            Version = result.Version,
                            WatermarkAnchor = result.WatermarkAnchor,
                            WatermarkOpacity = result.WatermarkOpacity,
                            WithdrawSystem = result.WithdrawSystem,
                        };

                        ListUtils.SettingsSiteList = settingsData;

                        AppSettings.OneSignalAppId = settingsData.PushId;
                        OneSignalNotification.RegisterNotificationDevice();

                        return settingsData;
                    }
                    else
                    {
                        return null!;
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetSettings();
                else
                { 
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                }
            }
        }

        #endregion

        #region My Profile

        //Insert Or Update data My Profile Table
        public void InsertOrUpdateToMyProfileTable(UserDataObject user)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                     
                    var data = Connection.Table<DataTables.MyProfileTb>().FirstOrDefault();
                    if (data != null)
                    {
                        data.UserId = user.UserId;
                        data.Username = user.Username;
                        data.Email = user.Email;
                        data.IpAddress = user.IpAddress;
                        data.Fname = user.Fname;
                        data.Lname = user.Lname;
                        data.Gender = user.Gender;
                        data.Language = user.Language;
                        data.Avatar = user.Avatar;
                        data.Cover = user.Cover;
                        data.CountryId = user.CountryId;
                        data.About = user.About;
                        data.Google = user.Google;
                        data.Facebook = user.Facebook;
                        data.Twitter = user.Twitter;
                        data.Website = user.Website;
                        data.Active = user.Active;
                        data.Admin = user.Admin;
                        data.Verified = user.Verified;
                        data.LastSeen = user.LastSeen;
                        data.Registered = user.Registered;
                        data.IsPro = user.IsPro;
                        data.Posts = user.Posts;
                        data.PPrivacy = user.PPrivacy;
                        data.CPrivacy = user.CPrivacy;
                        data.NOnLike = user.NOnLike;
                        data.NOnMention = user.NOnMention;
                        data.NOnComment = user.NOnComment;
                        data.NOnFollow = user.NOnFollow;
                        data.StartupAvatar = user.StartupAvatar;
                        data.StartupInfo = user.StartupInfo;
                        data.StartupFollow = user.StartupFollow;
                        data.Src = user.Src;
                        data.SearchEngines = user.SearchEngines;
                        data.Mode = user.Mode;
                        data.DeviceId = user.DeviceId;
                        data.Name = user.Name;
                        data.Uname = user.Uname;
                        data.Url = user.Url;
                        data.Followers = user.Followers;
                        data.Following = user.Following;
                        data.Favourites = user.Favourites;
                        data.PostsCount = user.PostsCount;
                        data.TimeText = user.TimeText;
                        data.IsFollowing = user.IsFollowing != null && user.IsFollowing.Value;
                        data.IsBlocked = user.IsBlocked != null && user.IsBlocked.Value;
                        data.Balance = user.Balance;
                        data.BEmail = user.BEmail;
                        data.BName = user.BName;
                        data.BPhone = user.BPhone;
                        data.BSite = user.BSite;
                        data.BSiteAction = user.BSiteAction;
                        data.BusinessAccount = user.BusinessAccount;
                        data.NOnCommentLike = user.NOnCommentLike;
                        data.NOnCommentReply = user.NOnCommentReply;
                        data.PaypalEmail = user.PaypalEmail;
                        data.Profile = user.Profile;
                        data.Referrer = user.Referrer;
                        data.Wallet = user.Wallet;
                         
                        Connection.Update(data);
                    }
                    else
                    {
                        var udb = new DataTables.MyProfileTb
                        {
                            UserId = user.UserId,
                            Username = user.Username,
                            Email = user.Email,
                            IpAddress = user.IpAddress,
                            Fname = user.Fname,
                            Lname = user.Lname,
                            Gender = user.Gender,
                            Language = user.Language,
                            Avatar = user.Avatar,
                            Cover = user.Cover,
                            CountryId = user.CountryId,
                            About = user.About,
                            Google = user.Google,
                            Facebook = user.Facebook,
                            Twitter = user.Twitter,
                            Website = user.Website,
                            Active = user.Active,
                            Admin = user.Admin,
                            Verified = user.Verified,
                            LastSeen = user.LastSeen,
                            Registered = user.Registered,
                            IsPro = user.IsPro,
                            Posts = user.Posts,
                            PPrivacy = user.PPrivacy,
                            CPrivacy = user.CPrivacy,
                            NOnLike = user.NOnLike,
                            NOnMention = user.NOnMention,
                            NOnComment = user.NOnComment,
                            NOnFollow = user.NOnFollow,
                            StartupAvatar = user.StartupAvatar,
                            StartupInfo = user.StartupInfo,
                            StartupFollow = user.StartupFollow,
                            Src = user.Src,
                            SearchEngines = user.SearchEngines,
                            Mode = user.Mode,
                            DeviceId = user.DeviceId,
                            Name = user.Name,
                            Uname = user.Uname,
                            Url = user.Url,
                            Followers = user.Followers,
                            Following = user.Following,
                            Favourites = user.Favourites,
                            PostsCount = user.PostsCount,
                            TimeText = user.TimeText,
                            IsFollowing = user.IsFollowing != null && user.IsFollowing.Value,
                            IsBlocked = user.IsBlocked != null && user.IsBlocked.Value,
                            Balance = user.Balance,
                            BEmail = user.BEmail,
                            BName = user.BName,
                            BPhone = user.BPhone,
                            BSite = user.BSite,
                            BSiteAction = user.BSiteAction,
                            BusinessAccount = user.BusinessAccount,
                            NOnCommentLike = user.NOnCommentLike,
                            NOnCommentReply = user.NOnCommentReply,
                            PaypalEmail = user.PaypalEmail,
                            Profile = user.Profile,
                            Referrer = user.Referrer,
                            Wallet = user.Wallet,
                        };
                         
                        //Insert 
                        Connection.Insert(udb);
                    }

                    ListUtils.MyProfileList = new ObservableCollection<UserDataObject> {user};
                    UserDetails.Avatar = user.Avatar;
                    UserDetails.Username = user.Username;
                    UserDetails.FullName = user.Name;
                    UserDetails.Email = user.Email;
                    UserDetails.Cover = user.Cover; 
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateToMyProfileTable(user);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        // Get data To My Profile Table
        public UserDataObject GetMyProfile()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return null!;
                     
                    var user = Connection.Table<DataTables.MyProfileTb>().FirstOrDefault();
                    if (user != null)
                    {
                        var data = new UserDataObject
                        {
                            UserId = user.UserId,
                            Username = user.Username,
                            Email = user.Email,
                            IpAddress = user.IpAddress,
                            Fname = user.Fname,
                            Lname = user.Lname,
                            Gender = user.Gender,
                            Language = user.Language,
                            Avatar = user.Avatar,
                            Cover = user.Cover,
                            CountryId = user.CountryId,
                            About = user.About,
                            Google = user.Google,
                            Facebook = user.Facebook,
                            Twitter = user.Twitter,
                            Website = user.Website,
                            Active = user.Active,
                            Admin = user.Admin,
                            Verified = user.Verified,
                            LastSeen = user.LastSeen,
                            Registered = user.Registered,
                            IsPro = user.IsPro,
                            Posts = user.Posts,
                            PPrivacy = user.PPrivacy,
                            CPrivacy = user.CPrivacy,
                            NOnLike = user.NOnLike,
                            NOnMention = user.NOnMention,
                            NOnComment = user.NOnComment,
                            NOnFollow = user.NOnFollow,
                            StartupAvatar = user.StartupAvatar,
                            StartupInfo = user.StartupInfo,
                            StartupFollow = user.StartupFollow,
                            Src = user.Src,
                            SearchEngines = user.SearchEngines,
                            Mode = user.Mode,
                            DeviceId = user.DeviceId,
                            Name = user.Name,
                            Uname = user.Uname,
                            Url = user.Url,
                            Followers = user.Followers,
                            Following = user.Following,
                            Favourites = user.Favourites,
                            PostsCount = user.PostsCount,
                            TimeText = user.TimeText,
                            IsFollowing = Convert.ToBoolean(user.IsFollowing),
                            IsBlocked = Convert.ToBoolean(user.IsBlocked),
                            Balance = user.Balance,
                            BEmail = user.BEmail,
                            BName = user.BName,
                            BPhone = user.BPhone,
                            BSite = user.BSite,
                            BSiteAction = user.BSiteAction,
                            BusinessAccount = user.BusinessAccount,
                            NOnCommentLike = user.NOnCommentLike,
                            NOnCommentReply = user.NOnCommentReply,
                            PaypalEmail = user.PaypalEmail,
                            Profile = user.Profile,
                            Referrer = user.Referrer,
                            Wallet = user.Wallet,
                        };

                        UserDetails.Avatar = user.Avatar;
                        UserDetails.Username = user.Username;
                        UserDetails.FullName = user.Name;
                        UserDetails.Email = user.Email;
                        UserDetails.Cover = user.Cover;

                        ListUtils.MyProfileList = new ObservableCollection<UserDataObject> {data};

                        return data;
                    }
                    else
                    {
                        return null!;
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetMyProfile();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                }
            }
        }

        #endregion

        #region Last Chat

        //Insert data To Last Chat Table
        public void InsertOrReplaceLastChatTable(ObservableCollection<ChatDataObject> usersContactList)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                     
                    if (usersContactList.Count > 0)
                    {
                        var result = Connection.Table<DataTables.LastChatTb>().ToList();

                        var list = new List<DataTables.LastChatTb>();
                        foreach (var info in usersContactList)
                        {
                            var db = new DataTables.LastChatTb
                            {
                                UserId = info.UserId,
                                Username = info.Username,
                                Avatar = info.Avatar,
                                Time = info.Time,
                                Id = info.Id,
                                LastMessage = info.LastMessage,
                                NewMessage = info.NewMessage,
                                TimeText = info.TimeText,
                                UserDataJson = JsonConvert.SerializeObject(info.UserData),
                            };
                            list.Add(db);

                            var update = result.FirstOrDefault(a => a.UserId == info.UserId);
                            if (update != null)
                            {
                                update = db;
                                update.UserId = info.UserId;
                                update.Username = info.Username;
                                update.Avatar = info.Avatar;
                                update.Time = info.Time;
                                update.Id = info.Id;
                                update.LastMessage = info.LastMessage;
                                update.NewMessage = info.NewMessage;
                                update.TimeText = info.TimeText;
                                update.UserDataJson = JsonConvert.SerializeObject(info.UserData);
                                
                                Connection.Update(update);
                            }
                        } 

                        if (list.Count > 0)
                        {
                            Connection.BeginTransaction();
                            //Bring new  
                            var newItemList = list.Where(c => !result.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                            if (newItemList.Count > 0)
                            {
                                Connection.InsertAll(newItemList);
                            }

                            var deleteItemList = result.Where(c => !list.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                            if (deleteItemList.Count > 0)
                            {
                                foreach (var delete in deleteItemList)
                                {
                                    Connection.Delete(delete);
                                }
                            }
                             
                            Connection.Commit();
                        }
                    } 
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrReplaceLastChatTable(usersContactList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data To LastChat Table
        public ObservableCollection<ChatDataObject> GetAllLastChat()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return new ObservableCollection<ChatDataObject>();
                     
                    var select = Connection.Table<DataTables.LastChatTb>().ToList();
                    if (select.Count > 0)
                    {
                        var list = select.Select(user => new ChatDataObject
                        {
                            UserId = user.UserId,
                            Username = user.Username,
                            Avatar = user.Avatar,
                            Time = user.Time,
                            Id = user.Id,
                            LastMessage = user.LastMessage,
                            NewMessage = user.NewMessage,
                            TimeText = user.TimeText,
                            UserData = JsonConvert.DeserializeObject<UserDataObject>(user.UserDataJson),
                        }).ToList();

                        return new ObservableCollection<ChatDataObject>(list);
                    }
                    else
                        return new ObservableCollection<ChatDataObject>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetAllLastChat();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<ChatDataObject>();
                }
            }
        }

        // Get data To LastChat Table By Id >> Load More
        public ObservableCollection<ChatDataObject> GetLastChatById(int id, int nSize)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return new ObservableCollection<ChatDataObject>();
 
                    var query = Connection.Table<DataTables.LastChatTb>().Where(w => w.AutoIdLastChat >= id)
                        .OrderBy(q => q.AutoIdLastChat).Take(nSize).ToList();
                    if (query.Count > 0)
                    {
                        var list = query.Select(user => new ChatDataObject
                        {
                            UserId = user.UserId,
                            Username = user.Username,
                            Avatar = user.Avatar,
                            Time = user.Time,
                            Id = user.Id,
                            LastMessage = user.LastMessage,
                            NewMessage = user.NewMessage,
                            TimeText = user.TimeText,
                            UserData = JsonConvert.DeserializeObject<UserDataObject>(user.UserDataJson),
                        }).ToList();

                        if (list.Count > 0)
                            return new ObservableCollection<ChatDataObject>(list);
                        else
                            return new ObservableCollection<ChatDataObject>();
                    }
                    else
                        return new ObservableCollection<ChatDataObject>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetLastChatById(id, nSize);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<ChatDataObject>();
                }
            }
        }

        //Remove data To LastChat Table
        public void DeleteUserLastChat(string userId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                     
                    var user = Connection.Table<DataTables.LastChatTb>().FirstOrDefault(c => c.UserId.ToString() == userId);
                    if (user != null)
                    {
                        Connection.Delete(user);
                    } 
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteUserLastChat(userId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Clear All data LastChat
        public void ClearLastChat()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                     
                    Connection.DeleteAll<DataTables.LastChatTb>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    ClearLastChat();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Message

        //Insert data To Message Table
        public void InsertOrReplaceMessages(ObservableCollection<MessageDataObject> messageList)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                     
                    var listOfDatabaseForInsert = new List<DataTables.MessageTb>();

                    // get data from database
                    var resultMessage = Connection.Table<DataTables.MessageTb>().ToList();
                    var listAllMessage = resultMessage.Select(messages => new MessageDataObject
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
                        Position = messages.Position,
                        MessageType = messages.MessageType
                    }).ToList();

                    foreach (var messages in messageList)
                    {
                        var maTb = new DataTables.MessageTb
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
                            Position = messages.Position,
                            MessageType = messages.MessageType
                        };

                        var dataCheck = listAllMessage.FirstOrDefault(a => a.Id == messages.Id);
                        if (dataCheck != null)
                        {
                            var checkForUpdate = resultMessage.FirstOrDefault(a => a.Id == dataCheck.Id);
                            if (checkForUpdate != null)
                            {
                                checkForUpdate.Id = messages.Id;
                                checkForUpdate.FromId = messages.FromId;
                                checkForUpdate.ToId = messages.ToId;
                                checkForUpdate.Text = messages.Text;
                                checkForUpdate.MediaFile = messages.MediaFile;
                                checkForUpdate.MediaType = messages.MediaType;
                                checkForUpdate.DeletedFs1 = messages.DeletedFs1;
                                checkForUpdate.DeletedFs2 = messages.DeletedFs2;
                                checkForUpdate.Seen = messages.Seen;
                                checkForUpdate.Time = messages.Time;
                                checkForUpdate.Extra = messages.Extra;
                                checkForUpdate.TimeText = messages.TimeText;
                                checkForUpdate.Position = messages.Position;
                                checkForUpdate.MessageType = messages.MessageType;

                                Connection.Update(checkForUpdate);
                            }
                            else
                            {
                                listOfDatabaseForInsert.Add(maTb);
                            }
                        }
                        else
                        {
                            listOfDatabaseForInsert.Add(maTb);
                        }
                    }

                    Connection.BeginTransaction();

                    //Bring new  
                    if (listOfDatabaseForInsert.Count > 0)
                    {
                        Connection.InsertAll(listOfDatabaseForInsert);
                    }

                    Connection.Commit();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrReplaceMessages(messageList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Update one Messages Table
        public void InsertOrUpdateToOneMessages(MessageDataObject message)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                     
                    var data = Connection.Table<DataTables.MessageTb>().FirstOrDefault(a => a.Id == message.Id);
                    if (data != null)
                    {
                        data.Id = message.Id;
                        data.FromId = message.FromId;
                        data.ToId = message.ToId;
                        data.Text = message.Text;
                        data.MediaFile = message.MediaFile;
                        data.MediaType = message.MediaType;
                        data.DeletedFs1 = message.DeletedFs1;
                        data.DeletedFs2 = message.DeletedFs2;
                        data.Seen = message.Seen;
                        data.Time = message.Time;
                        data.Extra = message.Extra;
                        data.TimeText = message.TimeText;
                        data.Position = message.Position;
                        data.MessageType = message.MessageType;
                        Connection.Update(data);
                    }
                    else
                    {
                        var mdb = new DataTables.MessageTb
                        {
                            Id = message.Id,
                            FromId = message.FromId,
                            ToId = message.ToId,
                            Text = message.Text,
                            MediaFile = message.MediaFile,
                            MediaType = message.MediaType,
                            DeletedFs1 = message.DeletedFs1,
                            DeletedFs2 = message.DeletedFs2,
                            Seen = message.Seen,
                            Time = message.Time,
                            Extra = message.Extra,
                            TimeText = message.TimeText,
                            Position = message.Position,
                            MessageType = message.MessageType,
                        };

                        //Insert  one Messages Table
                        Connection.Insert(mdb);
                    } 
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateToOneMessages(message);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data To Messages
        public string GetMessagesList(string fromId, string toId, string beforeMessageId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return "0";


                    var beforeQ = "";
                    if (beforeMessageId != "0")
                    {
                        beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
                    }

                    var query = Connection.Query<DataTables.MessageTb>("SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" +toId + " and ToId=" + fromId + ")) " + beforeQ);
                    var queryList = query.Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId).OrderBy(q => q.Time).TakeLast(35).ToList();
                    if (queryList.Count > 0)
                    {
                        foreach (var m in queryList.Select(message => new MessageDataObject
                        {
                            Id = message.Id,
                            FromId = message.FromId,
                            ToId = message.ToId,
                            Text = message.Text,
                            MediaFile = message.MediaFile,
                            MediaType = message.MediaType,
                            DeletedFs1 = message.DeletedFs1,
                            DeletedFs2 = message.DeletedFs2,
                            Seen = message.Seen,
                            Time = message.Time,
                            Extra = message.Extra,
                            TimeText = message.TimeText,
                            Position = message.Position,
                            MessageType = !string.IsNullOrEmpty(message.MediaFile) ? "Media" : "Text",
                        }))
                        { 
                            var mAdapter = MessagesBoxActivity.Instance?.MAdapter;
                            if (mAdapter != null)
                            {
                                if (beforeMessageId == "0")
                                {
                                    mAdapter.MessageList.Add(m);
                                    mAdapter.NotifyItemInserted(mAdapter.MessageList.IndexOf(m));
                                }
                                else
                                {
                                    mAdapter.MessageList.Insert(0, m);
                                    mAdapter.NotifyItemInserted(mAdapter.MessageList.IndexOf(m));
                                }
                            } 
                        }

                        return "1";
                    }
                    else
                    {
                        return "0";
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetMessagesList(fromId, toId, beforeMessageId);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return "0";
                }
            }
        }

        //Get data To where first Messages >> load more
        public List<DataTables.MessageTb> GetMessageList(string fromId, string toId, string beforeMessageId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return new List<DataTables.MessageTb>();
                     
                    var wbeforeQ = "";
                    if (beforeMessageId != "0")
                    {
                        wbeforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
                    }
                     
                    var ss1 = Connection.Query<DataTables.MessageTb>("SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" +toId + " and ToId=" + fromId + ")) " + wbeforeQ);
                    var queryLimitFrom = ss1.Count - 26;
                    if (queryLimitFrom < 1)
                    {
                        queryLimitFrom = 0;
                    }
                     
                    //====================================

                    var beforeQ = "";
                    if (beforeMessageId != "0")
                    {
                        beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
                    }

                    var query = Connection.Query<DataTables.MessageTb>("SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" +toId + " and ToId=" + fromId + ")) " + beforeQ);
                    var queryList = query
                        .Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId)
                        .OrderBy(q => q.Time).TakeLast(35).ToList();
                    return queryList;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetMessageList(fromId, toId, beforeMessageId);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new List<DataTables.MessageTb>();
                }
            }
        }

        //Remove data To Messages Table
        public void Delete_OneMessageUser(string messageId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                     
                    var user = Connection.Table<DataTables.MessageTb>().FirstOrDefault(c => c.Id == messageId);
                    if (user != null)
                    {
                        Connection.Delete(user);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Delete_OneMessageUser(messageId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public void DeleteAllMessagesUser(string fromId, string toId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                     
                    var query = Connection.Query<DataTables.MessageTb>("Delete FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" + toId +" and ToId=" + fromId + "))");
                    Console.WriteLine(query);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteAllMessagesUser(fromId, toId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Remove All data To Messages Table
        public void ClearAll_Messages()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    Connection.DeleteAll<DataTables.MessageTb>();
                } 
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    ClearAll_Messages();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
    }
}