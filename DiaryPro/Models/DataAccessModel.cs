using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Windows.Storage;

namespace DiaryPro.Models
{
    class DataAccessModel
    {
        public static string DB_FILENAME = "SqliteSample.db";
        public static string DB_NOTE_TABLE_NAME = "NoteTable";
        public static string DB_NOTE_TABLE_PKEY = "NoteTableID";
        public static string DB_NOTE_TABLE_DATE = "NoteTableDate";
        public static string DB_NOTE_TABLE_HEADER = "NoteTableHeader";
        public static string DB_NOTE_TABLE_CONTENT = "NoteTableContent";
        public static string DB_IMG_TABLE_NAME = "ImgTable";
        public static string DB_IMG_TABLE_PKEY = "ImgTableID";
        public static string DB_IMG_TABLE_BYTES = "ImgTableBytes";
        public static string DB_IMG_TABLE_DESCRIPT = "ImgTableDes";

        public async static void InitializeDatabase()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(DB_FILENAME, CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_FILENAME);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                List<String> tableCommands = new List<string>();

                String str = "CREATE TABLE IF NOT " +
                    "EXISTS " + DB_NOTE_TABLE_NAME + " (" + 
                    DB_NOTE_TABLE_PKEY + " INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                    DB_NOTE_TABLE_DATE + " NVARCHAR NULL, " +
                    DB_NOTE_TABLE_HEADER + " NVARCHAR NULL, " +
                    DB_NOTE_TABLE_CONTENT + " NVARCHAR NULL" + ")";

                tableCommands.Add(str);

                str = "CREATE TABLE IF NOT " +
                    "EXISTS " + DB_IMG_TABLE_NAME + " (" +
                    DB_IMG_TABLE_PKEY + " INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                    DB_IMG_TABLE_BYTES + " BLOB NULL, " +
                    DB_IMG_TABLE_DESCRIPT + " NVARCHAR NULL, " +
                    DB_NOTE_TABLE_PKEY + " INTEGER, " +
                    "FOREIGN KEY (" + DB_NOTE_TABLE_PKEY + ") REFERENCES " + DB_NOTE_TABLE_NAME + " (" + DB_NOTE_TABLE_PKEY + ")" + ")";

                tableCommands.Add(str);

                foreach (String tableCommand in tableCommands)
                {
                    Trace.WriteLine("DB Command: " + tableCommand);
                    SqliteCommand createTable = new SqliteCommand(tableCommand, db);
                    createTable.ExecuteReader();
                }

                db.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public static int AddData(NoteModel note)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_FILENAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = "INSERT INTO " + DB_NOTE_TABLE_NAME + " VALUES (NULL, @" + DB_NOTE_TABLE_DATE
                    + ", @" + DB_NOTE_TABLE_HEADER
                    + ", @" + DB_NOTE_TABLE_CONTENT
                    + "); ";
                insertCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_DATE, note.date);
                insertCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_HEADER, note.header);
                insertCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_CONTENT, note.content);

                using (var query = insertCommand.ExecuteReader()) { }

                // AUTOINCREMENTのDB_NOTE_TABLE_PKEYを取得
                int lastInsertID = -1;
                insertCommand.CommandText = "SELECT last_insert_rowid();";
                insertCommand.Parameters.Clear();
                using (var query = insertCommand.ExecuteReader())
                {
                    while (query.Read())
                    {
                        lastInsertID = query.GetInt32(0);
                    }
                }

                db.Close();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                foreach(ImgModel image in note.images)
                {
                    db.Open();
                    insertCommand.Connection = db;
                    insertCommand.CommandText = "INSERT INTO " + DB_IMG_TABLE_NAME + " VALUES (NULL, @" + DB_IMG_TABLE_BYTES
                    + ", @" + DB_IMG_TABLE_DESCRIPT
                    + ", @" + DB_NOTE_TABLE_PKEY
                    + "); ";
                    insertCommand.Parameters.Clear();
                    insertCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_PKEY, lastInsertID);
                    insertCommand.Parameters.AddWithValue("@" + DB_IMG_TABLE_DESCRIPT, image.descript);
                    insertCommand.Parameters.AddWithValue("@" + DB_IMG_TABLE_BYTES, image.img);
                    using (var query = insertCommand.ExecuteReader()) { }
                    db.Close();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                return lastInsertID;
            }
        }

        public static ObservableCollection<NoteModel> GetAllData()
        {
            ObservableCollection<NoteModel> notes = new ObservableCollection<NoteModel>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_FILENAME);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT * FROM " + DB_NOTE_TABLE_NAME +
                    " INNER JOIN " + DB_IMG_TABLE_NAME +
                    " ON " + DB_NOTE_TABLE_NAME + "." + DB_NOTE_TABLE_PKEY + " = "
                    + DB_IMG_TABLE_NAME + "." + DB_NOTE_TABLE_PKEY, db);

                int oldNoteTableID = -1;
                int newNoteTableID = -1;
                NoteModel note = new NoteModel();
                using (var query = selectCommand.ExecuteReader())
                {
                    while (query.Read())
                    {
                        var image = new ImgModel();
                        image.descript = query.GetString(6);
                        image.img = new byte[query.GetStream(5).Length];
                        query.GetStream(5).CopyTo(new MemoryStream(image.img, true));
                        newNoteTableID = query.GetInt32(0);
                        if(newNoteTableID != oldNoteTableID)
                        {
                            oldNoteTableID = newNoteTableID;
                            note = new NoteModel();
                            notes.Add(note);
                            note.date = query.GetString(1);
                            note.header = query.GetString(2);
                            note.content = query.GetString(3);
                            note.images.Add(image);
                        }
                        else
                        {
                            note.images.Add(image);
                        }    
                    }
                }
                db.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return notes;
            }
        }

        public static ObservableCollection<NoteModel> GetData(int limit, int offset)
        {
            ObservableCollection<NoteModel> notes = new ObservableCollection<NoteModel>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_FILENAME);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT * FROM " + DB_NOTE_TABLE_NAME +
                    " ORDER BY " + DB_NOTE_TABLE_DATE + " DESC " +
                    " LIMIT " + limit +
                    " OFFSET " + offset, db);

                using (var query = selectCommand.ExecuteReader()) 
                {
                    while (query.Read())
                    {
                        NoteModel note = new NoteModel();
                        note.ID = query.GetInt32(0);
                        note.date = query.GetString(1);
                        note.header = query.GetString(2);
                        note.content = query.GetString(3);
                        notes.Add(note);
                    }
                }

                db.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                foreach(NoteModel note in notes)
                {
                    db.Open();

                    selectCommand = new SqliteCommand
                    ("SELECT * FROM " + DB_IMG_TABLE_NAME +
                    " WHERE " + DB_NOTE_TABLE_PKEY + " = " + note.ID
                    , db);

                    using (var query = selectCommand.ExecuteReader())
                    {
                        while(query.Read())
                        {
                            ImgModel image = new ImgModel();
                            image.descript = query.GetString(2);
                            image.img = new byte[query.GetStream(1).Length];
                            query.GetStream(1).CopyTo(new MemoryStream(image.img, true));
                            note.images.Add(image);
                        }    
                    }

                    db.Close();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }    
            }
            return notes;
        }

        public static void DeleteData(int noteID)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_FILENAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = "DELETE FROM " + DB_NOTE_TABLE_NAME + 
                    " WHERE " + DB_NOTE_TABLE_PKEY + " = @" + DB_NOTE_TABLE_PKEY;
                insertCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_PKEY, noteID);

                using (var query = insertCommand.ExecuteReader()) { }

                db.Close();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
