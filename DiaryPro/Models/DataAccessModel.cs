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
                    DB_NOTE_TABLE_PKEY + " INTEGER " + ")";

                tableCommands.Add(str);

                foreach (String tableCommand in tableCommands)
                {
                    Trace.WriteLine("DB Command: " + tableCommand);
                    using (SqliteCommand createTableCommand = new SqliteCommand(tableCommand, db))
                    {
                        using (var query = createTableCommand.ExecuteReader()) { }
                        createTableCommand.Dispose();
                    }
                    
                }
                db.Close();
                db.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public static int AddData(NoteModel note)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_FILENAME);
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                int lastInsertID = -1;

                using (SqliteCommand insertCommand = new SqliteCommand())
                {
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
                    insertCommand.CommandText = "SELECT last_insert_rowid();";
                    insertCommand.Parameters.Clear();
                    using (var query = insertCommand.ExecuteReader())
                    {
                        while (query.Read())
                        {
                            lastInsertID = query.GetInt32(0);
                        }
                    }

                    insertCommand.Dispose();
                }

                db.Close();
                db.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                foreach (ImgModel image in note.images)
                {
                    db.Open();

                    using (SqliteCommand insertCommand = new SqliteCommand())
                    {
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
                        insertCommand.Dispose();
                    }
                    db.Close();
                    db.Dispose();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                return lastInsertID;
            }
        }

        public static int AddData(ImgModel image, int noteID)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_FILENAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                int lastInsertID = -1;

                using (SqliteCommand insertCommand = new SqliteCommand())
                {
                    insertCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    insertCommand.CommandText = "INSERT INTO " + DB_IMG_TABLE_NAME + " VALUES (NULL, @" + DB_IMG_TABLE_BYTES
                        + ", @" + DB_IMG_TABLE_DESCRIPT
                        + ", @" + DB_NOTE_TABLE_PKEY
                        + "); ";
                    insertCommand.Parameters.AddWithValue("@" + DB_IMG_TABLE_BYTES, image.img);
                    insertCommand.Parameters.AddWithValue("@" + DB_IMG_TABLE_DESCRIPT, image.descript);
                    insertCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_PKEY, noteID);

                    insertCommand.ExecuteNonQuery();

                    // AUTOINCREMENTのDB_IMG_TABLE_PKEYを取得

                    insertCommand.CommandText = "SELECT last_insert_rowid();";
                    insertCommand.Parameters.Clear();
                    using (var query = insertCommand.ExecuteReader())
                    {
                        while (query.Read())
                        {
                            lastInsertID = query.GetInt32(0);
                        }
                    }

                    insertCommand.Dispose();
                }

                db.Close();
                db.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();

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
                        if (newNoteTableID != oldNoteTableID)
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
                selectCommand.Dispose();
                db.Close();
                db.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return notes;
            }
        }

        public static ObservableCollection<NoteModel> GetData(int limit, int offset)
        {
            ObservableCollection<NoteModel> notes = new ObservableCollection<NoteModel>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_FILENAME);
            using (SqliteConnection db =  new SqliteConnection($"Filename={dbpath}"))
            {                
                using (SqliteCommand selectCommand = new SqliteCommand())
                {
                    selectCommand.Connection = db;
                    selectCommand.CommandText = "SELECT * FROM " + DB_NOTE_TABLE_NAME + " ORDER BY " + DB_NOTE_TABLE_DATE + " DESC " + " LIMIT " + limit + " OFFSET " + offset;
                    db.Open();
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
                    foreach (NoteModel note in notes)
                    {
                        selectCommand.CommandText = "SELECT * FROM " + DB_IMG_TABLE_NAME + " WHERE " + DB_NOTE_TABLE_PKEY + " = " + note.ID;
                        using (var query = selectCommand.ExecuteReader())
                        {
                            while (query.Read())
                            {
                                ImgModel image = new ImgModel();
                                image.descript = query.GetString(2);
                                //image.img = new byte[query.GetStream(1).Length];
                                //query.GetStream(1).CopyTo(new MemoryStream(image.img, true));
                                //byte[] buffer = GetBytes(query);
                                image.img = GetBytes(query,1);
                                note.images.Add(image);
                            }
                        }
                    }
                }
                db.Dispose();
                db.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return notes;
        }

        static byte[] GetBytes(SqliteDataReader reader, int column)
        {
            const int CHUNK_SIZE = 2 * 1024;
            byte[] buffer = new byte[CHUNK_SIZE];
            long bytesRead;
            long fieldOffset = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                while ((bytesRead = reader.GetBytes(column, fieldOffset, buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, (int)bytesRead);
                    fieldOffset += bytesRead;
                }
                return stream.ToArray();
            }
        }

        public static void DeleteData(int noteID)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_FILENAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                using (SqliteCommand deleteCommand = new SqliteCommand())
                {
                    deleteCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    deleteCommand.CommandText = "DELETE FROM " + DB_IMG_TABLE_NAME +
                        " WHERE " + DB_NOTE_TABLE_PKEY + " = @" + DB_NOTE_TABLE_PKEY;
                    deleteCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_PKEY, noteID);

                    using (var query = deleteCommand.ExecuteReader()) { }

                    deleteCommand.Dispose();
                }

                db.Close();
                db.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                db.Open();

                using (SqliteCommand deleteCommand = new SqliteCommand())
                {
                    deleteCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    deleteCommand.CommandText = "DELETE FROM " + DB_NOTE_TABLE_NAME +
                    " WHERE " + DB_NOTE_TABLE_PKEY + " = @" + DB_NOTE_TABLE_PKEY;
                    deleteCommand.Parameters.Clear();
                    deleteCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_PKEY, noteID);

                    using (var query = deleteCommand.ExecuteReader()) { }

                    deleteCommand.Dispose();
                }

                db.Close();
                db.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
