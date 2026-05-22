using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Kursova2.Services.Database;

namespace Kursova2.Services.Microservices
{
    public class DataMicroservice : IDataService
    {
        private readonly IDatabaseHelper _dbHelper;

        public DataMicroservice(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<string> GetAvailableTables(bool isAdmin)
        {
            var tables = new List<string>();
            if (isAdmin)
                tables.AddRange(new[] { "Book", "Author", "Reader", "Reservation", "Loan", "Fine", "AuthorBook" });
            else
                tables.AddRange(new[] { "Book", "Author" });

            return tables;
        }

        public DataTable GetTableData(string tableName)
        {
            return _dbHelper.ExecuteQuery($"SELECT * FROM {tableName}");
        }

        public DataTable SearchInTable(string tableName, string columnName, string keyword)
        {
            var query = $"SELECT * FROM {tableName} WHERE {columnName} LIKE @kw";
            return _dbHelper.ExecuteQuery(query, new SqlParameter("@kw", $"%{keyword}%"));
        }

        public void SaveChanges(string tableName, string xmlChanges)
        {
            if (!string.IsNullOrWhiteSpace(xmlChanges))
            {
                // 1. Запрашиваем пустую таблицу из БД, чтобы получить структуру
                DataTable schemaTable = _dbHelper.ExecuteQuery($"SELECT * FROM {tableName} WHERE 1=0");

                // Устанавливаем имя таблицы
                schemaTable.TableName = tableName;

                // 2. Оборачиваем таблицу в DataSet
                DataSet ds = new DataSet();
                ds.Tables.Add(schemaTable);

                // ИЗМЕНЕНИЕ ЗДЕСЬ: Отключаем локальную проверку ограничений
                // Это позволит загрузить новые строки без ID или с пустыми значениями,
                // чтобы база данных сама применила к ним свои правила (например, IDENTITY).
                ds.EnforceConstraints = false;

                // 3. Читаем DiffGram из строки
                using (var sr = new System.IO.StringReader(xmlChanges))
                {
                    ds.ReadXml(sr, XmlReadMode.DiffGram);
                }

                // Убедимся, что после чтения XML имя таблицы сохранилось 
                ds.Tables[0].TableName = tableName;

                // 4. Отправляем таблицу с восстановленными изменениями в базу
                _dbHelper.UpdateDataTable($"SELECT * FROM {tableName}", ds.Tables[0]);
            }
        }
    }
}