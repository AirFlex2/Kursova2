namespace Kursova2.Services.Strategies
{
    // ПАТЕРН: Strategy (Стратегія)
    // Визначає загальний інтерфейс для розрахунку штрафів
    public interface IFineStrategy
    {
        decimal CalculateFine(int overdueDays);
    }

    // Стандартний штраф (наприклад, 10 грн за день)
    public class StandardFineStrategy : IFineStrategy
    {
        public decimal CalculateFine(int overdueDays)
        {
            return overdueDays > 0 ? overdueDays * 10m : 0m;
        }
    }

    // Режим "Амністія" (свята, війна тощо) - штрафи не нараховуються
    public class AmnestyFineStrategy : IFineStrategy
    {
        public decimal CalculateFine(int overdueDays)
        {
            return 0m;
        }
    }
}