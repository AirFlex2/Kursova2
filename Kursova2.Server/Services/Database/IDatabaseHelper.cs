using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Kursova2.Services.Database
{
    // ИНТЕРФЕЙС БАЗЫ ДАННЫХ (DIP)
    public interface IDatabaseHelper
    {
        DataTable ExecuteQuery(string query, params SqlParameter[] parameters);
        int ExecuteNonQuery(string query, params SqlParameter[] parameters);
        object ExecuteScalar(string query, params SqlParameter[] parameters);
        void UpdateDataTable(string selectQuery, DataTable changes);
        void ExecuteInTransaction(Action<SqlCommand> transactionalAction);
    }

    // РЕАЛИЗАЦИЯ ПОДКЛЮЧЕНИЯ (Решает проблему DRY - весь SQL-бойлерплейт здесь)
    public class SqlDatabaseHelper : IDatabaseHelper
    {
        private readonly string _connectionString;

        public SqlDatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                var adapter = new SqlDataAdapter(command);

                // КРИТИЧЕСКИ ВАЖНО: загружаем Primary Key из БД для правильного маппинга
                adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                connection.Open();
                return command.ExecuteScalar();
            }
        }

        public void UpdateDataTable(string selectQuery, DataTable changes)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var adapter = new SqlDataAdapter(selectQuery, connection);

                // КРИТИЧЕСКИ ВАЖНО: SqlCommandBuilder не может сгенерировать UPDATE без ключей
                adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                var builder = new SqlCommandBuilder(adapter);
                adapter.Update(changes);
            }
        }

        //обертка для выполнения сложных операций в транзакции
        public void ExecuteInTransaction(Action<SqlCommand> transactionalAction)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    try
                    {
                        transactionalAction(command);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}