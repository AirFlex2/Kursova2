using Kursova2.Services.Microservices;

namespace Kursova2.Services
{
    public interface IApiGateway
    {
        IAuthService Auth { get; }
        IDataService Data { get; }
        IAnalyticsService Analytics { get; }
        IReservationService Reservation { get; }
        ILoanService Loan { get; }
    }

    public class ApiGateway : IApiGateway
    {
        public IAuthService Auth { get; }
        public IDataService Data { get; }
        public IAnalyticsService Analytics { get; }
        public IReservationService Reservation { get; }
        public ILoanService Loan { get; }

        public ApiGateway(IAuthService auth, IDataService data, IAnalyticsService analytics,
                          IReservationService reservation, ILoanService loan)
        {
            Auth = auth;
            Data = data;
            Analytics = analytics;
            Reservation = reservation;
            Loan = loan;
        }
    }
}