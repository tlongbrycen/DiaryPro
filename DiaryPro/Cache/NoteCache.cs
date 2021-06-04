using DiaryPro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiaryPro.Cache
{
    class NoteCache
    {
        private static List<NoteModel> changedNotes;

        public static void Init()
        {
            changedNotes = new List<NoteModel>();
        }

        public static void Clear()
        {
            changedNotes.Clear();
        }

        public static void Add(NoteModel note)
        {
            foreach(NoteModel changedNote in changedNotes)
            {
                if(changedNote.ID == note.ID)
                {
                    changedNote.header = note.header;
                    changedNote.content = note.content;
                    changedNote.date = note.date;
                    changedNote.images = note.images;
                    return;
                }
            }
            changedNotes.Add(note);
        }

        public static void Save()
        {
            foreach (NoteModel changedNote in changedNotes)
            {
                DataAccessModel.UpdateData(changedNote);
            }
        }
    }
}
