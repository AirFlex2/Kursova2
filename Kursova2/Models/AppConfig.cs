namespace Kursova2.Models
{
    // ПАТЕРН: Singleton (Одинак)
    // Гарантує, що конфігурація існує лише в одному екземплярі
    public class AppConfig
    {
        private static AppConfig _instance;
        private static readonly object _lock = new object();

        public string ConnectionString { get; private set; }

        private AppConfig()
        {
            ConnectionString = "Server=DESKTOP-FV08LVO\\SQLEXPRESS;Database=Library_kurs;Integrated Security=True;TrustServerCertificate=True;";
        }

        public static AppConfig Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AppConfig();
                    }
                    return _instance;
                }
            }
        }
    }
}