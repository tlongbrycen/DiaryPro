using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace DiaryPro.Models
{
    class UtilityModel
    {
        /// <summary>
        /// Convert a StorageFile to a byte array
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<byte[]> FileToByteAsync(StorageFile file)
        {
            if (file is null) return new byte[] { 0x00 };
            byte[] result;
            using (Stream stream = await file.OpenStreamForReadAsync())
            {
                using (var memoryStream = new MemoryStream())
                {

                    stream.CopyTo(memoryStream);
                    result = memoryStream.ToArray();
                }
            }
            return result;
        }

        /// <summary>
        /// Convert a byte array to a BitmapImage
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static BitmapImage BytesToImage(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            var image = new BitmapImage();
            using (InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream())
            {
                using (DataWriter outputStream = new DataWriter(memoryStream.GetOutputStreamAt(0)))
                {
                    outputStream.WriteBytes((byte[])data);
                    outputStream.StoreAsync().GetResults();
                }
                image.SetSource(memoryStream);
            }
            return image;
        }
    }
}
