using Kursova2.Services.Microservices;

namespace Kursova2.Services
{
    public interface IApiGateway
    {
        IAuthService Auth { get; }
        IDataService Data { get; }
        IAnalyticsService Analytics { get; }
        IReservationService Reservation { get; }
    }

    // Паттерн Facade + Dependency Injection
    public class ApiGateway : IApiGateway
    {
        public IAuthService Auth { get; }
        public IDataService Data { get; }
        public IAnalyticsService Analytics { get; }
        public IReservationService Reservation { get; }

        public ApiGateway(IAuthService auth, IDataService data, IAnalyticsService analytics, IReservationService reservation)
        {
            Auth = auth;
            Data = data;
            Analytics = analytics;
            Reservation = reservation;
        }
    }
}