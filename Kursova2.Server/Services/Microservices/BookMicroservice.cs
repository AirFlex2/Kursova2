using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Kursova2.Models;
using Kursova2.Services.Database;

namespace Kursova2.Services.Microservices
{
    public class BookMicroservice : IBookService
    {
        private readonly IDatabaseHelper _dbHelper;

        public BookMicroservice(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<Book> GetAllBooks()
        {
            var table = _dbHelper.ExecuteQuery("SELECT * FROM Book");
            return MapToBooks(table);
        }

        public List<Book> SearchBooksByName(string title)
        {
            var query = "SELECT * FROM Book WHERE BookName LIKE @title";
            var table = _dbHelper.ExecuteQuery(query, new SqlParameter("@title", $"%{title}%"));
            return MapToBooks(table);
        }

        private List<Book> MapToBooks(DataTable table)
        {
            var books = new List<Book>();
            foreach (DataRow row in table.Rows)
            {
                books.Add(new Book
                {
                    BookID = row["BookID"] != DBNull.Value ? Convert.ToInt32(row["BookID"]) : 0,
                    BookName = row["BookName"].ToString(),
                    YearOfPub = row["YearOfPub"].ToString(),
                    Genre = row["Genre"].ToString(),
                    AvailableCount = row["AvailableCount"] != DBNull.Value ? Convert.ToInt32(row["AvailableCount"]) : 0
                });
            }
            return books;
        }

        public bool CheckAvailability(int bookId)
        {
            var query = "SELECT AvailableCount FROM Book WHERE BookID = @BookID";
            var result = _dbHelper.ExecuteScalar(query, new SqlParameter("@BookID", bookId));
            return result != DBNull.Value && Convert.ToInt32(result) > 0;
        }

        public List<AuthorStatistic> GetAuthorStatistics()
        {
            var stats = new List<AuthorStatistic>();
            var query = @"SELECT a.AuthorName, COUNT(*) AS BookCount 
                          FROM Author a JOIN AuthorBook ab ON a.AuthorID = ab.AuthorID 
                          JOIN Book b ON ab.BookID = b.BookID GROUP BY a.AuthorName;";

            var table = _dbHelper.ExecuteQuery(query);
            foreach (DataRow row in table.Rows)
            {
                stats.Add(new AuthorStatistic
                {
                    AuthorName = row["AuthorName"].ToString(),
                    BookCount = Convert.ToInt32(row["BookCount"])
                });
            }
            return stats;
        }
    }
}