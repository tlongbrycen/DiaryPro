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
        public List<byte[]> images;

        public NoteModel()
        {
            images = new List<byte[]>();
        }
    }
}
