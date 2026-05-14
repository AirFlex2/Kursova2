using System;
using System.Windows.Forms;
using Kursova2.Services;
using Kursova2.Services.Microservices;
using Kursova2.Services.Database;
using Kursova2.Models;
using Kursova2.UI;

namespace Kursova2
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. Dependency Injection (Впровадження залежностей)
            string connString = AppConfig.Instance.ConnectionString;
            IDatabaseHelper dbHelper = new SqlDatabaseHelper(connString);

            // Створюємо мікросервіси
            IAuthService auth = new AuthMicroservice(dbHelper);
            IDataService data = new DataMicroservice(dbHelper);
            IAnalyticsService analytics = new AnalyticsMicroservice(dbHelper);
            IReservationService reservation = new ReservationMicroservice(dbHelper);

            // Створюємо новий сервіс позик та штрафів
            ILoanService loan = new LoanMicroservice(dbHelper);

            // Збираємо шлюз (Facade), додаючи новий сервіс
            IApiGateway apiGateway = new ApiGateway(auth, data, analytics, reservation, loan);

            // 2. Запуск інтерфейсу авторизації
            LoginForm loginForm = new LoginForm(apiGateway);

            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // Запуск відповідної форми залежно від ролі
                Form mainForm = loginForm.IsAdmin
                    ? (Form)new AdminForm(apiGateway)
                    : (Form)new ClientForm(apiGateway);

                Application.Run(mainForm);
            }
        }
    }
}