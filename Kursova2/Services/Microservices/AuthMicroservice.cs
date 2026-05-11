using System;
using System.Data;
using System.Data.SqlClient;
using Kursova2.Services.Database;

namespace Kursova2.Services.Microservices
{
    public class AuthMicroservice : IAuthService
    {
        private readonly IDatabaseHelper _dbHelper;

        public AuthMicroservice(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public bool RegisterReader(string fullName, string email, string address, string homePhone, string workPhone)
        {
            try
            {
                var query = @"
                    INSERT INTO Reader 
                    (ReaderName, ContactData, ReaderAddress, HomePhone, WorkPhone, RegistrationDate, ActiveStatus) 
                    VALUES (@name, @contact, @address, @homePhone, @workPhone, @regDate, @status)";

                _dbHelper.ExecuteNonQuery(query,
                    new SqlParameter("@name", fullName),
                    new SqlParameter("@contact", email ?? (object)DBNull.Value),
                    new SqlParameter("@address", address ?? (object)DBNull.Value),
                    new SqlParameter("@homePhone", homePhone ?? (object)DBNull.Value),
                    new SqlParameter("@workPhone", workPhone ?? (object)DBNull.Value),
                    new SqlParameter("@regDate", DateTime.Now.ToString("yyyy-MM-dd")),
                    new SqlParameter("@status", "Активний")
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Помилка при реєстрації: " + ex.Message);
            }
        }

        public DataTable GetReaderInfo(string readerName)
        {
            var query = "SELECT ReaderName as 'ПІБ', ContactData as 'Email', ReaderAddress as 'Адреса', HomePhone as 'Дім. Телефон', WorkPhone as 'Роб. Телефон', RegistrationDate as 'Дата реєстрації', ActiveStatus as 'Статус' FROM Reader WHERE ReaderName = @name";
            return _dbHelper.ExecuteQuery(query, new SqlParameter("@name", readerName));
        }
    }
}