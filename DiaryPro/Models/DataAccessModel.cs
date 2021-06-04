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

        public static ObservableCollection<NoteModel> GetData(ObservableCollection<NoteModel> notes, int limit, int offset,
            string[] filterHeaders = null)
        {
            notes.Clear();
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
                            if(filterHeaders == null)
                            {
                                notes.Add(note);
                            }
                            else
                            {
                                bool matched = false;
                                string[] headerWords = note.header.Split(' ');
                                foreach(string headerWord in headerWords)
                                {
                                    foreach(string filterHeader in filterHeaders)
                                    {
                                        if(headerWord.ToUpper().Equals(filterHeader.ToUpper()))
                                        {
                                            notes.Add(note);
                                            matched = true;
                                            break;
                                        }
                                    }
                                    if (matched) break;
                                }
                            }
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
                                image.ID = query.GetInt32(0);
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

        public static void DeleteData(NoteModel note)
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
                    deleteCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_PKEY, note.ID);

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
                    deleteCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_PKEY, note.ID);

                    using (var query = deleteCommand.ExecuteReader()) { }
                }

                db.Close();
                db.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public static void DeleteData(ImgModel image)
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
                        " WHERE " + DB_IMG_TABLE_PKEY + " = @" + DB_IMG_TABLE_PKEY;
                    deleteCommand.Parameters.AddWithValue("@" + DB_IMG_TABLE_PKEY, image.ID);

                    using (var query = deleteCommand.ExecuteReader()) { }

                    deleteCommand.Dispose();
                }

                db.Close();
                db.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public static void UpdateData(NoteModel note)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_FILENAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                using (SqliteCommand updateCommand = new SqliteCommand())
                {
                    updateCommand.Connection = db;
                    updateCommand.CommandText = "UPDATE " + DB_NOTE_TABLE_NAME +
                        " SET " + DB_NOTE_TABLE_HEADER + " = @" + DB_NOTE_TABLE_HEADER +
                        ", " + DB_NOTE_TABLE_CONTENT + " = @" + DB_NOTE_TABLE_CONTENT +
                        " WHERE " + DB_NOTE_TABLE_PKEY + " = @" + DB_NOTE_TABLE_PKEY;
                    updateCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_HEADER, note.header);
                    updateCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_CONTENT, note.content);
                    updateCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_PKEY, note.ID);
                    Trace.WriteLine(updateCommand);
                    using (var query = updateCommand.ExecuteReader()) { }
                }
                db.Close();
                db.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public static void UpdateData(ImgModel image)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_FILENAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                using (SqliteCommand updateCommand = new SqliteCommand())
                {
                    updateCommand.Connection = db;
                    updateCommand.CommandText = "UPDATE " + DB_IMG_TABLE_NAME +
                        " SET " + DB_IMG_TABLE_DESCRIPT + " = @" + DB_IMG_TABLE_DESCRIPT +
                        " WHERE " + DB_IMG_TABLE_PKEY + " = @" + DB_IMG_TABLE_PKEY;
                    updateCommand.Parameters.AddWithValue("@" + DB_IMG_TABLE_DESCRIPT, image.descript);
                    updateCommand.Parameters.AddWithValue("@" + DB_IMG_TABLE_PKEY, image.ID);
                    Trace.WriteLine(updateCommand);
                    using (var query = updateCommand.ExecuteReader()) { }
                }
                db.Close();
                db.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
