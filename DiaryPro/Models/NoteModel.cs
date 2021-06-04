using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiaryPro.Models
{
    class NoteModel
    {
        public int ID;
        public string header;
        public string date;
        public string content;
        public List<ImgModel> images;

        public NoteModel()
        {
            ID = -1;
            header = "";
            date = "";
            content = "";
            images = new List<ImgModel>();
        }

        public NoteModel(string p_date)
        {
            ID = -1;
            header = "";
            date = p_date;
            content = "";
            images = new List<ImgModel>();
        }
    }

    class ImgModel
    {
        public int ID;
        public string descript;
        public byte[] img;

        public ImgModel()
        {
            img = new byte[] {};
        }
    }
}
