using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PixelPhoto.SQLite;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.Classes.Messages;
using PixelPhotoClient.GlobalClass;

namespace PixelPhoto.Helpers.Utils
{
    public static class ListUtils
    {
        //############# DON'T MODIFY HERE #############
        //List Items Declaration 
        //*********************************************************
        public static ObservableCollection<DataTables.LoginTb> DataUserLoginList = new ObservableCollection<DataTables.LoginTb>();
        public static GetSettingsObject.Config SettingsSiteList;
        public static ObservableCollection<UserDataObject> MyProfileList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<ChatDataObject> ChatList = new ObservableCollection<ChatDataObject>();
        public static ObservableCollection<UserDataObject> FollowingLocalList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<UserDataObject> FollowersLocalList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<PostsObject> FavoritesLocalList = new ObservableCollection<PostsObject>();
        public static ObservableCollection<PostsObject> BlockedUsersLocalList = new ObservableCollection<PostsObject>();
        public static ObservableCollection<FundingDataObject> FundingList = new ObservableCollection<FundingDataObject>();

        public static void ClearAllList()
        {
            try
            {
                DataUserLoginList.Clear();
                SettingsSiteList = null!;
                MyProfileList.Clear();
                ChatList.Clear();
                FollowingLocalList.Clear();
                FollowersLocalList.Clear();
                FavoritesLocalList.Clear();
                BlockedUsersLocalList.Clear();
                FundingList.Clear(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void AddRange<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        {
            try
            {
                items.ToList().ForEach(collection.Add);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            var list = new List<List<T>>();

            for (var i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }

        public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int n)
        {
            var enumerable = source as T[] ?? source.ToArray();

            return enumerable.Skip(Math.Max(0, enumerable.Count() - n));
        } 
    }
}