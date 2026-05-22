using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Kursova2.Services.Database;
using Kursova2.Services.Strategies;

namespace Kursova2.Services.Microservices
{
    public class LoanMicroservice : ILoanService
    {
        private IFineStrategy _fineStrategy;
        private readonly IDatabaseHelper _dbHelper;

        public LoanMicroservice(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _fineStrategy = new StandardFineStrategy(); // Стандартна стратегія за замовчуванням
        }

        public void SetFineStrategy(IFineStrategy strategy)
        {
            _fineStrategy = strategy;
        }

        public decimal CalculateReturnFine(DateTime expectedReturn, DateTime actualReturn)
        {
            int overdueDays = (actualReturn - expectedReturn).Days;
            return _fineStrategy.CalculateFine(overdueDays);
        }

        public string ReturnBook(int loanId)
        {
            try
            {
                string message = "";
                _dbHelper.ExecuteInTransaction(cmd =>
                {
                    // 1. Отримуємо дані про позику
                    cmd.CommandText = "SELECT BookID, DateOfReturn, LoanStatus FROM Loan WHERE LoanID = @id";
                    cmd.Parameters.AddWithValue("@id", loanId);

                    int bookId;
                    DateTime expectedDate;
                    string currentStatus = "";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) throw new Exception("Запис про позику не знайдено!");

                        currentStatus = reader["LoanStatus"].ToString();
                        if (currentStatus == "Закрито")
                            throw new Exception("Цю книгу вже було повернуто раніше!");

                        bookId = Convert.ToInt32(reader["BookID"]);

                        // C# сам спробує розпізнати дату, навіть якщо в базі вона лежить як текст
                        expectedDate = Convert.ToDateTime(reader["DateOfReturn"]);
                    }

                    // 2. Використовуємо поточну стратегію для розрахунку штрафу
                    decimal fineAmount = CalculateReturnFine(expectedDate, DateTime.Now);

                    // 3. Записуємо штраф, якщо він є
                    if (fineAmount > 0)
                    {
                        cmd.CommandText = @"
                            INSERT INTO Fine (LoanID, Amount, DateIssued, PaidStatus) 
                            VALUES (@lId, @amt, @date, 'Не сплачено')";

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@lId", loanId);
                        cmd.Parameters.AddWithValue("@amt", fineAmount);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now);
                        cmd.ExecuteNonQuery();

                        message = $"Книгу повернуто. Нараховано штраф: {fineAmount} грн.";
                    }
                    else
                    {
                        message = "Книгу повернуто вчасно (або спрацювала амністія). Штрафів немає.";
                    }

                    // 4. Збільшуємо кількість доступних книг на полиці
                    cmd.CommandText = "UPDATE Book SET AvailableCount = AvailableCount + 1 WHERE BookID = @bId";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@bId", bookId);
                    cmd.ExecuteNonQuery();

                    // 5. ОНОВЛЮЄМО ПОЗИКУ - ставимо статус "Закрито" і реальну дату
                    cmd.CommandText = "UPDATE Loan SET LoanStatus = 'Закрито', RealDateOfReturn = @realDate WHERE LoanID = @lId";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@lId", loanId);
                    cmd.Parameters.AddWithValue("@realDate", DateTime.Now);
                    cmd.ExecuteNonQuery();
                });
                return message;
            }
            catch (Exception ex)
            {
                return "Помилка: " + ex.Message;
            }
        }
    }
}