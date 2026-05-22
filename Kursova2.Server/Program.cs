using Kursova2.Services.Database;
using Kursova2.Services.Microservices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры и поддержку сериализации DataTable (NewtonsoftJson)
builder.Services.AddControllers().AddNewtonsoftJson();

// Настраиваем подключение к БД (строка подключения теперь ТОЛЬКО на сервере)
string connString = "Server=DESKTOP-FV08LVO\\SQLEXPRESS;Database=Library_kurs;Integrated Security=True;TrustServerCertificate=True;";
builder.Services.AddScoped<IDatabaseHelper>(x => new SqlDatabaseHelper(connString));

// Регистрируем все ваши микросервисы
builder.Services.AddScoped<IAuthService, AuthMicroservice>();
builder.Services.AddScoped<IDataService, DataMicroservice>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsMicroservice>();
builder.Services.AddScoped<IReservationService, ReservationMicroservice>();
builder.Services.AddScoped<ILoanService, LoanMicroservice>();

var app = builder.Build();

app.MapControllers();
app.Run();