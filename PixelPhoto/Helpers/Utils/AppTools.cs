using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Media;
using Android.OS;
using Newtonsoft.Json;
using PixelPhoto.Activities.Posts.Extras;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Activities.UserProfile;
using PixelPhoto.Helpers.Model;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Messages;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;

namespace PixelPhoto.Helpers.Utils
{
    public enum ProfileTheme
    {
        DefaultTheme,
        TikTheme,
    }
     
    public static class AppTools
    {
        public static string GetNameFinal(UserDataObject dataUser)
        {
            try
            {
                if (!string.IsNullOrEmpty(dataUser.Name) && !string.IsNullOrWhiteSpace(dataUser.Name))
                    return Methods.FunString.DecodeString(dataUser.Name);

                if (!string.IsNullOrEmpty(dataUser.Username) && !string.IsNullOrWhiteSpace(dataUser.Username))
                    return Methods.FunString.DecodeString(dataUser.Username);

                return "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }

        public static string GetAboutFinal(UserDataObject dataUser)
        {
            try
            {
                if (!string.IsNullOrEmpty(dataUser.About) && !string.IsNullOrWhiteSpace(dataUser.About))
                    return Methods.FunString.DecodeString(dataUser.About);

                return Application.Context.Resources?.GetString(Resource.String.Lbl_DefaultAbout) + " " + AppSettings.ApplicationName;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Application.Context.Resources?.GetString(Resource.String.Lbl_DefaultAbout) + " " + AppSettings.ApplicationName;
            }
        }
         
        public static void OpenProfile(Activity activity, string userId, UserDataObject item)
        {
            try
            {
                if (userId != UserDetails.UserId)
                {
                    var bundle = new Bundle();
                    bundle.PutString("userinfo", JsonConvert.SerializeObject(item));
                    bundle.PutString("type", "UserData");
                    bundle.PutString("userid", userId);
                    bundle.PutString("avatar", item.Avatar);
                    bundle.PutString("fullname", item.Username);
                    switch (AppSettings.ProfileTheme)
                    {
                        case ProfileTheme.DefaultTheme:
                        {
                            var profileFragment = new UserProfileFragment
                            {
                                Arguments = bundle
                            };

                            HomeActivity.GetInstance()?.OpenFragment(profileFragment);
                            break;
                        }
                        case ProfileTheme.TikTheme:
                        {
                            var profileFragment = new TikUserProfileFragment
                            {
                                Arguments = bundle
                            };
                            HomeActivity.GetInstance()?.OpenFragment(profileFragment);
                            break;
                        }
                    } 
                }
                else
                {
                    HomeActivity.GetInstance()?.FragmentBottomNavigator.OpenProfileTab();
                }  
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void OpenProfile(Activity activity, string userId, CommentObject item)
        {
            try
            {
                if (userId != UserDetails.UserId)
                {
                    var bundle = new Bundle();
                    bundle.PutString("userinfo", JsonConvert.SerializeObject(item));
                    bundle.PutString("type", "comment");
                    bundle.PutString("userid", userId);
                    bundle.PutString("avatar", item.Avatar);
                    bundle.PutString("fullname", item.Username);
                    if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                    {
                        var profileFragment = new UserProfileFragment
                        {
                            Arguments = bundle
                        };

                        HomeActivity.GetInstance()?.OpenFragment(profileFragment);
                    }
                    else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                    {
                        var profileFragment = new TikUserProfileFragment
                        {
                            Arguments = bundle
                        };
                        HomeActivity.GetInstance()?.OpenFragment(profileFragment);
                    } 
                }
                else
                { 
                    HomeActivity.GetInstance()?.FragmentBottomNavigator.OpenProfileTab();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static bool GetStatusOnline(int lastSeen, string isShowOnline)
        {
            try
            {
                var time = Methods.Time.TimeAgo(lastSeen, false);
                var status = !string.IsNullOrEmpty(isShowOnline) && (isShowOnline == "on" && time == Methods.Time.LblJustNow ? true : false);
                return status;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static bool CheckAllowedUploadInServer(string type)
        {
            try
            {
                var settings = ListUtils.SettingsSiteList;
                if (settings == null || string.IsNullOrEmpty(settings.UploadImages) || string.IsNullOrEmpty(settings.UploadVideos))
                    return false;
                 
                switch (type)
                {
                    case "Image":
                    {
                        var check = settings.UploadImages;
                        if (check == "on")
                        {
                            // Allowed
                            return true;
                        }
                        break;
                    }
                    case "Video":
                    {
                        var check = settings.UploadVideos;
                        if (check == "on")
                        {
                            // Allowed
                            return true;
                        }
                        break;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static bool CheckAllowedFileSharingInServer(string path)
        {
            try
            {
                var settings = ListUtils.SettingsSiteList;
                if (settings == null || string.IsNullOrEmpty(settings.UploadImages) || string.IsNullOrEmpty(settings.UploadVideos))
                    return false;

                if (!string.IsNullOrEmpty(path))
                {
                    var type = Methods.AttachmentFiles.Check_FileExtension(path);
                    var check = CheckAllowedUploadInServer(type);
                    if (check)
                    {
                        // Allowed
                        return true;
                    } 
                } 
                
                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static bool CheckMimeTypesWithServer(string path)
        {
            try
            {
                var allowedExtenstionStatic = "jpg,png,jpeg,gif,mp4,webm";
                var mimeTypes = "video/mp4,video/mov,video/mpeg,video/flv,video/avi,video/webm,audio/wav,audio/mpeg,video/quicktime,audio/mp3,image/png,image/jpeg,image/gif,application/pdf,application/msword,application/zip,application/x-rar-compressed,text/pdf,application/x-pointplus,text/css,text/plain,application/x-zip-compressed"; //video/mp4,video/mov,video/mpeg,video/flv,video/avi,video/webm,audio/wav,audio/mpeg,video/quicktime,audio/mp3,image/png,image/jpeg,image/gif,application/pdf,application/msword,application/zip,application/x-rar-compressed,text/pdf,application/x-pointplus,text/css

                var fileName = path.Split('/').Last();
                var fileNameWithExtension = fileName.Split('.').Last();

                var getMimeType = MimeTypeMap.GetMimeType(fileNameWithExtension);

                if (allowedExtenstionStatic.Contains(fileNameWithExtension) && mimeTypes.Contains(getMimeType))
                {
                    var check = CheckAllowedFileSharingInServer(path);
                    if (check)  // Allowed
                        return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }
         
        // Functions Save Images
        private static void SaveFile(string id, string folder, string fileName, string url)
        {
            try
            {
                if (url.Contains("http"))
                {
                    var folderDestination = folder + id + "/";

                    var filePath = Path.Combine(folderDestination);
                    var mediaFile = filePath + "/" + fileName;

                    if (!File.Exists(mediaFile))
                    {
                        var webClient = new WebClient();

                        webClient.DownloadDataAsync(new Uri(url));
                        webClient.DownloadDataCompleted += (s, e) =>
                        {
                            try
                            {
                                File.WriteAllBytes(mediaFile, e.Result);
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        };
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Functions file from folder
        public static string GetFile(string id, string folder, string filename, string url)
        {
            try
            {
                var folderDestination = folder + id + "/";

                if (!Directory.Exists(folderDestination))
                {
                    Directory.Delete(folder, true); 
                    Directory.CreateDirectory(folderDestination);
                }
                  
                var imageFile = Methods.MultiMedia.GetMediaFrom_Gallery(folderDestination, filename);
                if (imageFile == "File Dont Exists")
                {
                    //This code runs on a new thread, control is returned to the caller on the UI thread.
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            SaveFile(id, folder, filename, url);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e); 
                        }
                    });
                    return url;
                }
                else
                {
                    return imageFile;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return url;
            }
        }

        public static string GetDuration(string mediaFile)
        {
            try
            {
                string duration;
                MediaMetadataRetriever retriever;
                if (mediaFile.Contains("http"))
                {
                    retriever = new MediaMetadataRetriever();
                    if ((int)Build.VERSION.SdkInt >= 14)
                        retriever.SetDataSource(mediaFile, new Dictionary<string, string>());
                    else
                        retriever.SetDataSource(mediaFile);

                    duration = retriever.ExtractMetadata(MetadataKey.Duration); //time In Millisec 
                    retriever.Release();
                }
                else
                { 
                    var file = Android.Net.Uri.FromFile(new Java.IO.File(mediaFile));
                    retriever = new MediaMetadataRetriever();
                    //if ((int)Build.VERSION.SdkInt >= 14)
                    //    retriever.SetDataSource(file.Path, new Dictionary<string, string>());
                    //else
                    retriever.SetDataSource(file.Path);

                    duration = retriever.ExtractMetadata(MetadataKey.Duration); //time In Millisec 
                    retriever.Release();
                }

                return duration;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "0";
            }
        }
         
        public static List<MessageDataObject> FilterMessage(List<MessageDataObject> messages)
        {
            try
            {
                foreach (var message in messages)
                {
                    message.MessageType = !string.IsNullOrEmpty(message.MediaFile) ? "Media" : "Text";
                }

                return messages;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new List<MessageDataObject>();
            }
        }
        
        public static List<PostsObject> FilterPost(List<PostsObject> postsObjects)
        {
            try
            {
                postsObjects.RemoveAll(a => a == null);

                foreach (var item in postsObjects.Where(item => item != null))
                {
                    item.MediaSet ??= new List<MediaSet>();

                    var typePost = NewsFeedAdapter.GetPostType(item);
                    if (typePost == NativeFeedType.Youtube)
                    {
                        item.MediaSet = new List<MediaSet>
                        {
                            new MediaSet
                            {
                                PostId = item.PostId,
                                UserId = item.UserId,
                                Extra = "https://img.youtube.com/vi/" + item.Youtube + "/hqdefault.jpg",
                                File = "https://img.youtube.com/vi/" + item.Youtube + "/hqdefault.jpg",
                            }
                        };
                    }

                    if (item.MediaSet.Count > 0 && !string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                    {
                        if (!item.MediaSet[0].Extra.Contains("http")) item.MediaSet[0].Extra = item.MediaSet[0]?.File;

                        if (string.IsNullOrEmpty(item.MediaSet[0]?.Extra)) item.MediaSet[0].Extra = item.MediaSet[0]?.File;

                        var type = Methods.AttachmentFiles.Check_FileExtension(item.MediaSet[0].File);
                        if (type == "Video")
                        {
                            PRecyclerView.GetInstance()?.CacheVideosFiles(Android.Net.Uri.Parse(item.MediaSet[0].File));
                        }
                    }
                     
                    item.Description = Methods.FunString.DecodeString(item.Description); 
                    //item.Mp4 = Methods.FunString.StringNullRemover(item.Mp4); 
                    item.IsOwner = UserDetails.UserId == item.UserId.ToString();
                }

                return postsObjects;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                postsObjects.RemoveAll(a => a == null || a.MediaSet?.Count == 0 || a.MediaSet == null);
                return new List<PostsObject>(postsObjects);
            }
        }

        public static Dictionary<string, string> GetCategoryStoreList()
        {
            try
            {
                var arrayAdapter = new Dictionary<string, string>
                {
                    {"491", Application.Context.GetText(Resource.String.Lbl_Category491)}, // "Abstract"},
                    {"492", Application.Context.GetText(Resource.String.Lbl_Category492)}, // "Animals/Wildlife"},
                    {"493", Application.Context.GetText(Resource.String.Lbl_Category493)}, // "Arts"},
                    {"494", Application.Context.GetText(Resource.String.Lbl_Category494)}, // "Backgrounds/Textures"},
                    {"495", Application.Context.GetText(Resource.String.Lbl_Category495)}, // "Beauty/Fashion"},
                    {"496", Application.Context.GetText(Resource.String.Lbl_Category496)}, // "Buildings/Landmarks"},
                    {"497", Application.Context.GetText(Resource.String.Lbl_Category497)}, // "Business/Finance"},
                    {"498", Application.Context.GetText(Resource.String.Lbl_Category498)}, // "Celebrities"},
                    {"499", Application.Context.GetText(Resource.String.Lbl_Category499)}, // "Education"},
                    {"500", Application.Context.GetText(Resource.String.Lbl_Category500)}, // "Food and drink"},
                    {"501", Application.Context.GetText(Resource.String.Lbl_Category501)}, // "Healthcare/Medical"},
                    {"502", Application.Context.GetText(Resource.String.Lbl_Category502)}, // "Holidays"},
                    {"503", Application.Context.GetText(Resource.String.Lbl_Category503)}, // "Industrial"},
                    {"504", Application.Context.GetText(Resource.String.Lbl_Category504)}, // "Interiors"},
                    {"505", Application.Context.GetText(Resource.String.Lbl_Category505)}, // "Miscellaneous"},
                    {"506", Application.Context.GetText(Resource.String.Lbl_Category506)}, // "Nature"},
                    {"507", Application.Context.GetText(Resource.String.Lbl_Category507)}, // "Objects"},
                    {"508", Application.Context.GetText(Resource.String.Lbl_Category508)}, // "Parks/Outdoor"},
                    {"509", Application.Context.GetText(Resource.String.Lbl_Category509)}, // "People"},
                    {"510", Application.Context.GetText(Resource.String.Lbl_Category510)}, // "Religion"},
                    {"511", Application.Context.GetText(Resource.String.Lbl_Category511)}, // "Science"},
                    {"512", Application.Context.GetText(Resource.String.Lbl_Category512)}, // "Signs/Symbols"},
                    {"513", Application.Context.GetText(Resource.String.Lbl_Category513)}, // "Sports/Recreation"},
                    {"514", Application.Context.GetText(Resource.String.Lbl_Category514)}, // "Technology"},
                    {"515", Application.Context.GetText(Resource.String.Lbl_Category515)}, // "Transportation"},
                    {"516", Application.Context.GetText(Resource.String.Lbl_Category516)}, // "Vintage"},
                };

                return arrayAdapter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new Dictionary<string, string>();
            }
        }
         
        public static Dictionary<string, string> GetLicenseTypeStoreList()
        {
            try
            {
                var arrayAdapter = new Dictionary<string, string>
                {
                   {"rights_managed_license", Application.Context.GetText(Resource.String.Lbl_rights_managed_license)},
                   {"editorial_use_license", Application.Context.GetText(Resource.String.Lbl_editorial_use_license)},
                   {"royalty_free_license", Application.Context.GetText(Resource.String.Lbl_royalty_free_license)},
                   {"royalty_free_extended_license", Application.Context.GetText(Resource.String.Lbl_royalty_free_extended_license)},
                   {"creative_commons_license", Application.Context.GetText(Resource.String.Lbl_creative_commons_license)},
                   {"public_domain", Application.Context.GetText(Resource.String.Lbl_public_domain)},
                };

                return arrayAdapter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new Dictionary<string, string>();
            }
        }

    }
}
 