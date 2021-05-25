using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiaryPro.Models
{
    class NoteModel
    {
        public string header;
        public string date;
        public string content;
        public List<ImgModel> images;

        public NoteModel()
        {
            images = new List<ImgModel>();
        }
    }

    class ImgModel
    {
        public string descript;
        public byte[] img;

        public ImgModel()
        {
            img = new byte[] {};
        }
    }
}
