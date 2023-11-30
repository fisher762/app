using Android.Views;
using PixelPhotoClient.Classes.Messages;

namespace PixelPhoto.Helpers.Model
{
   public interface IOnClickListenerSelectedMessages
    {
        void OnItemClick(View view, MessageDataObject obj, int pos);
        void OnItemLongClick(View view, MessageDataObject obj, int pos);

    }
}