using DiaryPro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiaryPro.Cache
{
    class ImageCache
    {
        private static List<ImgModel> changedImages;

        public static void Init()
        {
            changedImages = new List<ImgModel>();
        }

        public static void Clear()
        {
            changedImages.Clear();
        }

        public static void Add(ImgModel img)
        {
            foreach (ImgModel changedImage in changedImages)
            {
                if (changedImage.ID == img.ID)
                {
                    changedImage.descript = img.descript;
                    return;
                }
            }
            changedImages.Add(img);
        }

        public static void Save()
        {
            foreach (ImgModel changedImage in changedImages)
            {
                DataAccessModel.UpdateData(changedImage);
            }
        }
    }
}
