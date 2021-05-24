using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Windows.Storage;

namespace DiaryPro.Models
{
    class DataAccess
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
                    DB_NOTE_TABLE_PKEY + " INTEGER PRIMARY KEY, " +
                    DB_NOTE_TABLE_DATE + " NVARCHAR NULL, " +
                    DB_NOTE_TABLE_HEADER + " NVARCHAR NULL, " +
                    DB_NOTE_TABLE_CONTENT + " NVARCHAR NULL" + ")";

                tableCommands.Add(str);

                str = "CREATE TABLE IF NOT " +
                    "EXISTS " + DB_IMG_TABLE_NAME + " (" +
                    DB_IMG_TABLE_PKEY + " INTEGER PRIMARY KEY, " +
                    DB_IMG_TABLE_BYTES + " BLOB NULL, " +
                    DB_NOTE_TABLE_PKEY + " INTEGER, " +
                    "FOREIGN KEY (" + DB_NOTE_TABLE_PKEY + ") REFERENCES " + DB_NOTE_TABLE_NAME + " (" + DB_NOTE_TABLE_PKEY + ")" + ")";

                tableCommands.Add(str);

                foreach (String tableCommand in tableCommands)
                {
                    Trace.WriteLine("DB Command: " + tableCommand);
                    SqliteCommand createTable = new SqliteCommand(tableCommand, db);
                    createTable.ExecuteReader();
                }
            }
        }

        public static void AddData(NoteModel note)
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
                insertCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_HEADER, note.date);
                insertCommand.Parameters.AddWithValue("@" + DB_NOTE_TABLE_CONTENT, note.date);

                insertCommand.ExecuteReader();

                db.Close();
            }

        }

        public static List<String> GetData()
        {
            List<String> entries = new List<string>();

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

                //https://www.tutorialspoint.com/sqlite/sqlite_limit_clause.htm

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    entries.Add(query.GetString(0));
                }

                db.Close();
            }

            return entries;
        }
    }
}
