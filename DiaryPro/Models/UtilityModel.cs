using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

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
    }
}
