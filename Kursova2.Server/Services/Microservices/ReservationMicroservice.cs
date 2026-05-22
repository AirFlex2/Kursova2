using System;
using System.Data;
using System.Data.SqlClient;
using Kursova2.Services.Database;

namespace Kursova2.Services.Microservices
{
    public class ReservationMicroservice : IReservationService
    {
        private readonly IDatabaseHelper _dbHelper;

        public ReservationMicroservice(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public string ReserveBook(int bookId, string readerName)
        {
            try
            {
                string resultMessage = "";

                _dbHelper.ExecuteInTransaction(cmd =>
                {
                    cmd.CommandText = "SELECT ReaderID FROM Reader WHERE ReaderName = @name";
                    cmd.Parameters.AddWithValue("@name", readerName);
                    var readerIdObj = cmd.ExecuteScalar();

                    if (readerIdObj == null)
                    {
                        resultMessage = "Читача з таким ПІБ не знайдено. Будь ласка, зареєструйтесь у системі.";
                        return;
                    }
                    int readerId = Convert.ToInt32(readerIdObj);

                    cmd.CommandText = "SELECT AvailableCount FROM Book WHERE BookID = @bookId";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@bookId", bookId);
                    var availableObj = cmd.ExecuteScalar();

                    if (availableObj == null || Convert.ToInt32(availableObj) <= 0)
                    {
                        resultMessage = "На жаль, ця книга наразі недоступна для бронювання (немає вільних копій).";
                        return;
                    }

                    cmd.CommandText = @"
                        DECLARE @newId INT;
                        SELECT @newId = ISNULL(MAX(ReservationID), 0) + 1 FROM Reservation;
                        INSERT INTO Reservation (ReservationID, DateOfReservation, BookID, ReaderID) 
                        VALUES (@newId, @date, @bId, @rId)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@bId", bookId);
                    cmd.Parameters.AddWithValue("@rId", readerId);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "UPDATE Book SET AvailableCount = AvailableCount - 1 WHERE BookID = @bookId";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@bookId", bookId);
                    cmd.ExecuteNonQuery();

                    resultMessage = "Книгу успішно заброньовано!";
                });

                return resultMessage;
            }
            catch (Exception ex)
            {
                return "Помилка бронювання: " + ex.Message;
            }
        }

        public DataTable GetUserReservations(string readerName)
        {
            var query = @"
                SELECT r.ReservationID, b.BookName as 'Книга', r.DateOfReservation as 'Дата бронювання'
                FROM Reservation r
                JOIN Book b ON r.BookID = b.BookID
                JOIN Reader rdr ON r.ReaderID = rdr.ReaderID
                WHERE rdr.ReaderName = @name";

            return _dbHelper.ExecuteQuery(query, new SqlParameter("@name", readerName));
        }

        public string CancelReservation(int reservationId)
        {
            try
            {
                _dbHelper.ExecuteInTransaction(cmd =>
                {
                    cmd.CommandText = "SELECT BookID FROM Reservation WHERE ReservationID = @resId";
                    cmd.Parameters.AddWithValue("@resId", reservationId);
                    var bookIdObj = cmd.ExecuteScalar();

                    if (bookIdObj == null)
                        throw new Exception("Бронювання не знайдено!");

                    int bookId = Convert.ToInt32(bookIdObj);

                    cmd.CommandText = "DELETE FROM Reservation WHERE ReservationID = @resId";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "UPDATE Book SET AvailableCount = AvailableCount + 1 WHERE BookID = @bookId";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@bookId", bookId);
                    cmd.ExecuteNonQuery();
                });

                return "Бронювання успішно скасовано, книгу повернуто!";
            }
            catch (Exception ex)
            {
                return "Помилка при скасуванні: " + ex.Message;
            }
        }
    }
}