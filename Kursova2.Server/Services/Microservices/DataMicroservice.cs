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

        public void SaveChanges(string tableName, DataTable changes)
        {
            if (changes != null)
            {
                _dbHelper.UpdateDataTable($"SELECT * FROM {tableName}", changes);
            }
        }
    }
}