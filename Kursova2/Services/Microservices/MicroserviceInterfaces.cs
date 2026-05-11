using System.Collections.Generic;
using System.Data;
using Kursova2.Models;

namespace Kursova2.Services.Microservices
{
    // Внедрение SOLID (DIP) - Интерфейсы для микросервисов
    public interface IAuthService
    {
        bool RegisterReader(string fullName, string email, string address, string homePhone, string workPhone);
        DataTable GetReaderInfo(string readerName);
    }

    public interface IAnalyticsService
    {
        DataTable GetBooksGroupedByGenre();
        DataTable GetBookRankings();
        DataTable GetBooksByAuthors();
        DataTable GetFinesSummary();
        DataTable GetUserFines(string readerName);
    }

    public interface IDataService
    {
        List<string> GetAvailableTables(bool isAdmin);
        DataTable GetTableData(string tableName);
        DataTable SearchInTable(string tableName, string columnName, string keyword);
        void SaveChanges(string tableName, DataTable changes);
    }

    public interface IReservationService
    {
        string ReserveBook(int bookId, string readerName);
        DataTable GetUserReservations(string readerName);
        string CancelReservation(int reservationId);
    }

    public interface IBookService
    {
        List<Book> GetAllBooks();
        List<Book> SearchBooksByName(string title);
        bool CheckAvailability(int bookId);
        List<AuthorStatistic> GetAuthorStatistics();
    }
}