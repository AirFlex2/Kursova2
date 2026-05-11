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

            // 1. Dependency Injection (Внедрение зависимостей) - "Сборка" приложения
            string connString = AppConfig.Instance.ConnectionString;
            IDatabaseHelper dbHelper = new SqlDatabaseHelper(connString);

            // Создаем сервисы, передавая им общего помощника БД
            IAuthService auth = new AuthMicroservice(dbHelper);
            IDataService data = new DataMicroservice(dbHelper);
            IAnalyticsService analytics = new AnalyticsMicroservice(dbHelper);
            IReservationService reservation = new ReservationMicroservice(dbHelper);

            // Создаем шлюз, передавая ему готовые интерфейсы
            IApiGateway apiGateway = new ApiGateway(auth, data, analytics, reservation);

            // 2. Запуск UI
            LoginForm loginForm = new LoginForm(apiGateway);

            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Form mainForm = loginForm.IsAdmin
                    ? (Form)new AdminForm(apiGateway)
                    : (Form)new ClientForm(apiGateway);

                Application.Run(mainForm);
            }
        }
    }
}