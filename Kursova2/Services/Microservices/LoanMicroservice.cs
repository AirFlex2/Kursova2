using System;
using Kursova2.Services.Strategies;

namespace Kursova2.Services.Microservices
{
    // МІКРОСЕРВІСНА АРХІТЕКТУРА (Симуляція)
    // Відповідає за позики, повернення та нарахування штрафів.
    public class LoanMicroservice
    {
        private IFineStrategy _fineStrategy;

        public LoanMicroservice()
        {
            _fineStrategy = new StandardFineStrategy(); // За замовчуванням
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
    }
}
