using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Kursova2.UI
{
    // (Діаграми)
    public class ChartForm : Form
    {
        public ChartForm(DataTable data, string chartTitle, string xColumn, string yColumn)
        {
            this.Text = "Статистика: " + chartTitle;
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Створюємо елемент графіку
            Chart chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke
            };

            // Додаємо область для малювання
            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.BackColor = Color.White;
            chart.ChartAreas.Add(chartArea);

            // Налаштовуємо легенду (пояснення кольорів)
            Legend legend = new Legend("Default");
            legend.Docking = Docking.Right;
            chart.Legends.Add(legend);

            // Налаштовуємо серію даних (Кругова діаграма)
            Series series = new Series("Data");
            series.ChartType = SeriesChartType.Pie; // Тип: Кругова діаграма
            series.XValueMember = xColumn;          // Назва колонки для підписів (напр. Жанр)
            series.YValueMembers = yColumn;         // Назва колонки для значень (напр. Кількість)
            series.IsValueShownAsLabel = true;      // Показувати цифри на графіку
            series.Font = new Font("Arial", 12, FontStyle.Bold);

            // Дизайн графіку
            series["PieLabelStyle"] = "Outside";
            series["PieLineColor"] = "Black";

            // Додаємо заголовок на сам графік
            chart.Titles.Add(new Title(chartTitle, Docking.Top, new Font("Arial", 14, FontStyle.Bold), Color.Black));

            chart.Series.Add(series);

            // Прив'язуємо дані з "мікросервісу"
            chart.DataSource = data;
            chart.DataBind();

            this.Controls.Add(chart);
        }
    }
}