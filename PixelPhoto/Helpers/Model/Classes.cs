using System;
using System.Collections.Generic;
using AFollestad.MaterialDialogs;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Store;
using PixelPhotoClient.GlobalClass;

namespace PixelPhoto.Helpers.Model
{
    public class Classes
    {
        public class ViewPagerStrings
        {
            public string Description;
            public string Header;
        }

        public class PostType
        {
            public int Id { get; set; }
            public string TypeText { get; set; }
            public int Image { get; set; }
            public string ImageColor { get; set; }
        }
        
        public class ExploreClass
        {
            public int Id { get; set; }
            public ItemType Type { get; set; }
            public List<UserDataObject> UserList { get; set; }
            public List<StoreDataObject> StoreList { get; set; }
            public List<PostsObject> PostList { get; set; }

        }
         
        public enum TypePostEnum
        {
            Image,
            Video,
            Mention,
            Camera,
            Gif,
            EmbedVideo
        }
         
        public enum ItemType
        {
            User = 100, Store = 200, Post = 300, EmptyPage = 400 , Featured = 500
        }
    }

    #region MaterialDialog

    public class MyMaterialDialog : Java.Lang.Object, MaterialDialog.ISingleButtonCallback
    {
        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    #endregion
}