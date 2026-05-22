using System.Data;
using Microsoft.Data.SqlClient;
using Kursova2.Services.Database;

namespace Kursova2.Services.Microservices
{
    public class AnalyticsMicroservice : IAnalyticsService
    {
        private readonly IDatabaseHelper _dbHelper;

        // Внедрение зависимости помощника БД (DI)
        public AnalyticsMicroservice(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public DataTable GetBooksGroupedByGenre()
        {
            var query = "SELECT Genre, COUNT(*) as Count FROM Book GROUP BY Genre";
            return _dbHelper.ExecuteQuery(query);
        }

        public DataTable GetBookRankings()
        {
            var query = "SELECT BookName, AvailableCount, DENSE_RANK() OVER(ORDER BY AvailableCount DESC) as Rank FROM Book";
            return _dbHelper.ExecuteQuery(query);
        }

        public DataTable GetBooksByAuthors()
        {
            var query = @"
                SELECT a.AuthorName, b.BookName 
                FROM Author a 
                JOIN AuthorBook ab ON a.AuthorID = ab.AuthorID 
                JOIN Book b ON ab.BookID = b.BookID";
            return _dbHelper.ExecuteQuery(query);
        }

        public DataTable GetFinesSummary()
        {
            var query = @"
                SELECT r.ReaderName as 'Читач', 
                       SUM(CAST(f.Amount AS DECIMAL(10,2))) as 'Сума штрафів' 
                FROM Reader r 
                JOIN Loan l ON r.ReaderID = l.ReaderID 
                JOIN Fine f ON l.LoanID = f.LoanID 
                GROUP BY r.ReaderName";
            return _dbHelper.ExecuteQuery(query);
        }

        public DataTable GetUserFines(string readerName)
        {
            var query = @"
                SELECT b.BookName as 'Книга', f.Amount as 'Сума штрафу', f.DateIssued as 'Дата виписування', f.PaidStatus as 'Статус Оплати'
                FROM Fine f
                JOIN Loan l ON f.LoanID = l.LoanID
                JOIN Reader r ON l.ReaderID = r.ReaderID
                JOIN Book b ON l.BookID = b.BookID
                WHERE r.ReaderName = @name";

            return _dbHelper.ExecuteQuery(query, new SqlParameter("@name", readerName));
        }
    }
}